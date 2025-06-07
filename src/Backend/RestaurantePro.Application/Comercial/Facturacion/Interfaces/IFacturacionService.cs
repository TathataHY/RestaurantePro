using System;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;

namespace RestaurantePro.Application.Comercial.Facturacion.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de facturación
    /// </summary>
    public interface IFacturacionService
    {
        /// <summary>
        /// Crea una nueva factura
        /// </summary>
        /// <param name="request">Datos para la creación de la factura</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con la factura creada</returns>
        Task<Result<Factura>> CrearFacturaAsync(FacturaRequest request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Datos para la creación de una factura
    /// </summary>
    public class FacturaRequest
    {
        /// <summary>
        /// ID de la comanda asociada a la factura
        /// </summary>
        public Guid ComandaId { get; set; }

        /// <summary>
        /// ID del cliente
        /// </summary>
        public Guid ClienteId { get; set; }

        /// <summary>
        /// ID del usuario que crea la factura
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Observaciones adicionales
        /// </summary>
        public string Observaciones { get; set; } = string.Empty;
    }
} 