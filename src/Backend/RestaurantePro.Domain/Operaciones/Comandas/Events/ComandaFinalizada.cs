using System;
using RestaurantePro.Domain.Core.Base;

namespace RestaurantePro.Domain.Operaciones.Comandas.Events
{
    /// <summary>
    /// Evento emitido cuando una comanda se finaliza (pagada)
    /// </summary>
    public class ComandaFinalizada : IDomainEvent
    {
        /// <summary>
        /// ID de la comanda
        /// </summary>
        public Guid ComandaId { get; }
        
        /// <summary>
        /// Total final pagado
        /// </summary>
        public decimal TotalPagado { get; }
        
        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ComandaFinalizada(Guid comandaId, decimal totalPagado)
        {
            ComandaId = comandaId;
            TotalPagado = totalPagado;
            OccurredOn = DateTime.Now;
        }
    }
} 