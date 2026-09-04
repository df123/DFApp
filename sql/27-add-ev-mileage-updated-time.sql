-- 电动车表新增"总里程最后更新时间"列
-- 日期：2026-09-04
-- 背景：当前总里程原先只能随充电记录新增/编辑联动更新，不充电时无法单独记录；
--       家用充电按月结算、外部充电穿插，里程记录与充电记录天然不同步。
-- 说明：新增独立里程更新接口（PUT /api/app/electric-vehicle/{id}/mileage）及
--       车辆管理页"更新里程"入口，本列记录里程值最后更新时间用于展示数据新鲜度；
--       充电记录联动更新里程时同样刷新本列。

ALTER TABLE AppElectricVehicle ADD COLUMN MileageLastUpdatedTime TEXT;
