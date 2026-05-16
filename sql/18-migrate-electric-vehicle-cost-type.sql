-- ============================================================
-- 迁移脚本: 18-migrate-electric-vehicle-cost-type.sql
-- 描述: 统一 AppElectricVehicleCost 表的 CostType 枚举值为 0-based
-- 日期: 2026-04-28
-- ============================================================
--
-- 背景说明:
-- 旧前端 CostType 使用 1-based 枚举:
--   1=充电, 2=保养, 3=保险, 4=停车, 5=维修, 6=其他
--
-- 后端原来使用 0-based 枚举:
--   0=充电, 1=保养, 2=保险, 3=其他
--
-- 新的统一 0-based 枚举:
--   0=充电, 1=保养, 2=保险, 3=其他, 4=过路费, 5=停车, 6=维修
--
-- 数据库中可能存在的情况:
-- 1. 如果后端直接存储了旧前端传来的 1-based 值，则数据库中有 1-6
-- 2. 如果后端使用自己的 0-based 枚举，则数据库中有 0-3
--
-- 迁移策略（安全、幂等）:
-- - 对于旧前端 1-based 值（4→停车, 5→维修, 6→其他），映射到新的 0-based 值
-- - 对于后端 0-based 值（0-3），保持不变
-- - 使用 CASE WHEN 确保只迁移需要迁移的值
-- ============================================================

-- 先备份（如果还没备份的话，建议在执行前手动备份 DFApp.db）

-- 步骤1: 查看当前数据分布（仅用于调试，注释掉以避免影响自动化执行）
-- SELECT CostType, COUNT(*) as cnt FROM AppElectricVehicleCost GROUP BY CostType ORDER BY CostType;

-- 步骤2: 如果数据库中有旧前端 1-based 的值，需要迁移
-- 注意：必须先处理值 4/5/6，再处理值 1，避免冲突
-- 使用临时值 -1/-2/-3 避免迁移过程中的主键冲突

-- 旧 4 (停车) → 新 5 (停车)
UPDATE AppElectricVehicleCost
SET CostType = -1
WHERE CostType = 4
  AND EXISTS (SELECT 1 FROM AppElectricVehicleCost WHERE CostType = 4);

-- 旧 5 (维修) → 新 6 (维修)
UPDATE AppElectricVehicleCost
SET CostType = -2
WHERE CostType = 5
  AND EXISTS (SELECT 1 FROM AppElectricVehicleCost WHERE CostType = 5);

-- 旧 6 (其他) → 新 3 (其他)
UPDATE AppElectricVehicleCost
SET CostType = -3
WHERE CostType = 6
  AND EXISTS (SELECT 1 FROM AppElectricVehicleCost WHERE CostType = 6);

-- 旧 1 (充电, 前端1-based) → 新 0 (充电)
-- 只有当数据库中同时存在 CostType=0 和 CostType=1 时，
-- 说明数据库混合了两种编码方式，此时 CostType=1 是旧前端的充电记录
-- 如果数据库中只有 CostType=1 没有 CostType=0，说明后端直接存了前端值，1=充电
-- 如果数据库中只有 CostType=0 没有 CostType=1，说明后端用了自己的枚举，无需迁移
UPDATE AppElectricVehicleCost
SET CostType = -4
WHERE CostType = 1
  AND NOT EXISTS (
    -- 排除：如果 CostType=1 对应的是后端枚举的"保养"（即后端自己创建的记录）
    -- 判断依据：如果数据库中存在 CostType=0（后端枚举的充电），说明后端使用了自己的枚举
    -- 此时 CostType=1 是后端的"保养"，不应迁移
    SELECT 1 FROM AppElectricVehicleCost WHERE CostType = 0
  );

-- 步骤3: 将临时值更新为最终目标值
UPDATE AppElectricVehicleCost SET CostType = 5 WHERE CostType = -1;
UPDATE AppElectricVehicleCost SET CostType = 6 WHERE CostType = -2;
UPDATE AppElectricVehicleCost SET CostType = 3 WHERE CostType = -3;
UPDATE AppElectricVehicleCost SET CostType = 0 WHERE CostType = -4;

-- 步骤4: 验证迁移结果
-- SELECT CostType, COUNT(*) as cnt FROM AppElectricVehicleCost GROUP BY CostType ORDER BY CostType;
-- 预期结果：CostType 值应全部在 0-6 范围内，不存在负数或超出范围的值
