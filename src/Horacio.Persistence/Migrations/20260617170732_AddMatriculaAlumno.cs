using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Horacio.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatriculaAlumno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PeriodoAcademicoId",
                table: "alumnos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Seccion",
                table: "alumnos",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_alumnos_PeriodoAcademicoId",
                table: "alumnos",
                column: "PeriodoAcademicoId");

            migrationBuilder.AddForeignKey(
                name: "FK_alumnos_periodos_academicos_PeriodoAcademicoId",
                table: "alumnos",
                column: "PeriodoAcademicoId",
                principalTable: "periodos_academicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alumnos_periodos_academicos_PeriodoAcademicoId",
                table: "alumnos");

            migrationBuilder.DropIndex(
                name: "IX_alumnos_PeriodoAcademicoId",
                table: "alumnos");

            migrationBuilder.DropColumn(
                name: "PeriodoAcademicoId",
                table: "alumnos");

            migrationBuilder.DropColumn(
                name: "Seccion",
                table: "alumnos");
        }
    }
}
