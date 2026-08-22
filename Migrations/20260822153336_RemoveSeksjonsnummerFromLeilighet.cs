using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeksjonsnummerFromLeilighet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Leiligheter_Seksjonsnummer",
                table: "Leiligheter");

            migrationBuilder.DropColumn(
                name: "Seksjonsnummer",
                table: "Leiligheter");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Seksjonsnummer",
                table: "Leiligheter",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Leiligheter_Seksjonsnummer",
                table: "Leiligheter",
                column: "Seksjonsnummer",
                unique: true);
        }
    }
}
