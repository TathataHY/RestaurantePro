namespace RestaurantePro.Domain.Inventario.Ingredientes.Enums
{
    /// <summary>
    /// Representa la temporada o estacionalidad de un ingrediente.
    /// Se utiliza para ajustar las políticas de stock y ordenes de compra
    /// en función de la disponibilidad estacional del ingrediente.
    /// </summary>
    public enum TemporadaIngrediente
    {
        /// <summary>
        /// Ingrediente disponible todo el año sin variaciones importantes
        /// </summary>
        TodoElAño = 0,
        
        /// <summary>
        /// Ingrediente propio de la temporada de primavera (sep-nov en hemisferio sur)
        /// </summary>
        Primavera = 1,
        
        /// <summary>
        /// Ingrediente propio de la temporada de verano (dic-feb en hemisferio sur)
        /// </summary>
        Verano = 2,
        
        /// <summary>
        /// Ingrediente propio de la temporada de otoño (mar-may en hemisferio sur)
        /// </summary>
        Otoño = 3,
        
        /// <summary>
        /// Ingrediente propio de la temporada de invierno (jun-ago en hemisferio sur)
        /// </summary>
        Invierno = 4
    }
} 