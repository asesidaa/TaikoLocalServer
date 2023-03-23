using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Migrations
{
    /// <inheritdoc />
    public partial class AddSongBestDatumDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "BestComboCount",
                table: "SongBestData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "BestDrumrollCount",
                table: "SongBestData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "BestGoodCount",
                table: "SongBestData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "BestHitCount",
                table: "SongBestData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "BestMissCount",
                table: "SongBestData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "BestOkCount",
                table: "SongBestData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BestComboCount",
                table: "SongBestData");

            migrationBuilder.DropColumn(
                name: "BestDrumrollCount",
                table: "SongBestData");

            migrationBuilder.DropColumn(
                name: "BestGoodCount",
                table: "SongBestData");

            migrationBuilder.DropColumn(
                name: "BestHitCount",
                table: "SongBestData");

            migrationBuilder.DropColumn(
                name: "BestMissCount",
                table: "SongBestData");

            migrationBuilder.DropColumn(
                name: "BestOkCount",
                table: "SongBestData");
        }
    }
}
