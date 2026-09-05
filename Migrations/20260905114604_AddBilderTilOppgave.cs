using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddBilderTilOppgave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OppgaveBilder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OppgaveId = table.Column<int>(type: "integer", nullable: false),
                    Filnavn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginaltFilnavn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LastetOpp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OppgaveBilder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OppgaveBilder_Oppgaver_OppgaveId",
                        column: x => x.OppgaveId,
                        principalTable: "Oppgaver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OppgaveBilder_OppgaveId",
                table: "OppgaveBilder",
                column: "OppgaveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OppgaveBilder");
        }
    }
}
