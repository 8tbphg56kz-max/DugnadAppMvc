using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddBilderTilDugnad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DugnadBilder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DugnadId = table.Column<int>(type: "integer", nullable: false),
                    Filnavn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginaltFilnavn = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LastetOpp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DugnadBilder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DugnadBilder_Dugnader_DugnadId",
                        column: x => x.DugnadId,
                        principalTable: "Dugnader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DugnadBilder_DugnadId",
                table: "DugnadBilder",
                column: "DugnadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DugnadBilder");
        }
    }
}
