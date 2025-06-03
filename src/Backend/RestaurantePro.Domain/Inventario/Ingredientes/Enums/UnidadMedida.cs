namespace RestaurantePro.Domain.Inventario.Ingredientes.Enums
{
    /// <summary>
    /// Unidades de medida para ingredientes
    /// </summary>
    public enum UnidadMedida
    {
        /// <summary>
        /// Unidad individual (pieza, unidad)
        /// </summary>
        Unidad = 0,
        
        /// <summary>
        /// Kilogramo
        /// </summary>
        Kilogramo = 1,
        
        /// <summary>
        /// Gramo
        /// </summary>
        Gramo = 2,
        
        /// <summary>
        /// Litro
        /// </summary>
        Litro = 3,
        
        /// <summary>
        /// Mililitro
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
        Paquete = 8,
        
        /// <summary>
        /// Piezas (unidades individuales)
        /// </summary>
        Piezas = 9,
        
        // Alias en plural para compatibilidad con tests
        /// <summary>
        /// Alias en plural para Kilogramo
        /// </summary>
        Kilogramos = Kilogramo,
        
        /// <summary>
        /// Alias en plural para Gramo
        /// </summary>
        Gramos = Gramo,
        
        /// <summary>
        /// Alias en plural para Litro
        /// </summary>
        Litros = Litro,
        
        /// <summary>
        /// Alias en plural para Mililitro
        /// </summary>
        Mililitros = Mililitro,
        
        /// <summary>
        /// Alias en plural para Unidad
        /// </summary>
        Unidades = Unidad
    }
}
