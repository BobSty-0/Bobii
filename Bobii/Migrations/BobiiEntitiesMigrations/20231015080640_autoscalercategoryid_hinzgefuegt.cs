using Microsoft.EntityFrameworkCore.Migrations;

namespace Bobii.Migrations
{
    public partial class autoscalercategoryid_hinzgefuegt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "autoscalercategoryid",
                table: "TempChannels",
                type: "numeric(20,0)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "autoscalercategoryid",
                table: "TempChannels");
        }
    }
}
