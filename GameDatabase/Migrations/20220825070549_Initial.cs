using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Card",
                columns: table => new
                {
                    AccessCode = table.Column<string>(type: "TEXT", nullable: false),
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Card", x => x.AccessCode);
                    table.UniqueConstraint("AK_Card_Baid", x => x.Baid);
                });

            migrationBuilder.CreateTable(
                name: "SongBestData",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestScore = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestRate = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestCrown = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestScoreRank = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongBestData", x => new { x.Baid, x.SongId, x.Difficulty });
                    table.ForeignKey(
                        name: "FK_SongBestData_Card_Baid",
                        column: x => x.Baid,
                        principalTable: "Card",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongPlayData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<uint>(type: "INTEGER", nullable: false),
                    Crown = table.Column<uint>(type: "INTEGER", nullable: false),
                    Score = table.Column<uint>(type: "INTEGER", nullable: false),
                    ScoreRate = table.Column<uint>(type: "INTEGER", nullable: false),
                    ScoreRank = table.Column<uint>(type: "INTEGER", nullable: false),
                    GoodCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    OkCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    MissCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    ComboCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    HitCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    Skipped = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlayTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongPlayData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongPlayData_Card_Baid",
                        column: x => x.Baid,
                        principalTable: "Card",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserData",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    MyDonName = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    TitlePlateId = table.Column<uint>(type: "INTEGER", nullable: false),
                    FavoriteSongsArray = table.Column<string>(type: "TEXT", nullable: false),
                    OptionSetting = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVoiceOn = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSkipOn = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastPlayDatetime = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastPlayMode = table.Column<long>(type: "INTEGER", nullable: false),
                    ColorBody = table.Column<uint>(type: "INTEGER", nullable: false),
                    ColorFace = table.Column<uint>(type: "INTEGER", nullable: false),
                    ColorLimb = table.Column<uint>(type: "INTEGER", nullable: false),
                    CostumeData = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayDan = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayAchievement = table.Column<bool>(type: "INTEGER", nullable: false),
                    AchievementDisplayDifficulty = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserData", x => x.Baid);
                    table.ForeignKey(
                        name: "FK_UserData_Card_Baid",
                        column: x => x.Baid,
                        principalTable: "Card",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Card_Baid",
                table: "Card",
                column: "Baid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayData_Baid",
                table: "SongPlayData",
                column: "Baid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SongBestData");

            migrationBuilder.DropTable(
                name: "SongPlayData");

            migrationBuilder.DropTable(
                name: "UserData");

            migrationBuilder.DropTable(
                name: "Card");
        }
    }
}
