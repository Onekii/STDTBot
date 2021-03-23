using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;
using MySql.EntityFrameworkCore.Storage.Internal;

namespace STDTBot.Migrations
{
    public partial class Users : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "STDT_Channels",
                columns: table => new
                {
                    ChannelID = table.Column<long>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ChannelType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STDT_Channels", x => x.ChannelID);
                });

            migrationBuilder.CreateTable(
                name: "STDT_RaidAttendees",
                columns: table => new
                {
                    UserID = table.Column<long>(nullable: false),
                    RaidID = table.Column<long>(nullable: false),
                    MinutesInRaid = table.Column<long>(nullable: false),
                    PointsObtained = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STDT_RaidAttendees", x => new { x.UserID, x.RaidID });
                });

            migrationBuilder.CreateTable(
                name: "STDT_Raids",
                columns: table => new
                {
                    RaidID = table.Column<long>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    DateOfRaid = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STDT_Raids", x => x.RaidID);
                });

            migrationBuilder.CreateTable(
                name: "STDT_RankHistory",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<long>(nullable: false),
                    DateReset = table.Column<DateTime>(nullable: false),
                    RankID = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STDT_RankHistory", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "STDT_Ranks",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    OfflineRole = table.Column<long>(nullable: false),
                    OnlineRole = table.Column<long>(nullable: false),
                    PointsNeeded = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STDT_Ranks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "STDT_Referrals",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ReferredBy = table.Column<long>(nullable: false),
                    ReferralTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STDT_Referrals", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "STDT_Users",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: true),
                    CurrentNickname = table.Column<string>(nullable: true),
                    Joined = table.Column<DateTime>(nullable: false),
                    Left = table.Column<DateTime>(nullable: false),
                    UserAvatar = table.Column<string>(nullable: true),
                    HistoricPoints = table.Column<long>(nullable: false),
                    CurrentPoints = table.Column<long>(nullable: false),
                    CurrentRank = table.Column<long>(nullable: false),
                    IsStreaming = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STDT_Users", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "STDT_Channels");

            migrationBuilder.DropTable(
                name: "STDT_RaidAttendees");

            migrationBuilder.DropTable(
                name: "STDT_Raids");

            migrationBuilder.DropTable(
                name: "STDT_RankHistory");

            migrationBuilder.DropTable(
                name: "STDT_Ranks");

            migrationBuilder.DropTable(
                name: "STDT_Referrals");

            migrationBuilder.DropTable(
                name: "STDT_Users");
        }
    }
}
