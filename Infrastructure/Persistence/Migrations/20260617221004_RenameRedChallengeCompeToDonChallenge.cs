using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameRedChallengeCompeToDonChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "RedChallengeCompeProgress",
                newName: "RedDonChallengeProgress");

            migrationBuilder.RenameTable(
                name: "RedChallengeCompeRawFacts",
                newName: "RedDonChallengeRawFacts");

            migrationBuilder.DropIndex(
                name: "IX_RedChallengeCompeProgress_BundleId_TaskId_TrackNo",
                table: "RedDonChallengeProgress");

            migrationBuilder.DropIndex(
                name: "IX_RedChallengeCompeRawFacts_Baid_BundleId_TaskId_TrackNo_PlayTime",
                table: "RedDonChallengeRawFacts");

            migrationBuilder.CreateIndex(
                name: "IX_RedDonChallengeProgress_BundleId_TaskId_TrackNo",
                table: "RedDonChallengeProgress",
                columns: new[] { "BundleId", "TaskId", "TrackNo" });

            migrationBuilder.CreateIndex(
                name: "IX_RedDonChallengeRawFacts_Baid_BundleId_TaskId_TrackNo_PlayTime",
                table: "RedDonChallengeRawFacts",
                columns: new[] { "Baid", "BundleId", "TaskId", "TrackNo", "PlayTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RedDonChallengeProgress_BundleId_TaskId_TrackNo",
                table: "RedDonChallengeProgress");

            migrationBuilder.DropIndex(
                name: "IX_RedDonChallengeRawFacts_Baid_BundleId_TaskId_TrackNo_PlayTime",
                table: "RedDonChallengeRawFacts");

            migrationBuilder.RenameTable(
                name: "RedDonChallengeProgress",
                newName: "RedChallengeCompeProgress");

            migrationBuilder.RenameTable(
                name: "RedDonChallengeRawFacts",
                newName: "RedChallengeCompeRawFacts");

            migrationBuilder.CreateIndex(
                name: "IX_RedChallengeCompeProgress_BundleId_TaskId_TrackNo",
                table: "RedChallengeCompeProgress",
                columns: new[] { "BundleId", "TaskId", "TrackNo" });

            migrationBuilder.CreateIndex(
                name: "IX_RedChallengeCompeRawFacts_Baid_BundleId_TaskId_TrackNo_PlayTime",
                table: "RedChallengeCompeRawFacts",
                columns: new[] { "Baid", "BundleId", "TaskId", "TrackNo", "PlayTime" });
        }
    }
}
