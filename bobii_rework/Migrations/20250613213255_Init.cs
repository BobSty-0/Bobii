using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace bobii_rework.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    commandname = table.Column<string>(type: "text", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    guildguid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    createchannelid = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CreateTempChannels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    tempchannelname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    createchannelid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channelsize = table.Column<int>(type: "integer", nullable: true),
                    delay = table.Column<int>(type: "integer", nullable: true),
                    autodelete = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreateTempChannels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "InterfaceInformations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CommandName = table.Column<string>(type: "text", nullable: false),
                    EmoteId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CustomCommandName = table.Column<string>(type: "text", nullable: false),
                    CustomCommandColorRGBA = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterfaceInformations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TempChannels",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channelid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    createchannelid = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    channelownerid = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    count = table.Column<int>(type: "integer", nullable: false),
                    deletedate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    unixtimestamp = table.Column<long>(type: "bigint", nullable: false),
                    autoscale = table.Column<bool>(type: "boolean", nullable: false),
                    autoscalercategoryid = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempChannels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TempChannelUserConfigs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    userid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    createchannelid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    tempchannelname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    channelsize = table.Column<int>(type: "integer", nullable: true),
                    autodelete = table.Column<int>(type: "integer", nullable: true),
                    usernamemode = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempChannelUserConfigs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UsedFunctions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    function = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    userid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    affecteduserid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    doneat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    channelid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    guildid = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    isuser = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsedFunctions", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "CreateTempChannels");

            migrationBuilder.DropTable(
                name: "InterfaceInformations");

            migrationBuilder.DropTable(
                name: "TempChannels");

            migrationBuilder.DropTable(
                name: "TempChannelUserConfigs");

            migrationBuilder.DropTable(
                name: "UsedFunctions");
        }
    }
}
