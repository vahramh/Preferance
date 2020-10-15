using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class matchdata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Player12CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player12Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player13CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player13Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player14CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player14Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player1CurrentDump",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Player1CurrentPool",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Player1CurrentScore",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player1Dump",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Player1Pool",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player21CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player21Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player23CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player23Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player24CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player24Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player2CurrentDump",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Player2CurrentPool",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Player2CurrentScore",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player2Dump",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Player2Pool",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player31CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player31Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player32CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player32Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player34CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player34Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player3CurrentDump",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Player3CurrentPool",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Player3CurrentScore",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player3Dump",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Player3Pool",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player41CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player41Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player42CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player42Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player43CurrentWhist",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player43Whist",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Player4CurrentDump",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Player4CurrentPool",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Player4CurrentScore",
                table: "Match",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Player4Dump",
                table: "Match",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Player4Pool",
                table: "Match",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Player12CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player12Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player13CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player13Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player14CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player14Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player1CurrentDump",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player1CurrentPool",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player1CurrentScore",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player1Dump",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player1Pool",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player21CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player21Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player23CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player23Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player24CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player24Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player2CurrentDump",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player2CurrentPool",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player2CurrentScore",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player2Dump",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player2Pool",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player31CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player31Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player32CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player32Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player34CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player34Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player3CurrentDump",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player3CurrentPool",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player3CurrentScore",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player3Dump",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player3Pool",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player41CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player41Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player42CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player42Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player43CurrentWhist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player43Whist",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player4CurrentDump",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player4CurrentPool",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player4CurrentScore",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player4Dump",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Player4Pool",
                table: "Match");
        }
    }
}
