namespace RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente
{
    /// <summary>
    /// Evento que se genera cuando se actualiza el estado de bloqueo por control de calidad de un ingrediente.
    /// </summary>
    public class BloqueoControlCalidadActualizado : DomainEvent
    {
        /// <summary>
        /// ID del ingrediente cuyo bloqueo fue actualizado
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Estado del bloqueo (true = bloqueado, false = desbloqueado)
        /// </summary>
        public bool EstaBloqueado { get; }
        
        /// <summary>
        /// Motivo del bloqueo o desbloqueo
        /// </summary>
        public string Motivo { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="estaBloqueado">Estado del bloqueo</param>
        /// <param name="motivo">Motivo del cambio</param>
        public BloqueoControlCalidadActualizado(Guid ingredienteId, string nombre, bool estaBloqueado, string motivo) : base()
        {
            IngredienteId = ingredienteId;
            Nombre = nombre;
            EstaBloqueado = estaBloqueado;
            Motivo = motivo;
        }
    }
} 