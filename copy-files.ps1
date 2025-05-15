# Script para copiar archivos a la nueva estructura de RestaurantePro.Domain

$sourceDomain = "src\RestaurantePro.Domain"
$targetDomain = "src\RestaurantePro.Domain.New"

# 1. Base
Copy-Item -Path "$sourceDomain\Entities\BaseEntity.cs" -Destination "$targetDomain\Base\Entities" -Force

# 2. Comandas
Copy-Item -Path "$sourceDomain\Entities\Comanda.cs" -Destination "$targetDomain\Comandas\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\ComandaDetalle.cs" -Destination "$targetDomain\Comandas\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\ComandaDetallePersonalizacion.cs" -Destination "$targetDomain\Comandas\Entities" -Force
Copy-Item -Path "$sourceDomain\Enums\EstadoComanda.cs" -Destination "$targetDomain\Comandas\Enums" -Force
Copy-Item -Path "$sourceDomain\Enums\EstadoComandaDetalle.cs" -Destination "$targetDomain\Comandas\Enums" -Force
Copy-Item -Path "$sourceDomain\Enums\AccionPersonalizacion.cs" -Destination "$targetDomain\Comandas\Enums" -Force

# 3. Productos
Copy-Item -Path "$sourceDomain\Entities\Producto.cs" -Destination "$targetDomain\Productos\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\Categoria.cs" -Destination "$targetDomain\Productos\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\ProductoIngrediente.cs" -Destination "$targetDomain\Productos\Entities" -Force

# 4. Ingredientes
Copy-Item -Path "$sourceDomain\Entities\Ingrediente.cs" -Destination "$targetDomain\Ingredientes\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\IngredienteProducto.cs" -Destination "$targetDomain\Ingredientes\Entities" -Force

# 5. Inventario
Copy-Item -Path "$sourceDomain\Entities\Inventario.cs" -Destination "$targetDomain\Inventario\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\MovimientoInventario.cs" -Destination "$targetDomain\Inventario\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\InventarioMovimiento.cs" -Destination "$targetDomain\Inventario\Entities" -Force
Copy-Item -Path "$sourceDomain\Enums\TipoMovimientoInventario.cs" -Destination "$targetDomain\Inventario\Enums" -Force
Copy-Item -Path "$sourceDomain\Enums\EstadoInventario.cs" -Destination "$targetDomain\Inventario\Enums" -Force

# 6. Proveedores
Copy-Item -Path "$sourceDomain\Entities\Proveedor.cs" -Destination "$targetDomain\Proveedores\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\ProveedorCategoria.cs" -Destination "$targetDomain\Proveedores\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\ProveedorIngrediente.cs" -Destination "$targetDomain\Proveedores\Entities" -Force

# 7. Compras
Copy-Item -Path "$sourceDomain\Entities\OrdenCompra.cs" -Destination "$targetDomain\Compras\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\DetalleOrdenCompra.cs" -Destination "$targetDomain\Compras\Entities" -Force
Copy-Item -Path "$sourceDomain\Enums\EstadoOrdenCompra.cs" -Destination "$targetDomain\Compras\Enums" -Force
Copy-Item -Path "$sourceDomain\Enums\TipoMovimiento.cs" -Destination "$targetDomain\Compras\Enums" -Force

# 8. Clientes
Copy-Item -Path "$sourceDomain\Entities\Cliente.cs" -Destination "$targetDomain\Clientes\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\TarjetaFidelizacion.cs" -Destination "$targetDomain\Clientes\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\NivelFidelizacion.cs" -Destination "$targetDomain\Clientes\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\HistorialPuntos.cs" -Destination "$targetDomain\Clientes\Entities" -Force
Copy-Item -Path "$sourceDomain\Enums\EstadoTarjeta.cs" -Destination "$targetDomain\Clientes\Enums" -Force
Copy-Item -Path "$sourceDomain\Enums\TipoMovimientoPuntos.cs" -Destination "$targetDomain\Clientes\Enums" -Force

# 9. Promociones
Copy-Item -Path "$sourceDomain\Entities\Promocion.cs" -Destination "$targetDomain\Promociones\Entities" -Force
Copy-Item -Path "$sourceDomain\Enums\TipoPromocion.cs" -Destination "$targetDomain\Promociones\Enums" -Force

# 10. Reservaciones
Copy-Item -Path "$sourceDomain\Entities\Reservacion.cs" -Destination "$targetDomain\Reservaciones\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\Mesa.cs" -Destination "$targetDomain\Reservaciones\Entities" -Force
Copy-Item -Path "$sourceDomain\Enums\EstadoReservacion.cs" -Destination "$targetDomain\Reservaciones\Enums" -Force
Copy-Item -Path "$sourceDomain\Enums\EstadoMesa.cs" -Destination "$targetDomain\Reservaciones\Enums" -Force

# 11. Pagos
Copy-Item -Path "$sourceDomain\Entities\Pago.cs" -Destination "$targetDomain\Pagos\Entities" -Force
Copy-Item -Path "$sourceDomain\Enums\EstadoPago.cs" -Destination "$targetDomain\Pagos\Enums" -Force
Copy-Item -Path "$sourceDomain\Enums\MetodoPago.cs" -Destination "$targetDomain\Pagos\Enums" -Force

# 12. Usuarios
Copy-Item -Path "$sourceDomain\Entities\Usuario.cs" -Destination "$targetDomain\Usuarios\Entities" -Force
Copy-Item -Path "$sourceDomain\Entities\ApplicationUser.cs" -Destination "$targetDomain\Usuarios\Entities" -Force

Write-Host "Archivos copiados correctamente a la nueva estructura." 