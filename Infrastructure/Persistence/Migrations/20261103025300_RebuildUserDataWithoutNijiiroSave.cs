using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// On a fresh database, AddGreenEraSupport moves Nijiiro's save data out of UserData, then AddFriendCode and
    /// AddUserWebProfiles (made before the merge) rebuild UserData from their own model and put those columns back,
    /// NOT NULL, so a new player can't be saved. A column change makes SQLite rebuild the table from this migration's
    /// model, copying only the columns it has, so this drops them where they came back and changes nothing elsewhere.
    /// </summary>
    public partial class RebuildUserDataWithoutNijiiroSave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MyDonName",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
