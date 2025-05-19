namespace RestaurantePro.Domain.Inventario.Ingredientes.Events
{
    /// <summary>
    /// Evento que se genera cuando un ingrediente es bloqueado por control de calidad.
    /// </summary>
    public class IngredienteBloqueadoPorCalidad : DomainEvent
    {
        /// <summary>
        /// ID del ingrediente bloqueado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Motivo del bloqueo
        /// </summary>
        public string Motivo { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="motivo">Motivo del bloqueo</param>
        public IngredienteBloqueadoPorCalidad(Guid ingredienteId, string nombre, string motivo) : base()
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            Motivo = motivo;
        }
    }
} 