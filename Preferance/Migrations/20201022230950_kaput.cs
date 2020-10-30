using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class kaput : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Kaput",
                table: "GameB",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kaput",
                table: "GameB");
        }
    }
}
