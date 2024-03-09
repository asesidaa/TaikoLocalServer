using System.Text.Json;
using GameDatabase.Context;
using GameDatabase.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDatabase.Migrations
{
    internal record Pair(uint Baid, string TokenCountDict);
    /// <inheritdoc />
    public partial class SeparateTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using var context = new TaikoDbContext();
            var tokenJsons = context.Database
                .SqlQuery<Pair>($"SELECT Baid, TokenCountDict FROM UserData")
                .ToList();

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => new { x.Baid, x.Id });
                    table.ForeignKey(
                        name: "FK_Tokens_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });
            foreach (var (baid, tokenCountDict) in tokenJsons)
            {
                var tokenDict = JsonSerializer.Deserialize<Dictionary<int, int>>(tokenCountDict);
                foreach (var (key, value) in tokenDict)
                {
                    migrationBuilder.InsertData(
                        table: "Tokens",
                        columns: new[] { "Baid", "Id", "Count" },
                        values: new object[] { baid, key, value });
                }
            }
            migrationBuilder.DropColumn(
                name: "TokenCountDict",
                table: "UserData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.AddColumn<string>(
                name: "TokenCountDict",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
