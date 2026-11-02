using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SongPlayDataPlayerSongIndex : Migration
    {
        /// <inheritdoc />
        // A no-op since the eras merge: this was written for the table before green's AddGreenEraSupport renamed it to
        // *_Nijiiro, and runs after that rename, so on a fresh database it would fail. IX_SongPlayData_Baid_SongId_Difficulty_PlayTime: green's AddSongBestAndPlayCompositeIndexes makes the same index on SongPlayDatum_Nijiiro.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
