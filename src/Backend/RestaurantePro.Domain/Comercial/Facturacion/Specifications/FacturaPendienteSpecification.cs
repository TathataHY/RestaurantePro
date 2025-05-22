using System.Linq.Expressions;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Specifications;

namespace RestaurantePro.Domain.Comercial.Facturacion.Specifications
{
    /// <summary>
    /// Especificación para filtrar facturas pendientes de pago
    /// </summary>
    public class FacturaPendienteSpecification : Specification<Factura>
    {
        /// <summary>
        /// Constructor predeterminado que incluye criterios básicos
        /// </summary>
        public FacturaPendienteSpecification()
        {
        }

        /// <summary>
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        /// <returns>Expresión LINQ que representa la especificación</returns>
        public override Expression<Func<Factura, bool>> ToExpression()
        {
            return factura => 
                // Factura emitida o pagada parcialmente
                (factura.Estado == EstadoFactura.Emitida || factura.Estado == EstadoFactura.PagadaParcialmente) &&
                // No anulada ni rectificativa
                factura.Estado != EstadoFactura.Anulada &&
                factura.Estado != EstadoFactura.Rectificativa &&
                // Con total pendiente mayor a cero
                (factura.Total - factura.TotalPagado) > 0;
        }
    }
} 