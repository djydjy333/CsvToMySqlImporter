# CSV 批量导入 MySQL 工具

一个使用 C# 实现的 CSV 文件批量导入 MySQL 数据库的命令行工具。

## 项目结构

```
CsvToMySqlImporter/
├── CsvToMySqlImporter.csproj   # 项目文件
├── Program.cs                   # 程序入口
├── README.md                    # 项目说明
├── Models/
│   ├── Product.cs              # 产品数据模型
│   ├── ProductMap.cs           # CsvHelper 映射配置
│   └── ImportResult.cs         # 导入结果模型
├── Services/
│   └── CsvImportService.cs     # CSV 导入服务（核心逻辑）
├── Scripts/
│   └── create_database.sql     # MySQL 表结构 DDL
└── SampleData/
    ├── products_sample.csv     # 示例 CSV 文件
    └── products_with_errors.csv # 包含错误数据的测试文件
```

## 功能特性

- **CSV 解析**: 使用 CsvHelper 库进行高效解析
- **MySQL 连接**: 使用 MySqlConnector 高性能连接器
- **错误处理**: 单行错误记录日志并跳过，不中断批处理
- **SQL 安全**: 使用参数化查询防止 SQL 注入
- **事务支持**: 批量提交，确保数据一致性
- **异步编程**: 全程使用 async/await 模式
- **取消支持**: 支持 Ctrl+C 优雅终止

## 快速开始

### 1. 创建数据库

```bash
mysql -u root -p < Scripts/create_database.sql
```

### 2. 编译项目

```bash
cd CsvToMySqlImporter
dotnet restore
dotnet build
```

### 3. 运行导入

```bash
# 使用环境变量配置连接字符串
export MYSQL_CONNECTION_STRING="Server=localhost;Port=3306;Database=csv_import_db;User=root;Password=root;"
dotnet run -- SampleData/products_sample.csv

# 或直接指定连接字符串
dotnet run -- .\SampleData\products_sample.csv "Server=localhost;Port=3306;Database=csv_import_db;User=root;Password=root;"
```

## CSV 文件格式

| 列名 | 类型 | 必填 | 说明 |
|------|------|------|------|
| product_code | string | 是 | 产品编码（唯一标识） |
| product_name | string | 是 | 产品名称 |
| category | string | 否 | 产品分类 |
| price | decimal | 是 | 价格 |
| quantity | int | 是 | 库存数量 |
| manufacture_date | date | 否 | 生产日期 (YYYY-MM-DD) |
| is_active | bool | 是 | 是否启用 (1/0, true/false, yes/no, Y/N) |

## 返回码

| 返回码 | 说明 |
|--------|------|
| 0 | 成功，无错误 |
| 1 | 参数错误或文件不存在 |
| 2 | 部分记录导入失败 |
| 3 | 用户取消操作 |

## 依赖包

- `CsvHelper` (v31.0.0) - CSV 解析
- `MySqlConnector` (v2.3.5) - MySQL 连接
- `Microsoft.Extensions.Logging.Console` (v8.0.0) - 日志输出
