using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DF.Telegram.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMediainfoEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDownload",
                table: "AppMediaInfo");

            migrationBuilder.DropColumn(
                name: "IsReturn",
                table: "AppMediaInfo");

            migrationBuilder.RenameColumn(
                name: "TaskComplete",
                table: "AppMediaInfo",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "AppMediaInfo");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "AppMediaInfo",
                newName: "TaskComplete");

            migrationBuilder.AddColumn<bool>(
                name: "IsDownload",
                table: "AppMediaInfo",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReturn",
                table: "AppMediaInfo",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
