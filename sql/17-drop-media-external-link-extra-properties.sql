-- 移除 AppMediaExternalLink 表的 ExtraProperties 列（ABP 遗留字段，不再需要兼容）
ALTER TABLE AppMediaExternalLink DROP COLUMN ExtraProperties;
