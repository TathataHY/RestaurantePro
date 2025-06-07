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
        /// Genera una factura para una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="clienteId">ID del cliente (opcional)</param>
        /// <param name="montoTotal">Monto total a facturar</param>
        /// <param name="observaciones">Observaciones adicionales</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Factura generada</returns>
        Task<Result<Factura>> GenerarFacturaAsync(Guid comandaId, Guid? clienteId, decimal montoTotal, string observaciones, CancellationToken cancellationToken);
        
        /// <summary>
        /// Anula una factura existente
        /// </summary>
        /// <param name="facturaId">ID de la factura a anular</param>
        /// <param name="motivo">Motivo de la anulación</param>
        /// <param name="usuarioId">ID del usuario que realiza la anulación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result<bool>> AnularFacturaAsync(Guid facturaId, string motivo, Guid usuarioId, CancellationToken cancellationToken);
        
        /// <summary>
        /// Genera un reporte de ventas para un período específico
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del período</param>
        /// <param name="fechaFin">Fecha de fin del período</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Reporte de ventas</returns>
        Task<Result<object>> GenerarReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken);
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