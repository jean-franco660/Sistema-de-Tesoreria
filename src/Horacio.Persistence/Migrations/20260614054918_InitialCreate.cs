using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Horacio.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Usuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Accion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Detalle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Serie = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    UltimoValor = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "programas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "turnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turnos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "alumnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Dni = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Nombres = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Apellidos = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ProgramaId = table.Column<int>(type: "integer", nullable: false),
                    TurnoId = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alumnos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alumnos_programas_ProgramaId",
                        column: x => x.ProgramaId,
                        principalTable: "programas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_alumnos_turnos_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "turnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroTicket = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Contador = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AlumnoId = table.Column<int>(type: "integer", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tickets_alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tickets_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalles_ticket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    ServicioId = table.Column<int>(type: "integer", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalles_ticket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_detalles_ticket_servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_detalles_ticket_tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alumnos_Dni",
                table: "alumnos",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alumnos_ProgramaId",
                table: "alumnos",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_alumnos_TurnoId",
                table: "alumnos",
                column: "TurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Fecha",
                table: "audit_logs",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_contadores_Nombre",
                table: "contadores",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_detalles_ticket_ServicioId",
                table: "detalles_ticket",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_detalles_ticket_TicketId",
                table: "detalles_ticket",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_programas_Nombre",
                table: "programas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_servicios_Nombre",
                table: "servicios",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tickets_AlumnoId",
                table: "tickets",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_Contador",
                table: "tickets",
                column: "Contador",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tickets_FechaEmision",
                table: "tickets",
                column: "FechaEmision");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_NumeroTicket",
                table: "tickets",
                column: "NumeroTicket");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_UsuarioId",
                table: "tickets",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_turnos_Nombre",
                table: "turnos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Username",
                table: "usuarios",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "contadores");

            migrationBuilder.DropTable(
                name: "detalles_ticket");

            migrationBuilder.DropTable(
                name: "servicios");

            migrationBuilder.DropTable(
                name: "tickets");

            migrationBuilder.DropTable(
                name: "alumnos");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "programas");

            migrationBuilder.DropTable(
                name: "turnos");
        }
    }
}
