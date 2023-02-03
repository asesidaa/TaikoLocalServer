using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Migrations
{
    /// <inheritdoc />
    public partial class AddUnlockedSongIdListToUserData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnlockedSongIdList",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnlockedSongIdList",
                table: "UserData");
        }
    }
}
