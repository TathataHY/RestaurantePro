using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTelefonoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                schema: "Core",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleFactura_ProductoId",
                schema: "Comercial",
                table: "DetalleFactura",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleFactura_Productos_ProductoId",
                schema: "Comercial",
                table: "DetalleFactura",
                column: "ProductoId",
                principalSchema: "Core",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleFactura_Productos_ProductoId",
                schema: "Comercial",
                table: "DetalleFactura");

            migrationBuilder.DropIndex(
                name: "IX_DetalleFactura_ProductoId",
                schema: "Comercial",
                table: "DetalleFactura");

            migrationBuilder.DropColumn(
                name: "Telefono",
                schema: "Core",
                table: "Usuarios");
        }
    }
}
