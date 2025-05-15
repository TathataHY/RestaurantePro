namespace RestaurantePro.Domain.Comercial.Clientes.Enums
{
    /// <summary>
    /// Estado de una tarjeta de fidelización
    /// </summary>
    public enum EstadoTarjeta
    {
        /// <summary>
        /// Tarjeta creada pero no entregada/activada
        /// </summary>
        Emitida = 0,
        
        /// <summary>
        /// Tarjeta entregada al cliente y activada
        /// </summary>
        Activa = 1,
        
        /// <summary>
        /// Tarjeta suspendida temporalmente
        /// </summary>
        Suspendida = 2,
        
        /// <summary>
        /// Tarjeta cancelada por el cliente o la administración
        /// </summary>
        Cancelada = 3,
        
        /// <summary>
        /// Tarjeta expirada por fecha
        /// </summary>
        Expirada = 4,
        
        /// <summary>
        /// Tarjeta reportada como perdida
        /// </summary>
        Perdida = 5,
        
        /// <summary>
        /// Tarjeta reemplazada por una nueva
        /// </summary>
        Reemplazada = 6
    }
} 