namespace RestaurantePro.Domain.Enums
{
    /// <summary>
    /// Estados posibles de un pago
    /// </summary>
    public enum EstadoPago
    {
        /// <summary>
        /// Pago pendiente de procesamiento
        /// </summary>
        Pendiente = 0,

        /// <summary>
        /// Pago completado exitosamente
        /// </summary>
        Completado = 1,

        /// <summary>
        /// Pago rechazado o fallido
        /// </summary>
        Rechazado = 2,

        /// <summary>
        /// Pago cancelado
        /// </summary>
        Cancelado = 3,

        /// <summary>
        /// Pago en proceso de reembolso
        /// </summary>
        EnReembolso = 4,

        /// <summary>
        /// Pago reembolsado
        /// </summary>
        Reembolsado = 5
    }
} 