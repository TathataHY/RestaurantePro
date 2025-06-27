using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantePro.Infrastructure.Migrations.RestauranteProDb
{
    /// <inheritdoc />
    public partial class FixRowVersionConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredientes_Proveedores_ProveedorPrincipalId",
                schema: "Inventario",
                table: "Ingredientes");

            migrationBuilder.RenameTable(
                name: "MovimientosInventario",
                schema: "Inventario",
                newName: "MovimientosInventario");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                schema: "Inventario",
                table: "Ingredientes",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "rowversion",
                oldRowVersion: true,
                oldDefaultValue: new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                schema: "Inventario",
                table: "Ingredientes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                schema: "Inventario",
                table: "Ingredientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EstaEliminado",
                schema: "Inventario",
                table: "Ingredientes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "Inventario",
                table: "Ingredientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TipoMovimiento",
                table: "MovimientosInventario",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Motivo",
                table: "MovimientosInventario",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "MovimientosInventario",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredientes_Proveedores_ProveedorPrincipalId",
                schema: "Inventario",
                table: "Ingredientes",
                column: "ProveedorPrincipalId",
                principalSchema: "Proveedores",
                principalTable: "Proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredientes_Proveedores_ProveedorPrincipalId",
                schema: "Inventario",
                table: "Ingredientes");

            migrationBuilder.RenameTable(
                name: "MovimientosInventario",
                newName: "MovimientosInventario",
                newSchema: "Inventario");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                schema: "Inventario",
                table: "Ingredientes",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 },
                oldClrType: typeof(byte[]),
                oldType: "rowversion",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                schema: "Inventario",
                table: "Ingredientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                schema: "Inventario",
                table: "Ingredientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EstaEliminado",
                schema: "Inventario",
                table: "Ingredientes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "Inventario",
                table: "Ingredientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoMovimiento",
                schema: "Inventario",
                table: "MovimientosInventario",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Motivo",
                schema: "Inventario",
                table: "MovimientosInventario",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                schema: "Inventario",
                table: "MovimientosInventario",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredientes_Proveedores_ProveedorPrincipalId",
                schema: "Inventario",
                table: "Ingredientes",
                column: "ProveedorPrincipalId",
                principalSchema: "Proveedores",
                principalTable: "Proveedores",
                principalColumn: "Id");
        }
    }
}
