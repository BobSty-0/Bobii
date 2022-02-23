using Microsoft.EntityFrameworkCore.Migrations;

namespace Bobii.Migrations
{
    public partial class Added_delay_to_CreateTempChannel_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "delay",
                table: "CreateTempChannels",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "delay",
                table: "CreateTempChannels");
        }
    }
}
