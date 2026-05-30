using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlueBattleState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlueBattleNpcStates",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    NpcId = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalExp = table.Column<uint>(type: "INTEGER", nullable: true),
                    MaxDaniPower = table.Column<uint>(type: "INTEGER", nullable: true),
                    NpcCostumeFlg = table.Column<byte[]>(type: "BLOB", nullable: true),
                    SelectedSpecialId = table.Column<uint>(type: "INTEGER", nullable: true),
                    ReleaseSpecialFlg = table.Column<byte[]>(type: "BLOB", nullable: true),
                    BondsLevel = table.Column<uint>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlueBattleNpcStates", x => new { x.Baid, x.NpcId });
                    table.ForeignKey(
                        name: "FK_BlueBattleNpcStates_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlueBattleReleaseStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    ReleaseInfoId = table.Column<uint>(type: "INTEGER", nullable: true),
                    ReleaseBattleStageId = table.Column<uint>(type: "INTEGER", nullable: true),
                    ReleaseNpcId = table.Column<uint>(type: "INTEGER", nullable: true),
                    ReleaseNpcCostumeId = table.Column<uint>(type: "INTEGER", nullable: true),
                    ReleaseNpcSpecialId = table.Column<uint>(type: "INTEGER", nullable: true),
                    AssignNextStageId = table.Column<uint>(type: "INTEGER", nullable: true),
                    TokenId = table.Column<uint>(type: "INTEGER", nullable: true),
                    TokenValue = table.Column<uint>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlueBattleReleaseStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlueBattleReleaseStates_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlueBattleStageResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    PlayDatetime = table.Column<DateTime>(type: "datetime", nullable: true),
                    PlayMode = table.Column<uint>(type: "INTEGER", nullable: true),
                    StageMode = table.Column<uint>(type: "INTEGER", nullable: true),
                    StageIndex = table.Column<uint>(type: "INTEGER", nullable: true),
                    SongNo = table.Column<uint>(type: "INTEGER", nullable: true),
                    Level = table.Column<uint>(type: "INTEGER", nullable: true),
                    BattleStageId = table.Column<uint>(type: "INTEGER", nullable: true),
                    NpcId = table.Column<uint>(type: "INTEGER", nullable: true),
                    ResultType = table.Column<uint>(type: "INTEGER", nullable: true),
                    ClearFlag = table.Column<uint>(type: "INTEGER", nullable: true),
                    BossLife = table.Column<uint>(type: "INTEGER", nullable: true),
                    TotalExp = table.Column<uint>(type: "INTEGER", nullable: true),
                    AcquiredExp = table.Column<uint>(type: "INTEGER", nullable: true),
                    DaniPower = table.Column<uint>(type: "INTEGER", nullable: true),
                    TokenId = table.Column<uint>(type: "INTEGER", nullable: true),
                    TokenValue = table.Column<uint>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlueBattleStageResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlueBattleStageResults_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlueBattleTokenStates",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    TokenId = table.Column<uint>(type: "INTEGER", nullable: false),
                    TokenValue = table.Column<uint>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlueBattleTokenStates", x => new { x.Baid, x.TokenId });
                    table.ForeignKey(
                        name: "FK_BlueBattleTokenStates_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlueBattleUserStates",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    ReleaseInfoFlg = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ReleaseBattleStageFlg = table.Column<byte[]>(type: "BLOB", nullable: true),
                    LastBattleStageId = table.Column<uint>(type: "INTEGER", nullable: true),
                    LastBossLife = table.Column<uint>(type: "INTEGER", nullable: true),
                    LastNpcId = table.Column<uint>(type: "INTEGER", nullable: true),
                    AssignStageId = table.Column<uint>(type: "INTEGER", nullable: true),
                    BattleBondsLvCap = table.Column<uint>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlueBattleUserStates", x => x.Baid);
                    table.ForeignKey(
                        name: "FK_BlueBattleUserStates_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlueBattleReleaseStates_Baid_CreatedAt",
                table: "BlueBattleReleaseStates",
                columns: new[] { "Baid", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BlueBattleStageResults_Baid_PlayDatetime",
                table: "BlueBattleStageResults",
                columns: new[] { "Baid", "PlayDatetime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlueBattleNpcStates");

            migrationBuilder.DropTable(
                name: "BlueBattleReleaseStates");

            migrationBuilder.DropTable(
                name: "BlueBattleStageResults");

            migrationBuilder.DropTable(
                name: "BlueBattleTokenStates");

            migrationBuilder.DropTable(
                name: "BlueBattleUserStates");
        }
    }
}
