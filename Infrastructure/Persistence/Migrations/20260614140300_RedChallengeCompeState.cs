using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RedChallengeCompeState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RedChallengeCompeProgress",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    BundleId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TaskId = table.Column<uint>(type: "INTEGER", nullable: false),
                    TrackNo = table.Column<uint>(type: "INTEGER", nullable: false),
                    Slot = table.Column<uint>(type: "INTEGER", nullable: false),
                    CompeId = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongNo = table.Column<uint>(type: "INTEGER", nullable: false),
                    Level = table.Column<uint>(type: "INTEGER", nullable: false),
                    OptionFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    StageMode = table.Column<uint>(type: "INTEGER", nullable: false),
                    HighScore = table.Column<uint>(type: "INTEGER", nullable: false),
                    ProgressValue = table.Column<uint>(type: "INTEGER", nullable: false),
                    Completed = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedChallengeCompeProgress", x => new { x.Baid, x.BundleId, x.TaskId, x.TrackNo });
                    table.ForeignKey(
                        name: "FK_RedChallengeCompeProgress_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RedChallengeCompeRawFacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    BundleId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TaskId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Slot = table.Column<uint>(type: "INTEGER", nullable: false),
                    CompeId = table.Column<uint>(type: "INTEGER", nullable: false),
                    TrackNo = table.Column<uint>(type: "INTEGER", nullable: false),
                    SongNo = table.Column<uint>(type: "INTEGER", nullable: false),
                    Level = table.Column<uint>(type: "INTEGER", nullable: false),
                    OptionFlg = table.Column<byte[]>(type: "BLOB", nullable: false),
                    StageMode = table.Column<uint>(type: "INTEGER", nullable: false),
                    HighScore = table.Column<uint>(type: "INTEGER", nullable: false),
                    PlayResult = table.Column<uint>(type: "INTEGER", nullable: false),
                    ProgressValue = table.Column<uint>(type: "INTEGER", nullable: false),
                    Completed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlayTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedChallengeCompeRawFacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedChallengeCompeRawFacts_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RedChallengeCompeProgress_BundleId_TaskId_TrackNo",
                table: "RedChallengeCompeProgress",
                columns: new[] { "BundleId", "TaskId", "TrackNo" });

            migrationBuilder.CreateIndex(
                name: "IX_RedChallengeCompeRawFacts_Baid_BundleId_TaskId_TrackNo_PlayTime",
                table: "RedChallengeCompeRawFacts",
                columns: new[] { "Baid", "BundleId", "TaskId", "TrackNo", "PlayTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RedChallengeCompeProgress");

            migrationBuilder.DropTable(
                name: "RedChallengeCompeRawFacts");
        }
    }
}
