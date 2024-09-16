using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeCompetition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChallengeCompeteData",
                columns: table => new
                {
                    CompId = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompeteMode = table.Column<uint>(type: "INTEGER", nullable: false),
                    State = table.Column<uint>(type: "INTEGER", nullable: false),
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    CompeteName = table.Column<string>(type: "TEXT", nullable: false),
                    CompeteDescribe = table.Column<string>(type: "TEXT", nullable: false),
                    MaxParticipant = table.Column<uint>(type: "INTEGER", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpireTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    RequireTitle = table.Column<uint>(type: "INTEGER", nullable: false),
                    OnlyPlayOnce = table.Column<bool>(type: "INTEGER", nullable: false),
                    Share = table.Column<uint>(type: "INTEGER", nullable: false),
                    CompeteTarget = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeCompeteData", x => x.CompId);
                    table.ForeignKey(
                        name: "FK_ChallengeCompeteData_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeCompeteParticipantData",
                columns: table => new
                {
                    CompId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeCompeteParticipantData", x => new { x.CompId, x.Baid });
                    table.ForeignKey(
                        name: "FK_ChallengeCompeteParticipantData_ChallengeCompeteData_CompId",
                        column: x => x.CompId,
                        principalTable: "ChallengeCompeteData",
                        principalColumn: "CompId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeCompeteParticipantData_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeCompeteSongData",
                columns: table => new
                {
                    CompId = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<uint>(type: "INTEGER", nullable: false),
                    Speed = table.Column<uint>(type: "INTEGER", nullable: true),
                    IsVanishOn = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsInverseOn = table.Column<bool>(type: "INTEGER", nullable: true),
                    RandomType = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeCompeteSongData", x => new { x.CompId, x.SongId });
                    table.ForeignKey(
                        name: "FK_ChallengeCompeteSongData_ChallengeCompeteData_CompId",
                        column: x => x.CompId,
                        principalTable: "ChallengeCompeteData",
                        principalColumn: "CompId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeCompeteBestData",
                columns: table => new
                {
                    CompId = table.Column<uint>(type: "INTEGER", nullable: false),
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
                    DrumrollCount = table.Column<uint>(type: "INTEGER", nullable: false),
                    Skipped = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeCompeteBestData", x => new { x.CompId, x.Baid, x.SongId });
                    table.ForeignKey(
                        name: "FK_ChallengeCompeteBestData_ChallengeCompeteSongData_CompId_SongId",
                        columns: x => new { x.CompId, x.SongId },
                        principalTable: "ChallengeCompeteSongData",
                        principalColumns: new[] { "CompId", "SongId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeCompeteBestData_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeCompeteBestData_Baid",
                table: "ChallengeCompeteBestData",
                column: "Baid");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeCompeteBestData_CompId_SongId",
                table: "ChallengeCompeteBestData",
                columns: new[] { "CompId", "SongId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeCompeteData_Baid",
                table: "ChallengeCompeteData",
                column: "Baid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeCompeteParticipantData_Baid",
                table: "ChallengeCompeteParticipantData",
                column: "Baid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChallengeCompeteBestData");

            migrationBuilder.DropTable(
                name: "ChallengeCompeteParticipantData");

            migrationBuilder.DropTable(
                name: "ChallengeCompeteSongData");

            migrationBuilder.DropTable(
                name: "ChallengeCompeteData");
        }
    }
}
