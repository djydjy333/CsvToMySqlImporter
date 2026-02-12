namespace CsvToMySqlImporter.Models;

/// <summary>
/// 产品数据模型，对应 CSV 文件和数据库表结构
/// </summary>
public class Product
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public bool IsActive { get; set; }
}
