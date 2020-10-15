﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Preferance.Data;

namespace Preferance.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20200917051313_handinplay")]
    partial class handinplay
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Preferance.Models.Card", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Colour")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HandId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Seniority")
                        .HasColumnType("int");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("HandId");

                    b.ToTable("Card");
                });

            modelBuilder.Entity("Preferance.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ActivePlayerId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DealerId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DiscardedId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("HandInPlayId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("MatchId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("MisereOffered")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("MiserePossible")
                        .HasColumnType("bit");

                    b.Property<bool>("MisereSharable")
                        .HasColumnType("bit");

                    b.Property<bool>("MisereShared")
                        .HasColumnType("bit");

                    b.Property<string>("NextPlayerId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Player1Bidding")
                        .HasColumnType("bit");

                    b.Property<string>("Player1HandId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Player1HandResultId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Player1Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Player2Bidding")
                        .HasColumnType("bit");

                    b.Property<string>("Player2HandId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Player2HandResultId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Player2Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Player3Bidding")
                        .HasColumnType("bit");

                    b.Property<string>("Player3HandId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Player3HandResultId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Player3Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("Player4Bidding")
                        .HasColumnType("bit");

                    b.Property<string>("Player4HandId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Player4HandResultId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Player4Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TalonId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ActivePlayerId");

                    b.HasIndex("DealerId");

                    b.HasIndex("DiscardedId");

                    b.HasIndex("HandInPlayId");

                    b.HasIndex("MatchId");

                    b.HasIndex("NextPlayerId");

                    b.HasIndex("Player1HandId");

                    b.HasIndex("Player1HandResultId");

                    b.HasIndex("Player1Id");

                    b.HasIndex("Player2HandId");

                    b.HasIndex("Player2HandResultId");

                    b.HasIndex("Player2Id");

                    b.HasIndex("Player3HandId");

                    b.HasIndex("Player3HandResultId");

                    b.HasIndex("Player3Id");

                    b.HasIndex("Player4HandId");

                    b.HasIndex("Player4HandResultId");

                    b.HasIndex("Player4Id");

                    b.HasIndex("TalonId");

                    b.ToTable("Game");
                });

            modelBuilder.Entity("Preferance.Models.Hand", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("SetOfHandsId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("SetOfHandsId");

                    b.ToTable("Hand");
                });

            modelBuilder.Entity("Preferance.Models.Match", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("MatchDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Player1Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Player2Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Player3Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Player4Id")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Player1Id");

                    b.HasIndex("Player2Id");

                    b.HasIndex("Player3Id");

                    b.HasIndex("Player4Id");

                    b.ToTable("Match");
                });

            modelBuilder.Entity("Preferance.Models.Player", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Image")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MatchId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("MatchId");

                    b.ToTable("Player");
                });

            modelBuilder.Entity("Preferance.Models.SetOfHands", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.ToTable("SetOfHands");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Preferance.Models.Card", b =>
                {
                    b.HasOne("Preferance.Models.Hand", null)
                        .WithMany("Cards")
                        .HasForeignKey("HandId");
                });

            modelBuilder.Entity("Preferance.Models.Game", b =>
                {
                    b.HasOne("Preferance.Models.Player", "ActivePlayer")
                        .WithMany()
                        .HasForeignKey("ActivePlayerId");

                    b.HasOne("Preferance.Models.Player", "Dealer")
                        .WithMany()
                        .HasForeignKey("DealerId");

                    b.HasOne("Preferance.Models.Hand", "Discarded")
                        .WithMany()
                        .HasForeignKey("DiscardedId");

                    b.HasOne("Preferance.Models.Hand", "HandInPlay")
                        .WithMany()
                        .HasForeignKey("HandInPlayId");

                    b.HasOne("Preferance.Models.Match", null)
                        .WithMany("Games")
                        .HasForeignKey("MatchId");

                    b.HasOne("Preferance.Models.Player", "NextPlayer")
                        .WithMany()
                        .HasForeignKey("NextPlayerId");

                    b.HasOne("Preferance.Models.Hand", "Player1Hand")
                        .WithMany()
                        .HasForeignKey("Player1HandId");

                    b.HasOne("Preferance.Models.SetOfHands", "Player1HandResult")
                        .WithMany()
                        .HasForeignKey("Player1HandResultId");

                    b.HasOne("Preferance.Models.Player", "Player1")
                        .WithMany()
                        .HasForeignKey("Player1Id");

                    b.HasOne("Preferance.Models.Hand", "Player2Hand")
                        .WithMany()
                        .HasForeignKey("Player2HandId");

                    b.HasOne("Preferance.Models.SetOfHands", "Player2HandResult")
                        .WithMany()
                        .HasForeignKey("Player2HandResultId");

                    b.HasOne("Preferance.Models.Player", "Player2")
                        .WithMany()
                        .HasForeignKey("Player2Id");

                    b.HasOne("Preferance.Models.Hand", "Player3Hand")
                        .WithMany()
                        .HasForeignKey("Player3HandId");

                    b.HasOne("Preferance.Models.SetOfHands", "Player3HandResult")
                        .WithMany()
                        .HasForeignKey("Player3HandResultId");

                    b.HasOne("Preferance.Models.Player", "Player3")
                        .WithMany()
                        .HasForeignKey("Player3Id");

                    b.HasOne("Preferance.Models.Hand", "Player4Hand")
                        .WithMany()
                        .HasForeignKey("Player4HandId");

                    b.HasOne("Preferance.Models.SetOfHands", "Player4HandResult")
                        .WithMany()
                        .HasForeignKey("Player4HandResultId");

                    b.HasOne("Preferance.Models.Player", "Player4")
                        .WithMany()
                        .HasForeignKey("Player4Id");

                    b.HasOne("Preferance.Models.Hand", "Talon")
                        .WithMany()
                        .HasForeignKey("TalonId");
                });

            modelBuilder.Entity("Preferance.Models.Hand", b =>
                {
                    b.HasOne("Preferance.Models.SetOfHands", null)
                        .WithMany("hands")
                        .HasForeignKey("SetOfHandsId");
                });

            modelBuilder.Entity("Preferance.Models.Match", b =>
                {
                    b.HasOne("Preferance.Models.Player", "Player1")
                        .WithMany()
                        .HasForeignKey("Player1Id");

                    b.HasOne("Preferance.Models.Player", "Player2")
                        .WithMany()
                        .HasForeignKey("Player2Id");

                    b.HasOne("Preferance.Models.Player", "Player3")
                        .WithMany()
                        .HasForeignKey("Player3Id");

                    b.HasOne("Preferance.Models.Player", "Player4")
                        .WithMany()
                        .HasForeignKey("Player4Id");
                });

            modelBuilder.Entity("Preferance.Models.Player", b =>
                {
                    b.HasOne("Preferance.Models.Match", null)
                        .WithMany("AllPlayers")
                        .HasForeignKey("MatchId");
                });
#pragma warning restore 612, 618
        }
    }
}
