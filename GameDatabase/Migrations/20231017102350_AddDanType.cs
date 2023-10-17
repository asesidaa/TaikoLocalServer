using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddDanType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanStageScoreData_DanScoreData_Baid_DanId",
                table: "DanStageScoreData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DanStageScoreData",
                table: "DanStageScoreData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DanScoreData",
                table: "DanScoreData");
            
            migrationBuilder.AddColumn<int>(
                name: "DanType",
                table: "DanScoreData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "DanType",
                table: "DanStageScoreData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
            
            /*migrationBuilder.AddPrimaryKey(
                name: "PK_DanScoreData",
                table: "DanScoreData",
                columns: new[] { "Baid", "DanId", "DanType" });*/
            // Add primary key to DanScoreData with raw sql
            migrationBuilder.Sql("CREATE UNIQUE INDEX PK_DanScoreData ON DanScoreData (Baid, DanId, DanType);");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DanStageScoreData",
                table: "DanStageScoreData",
                columns: new[] { "Baid", "DanId", "DanType", "SongNumber" });

            migrationBuilder.AddForeignKey(
                name: "FK_DanStageScoreData_DanScoreData_Baid_DanId_DanType",
                table: "DanStageScoreData",
                columns: new[] { "Baid", "DanId", "DanType" },
                principalTable: "DanScoreData",
                principalColumns: new[] { "Baid", "DanId", "DanType" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanStageScoreData_DanScoreData_Baid_DanId_DanType",
                table: "DanStageScoreData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DanStageScoreData",
                table: "DanStageScoreData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DanScoreData",
                table: "DanScoreData");

            migrationBuilder.DropColumn(
                name: "DanType",
                table: "DanStageScoreData");

            migrationBuilder.DropColumn(
                name: "DanType",
                table: "DanScoreData");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DanStageScoreData",
                table: "DanStageScoreData",
                columns: new[] { "Baid", "DanId", "SongNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DanScoreData",
                table: "DanScoreData",
                columns: new[] { "Baid", "DanId" });

            migrationBuilder.AddForeignKey(
                name: "FK_DanStageScoreData_DanScoreData_Baid_DanId",
                table: "DanStageScoreData",
                columns: new[] { "Baid", "DanId" },
                principalTable: "DanScoreData",
                principalColumns: new[] { "Baid", "DanId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
