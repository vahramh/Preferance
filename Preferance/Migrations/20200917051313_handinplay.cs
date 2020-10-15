using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class handinplay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HandInPlayId",
                table: "Game",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Game_HandInPlayId",
                table: "Game",
                column: "HandInPlayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Hand_HandInPlayId",
                table: "Game",
                column: "HandInPlayId",
                principalTable: "Hand",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Hand_HandInPlayId",
                table: "Game");

            migrationBuilder.DropIndex(
                name: "IX_Game_HandInPlayId",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "HandInPlayId",
                table: "Game");
        }
    }
}
