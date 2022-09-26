using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bobii.Migrations
{
    public partial class removed_filter_link_and_filter_word_tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilterLink");

            migrationBuilder.DropTable(
                name: "FilterLinkLogs");

            migrationBuilder.DropTable(
                name: "FilterLinkOptions");

            migrationBuilder.DropTable(
                name: "FilterLinksGuild");

            migrationBuilder.DropTable(
                name: "FilterLinkUserGuild");

            migrationBuilder.DropTable(
                name: "FilterWords");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilterLink",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    filterlinkactive = table.Column<bool>(type: "boolean", nullable: false),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterLink", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "FilterLinkLogs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    channelid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterLinkLogs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "FilterLinkOptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bezeichnung = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: true),
                    linkbody = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterLinkOptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "FilterLinksGuild",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bezeichnung = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterLinksGuild", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "FilterLinkUserGuild",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false),
                    userid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterLinkUserGuild", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "FilterWords",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    filterword = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false),
                    replaceword = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterWords", x => x.id);
                });
        }
    }
}
