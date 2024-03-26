using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDatabase.Migrations
{
    /// <inheritdoc />
    public partial class SplitCurrentCostume : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "CurrentBody",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "CurrentFace",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "CurrentHead",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "CurrentKigurumi",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "CurrentPuchi",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
            // Split CostumeData (json array) into the new fields
            migrationBuilder.Sql(@"
                UPDATE UserData
                SET CurrentKigurumi = json_extract(CostumeData, '$[0]'),
                    CurrentHead = json_extract(CostumeData, '$[1]'),
                    CurrentBody = json_extract(CostumeData, '$[2]'),
                    CurrentFace = json_extract(CostumeData, '$[3]'),
                    CurrentPuchi = json_extract(CostumeData, '$[4]')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentBody",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "CurrentFace",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "CurrentHead",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "CurrentKigurumi",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "CurrentPuchi",
                table: "UserData");
        }
    }
}
