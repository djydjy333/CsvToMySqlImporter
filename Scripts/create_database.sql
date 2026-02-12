-- ============================================
-- CSV 导入工具 - MySQL 数据库表结构
-- ============================================

-- 创建数据库（如果不存在）
CREATE DATABASE IF NOT EXISTS csv_import_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE csv_import_db;

-- ============================================
-- 产品表 (products)
-- ============================================
DROP TABLE IF EXISTS products;

CREATE TABLE products (
    -- 主键
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,

    -- 业务字段
    product_code VARCHAR(50) NOT NULL COMMENT '产品编码（唯一）',
    product_name VARCHAR(200) NOT NULL COMMENT '产品名称',
    category VARCHAR(100) DEFAULT NULL COMMENT '产品分类',
    price DECIMAL(12, 2) NOT NULL DEFAULT 0.00 COMMENT '价格',
    quantity INT NOT NULL DEFAULT 0 COMMENT '库存数量',
    manufacture_date DATE DEFAULT NULL COMMENT '生产日期',
    is_active TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否启用 (1=是, 0=否)',

    -- 审计字段
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    updated_at DATETIME DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',

    -- 索引
    UNIQUE KEY uk_product_code (product_code),
    INDEX idx_category (category),
    INDEX idx_is_active (is_active),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='产品信息表';

-- ============================================
-- 导入日志表 (import_logs) - 可选
-- ============================================
DROP TABLE IF EXISTS import_logs;

CREATE TABLE import_logs (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    file_name VARCHAR(500) NOT NULL COMMENT '导入文件名',
    total_rows INT NOT NULL DEFAULT 0 COMMENT '总行数',
    success_count INT NOT NULL DEFAULT 0 COMMENT '成功数',
    failed_count INT NOT NULL DEFAULT 0 COMMENT '失败数',
    elapsed_seconds DECIMAL(10, 2) DEFAULT NULL COMMENT '执行耗时(秒)',
    error_log TEXT DEFAULT NULL COMMENT '错误日志',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '导入时间',

    INDEX idx_created_at (created_at)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='导入日志表';

-- ============================================
-- 验证表结构
-- ============================================
DESCRIBE products;
DESCRIBE import_logs;
