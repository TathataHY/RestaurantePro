using System;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de fidelización de clientes
    /// </summary>
    public interface IFidelizacionService
    {
        /// <summary>
        /// Acumula puntos para un cliente basado en una compra
        /// </summary>
        /// <param name="request">Datos de la solicitud para acumular puntos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de puntos acumulados</returns>
        Task<Result<int>> AcumularPuntosAsync(AcumularPuntosRequest request, CancellationToken cancellationToken);
        
        /// <summary>
        /// Obtiene el saldo de puntos actual de un cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Saldo de puntos actual</returns>
        Task<Result<int>> ObtenerSaldoPuntosAsync(Guid clienteId, CancellationToken cancellationToken);
        
        /// <summary>
        /// Canjea puntos por una recompensa
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="recompensaId">ID de la recompensa a canjear</param>
        /// <param name="cantidadPuntos">Cantidad de puntos a canjear</param>
        /// <param name="usuarioId">ID del usuario que realiza la operación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado del canje</returns>
        Task<Result<bool>> CanjearPuntosAsync(Guid clienteId, Guid recompensaId, int cantidadPuntos, Guid usuarioId, CancellationToken cancellationToken);
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