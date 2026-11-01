using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FriendCode",
                table: "UserData",
                type: "TEXT",
                maxLength: 12,
                nullable: false,
                defaultValue: "");

            // Backfill: 12 random digits per existing user (random() runs per row), then re-roll any duplicates
            // so the unique index can be created. New users get theirs from FriendCode.New() at sign-up.
            const string newCode = "substr('000000000000' || abs(random() % 1000000000000), -12, 12)";
            migrationBuilder.Sql($"UPDATE UserData SET FriendCode = {newCode};");
            for (var pass = 0; pass < 3; pass++)
            {
                migrationBuilder.Sql($"UPDATE UserData SET FriendCode = {newCode} WHERE FriendCode IN " +
                                     "(SELECT FriendCode FROM UserData GROUP BY FriendCode HAVING COUNT(*) > 1);");
            }

            migrationBuilder.CreateIndex(
                name: "IX_UserData_FriendCode",
                table: "UserData",
                column: "FriendCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserData_FriendCode",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "FriendCode",
                table: "UserData");
        }
    }
}
