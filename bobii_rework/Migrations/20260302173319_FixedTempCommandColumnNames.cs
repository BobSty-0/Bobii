using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bobii_rework.Migrations
{
    /// <inheritdoc />
    public partial class FixedTempCommandColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "enabled",
                table: "Commands",
                newName: "Enabled");

            migrationBuilder.RenameColumn(
                name: "createchannelid",
                table: "Commands",
                newName: "CreateChannelId");

            migrationBuilder.RenameColumn(
                name: "commandname",
                table: "Commands",
                newName: "CommandName");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Commands",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "guildguid",
                table: "Commands",
                newName: "GuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Enabled",
                table: "Commands",
                newName: "enabled");

            migrationBuilder.RenameColumn(
                name: "CreateChannelId",
                table: "Commands",
                newName: "createchannelid");

            migrationBuilder.RenameColumn(
                name: "CommandName",
                table: "Commands",
                newName: "commandname");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Commands",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "Commands",
                newName: "guildguid");
        }
    }
}
