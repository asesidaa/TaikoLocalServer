using GameDatabase.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDatabase.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedArrays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //We messed up a few migrations before so this is aiming to fix the usual issues you can encounter when migrating from 08.18 and CHN.

            //Removing the following tables as they have been split a while ago and are not longer being used.
            migrationBuilder.Sql(@"ALTER TABLE UserData DROP COLUMN CostumeData");
            migrationBuilder.Sql(@"ALTER TABLE UserData DROP COLUMN CostumeFlgArray");
            migrationBuilder.Sql(@"ALTER TABLE UserData DROP COLUMN DifficultyPlayedArray");
            migrationBuilder.Sql(@"ALTER TABLE UserData DROP COLUMN DifficultySettingArray");

            //The default value of the previous migration was incorrectly set, causing a crash on load.
            migrationBuilder.Sql(@"UPDATE UserData SET UnlockedUraSongIdList = '[]' WHERE UnlockedUraSongIdList = ''");
            migrationBuilder.Sql(@"UPDATE UserData SET UnlockedSongIdList = '[]' WHERE UnlockedSongIdList = ''");

            //These arrays should not be empty
            migrationBuilder.Sql(@"UPDATE UserData SET UnlockedPuchi = '[0]' WHERE UnlockedPuchi = '[]'");
            migrationBuilder.Sql(@"UPDATE UserData SET UnlockedHead = '[0]' WHERE UnlockedHead = '[]'");
            migrationBuilder.Sql(@"UPDATE UserData SET UnlockedFace = '[0]' WHERE UnlockedFace = '[]'");
            migrationBuilder.Sql(@"UPDATE UserData SET UnlockedBody = '[0]' WHERE UnlockedBody = '[]'");
            migrationBuilder.Sql(@"UPDATE UserData SET ToneFlgArray = '[0]' WHERE ToneFlgArray = '[]'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CostumeData",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "CostumeFlgArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "DifficultyPlayedArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[0,0,0]");

            migrationBuilder.AddColumn<string>(
                name: "DifficultySettingArray",
                table: "UserData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[0,0,0]");
        }
    }
}
