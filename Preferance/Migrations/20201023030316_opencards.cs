using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class opencards : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpenCardsId",
                table: "GameB",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameB_OpenCardsId",
                table: "GameB",
                column: "OpenCardsId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameB_HandB_OpenCardsId",
                table: "GameB",
                column: "OpenCardsId",
                principalTable: "HandB",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameB_HandB_OpenCardsId",
                table: "GameB");

            migrationBuilder.DropIndex(
                name: "IX_GameB_OpenCardsId",
                table: "GameB");

            migrationBuilder.DropColumn(
                name: "OpenCardsId",
                table: "GameB");
        }
    }
}
