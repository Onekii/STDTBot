using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;
using MySql.Data.EntityFrameworkCore.Storage.Internal;

namespace STDTBot.Migrations
{
    public partial class permissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "STDT_Command_Channels",
                columns: table => new
                {
                    CommandName = table.Column<string>(nullable: false, maxLength: 50),
                    ChannelID = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STDT_Command_Channels", x => new { x.CommandName, x.ChannelID });
                });

            migrationBuilder.CreateTable(
                name: "STDT_Command_Roles",
                columns: table => new
                {
                    CommandName = table.Column<string>(nullable: false, maxLength: 50),
                    MinimumRole = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STDT_Command_Roles", x => x.CommandName);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "STDT_Command_Channels");

            migrationBuilder.DropTable(
                name: "STDT_Command_Roles");
        }
    }
}
