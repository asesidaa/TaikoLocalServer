using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSongBestAndPlayCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AddGreenEraSupport renamed SongPlayData -> SongPlayDatum_Nijiiro but didn't
            // rename the attached IX_SongPlayData_Baid index (SQLite's ALTER TABLE RENAME
            // leaves index names untouched). Drop by every known historical name so this
            // migration is idempotent across both legacy and freshly-built databases.
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_SongPlayData_Baid"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_SongPlayDatum_Nijiiro_Baid"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_SongPlayDatum_Green_Baid"";");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayDatum_Nijiiro_Baid_SongId_Difficulty_PlayTime",
                table: "SongPlayDatum_Nijiiro",
                columns: new[] { "Baid", "SongId", "Difficulty", "PlayTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayDatum_Green_Baid_SongId_Difficulty_PlayTime",
                table: "SongPlayDatum_Green",
                columns: new[] { "Baid", "SongId", "Difficulty", "PlayTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SongBestDatum_Nijiiro_SongId_Difficulty_BestScore",
                table: "SongBestDatum_Nijiiro",
                columns: new[] { "SongId", "Difficulty", "BestScore" });

            migrationBuilder.CreateIndex(
                name: "IX_SongBestDatum_Green_SongId_Difficulty_BestScore",
                table: "SongBestDatum_Green",
                columns: new[] { "SongId", "Difficulty", "BestScore" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SongPlayDatum_Nijiiro_Baid_SongId_Difficulty_PlayTime",
                table: "SongPlayDatum_Nijiiro");

            migrationBuilder.DropIndex(
                name: "IX_SongPlayDatum_Green_Baid_SongId_Difficulty_PlayTime",
                table: "SongPlayDatum_Green");

            migrationBuilder.DropIndex(
                name: "IX_SongBestDatum_Nijiiro_SongId_Difficulty_BestScore",
                table: "SongBestDatum_Nijiiro");

            migrationBuilder.DropIndex(
                name: "IX_SongBestDatum_Green_SongId_Difficulty_BestScore",
                table: "SongBestDatum_Green");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayDatum_Nijiiro_Baid",
                table: "SongPlayDatum_Nijiiro",
                column: "Baid");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayDatum_Green_Baid",
                table: "SongPlayDatum_Green",
                column: "Baid");
        }
    }
}
