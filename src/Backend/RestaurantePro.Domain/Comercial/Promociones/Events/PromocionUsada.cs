using RestaurantePro.Domain.Core.Base.Events;
using System;

namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se usa una promoción
    /// </summary>
    public class PromocionUsada : DomainEvent
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
        /// ID del cliente que usó la promoción
        /// </summary>
        public Guid ClienteId { get; }
        
        /// <summary>
        /// ID de la comanda donde se aplicó la promoción
        /// </summary>
        public Guid ComandaId { get; }
        
        /// <summary>
        /// Monto del descuento aplicado
        /// </summary>
        public decimal MontoAplicado { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionUsada
        /// </summary>
        public PromocionUsada(
            Guid promocionId, 
            string codigo, 
            string nombre, 
            Guid clienteId, 
            Guid comandaId, 
            decimal montoAplicado)
        {
            PromocionId = promocionId;
            Codigo = codigo;
            Nombre = nombre;
            ClienteId = clienteId;
            ComandaId = comandaId;
            MontoAplicado = montoAplicado;
        }
    }
} 