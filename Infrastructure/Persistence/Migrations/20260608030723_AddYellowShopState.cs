using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddYellowShopState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YellowShopItemStates",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SeasonId = table.Column<uint>(type: "INTEGER", nullable: false),
                    ItemType = table.Column<uint>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<uint>(type: "INTEGER", nullable: false),
                    ItemNo = table.Column<uint>(type: "INTEGER", nullable: false),
                    ItemPrice = table.Column<uint>(type: "INTEGER", nullable: false),
                    Status = table.Column<uint>(type: "INTEGER", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YellowShopItemStates", x => new { x.Baid, x.SeasonId, x.ItemType, x.ItemId });
                    table.ForeignKey(
                        name: "FK_YellowShopItemStates_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YellowShopSeasonStates",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    SeasonId = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalGetDonmedal = table.Column<uint>(type: "INTEGER", nullable: false),
                    TotalUseDonmedal = table.Column<uint>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YellowShopSeasonStates", x => new { x.Baid, x.SeasonId });
                    table.ForeignKey(
                        name: "FK_YellowShopSeasonStates_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YellowShopItemStates");

            migrationBuilder.DropTable(
                name: "YellowShopSeasonStates");
        }
    }
}
