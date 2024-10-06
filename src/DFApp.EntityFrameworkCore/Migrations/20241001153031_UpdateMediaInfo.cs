using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMediaInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessHash",
                table: "AppMediaInfo");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppMediaInfo");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "AppMediaInfo");

            migrationBuilder.RenameColumn(
                name: "ValueSHA1",
                table: "AppMediaInfo",
                newName: "FileMessage");

            migrationBuilder.RenameColumn(
                name: "TID",
                table: "AppMediaInfo",
                newName: "ChatId");

            migrationBuilder.AlterColumn<string>(
                name: "SavePath",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MimeType",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChatTitle",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SHA256",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatTitle",
                table: "AppMediaInfo");

            migrationBuilder.DropColumn(
                name: "SHA256",
                table: "AppMediaInfo");

            migrationBuilder.RenameColumn(
                name: "FileMessage",
                table: "AppMediaInfo",
                newName: "ValueSHA1");

            migrationBuilder.RenameColumn(
                name: "ChatId",
                table: "AppMediaInfo",
                newName: "TID");

            migrationBuilder.AlterColumn<string>(
                name: "SavePath",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "MimeType",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<long>(
                name: "AccessHash",
                table: "AppMediaInfo",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppMediaInfo",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: true);
        }
    }
}
