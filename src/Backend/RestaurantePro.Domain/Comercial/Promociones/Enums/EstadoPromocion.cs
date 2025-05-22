namespace RestaurantePro.Domain.Comercial.Promociones.Enums
{
    /// <summary>
    /// Estados posibles para una promoción
    /// </summary>
    public enum EstadoPromocion
    {
        /// <summary>
        /// Promoción creada pero aún no activa
        /// </summary>
        Creada = 0,
        
        /// <summary>
        /// Promoción actualmente activa
        /// </summary>
        Activa = 1,
        
        /// <summary>
        /// Promoción pausada temporalmente
        /// </summary>
        Pausada = 2,
        
        /// <summary>
        /// Promoción finalizada (por fecha o límite de usos)
        /// </summary>
        Finalizada = 3,
        
        /// <summary>
        /// Promoción cancelada antes de su fecha de finalización
        /// </summary>
        Cancelada = 4
    }
} 