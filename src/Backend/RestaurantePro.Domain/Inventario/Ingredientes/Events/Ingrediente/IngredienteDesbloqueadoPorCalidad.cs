namespace RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente
{
    /// <summary>
    /// Evento que se genera cuando un ingrediente es desbloqueado por control de calidad.
    /// </summary>
    public class IngredienteDesbloqueadoPorCalidad : DomainEvent
    {
        /// <summary>
        /// ID del ingrediente desbloqueado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Motivo del desbloqueo
        /// </summary>
        public string Motivo { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="motivo">Motivo del desbloqueo</param>
        public IngredienteDesbloqueadoPorCalidad(Guid ingredienteId, string nombre, string motivo) : base()
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            Motivo = motivo;
        }
    }
} 
