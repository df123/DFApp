using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class Aria21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAria2TellStatusResult",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Bitfield = table.Column<string>(type: "TEXT", nullable: false),
                    CompletedLength = table.Column<long>(type: "INTEGER", nullable: false),
                    Connections = table.Column<long>(type: "INTEGER", nullable: false),
                    Dir = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    ErrorCode = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: false),
                    GID = table.Column<string>(type: "TEXT", nullable: false),
                    NumPieces = table.Column<long>(type: "INTEGER", nullable: false),
                    PieceLength = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    TotalLength = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadLength = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadSpeed = table.Column<long>(type: "INTEGER", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAria2TellStatusResult", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppAria2FilesItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompletedLength = table.Column<long>(type: "INTEGER", nullable: false),
                    Index = table.Column<long>(type: "INTEGER", nullable: false),
                    Length = table.Column<long>(type: "INTEGER", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    Selected = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResultId = table.Column<long>(type: "INTEGER", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAria2FilesItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAria2FilesItem_AppAria2TellStatusResult_ResultId",
                        column: x => x.ResultId,
                        principalTable: "AppAria2TellStatusResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppAria2UrisItem",
                columns: table => new
                {
                    Id = table.Column<short>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Uri = table.Column<string>(type: "TEXT", nullable: false),
                    FilesItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAria2UrisItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAria2UrisItem_AppAria2FilesItem_FilesItemId",
                        column: x => x.FilesItemId,
                        principalTable: "AppAria2FilesItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAria2FilesItem_ResultId",
                table: "AppAria2FilesItem",
                column: "ResultId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAria2UrisItem_FilesItemId",
                table: "AppAria2UrisItem",
                column: "FilesItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAria2UrisItem");

            migrationBuilder.DropTable(
                name: "AppAria2FilesItem");

            migrationBuilder.DropTable(
                name: "AppAria2TellStatusResult");
        }
    }
}
