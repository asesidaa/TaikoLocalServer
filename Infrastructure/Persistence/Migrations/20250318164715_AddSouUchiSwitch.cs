using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSouUchiSwitch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostumeData",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "CostumeFlgArray",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultyPlayedArray",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultySettingArray",
                table: "UserData");

            migrationBuilder.AddColumn<bool>(
                name: "DisplaySouUchi",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplaySouUchi",
                table: "UserData");

            migrationBuilder.AddColumn<string>(
                name: "CostumeData",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CostumeFlgArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DifficultyPlayedArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DifficultySettingArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
