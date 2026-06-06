using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlueTokkunState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "TokkunTutorialFlg",
                table: "UserSaveData_Blue",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BlueTokkunStageResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    PlayDatetime = table.Column<string>(type: "TEXT", nullable: false),
                    PlayMode = table.Column<uint>(type: "INTEGER", nullable: false),
                    BanacoinDatetime = table.Column<string>(type: "TEXT", nullable: false),
                    TokkunSongCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    TookunSongnoesJson = table.Column<string>(type: "TEXT", nullable: false),
                    TokkunSpeedchangeCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    TokkunAutoplayCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    TokkunJumpCnt = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlueTokkunStageResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlueTokkunStageResults_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlueTokkunStageResults_Baid_PlayDatetime",
                table: "BlueTokkunStageResults",
                columns: new[] { "Baid", "PlayDatetime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlueTokkunStageResults");

            migrationBuilder.DropColumn(
                name: "TokkunTutorialFlg",
                table: "UserSaveData_Blue");
        }
    }
}
