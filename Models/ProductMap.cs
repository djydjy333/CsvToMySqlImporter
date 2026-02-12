using CsvHelper.Configuration;

namespace CsvToMySqlImporter.Models;

/// <summary>
/// CsvHelper 映射配置，定义 CSV 列与 Product 属性的对应关系
/// </summary>
public sealed class ProductMap : ClassMap<Product>
{
    public ProductMap()
    {
        Map(m => m.ProductCode).Name("product_code");
        Map(m => m.ProductName).Name("product_name");
        Map(m => m.Category).Name("category");
        Map(m => m.Price).Name("price");
        Map(m => m.Quantity).Name("quantity");
        Map(m => m.ManufactureDate).Name("manufacture_date").TypeConverterOption.NullValues(string.Empty, "NULL", "null");
        Map(m => m.IsActive).Name("is_active").TypeConverterOption.BooleanValues(true, true, "1", "true", "yes", "Y")
                                               .TypeConverterOption.BooleanValues(false, true, "0", "false", "no", "N");
    }
}
