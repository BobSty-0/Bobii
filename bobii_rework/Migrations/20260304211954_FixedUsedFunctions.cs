using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bobii_rework.Migrations
{
    /// <inheritdoc />
    public partial class FixedUsedFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "userid",
                table: "UsedFunctions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "isuser",
                table: "UsedFunctions",
                newName: "IsUser");

            migrationBuilder.RenameColumn(
                name: "guildid",
                table: "UsedFunctions",
                newName: "GuildId");

            migrationBuilder.RenameColumn(
                name: "function",
                table: "UsedFunctions",
                newName: "Function");

            migrationBuilder.RenameColumn(
                name: "doneat",
                table: "UsedFunctions",
                newName: "DoneAt");

            migrationBuilder.RenameColumn(
                name: "channelid",
                table: "UsedFunctions",
                newName: "ChannelId");

            migrationBuilder.RenameColumn(
                name: "affecteduserid",
                table: "UsedFunctions",
                newName: "AffectedUserId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "UsedFunctions",
                newName: "Id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DoneAt",
                table: "UsedFunctions",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UsedFunctions",
                newName: "userid");

            migrationBuilder.RenameColumn(
                name: "IsUser",
                table: "UsedFunctions",
                newName: "isuser");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "UsedFunctions",
                newName: "guildid");

            migrationBuilder.RenameColumn(
                name: "Function",
                table: "UsedFunctions",
                newName: "function");

            migrationBuilder.RenameColumn(
                name: "DoneAt",
                table: "UsedFunctions",
                newName: "doneat");

            migrationBuilder.RenameColumn(
                name: "ChannelId",
                table: "UsedFunctions",
                newName: "channelid");

            migrationBuilder.RenameColumn(
                name: "AffectedUserId",
                table: "UsedFunctions",
                newName: "affecteduserid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UsedFunctions",
                newName: "id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "doneat",
                table: "UsedFunctions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
