using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Migrations
{
    /// <inheritdoc />
    public partial class AddDifficultyPlayedArrayAndDifficultySettingArrayToUserData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DifficultyPlayedArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DifficultySettingArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DifficultyPlayedArray",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultySettingArray",
                table: "UserData");
        }
    }
}
