using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbpBlobContainers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpBlobContainers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDynamicIP",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IP = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<string>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDynamicIP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLottery",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IndexNo = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<string>(type: "TEXT", nullable: true),
                    ColorType = table.Column<string>(type: "TEXT", nullable: true),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLottery", x => x.Id);
                });

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
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
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
                name: "AppMediaInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccessHash = table.Column<long>(type: "INTEGER", nullable: false),
                    TID = table.Column<long>(type: "INTEGER", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    SavePath = table.Column<string>(type: "TEXT", nullable: true),
                    ValueSHA1 = table.Column<string>(type: "TEXT", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMediaInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpBlobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContainerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Content = table.Column<byte[]>(type: "BLOB", maxLength: 2147483647, nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpBlobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpBlobs_AbpBlobContainers_ContainerId",
                        column: x => x.ContainerId,
                        principalTable: "AbpBlobContainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
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
                name: "IX_AbpBlobContainers_TenantId_Name",
                table: "AbpBlobContainers",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpBlobs_ContainerId",
                table: "AbpBlobs",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpBlobs_TenantId_ContainerId_Name",
                table: "AbpBlobs",
                columns: new[] { "TenantId", "ContainerId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AppLotteryPrizegrades_LotteryResultId",
                table: "AppLotteryPrizegrades",
                column: "LotteryResultId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLotteryResult_Code",
                table: "AppLotteryResult",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbpBlobs");

            migrationBuilder.DropTable(
                name: "AppDynamicIP");

            migrationBuilder.DropTable(
                name: "AppLottery");

            migrationBuilder.DropTable(
                name: "AppLotteryPrizegrades");

            migrationBuilder.DropTable(
                name: "AppMediaInfo");

            migrationBuilder.DropTable(
                name: "AbpBlobContainers");

            migrationBuilder.DropTable(
                name: "AppLotteryResult");
        }
    }
}
