using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class FjernKreverBekreftelseFraOppgave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KreverBekreftelse",
                table: "Oppgaver");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "KreverBekreftelse",
                table: "Oppgaver",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
