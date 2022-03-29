using Microsoft.EntityFrameworkCore.Migrations;

namespace Bobii.Migrations
{
    public partial class Added_createchannelid_and_channelownerid_to_tempchannels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "channelownerid",
                table: "TempChannels",
                type: "numeric(20,0)",
                maxLength: 18,
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "createchannelid",
                table: "TempChannels",
                type: "numeric(20,0)",
                maxLength: 18,
                nullable: true,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "channelownerid",
                table: "TempChannels");

            migrationBuilder.DropColumn(
                name: "createchannelid",
                table: "TempChannels");
        }
    }
}
