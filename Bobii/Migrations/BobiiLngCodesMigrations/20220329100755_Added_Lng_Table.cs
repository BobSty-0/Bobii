using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bobii.Migrations.BobiiLngCodesMigrations
{
    public partial class Added_Lng_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Captions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    msgid = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    en = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    de = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Captions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    command = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    en = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    de = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Contents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    msgid = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    en = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    de = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false),
                    langugeshort = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Captions");

            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "Contents");

            migrationBuilder.DropTable(
                name: "Languages");
        }
    }
}
