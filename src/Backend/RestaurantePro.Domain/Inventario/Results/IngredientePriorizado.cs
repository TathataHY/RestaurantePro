namespace RestaurantePro.Domain.Inventario.Results
{
    /// <summary>
    /// Clase que representa un ingrediente priorizado para reposición
    /// </summary>
    public class IngredientePriorizado
    {
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Prioridad calculada (mayor número = mayor prioridad)
        /// </summary>
        public int Prioridad { get; }
        
        /// <summary>
        /// Nivel de rotación
        /// </summary>
        public RotacionIngrediente Rotacion { get; }
        
        /// <summary>
        /// Temporada
        /// </summary>
        public TemporadaIngrediente Temporada { get; }
        
        /// <summary>
        /// Stock actual
        /// </summary>
        public decimal Stock { get; }
        
        /// <summary>
        /// Stock mínimo
        /// </summary>
        public decimal StockMinimo { get; }
        
        /// <summary>
        /// Porcentaje de stock (actual / mínimo)
        /// </summary>
        public decimal PorcentajeStock => Stock / (StockMinimo > 0 ? StockMinimo : 1) * 100;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public IngredientePriorizado(
            Guid ingredienteId, 
            string nombre, 
            int prioridad, 
            RotacionIngrediente rotacion, 
            TemporadaIngrediente temporada,
            decimal stock,
            decimal stockMinimo)
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            Prioridad = prioridad;
            Rotacion = rotacion;
            Temporada = temporada;
            Stock = stock;
            StockMinimo = stockMinimo;
        }
    }
} 