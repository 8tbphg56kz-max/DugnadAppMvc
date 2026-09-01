using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddErUtførtToDugnad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErDugnad",
                table: "Timeforinger");

            migrationBuilder.AddColumn<bool>(
                name: "ErUtført",
                table: "Dugnader",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErUtført",
                table: "Dugnader");

            migrationBuilder.AddColumn<bool>(
                name: "ErDugnad",
                table: "Timeforinger",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
