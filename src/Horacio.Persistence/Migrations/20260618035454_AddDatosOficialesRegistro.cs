using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Horacio.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDatosOficialesRegistro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModuloFormativo",
                table: "registros_matricula",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profesor",
                table: "registros_matricula",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DreGre",
                table: "configuracion",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModalidadServicio",
                table: "configuracion",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NivelFormativo",
                table: "configuracion",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PeriodoLectivo",
                table: "configuracion",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResolucionAutorizacion",
                table: "configuracion",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResolucionCreacion",
                table: "configuracion",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoPlan",
                table: "configuracion",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ugel",
                table: "configuracion",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModuloFormativo",
                table: "registros_matricula");

            migrationBuilder.DropColumn(
                name: "Profesor",
                table: "registros_matricula");

            migrationBuilder.DropColumn(
                name: "DreGre",
                table: "configuracion");

            migrationBuilder.DropColumn(
                name: "ModalidadServicio",
                table: "configuracion");

            migrationBuilder.DropColumn(
                name: "NivelFormativo",
                table: "configuracion");

            migrationBuilder.DropColumn(
                name: "PeriodoLectivo",
                table: "configuracion");

            migrationBuilder.DropColumn(
                name: "ResolucionAutorizacion",
                table: "configuracion");

            migrationBuilder.DropColumn(
                name: "ResolucionCreacion",
                table: "configuracion");

            migrationBuilder.DropColumn(
                name: "TipoPlan",
                table: "configuracion");

            migrationBuilder.DropColumn(
                name: "Ugel",
                table: "configuracion");
        }
    }
}
