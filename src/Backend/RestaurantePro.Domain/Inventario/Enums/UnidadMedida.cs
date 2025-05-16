namespace RestaurantePro.Domain.Inventario.Enums
{
    /// <summary>
    /// Unidades de medida para ingredientes y productos
    /// </summary>
    public enum UnidadMedida
    {
        /// <summary>
        /// Unidad individual (ej. huevos, limones)
        /// </summary>
        Unidad = 0,
        
        /// <summary>
        /// Kilogramos
        /// </summary>
        Kilogramo = 1,
        
        /// <summary>
        /// Gramos
        /// </summary>
        Gramo = 2,
        
        /// <summary>
        /// Litros
        /// </summary>
        Litro = 3,
        
        /// <summary>
        /// Mililitros
        /// </summary>
        Mililitro = 4,
        
        /// <summary>
        /// Cucharadas
        /// </summary>
        Cucharada = 5,
        
        /// <summary>
        /// Cucharaditas
        /// </summary>
        Cucharadita = 6,
        
        /// <summary>
        /// Tazas
        /// </summary>
        Taza = 7,
        
        /// <summary>
        /// Paquete (cantidad predefinida)
        /// </summary>
        Paquete = 8
    }
}
