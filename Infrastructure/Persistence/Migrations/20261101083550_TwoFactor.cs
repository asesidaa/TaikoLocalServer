using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TwoFactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecoveryCodes",
                table: "Credential",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "TotpLastStep",
                table: "Credential",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "TotpSecret",
                table: "Credential",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TwoFactor",
                table: "AccessRules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecoveryCodes",
                table: "Credential");

            migrationBuilder.DropColumn(
                name: "TotpLastStep",
                table: "Credential");

            migrationBuilder.DropColumn(
                name: "TotpSecret",
                table: "Credential");

            migrationBuilder.DropColumn(
                name: "TwoFactor",
                table: "AccessRules");
        }
    }
}
