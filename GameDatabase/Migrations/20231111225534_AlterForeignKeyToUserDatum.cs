using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AlterForeignKeyToUserDatum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiScoreData_Card_Baid",
                table: "AiScoreData");

            migrationBuilder.DropForeignKey(
                name: "FK_Credential_Card_Baid",
                table: "Credential");

            migrationBuilder.DropForeignKey(
                name: "FK_DanScoreData_Card_Baid",
                table: "DanScoreData");

            migrationBuilder.DropForeignKey(
                name: "FK_SongBestData_Card_Baid",
                table: "SongBestData");

            migrationBuilder.DropForeignKey(
                name: "FK_SongPlayData_Card_Baid",
                table: "SongPlayData");

            migrationBuilder.DropForeignKey(
                name: "FK_UserData_Card_Baid",
                table: "UserData");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Card_Baid",
                table: "Card");

            migrationBuilder.AlterColumn<ulong>(
                name: "Baid",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateIndex(
                name: "IX_Card_Baid",
                table: "Card",
                column: "Baid");

            migrationBuilder.AddForeignKey(
                name: "FK_AiScoreData_UserData_Baid",
                table: "AiScoreData",
                column: "Baid",
                principalTable: "UserData",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Card_UserData_Baid",
                table: "Card",
                column: "Baid",
                principalTable: "UserData",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Credential_UserData_Baid",
                table: "Credential",
                column: "Baid",
                principalTable: "UserData",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanScoreData_UserData_Baid",
                table: "DanScoreData",
                column: "Baid",
                principalTable: "UserData",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SongBestData_UserData_Baid",
                table: "SongBestData",
                column: "Baid",
                principalTable: "UserData",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SongPlayData_UserData_Baid",
                table: "SongPlayData",
                column: "Baid",
                principalTable: "UserData",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiScoreData_UserData_Baid",
                table: "AiScoreData");

            migrationBuilder.DropForeignKey(
                name: "FK_Card_UserData_Baid",
                table: "Card");

            migrationBuilder.DropForeignKey(
                name: "FK_Credential_UserData_Baid",
                table: "Credential");

            migrationBuilder.DropForeignKey(
                name: "FK_DanScoreData_UserData_Baid",
                table: "DanScoreData");

            migrationBuilder.DropForeignKey(
                name: "FK_SongBestData_UserData_Baid",
                table: "SongBestData");

            migrationBuilder.DropForeignKey(
                name: "FK_SongPlayData_UserData_Baid",
                table: "SongPlayData");

            migrationBuilder.DropIndex(
                name: "IX_Card_Baid",
                table: "Card");

            migrationBuilder.AlterColumn<ulong>(
                name: "Baid",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Card_Baid",
                table: "Card",
                column: "Baid");

            migrationBuilder.AddForeignKey(
                name: "FK_AiScoreData_Card_Baid",
                table: "AiScoreData",
                column: "Baid",
                principalTable: "Card",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Credential_Card_Baid",
                table: "Credential",
                column: "Baid",
                principalTable: "Card",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanScoreData_Card_Baid",
                table: "DanScoreData",
                column: "Baid",
                principalTable: "Card",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SongBestData_Card_Baid",
                table: "SongBestData",
                column: "Baid",
                principalTable: "Card",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SongPlayData_Card_Baid",
                table: "SongPlayData",
                column: "Baid",
                principalTable: "Card",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserData_Card_Baid",
                table: "UserData",
                column: "Baid",
                principalTable: "Card",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
