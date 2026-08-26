using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddOppgaveToEndringslogg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TimeforingId",
                table: "Endringslogger",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "OppgaveId",
                table: "Endringslogger",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Endringslogger_OppgaveId",
                table: "Endringslogger",
                column: "OppgaveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Endringslogger_Oppgaver_OppgaveId",
                table: "Endringslogger",
                column: "OppgaveId",
                principalTable: "Oppgaver",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Endringslogger_Oppgaver_OppgaveId",
                table: "Endringslogger");

            migrationBuilder.DropIndex(
                name: "IX_Endringslogger_OppgaveId",
                table: "Endringslogger");

            migrationBuilder.DropColumn(
                name: "OppgaveId",
                table: "Endringslogger");

            migrationBuilder.AlterColumn<int>(
                name: "TimeforingId",
                table: "Endringslogger",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
