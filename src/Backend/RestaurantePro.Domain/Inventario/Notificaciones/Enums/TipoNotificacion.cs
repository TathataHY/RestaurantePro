namespace RestaurantePro.Domain.Inventario.Notificaciones.Enums
{
    /// <summary>
    /// Tipos de notificaciones del sistema
    /// </summary>
    public enum TipoNotificacion
    {
        /// <summary>
        /// Notificación informativa
        /// </summary>
        Informativa = 1,
        
        /// <summary>
        /// Advertencia que requiere atención pero no es crítica
        /// </summary>
        Advertencia = 2,
        
        /// <summary>
        /// Alerta que requiere atención urgente
        /// </summary>
        Alerta = 3,
        
        /// <summary>
        /// Error en el sistema
        /// </summary>
        Error = 4,
        
        /// <summary>
        /// Notificación sobre stock bajo de un ingrediente
        /// </summary>
        StockBajo = 5,
        
        /// <summary>
        /// Notificación sobre orden de compra generada
        /// </summary>
        OrdenCompraGenerada = 6,
        
        /// <summary>
        /// Notificación personalizada
        /// </summary>
        Personalizada = 7
    }
} 