using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFellesDugnad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FellesDugnader");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FellesDugnader",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Beskrivelse = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ErAvlyst = table.Column<bool>(type: "boolean", nullable: false),
                    Oppmotested = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SluttTid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartTid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tittel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FellesDugnader", x => x.Id);
                });
        }
    }
}
