-- 电动车里程记录表：表显里程的历史快照
-- 日期：2026-09-04
-- 背景：充电记录页已移除"当前里程"录入（按月统一登记模式），油电对比的区间行驶里程
--       改由独立里程记录计算——车辆管理页"里程"按钮每次更新都留一条快照。
-- 回填：从充电记录的历史里程值迁移初始快照（同日多条取最大值），
--       并把车辆当前总里程作为最新一条快照（避免重复）。

CREATE TABLE IF NOT EXISTS "AppElectricVehicleMileageRecord" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AppElectricVehicleMileageRecord" PRIMARY KEY,
    "VehicleId" TEXT NOT NULL,
    "Mileage" TEXT NOT NULL,
    "RecordedTime" TEXT NOT NULL,
    "Remark" TEXT NULL,
    "ConcurrencyStamp" TEXT NOT NULL,
    "CreationTime" TEXT NOT NULL,
    "CreatorId" TEXT NULL,
    "LastModificationTime" TEXT NULL,
    "LastModifierId" TEXT NULL,
    "DeleterId" TEXT NULL,
    "DeletionTime" TEXT NULL,
    CONSTRAINT "FK_AppElectricVehicleMileageRecord_AppElectricVehicle_VehicleId"
        FOREIGN KEY ("VehicleId") REFERENCES "AppElectricVehicle" ("Id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "IX_AppElectricVehicleMileageRecord_VehicleId"
    ON "AppElectricVehicleMileageRecord" ("VehicleId");
CREATE INDEX IF NOT EXISTS "IX_AppElectricVehicleMileageRecord_RecordedTime"
    ON "AppElectricVehicleMileageRecord" ("RecordedTime");

-- 回填充电记录中的历史里程快照（同车同日取最大里程）
INSERT INTO AppElectricVehicleMileageRecord
    (Id, VehicleId, Mileage, RecordedTime, ConcurrencyStamp, CreationTime)
SELECT lower(hex(randomblob(16))),
       VehicleId,
       MAX(CurrentMileage),
       date(ChargingDate) || ' 12:00:00',
       lower(hex(randomblob(16))),
       datetime('now')
FROM AppElectricVehicleChargingRecord
WHERE CurrentMileage IS NOT NULL
GROUP BY VehicleId, date(ChargingDate);

-- 车辆当前总里程作为最新快照（同日同值已存在则跳过）
INSERT INTO AppElectricVehicleMileageRecord
    (Id, VehicleId, Mileage, RecordedTime, ConcurrencyStamp, CreationTime)
SELECT lower(hex(randomblob(16))),
       v.Id,
       v.TotalMileage,
       COALESCE(v.MileageLastUpdatedTime, datetime('now')),
       lower(hex(randomblob(16))),
       datetime('now')
FROM AppElectricVehicle v
WHERE v.TotalMileage > 0
  AND NOT EXISTS (
      SELECT 1 FROM AppElectricVehicleMileageRecord m
      WHERE m.VehicleId = v.Id
        AND m.Mileage = v.TotalMileage
        AND date(m.RecordedTime) = date(COALESCE(v.MileageLastUpdatedTime, datetime('now')))
  );
