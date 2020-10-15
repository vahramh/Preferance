using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Preferance.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SetOfHands",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetOfHands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hand",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    SetOfHandsId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hand_SetOfHands_SetOfHandsId",
                        column: x => x.SetOfHandsId,
                        principalTable: "SetOfHands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Card",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Colour = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    HandId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Card", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Card_Hand_HandId",
                        column: x => x.HandId,
                        principalTable: "Hand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Game",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DealerId = table.Column<string>(nullable: true),
                    ActivePlayerId = table.Column<string>(nullable: true),
                    NextPlayerId = table.Column<string>(nullable: true),
                    Player1Id = table.Column<string>(nullable: true),
                    Player2Id = table.Column<string>(nullable: true),
                    Player3Id = table.Column<string>(nullable: true),
                    Player4Id = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Value = table.Column<int>(nullable: false),
                    Player1Bidding = table.Column<bool>(nullable: false),
                    Player2Bidding = table.Column<bool>(nullable: false),
                    Player3Bidding = table.Column<bool>(nullable: false),
                    Player4Bidding = table.Column<bool>(nullable: false),
                    MiserShared = table.Column<bool>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Player1HandId = table.Column<string>(nullable: true),
                    Player2HandId = table.Column<string>(nullable: true),
                    Player3HandId = table.Column<string>(nullable: true),
                    Player4HandId = table.Column<string>(nullable: true),
                    TalonId = table.Column<string>(nullable: true),
                    DiscardedId = table.Column<string>(nullable: true),
                    Player1HandResultId = table.Column<string>(nullable: true),
                    Player2HandResultId = table.Column<string>(nullable: true),
                    Player3HandResultId = table.Column<string>(nullable: true),
                    Player4HandResultId = table.Column<string>(nullable: true),
                    MatchId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Game_Hand_DiscardedId",
                        column: x => x.DiscardedId,
                        principalTable: "Hand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Hand_Player1HandId",
                        column: x => x.Player1HandId,
                        principalTable: "Hand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_SetOfHands_Player1HandResultId",
                        column: x => x.Player1HandResultId,
                        principalTable: "SetOfHands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Hand_Player2HandId",
                        column: x => x.Player2HandId,
                        principalTable: "Hand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_SetOfHands_Player2HandResultId",
                        column: x => x.Player2HandResultId,
                        principalTable: "SetOfHands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Hand_Player3HandId",
                        column: x => x.Player3HandId,
                        principalTable: "Hand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_SetOfHands_Player3HandResultId",
                        column: x => x.Player3HandResultId,
                        principalTable: "SetOfHands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Hand_Player4HandId",
                        column: x => x.Player4HandId,
                        principalTable: "Hand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_SetOfHands_Player4HandResultId",
                        column: x => x.Player4HandResultId,
                        principalTable: "SetOfHands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Game_Hand_TalonId",
                        column: x => x.TalonId,
                        principalTable: "Hand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    MatchId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Match",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Player1Id = table.Column<string>(nullable: true),
                    Player2Id = table.Column<string>(nullable: true),
                    Player3Id = table.Column<string>(nullable: true),
                    Player4Id = table.Column<string>(nullable: true),
                    MatchDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Match", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Match_Player_Player1Id",
                        column: x => x.Player1Id,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Match_Player_Player2Id",
                        column: x => x.Player2Id,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Match_Player_Player3Id",
                        column: x => x.Player3Id,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Match_Player_Player4Id",
                        column: x => x.Player4Id,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Card_HandId",
                table: "Card",
                column: "HandId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_ActivePlayerId",
                table: "Game",
                column: "ActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_DealerId",
                table: "Game",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_DiscardedId",
                table: "Game",
                column: "DiscardedId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_MatchId",
                table: "Game",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_NextPlayerId",
                table: "Game",
                column: "NextPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player1HandId",
                table: "Game",
                column: "Player1HandId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player1HandResultId",
                table: "Game",
                column: "Player1HandResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player1Id",
                table: "Game",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player2HandId",
                table: "Game",
                column: "Player2HandId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player2HandResultId",
                table: "Game",
                column: "Player2HandResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player2Id",
                table: "Game",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player3HandId",
                table: "Game",
                column: "Player3HandId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player3HandResultId",
                table: "Game",
                column: "Player3HandResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player3Id",
                table: "Game",
                column: "Player3Id");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player4HandId",
                table: "Game",
                column: "Player4HandId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player4HandResultId",
                table: "Game",
                column: "Player4HandResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Game_Player4Id",
                table: "Game",
                column: "Player4Id");

            migrationBuilder.CreateIndex(
                name: "IX_Game_TalonId",
                table: "Game",
                column: "TalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Hand_SetOfHandsId",
                table: "Hand",
                column: "SetOfHandsId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_Player1Id",
                table: "Match",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Match_Player2Id",
                table: "Match",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Match_Player3Id",
                table: "Match",
                column: "Player3Id");

            migrationBuilder.CreateIndex(
                name: "IX_Match_Player4Id",
                table: "Match",
                column: "Player4Id");

            migrationBuilder.CreateIndex(
                name: "IX_Player_MatchId",
                table: "Player",
                column: "MatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Player_ActivePlayerId",
                table: "Game",
                column: "ActivePlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Player_DealerId",
                table: "Game",
                column: "DealerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Player_NextPlayerId",
                table: "Game",
                column: "NextPlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Player_Player1Id",
                table: "Game",
                column: "Player1Id",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Player_Player2Id",
                table: "Game",
                column: "Player2Id",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Player_Player3Id",
                table: "Game",
                column: "Player3Id",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Player_Player4Id",
                table: "Game",
                column: "Player4Id",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Match_MatchId",
                table: "Game",
                column: "MatchId",
                principalTable: "Match",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Player_Match_MatchId",
                table: "Player",
                column: "MatchId",
                principalTable: "Match",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Match_Player_Player1Id",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_Match_Player_Player2Id",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_Match_Player_Player3Id",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_Match_Player_Player4Id",
                table: "Match");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Card");

            migrationBuilder.DropTable(
                name: "Game");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Hand");

            migrationBuilder.DropTable(
                name: "SetOfHands");

            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "Match");
        }
    }
}
