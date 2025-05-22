namespace RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums
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
        Egreso = 1,
        
        /// <summary>
        /// Alias de Ingreso - Entrada de productos
        /// </summary>
        Entrada = Ingreso,
        
        /// <summary>
        /// Alias de Egreso - Salida de productos
        /// </summary>
        Salida = Egreso,
        
        /// <summary>
        /// Ajuste de inventario (puede ser positivo o negativo)
        /// </summary>
        Ajuste = 2
    }
} 
