using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDatabase.Migrations
{
    /// <inheritdoc />
    public partial class SplitCostumeUnlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnlockedBody",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedFace",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedHead",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedKigurumi",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedPuchi",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
            
            // Split the costumeflgarray into separate fields
            migrationBuilder.Sql(@"
                UPDATE UserData
                SET UnlockedKigurumi = coalesce(json_extract(CostumeFlgArray, '$[0]'), '[]'),
                    UnlockedHead = coalesce(json_extract(CostumeFlgArray, '$[1]'), '[]'),
                    UnlockedBody = coalesce(json_extract(CostumeFlgArray, '$[2]'), '[]'),
                    UnlockedFace = coalesce(json_extract(CostumeFlgArray, '$[3]'), '[]'),
                    UnlockedPuchi = coalesce(json_extract(CostumeFlgArray, '$[4]'), '[]')");
            // Deduplicate values
            migrationBuilder.Sql(@"
                UPDATE UserData
                    SET UnlockedKigurumi = (
                        SELECT json_group_array(DISTINCT value)
                        FROM (
                            SELECT value
                            FROM UserData AS ud
                            CROSS JOIN json_each(ud.UnlockedKigurumi)
                            WHERE ud.Baid = UserData.Baid
                        )
                    )");
            migrationBuilder.Sql(@"
                UPDATE UserData
                    SET UnlockedHead = (
                        SELECT json_group_array(DISTINCT value)
                        FROM (
                            SELECT value
                            FROM UserData AS ud
                            CROSS JOIN json_each(ud.UnlockedHead)
                            WHERE ud.Baid = UserData.Baid
                        )
                    )");
            migrationBuilder.Sql(@"
                UPDATE UserData
                    SET UnlockedBody = (
                        SELECT json_group_array(DISTINCT value)
                        FROM (
                            SELECT value
                            FROM UserData AS ud
                            CROSS JOIN json_each(ud.UnlockedBody)
                            WHERE ud.Baid = UserData.Baid
                        )
                    )");
            migrationBuilder.Sql(@"
                UPDATE UserData
                    SET UnlockedFace = (
                        SELECT json_group_array(DISTINCT value)
                        FROM (
                            SELECT value
                            FROM UserData AS ud
                            CROSS JOIN json_each(ud.UnlockedFace)
                            WHERE ud.Baid = UserData.Baid
                        )
                    )");
            migrationBuilder.Sql(@"
                UPDATE UserData
                    SET UnlockedPuchi = (
                        SELECT json_group_array(DISTINCT value)
                        FROM (
                            SELECT value
                            FROM UserData AS ud
                            CROSS JOIN json_each(ud.UnlockedPuchi)
                            WHERE ud.Baid = UserData.Baid
                        )
                    )");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnlockedBody",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "UnlockedFace",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "UnlockedHead",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "UnlockedKigurumi",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "UnlockedPuchi",
                table: "UserData");
        }
    }
}
