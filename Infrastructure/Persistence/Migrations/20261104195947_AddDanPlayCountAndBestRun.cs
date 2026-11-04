using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDanPlayCountAndBestRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BestRunAt",
                table: "DanScoreDatum_Nijiiro",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "PlayCount",
                table: "DanScoreDatum_Nijiiro",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BestRunAt",
                table: "DanScoreDatum_Nijiiro");

            migrationBuilder.DropColumn(
                name: "PlayCount",
                table: "DanScoreDatum_Nijiiro");
        }
    }
}
