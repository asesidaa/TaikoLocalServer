using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlueBattleNpcSelectedSpecials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SelectedSpecialId",
                table: "BlueBattleNpcStates",
                newName: "SelectedSpecialId1");

            migrationBuilder.AddColumn<uint>(
                name: "NpcCostumeId",
                table: "BlueBattleNpcStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "SelectedSpecialId2",
                table: "BlueBattleNpcStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "SelectedSpecialId3",
                table: "BlueBattleNpcStates",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NpcCostumeId",
                table: "BlueBattleNpcStates");

            migrationBuilder.DropColumn(
                name: "SelectedSpecialId2",
                table: "BlueBattleNpcStates");

            migrationBuilder.DropColumn(
                name: "SelectedSpecialId3",
                table: "BlueBattleNpcStates");

            migrationBuilder.RenameColumn(
                name: "SelectedSpecialId1",
                table: "BlueBattleNpcStates",
                newName: "SelectedSpecialId");
        }
    }
}
