-- ============================================================
-- 30: 修复里程快照表 Id 存储格式不一致
-- ============================================================
-- 问题：sql/28 迁移时用 lower(hex(randomblob(16))) 生成的 Id
--       为 32 位无连字符字符串（如 0c271b60b86cec30...），
--       而 SqlSugar 按 Guid 主键查询时生成带连字符串
--       （0c271b60-b86c-ec30-...），SQLite TEXT 精确匹配失败，
--       导致"删除里程记录"接口报 400 里程记录不存在。
-- 修复：将存量无连字符 Id 统一规范化为带连字符小写格式。
-- 幂等：仅处理长度为 32 的无连字符 Id，可重复执行。
-- 顺序：可在新后端部署前或后执行，互不影响。
-- ============================================================

UPDATE "AppElectricVehicleMileageRecord"
SET "Id" = lower(
    substr("Id", 1, 8) || '-' ||
    substr("Id", 9, 4) || '-' ||
    substr("Id", 13, 4) || '-' ||
    substr("Id", 17, 4) || '-' ||
    substr("Id", 21, 12)
)
WHERE length("Id") = 32;

-- 验证：应无剩余 32 位无连字符 Id
-- SELECT COUNT(*) FROM "AppElectricVehicleMileageRecord" WHERE length("Id") = 32;
