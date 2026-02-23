using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.src.DFApp.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleNavigationToChargingRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_AppElectricVehicleChargingRecord_AppElectricVehicle_VehicleId",
                table: "AppElectricVehicleChargingRecord",
                column: "VehicleId",
                principalTable: "AppElectricVehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppElectricVehicleChargingRecord_AppElectricVehicle_VehicleId",
                table: "AppElectricVehicleChargingRecord");
        }
    }
}
