using Microsoft.EntityFrameworkCore.Migrations;

namespace Bobii.Migrations
{
    public partial class Added_guildId_to_filterlinkoptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "guildid",
                table: "FilterLinkOptions",
                type: "numeric(20,0)",
                maxLength: 18,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "guildid",
                table: "FilterLinkOptions");
        }
    }
}
