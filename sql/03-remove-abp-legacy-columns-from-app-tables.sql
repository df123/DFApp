-- 修复 App 表：移除 ABP Framework 遗留的 ExtraProperties 和 IsDeleted 列
-- 原因：项目已从 ABP Framework 迁移到轻量级 ASP.NET Core，
--       实体类（AuditedEntity / CreationAuditedEntity）不包含 ExtraProperties 和 IsDeleted 属性，
--       ExtraProperties 的 NOT NULL 约束导致 INSERT 时报错 500

-- 记账模块
ALTER TABLE AppBookkeepingExpenditure DROP COLUMN ExtraProperties;
ALTER TABLE AppBookkeepingExpenditure DROP COLUMN IsDeleted;
ALTER TABLE AppBookkeepingCategory DROP COLUMN ExtraProperties;
ALTER TABLE AppBookkeepingCategory DROP COLUMN IsDeleted;

-- 配置模块
ALTER TABLE AppConfigurationInfo DROP COLUMN ExtraProperties;
ALTER TABLE AppConfigurationInfo DROP COLUMN IsDeleted;

-- 动态 IP
ALTER TABLE AppDynamicIP DROP COLUMN ExtraProperties;
ALTER TABLE AppDynamicIP DROP COLUMN IsDeleted;

-- 电动车模块
ALTER TABLE AppElectricVehicle DROP COLUMN ExtraProperties;
ALTER TABLE AppElectricVehicle DROP COLUMN IsDeleted;
ALTER TABLE AppElectricVehicleChargingRecord DROP COLUMN ExtraProperties;
ALTER TABLE AppElectricVehicleChargingRecord DROP COLUMN IsDeleted;
ALTER TABLE AppElectricVehicleCost DROP COLUMN ExtraProperties;
ALTER TABLE AppElectricVehicleCost DROP COLUMN IsDeleted;
ALTER TABLE AppGasolinePrice DROP COLUMN ExtraProperties;
ALTER TABLE AppGasolinePrice DROP COLUMN IsDeleted;

-- 文件上传模块
ALTER TABLE AppFileUploadInfo DROP COLUMN ExtraProperties;
ALTER TABLE AppFileUploadInfo DROP COLUMN IsDeleted;

-- 文件过滤规则（仅有 ExtraProperties）
ALTER TABLE AppKeywordFilterRule DROP COLUMN ExtraProperties;

-- 彩票模块
ALTER TABLE AppLottery DROP COLUMN ExtraProperties;
ALTER TABLE AppLottery DROP COLUMN IsDeleted;
ALTER TABLE AppLotteryPrizegrades DROP COLUMN ExtraProperties;
ALTER TABLE AppLotteryPrizegrades DROP COLUMN IsDeleted;
ALTER TABLE AppLotteryResult DROP COLUMN ExtraProperties;
ALTER TABLE AppLotteryResult DROP COLUMN IsDeleted;
ALTER TABLE AppLotterySimulation DROP COLUMN ExtraProperties;

-- 媒体模块（AppMediaInfo 和 AppMediaExternalLink 无遗留列，无需处理）

-- Aria2 模块（仅有 ExtraProperties）
ALTER TABLE AppAria2FilesItem DROP COLUMN ExtraProperties;
ALTER TABLE AppAria2TellStatusResult DROP COLUMN ExtraProperties;
ALTER TABLE AppAria2UrisItem DROP COLUMN ExtraProperties;
