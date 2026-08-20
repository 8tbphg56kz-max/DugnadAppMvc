using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddBegrunnelseTilEndringslogg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Begrunnelse",
                table: "Endringslogger",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Endringslogger_BeboerId",
                table: "Endringslogger",
                column: "BeboerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Endringslogger_Beboere_BeboerId",
                table: "Endringslogger",
                column: "BeboerId",
                principalTable: "Beboere",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Endringslogger_Beboere_BeboerId",
                table: "Endringslogger");

            migrationBuilder.DropIndex(
                name: "IX_Endringslogger_BeboerId",
                table: "Endringslogger");

            migrationBuilder.DropColumn(
                name: "Begrunnelse",
                table: "Endringslogger");
        }
    }
}
