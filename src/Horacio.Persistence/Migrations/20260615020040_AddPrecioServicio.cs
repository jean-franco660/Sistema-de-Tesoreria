using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Horacio.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPrecioServicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "servicios",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Precio",
                table: "servicios");
        }
    }
}
