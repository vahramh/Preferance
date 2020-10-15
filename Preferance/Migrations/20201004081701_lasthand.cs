using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class lasthand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastHandId",
                table: "Match",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Match_LastHandId",
                table: "Match",
                column: "LastHandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Hand_LastHandId",
                table: "Match",
                column: "LastHandId",
                principalTable: "Hand",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Match_Hand_LastHandId",
                table: "Match");

            migrationBuilder.DropIndex(
                name: "IX_Match_LastHandId",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "LastHandId",
                table: "Match");
        }
    }
}
