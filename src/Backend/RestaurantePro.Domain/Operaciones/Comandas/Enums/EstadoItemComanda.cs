namespace RestaurantePro.Domain.Operaciones.Comandas.Enums
{
    /// <summary>
    /// Estados posibles para un ítem de comanda
    /// </summary>
    public enum EstadoItemComanda
    {
        /// <summary>
        /// El ítem está pendiente de preparación
        /// </summary>
        Pendiente = 0,
        
        /// <summary>
        /// El ítem está en proceso de preparación en cocina/barra
        /// </summary>
        EnPreparacion = 1,
        
        /// <summary>
        /// El ítem está listo para ser entregado al cliente
        /// </summary>
        Listo = 2,
        
        /// <summary>
        /// El ítem ha sido entregado al cliente
        /// </summary>
        Entregado = 3,
        
        /// <summary>
        /// El ítem ha sido cancelado
        /// </summary>
        Cancelado = 4
    }
} 