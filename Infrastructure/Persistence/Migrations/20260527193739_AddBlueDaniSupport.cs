using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlueDaniSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanScoreDatum_Blue",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    DanId = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsExtra = table.Column<bool>(type: "INTEGER", nullable: false),
                    MedleyUniqueId = table.Column<uint>(type: "INTEGER", nullable: false),
                    ArrivalSongCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    SoulGaugeTotal = table.Column<uint>(type: "INTEGER", nullable: false),
                    ComboCountTotal = table.Column<uint>(type: "INTEGER", nullable: false),
                    ClearGrade = table.Column<uint>(type: "INTEGER", nullable: false, defaultValue: 0u)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanScoreDatum_Blue", x => new { x.Baid, x.DanId, x.IsExtra });
                    table.ForeignKey(
                        name: "FK_DanScoreDatum_Blue_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanStageScoreDatum_Blue",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    DanId = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsExtra = table.Column<bool>(type: "INTEGER", nullable: false),
                    StageIndex = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongNumber = table.Column<uint>(type: "INTEGER", nullable: false),
                    PlayScore = table.Column<uint>(type: "INTEGER", nullable: false),
                    GoodCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    OkCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    BadCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    DrumrollCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalHitCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    ComboCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    HighScore = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanStageScoreDatum_Blue", x => new { x.Baid, x.DanId, x.IsExtra, x.StageIndex });
                    table.ForeignKey(
                        name: "FK_DanStageScoreDatum_Blue_DanScoreDatum_Blue_Baid_DanId_IsExtra",
                        columns: x => new { x.Baid, x.DanId, x.IsExtra },
                        principalTable: "DanScoreDatum_Blue",
                        principalColumns: new[] { "Baid", "DanId", "IsExtra" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanScoreDatum_Blue_MedleyUniqueId",
                table: "DanScoreDatum_Blue",
                column: "MedleyUniqueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanStageScoreDatum_Blue");

            migrationBuilder.DropTable(
                name: "DanScoreDatum_Blue");
        }
    }
}
