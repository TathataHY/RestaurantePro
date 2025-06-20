using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Comercial.Fidelizacion.Interfaces;
using RestaurantePro.Domain.Comercial.Services;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Infrastructure.Services
{
    /// <summary>
    /// Implementación de IFidelizacionService que actúa como adapter del servicio de dominio
    /// </summary>
    public class FidelizacionService : IFidelizacionService
    {
        private readonly IServicioFidelizacion _servicioFidelizacion;
        private readonly ILogger<FidelizacionService> _logger;

        public FidelizacionService(
            IServicioFidelizacion servicioFidelizacion,
            ILogger<FidelizacionService> logger)
        {
            _servicioFidelizacion = servicioFidelizacion ?? throw new ArgumentNullException(nameof(servicioFidelizacion));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Acumula puntos para un cliente basado en una compra
        /// </summary>
        public async Task<Result<int>> AcumularPuntosAsync(AcumularPuntosRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null)
                    return Result.Failure<int>("La solicitud no puede ser nula");

                // Usar el servicio de dominio para acumular puntos
                var resultado = await _servicioFidelizacion.AcumularPuntosAsync(
                    request.ClienteId, 
                    request.FacturaId, 
                    request.MontoFactura);

                if (resultado.Succeeded)
                {
                    _logger.LogInformation("✅ Puntos acumulados exitosamente para cliente {ClienteId}: {Puntos}", 
                        request.ClienteId, resultado.Value);
                }
                else
                {
                    _logger.LogWarning("⚠️ Error al acumular puntos para cliente {ClienteId}: {Error}", 
                        request.ClienteId, resultado.Error);
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado al acumular puntos para cliente {ClienteId}", request.ClienteId);
                return Result.Failure<int>($"Error interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el saldo de puntos actual de un cliente
        /// </summary>
        public async Task<Result<int>> ObtenerSaldoPuntosAsync(Guid clienteId, CancellationToken cancellationToken)
        {
            try
            {
                if (clienteId == Guid.Empty)
                    return Result.Failure<int>("El ID del cliente no puede estar vacío");

                // Por ahora retornamos 0 hasta implementar completamente
                // TODO: Implementar cuando tengamos el método en el dominio
                _logger.LogInformation("📊 Obteniendo saldo de puntos para cliente {ClienteId}", clienteId);
                
                return Result.Success(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener saldo de puntos para cliente {ClienteId}", clienteId);
                return Result.Failure<int>($"Error interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Canjea puntos por una recompensa
        /// </summary>
        public async Task<Result<bool>> CanjearPuntosAsync(Guid clienteId, Guid recompensaId, int cantidadPuntos, Guid usuarioId, CancellationToken cancellationToken)
        {
            try
            {
                if (clienteId == Guid.Empty)
                    return Result.Failure<bool>("El ID del cliente no puede estar vacío");

                if (recompensaId == Guid.Empty)
                    return Result.Failure<bool>("El ID de la recompensa no puede estar vacío");

                if (cantidadPuntos <= 0)
                    return Result.Failure<bool>("La cantidad de puntos debe ser mayor a cero");

                // Usar el servicio de dominio para canjear puntos
                var resultado = await _servicioFidelizacion.CanjearPuntosAsync(
                    clienteId, 
                    cantidadPuntos, 
                    $"Canje por recompensa {recompensaId}");

                if (resultado.Succeeded)
                {
                    _logger.LogInformation("✅ Puntos canjeados exitosamente - Cliente: {ClienteId}, Puntos: {Puntos}, Recompensa: {RecompensaId}", 
                        clienteId, cantidadPuntos, recompensaId);
                    return Result.Success(true);
                }
                else
                {
                    _logger.LogWarning("⚠️ Error al canjear puntos - Cliente: {ClienteId}, Error: {Error}", 
                        clienteId, resultado.Error);
                    return Result.Failure<bool>(resultado.Error ?? "Error al canjear puntos");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado al canjear puntos para cliente {ClienteId}", clienteId);
                return Result.Failure<bool>($"Error interno: {ex.Message}");
            }
        }
    }
} 