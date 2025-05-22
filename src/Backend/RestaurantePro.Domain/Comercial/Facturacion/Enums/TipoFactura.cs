namespace RestaurantePro.Domain.Comercial.Facturacion.Enums
{
    /// <summary>
    /// Enumera los diferentes tipos de facturas soportados
    /// </summary>
    public enum TipoFactura
    {
        /// <summary>
        /// Factura normal (para consumidores finales)
        /// </summary>
        Normal = 1,
        
        /// <summary>
        /// Factura para empresas o consumidores con RFC
        /// </summary>
        Fiscal = 2,
        
        /// <summary>
        /// Nota de crédito (abono/devolución)
        /// </summary>
        NotaCredito = 3,
        
        /// <summary>
        /// Factura simplificada (ticket)
        /// </summary>
        Simplificada = 4,
        
        /// <summary>
        /// Factura electrónica
        /// </summary>
        Electronica = 5
    }
} 