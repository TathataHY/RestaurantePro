namespace RestaurantePro.Domain.Comercial.Pagos.Enums
{
    /// <summary>
    /// Enumera los diferentes estados posibles de un pago en el sistema
    /// </summary>
    public enum EstadoPago
    {
        /// <summary>
        /// Pago pendiente de procesamiento o confirmación
        /// </summary>
        Pendiente = 1,

        /// <summary>
        /// Pago completado y confirmado exitosamente
        /// </summary>
        Completado = 2,

        /// <summary>
        /// Pago rechazado por el proveedor de servicios de pago
        /// </summary>
        Rechazado = 3,

        /// <summary>
        /// Pago cancelado por el usuario o el sistema
        /// </summary>
        Cancelado = 4,

        /// <summary>
        /// Pago en proceso de reembolso
        /// </summary>
        EnReembolso = 5,

        /// <summary>
        /// Pago reembolsado completamente
        /// </summary>
        Reembolsado = 6,

        /// <summary>
        /// Pago parcialmente completado
        /// </summary>
        ParcialmentePagado = 7,

        /// <summary>
        /// Pago en espera de verificación
        /// </summary>
        EnVerificacion = 8
    }
} 