using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class AddElectricVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppElectricVehicle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Brand = table.Column<string>(type: "TEXT", nullable: true),
                    Model = table.Column<string>(type: "TEXT", nullable: true),
                    LicensePlate = table.Column<string>(type: "TEXT", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BatteryCapacity = table.Column<decimal>(type: "TEXT", nullable: true),
                    TotalMileage = table.Column<decimal>(type: "TEXT", nullable: false, defaultValue: 0m),
                    Remark = table.Column<string>(type: "TEXT", nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppElectricVehicle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppGasolinePrice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Province = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Price0H = table.Column<decimal>(type: "TEXT", nullable: true),
                    Price89H = table.Column<decimal>(type: "TEXT", nullable: true),
                    Price90H = table.Column<decimal>(type: "TEXT", nullable: true),
                    Price92H = table.Column<decimal>(type: "TEXT", nullable: true),
                    Price93H = table.Column<decimal>(type: "TEXT", nullable: true),
                    Price95H = table.Column<decimal>(type: "TEXT", nullable: true),
                    Price97H = table.Column<decimal>(type: "TEXT", nullable: true),
                    Price98H = table.Column<decimal>(type: "TEXT", nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppGasolinePrice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppElectricVehicleChargingRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VehicleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChargingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StationName = table.Column<string>(type: "TEXT", nullable: true),
                    ChargingDuration = table.Column<int>(type: "INTEGER", nullable: true),
                    Energy = table.Column<decimal>(type: "TEXT", nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    StartSOC = table.Column<int>(type: "INTEGER", nullable: true),
                    EndSOC = table.Column<int>(type: "INTEGER", nullable: true),
                    IsBelongToSelf = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Remark = table.Column<string>(type: "TEXT", nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppElectricVehicleChargingRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppElectricVehicleChargingRecord_AppElectricVehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "AppElectricVehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppElectricVehicleCost",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VehicleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CostType = table.Column<int>(type: "INTEGER", nullable: false),
                    CostDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsBelongToSelf = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Remark = table.Column<string>(type: "TEXT", nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppElectricVehicleCost", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppElectricVehicleCost_AppElectricVehicle_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "AppElectricVehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppElectricVehicleChargingRecord_ChargingDate",
                table: "AppElectricVehicleChargingRecord",
                column: "ChargingDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppElectricVehicleChargingRecord_VehicleId",
                table: "AppElectricVehicleChargingRecord",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppElectricVehicleCost_CostDate",
                table: "AppElectricVehicleCost",
                column: "CostDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppElectricVehicleCost_CostType",
                table: "AppElectricVehicleCost",
                column: "CostType");

            migrationBuilder.CreateIndex(
                name: "IX_AppElectricVehicleCost_VehicleId",
                table: "AppElectricVehicleCost",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppGasolinePrice_Date",
                table: "AppGasolinePrice",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_AppGasolinePrice_Province",
                table: "AppGasolinePrice",
                column: "Province");

            migrationBuilder.CreateIndex(
                name: "IX_AppGasolinePrice_Province_Date",
                table: "AppGasolinePrice",
                columns: new[] { "Province", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppElectricVehicleChargingRecord");

            migrationBuilder.DropTable(
                name: "AppElectricVehicleCost");

            migrationBuilder.DropTable(
                name: "AppGasolinePrice");

            migrationBuilder.DropTable(
                name: "AppElectricVehicle");
        }
    }
}
