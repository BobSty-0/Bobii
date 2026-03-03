using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bobii_rework.Migrations
{
    /// <inheritdoc />
    public partial class RenamedCustomCommandColorHex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomCommandColorHex",
                table: "InterfaceInformations",
                newName: "CustomCommandBackgroundColorHex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomCommandBackgroundColorHex",
                table: "InterfaceInformations",
                newName: "CustomCommandColorHex");
        }
    }
}
