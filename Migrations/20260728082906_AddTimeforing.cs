using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeforing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Timeforinger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OppgaveId = table.Column<int>(type: "integer", nullable: false),
                    BeboerId = table.Column<int>(type: "integer", nullable: false),
                    AntallTimer = table.Column<decimal>(type: "numeric", nullable: false),
                    Kommentar = table.Column<string>(type: "text", nullable: true),
                    RegistrertDato = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timeforinger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timeforinger_Beboere_BeboerId",
                        column: x => x.BeboerId,
                        principalTable: "Beboere",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Timeforinger_Oppgaver_OppgaveId",
                        column: x => x.OppgaveId,
                        principalTable: "Oppgaver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Timeforinger_BeboerId",
                table: "Timeforinger",
                column: "BeboerId");

            migrationBuilder.CreateIndex(
                name: "IX_Timeforinger_OppgaveId",
                table: "Timeforinger",
                column: "OppgaveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Timeforinger");
        }
    }
}
