BEGIN TRANSACTION;
CREATE TABLE "ef_temp_AppElectricVehicleChargingRecord" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AppElectricVehicleChargingRecord" PRIMARY KEY,
    "Amount" TEXT NOT NULL,
    "ChargingDate" TEXT NOT NULL,
    "ConcurrencyStamp" TEXT NOT NULL,
    "CreationTime" TEXT NOT NULL,
    "CreatorId" TEXT NULL,
    "CurrentMileage" TEXT NULL,
    "Energy" TEXT NULL,
    "ExtraProperties" TEXT NOT NULL,
    "LastModificationTime" TEXT NULL,
    "LastModifierId" TEXT NULL,
    "VehicleId" TEXT NOT NULL,
    CONSTRAINT "FK_AppElectricVehicleChargingRecord_AppElectricVehicle_VehicleId" FOREIGN KEY ("VehicleId") REFERENCES "AppElectricVehicle" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_AppElectricVehicleChargingRecord" ("Id", "Amount", "ChargingDate", "ConcurrencyStamp", "CreationTime", "CreatorId", "CurrentMileage", "Energy", "ExtraProperties", "LastModificationTime", "LastModifierId", "VehicleId")
SELECT "Id", "Amount", "ChargingDate", "ConcurrencyStamp", "CreationTime", "CreatorId", "CurrentMileage", "Energy", "ExtraProperties", "LastModificationTime", "LastModifierId", "VehicleId"
FROM "AppElectricVehicleChargingRecord";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "AppElectricVehicleChargingRecord";

ALTER TABLE "ef_temp_AppElectricVehicleChargingRecord" RENAME TO "AppElectricVehicleChargingRecord";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_AppElectricVehicleChargingRecord_ChargingDate" ON "AppElectricVehicleChargingRecord" ("ChargingDate");

CREATE INDEX "IX_AppElectricVehicleChargingRecord_VehicleId" ON "AppElectricVehicleChargingRecord" ("VehicleId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260223131241_AddVehicleNavigationToChargingRecord', '10.0.1');

