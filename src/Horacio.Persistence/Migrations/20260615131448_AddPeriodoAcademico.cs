using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Horacio.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodoAcademico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PeriodoAcademicoId",
                table: "tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "periodos_academicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    UsuarioApertura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FechaApertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCierre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodos_academicos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_PeriodoAcademicoId",
                table: "tickets",
                column: "PeriodoAcademicoId");

            migrationBuilder.CreateIndex(
                name: "IX_periodos_academicos_Nombre",
                table: "periodos_academicos",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tickets_periodos_academicos_PeriodoAcademicoId",
                table: "tickets",
                column: "PeriodoAcademicoId",
                principalTable: "periodos_academicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tickets_periodos_academicos_PeriodoAcademicoId",
                table: "tickets");

            migrationBuilder.DropTable(
                name: "periodos_academicos");

            migrationBuilder.DropIndex(
                name: "IX_tickets_PeriodoAcademicoId",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "PeriodoAcademicoId",
                table: "tickets");
        }
    }
}
