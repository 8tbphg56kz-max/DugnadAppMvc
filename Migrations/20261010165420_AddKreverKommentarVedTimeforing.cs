using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddKreverKommentarVedTimeforing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "KreverKommentarVedTimeforing",
                table: "Oppgaver",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KreverKommentarVedTimeforing",
                table: "Oppgaver");
        }
    }
}
