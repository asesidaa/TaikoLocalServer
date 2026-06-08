using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddYellowTokkunState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YellowTokkunStageResults",
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
                    table.PrimaryKey("PK_YellowTokkunStageResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YellowTokkunStageResults_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YellowTokkunStageResults_Baid_PlayDatetime",
                table: "YellowTokkunStageResults",
                columns: new[] { "Baid", "PlayDatetime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YellowTokkunStageResults");
        }
    }
}
