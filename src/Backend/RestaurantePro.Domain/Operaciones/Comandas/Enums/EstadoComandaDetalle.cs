namespace RestaurantePro.Domain.Enums
{
    /// <summary>
    /// Estados posibles de un detalle de comanda
    /// </summary>
    public enum EstadoComandaDetalle
    {
        /// <summary>
        /// Producto pendiente de preparación
        /// </summary>
        Pendiente = 0,

        /// <summary>
        /// Producto en proceso de preparación
        /// </summary>
        EnPreparacion = 1,

        /// <summary>
        /// Producto listo para ser entregado
        /// </summary>
        Listo = 2,

        /// <summary>
        /// Producto entregado al cliente
        /// </summary>
        Entregado = 3,

        /// <summary>
        /// Producto no disponible o cancelado
        /// </summary>
        NoDisponible = 4
    }
}
