using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlayerQuality3D : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quality3D",
                table: "ServerOptions");

            migrationBuilder.AddColumn<int>(
                name: "Quality3D",
                table: "UserWebProfiles",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quality3D",
                table: "UserWebProfiles");

            migrationBuilder.AddColumn<int>(
                name: "Quality3D",
                table: "ServerOptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
