-- MySQL dump 10.13  Distrib 5.7.30, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: csv_import_db
-- ------------------------------------------------------
-- Server version	5.7.30-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `import_logs`
--

DROP TABLE IF EXISTS `import_logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `import_logs` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `file_name` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '导入文件名',
  `total_rows` int(11) NOT NULL DEFAULT '0' COMMENT '总行数',
  `success_count` int(11) NOT NULL DEFAULT '0' COMMENT '成功数',
  `failed_count` int(11) NOT NULL DEFAULT '0' COMMENT '失败数',
  `elapsed_seconds` decimal(10,2) DEFAULT NULL COMMENT '执行耗时(秒)',
  `error_log` text COLLATE utf8mb4_unicode_ci COMMENT '错误日志',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '导入时间',
  PRIMARY KEY (`id`),
  KEY `idx_created_at` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='导入日志表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `import_logs`
--

LOCK TABLES `import_logs` WRITE;
/*!40000 ALTER TABLE `import_logs` DISABLE KEYS */;
/*!40000 ALTER TABLE `import_logs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `products`
--

DROP TABLE IF EXISTS `products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `products` (
  `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,
  `product_code` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '产品编码（唯一）',
  `product_name` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '产品名称',
  `category` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '产品分类',
  `price` decimal(12,2) NOT NULL DEFAULT '0.00' COMMENT '价格',
  `quantity` int(11) NOT NULL DEFAULT '0' COMMENT '库存数量',
  `manufacture_date` date DEFAULT NULL COMMENT '生产日期',
  `is_active` tinyint(1) NOT NULL DEFAULT '1' COMMENT '是否启用 (1=是, 0=否)',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_product_code` (`product_code`),
  KEY `idx_category` (`category`),
  KEY `idx_is_active` (`is_active`),
  KEY `idx_created_at` (`created_at`)
) ENGINE=InnoDB AUTO_INCREMENT=44 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='产品信息表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `products`
--

LOCK TABLES `products` WRITE;
/*!40000 ALTER TABLE `products` DISABLE KEYS */;
INSERT INTO `products` VALUES (1,'PROD001','智能手机 Pro Max','电子产品',5999.00,100,'2024-01-15',1,'2026-02-07 18:29:48','2026-02-12 10:19:45'),(2,'PROD002','无线蓝牙耳机','电子产品',299.00,500,'2024-02-20',1,'2026-02-07 18:29:48','2026-02-12 10:19:45'),(3,'PROD003','笔记本电脑 15寸','电子产品',7999.00,50,'2024-01-10',1,'2026-02-07 18:29:48','2026-02-12 10:19:45'),(4,'PROD004','机械键盘 RGB','电脑配件',399.00,200,'2023-12-01',1,'2026-02-07 18:29:48','2026-02-12 10:19:45'),(5,'PROD005','游戏鼠标','电脑配件',199.00,300,'2023-11-15',1,'2026-02-07 18:29:48','2026-02-12 10:19:45'),(6,'PROD006','4K显示器 27寸','电脑配件',2499.00,80,'2024-03-01',1,'2026-02-07 18:29:48','2026-02-12 10:19:45'),(7,'PROD007','移动电源 20000mAh','电子产品',149.00,1000,'2024-02-28',1,'2026-02-07 18:29:48','2026-02-12 10:19:45'),(8,'PROD008','智能手表','电子产品',1299.00,150,'2024-01-25',1,'2026-02-07 18:29:48','2026-02-12 10:19:45'),(9,'PROD009','USB-C 扩展坞','电脑配件',299.00,400,'2023-10-20',0,'2026-02-07 18:29:48','2026-02-12 10:19:45'),(10,'PROD010','固态硬盘 1TB','电脑配件',599.00,600,'2024-03-10',1,'2026-02-07 18:29:48','2026-02-12 10:19:45'),(31,'PROD011','固态硬盘 2TB','电脑配件',599.00,600,'2024-03-10',1,'2026-02-08 02:18:35','2026-02-12 10:19:45'),(43,'PROD012','硬盘 2TB','电脑配件',599.00,600,'2026-03-10',1,'2026-02-12 10:19:45',NULL);
/*!40000 ALTER TABLE `products` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-02-12 18:02:31
