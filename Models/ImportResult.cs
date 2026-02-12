namespace CsvToMySqlImporter.Models;

/// <summary>
/// 导入结果统计
/// </summary>
public class ImportResult
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<RowError> Errors { get; } = new();
    public TimeSpan ElapsedTime { get; set; }
}

/// <summary>
/// 单行错误信息
/// </summary>
public class RowError
{
    public int RowNumber { get; set; }
    public string RawData { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
