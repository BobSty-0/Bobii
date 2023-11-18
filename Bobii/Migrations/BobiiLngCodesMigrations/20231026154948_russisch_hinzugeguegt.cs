using Microsoft.EntityFrameworkCore.Migrations;

namespace Bobii.Migrations.BobiiLngCodesMigrations
{
    public partial class russisch_hinzugeguegt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ru",
                table: "Contents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ru",
                table: "Captions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ru",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "ru",
                table: "Captions");
        }
    }
}
