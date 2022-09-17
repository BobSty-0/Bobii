using Microsoft.EntityFrameworkCore.Migrations;

namespace Bobii.Migrations
{
    public partial class deleted_text_channel_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "textchannelid",
                table: "TempChannels");

            migrationBuilder.DropColumn(
                name: "textchannel",
                table: "CreateTempChannels");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "textchannelid",
                table: "TempChannels",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "textchannel",
                table: "CreateTempChannels",
                type: "boolean",
                nullable: true);
        }
    }
}
