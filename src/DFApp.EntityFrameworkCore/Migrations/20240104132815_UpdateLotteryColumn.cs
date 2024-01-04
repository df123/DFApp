using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLotteryColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "lotteryType",
                table: "AppLottery",
                newName: "LotteryType");

            migrationBuilder.AlterColumn<string>(
                name: "LotteryType",
                table: "AppLottery",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LotteryType",
                table: "AppLottery",
                newName: "lotteryType");

            migrationBuilder.AlterColumn<int>(
                name: "lotteryType",
                table: "AppLottery",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
