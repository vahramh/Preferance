using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class blot4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandId",
                table: "CardB");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HandId",
                table: "CardB",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
