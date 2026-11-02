using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NijiiroScoreIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A database made before the eras merge has our score indexes under the tables' old names (renaming a table
            // keeps them); two of them now repeat green's. Fresh databases never had them (those migrations are no-ops).
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_SongPlayData_SongId_Skipped;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_SongPlayData_Baid_PlayTime;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_SongPlayData_Baid_SongId_Difficulty_PlayTime;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_SongBestData_SongId_Difficulty_BestScore;");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayDatum_Nijiiro_Baid_PlayTime",
                table: "SongPlayDatum_Nijiiro",
                columns: new[] { "Baid", "PlayTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayDatum_Nijiiro_SongId_Skipped",
                table: "SongPlayDatum_Nijiiro",
                columns: new[] { "SongId", "Skipped" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SongPlayDatum_Nijiiro_Baid_PlayTime",
                table: "SongPlayDatum_Nijiiro");

            migrationBuilder.DropIndex(
                name: "IX_SongPlayDatum_Nijiiro_SongId_Skipped",
                table: "SongPlayDatum_Nijiiro");
        }
    }
}
