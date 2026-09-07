using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class ForbedreEpostVarsel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AntallFeilet",
                table: "EpostVarsler");

            migrationBuilder.DropColumn(
                name: "AntallMottakere",
                table: "EpostVarsler");

            migrationBuilder.DropColumn(
                name: "AntallSendt",
                table: "EpostVarsler");

            migrationBuilder.CreateTable(
                name: "EpostVarselMottakere",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EpostVarselId = table.Column<int>(type: "integer", nullable: false),
                    BeboerId = table.Column<int>(type: "integer", nullable: false),
                    Epostadresse = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SendtDato = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Sendt = table.Column<bool>(type: "boolean", nullable: false),
                    Feilmelding = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpostVarselMottakere", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EpostVarselMottakere_Beboere_BeboerId",
                        column: x => x.BeboerId,
                        principalTable: "Beboere",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EpostVarselMottakere_EpostVarsler_EpostVarselId",
                        column: x => x.EpostVarselId,
                        principalTable: "EpostVarsler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EpostVarselMottakere_BeboerId",
                table: "EpostVarselMottakere",
                column: "BeboerId");

            migrationBuilder.CreateIndex(
                name: "IX_EpostVarselMottakere_EpostVarselId",
                table: "EpostVarselMottakere",
                column: "EpostVarselId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EpostVarselMottakere");

            migrationBuilder.AddColumn<int>(
                name: "AntallFeilet",
                table: "EpostVarsler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AntallMottakere",
                table: "EpostVarsler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AntallSendt",
                table: "EpostVarsler",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
