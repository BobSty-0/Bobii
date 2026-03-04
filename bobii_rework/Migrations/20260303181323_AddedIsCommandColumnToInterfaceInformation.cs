using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bobii_rework.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsCommandColumnToInterfaceInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCommand",
                table: "InterfaceInformations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCommand",
                table: "InterfaceInformations");
        }
    }
}
