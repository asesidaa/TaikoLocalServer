using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDanSetToDanScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Written out rather than generated: EF fills the rebuilt per-song table while foreign keys are still on, and
            // its new link to the results table's (not yet rebuilt) four-column key fails with "foreign key mismatch".
            // So both tables are rebuilt with foreign keys off. Every existing row gets an empty DanSet; at startup the
            // server files the Normal ones under the default dan set (Program.cs).
            migrationBuilder.Sql("PRAGMA foreign_keys = 0;", suppressTransaction: true);
            migrationBuilder.Sql("""
                CREATE TABLE "ef_temp_DanScoreDatum_Nijiiro" (
                    "Baid" INTEGER NOT NULL,
                    "DanSet" TEXT NOT NULL DEFAULT '',
                    "DanId" INTEGER NOT NULL,
                    "DanType" INTEGER NOT NULL DEFAULT 1,
                    "ArrivalSongCount" INTEGER NOT NULL,
                    "BestRunAt" TEXT NULL,
                    "ClearState" INTEGER NOT NULL DEFAULT 0,
                    "ComboCountTotal" INTEGER NOT NULL,
                    "PlayCount" INTEGER NOT NULL,
                    "SoulGaugeTotal" INTEGER NOT NULL,
                    CONSTRAINT "PK_DanScoreDatum_Nijiiro" PRIMARY KEY ("Baid", "DanSet", "DanId", "DanType"),
                    CONSTRAINT "FK_DanScoreDatum_Nijiiro_UserData_Baid" FOREIGN KEY ("Baid") REFERENCES "UserData" ("Baid") ON DELETE CASCADE
                );
                INSERT INTO "ef_temp_DanScoreDatum_Nijiiro" ("Baid", "DanSet", "DanId", "DanType", "ArrivalSongCount", "BestRunAt", "ClearState", "ComboCountTotal", "PlayCount", "SoulGaugeTotal")
                SELECT "Baid", '', "DanId", "DanType", "ArrivalSongCount", "BestRunAt", "ClearState", "ComboCountTotal", "PlayCount", "SoulGaugeTotal"
                FROM "DanScoreDatum_Nijiiro";
                CREATE TABLE "ef_temp_DanStageScoreDatum_Nijiiro" (
                    "Baid" INTEGER NOT NULL,
                    "DanSet" TEXT NOT NULL DEFAULT '',
                    "DanId" INTEGER NOT NULL,
                    "DanType" INTEGER NOT NULL DEFAULT 1,
                    "SongNumber" INTEGER NOT NULL,
                    "BadCount" INTEGER NOT NULL,
                    "ComboCount" INTEGER NOT NULL,
                    "DrumrollCount" INTEGER NOT NULL,
                    "GoodCount" INTEGER NOT NULL,
                    "HighScore" INTEGER NOT NULL,
                    "OkCount" INTEGER NOT NULL,
                    "PlayScore" INTEGER NOT NULL,
                    "TotalHitCount" INTEGER NOT NULL,
                    CONSTRAINT "PK_DanStageScoreDatum_Nijiiro" PRIMARY KEY ("Baid", "DanSet", "DanId", "DanType", "SongNumber"),
                    CONSTRAINT "FK_DanStageScoreDatum_Nijiiro_DanScoreDatum_Nijiiro_Baid_DanSet_DanId_DanType" FOREIGN KEY ("Baid", "DanSet", "DanId", "DanType") REFERENCES "DanScoreDatum_Nijiiro" ("Baid", "DanSet", "DanId", "DanType") ON DELETE CASCADE
                );
                INSERT INTO "ef_temp_DanStageScoreDatum_Nijiiro" ("Baid", "DanSet", "DanId", "DanType", "SongNumber", "BadCount", "ComboCount", "DrumrollCount", "GoodCount", "HighScore", "OkCount", "PlayScore", "TotalHitCount")
                SELECT "Baid", '', "DanId", "DanType", "SongNumber", "BadCount", "ComboCount", "DrumrollCount", "GoodCount", "HighScore", "OkCount", "PlayScore", "TotalHitCount"
                FROM "DanStageScoreDatum_Nijiiro";
                DROP TABLE "DanStageScoreDatum_Nijiiro";
                DROP TABLE "DanScoreDatum_Nijiiro";
                ALTER TABLE "ef_temp_DanScoreDatum_Nijiiro" RENAME TO "DanScoreDatum_Nijiiro";
                ALTER TABLE "ef_temp_DanStageScoreDatum_Nijiiro" RENAME TO "DanStageScoreDatum_Nijiiro";
                """);
            migrationBuilder.Sql("PRAGMA foreign_keys = 1;", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanStageScoreDatum_Nijiiro_DanScoreDatum_Nijiiro_Baid_DanSet_DanId_DanType",
                table: "DanStageScoreDatum_Nijiiro");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DanStageScoreDatum_Nijiiro",
                table: "DanStageScoreDatum_Nijiiro");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DanScoreDatum_Nijiiro",
                table: "DanScoreDatum_Nijiiro");

            migrationBuilder.DropColumn(
                name: "DanSet",
                table: "DanStageScoreDatum_Nijiiro");

            migrationBuilder.DropColumn(
                name: "DanSet",
                table: "DanScoreDatum_Nijiiro");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DanStageScoreDatum_Nijiiro",
                table: "DanStageScoreDatum_Nijiiro",
                columns: new[] { "Baid", "DanId", "DanType", "SongNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DanScoreDatum_Nijiiro",
                table: "DanScoreDatum_Nijiiro",
                columns: new[] { "Baid", "DanId", "DanType" });

            migrationBuilder.AddForeignKey(
                name: "FK_DanStageScoreDatum_Nijiiro_DanScoreDatum_Nijiiro_Baid_DanId_DanType",
                table: "DanStageScoreDatum_Nijiiro",
                columns: new[] { "Baid", "DanId", "DanType" },
                principalTable: "DanScoreDatum_Nijiiro",
                principalColumns: new[] { "Baid", "DanId", "DanType" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
