using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGreenEraSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "AiSectionScoreData",
                newName: "AiSectionScoreDatum_Nijiiro");

            migrationBuilder.RenameTable(
                name: "DanStageScoreData",
                newName: "DanStageScoreDatum_Nijiiro");

            migrationBuilder.RenameTable(
                name: "SongBestData",
                newName: "SongBestDatum_Nijiiro");

            migrationBuilder.RenameTable(
                name: "SongPlayData",
                newName: "SongPlayDatum_Nijiiro");

            migrationBuilder.RenameTable(
                name: "AiScoreData",
                newName: "AiScoreDatum_Nijiiro");

            migrationBuilder.RenameTable(
                name: "DanScoreData",
                newName: "DanScoreDatum_Nijiiro");

            migrationBuilder.CreateTable(
                name: "UserSaveData_Nijiiro",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    TitlePlateId = table.Column<uint>(type: "INTEGER", nullable: false),
                    FavoriteSongsArray = table.Column<string>(type: "TEXT", nullable: false),
                    ToneFlgArray = table.Column<string>(type: "TEXT", nullable: false),
                    TitleFlgArray = table.Column<string>(type: "TEXT", nullable: false),
                    UnlockedKigurumi = table.Column<string>(type: "TEXT", nullable: false),
                    UnlockedHead = table.Column<string>(type: "TEXT", nullable: false),
                    UnlockedBody = table.Column<string>(type: "TEXT", nullable: false),
                    UnlockedFace = table.Column<string>(type: "TEXT", nullable: false),
                    UnlockedPuchi = table.Column<string>(type: "TEXT", nullable: false),
                    GenericInfoFlgArray = table.Column<string>(type: "TEXT", nullable: false),
                    OptionSetting = table.Column<short>(type: "INTEGER", nullable: false),
                    NotesPosition = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVoiceOn = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSkipOn = table.Column<bool>(type: "INTEGER", nullable: false),
                    DifficultyPlayedCourse = table.Column<uint>(type: "INTEGER", nullable: false),
                    DifficultyPlayedStar = table.Column<uint>(type: "INTEGER", nullable: false),
                    DifficultyPlayedSort = table.Column<uint>(type: "INTEGER", nullable: false),
                    DifficultySettingCourse = table.Column<uint>(type: "INTEGER", nullable: false),
                    DifficultySettingStar = table.Column<uint>(type: "INTEGER", nullable: false),
                    DifficultySettingSort = table.Column<uint>(type: "INTEGER", nullable: false),
                    SelectedToneId = table.Column<uint>(type: "INTEGER", nullable: false),
                    LastPlayDatetime = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastPlayMode = table.Column<uint>(type: "INTEGER", nullable: false),
                    ColorBody = table.Column<uint>(type: "INTEGER", nullable: false),
                    ColorFace = table.Column<uint>(type: "INTEGER", nullable: false),
                    ColorLimb = table.Column<uint>(type: "INTEGER", nullable: false),
                    CurrentKigurumi = table.Column<uint>(type: "INTEGER", nullable: false),
                    CurrentHead = table.Column<uint>(type: "INTEGER", nullable: false),
                    CurrentBody = table.Column<uint>(type: "INTEGER", nullable: false),
                    CurrentFace = table.Column<uint>(type: "INTEGER", nullable: false),
                    CurrentPuchi = table.Column<uint>(type: "INTEGER", nullable: false),
                    DisplayDan = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayAchievement = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplaySouUchi = table.Column<bool>(type: "INTEGER", nullable: false),
                    AchievementDisplayDifficulty = table.Column<uint>(type: "INTEGER", nullable: false),
                    AiWinCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UnlockedSongIdList = table.Column<string>(type: "TEXT", nullable: false),
                    UnlockedUraSongIdList = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSaveData_Nijiiro", x => x.Baid);
                    table.ForeignKey(
                        name: "FK_UserSaveData_Nijiiro_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ""UserSaveData_Nijiiro"" (
                    ""Baid"",
                    ""Title"",
                    ""TitlePlateId"",
                    ""FavoriteSongsArray"",
                    ""ToneFlgArray"",
                    ""TitleFlgArray"",
                    ""UnlockedKigurumi"",
                    ""UnlockedHead"",
                    ""UnlockedBody"",
                    ""UnlockedFace"",
                    ""UnlockedPuchi"",
                    ""GenericInfoFlgArray"",
                    ""OptionSetting"",
                    ""NotesPosition"",
                    ""IsVoiceOn"",
                    ""IsSkipOn"",
                    ""DifficultyPlayedCourse"",
                    ""DifficultyPlayedStar"",
                    ""DifficultyPlayedSort"",
                    ""DifficultySettingCourse"",
                    ""DifficultySettingStar"",
                    ""DifficultySettingSort"",
                    ""SelectedToneId"",
                    ""LastPlayDatetime"",
                    ""LastPlayMode"",
                    ""ColorBody"",
                    ""ColorFace"",
                    ""ColorLimb"",
                    ""CurrentKigurumi"",
                    ""CurrentHead"",
                    ""CurrentBody"",
                    ""CurrentFace"",
                    ""CurrentPuchi"",
                    ""DisplayDan"",
                    ""DisplayAchievement"",
                    ""DisplaySouUchi"",
                    ""AchievementDisplayDifficulty"",
                    ""AiWinCount"",
                    ""UnlockedSongIdList"",
                    ""UnlockedUraSongIdList""
                )
                SELECT
                    ""Baid"",
                    ""Title"",
                    ""TitlePlateId"",
                    ""FavoriteSongsArray"",
                    ""ToneFlgArray"",
                    ""TitleFlgArray"",
                    ""UnlockedKigurumi"",
                    ""UnlockedHead"",
                    ""UnlockedBody"",
                    ""UnlockedFace"",
                    ""UnlockedPuchi"",
                    ""GenericInfoFlgArray"",
                    ""OptionSetting"",
                    ""NotesPosition"",
                    ""IsVoiceOn"",
                    ""IsSkipOn"",
                    ""DifficultyPlayedCourse"",
                    ""DifficultyPlayedStar"",
                    ""DifficultyPlayedSort"",
                    ""DifficultySettingCourse"",
                    ""DifficultySettingStar"",
                    ""DifficultySettingSort"",
                    ""SelectedToneId"",
                    ""LastPlayDatetime"",
                    ""LastPlayMode"",
                    ""ColorBody"",
                    ""ColorFace"",
                    ""ColorLimb"",
                    ""CurrentKigurumi"",
                    ""CurrentHead"",
                    ""CurrentBody"",
                    ""CurrentFace"",
                    ""CurrentPuchi"",
                    ""DisplayDan"",
                    ""DisplayAchievement"",
                    ""DisplaySouUchi"",
                    ""AchievementDisplayDifficulty"",
                    ""AiWinCount"",
                    ""UnlockedSongIdList"",
                    ""UnlockedUraSongIdList""
                FROM ""UserData"";
            ");

            migrationBuilder.DropColumn(
                name: "AchievementDisplayDifficulty",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "AiWinCount",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "ColorBody",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "ColorFace",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "ColorLimb",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "CurrentBody",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "CurrentFace",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "CurrentHead",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "CurrentKigurumi",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "CurrentPuchi",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultyPlayedCourse",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultyPlayedSort",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultyPlayedStar",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultySettingCourse",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultySettingSort",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultySettingStar",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DisplayAchievement",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DisplayDan",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DisplaySouUchi",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "FavoriteSongsArray",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "GenericInfoFlgArray",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "IsSkipOn",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "IsVoiceOn",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "LastPlayDatetime",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "LastPlayMode",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "NotesPosition",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "OptionSetting",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "SelectedToneId",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "TitleFlgArray",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "TitlePlateId",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "ToneFlgArray",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "UnlockedBody",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "UnlockedFace",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "UnlockedHead",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "UnlockedKigurumi",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "UnlockedPuchi",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "UnlockedSongIdList",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "UnlockedUraSongIdList",
                table: "UserData");

            migrationBuilder.CreateTable(
                name: "GreenFavoriteSongs",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongNo = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GreenFavoriteSongs", x => new { x.Baid, x.SongNo });
                    table.ForeignKey(
                        name: "FK_GreenFavoriteSongs_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GreenFriends",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    FriendBaid = table.Column<uint>(type: "INTEGER", nullable: false),
                    FriendName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GreenFriends", x => new { x.Baid, x.FriendBaid });
                    table.ForeignKey(
                        name: "FK_GreenFriends_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GreenGhostTokens",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    TokenId = table.Column<uint>(type: "INTEGER", nullable: false),
                    TokenValue = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GreenGhostTokens", x => new { x.Baid, x.TokenId });
                    table.ForeignKey(
                        name: "FK_GreenGhostTokens_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GreenGhostWinnings",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    LevelId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Winnings = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GreenGhostWinnings", x => new { x.Baid, x.LevelId });
                    table.ForeignKey(
                        name: "FK_GreenGhostWinnings_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GreenRecentSongs",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongNo = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GreenRecentSongs", x => new { x.Baid, x.SongNo });
                    table.ForeignKey(
                        name: "FK_GreenRecentSongs_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongBestDatum_Green",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestScore = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestRate = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestCrown = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongBestDatum_Green", x => new { x.Baid, x.SongId, x.Difficulty });
                    table.ForeignKey(
                        name: "FK_SongBestDatum_Green_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongPlayDatum_Green",
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
                    SupportLevel = table.Column<uint>(type: "INTEGER", nullable: false),
                    OptionFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ToneFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PlayMode = table.Column<uint>(type: "INTEGER", nullable: false),
                    StageMode = table.Column<uint>(type: "INTEGER", nullable: false),
                    MusicCategory = table.Column<uint>(type: "INTEGER", nullable: false),
                    SelectedFolderId = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsRecent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPapamama = table.Column<bool>(type: "INTEGER", nullable: false),
                    SoulGauge = table.Column<uint>(type: "INTEGER", nullable: false),
                    PlayDan = table.Column<uint>(type: "INTEGER", nullable: false),
                    WaiwaiResult = table.Column<uint>(type: "INTEGER", nullable: false),
                    WaiwaiGauge = table.Column<uint>(type: "INTEGER", nullable: false),
                    PlayTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongPlayDatum_Green", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongPlayDatum_Green_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSaveData_Green",
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
                    OptionFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    DefaultOptionSetting = table.Column<byte[]>(type: "BLOB", nullable: false),
                    DefaultShinSetting = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultToneSetting = table.Column<uint>(type: "INTEGER", nullable: false),
                    DispDanType = table.Column<uint>(type: "INTEGER", nullable: false),
                    GotDanMax = table.Column<uint>(type: "INTEGER", nullable: false),
                    GotDanFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    GotDanExtraFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    DispTaikojukuDan = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalGetDonmedal = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalUseDonmedal = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalGetKatsumedal = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalUseKatsumedal = table.Column<uint>(type: "INTEGER", nullable: false),
                    ItemshopTutorialFlg = table.Column<uint>(type: "INTEGER", nullable: false),
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
                    WaiwaiTutorialFlg = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsChallengeCompe = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTojiru = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsExplain = table.Column<bool>(type: "INTEGER", nullable: false),
                    GhostInputMedian = table.Column<int>(type: "INTEGER", nullable: false),
                    GhostInputVariance = table.Column<uint>(type: "INTEGER", nullable: false),
                    GhostRankId = table.Column<uint>(type: "INTEGER", nullable: false),
                    GhostWinPoint = table.Column<uint>(type: "INTEGER", nullable: false),
                    GhostCertifiedLevelId = table.Column<uint>(type: "INTEGER", nullable: false),
                    GhostTotalWinnings = table.Column<uint>(type: "INTEGER", nullable: false),
                    GhostReleaseInfoFlag = table.Column<byte[]>(type: "BLOB", nullable: false),
                    GhostPlayedSongFlag = table.Column<byte[]>(type: "BLOB", nullable: false),
                    LastPlayDatetime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSaveData_Green", x => x.Baid);
                    table.ForeignKey(
                        name: "FK_UserSaveData_Green_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GhostStageSectionDatum_Green",
                columns: table => new
                {
                    PlayId = table.Column<long>(type: "INTEGER", nullable: false),
                    SectionNo = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsWin = table.Column<bool>(type: "INTEGER", nullable: false),
                    GoodCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    OkCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    NgCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    PoundCount = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GhostStageSectionDatum_Green", x => new { x.PlayId, x.SectionNo });
                    table.ForeignKey(
                        name: "FK_GhostStageSectionDatum_Green_SongPlayDatum_Green_PlayId",
                        column: x => x.PlayId,
                        principalTable: "SongPlayDatum_Green",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayDatum_Green_Baid",
                table: "SongPlayDatum_Green",
                column: "Baid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiSectionScoreDatum_Nijiiro");

            migrationBuilder.DropTable(
                name: "DanStageScoreDatum_Nijiiro");

            migrationBuilder.DropTable(
                name: "GhostStageSectionDatum_Green");

            migrationBuilder.DropTable(
                name: "GreenFavoriteSongs");

            migrationBuilder.DropTable(
                name: "GreenFriends");

            migrationBuilder.DropTable(
                name: "GreenGhostTokens");

            migrationBuilder.DropTable(
                name: "GreenGhostWinnings");

            migrationBuilder.DropTable(
                name: "GreenRecentSongs");

            migrationBuilder.DropTable(
                name: "SongBestDatum_Green");

            migrationBuilder.DropTable(
                name: "SongBestDatum_Nijiiro");

            migrationBuilder.DropTable(
                name: "SongPlayDatum_Nijiiro");

            migrationBuilder.DropTable(
                name: "UserSaveData_Green");

            migrationBuilder.DropTable(
                name: "UserSaveData_Nijiiro");

            migrationBuilder.DropTable(
                name: "AiScoreDatum_Nijiiro");

            migrationBuilder.DropTable(
                name: "DanScoreDatum_Nijiiro");

            migrationBuilder.DropTable(
                name: "SongPlayDatum_Green");

            migrationBuilder.AddColumn<uint>(
                name: "AchievementDisplayDifficulty",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<int>(
                name: "AiWinCount",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<uint>(
                name: "ColorBody",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "ColorFace",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "ColorLimb",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "CurrentBody",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "CurrentFace",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "CurrentHead",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "CurrentKigurumi",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "CurrentPuchi",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "DifficultyPlayedCourse",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "DifficultyPlayedSort",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "DifficultyPlayedStar",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "DifficultySettingCourse",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "DifficultySettingSort",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "DifficultySettingStar",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<bool>(
                name: "DisplayAchievement",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DisplayDan",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DisplaySouUchi",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FavoriteSongsArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GenericInfoFlgArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSkipOn",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVoiceOn",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPlayDatetime",
                table: "UserData",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<uint>(
                name: "LastPlayMode",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<int>(
                name: "NotesPosition",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "OptionSetting",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<uint>(
                name: "SelectedToneId",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleFlgArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<uint>(
                name: "TitlePlateId",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<string>(
                name: "ToneFlgArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedBody",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedFace",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedHead",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedKigurumi",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedPuchi",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedSongIdList",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedUraSongIdList",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

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
                        name: "FK_AiScoreData_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanScoreData",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    DanId = table.Column<uint>(type: "INTEGER", nullable: false),
                    DanType = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    ArrivalSongCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    ClearState = table.Column<uint>(type: "INTEGER", nullable: false, defaultValue: 0u),
                    ComboCountTotal = table.Column<uint>(type: "INTEGER", nullable: false),
                    SoulGaugeTotal = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanScoreData", x => new { x.Baid, x.DanId, x.DanType });
                    table.ForeignKey(
                        name: "FK_DanScoreData_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongBestData",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestCrown = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestRate = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestScore = table.Column<uint>(type: "INTEGER", nullable: false),
                    BestScoreRank = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongBestData", x => new { x.Baid, x.SongId, x.Difficulty });
                    table.ForeignKey(
                        name: "FK_SongBestData_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
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
                    ComboCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    Crown = table.Column<uint>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<uint>(type: "INTEGER", nullable: false),
                    DrumrollCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    GoodCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    HitCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    MissCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    OkCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    OptionSetting = table.Column<uint>(type: "INTEGER", nullable: false),
                    PlayTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    Score = table.Column<uint>(type: "INTEGER", nullable: false),
                    ScoreRank = table.Column<uint>(type: "INTEGER", nullable: false),
                    ScoreRate = table.Column<uint>(type: "INTEGER", nullable: false),
                    Skipped = table.Column<bool>(type: "INTEGER", nullable: false),
                    SongId = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongNumber = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongPlayData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongPlayData_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
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
                    DrumrollCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    GoodCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsWin = table.Column<bool>(type: "INTEGER", nullable: false),
                    MissCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    OkCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    Score = table.Column<uint>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "DanStageScoreData",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    DanId = table.Column<uint>(type: "INTEGER", nullable: false),
                    DanType = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    SongNumber = table.Column<uint>(type: "INTEGER", nullable: false),
                    BadCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    ComboCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    DrumrollCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    GoodCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    HighScore = table.Column<uint>(type: "INTEGER", nullable: false),
                    OkCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    PlayScore = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalHitCount = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanStageScoreData", x => new { x.Baid, x.DanId, x.DanType, x.SongNumber });
                    table.ForeignKey(
                        name: "FK_DanStageScoreData_DanScoreData_Baid_DanId_DanType",
                        columns: x => new { x.Baid, x.DanId, x.DanType },
                        principalTable: "DanScoreData",
                        principalColumns: new[] { "Baid", "DanId", "DanType" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayData_Baid",
                table: "SongPlayData",
                column: "Baid");
        }
    }
}
