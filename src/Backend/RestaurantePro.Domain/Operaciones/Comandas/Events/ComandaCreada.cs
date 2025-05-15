using System;
using RestaurantePro.Domain.Core.Base.Interfaces;

namespace RestaurantePro.Domain.Operaciones.Comandas.Events
{
    /// <summary>
    /// Evento emitido cuando se crea una nueva comanda
    /// </summary>
    public class ComandaCreada : IDomainEvent
    {
        /// <summary>
        /// ID de la comanda creada
        /// </summary>
        public Guid ComandaId { get; }
        
        /// <summary>
        /// ID de la mesa asociada
        /// </summary>
        public Guid MesaId { get; }
        
        /// <summary>
        /// ID del mesero que creó la comanda
        /// </summary>
        public Guid MeseroId { get; }
        
        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ComandaCreada(Guid comandaId, Guid mesaId, Guid meseroId)
        {
            ComandaId = comandaId;
            MesaId = mesaId;
            MeseroId = meseroId;
            OccurredOn = DateTime.Now;
        }
    }
} 