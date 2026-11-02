using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CleanupGreenDaniNormalBests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "SongBestDatum_Green"
                SET
                    "BestScore" = (
                        SELECT p."Score"
                        FROM "SongPlayDatum_Green" AS p
                        WHERE p."Baid" = "SongBestDatum_Green"."Baid"
                            AND p."SongId" = "SongBestDatum_Green"."SongId"
                            AND p."Difficulty" = "SongBestDatum_Green"."Difficulty"
                            AND p."IsShin" = 0
                            AND p."PlayMode" <> 1
                        ORDER BY p."Score" DESC, p."Id" DESC
                        LIMIT 1
                    ),
                    "BestRate" = (
                        SELECT p."ScoreRate"
                        FROM "SongPlayDatum_Green" AS p
                        WHERE p."Baid" = "SongBestDatum_Green"."Baid"
                            AND p."SongId" = "SongBestDatum_Green"."SongId"
                            AND p."Difficulty" = "SongBestDatum_Green"."Difficulty"
                            AND p."IsShin" = 0
                            AND p."PlayMode" <> 1
                        ORDER BY p."Score" DESC, p."Id" DESC
                        LIMIT 1
                    ),
                    "BestCrown" = (
                        SELECT MAX(p."Crown")
                        FROM "SongPlayDatum_Green" AS p
                        WHERE p."Baid" = "SongBestDatum_Green"."Baid"
                            AND p."SongId" = "SongBestDatum_Green"."SongId"
                            AND p."Difficulty" = "SongBestDatum_Green"."Difficulty"
                            AND p."IsShin" = 0
                            AND p."PlayMode" <> 1
                    )
                WHERE "IsShin" = 0
                    AND EXISTS (
                        SELECT 1
                        FROM "SongPlayDatum_Green" AS p
                        WHERE p."Baid" = "SongBestDatum_Green"."Baid"
                            AND p."SongId" = "SongBestDatum_Green"."SongId"
                            AND p."Difficulty" = "SongBestDatum_Green"."Difficulty"
                            AND p."IsShin" = 0
                            AND p."PlayMode" = 1
                    )
                    AND EXISTS (
                        SELECT 1
                        FROM "SongPlayDatum_Green" AS p
                        WHERE p."Baid" = "SongBestDatum_Green"."Baid"
                            AND p."SongId" = "SongBestDatum_Green"."SongId"
                            AND p."Difficulty" = "SongBestDatum_Green"."Difficulty"
                            AND p."IsShin" = 0
                            AND p."PlayMode" <> 1
                    );
                """);

            migrationBuilder.Sql(
                """
                DELETE FROM "SongBestDatum_Green"
                WHERE "IsShin" = 0
                    AND EXISTS (
                        SELECT 1
                        FROM "SongPlayDatum_Green" AS p
                        WHERE p."Baid" = "SongBestDatum_Green"."Baid"
                            AND p."SongId" = "SongBestDatum_Green"."SongId"
                            AND p."Difficulty" = "SongBestDatum_Green"."Difficulty"
                            AND p."IsShin" = 0
                            AND p."PlayMode" = 1
                    )
                    AND NOT EXISTS (
                        SELECT 1
                        FROM "SongPlayDatum_Green" AS p
                        WHERE p."Baid" = "SongBestDatum_Green"."Baid"
                            AND p."SongId" = "SongBestDatum_Green"."SongId"
                            AND p."Difficulty" = "SongBestDatum_Green"."Difficulty"
                            AND p."IsShin" = 0
                            AND p."PlayMode" <> 1
                    );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data cleanup is intentionally irreversible.
        }
    }
}
