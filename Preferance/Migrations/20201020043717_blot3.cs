using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class blot3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveTeam",
                table: "GameB");

            migrationBuilder.AddColumn<bool>(
                name: "ActiveTeamNS",
                table: "GameB",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveTeamNS",
                table: "GameB");

            migrationBuilder.AddColumn<string>(
                name: "ActiveTeam",
                table: "GameB",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
