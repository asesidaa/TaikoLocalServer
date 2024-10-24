using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDatabase.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChallengeCompetition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChallengeCompeteData_UserData_Baid",
                table: "ChallengeCompeteData");

            migrationBuilder.DropIndex(
                name: "IX_ChallengeCompeteData_Baid",
                table: "ChallengeCompeteData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ChallengeCompeteData_Baid",
                table: "ChallengeCompeteData",
                column: "Baid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengeCompeteData_UserData_Baid",
                table: "ChallengeCompeteData",
                column: "Baid",
                principalTable: "UserData",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
