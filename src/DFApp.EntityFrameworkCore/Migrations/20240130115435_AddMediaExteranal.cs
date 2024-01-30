using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaExteranal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExternalLinkGenerated",
                table: "AppMediaInfo",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFileDeleted",
                table: "AppMediaInfo",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "AppConfigurationInfo",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AppMediaExternalLink",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    TimeConsumed = table.Column<long>(type: "INTEGER", nullable: false),
                    IsRemove = table.Column<bool>(type: "INTEGER", nullable: false),
                    LinkContent = table.Column<string>(type: "TEXT", nullable: false),
                    MediaIds = table.Column<string>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMediaExternalLink", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppMediaExternalLink");

            migrationBuilder.DropColumn(
                name: "IsExternalLinkGenerated",
                table: "AppMediaInfo");

            migrationBuilder.DropColumn(
                name: "IsFileDeleted",
                table: "AppMediaInfo");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "AppConfigurationInfo");
        }
    }
}
