using System;
using RestaurantePro.Domain.Core.Base;

namespace RestaurantePro.Domain.Operaciones.Comandas.Events
{
    /// <summary>
    /// Evento emitido cuando una comanda es cancelada
    /// </summary>
    public class ComandaCancelada : IDomainEvent
    {
        /// <summary>
        /// ID de la comanda
        /// </summary>
        public Guid ComandaId { get; }
        
        /// <summary>
        /// Motivo de la cancelación
        /// </summary>
        public string Motivo { get; }
        
        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ComandaCancelada(Guid comandaId, string motivo)
        {
            ComandaId = comandaId;
            Motivo = motivo;
            OccurredOn = DateTime.Now;
        }
    }
} 