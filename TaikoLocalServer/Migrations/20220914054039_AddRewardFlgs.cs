using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardFlgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CostumeFlgArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[[],[],[],[],[]]");

            migrationBuilder.AddColumn<string>(
                name: "TitleFlgArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "ToneFlgArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostumeFlgArray",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "TitleFlgArray",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "ToneFlgArray",
                table: "UserData");
        }
    }
}
