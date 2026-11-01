using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SongPlayDataPlayerSongIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SongPlayData_Baid",
                table: "SongPlayData");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayData_Baid_SongId_Difficulty_PlayTime",
                table: "SongPlayData",
                columns: new[] { "Baid", "SongId", "Difficulty", "PlayTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SongPlayData_Baid_SongId_Difficulty_PlayTime",
                table: "SongPlayData");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayData_Baid",
                table: "SongPlayData",
                column: "Baid");
        }
    }
}
