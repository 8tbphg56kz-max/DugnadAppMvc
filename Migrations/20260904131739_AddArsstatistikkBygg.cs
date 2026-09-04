using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddArsstatistikkBygg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArsstatistikkBygg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Aar = table.Column<int>(type: "integer", nullable: false),
                    ByggKode = table.Column<string>(type: "text", nullable: false),
                    AntallLeiligheter = table.Column<int>(type: "integer", nullable: false),
                    AndelLeiligheter = table.Column<decimal>(type: "numeric", nullable: false),
                    Dugnadstimer = table.Column<decimal>(type: "numeric", nullable: false),
                    AndelDugnadstimer = table.Column<decimal>(type: "numeric", nullable: false),
                    Dugnadsindeks = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArsstatistikkBygg", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArsstatistikkBygg_Aar_ByggKode",
                table: "ArsstatistikkBygg",
                columns: new[] { "Aar", "ByggKode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArsstatistikkBygg");
        }
    }
}
