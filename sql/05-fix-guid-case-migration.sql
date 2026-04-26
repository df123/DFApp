-- ============================================================
-- 修复数据库中所有大写 Guid 为小写
-- 根因：ABP Framework 时期创建的数据 Guid 以大写格式存储，
--       SqlSugar 查询使用小写格式，SQLite TEXT 比较区分大小写导致匹配失败
-- ============================================================

-- 禁用外键约束检查，避免更新外键列时触发约束冲突
PRAGMA foreign_keys = OFF;

BEGIN TRANSACTION;

-- ============================================================
-- 第一批：被引用的表（先更新主键，确保引用它们的表能正确匹配）
-- ============================================================

-- 1. AbpUsers（用户表，被 AbpUserRoles、AbpPermissionGrants 等引用）
--    列：Id, CreatorId, LastModifierId
UPDATE AbpUsers SET Id = LOWER(Id) WHERE Id != LOWER(Id);
UPDATE AbpUsers SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AbpUsers SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- 2. AbpRoles（角色表，被 AbpUserRoles 引用）
--    列：Id
UPDATE AbpRoles SET Id = LOWER(Id) WHERE Id != LOWER(Id);

-- 3. AppElectricVehicle（电车表，被充电记录和费用表引用）
--    列：Id, CreatorId, LastModifierId
UPDATE AppElectricVehicle SET Id = LOWER(Id) WHERE Id != LOWER(Id);
UPDATE AppElectricVehicle SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppElectricVehicle SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- ============================================================
-- 第二批：引用其他表的表（外键列 + 审计列）
-- ============================================================

-- 4. AbpUserRoles（联合主键 UserId + RoleId，均引用其他表）
--    列：UserId, RoleId
UPDATE AbpUserRoles SET UserId = LOWER(UserId) WHERE UserId != LOWER(UserId);
UPDATE AbpUserRoles SET RoleId = LOWER(RoleId) WHERE RoleId != LOWER(RoleId);

-- 5. AbpPermissionGrants（权限授予，ProviderKey 存储角色/用户 Guid）
--    列：Id, ProviderKey
UPDATE AbpPermissionGrants SET Id = LOWER(Id) WHERE Id != LOWER(Id);
UPDATE AbpPermissionGrants SET ProviderKey = LOWER(ProviderKey) WHERE ProviderKey != LOWER(ProviderKey);

-- 6. AbpPermissionGroups（权限组）
--    列：Id
UPDATE AbpPermissionGroups SET Id = LOWER(Id) WHERE Id != LOWER(Id);

-- 7. AbpPermissions（权限定义）
--    列：Id
UPDATE AbpPermissions SET Id = LOWER(Id) WHERE Id != LOWER(Id);

-- 8. AppElectricVehicleChargingRecord（充电记录，引用 AppElectricVehicle）
--    列：Id, VehicleId, CreatorId, LastModifierId
UPDATE AppElectricVehicleChargingRecord SET Id = LOWER(Id) WHERE Id != LOWER(Id);
UPDATE AppElectricVehicleChargingRecord SET VehicleId = LOWER(VehicleId) WHERE VehicleId != LOWER(VehicleId);
UPDATE AppElectricVehicleChargingRecord SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppElectricVehicleChargingRecord SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- 9. AppElectricVehicleCost（电车费用，引用 AppElectricVehicle）
--    列：Id, VehicleId, CreatorId, LastModifierId
UPDATE AppElectricVehicleCost SET Id = LOWER(Id) WHERE Id != LOWER(Id);
UPDATE AppElectricVehicleCost SET VehicleId = LOWER(VehicleId) WHERE VehicleId != LOWER(VehicleId);
UPDATE AppElectricVehicleCost SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppElectricVehicleCost SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- ============================================================
-- 第三批：独立表（无外键引用其他 App/Abp 表，仅有审计列）
-- ============================================================

-- 10. AppGasolinePrice（汽油价格）
--     列：Id, CreatorId, LastModifierId, DeleterId
UPDATE AppGasolinePrice SET Id = LOWER(Id) WHERE Id != LOWER(Id);
UPDATE AppGasolinePrice SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppGasolinePrice SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);
UPDATE AppGasolinePrice SET DeleterId = LOWER(DeleterId) WHERE DeleterId IS NOT NULL AND DeleterId != LOWER(DeleterId);

-- 11. AppLotterySimulation（彩票模拟，大量数据 50944 行）
--     列：Id, CreatorId
UPDATE AppLotterySimulation SET Id = LOWER(Id) WHERE Id != LOWER(Id);
UPDATE AppLotterySimulation SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);

-- 12. AppFileUploadInfo（文件上传信息）
--     列：CreatorId, LastModifierId
UPDATE AppFileUploadInfo SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppFileUploadInfo SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- 13. AppBookkeepingCategory（记账分类）
--     列：CreatorId, LastModifierId
UPDATE AppBookkeepingCategory SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppBookkeepingCategory SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- 14. AppBookkeepingExpenditure（记账支出）
--     列：CreatorId, LastModifierId
UPDATE AppBookkeepingExpenditure SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppBookkeepingExpenditure SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- 15. AppConfigurationInfo（配置信息）
--     列：CreatorId, LastModifierId
UPDATE AppConfigurationInfo SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppConfigurationInfo SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- 16. AppKeywordFilterRule（关键词过滤规则）
--     列：CreatorId
UPDATE AppKeywordFilterRule SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);

