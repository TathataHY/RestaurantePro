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
        Especial = 7,
        
        /// <summary>
        /// Agregar ingrediente adicional
        /// </summary>
        AgregarIngrediente = 8,
        
        /// <summary>
        /// Quitar ingrediente específico
        /// </summary>
        QuitarIngrediente = 9,
        
        /// <summary>
        /// Cambiar un ingrediente por otro
        /// </summary>
        CambiarIngrediente = 10,
        
        /// <summary>
        /// Instrucción especial para el personal de cocina
        /// </summary>
        InstruccionEspecial = 11
    }
} 