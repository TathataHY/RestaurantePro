namespace RestaurantePro.Domain.Comercial.Promociones.Enums
{
    /// <summary>
    /// Tipos de descuentos disponibles en el sistema
    /// </summary>
    public enum TipoDescuento
    {
        /// <summary>
        /// Descuento por porcentaje
        /// </summary>
        Porcentaje = 0,
        
        /// <summary>
        /// Descuento por monto fijo
        /// </summary>
        MontoFijo = 1,
        
        /// <summary>
        /// Descuento promocional especial
        /// </summary>
        Promocional = 2,
        
        /// <summary>
        /// Descuento por empleado
        /// </summary>
        Empleado = 3,
        
        /// <summary>
        /// Descuento por volumen de compra
        /// </summary>
        Volumen = 4,
        
        /// <summary>
        /// Descuento en productos específicos
        /// </summary>
        ProductosEspecificos = 5,
        
        /// <summary>
        /// Descuento por categoría
        /// </summary>
        Categoria = 6,
        
        /// <summary>
        /// Descuento de cortesía
        /// </summary>
        Cortesia = 7,
        
        /// <summary>
        /// Descuento general
        /// </summary>
        General = 8
    }
} 