using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvToMySqlImporter.Models;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace CsvToMySqlImporter.Services;

/// <summary>
/// CSV 导入服务，负责解析 CSV 文件并批量写入 MySQL 数据库
/// </summary>
public class CsvImportService
{
    private readonly string _connectionString;
    private readonly ILogger<CsvImportService> _logger;
    private const int BatchSize = 100; // 批量提交的记录数

    public CsvImportService(string connectionString, ILogger<CsvImportService> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// 异步导入 CSV 文件到数据库
    /// </summary>
    /// <param name="csvFilePath">CSV 文件路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>导入结果</returns>
    public async Task<ImportResult> ImportAsync(string csvFilePath, CancellationToken cancellationToken = default)
    {
        var result = new ImportResult();
        var stopwatch = Stopwatch.StartNew();

        if (!File.Exists(csvFilePath))
        {
            throw new FileNotFoundException($"CSV 文件不存在: {csvFilePath}");
        }

        _logger.LogInformation("开始导入 CSV 文件: {FilePath}", csvFilePath);

        var validRecords = new List<(Product Product, int RowNumber)>();

        // 第一阶段：解析 CSV 文件
        await ParseCsvFileAsync(csvFilePath, validRecords, result, cancellationToken);

        // 第二阶段：批量写入数据库
        if (validRecords.Count > 0)
        {
            await WriteToDbAsync(validRecords, result, cancellationToken);
        }

        stopwatch.Stop();
        result.ElapsedTime = stopwatch.Elapsed;

        _logger.LogInformation(
            "导入完成。总行数: {Total}, 成功: {Success}, 失败: {Failed}, 耗时: {Elapsed:F2}s",
            result.TotalRows, result.SuccessCount, result.FailedCount, result.ElapsedTime.TotalSeconds);

        return result;
    }

    /// <summary>
    /// 解析 CSV 文件，逐行处理并收集有效记录
    /// </summary>
    private async Task ParseCsvFileAsync(
        string csvFilePath,
        List<(Product, int)> validRecords,
        ImportResult result,
        CancellationToken cancellationToken)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null, // 忽略缺失字段
            BadDataFound = context =>
            {
                _logger.LogWarning("发现错误数据，行 {Row}: {RawRecord}", context.Context?.Parser?.Row, context.RawRecord);
            }
        };

        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, config);

        csv.Context.RegisterClassMap<ProductMap>();

        int rowNumber = 1; // CSV 行号（包含表头）

        await foreach (var record in ReadRecordsAsync(csv, result, cancellationToken))
        {
            rowNumber++;
            result.TotalRows++;

            if (record != null)
            {
                validRecords.Add((record, rowNumber));
            }
        }
    }

    /// <summary>
    /// 异步读取 CSV 记录，捕获单行错误
    /// </summary>
    private async IAsyncEnumerable<Product?> ReadRecordsAsync(
        CsvReader csv,
        ImportResult result,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            Product? product = null;
            try
            {
                product = csv.GetRecord<Product>();
            }
            catch (Exception ex)
            {
                var rowError = new RowError
                {
                    RowNumber = csv.Context.Parser?.Row ?? 0,
                    RawData = csv.Context.Parser?.RawRecord ?? string.Empty,
                    ErrorMessage = ex.Message
                };
                result.Errors.Add(rowError);

                _logger.LogWarning(
                    "解析错误，行 {Row}: {Error}。原始数据: {RawData}",
                    rowError.RowNumber, ex.Message, rowError.RawData);
            }

            yield return product;
        }
    }

    /// <summary>
    /// 批量写入数据库，使用事务确保数据一致性
    /// </summary>
    private async Task WriteToDbAsync(
        List<(Product Product, int RowNumber)> records,
        ImportResult result,
        CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // 分批处理
        var batches = records.Chunk(BatchSize);
        int batchNumber = 0;

        foreach (var batch in batches)
        {
            batchNumber++;
            cancellationToken.ThrowIfCancellationRequested();

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var (product, rowNumber) in batch)
                {
                    try
                    {
                        await InsertProductAsync(connection, transaction, product, cancellationToken);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add(new RowError
                        {
                            RowNumber = rowNumber,
                            RawData = $"{product.ProductCode},{product.ProductName}",
                            ErrorMessage = $"数据库写入错误: {ex.Message}"
                        });

                        _logger.LogWarning(
                            "数据库写入失败，行 {Row}, 产品编码 {Code}: {Error}",
                            rowNumber, product.ProductCode, ex.Message);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
                _logger.LogDebug("批次 {Batch} 提交成功，包含 {Count} 条记录", batchNumber, batch.Length);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "批次 {Batch} 事务回滚", batchNumber);

                // 记录该批次所有记录为失败
                foreach (var (product, rowNumber) in batch)
                {
                    if (!result.Errors.Any(e => e.RowNumber == rowNumber))
                    {
                        result.FailedCount++;
                        result.Errors.Add(new RowError
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = $"事务回滚: {ex.Message}"
                        });
                    }
                }
            }
        }
    }

    /// <summary>
    /// 插入单条产品记录（使用参数化查询防止 SQL 注入）
    /// </summary>
    private static async Task InsertProductAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        Product product,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO products (product_code, product_name, category, price, quantity, manufacture_date, is_active, created_at)
            VALUES (@ProductCode, @ProductName, @Category, @Price, @Quantity, @ManufactureDate, @IsActive, @CreatedAt)
            ON DUPLICATE KEY UPDATE
                product_name = VALUES(product_name),
                category = VALUES(category),
                price = VALUES(price),
                quantity = VALUES(quantity),
                manufacture_date = VALUES(manufacture_date),
                is_active = VALUES(is_active),
                updated_at = NOW()";

        await using var command = new MySqlCommand(sql, connection, transaction);

        command.Parameters.AddWithValue("@ProductCode", product.ProductCode);
        command.Parameters.AddWithValue("@ProductName", product.ProductName);
        command.Parameters.AddWithValue("@Category", product.Category);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@Quantity", product.Quantity);
        command.Parameters.AddWithValue("@ManufactureDate", product.ManufactureDate.HasValue ? product.ManufactureDate.Value : DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", product.IsActive);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
