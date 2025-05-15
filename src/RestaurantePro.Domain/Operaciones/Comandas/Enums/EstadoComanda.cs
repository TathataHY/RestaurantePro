namespace RestaurantePro.Domain.Enums
{
    /// <summary>
    /// Estados posibles de una comanda
    /// </summary>
    public enum EstadoComanda
    {
        /// <summary>
        /// Comanda recién creada, pendiente de preparación
        /// </summary>
        Pendiente = 0,

        /// <summary>
        /// Comanda en proceso de preparación en cocina
        /// </summary>
        EnPreparacion = 1,

        /// <summary>
        /// Comanda lista para ser entregada al cliente
        /// </summary>
        Lista = 2,

        /// <summary>
        /// Comanda entregada al cliente
        /// </summary>
        Entregada = 3,

        /// <summary>
        /// Comanda pagada
        /// </summary>
        Pagada = 4,

        /// <summary>
        /// Comanda cancelada
        /// </summary>
        Cancelada = 5
    }
} 