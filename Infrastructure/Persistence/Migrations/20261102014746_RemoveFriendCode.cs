using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFriendCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserWebProfiles_FriendCode",
                table: "UserWebProfiles");

            migrationBuilder.DropColumn(
                name: "FriendCode",
                table: "UserWebProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FriendCode",
                table: "UserWebProfiles",
                type: "TEXT",
                maxLength: 12,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserWebProfiles_FriendCode",
                table: "UserWebProfiles",
                column: "FriendCode",
                unique: true);
        }
    }
}
