using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWhiteRuntimeState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanScoreDatum_White",
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
                    table.PrimaryKey("PK_DanScoreDatum_White", x => new { x.Baid, x.DanId, x.IsExtra });
                    table.ForeignKey(
                        name: "FK_DanScoreDatum_White_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongBestDatum_White",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsShin = table.Column<bool>(type: "INTEGER", nullable: false),
                    BestScore = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestRate = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestCrown = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongBestDatum_White", x => new { x.Baid, x.SongId, x.Difficulty, x.IsShin });
                    table.ForeignKey(
                        name: "FK_SongBestDatum_White_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongPlayDatum_White",
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
                    GoodCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    OkCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    MissCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    ComboCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    HitCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    PoundCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    StarLevel = table.Column<uint>(type: "INTEGER", nullable: false),
                    OptionFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ToneFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PlayMode = table.Column<uint>(type: "INTEGER", nullable: false),
                    StageMode = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsShin = table.Column<bool>(type: "INTEGER", nullable: false),
                    MusicCategory = table.Column<uint>(type: "INTEGER", nullable: false),
                    SelectedFolderId = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsRecent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPapamama = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPushed = table.Column<bool>(type: "INTEGER", nullable: false),
                    SoulGauge = table.Column<uint>(type: "INTEGER", nullable: false),
                    PlayDan = table.Column<uint>(type: "INTEGER", nullable: false),
                    WaiwaiResult = table.Column<uint>(type: "INTEGER", nullable: false),
                    WaiwaiGauge = table.Column<uint>(type: "INTEGER", nullable: false),
                    PlayTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongPlayDatum_White", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongPlayDatum_White_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSaveData_White",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    TitleplateId = table.Column<uint>(type: "INTEGER", nullable: false),
                    ColorBody = table.Column<uint>(type: "INTEGER", nullable: false),
                    ColorFace = table.Column<uint>(type: "INTEGER", nullable: false),
                    ColorLimb = table.Column<uint>(type: "INTEGER", nullable: false),
                    Costume1 = table.Column<uint>(type: "INTEGER", nullable: false),
                    Costume2 = table.Column<uint>(type: "INTEGER", nullable: false),
                    Costume3 = table.Column<uint>(type: "INTEGER", nullable: false),
                    Costume4 = table.Column<uint>(type: "INTEGER", nullable: false),
                    Costume5 = table.Column<uint>(type: "INTEGER", nullable: false),
                    CostumeFlg1 = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CostumeFlg2 = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CostumeFlg3 = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CostumeFlg4 = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CostumeFlg5 = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ToneFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    TitleFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ReleaseSongFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    OptionFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    DefaultOptionSetting = table.Column<byte[]>(type: "BLOB", nullable: false),
                    DefaultShinSetting = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultToneSetting = table.Column<uint>(type: "INTEGER", nullable: false),
                    DispDanType = table.Column<uint>(type: "INTEGER", nullable: false),
                    GotDanMax = table.Column<uint>(type: "INTEGER", nullable: false),
                    GotDanFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    GotDanExtraFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    DispTaikojukuDan = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalGetDonpoint = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalUseDonpoint = table.Column<uint>(type: "INTEGER", nullable: false),
                    RewardPtn = table.Column<uint>(type: "INTEGER", nullable: false),
                    RewardProgress = table.Column<uint>(type: "INTEGER", nullable: false),
                    DifficultyTutorialFlg = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsAutoCostumeOn = table.Column<bool>(type: "INTEGER", nullable: false),
                    CategJpopCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    CategAnimeCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    CategDoyoCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    CategVarietyCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    CategClassicCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    CategGameCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    CategNamcoCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    CategVocaloidCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongPushedCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongFavoriteCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongRecentCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalCreditCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    PrevAreaCode = table.Column<uint>(type: "INTEGER", nullable: false),
                    ConsecAreaCnt = table.Column<uint>(type: "INTEGER", nullable: false),
                    DispLevelTotal = table.Column<uint>(type: "INTEGER", nullable: false),
                    DispLevelChassis = table.Column<uint>(type: "INTEGER", nullable: false),
                    DispLevelSelf = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsDevil = table.Column<bool>(type: "INTEGER", nullable: false),
                    DispScoreType = table.Column<uint>(type: "INTEGER", nullable: false),
                    DifficultyPlayedCourse = table.Column<uint>(type: "INTEGER", nullable: false),
                    DifficultyPlayedStar = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsTojiru = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsExplain = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastPlayDatetime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSaveData_White", x => x.Baid);
                    table.ForeignKey(
                        name: "FK_UserSaveData_White_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WhiteFavoriteSongs",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongNo = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteFavoriteSongs", x => new { x.Baid, x.SongNo });
                    table.ForeignKey(
                        name: "FK_WhiteFavoriteSongs_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WhiteRecentSongs",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongNo = table.Column<uint>(type: "INTEGER", nullable: false),
                    LastPlayed = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteRecentSongs", x => new { x.Baid, x.SongNo });
                    table.ForeignKey(
                        name: "FK_WhiteRecentSongs_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanStageScoreDatum_White",
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
                    table.PrimaryKey("PK_DanStageScoreDatum_White", x => new { x.Baid, x.DanId, x.IsExtra, x.StageIndex });
                    table.ForeignKey(
                        name: "FK_DanStageScoreDatum_White_DanScoreDatum_White_Baid_DanId_IsExtra",
                        columns: x => new { x.Baid, x.DanId, x.IsExtra },
                        principalTable: "DanScoreDatum_White",
                        principalColumns: new[] { "Baid", "DanId", "IsExtra" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanScoreDatum_White_MedleyUniqueId",
                table: "DanScoreDatum_White",
                column: "MedleyUniqueId");

            migrationBuilder.CreateIndex(
                name: "IX_SongBestDatum_White_SongId_Difficulty_BestScore",
                table: "SongBestDatum_White",
                columns: new[] { "SongId", "Difficulty", "BestScore" });

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayDatum_White_Baid_SongId_Difficulty_PlayTime",
                table: "SongPlayDatum_White",
                columns: new[] { "Baid", "SongId", "Difficulty", "PlayTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanStageScoreDatum_White");

            migrationBuilder.DropTable(
                name: "SongBestDatum_White");

            migrationBuilder.DropTable(
                name: "SongPlayDatum_White");

            migrationBuilder.DropTable(
                name: "UserSaveData_White");

            migrationBuilder.DropTable(
                name: "WhiteFavoriteSongs");

            migrationBuilder.DropTable(
                name: "WhiteRecentSongs");

            migrationBuilder.DropTable(
                name: "DanScoreDatum_White");
        }
    }
}