-- 17. AppLottery（彩票数据，Id 为 INTEGER 自增，无需更新）
--     列：CreatorId
UPDATE AppLottery SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);

-- 18. AppLotteryPrizegrades（彩票奖级，Id 为 INTEGER 自增）
--     列：CreatorId
UPDATE AppLotteryPrizegrades SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);

-- 19. AppLotteryResult（彩票开奖结果，Id 为 INTEGER 自增）
--     列：CreatorId
UPDATE AppLotteryResult SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);

-- 20. AppPermissionGrants 已移至脚本 06 处理
--     原因：AppPermissionGrants 表在脚本 06 中创建，脚本 05 执行时该表尚不存在
--     脚本 06 在数据迁移时已统一使用 lower(ProviderKey)，并在迁移完成后清理残留大写 Guid

-- 21. AppMediaExternalLink（媒体外链）
--     列：CreatorId, LastModifierId
UPDATE AppMediaExternalLink SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppMediaExternalLink SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- 22. AppMediaInfo（媒体信息）
--     列：CreatorId, LastModifierId
UPDATE AppMediaInfo SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppMediaInfo SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- 23. AppRssMirrorItem（RSS 镜像项）
--     列：CreatorId, LastModifierId
UPDATE AppRssMirrorItem SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppRssMirrorItem SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- 24. AppRssSource（RSS 源）
--     列：CreatorId
UPDATE AppRssSource SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);

-- 25. AppRssSubscriptions（RSS 订阅）
--     列：CreatorId, LastModifierId
UPDATE AppRssSubscriptions SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);
UPDATE AppRssSubscriptions SET LastModifierId = LOWER(LastModifierId) WHERE LastModifierId IS NOT NULL AND LastModifierId != LOWER(LastModifierId);

-- 26. AppRssSubscriptionDownloads（RSS 订阅下载）
--     列：CreatorId
UPDATE AppRssSubscriptionDownloads SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);

-- 27. AppRssWordSegment（RSS 分词）
--     列：CreatorId
UPDATE AppRssWordSegment SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);

-- 28. AppAria2TellStatusResult（Aria2 状态结果）
--     列：CreatorId
UPDATE AppAria2TellStatusResult SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);

-- 29. AppAria2FilesItem（Aria2 文件项）
--     列：CreatorId
UPDATE AppAria2FilesItem SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);

-- 30. AppAria2UrisItem（Aria2 URI 项）
--     列：CreatorId
UPDATE AppAria2UrisItem SET CreatorId = LOWER(CreatorId) WHERE CreatorId IS NOT NULL AND CreatorId != LOWER(CreatorId);

COMMIT;

-- 恢复外键约束检查
PRAGMA foreign_keys = ON;

-- ============================================================
-- 验证查询：确认没有大写 Guid 残留
-- ============================================================
-- 取消注释以下语句进行验证：
-- SELECT 'AbpUsers' as tbl, COUNT(*) FROM AbpUsers WHERE Id != LOWER(Id)
-- UNION ALL SELECT 'AbpRoles', COUNT(*) FROM AbpRoles WHERE Id != LOWER(Id)
-- UNION ALL SELECT 'AbpUserRoles', COUNT(*) FROM AbpUserRoles WHERE UserId != LOWER(UserId) OR RoleId != LOWER(RoleId)
-- UNION ALL SELECT 'AbpPermissionGrants', COUNT(*) FROM AbpPermissionGrants WHERE Id != LOWER(Id) OR ProviderKey != LOWER(ProviderKey)
-- UNION ALL SELECT 'AbpPermissionGroups', COUNT(*) FROM AbpPermissionGroups WHERE Id != LOWER(Id)
-- UNION ALL SELECT 'AbpPermissions', COUNT(*) FROM AbpPermissions WHERE Id != LOWER(Id)
-- UNION ALL SELECT 'AppElectricVehicle', COUNT(*) FROM AppElectricVehicle WHERE Id != LOWER(Id)
-- UNION ALL SELECT 'AppElectricVehicleChargingRecord', COUNT(*) FROM AppElectricVehicleChargingRecord WHERE Id != LOWER(Id) OR VehicleId != LOWER(VehicleId)
-- UNION ALL SELECT 'AppElectricVehicleCost', COUNT(*) FROM AppElectricVehicleCost WHERE Id != LOWER(Id) OR VehicleId != LOWER(VehicleId)
-- UNION ALL SELECT 'AppGasolinePrice', COUNT(*) FROM AppGasolinePrice WHERE Id != LOWER(Id)
-- UNION ALL SELECT 'AppLotterySimulation', COUNT(*) FROM AppLotterySimulation WHERE Id != LOWER(Id);
