using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DFApp.Migrations
{
    /// <inheritdoc />
    public partial class RemovedMediaInfoColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "AppMediaInfo");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "AppMediaInfo");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "AppMediaInfo");

            migrationBuilder.DropColumn(
                name: "MD5",
                table: "AppMediaInfo");

            migrationBuilder.AddColumn<bool>(
                name: "IsDownloadCompleted",
                table: "AppMediaInfo",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDownloadCompleted",
                table: "AppMediaInfo");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MD5",
                table: "AppMediaInfo",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
