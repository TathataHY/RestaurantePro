namespace RestaurantePro.Domain.Inventario.Enums
{
    /// <summary>
    /// Tipos de movimientos de inventario
    /// </summary>
    public enum TipoMovimientoInventario
    {
        /// <summary>
        /// Entrada de productos al inventario (compras, devoluciones, ajustes positivos)
        /// </summary>
        Ingreso = 0,
        
        /// <summary>
        /// Salida de productos del inventario (consumo, ventas, pérdidas, ajustes negativos)
        /// </summary>
        Egreso = 1
    }
} 