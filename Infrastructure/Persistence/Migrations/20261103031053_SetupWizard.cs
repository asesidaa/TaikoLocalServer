using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaikoLocalServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SetupWizard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SetupFinishedAt",
                table: "ServerSecrets",
                type: "TEXT",
                nullable: true);

            // An install that has already started once (its secrets row exists) was set up before the wizard.
            migrationBuilder.Sql("UPDATE \"ServerSecrets\" SET \"SetupFinishedAt\" = CURRENT_TIMESTAMP;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SetupFinishedAt",
                table: "ServerSecrets");
        }
    }
}
