using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDatabase.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBaidUniquenessFromCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Card_Baid",
                table: "Card");

            migrationBuilder.AlterColumn<ulong>(
                name: "Baid",
                table: "Credential",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddForeignKey(
                name: "FK_Credential_Card_Baid",
                table: "Credential",
                column: "Baid",
                principalTable: "Card",
                principalColumn: "Baid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Credential_Card_Baid",
                table: "Credential");

            migrationBuilder.AlterColumn<ulong>(
                name: "Baid",
                table: "Credential",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.CreateIndex(
                name: "IX_Card_Baid",
                table: "Card",
                column: "Baid",
                unique: true);
        }
    }
}
