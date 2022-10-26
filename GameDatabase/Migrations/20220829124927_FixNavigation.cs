using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Migrations
{
    /// <inheritdoc />
    public partial class FixNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanStageScoreData_DanScoreData_DanScoreDatumBaid_DanScoreDatumDanId",
                table: "DanStageScoreData");

            migrationBuilder.DropIndex(
                name: "IX_DanStageScoreData_DanScoreDatumBaid_DanScoreDatumDanId",
                table: "DanStageScoreData");

            migrationBuilder.DropColumn(
                name: "DanScoreDatumBaid",
                table: "DanStageScoreData");

            migrationBuilder.DropColumn(
                name: "DanScoreDatumDanId",
                table: "DanStageScoreData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "DanScoreDatumBaid",
                table: "DanStageScoreData",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "DanScoreDatumDanId",
                table: "DanStageScoreData",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanStageScoreData_DanScoreDatumBaid_DanScoreDatumDanId",
                table: "DanStageScoreData",
                columns: new[] { "DanScoreDatumBaid", "DanScoreDatumDanId" });

            migrationBuilder.AddForeignKey(
                name: "FK_DanStageScoreData_DanScoreData_DanScoreDatumBaid_DanScoreDatumDanId",
                table: "DanStageScoreData",
                columns: new[] { "DanScoreDatumBaid", "DanScoreDatumDanId" },
                principalTable: "DanScoreData",
                principalColumns: new[] { "Baid", "DanId" });
        }
    }
}
