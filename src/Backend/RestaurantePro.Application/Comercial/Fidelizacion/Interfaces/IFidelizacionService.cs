using System;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de fidelización de clientes
    /// </summary>
    public interface IFidelizacionService
    {
        /// <summary>
        /// Acumula puntos para un cliente basado en una factura
        /// </summary>
        /// <param name="request">Datos para la acumulación de puntos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con los puntos acumulados</returns>
        Task<Result<int>> AcumularPuntosAsync(AcumularPuntosRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Datos para acumular puntos de fidelización
    /// </summary>
    public class AcumularPuntosRequest
    {
        /// <summary>
        /// ID del cliente
        /// </summary>
        public Guid ClienteId { get; set; }

        /// <summary>
        /// ID de la factura relacionada
        /// </summary>
        public Guid FacturaId { get; set; }

        /// <summary>
        /// Monto total de la factura
        /// </summary>
        public decimal MontoFactura { get; set; }

        /// <summary>
        /// Fecha de la operación
        /// </summary>
        public DateTime FechaOperacion { get; set; }

        /// <summary>
        /// ID del usuario que registra la operación
        /// </summary>
        public Guid UsuarioId { get; set; }
    }
} 