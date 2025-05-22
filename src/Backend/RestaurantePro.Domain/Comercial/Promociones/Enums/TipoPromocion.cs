namespace RestaurantePro.Domain.Comercial.Promociones.Enums
{
    /// <summary>
    /// Tipos de promociones disponibles en el sistema
    /// </summary>
    public enum TipoPromocion
    {
        /// <summary>
        /// Descuento de un porcentaje sobre el total de la comanda
        /// </summary>
        PorcentajeTotal = 0,
        
        /// <summary>
        /// Descuento de un valor fijo sobre el total de la comanda
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
        /// Un producto adicional gratis con la compra
        /// </summary>
        RegaloConCompra = 5,
        
        /// <summary>
        /// Postre o bebida de cortesía
        /// </summary>
        Cortesia = 6,
        
        /// <summary>
        /// Canje de puntos de fidelización por descuento
        /// </summary>
        CanjePuntos = 7,
        
        /// <summary>
        /// Envío gratuito o con descuento para comandas a domicilio
        /// </summary>
        EnvioGratis = 8
    }
} 