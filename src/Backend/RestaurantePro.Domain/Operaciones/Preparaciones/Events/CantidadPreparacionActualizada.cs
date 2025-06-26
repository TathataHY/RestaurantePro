using RestaurantePro.Domain.Core.Base;

namespace RestaurantePro.Domain.Operaciones.Preparaciones.Events
{
    /// <summary>
    /// Evento que se dispara cuando se actualiza la cantidad de una preparación
    /// </summary>
    public class CantidadPreparacionActualizada : DomainEvent
    {
        public Guid PreparacionId { get; }
        public Guid ProductoId { get; }
        public int CantidadAnterior { get; }
        public int CantidadNueva { get; }
        public int CantidadDisponible { get; }

        public CantidadPreparacionActualizada(
            Guid preparacionId, 
            Guid productoId, 
            int cantidadAnterior, 
            int cantidadNueva, 
            int cantidadDisponible)
        {
            PreparacionId = preparacionId;
            ProductoId = productoId;
            CantidadAnterior = cantidadAnterior;
            CantidadNueva = cantidadNueva;
            CantidadDisponible = cantidadDisponible;
        }
    }
} 