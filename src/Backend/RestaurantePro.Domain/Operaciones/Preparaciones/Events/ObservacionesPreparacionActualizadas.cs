using RestaurantePro.Domain.Core.Base;

namespace RestaurantePro.Domain.Operaciones.Preparaciones.Events
{
    /// <summary>
    /// Evento que se dispara cuando se actualizan las observaciones de una preparación
    /// </summary>
    public class ObservacionesPreparacionActualizadas : DomainEvent
    {
        public Guid PreparacionId { get; }
        public Guid ProductoId { get; }
        public string ObservacionesAnteriores { get; }
        public string ObservacionesNuevas { get; }

        public ObservacionesPreparacionActualizadas(
            Guid preparacionId, 
            Guid productoId, 
            string observacionesAnteriores, 
            string observacionesNuevas)
        {
            PreparacionId = preparacionId;
            ProductoId = productoId;
            ObservacionesAnteriores = observacionesAnteriores;
            ObservacionesNuevas = observacionesNuevas;
        }
    }
} 