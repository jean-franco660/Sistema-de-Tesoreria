using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Horacio.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComprobante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comprobantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Proveedor = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Ruc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TipoDocumento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NumeroComprobante = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HoraEmision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    Igv = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    Total = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Concepto = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    MetodoPago = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Confianza = table.Column<int>(type: "integer", nullable: false),
                    EsDuplicadoProbable = table.Column<bool>(type: "boolean", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImagenRuta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImagenUrl = table.Column<string>(type: "character varying(700)", maxLength: 700, nullable: true),
                    ImagenBase64 = table.Column<string>(type: "text", nullable: true),
                    RespuestaIaJson = table.Column<string>(type: "text", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    PeriodoAcademicoId = table.Column<int>(type: "integer", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comprobantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comprobantes_periodos_academicos_PeriodoAcademicoId",
                        column: x => x.PeriodoAcademicoId,
                        principalTable: "periodos_academicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comprobantes_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "comprobante_productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComprobanteId = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Cantidad = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    Importe = table.Column<decimal>(type: "numeric(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comprobante_productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comprobante_productos_comprobantes_ComprobanteId",
                        column: x => x.ComprobanteId,
                        principalTable: "comprobantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_comprobante_productos_ComprobanteId",
                table: "comprobante_productos",
                column: "ComprobanteId");

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_Categoria",
                table: "comprobantes",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_FechaEmision",
                table: "comprobantes",
                column: "FechaEmision");

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_FechaRegistro",
                table: "comprobantes",
                column: "FechaRegistro");

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_PeriodoAcademicoId",
                table: "comprobantes",
                column: "PeriodoAcademicoId");

            migrationBuilder.CreateIndex(
                name: "IX_comprobantes_UsuarioId",
                table: "comprobantes",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comprobante_productos");

            migrationBuilder.DropTable(
                name: "comprobantes");
        }
    }
}
