namespace RestaurantePro.Domain.Operaciones.Results
{
    /// <summary>
    /// Resultado de la verificación de disponibilidad de ingredientes para una comanda.
    /// </summary>
    public class DisponibilidadIngredientesResult
    {
        /// <summary>
        /// Indica si todos los ingredientes necesarios están disponibles.
        /// </summary>
        public bool TodosDisponibles { get; set; }
        
        /// <summary>
        /// Diccionario de ingredientes faltantes. 
        /// Clave: Nombre del ingrediente, Valor: Cantidad faltante.
        /// </summary>
        public Dictionary<string, decimal> IngredientesFaltantes { get; set; } = new();
        
        /// <summary>
        /// Diccionario de productos no disponibles.
        /// Clave: ID del producto, Valor: Razón por la que no está disponible.
        /// </summary>
        public Dictionary<Guid, string> ProductosNoDisponibles { get; set; } = new();
        
        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public DisponibilidadIngredientesResult()
        {
            TodosDisponibles = true;
            IngredientesFaltantes = new Dictionary<string, decimal>();
            ProductosNoDisponibles = new Dictionary<Guid, string>();
        }
    }
} 