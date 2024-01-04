using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLotteryIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppLotteryResult_Code",
                table: "AppLotteryResult");

            migrationBuilder.CreateIndex(
                name: "IX_AppLotteryResult_Code_Name",
                table: "AppLotteryResult",
                columns: new[] { "Code", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppLotteryResult_Code_Name",
                table: "AppLotteryResult");

            migrationBuilder.CreateIndex(
                name: "IX_AppLotteryResult_Code",
                table: "AppLotteryResult",
                column: "Code",
                unique: true);
        }
    }
}
