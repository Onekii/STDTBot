using Microsoft.EntityFrameworkCore.Migrations;

namespace STDTBot.Migrations
{
    public partial class raidWeighting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RaidWeighting",
                table: "STDT_Ranks",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RaidWeighting",
                table: "STDT_Ranks");
        }
    }
}
