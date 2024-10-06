using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMediaInfo2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SHA256",
                table: "AppMediaInfo",
                newName: "MD5");

            migrationBuilder.RenameColumn(
                name: "FileMessage",
                table: "AppMediaInfo",
                newName: "Message");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Message",
                table: "AppMediaInfo",
                newName: "FileMessage");

            migrationBuilder.RenameColumn(
                name: "MD5",
                table: "AppMediaInfo",
                newName: "SHA256");
        }
    }
}
