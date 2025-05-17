namespace RestaurantePro.Domain.Comercial.Clientes.Enums
{
    /// <summary>
    /// Tipos de operaciones que pueden realizarse con los puntos
    /// </summary>
    public enum TipoOperacionPuntos
    {
        /// <summary>
        /// Puntos agregados a la tarjeta
        /// </summary>
        Agregados = 1,
        
        /// <summary>
        /// Puntos canjeados por el cliente
        /// </summary>
        Canjeados = 2,
        
        /// <summary>
        /// Puntos vencidos por tiempo
        /// </summary>
        Vencidos = 3,
        
        /// <summary>
        /// Ajuste manual de puntos
        /// </summary>
        Ajuste = 4
    }
}
