namespace RestaurantePro.Domain.Comercial.Clientes.Enums
{
    /// <summary>
    /// Tipos de movimientos de puntos en tarjetas de fidelización
    /// </summary>
    public enum TipoMovimientoPuntos
    {
        /// <summary>
        /// Puntos ganados por consumo
        /// </summary>
        Acumulacion = 0,
        
        /// <summary>
        /// Puntos usados para canjear una promoción
        /// </summary>
        Canje = 1,
        
        /// <summary>
        /// Puntos expirados por tiempo
        /// </summary>
        Expiracion = 2,
        
        /// <summary>
        /// Puntos ajustados manualmente por la administración
        /// </summary>
        AjusteManual = 3,
        
        /// <summary>
        /// Puntos otorgados como bono de bienvenida
        /// </summary>
        BonoInicial = 4,
        
        /// <summary>
        /// Puntos otorgados en el cumpleaños del cliente
        /// </summary>
        BonoCumpleaños = 5,
        
        /// <summary>
        /// Puntos otorgados por una promoción especial
        /// </summary>
        BonoPromocion = 6,
        
        /// <summary>
        /// Puntos otorgados por registro en línea
        /// </summary>
        BonoRegistro = 7
    }
} 