using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class Added_KeywordFilterRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppKeywordFilterRule",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Keyword = table.Column<string>(type: "TEXT", nullable: false),
                    MatchMode = table.Column<int>(type: "INTEGER", nullable: false),
                    FilterType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Remark = table.Column<string>(type: "TEXT", nullable: true),
                    IsCaseSensitive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppKeywordFilterRule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppKeywordFilterRule_FilterType",
                table: "AppKeywordFilterRule",
                column: "FilterType");

            migrationBuilder.CreateIndex(
                name: "IX_AppKeywordFilterRule_IsEnabled",
                table: "AppKeywordFilterRule",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AppKeywordFilterRule_IsEnabled_Priority",
                table: "AppKeywordFilterRule",
                columns: new[] { "IsEnabled", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_AppKeywordFilterRule_Priority",
                table: "AppKeywordFilterRule",
                column: "Priority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppKeywordFilterRule");
        }
    }
}
