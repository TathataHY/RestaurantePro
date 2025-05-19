namespace RestaurantePro.Domain.Comercial.Clientes.Enums
{
    /// <summary>
    /// Representa los diferentes segmentos en los que se pueden clasificar los clientes
    /// según su comportamiento y patrones de consumo.
    /// </summary>
    public enum SegmentoCliente
    {
        /// <summary>
        /// Cliente nuevo o sin un patrón definido
        /// </summary>
        SinClasificar = 0,
        
        /// <summary>
        /// Cliente que visita con frecuencia pero gasta poco por visita
        /// </summary>
        FrecuenciaAlta = 1,
        
        /// <summary>
        /// Cliente que no visita con mucha frecuencia pero gasta más en cada visita
        /// </summary>
        TicketAlto = 2,
        
        /// <summary>
        /// Cliente que visita con frecuencia y gasta cantidades importantes
        /// </summary>
        Premium = 3,
        
        /// <summary>
        /// Cliente que ha reducido su frecuencia en el período reciente
        /// </summary>
        Decreciente = 4,
        
        /// <summary>
        /// Cliente que ha aumentado su frecuencia en el período reciente
        /// </summary>
        Creciente = 5,
        
        /// <summary>
        /// Cliente que no ha visitado el establecimiento en un período prolongado
        /// </summary>
        Inactivo = 6
    }
} 