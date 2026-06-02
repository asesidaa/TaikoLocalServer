using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBlueBattleReleaseState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlueBattleReleaseStates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlueBattleReleaseStates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    AssignNextStageId = table.Column<uint>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ReleaseBattleStageId = table.Column<uint>(type: "INTEGER", nullable: true),
                    ReleaseInfoId = table.Column<uint>(type: "INTEGER", nullable: true),
                    ReleaseNpcCostumeId = table.Column<uint>(type: "INTEGER", nullable: true),
                    ReleaseNpcId = table.Column<uint>(type: "INTEGER", nullable: true),
                    ReleaseNpcSpecialId = table.Column<uint>(type: "INTEGER", nullable: true),
                    TokenId = table.Column<uint>(type: "INTEGER", nullable: true),
                    TokenValue = table.Column<uint>(type: "INTEGER", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_BlueBattleReleaseStates_Baid_CreatedAt",
                table: "BlueBattleReleaseStates",
                columns: new[] { "Baid", "CreatedAt" });
        }
    }
}
