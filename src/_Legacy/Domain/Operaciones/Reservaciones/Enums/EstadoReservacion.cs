namespace RestaurantePro.Domain.Enums
{
    /// <summary>
    /// Estados posibles de una reservación
    /// </summary>
    public enum EstadoReservacion
    {
        /// <summary>
        /// Reservación pendiente de confirmación
        /// </summary>
        Pendiente = 0,

        /// <summary>
        /// Reservación confirmada
        /// </summary>
        Confirmada = 1,

        /// <summary>
        /// Cliente llegó y ocupó la mesa
        /// </summary>
        Ocupada = 2,

        /// <summary>
        /// Reservación completada (cliente se fue)
        /// </summary>
        Completada = 3,

        /// <summary>
        /// Cliente no se presentó
        /// </summary>
        NoShow = 4,

        /// <summary>
        /// Reservación cancelada por el cliente o el restaurante
        /// </summary>
        Cancelada = 5
    }
} 