namespace RestaurantePro.Domain.Enums
{
    /// <summary>
    /// Tipos de movimientos de inventario
    /// </summary>
    public enum TipoMovimientoInventario
    {
        /// <summary>
        /// Entrada por compra de ingredientes
        /// </summary>
        Entrada = 0,
        
        /// <summary>
        /// Salida por uso en comanda/preparación
        /// </summary>
        Salida = 1,
        
        /// <summary>
        /// Ajuste manual de inventario (positivo o negativo)
        /// </summary>
        Ajuste = 2,
        
        /// <summary>
        /// Devolución a proveedor
        /// </summary>
        Devolucion = 3,
        
        /// <summary>
        /// Merma por caducidad
        /// </summary>
        Merma = 4,
        
        /// <summary>
        /// Traspaso entre bodegas/ubicaciones
        /// </summary>
        Traspaso = 5,
        
        /// <summary>
        /// Conteo físico de inventario
        /// </summary>
        Inventario = 6
    }
} 