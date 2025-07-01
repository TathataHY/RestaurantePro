using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CambioEstadoOrdenCompraInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Inventario");

            migrationBuilder.CreateTable(
                name: "OrdenesCompra",
                schema: "Inventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEntregaEstimada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCancelacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ObservacionesRecepcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompra", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedor",
                schema: "Inventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreContacto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodigoPostal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pais = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RFC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InformacionBancaria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiasCredito = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimaOrden = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemsOrdenCompra",
                schema: "Inventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdenCompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreIngrediente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsOrdenCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsOrdenCompra_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalSchema: "Inventario",
                        principalTable: "OrdenesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactoProveedor",
                schema: "Inventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactoProveedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactoProveedor_Proveedor_ProveedorId",
                        column: x => x.ProveedorId,
                        principalSchema: "Inventario",
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingredientes",
                schema: "Inventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StockMinimo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    ProveedorPrincipalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Rotacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Temporada = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BloqueadoControlCalidad = table.Column<bool>(type: "bit", nullable: false),
                    CostoPromedio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredientes_Proveedor_ProveedorPrincipalId",
                        column: x => x.ProveedorPrincipalId,
                        principalSchema: "Inventario",
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                schema: "Inventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CantidadFinal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    EstaAplicado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Ingrediente",
                        column: x => x.IngredienteId,
                        principalSchema: "Inventario",
                        principalTable: "Ingredientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactoProveedor_ProveedorId",
                schema: "Inventario",
                table: "ContactoProveedor",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredientes_Codigo",
                schema: "Inventario",
                table: "Ingredientes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingredientes_Nombre",
                schema: "Inventario",
                table: "Ingredientes",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredientes_ProveedorPrincipalId",
                schema: "Inventario",
                table: "Ingredientes",
                column: "ProveedorPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredientes_Stock",
                schema: "Inventario",
                table: "Ingredientes",
                column: "Stock");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsOrdenCompra_IngredienteId",
                schema: "Inventario",
                table: "ItemsOrdenCompra",
                column: "IngredienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsOrdenCompra_OrdenCompraId",
                schema: "Inventario",
                table: "ItemsOrdenCompra",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_EstaAplicado",
                schema: "Inventario",
                table: "MovimientosInventario",
                column: "EstaAplicado");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_Fecha",
                schema: "Inventario",
                table: "MovimientosInventario",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_IngredienteId",
                schema: "Inventario",
                table: "MovimientosInventario",
                column: "IngredienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_TipoMovimiento",
                schema: "Inventario",
                table: "MovimientosInventario",
                column: "TipoMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_Estado",
                schema: "Inventario",
                table: "OrdenesCompra",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_FechaEmision",
                schema: "Inventario",
                table: "OrdenesCompra",
                column: "FechaEmision");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_ProveedorId",
                schema: "Inventario",
                table: "OrdenesCompra",
                column: "ProveedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactoProveedor",
                schema: "Inventario");

            migrationBuilder.DropTable(
                name: "ItemsOrdenCompra",
                schema: "Inventario");

            migrationBuilder.DropTable(
                name: "MovimientosInventario",
                schema: "Inventario");

            migrationBuilder.DropTable(
                name: "OrdenesCompra",
                schema: "Inventario");

            migrationBuilder.DropTable(
                name: "Ingredientes",
                schema: "Inventario");

            migrationBuilder.DropTable(
                name: "Proveedor",
                schema: "Inventario");
        }
    }
}
