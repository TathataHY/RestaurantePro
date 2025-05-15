namespace RestaurantePro.Domain.Operaciones.Reservaciones.Enums
{
    /// <summary>
    /// Representa los estados posibles de una reservación en el restaurante
    /// </summary>
    public enum EstadoReservacion
    {
        /// <summary>
        /// La reservación ha sido registrada pero está pendiente de confirmación
        /// </summary>
        Pendiente = 1,
        
        /// <summary>
        /// La reservación ha sido confirmada
        /// </summary>
        Confirmada = 2,
        
        /// <summary>
        /// La reservación ha sido cancelada
        /// </summary>
        Cancelada = 3,
        
        /// <summary>
        /// La reservación se ha completado (los clientes asistieron y fueron atendidos)
        /// </summary>
        Completada = 4,
        
        /// <summary>
        /// Los clientes no se presentaron a la reservación
        /// </summary>
        NoShow = 5
    }
} 