using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuitarColumnaEstaEliminadaTarjetaFidelizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstaEliminada",
                schema: "Comercial",
                table: "TarjetasFidelizacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EstaEliminada",
                schema: "Comercial",
                table: "TarjetasFidelizacion",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
