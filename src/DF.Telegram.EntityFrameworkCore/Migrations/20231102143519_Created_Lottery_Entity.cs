using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DF.Telegram.Migrations
{
    /// <inheritdoc />
    public partial class CreatedLotteryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppLottery",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IndexNo = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<string>(type: "TEXT", nullable: true),
                    ColorType = table.Column<string>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_AppLottery", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppLottery");
        }
    }
}
