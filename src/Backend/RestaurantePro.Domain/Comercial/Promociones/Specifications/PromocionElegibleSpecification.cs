using RestaurantePro.Domain.Core.SharedKernel.Specifications;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using System;
using System.Linq.Expressions;

namespace RestaurantePro.Domain.Comercial.Promociones.Specifications
{
    /// <summary>
    /// Especificación compuesta para filtrar promociones elegibles para ser aplicadas
    /// en un momento determinado (activas y válidas para el día)
    /// </summary>
    public class PromocionElegibleSpecification : Specification<Promocion>
    {
        private readonly Specification<Promocion> _especificacionCombinada;
        
        /// <summary>
        /// Constructor de la especificación
        /// </summary>
        /// <param name="fecha">Fecha para verificar si la promoción está activa</param>
        /// <param name="diaSemana">Día de la semana específico (opcional, por defecto se usa el día de la fecha)</param>
        public PromocionElegibleSpecification(DateTime? fecha = null, DayOfWeek? diaSemana = null)
        {
            DateTime fechaEfectiva = fecha ?? DateTime.Now;
            DayOfWeek diaEfectivo = diaSemana ?? fechaEfectiva.DayOfWeek;
            
            // Combinar especificaciones
            var promocionActiva = new PromocionActivaSpecification(fechaEfectiva);
            var promocionDiaSemana = new PromocionDiaSemanaSpecification(diaEfectivo);
            
            // Esta especificación combina ambas condiciones
            _especificacionCombinada = promocionActiva.And(promocionDiaSemana);
        }
        
        /// <summary>
        /// Retorna la expresión combinada
        /// </summary>
        public override Expression<Func<Promocion, bool>> ToExpression()
        {
            return _especificacionCombinada.ToExpression();
        }
    }
} 