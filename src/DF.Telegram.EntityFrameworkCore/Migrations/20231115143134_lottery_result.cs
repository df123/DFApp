using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DF.Telegram.Migrations
{
    /// <inheritdoc />
    public partial class lotteryresult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppLotteryResult",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", nullable: true),
                    DetailsLink = table.Column<string>(type: "TEXT", nullable: true),
                    VideoLink = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<string>(type: "TEXT", nullable: true),
                    Week = table.Column<string>(type: "TEXT", nullable: true),
                    Red = table.Column<string>(type: "TEXT", nullable: true),
                    Blue = table.Column<string>(type: "TEXT", nullable: true),
                    Blue2 = table.Column<string>(type: "TEXT", nullable: true),
                    Sales = table.Column<string>(type: "TEXT", nullable: true),
                    PoolMoney = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    AddMoney = table.Column<string>(type: "TEXT", nullable: true),
                    AddMoney2 = table.Column<string>(type: "TEXT", nullable: true),
                    Msg = table.Column<string>(type: "TEXT", nullable: true),
                    Z2Add = table.Column<string>(type: "TEXT", nullable: true),
                    M2Add = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLotteryResult", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLotteryPrizegrades",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LotteryResultId = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    TypeNum = table.Column<string>(type: "TEXT", nullable: true),
                    TypeMoney = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLotteryPrizegrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppLotteryPrizegrades_AppLotteryResult_LotteryResultId",
                        column: x => x.LotteryResultId,
                        principalTable: "AppLotteryResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });


            migrationBuilder.CreateIndex(
                name: "IX_AppLotteryPrizegrades_LotteryResultId",
                table: "AppLotteryPrizegrades",
                column: "LotteryResultId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "AppLotteryPrizegrades");

            migrationBuilder.DropTable(
                name: "AppLotteryResult");

        }
    }
}
