using RestaurantePro.Domain.Core.Base.Events;

namespace RestaurantePro.Domain.Operaciones.Preparaciones.Events
{
    /// <summary>
    /// Evento que se dispara cuando se agrega cantidad adicional a una preparación
    /// </summary>
    public class CantidadAgregadaPreparacion : DomainEvent
    {
        /// <summary>
        /// ID de la preparación
        /// </summary>
        public Guid PreparacionId { get; }
        
        /// <summary>
        /// ID del producto
        /// </summary>
        public Guid ProductoId { get; }
        
        /// <summary>
        /// Cantidad agregada
        /// </summary>
        public int CantidadAgregada { get; }
        
        /// <summary>
        /// Cantidad anterior
        /// </summary>
        public int CantidadAnterior { get; }
        
        /// <summary>
        /// Cantidad total después de agregar
        /// </summary>
        public int CantidadTotal { get; }

        /// <summary>
        /// Constructor del evento
        /// </summary>
        public CantidadAgregadaPreparacion(Guid preparacionId, Guid productoId, int cantidadAgregada, int cantidadAnterior, int cantidadTotal)
        {
            PreparacionId = preparacionId;
            ProductoId = productoId;
            CantidadAgregada = cantidadAgregada;
            CantidadAnterior = cantidadAnterior;
            CantidadTotal = cantidadTotal;
        }
    }
} 