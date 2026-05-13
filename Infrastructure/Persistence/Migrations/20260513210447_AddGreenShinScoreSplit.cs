using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGreenShinScoreSplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShin",
                table: "SongPlayDatum_Green",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShin",
                table: "SongBestDatum_Green",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.DropPrimaryKey(
                name: "PK_SongBestDatum_Green",
                table: "SongBestDatum_Green");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongBestDatum_Green",
                table: "SongBestDatum_Green",
                columns: new[] { "Baid", "SongId", "Difficulty", "IsShin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SongBestDatum_Green",
                table: "SongBestDatum_Green");

            migrationBuilder.DropColumn(
                name: "IsShin",
                table: "SongPlayDatum_Green");

            migrationBuilder.DropColumn(
                name: "IsShin",
                table: "SongBestDatum_Green");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongBestDatum_Green",
                table: "SongBestDatum_Green",
                columns: new[] { "Baid", "SongId", "Difficulty" });
        }
    }
}
