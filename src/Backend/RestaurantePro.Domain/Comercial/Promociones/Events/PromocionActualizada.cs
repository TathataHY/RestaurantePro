using RestaurantePro.Domain.Core.Base.Events;
using System;

namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza una promoción
    /// </summary>
    public class PromocionActualizada : DomainEvent
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
        /// Valor del descuento actualizado
        /// </summary>
        public decimal ValorDescuento { get; }
        
        /// <summary>
        /// Fecha de inicio actualizada
        /// </summary>
        public DateTime FechaInicio { get; }
        
        /// <summary>
        /// Fecha de fin actualizada
        /// </summary>
        public DateTime FechaFin { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionActualizada
        /// </summary>
        public PromocionActualizada(
            Guid promocionId, 
            string codigo, 
            string nombre, 
            decimal valorDescuento, 
            DateTime fechaInicio, 
            DateTime fechaFin)
        {
            PromocionId = promocionId;
            Codigo = codigo;
            Nombre = nombre;
            ValorDescuento = valorDescuento;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
        }
    }
} 