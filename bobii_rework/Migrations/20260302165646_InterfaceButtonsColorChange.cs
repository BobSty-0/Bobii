using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bobii_rework.Migrations
{
    /// <inheritdoc />
    public partial class InterfaceButtonsColorChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomCommandColorRGBA",
                table: "InterfaceInformations",
                newName: "CustomCommandColorHex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomCommandColorHex",
                table: "InterfaceInformations",
                newName: "CustomCommandColorRGBA");
        }
    }
}
