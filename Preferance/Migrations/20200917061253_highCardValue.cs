using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class highCardValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Player_HighCardId",
                table: "Game");

            migrationBuilder.AddColumn<string>(
                name: "HighCardPlayerId",
                table: "Game",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Game_HighCardPlayerId",
                table: "Game",
                column: "HighCardPlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Card_HighCardId",
                table: "Game",
                column: "HighCardId",
                principalTable: "Card",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Player_HighCardPlayerId",
                table: "Game",
                column: "HighCardPlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Card_HighCardId",
                table: "Game");

            migrationBuilder.DropForeignKey(
                name: "FK_Game_Player_HighCardPlayerId",
                table: "Game");

            migrationBuilder.DropIndex(
                name: "IX_Game_HighCardPlayerId",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "HighCardPlayerId",
                table: "Game");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Player_HighCardId",
                table: "Game",
                column: "HighCardId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
