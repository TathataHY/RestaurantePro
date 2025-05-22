namespace RestaurantePro.Domain.Comercial.Facturacion.Enums
{
    /// <summary>
    /// Enumera los diferentes estados posibles de una factura
    /// </summary>
    public enum EstadoFactura
    {
        /// <summary>
        /// Factura en borrador, aún no emitida oficialmente
        /// </summary>
        Borrador = 1,
        
        /// <summary>
        /// Factura emitida y válida
        /// </summary>
        Emitida = 2,
        
        /// <summary>
        /// Factura pagada completamente
        /// </summary>
        Pagada = 3,
        
        /// <summary>
        /// Factura pagada parcialmente
        /// </summary>
        PagadaParcialmente = 4,
        
        /// <summary>
        /// Factura anulada o cancelada
        /// </summary>
        Anulada = 5,
        
        /// <summary>
        /// Factura vencida sin pago
        /// </summary>
        Vencida = 6,
        
        /// <summary>
        /// Factura rectificativa (que corrige una factura anterior)
        /// </summary>
        Rectificativa = 7
    }
} 