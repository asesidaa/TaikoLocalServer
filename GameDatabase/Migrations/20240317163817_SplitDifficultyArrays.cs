﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDatabase.Migrations
{
    /// <inheritdoc />
    public partial class SplitDifficultyArrays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "DifficultyPlayedCourse",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "DifficultyPlayedSort",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "DifficultyPlayedStar",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "DifficultySettingCourse",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "DifficultySettingSort",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "DifficultySettingStar",
                table: "UserData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
            
            // Extract from json arrays
            migrationBuilder.Sql(@"
                UPDATE UserData
                SET DifficultyPlayedCourse = json_extract(DifficultyPlayedArray, '$[0]'),
                    DifficultyPlayedStar = json_extract(DifficultyPlayedArray, '$[1]'),
                    DifficultyPlayedSort = json_extract(DifficultyPlayedArray, '$[2]') ,
                    DifficultySettingCourse = json_extract(DifficultySettingArray, '$[0]'),
                    DifficultySettingStar = json_extract(DifficultySettingArray, '$[1]'),
                    DifficultySettingSort = json_extract(DifficultySettingArray, '$[2]');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DifficultyPlayedCourse",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultyPlayedSort",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultyPlayedStar",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultySettingCourse",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultySettingSort",
                table: "UserData");

            migrationBuilder.DropColumn(
                name: "DifficultySettingStar",
                table: "UserData");
        }
    }
}