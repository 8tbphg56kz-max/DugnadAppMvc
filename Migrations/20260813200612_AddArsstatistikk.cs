using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DugnadAppMvc.Migrations
{
    /// <inheritdoc />
    public partial class AddArsstatistikk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arsstatistikker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Aar = table.Column<int>(type: "integer", nullable: false),
                    AntallAktiviteter = table.Column<int>(type: "integer", nullable: false),
                    AntallPameldinger = table.Column<int>(type: "integer", nullable: false),
                    AntallDeltakere = table.Column<int>(type: "integer", nullable: false),
                    AntallTimer = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arsstatistikker", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Arsstatistikker");
        }
    }
}
