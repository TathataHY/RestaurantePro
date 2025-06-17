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
        Electronica = 5,
        
        /// <summary>
        /// Factura de venta (alias para Normal)
        /// </summary>
        Venta = 6,
        
        /// <summary>
        /// Factura de devolución
        /// </summary>
        Devolucion = 7,
        
        /// <summary>
        /// Nota de débito
        /// </summary>
        NotaDebito = 8,

        /// <summary>
        /// Factura de compra a proveedores
        /// </summary>
        Compra = 9
    }
} 