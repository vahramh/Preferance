using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class blot2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchId",
                table: "GameB");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatchId",
                table: "GameB",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
