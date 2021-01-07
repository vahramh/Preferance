using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class BlotScore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EastWestExtras",
                table: "GameB",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EastWestScore",
                table: "GameB",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NorthSouthExtras",
                table: "GameB",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NorthSouthScore",
                table: "GameB",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EastWestExtras",
                table: "GameB");

            migrationBuilder.DropColumn(
                name: "EastWestScore",
                table: "GameB");

            migrationBuilder.DropColumn(
                name: "NorthSouthExtras",
                table: "GameB");

            migrationBuilder.DropColumn(
                name: "NorthSouthScore",
                table: "GameB");
        }
    }
}
