using System.Linq.Expressions;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Specifications;

namespace RestaurantePro.Domain.Comercial.Facturacion.Specifications
{
    /// <summary>
    /// Especificación para identificar facturas vencidas
    /// </summary>
    public class FacturaVencidaSpecification : Specification<Factura>
    {
        private readonly DateTime _fechaReferencia;

        /// <summary>
        /// Constructor con fecha de referencia para determinar vencimiento
        /// </summary>
        /// <param name="fechaReferencia">Fecha de referencia (generalmente la fecha actual)</param>
        public FacturaVencidaSpecification(DateTime fechaReferencia)
        {
            _fechaReferencia = fechaReferencia;
        }

        /// <summary>
        /// Constructor que utiliza la fecha actual como referencia
        /// </summary>
        public FacturaVencidaSpecification() : this(DateTime.Now)
        {
        }

        /// <summary>
        /// Convierte la especificación a una expresión LINQ
        /// </summary>
        /// <returns>Expresión LINQ que representa la especificación</returns>
        public override Expression<Func<Factura, bool>> ToExpression()
        {
            return factura => 
                // Facturas emitidas o pagadas parcialmente
                (factura.Estado == EstadoFactura.Emitida || factura.Estado == EstadoFactura.PagadaParcialmente) &&
                // No anuladas
                factura.Estado != EstadoFactura.Anulada &&
                // Con fecha de vencimiento establecida
                factura.FechaVencimiento.HasValue &&
                // Fecha de vencimiento menor a la fecha de referencia
                factura.FechaVencimiento.Value < _fechaReferencia &&
                // Con saldo pendiente
                (factura.Total - factura.TotalPagado) > 0;
        }
    }
} 