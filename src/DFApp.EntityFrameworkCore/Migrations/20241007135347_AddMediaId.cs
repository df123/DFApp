using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaIds",
                table: "AppMediaExternalLink");

            migrationBuilder.RenameColumn(
                name: "IsFileDeleted",
                table: "AppMediaInfo",
                newName: "MediaId");

            migrationBuilder.CreateTable(
                name: "AppMediaExternalLinkMediaIds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MediaId = table.Column<long>(type: "INTEGER", nullable: false),
                    MediaExternalLinkId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMediaExternalLinkMediaIds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppMediaExternalLinkMediaIds_AppMediaExternalLink_MediaExternalLinkId",
                        column: x => x.MediaExternalLinkId,
                        principalTable: "AppMediaExternalLink",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppMediaInfo_MediaId",
                table: "AppMediaInfo",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMediaExternalLinkMediaIds_MediaExternalLinkId",
                table: "AppMediaExternalLinkMediaIds",
                column: "MediaExternalLinkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppMediaExternalLinkMediaIds");

            migrationBuilder.DropIndex(
                name: "IX_AppMediaInfo_MediaId",
                table: "AppMediaInfo");

            migrationBuilder.RenameColumn(
                name: "MediaId",
                table: "AppMediaInfo",
                newName: "IsFileDeleted");

            migrationBuilder.AddColumn<string>(
                name: "MediaIds",
                table: "AppMediaExternalLink",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
