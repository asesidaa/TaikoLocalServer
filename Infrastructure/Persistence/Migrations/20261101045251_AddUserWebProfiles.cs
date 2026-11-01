using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserWebProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserWebProfiles",
                columns: table => new
                {
                    Baid = table.Column<uint>(type: "INTEGER", nullable: false),
                    FriendCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    CardColor = table.Column<uint>(type: "INTEGER", nullable: false),
                    CardPattern = table.Column<int>(type: "INTEGER", nullable: false),
                    CardFade = table.Column<int>(type: "INTEGER", nullable: false),
                    CardAnimation = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWebProfiles", x => x.Baid);
                    table.ForeignKey(
                        name: "FK_UserWebProfiles_UserData_Baid",
                        column: x => x.Baid,
                        principalTable: "UserData",
                        principalColumn: "Baid",
                        onDelete: ReferentialAction.Cascade);
                });

            // Every user gets a profile: their existing friend code (unique since AddFriendCode) and the card
            // defaults (Don red, Dots, no fade, Clear). Copied before UserData.FriendCode is dropped.
            migrationBuilder.Sql(
                "INSERT INTO UserWebProfiles (Baid, FriendCode, CardColor, CardPattern, CardFade, CardAnimation) " +
                "SELECT Baid, FriendCode, 0, 0, -1, 0 FROM UserData;");

            migrationBuilder.CreateIndex(
                name: "IX_UserWebProfiles_FriendCode",
                table: "UserWebProfiles",
                column: "FriendCode",
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_UserData_FriendCode",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "FriendCode",
                table: "UserData");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FriendCode",
                table: "UserData",
                type: "TEXT",
                maxLength: 12,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE UserData SET FriendCode = " +
                "(SELECT FriendCode FROM UserWebProfiles WHERE UserWebProfiles.Baid = UserData.Baid) " +
                "WHERE Baid IN (SELECT Baid FROM UserWebProfiles);");

            migrationBuilder.CreateIndex(
                name: "IX_UserData_FriendCode",
                table: "UserData",
                column: "FriendCode",
                unique: true);

            migrationBuilder.DropTable(
                name: "UserWebProfiles");
        }
    }
}
