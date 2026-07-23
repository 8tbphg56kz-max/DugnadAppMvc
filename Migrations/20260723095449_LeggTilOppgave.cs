using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class LeggTilOppgave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Oppgaver",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Navn = table.Column<string>(type: "text", nullable: false),
                    Beskrivelse = table.Column<string>(type: "text", nullable: true),
                    FraDato = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Frist = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AntallPersoner = table.Column<int>(type: "integer", nullable: false),
                    KanRegistrereTimer = table.Column<bool>(type: "boolean", nullable: false),
                    KreverBekreftelse = table.Column<bool>(type: "boolean", nullable: false),
                    Utstyr = table.Column<string>(type: "text", nullable: true),
                    UtstyrPlassering = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oppgaver", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Oppgaver");
        }
    }
}
