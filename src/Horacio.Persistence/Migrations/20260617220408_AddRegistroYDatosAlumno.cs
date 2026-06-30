using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Horacio.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistroYDatosAlumno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Celular",
                table: "alumnos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "alumnos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sexo",
                table: "alumnos",
                type: "character varying(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "registros_matricula",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodoAcademicoId = table.Column<int>(type: "integer", nullable: false),
                    ProgramaId = table.Column<int>(type: "integer", nullable: false),
                    TurnoId = table.Column<int>(type: "integer", nullable: false),
                    Seccion = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registros_matricula", x => x.Id);
                    table.ForeignKey(
                        name: "FK_registros_matricula_periodos_academicos_PeriodoAcademicoId",
                        column: x => x.PeriodoAcademicoId,
                        principalTable: "periodos_academicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registros_matricula_programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "programas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_registros_matricula_turnos_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "turnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_registros_matricula_PeriodoAcademicoId_ProgramaId_TurnoId_S~",
                table: "registros_matricula",
                columns: new[] { "PeriodoAcademicoId", "ProgramaId", "TurnoId", "Seccion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registros_matricula_ProgramaId",
                table: "registros_matricula",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_registros_matricula_TurnoId",
                table: "registros_matricula",
                column: "TurnoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registros_matricula");

            migrationBuilder.DropColumn(
                name: "Celular",
                table: "alumnos");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "alumnos");

            migrationBuilder.DropColumn(
                name: "Sexo",
                table: "alumnos");
        }
    }
}
