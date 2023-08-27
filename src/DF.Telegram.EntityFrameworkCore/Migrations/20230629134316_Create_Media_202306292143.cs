using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DF.Telegram.Migrations
{
    /// <inheritdoc />
    public partial class CreateMedia202306292143 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppMediaInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccessHash = table.Column<long>(type: "INTEGER", nullable: false),
                    TID = table.Column<long>(type: "INTEGER", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    IsDownload = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsReturn = table.Column<bool>(type: "INTEGER", nullable: false),
                    TaskComplete = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SavePath = table.Column<string>(type: "TEXT", nullable: true),
                    ValueSHA1 = table.Column<string>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_AppMediaInfo", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppMediaInfo");
        }
    }
}
