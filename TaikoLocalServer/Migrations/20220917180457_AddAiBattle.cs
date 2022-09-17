using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAiBattle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiScoreData",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsWin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiScoreData", x => new { x.Baid, x.SongId, x.Difficulty });
                    table.ForeignKey(
                        name: "FK_AiScoreData_Card_Baid",
                        column: x => x.Baid,
                        principalTable: "Card",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiSectionScoreData",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<uint>(type: "INTEGER", nullable: false),
                    SectionIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    Crown = table.Column<int>(type: "INTEGER", nullable: false),
                    IsWin = table.Column<bool>(type: "INTEGER", nullable: false),
                    Score = table.Column<uint>(type: "INTEGER", nullable: false),
                    GoodCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    OkCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    MissCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    DrumrollCount = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSectionScoreData", x => new { x.Baid, x.SongId, x.Difficulty, x.SectionIndex });
                    table.ForeignKey(
                        name: "FK_AiSectionScoreData_AiScoreData_Baid_SongId_Difficulty",
                        columns: x => new { x.Baid, x.SongId, x.Difficulty },
                        principalTable: "AiScoreData",
                        principalColumns: new[] { "Baid", "SongId", "Difficulty" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiSectionScoreData");

            migrationBuilder.DropTable(
                name: "AiScoreData");
        }
    }
}
