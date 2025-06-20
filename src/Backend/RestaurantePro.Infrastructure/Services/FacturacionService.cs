using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Services;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Infrastructure.Services
{
    /// <summary>
    /// Implementación de IFacturacionService que actúa como adapter del servicio de dominio
    /// </summary>
    public class FacturacionService : IFacturacionService
    {
        private readonly IServicioFacturacion _servicioFacturacion;
        private readonly IFacturaRepository _facturaRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<FacturacionService> _logger;

        public FacturacionService(
            IServicioFacturacion servicioFacturacion,
            IFacturaRepository facturaRepository,
            IDateTimeService dateTimeService,
            ILogger<FacturacionService> logger)
        {
            _servicioFacturacion = servicioFacturacion ?? throw new ArgumentNullException(nameof(servicioFacturacion));
            _facturaRepository = facturaRepository ?? throw new ArgumentNullException(nameof(facturaRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Genera una factura para una comanda
        /// </summary>
        public async Task<Result<Factura>> GenerarFacturaAsync(
            Guid comandaId, 
            Guid? clienteId, 
            decimal montoTotal, 
            string observaciones, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Generando factura para comanda {ComandaId}", comandaId);

                // Usar valores por defecto para el adapter
                var tipoFactura = TipoFactura.Normal; // Tipo por defecto
                var nombreCliente = clienteId.HasValue ? $"Cliente {clienteId}" : "Cliente Genérico";

                return await _servicioFacturacion.GenerarFacturaParaComandaAsync(
                    comandaId,
                    tipoFactura,
                    nombreCliente,
                    clienteId,
                    null, // identificacionFiscal
                    null, // direccionCliente
                    observaciones,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando factura para comanda {ComandaId}", comandaId);
                return Result.Failure<Factura>($"Error generando factura: {ex.Message}");
            }
        }

        /// <summary>
        /// Anula una factura existente
        /// </summary>
        public async Task<Result<bool>> AnularFacturaAsync(
            Guid facturaId, 
            string motivo, 
            Guid usuarioId, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Anulando factura {FacturaId} por usuario {UsuarioId}", facturaId, usuarioId);

                var resultado = await _servicioFacturacion.AnularFacturaAsync(facturaId, motivo, cancellationToken);
                
                if (resultado.Succeeded)
                {
                    return Result.Success(true);
                }
                
                return Result.Failure<bool>(resultado.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anulando factura {FacturaId}", facturaId);
                return Result.Failure<bool>($"Error anulando factura: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera un reporte de ventas para un período específico
        /// </summary>
        public async Task<Result<object>> GenerarReporteVentasAsync(
            DateTime fechaInicio, 
            DateTime fechaFin, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Generando reporte de ventas desde {FechaInicio} hasta {FechaFin}", 
                    fechaInicio, fechaFin);

                // Obtener facturas en el período
                var facturas = await _facturaRepository.ObtenerPorRangoFechasAsync(
                    fechaInicio, fechaFin, cancellationToken);

                var reporte = new
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,
                    TotalFacturas = facturas.Count(),
                    MontoTotal = facturas.Sum(f => f.Total),
                    FacturasPagadas = facturas.Count(f => f.Estado == EstadoFactura.Pagada),
                    FacturasPendientes = facturas.Count(f => f.Estado == EstadoFactura.Emitida),
                    FacturasAnuladas = facturas.Count(f => f.Estado == EstadoFactura.Anulada),
                    PromedioVenta = facturas.Any() ? facturas.Average(f => f.Total) : 0,
                    FechaGeneracion = _dateTimeService.Now
                };

                return Result.Success<object>(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte de ventas");
                return Result.Failure<object>($"Error generando reporte: {ex.Message}");
            }
        }
    }
} 