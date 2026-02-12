using CsvToMySqlImporter.Services;
using Microsoft.Extensions.Logging;

namespace CsvToMySqlImporter;

/// <summary>
/// CSV 批量导入 MySQL 工具
/// 用法: CsvToMySqlImporter <csv文件路径> [连接字符串]
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // 配置日志
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddConsole(options =>
                {
                    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                });
        });

        var logger = loggerFactory.CreateLogger<Program>();
        var serviceLogger = loggerFactory.CreateLogger<CsvImportService>();

        try
        {
            // 解析命令行参数
            if (args.Length < 1)
            {
                PrintUsage();
                return 1;
            }

            string csvFilePath = args[0];

            // 连接字符串可从参数或环境变量获取
            string connectionString = args.Length >= 2
                ? args[1]
                : Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
                  ?? "Server=localhost;Port=3306;Database=csv_import_db;User=root;Password=your_password;";

            logger.LogInformation("========================================");
            logger.LogInformation("CSV 批量导入 MySQL 工具");
            logger.LogInformation("========================================");
            logger.LogInformation("CSV 文件: {CsvPath}", csvFilePath);

            // 创建导入服务并执行
            var importService = new CsvImportService(connectionString, serviceLogger);

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                logger.LogWarning("收到取消信号，正在停止导入...");
                cts.Cancel();
            };

            var result = await importService.ImportAsync(csvFilePath, cts.Token);

            // 输出导入结果
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("导入结果统计");
            Console.WriteLine("========================================");
            Console.WriteLine($"总记录数:   {result.TotalRows}");
            Console.WriteLine($"成功导入:   {result.SuccessCount}");
            Console.WriteLine($"失败记录:   {result.FailedCount}");
            Console.WriteLine($"执行耗时:   {result.ElapsedTime.TotalSeconds:F2} 秒");

            // 输出错误详情
            if (result.Errors.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("错误详情 (前 10 条):");
                Console.WriteLine("----------------------------------------");

                foreach (var error in result.Errors.Take(10))
                {
                    Console.WriteLine($"行 {error.RowNumber}: {error.ErrorMessage}");
                    if (!string.IsNullOrEmpty(error.RawData))
                    {
                        Console.WriteLine($"  原始数据: {error.RawData}");
                    }
                }

                if (result.Errors.Count > 10)
                {
                    Console.WriteLine($"... 还有 {result.Errors.Count - 10} 条错误未显示");
                }

                // 导出错误日志到文件
                string errorLogPath = Path.Combine(
                    Path.GetDirectoryName(csvFilePath) ?? ".",
                    $"import_errors_{DateTime.Now:yyyyMMdd_HHmmss}.log");

                await ExportErrorLogAsync(errorLogPath, result.Errors);
                Console.WriteLine();
                Console.WriteLine($"完整错误日志已导出至: {errorLogPath}");
            }

            return result.FailedCount > 0 ? 2 : 0;
        }
        catch (FileNotFoundException ex)
        {
            logger.LogError("文件未找到: {Message}", ex.Message);
            return 1;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("操作已取消");
            return 3;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "发生未处理的异常");
            return 1;
        }
    }

    /// <summary>
    /// 打印使用帮助
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("CSV 批量导入 MySQL 工具");
        Console.WriteLine();
        Console.WriteLine("用法:");
        Console.WriteLine("  CsvToMySqlImporter <csv文件路径> [连接字符串]");
        Console.WriteLine();
        Console.WriteLine("参数:");
        Console.WriteLine("  csv文件路径    必填，要导入的 CSV 文件路径");
        Console.WriteLine("  连接字符串     可选，MySQL 连接字符串");
        Console.WriteLine();
        Console.WriteLine("环境变量:");
        Console.WriteLine("  MYSQL_CONNECTION_STRING  MySQL 连接字符串（当命令行未指定时使用）");
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine("  CsvToMySqlImporter products.csv");
        Console.WriteLine("  CsvToMySqlImporter products.csv \"Server=localhost;Database=mydb;User=root;Password=xxx;\"");
    }

    /// <summary>
    /// 导出错误日志到文件
    /// </summary>
    private static async Task ExportErrorLogAsync(string filePath, IEnumerable<Models.RowError> errors)
    {
        await using var writer = new StreamWriter(filePath);
        await writer.WriteLineAsync($"CSV 导入错误日志 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        await writer.WriteLineAsync(new string('=', 60));
        await writer.WriteLineAsync();

        foreach (var error in errors)
        {
            await writer.WriteLineAsync($"行号: {error.RowNumber}");
            await writer.WriteLineAsync($"错误: {error.ErrorMessage}");
            if (!string.IsNullOrEmpty(error.RawData))
            {
                await writer.WriteLineAsync($"数据: {error.RawData}");
            }
            await writer.WriteLineAsync(new string('-', 40));
        }
    }
}
