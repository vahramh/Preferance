using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class highCard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HighCardId",
                table: "Game",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Game_HighCardId",
                table: "Game",
                column: "HighCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Player_HighCardId",
                table: "Game",
                column: "HighCardId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Player_HighCardId",
                table: "Game");

            migrationBuilder.DropIndex(
                name: "IX_Game_HighCardId",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "HighCardId",
                table: "Game");
        }
    }
}
