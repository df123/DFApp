using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DF.Telegram.Migrations
{
    /// <inheritdoc />
    public partial class LotteryResultAddUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppLotteryResult_Code",
                table: "AppLotteryResult",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppLotteryResult_Code",
                table: "AppLotteryResult");
        }
    }
}
