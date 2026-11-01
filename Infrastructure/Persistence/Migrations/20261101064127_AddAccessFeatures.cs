using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LockoutAttempts",
                table: "AccessRules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "LockoutMinutes",
                table: "AccessRules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.CreateTable(
                name: "ApiTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedByBaid = table.Column<uint>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Prefix = table.Column<string>(type: "TEXT", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", nullable: false),
                    Scopes = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUsedIp = table.Column<string>(type: "TEXT", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BannedCards",
                columns: table => new
                {
                    AccessCode = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    BannedBy = table.Column<uint>(type: "INTEGER", nullable: true),
                    BannedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Until = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RefusedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedCards", x => x.AccessCode);
                });

            migrationBuilder.CreateTable(
                name: "SignInAttempts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Baid = table.Column<uint>(type: "INTEGER", nullable: true),
                    AccessCode = table.Column<string>(type: "TEXT", nullable: false),
                    Result = table.Column<int>(type: "INTEGER", nullable: false),
                    Ip = table.Column<string>(type: "TEXT", nullable: false),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignInAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ip = table.Column<string>(type: "TEXT", nullable: false),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiTokens_Hash",
                table: "ApiTokens",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignInAttempts_Baid_Time",
                table: "SignInAttempts",
                columns: new[] { "Baid", "Time" });

            migrationBuilder.CreateIndex(
                name: "IX_SignInAttempts_Time",
                table: "SignInAttempts",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_WebSessions_Baid",
                table: "WebSessions",
                column: "Baid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiTokens");

            migrationBuilder.DropTable(
                name: "BannedCards");

            migrationBuilder.DropTable(
                name: "SignInAttempts");

            migrationBuilder.DropTable(
                name: "WebSessions");

            migrationBuilder.DropColumn(
                name: "LockoutAttempts",
                table: "AccessRules");

            migrationBuilder.DropColumn(
                name: "LockoutMinutes",
                table: "AccessRules");
        }
    }
}
