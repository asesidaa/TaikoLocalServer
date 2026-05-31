using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameBlueBattleNpcMaxDpn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxDaniPower",
                table: "BlueBattleNpcStates",
                newName: "MaxDpn");

            migrationBuilder.RenameColumn(
                name: "DaniPower",
                table: "BlueBattleStageResults",
                newName: "Dpn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxDpn",
                table: "BlueBattleNpcStates",
                newName: "MaxDaniPower");

            migrationBuilder.RenameColumn(
                name: "Dpn",
                table: "BlueBattleStageResults",
                newName: "DaniPower");
        }
    }
}
