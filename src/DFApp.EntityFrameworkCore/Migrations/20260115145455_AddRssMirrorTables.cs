using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRssMirrorTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRssMirrorItem",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RssSourceId = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Link = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    PublishDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Seeders = table.Column<int>(type: "INTEGER", nullable: true),
                    Leechers = table.Column<int>(type: "INTEGER", nullable: true),
                    Downloads = table.Column<int>(type: "INTEGER", nullable: true),
                    Extensions = table.Column<string>(type: "TEXT", nullable: true),
                    IsDownloaded = table.Column<bool>(type: "INTEGER", nullable: false),
                    DownloadTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRssMirrorItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRssSource",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    ProxyUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ProxyUsername = table.Column<string>(type: "TEXT", nullable: true),
                    ProxyPassword = table.Column<string>(type: "TEXT", nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    FetchIntervalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxItems = table.Column<int>(type: "INTEGER", nullable: false),
                    Query = table.Column<string>(type: "TEXT", nullable: true),
                    LastFetchTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FetchStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    Remark = table.Column<string>(type: "TEXT", nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRssSource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRssWordSegment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RssMirrorItemId = table.Column<long>(type: "INTEGER", nullable: false),
                    Word = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageType = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    PartOfSpeech = table.Column<string>(type: "TEXT", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRssWordSegment", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRssMirrorItem_CreationTime",
                table: "AppRssMirrorItem",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppRssMirrorItem_IsDownloaded",
                table: "AppRssMirrorItem",
                column: "IsDownloaded");

            migrationBuilder.CreateIndex(
                name: "IX_AppRssMirrorItem_PublishDate",
                table: "AppRssMirrorItem",
                column: "PublishDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppRssMirrorItem_RssSourceId",
                table: "AppRssMirrorItem",
                column: "RssSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRssSource_FetchStatus",
                table: "AppRssSource",
                column: "FetchStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AppRssSource_IsEnabled",
                table: "AppRssSource",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AppRssWordSegment_Count",
                table: "AppRssWordSegment",
                column: "Count");

            migrationBuilder.CreateIndex(
                name: "IX_AppRssWordSegment_LanguageType",
                table: "AppRssWordSegment",
                column: "LanguageType");

            migrationBuilder.CreateIndex(
                name: "IX_AppRssWordSegment_RssMirrorItemId",
                table: "AppRssWordSegment",
                column: "RssMirrorItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRssWordSegment_Word",
                table: "AppRssWordSegment",
                column: "Word");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRssMirrorItem");

            migrationBuilder.DropTable(
                name: "AppRssSource");

            migrationBuilder.DropTable(
                name: "AppRssWordSegment");
        }
    }
}
