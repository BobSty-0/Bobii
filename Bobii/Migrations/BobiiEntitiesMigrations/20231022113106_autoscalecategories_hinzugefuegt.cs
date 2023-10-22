using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bobii.Migrations
{
    public partial class autoscalecategories_hinzugefuegt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoScaleCategories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channelname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    categoryid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channelsize = table.Column<int>(type: "integer", nullable: true),
                    autodelete = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoScaleCategories", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoScaleCategories");
        }
    }
}
