using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToTarjetaFidelizacionNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Comercial",
                table: "TarjetasFidelizacion",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "Comercial",
                table: "TarjetasFidelizacion");
        }
    }
}
