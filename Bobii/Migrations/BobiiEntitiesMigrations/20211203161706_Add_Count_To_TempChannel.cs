using Microsoft.EntityFrameworkCore.Migrations;

namespace Bobii.Migrations
{
    public partial class Add_Count_To_TempChannel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "count",
                table: "TempChannels",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "count",
                table: "TempChannels");
        }
    }
}
