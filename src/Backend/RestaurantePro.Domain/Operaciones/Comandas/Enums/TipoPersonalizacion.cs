namespace RestaurantePro.Domain.Operaciones.Comandas.Enums
{
    /// <summary>
    /// Tipos de personalización disponibles para productos
    /// </summary>
    public enum TipoPersonalizacion
    {
        /// <summary>
        /// Personalización de ingredientes
        /// </summary>
        Ingrediente = 0,
        
        /// <summary>
        /// Personalización de tamaño o porción
        /// </summary>
        Tamaño = 1,
        
        /// <summary>
        /// Personalización de cocción (término, nivel)
        /// </summary>
        Coccion = 2,
        
        /// <summary>
        /// Personalización de condimentos y salsas
        /// </summary>
        Condimento = 3,
        
        /// <summary>
        /// Personalización de temperatura
        /// </summary>
        Temperatura = 4,
        
        /// <summary>
        /// Personalización de presentación
        /// </summary>
        Presentacion = 5,
        
        /// <summary>
        /// Personalización de acompañamientos
        /// </summary>
        Acompañamiento = 6,
        
        /// <summary>
        /// Personalización especial o comentario libre
        /// </summary>
        Especial = 7
    }
} 