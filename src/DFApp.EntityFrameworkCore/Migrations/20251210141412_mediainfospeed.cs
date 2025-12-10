using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class mediainfospeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DownloadSpeedBps",
                table: "AppMediaInfo",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "DownloadTimeMs",
                table: "AppMediaInfo",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadSpeedBps",
                table: "AppMediaInfo");

            migrationBuilder.DropColumn(
                name: "DownloadTimeMs",
                table: "AppMediaInfo");
        }
    }
}
