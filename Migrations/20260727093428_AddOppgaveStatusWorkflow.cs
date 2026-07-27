using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddOppgaveStatusWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErAktiv",
                table: "OppgavePamelding");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "OppgavePamelding",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UtfortDato",
                table: "OppgavePamelding",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "OppgavePamelding");

            migrationBuilder.DropColumn(
                name: "UtfortDato",
                table: "OppgavePamelding");

            migrationBuilder.AddColumn<bool>(
                name: "ErAktiv",
                table: "OppgavePamelding",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
