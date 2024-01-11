using Microsoft.EntityFrameworkCore.Migrations;

namespace Bobii.Migrations.BobiiLngCodesMigrations
{
    public partial class ru_hinzugefuegt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ru",
                table: "Commands",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ru",
                table: "Commands");
        }
    }
}
