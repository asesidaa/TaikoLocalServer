using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SongPlayDataSongIndex : Migration
    {
        /// <inheritdoc />
        // A no-op since the eras merge: this was written for the table before green's AddGreenEraSupport renamed it to
        // *_Nijiiro, and runs after that rename, so on a fresh database it would fail. IX_SongPlayData_SongId_Skipped: made by NijiiroScoreIndexes on SongPlayDatum_Nijiiro.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
