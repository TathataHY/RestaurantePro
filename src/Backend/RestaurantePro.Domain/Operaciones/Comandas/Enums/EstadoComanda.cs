namespace RestaurantePro.Domain.Operaciones.Comandas.Enums
{
    /// <summary>
    /// Estados posibles para una comanda en el restaurante
    /// </summary>
    public enum EstadoComanda
    {
        /// <summary>
        /// Comanda recién creada, aún no enviada a cocina
        /// </summary>
        Creada = 0,
        
        /// <summary>
        /// Comanda en proceso de preparación en cocina
        /// </summary>
        EnProceso = 1,
        
        /// <summary>
        /// Comanda lista para ser entregada al cliente
        /// </summary>
        Lista = 2,
        
        /// <summary>
        /// Comanda entregada al cliente
        /// </summary>
        Entregada = 3,
        
        /// <summary>
        /// Comanda finalizada (pagada)
        /// </summary>
        Finalizada = 4,
        
        /// <summary>
        /// Comanda cancelada
        /// </summary>
        Cancelada = 5
    }
} 