using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDatabase.Migrations
{
    /// <inheritdoc />
    public partial class CopyPasswordSaltFromCardToCredential : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO Credential (Baid, Password, Salt)
                SELECT Baid, Password, Salt
                FROM Card
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Card
                SET Password = (SELECT Password FROM Credential WHERE Credential.Baid = Card.Baid),
                    Salt = (SELECT Salt FROM Credential WHERE Credential.Baid = Card.Baid)
            ");
        }
    }
}
