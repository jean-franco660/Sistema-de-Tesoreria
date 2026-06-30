using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Horacio.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreInstitucion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Ciudad = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CodigoModular = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BaseLegal = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TituloComprobante = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TipoComprobante = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogoBase64 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracion");
        }
    }
}
