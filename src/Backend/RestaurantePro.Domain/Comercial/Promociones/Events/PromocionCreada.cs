using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using System;

namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se crea una promoción
    /// </summary>
    public class PromocionCreada : DomainEvent
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
        /// Tipo de promoción
        /// </summary>
        public TipoPromocion Tipo { get; }
        
        /// <summary>
        /// Valor del descuento (monto fijo o porcentaje según el tipo)
        /// </summary>
        public decimal ValorDescuento { get; }
        
        /// <summary>
        /// Fecha de inicio de la promoción
        /// </summary>
        public DateTime FechaInicio { get; }
        
        /// <summary>
        /// Fecha de fin de la promoción
        /// </summary>
        public DateTime FechaFin { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionCreada
        /// </summary>
        public PromocionCreada(
            Guid promocionId, 
            string codigo, 
            string nombre, 
            TipoPromocion tipo, 
            decimal valorDescuento, 
            DateTime fechaInicio, 
            DateTime fechaFin)
        {
            PromocionId = promocionId;
            Codigo = codigo;
            Nombre = nombre;
            Tipo = tipo;
            ValorDescuento = valorDescuento;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
        }
    }
} 