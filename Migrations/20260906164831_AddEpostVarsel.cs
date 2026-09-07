using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddEpostVarsel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EpostVarsler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OppgaveId = table.Column<int>(type: "integer", nullable: true),
                    DugnadId = table.Column<int>(type: "integer", nullable: true),
                    SendtDato = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SendtAvBrukerId = table.Column<string>(type: "text", nullable: false),
                    AntallMottakere = table.Column<int>(type: "integer", nullable: false),
                    AntallSendt = table.Column<int>(type: "integer", nullable: false),
                    AntallFeilet = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpostVarsler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EpostVarsler_AspNetUsers_SendtAvBrukerId",
                        column: x => x.SendtAvBrukerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EpostVarsler_Dugnader_DugnadId",
                        column: x => x.DugnadId,
                        principalTable: "Dugnader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EpostVarsler_Oppgaver_OppgaveId",
                        column: x => x.OppgaveId,
                        principalTable: "Oppgaver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EpostVarsler_DugnadId",
                table: "EpostVarsler",
                column: "DugnadId");

            migrationBuilder.CreateIndex(
                name: "IX_EpostVarsler_OppgaveId",
                table: "EpostVarsler",
                column: "OppgaveId");

            migrationBuilder.CreateIndex(
                name: "IX_EpostVarsler_SendtAvBrukerId",
                table: "EpostVarsler",
                column: "SendtAvBrukerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EpostVarsler");
        }
    }
}
