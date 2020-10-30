using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class blot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatchBId",
                table: "Player",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HandB",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandB", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardB",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Colour = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    Seniority = table.Column<int>(nullable: false),
                    SeniorityTrump = table.Column<int>(nullable: false),
                    HandId = table.Column<string>(nullable: true),
                    Sequence = table.Column<int>(nullable: false),
                    HandBId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardB_HandB_HandBId",
                        column: x => x.HandBId,
                        principalTable: "HandB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MatchB",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    NorthId = table.Column<string>(nullable: true),
                    SouthId = table.Column<string>(nullable: true),
                    EastId = table.Column<string>(nullable: true),
                    WestId = table.Column<string>(nullable: true),
                    MatchDate = table.Column<DateTime>(nullable: false),
                    LastHandId = table.Column<string>(nullable: true),
                    NorthSouthScore = table.Column<int>(nullable: false),
                    EastWestScore = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchB_Player_EastId",
                        column: x => x.EastId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchB_HandB_LastHandId",
                        column: x => x.LastHandId,
                        principalTable: "HandB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchB_Player_NorthId",
                        column: x => x.NorthId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchB_Player_SouthId",
                        column: x => x.SouthId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchB_Player_WestId",
                        column: x => x.WestId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameB",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DealerId = table.Column<string>(nullable: true),
                    ActiveTeam = table.Column<string>(nullable: true),
                    NextPlayerId = table.Column<string>(nullable: true),
                    NorthId = table.Column<string>(nullable: true),
                    SouthId = table.Column<string>(nullable: true),
                    EastId = table.Column<string>(nullable: true),
                    WestId = table.Column<string>(nullable: true),
                    HighCardPlayerId = table.Column<string>(nullable: true),
                    HighCardId = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Value = table.Column<int>(nullable: false),
                    Challenge = table.Column<bool>(nullable: false),
                    Contra = table.Column<bool>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    NorthHandId = table.Column<string>(nullable: true),
                    SouthHandId = table.Column<string>(nullable: true),
                    EastHandId = table.Column<string>(nullable: true),
                    WestHandId = table.Column<string>(nullable: true),
                    HandInPlayId = table.Column<string>(nullable: true),
                    NorthSouthHandResultId = table.Column<string>(nullable: true),
                    EastWestHandResultId = table.Column<string>(nullable: true),
                    NorthSouthPoints = table.Column<int>(nullable: false),
                    EastWestPoints = table.Column<int>(nullable: false),
                    MatchId = table.Column<string>(nullable: true),
                    MatchBId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameB_Player_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_HandB_EastHandId",
                        column: x => x.EastHandId,
                        principalTable: "HandB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_Player_EastId",
                        column: x => x.EastId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_HandB_EastWestHandResultId",
                        column: x => x.EastWestHandResultId,
                        principalTable: "HandB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_HandB_HandInPlayId",
                        column: x => x.HandInPlayId,
                        principalTable: "HandB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_CardB_HighCardId",
                        column: x => x.HighCardId,
                        principalTable: "CardB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_Player_HighCardPlayerId",
                        column: x => x.HighCardPlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_MatchB_MatchBId",
                        column: x => x.MatchBId,
                        principalTable: "MatchB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_Player_NextPlayerId",
                        column: x => x.NextPlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_HandB_NorthHandId",
                        column: x => x.NorthHandId,
                        principalTable: "HandB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_Player_NorthId",
                        column: x => x.NorthId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_HandB_NorthSouthHandResultId",
                        column: x => x.NorthSouthHandResultId,
                        principalTable: "HandB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_HandB_SouthHandId",
                        column: x => x.SouthHandId,
                        principalTable: "HandB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_Player_SouthId",
                        column: x => x.SouthId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_HandB_WestHandId",
                        column: x => x.WestHandId,
                        principalTable: "HandB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameB_Player_WestId",
                        column: x => x.WestId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Player_MatchBId",
                table: "Player",
                column: "MatchBId");

            migrationBuilder.CreateIndex(
                name: "IX_CardB_HandBId",
                table: "CardB",
                column: "HandBId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_DealerId",
                table: "GameB",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_EastHandId",
                table: "GameB",
                column: "EastHandId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_EastId",
                table: "GameB",
                column: "EastId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_EastWestHandResultId",
                table: "GameB",
                column: "EastWestHandResultId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_HandInPlayId",
                table: "GameB",
                column: "HandInPlayId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_HighCardId",
                table: "GameB",
                column: "HighCardId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_HighCardPlayerId",
                table: "GameB",
                column: "HighCardPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_MatchBId",
                table: "GameB",
                column: "MatchBId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_NextPlayerId",
                table: "GameB",
                column: "NextPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_NorthHandId",
                table: "GameB",
                column: "NorthHandId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_NorthId",
                table: "GameB",
                column: "NorthId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_NorthSouthHandResultId",
                table: "GameB",
                column: "NorthSouthHandResultId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_SouthHandId",
                table: "GameB",
                column: "SouthHandId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_SouthId",
                table: "GameB",
                column: "SouthId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_WestHandId",
                table: "GameB",
                column: "WestHandId");

            migrationBuilder.CreateIndex(
                name: "IX_GameB_WestId",
                table: "GameB",
                column: "WestId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchB_EastId",
                table: "MatchB",
                column: "EastId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchB_LastHandId",
                table: "MatchB",
                column: "LastHandId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchB_NorthId",
                table: "MatchB",
                column: "NorthId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchB_SouthId",
                table: "MatchB",
                column: "SouthId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchB_WestId",
                table: "MatchB",
                column: "WestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Player_MatchB_MatchBId",
                table: "Player",
                column: "MatchBId",
                principalTable: "MatchB",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Player_MatchB_MatchBId",
                table: "Player");

            migrationBuilder.DropTable(
                name: "GameB");

            migrationBuilder.DropTable(
                name: "CardB");

            migrationBuilder.DropTable(
                name: "MatchB");

            migrationBuilder.DropTable(
                name: "HandB");

            migrationBuilder.DropIndex(
                name: "IX_Player_MatchBId",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MatchBId",
                table: "Player");
        }
    }
}
