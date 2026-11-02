using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBluePlayResultSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ReleaseSongFlg",
                table: "UserSaveData_Blue",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "BlueFavoriteSongs",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongNo = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlueFavoriteSongs", x => new { x.Baid, x.SongNo });
                    table.ForeignKey(
                        name: "FK_BlueFavoriteSongs_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlueRecentSongs",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongNo = table.Column<uint>(type: "INTEGER", nullable: false),
                    LastPlayed = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlueRecentSongs", x => new { x.Baid, x.SongNo });
                    table.ForeignKey(
                        name: "FK_BlueRecentSongs_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongBestDatum_Blue",
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
                    table.PrimaryKey("PK_SongBestDatum_Blue", x => new { x.Baid, x.SongId, x.Difficulty, x.IsShin });
                    table.ForeignKey(
                        name: "FK_SongBestDatum_Blue_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SongPlayDatum_Blue",
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
                    table.PrimaryKey("PK_SongPlayDatum_Blue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SongPlayDatum_Blue_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SongBestDatum_Blue_SongId_Difficulty_BestScore",
                table: "SongBestDatum_Blue",
                columns: new[] { "SongId", "Difficulty", "BestScore" });

            migrationBuilder.CreateIndex(
                name: "IX_SongPlayDatum_Blue_Baid_SongId_Difficulty_PlayTime",
                table: "SongPlayDatum_Blue",
                columns: new[] { "Baid", "SongId", "Difficulty", "PlayTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlueFavoriteSongs");

            migrationBuilder.DropTable(
                name: "BlueRecentSongs");

            migrationBuilder.DropTable(
                name: "SongBestDatum_Blue");

            migrationBuilder.DropTable(
                name: "SongPlayDatum_Blue");

            migrationBuilder.DropColumn(
                name: "ReleaseSongFlg",
                table: "UserSaveData_Blue");
        }
    }
}
