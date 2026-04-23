-- 为 admin 角色补充所有缺失的 DFApp.* 权限
-- 原因：DFAppPermissions.cs 中定义了 22 个权限分组共 62 个权限，
--       但 admin 角色仅有其中 32 个，导致多个功能模块返回 403
-- 日期：2026-04-11
--
-- 注意：运行时数据库路径为 /home/df/dfapp/DFApp/DFApp.db
--       使用 INSERT OR IGNORE 避免重复插入

-- ========================================
-- Medias 媒体管理（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Medias', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Medias.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Medias.Edit', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Medias.Delete', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Medias.Download', 'Role', 'admin');

-- ========================================
-- DynamicIP 动态IP管理（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.DynamicIP', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.DynamicIP.Delete', 'Role', 'admin');

-- ========================================
-- Lottery 彩票管理（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Lottery', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Lottery.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Lottery.Edit', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Lottery.Delete', 'Role', 'admin');

-- ========================================
-- LogSink 日志接收器（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.LogSink', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.LogSink.QueueSink', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.LogSink.SignalRSink', 'Role', 'admin');

-- ========================================
-- Bookkeeping 记账管理（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Bookkeeping', 'Role', 'admin');

-- ========================================
-- BookkeepingCategory 记账分类（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Bookkeeping.Category', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Bookkeeping.Category.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Bookkeeping.Category.Edit', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Bookkeeping.Category.Delete', 'Role', 'admin');

-- ========================================
-- BookkeepingExpenditure 记账支出（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Bookkeeping.Expenditure', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Bookkeeping.Expenditure.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Bookkeeping.Expenditure.Edit', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Bookkeeping.Expenditure.Delete', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Bookkeeping.Expenditure.Analysis', 'Role', 'admin');

-- ========================================
-- FileUploadDownload 文件上传下载（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.FileUploadDownload', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.FileUploadDownload.Upload', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.FileUploadDownload.Download', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.FileUploadDownload.Delete', 'Role', 'admin');

-- ========================================
-- Aria2 管理（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Aria2', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Aria2.Link', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Aria2.Delete', 'Role', 'admin');

-- ========================================
-- LogViewer 日志查看（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.LogViewer', 'Role', 'admin');

-- ========================================
-- Rss RSS管理（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Rss', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Rss.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Rss.Update', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Rss.Delete', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.Rss.Download', 'Role', 'admin');

-- ========================================
-- ElectricVehicle 电动车管理（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.Edit', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.Delete', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.Statistics', 'Role', 'admin');

-- ========================================
-- ElectricVehicleCost 电动车费用（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.Cost', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.Cost.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.Cost.Edit', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.Cost.Delete', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.Cost.Analysis', 'Role', 'admin');

-- ========================================
-- ElectricVehicleChargingRecord 电动车充电记录（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.ChargingRecord', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.ChargingRecord.Create', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.ChargingRecord.Edit', 'Role', 'admin');

INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.ChargingRecord.Delete', 'Role', 'admin');

-- ========================================
-- GasolinePrice 油价信息（全部缺失）
-- ========================================
INSERT OR IGNORE INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey)
VALUES ('DFApp.ElectricVehicle.GasolinePrice', 'Role', 'admin');
