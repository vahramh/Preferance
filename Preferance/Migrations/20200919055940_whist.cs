using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class whist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Player1Whisting",
                table: "Game",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Player2Whisting",
                table: "Game",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Player3Whisting",
                table: "Game",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Player4Whisting",
                table: "Game",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Player1Whisting",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "Player2Whisting",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "Player3Whisting",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "Player4Whisting",
                table: "Game");
        }
    }
}
