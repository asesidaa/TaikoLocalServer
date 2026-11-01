using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SongBestDataChartIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SongBestData_SongId_Difficulty_BestScore",
                table: "SongBestData",
                columns: new[] { "SongId", "Difficulty", "BestScore" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SongBestData_SongId_Difficulty_BestScore",
                table: "SongBestData");
        }
    }
}
