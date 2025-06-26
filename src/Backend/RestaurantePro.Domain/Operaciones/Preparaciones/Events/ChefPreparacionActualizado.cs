using RestaurantePro.Domain.Core.Base;

namespace RestaurantePro.Domain.Operaciones.Preparaciones.Events
{
    /// <summary>
    /// Evento que se dispara cuando se actualiza el chef responsable de una preparación
    /// </summary>
    public class ChefPreparacionActualizado : DomainEvent
    {
        public Guid PreparacionId { get; }
        public Guid ProductoId { get; }
        public Guid ChefAnterior { get; }
        public Guid ChefNuevo { get; }

        public ChefPreparacionActualizado(
            Guid preparacionId, 
            Guid productoId, 
            Guid chefAnterior, 
            Guid chefNuevo)
        {
            PreparacionId = preparacionId;
            ProductoId = productoId;
            ChefAnterior = chefAnterior;
            ChefNuevo = chefNuevo;
        }
    }
} 