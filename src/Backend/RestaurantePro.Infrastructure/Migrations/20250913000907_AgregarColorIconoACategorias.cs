using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarColorIconoACategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Precio",
                schema: "Core",
                table: "Productos",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                schema: "Core",
                table: "ProductoCategorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Icono",
                schema: "Core",
                table: "ProductoCategorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                schema: "Core",
                table: "ProductoCategorias");

            migrationBuilder.DropColumn(
                name: "Icono",
                schema: "Core",
                table: "ProductoCategorias");

            migrationBuilder.AlterColumn<decimal>(
                name: "Precio",
                schema: "Core",
                table: "Productos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);
        }
    }
}
