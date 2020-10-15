using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class misere : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MiserShared",
                table: "Game");

            migrationBuilder.AddColumn<string>(
                name: "MisereOffered",
                table: "Game",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MiserePossible",
                table: "Game",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MisereSharable",
                table: "Game",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MisereShared",
                table: "Game",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MisereOffered",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "MiserePossible",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "MisereSharable",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "MisereShared",
                table: "Game");

            migrationBuilder.AddColumn<bool>(
                name: "MiserShared",
                table: "Game",
                type: "bit",
                nullable: true);
        }
    }
}
