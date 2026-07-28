using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class ExtendTimeforingForDugnad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Timeforinger_Oppgaver_OppgaveId",
                table: "Timeforinger");

            migrationBuilder.AlterColumn<int>(
                name: "OppgaveId",
                table: "Timeforinger",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "DugnadId",
                table: "Timeforinger",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Timeforinger_DugnadId",
                table: "Timeforinger",
                column: "DugnadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Timeforinger_Dugnader_DugnadId",
                table: "Timeforinger",
                column: "DugnadId",
                principalTable: "Dugnader",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Timeforinger_Oppgaver_OppgaveId",
                table: "Timeforinger",
                column: "OppgaveId",
                principalTable: "Oppgaver",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Timeforinger_Dugnader_DugnadId",
                table: "Timeforinger");

            migrationBuilder.DropForeignKey(
                name: "FK_Timeforinger_Oppgaver_OppgaveId",
                table: "Timeforinger");

            migrationBuilder.DropIndex(
                name: "IX_Timeforinger_DugnadId",
                table: "Timeforinger");

            migrationBuilder.DropColumn(
                name: "DugnadId",
                table: "Timeforinger");

            migrationBuilder.AlterColumn<int>(
                name: "OppgaveId",
                table: "Timeforinger",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Timeforinger_Oppgaver_OppgaveId",
                table: "Timeforinger",
                column: "OppgaveId",
                principalTable: "Oppgaver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
