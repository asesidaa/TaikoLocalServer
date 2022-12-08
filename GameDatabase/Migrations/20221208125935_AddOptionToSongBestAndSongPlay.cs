using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionToSongBestAndSongPlay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Option",
                table: "SongPlayData",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Option",
                table: "SongBestData",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Option",
                table: "SongPlayData");

            migrationBuilder.DropColumn(
                name: "Option",
                table: "SongBestData");
        }
    }
}
