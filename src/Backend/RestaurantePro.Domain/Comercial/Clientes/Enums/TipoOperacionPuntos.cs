namespace RestaurantePro.Domain.Comercial.Clientes.Enums
{
    /// <summary>
    /// Tipo de operación realizada con puntos de fidelización
    /// </summary>
    public enum TipoOperacionPuntos
    {
        /// <summary>
        /// Puntos agregados a la tarjeta (compras, bonificaciones, etc.)
        /// </summary>
        Agregados = 0,
        
        /// <summary>
        /// Puntos canjeados por beneficios
        /// </summary>
        Canjeados = 1,
        
        /// <summary>
        /// Puntos vencidos por tiempo o políticas
        /// </summary>
        Vencidos = 2,
        
        /// <summary>
        /// Puntos ajustados manualmente (corrección, compensación, etc.)
        /// </summary>
        Ajuste = 3
    }
} 