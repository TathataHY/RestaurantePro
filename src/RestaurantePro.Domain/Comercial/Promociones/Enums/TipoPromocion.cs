namespace RestaurantePro.Domain.Enums
{
    public enum TipoPromocion
    {
        /// <summary>
        /// Descuento de un porcentaje sobre el total
        /// </summary>
        PorcentajeTotal = 0,
        
        /// <summary>
        /// Descuento de un valor fijo sobre el total
        /// </summary>
        MontoFijoTotal = 1,
        
        /// <summary>
        /// Descuento de un porcentaje sobre productos específicos
        /// </summary>
        PorcentajeProducto = 2,
        
        /// <summary>
        /// Descuento de un valor fijo sobre productos específicos
        /// </summary>
        MontoFijoProducto = 3,
        
        /// <summary>
        /// 2x1 o promociones similares en productos específicos
        /// </summary>
        ProductoGratis = 4,
        
        /// <summary>
        /// Un producto adicional gratis
        /// </summary>
        RegaloConCompra = 5,
        
        /// <summary>
        /// Postre o bebida de cortesía
        /// </summary>
        Cortesia = 6
    }
} 