using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using System;

namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza el estado de una promoción
    /// </summary>
    public class PromocionEstadoActualizado : DomainEvent
    {
        /// <summary>
        /// ID de la promoción
        /// </summary>
        public Guid PromocionId { get; }
        
        /// <summary>
        /// Código único de la promoción
        /// </summary>
        public string Codigo { get; }
        
        /// <summary>
        /// Nombre de la promoción
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Estado anterior de la promoción
        /// </summary>
        public EstadoPromocion EstadoAnterior { get; }
        
        /// <summary>
        /// Nuevo estado de la promoción
        /// </summary>
        public EstadoPromocion NuevoEstado { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionEstadoActualizado
        /// </summary>
        public PromocionEstadoActualizado(
            Guid promocionId, 
            string codigo, 
            string nombre, 
            EstadoPromocion estadoAnterior, 
            EstadoPromocion nuevoEstado)
        {
            PromocionId = promocionId;
            Codigo = codigo;
            Nombre = nombre;
            EstadoAnterior = estadoAnterior;
            NuevoEstado = nuevoEstado;
        }
    }
} 