using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class ExtendOppgaveModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ErUtført",
                table: "Oppgaver",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Opprettet",
                table: "Oppgaver",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "OpprettetAvId",
                table: "Oppgaver",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Prioritet",
                table: "Oppgaver",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Oppgaver_OpprettetAvId",
                table: "Oppgaver",
                column: "OpprettetAvId");

            migrationBuilder.AddForeignKey(
                name: "FK_Oppgaver_AspNetUsers_OpprettetAvId",
                table: "Oppgaver",
                column: "OpprettetAvId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Oppgaver_AspNetUsers_OpprettetAvId",
                table: "Oppgaver");

            migrationBuilder.DropIndex(
                name: "IX_Oppgaver_OpprettetAvId",
                table: "Oppgaver");

            migrationBuilder.DropColumn(
                name: "ErUtført",
                table: "Oppgaver");

            migrationBuilder.DropColumn(
                name: "Opprettet",
                table: "Oppgaver");

            migrationBuilder.DropColumn(
                name: "OpprettetAvId",
                table: "Oppgaver");

            migrationBuilder.DropColumn(
                name: "Prioritet",
                table: "Oppgaver");
        }
    }
}
