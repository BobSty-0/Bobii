using Microsoft.EntityFrameworkCore.Migrations;

namespace Bobii.Migrations
{
    public partial class autoscalecategories_hinzugefuegt_emptychannelnumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "emptychannelnumber",
                table: "AutoScaleCategories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "emptychannelnumber",
                table: "AutoScaleCategories");
        }
    }
}
