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
                SET CurrentKigurumi = COALESCE(json_extract(CostumeData, '$[0]'), 0),
                    CurrentHead = COALESCE(json_extract(CostumeData, '$[1]'), 0),
                    CurrentBody = COALESCE(json_extract(CostumeData, '$[2]'), 0),
                    CurrentFace = COALESCE(json_extract(CostumeData, '$[3]'), 0),
                    CurrentPuchi = COALESCE(json_extract(CostumeData, '$[4]'), 0);
            ");
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
