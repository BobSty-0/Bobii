using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bobii.Migrations
{
    public partial class Added_deletedate_to_TempChannel_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deletedate",
                table: "TempChannels",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deletedate",
                table: "TempChannels");
        }
    }
}
