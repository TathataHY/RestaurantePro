namespace RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente
{
    /// <summary>
    /// Evento que se genera cuando se actualiza la temporada de un ingrediente.
    /// </summary>
    public class TemporadaIngredienteActualizada : DomainEvent
    {
        /// <summary>
        /// ID del ingrediente cuya temporada ha sido actualizada
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Nueva temporada asignada
        /// </summary>
        public TemporadaIngrediente Temporada { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="temporada">Nueva temporada</param>
        public TemporadaIngredienteActualizada(Guid ingredienteId, string nombre, TemporadaIngrediente temporada) : base()
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            Temporada = temporada;
        }
    }
} 
