using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddEndringslogg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Endringslogger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tidspunkt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BrukerId = table.Column<string>(type: "text", nullable: false),
                    Handling = table.Column<string>(type: "text", nullable: false),
                    TimeforingId = table.Column<int>(type: "integer", nullable: false),
                    BeboerId = table.Column<int>(type: "integer", nullable: false),
                    Aktivitet = table.Column<string>(type: "text", nullable: true),
                    GamleTimer = table.Column<decimal>(type: "numeric", nullable: true),
                    NyeTimer = table.Column<decimal>(type: "numeric", nullable: true),
                    GammelKommentar = table.Column<string>(type: "text", nullable: true),
                    NyKommentar = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Endringslogger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Endringslogger_AspNetUsers_BrukerId",
                        column: x => x.BrukerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Endringslogger_BrukerId",
                table: "Endringslogger",
                column: "BrukerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Endringslogger");
        }
    }
}
