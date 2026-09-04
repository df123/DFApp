-- 删除充电记录表的"当前里程"列
-- 日期：2026-09-04
-- 前置：必须先执行 sql/28（把充电记录中的历史里程值迁移为里程快照记录），
--       本列删除后充电记录不再保存里程，统计的区间里程与油费分段全部改用
--       AppElectricVehicleMileageRecord 快照计算。
-- 顺序要求：生产部署按 27 → 28 → 29 → 新后端 执行。

ALTER TABLE "AppElectricVehicleChargingRecord" DROP COLUMN "CurrentMileage";
