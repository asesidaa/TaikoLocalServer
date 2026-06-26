using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMomoiroPlayResultState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanScoreDatum_Momoiro",
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
                    table.PrimaryKey("PK_DanScoreDatum_Momoiro", x => new { x.Baid, x.DanId, x.IsExtra });
                    table.ForeignKey(
                        name: "FK_DanScoreDatum_Momoiro_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongPlayDatum_Momoiro",
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
                    table.PrimaryKey("PK_SongPlayDatum_Momoiro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongPlayDatum_Momoiro_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanStageScoreDatum_Momoiro",
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
                    table.PrimaryKey("PK_DanStageScoreDatum_Momoiro", x => new { x.Baid, x.DanId, x.IsExtra, x.StageIndex });
                    table.ForeignKey(
                        name: "FK_DanStageScoreDatum_Momoiro_DanScoreDatum_Momoiro_Baid_DanId_IsExtra",
                        columns: x => new { x.Baid, x.DanId, x.IsExtra },
                        principalTable: "DanScoreDatum_Momoiro",
                        principalColumns: new[] { "Baid", "DanId", "IsExtra" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanScoreDatum_Momoiro_MedleyUniqueId",
                table: "DanScoreDatum_Momoiro",
                column: "MedleyUniqueId");

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayDatum_Momoiro_Baid_SongId_Difficulty_PlayTime",
                table: "SongPlayDatum_Momoiro",
                columns: new[] { "Baid", "SongId", "Difficulty", "PlayTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanStageScoreDatum_Momoiro");

            migrationBuilder.DropTable(
                name: "SongPlayDatum_Momoiro");

            migrationBuilder.DropTable(
                name: "DanScoreDatum_Momoiro");
        }
    }
}
