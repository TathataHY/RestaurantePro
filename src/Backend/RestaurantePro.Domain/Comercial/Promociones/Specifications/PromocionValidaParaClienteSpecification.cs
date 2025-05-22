using RestaurantePro.Domain.Core.SharedKernel.Specifications;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using System;
using System.Linq.Expressions;

namespace RestaurantePro.Domain.Comercial.Promociones.Specifications
{
    /// <summary>
    /// Especificación para validar si una promoción es aplicable a un cliente específico
    /// </summary>
    public class PromocionValidaParaClienteSpecification : Specification<Promocion>
    {
        private readonly Guid _clienteId;
        private readonly int _puntosDisponibles;
        private readonly decimal _montoTotal;
        private readonly DateTime _fechaActual;
        
        /// <summary>
        /// Constructor de la especificación
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="puntosDisponibles">Puntos disponibles del cliente</param>
        /// <param name="montoTotal">Monto total de la compra</param>
        /// <param name="fechaActual">Fecha actual (inyectable para pruebas)</param>
        public PromocionValidaParaClienteSpecification(
            Guid clienteId, 
            int puntosDisponibles, 
            decimal montoTotal, 
            DateTime? fechaActual = null)
        {
            _clienteId = clienteId;
            _puntosDisponibles = puntosDisponibles;
            _montoTotal = montoTotal;
            _fechaActual = fechaActual ?? DateTime.Now;
        }
        
        /// <summary>
        /// Retorna la expresión que verifica si una promoción es válida para un cliente
        /// </summary>
        public override Expression<Func<Promocion, bool>> ToExpression()
        {
            return promocion => 
                // La promoción debe estar activa
                promocion.Estado == EstadoPromocion.Activa && 
                
                // Debe estar dentro del periodo de vigencia
                promocion.FechaInicio <= _fechaActual && 
                promocion.FechaFin >= _fechaActual &&
                
                // No debe haber alcanzado el máximo de usos
                (!promocion.MaximoUsos.HasValue || promocion.VecesUsada < promocion.MaximoUsos.Value) &&
                
                // El monto de la compra debe ser mayor o igual al monto mínimo
                _montoTotal >= promocion.MontoMinimo &&
                
                // Si es una promoción de canje de puntos, el cliente debe tener suficientes puntos
                (promocion.Tipo != TipoPromocion.CanjePuntos || _puntosDisponibles >= promocion.PuntosRequeridos);
        }
    }
} 