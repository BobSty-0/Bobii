using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bobii.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreateTempChannels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false),
                    tempchannelname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    createchannelid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreateTempChannels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "FilterLink",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false),
                    filterlinkactive = table.Column<bool>(type: "boolean", nullable: false)
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
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false),
                    channelid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false)
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
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false),
                    bezeichnung = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false),
                    filterword = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    replaceword = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterWords", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TempChannels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false),
                    channelid = table.Column<decimal>(type: "numeric(20,0)", maxLength: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempChannels", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreateTempChannels");

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

            migrationBuilder.DropTable(
                name: "TempChannels");
        }
    }
}
