using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillGreenDisplayDanFromGotDanMax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "UserSaveData_Green"
                SET "DispTaikojukuDan" = CASE
                    WHEN "GotDanMax" >= 25 THEN 25
                    ELSE "GotDanMax" + 1
                END
                WHERE "GotDanMax" >= 1
                    AND (
                        "DispTaikojukuDan" = 0
                        OR "DispTaikojukuDan" <= "GotDanMax"
                        OR "DispTaikojukuDan" > 25
                    );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data backfill is intentionally irreversible.
        }
    }
}
