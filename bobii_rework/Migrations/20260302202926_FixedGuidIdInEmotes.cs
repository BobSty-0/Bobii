using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bobii_rework.Migrations
{
    /// <inheritdoc />
    public partial class FixedGuidIdInEmotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "GuidId",
                table: "Emotes",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "GuidId",
                table: "Emotes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);
        }
    }
}
