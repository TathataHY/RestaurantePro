using Microsoft.Extensions.Logging;

namespace RestaurantePro.Domain.Inventario.Services;

/// <summary>
/// Implementación del servicio de dominio para gestión de alertas de stock
/// </summary>
public class AlertaStockService : IAlertaStockService
{
    private readonly ILogger<AlertaStockService> _logger;

    public AlertaStockService(ILogger<AlertaStockService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<bool> VerificarAlertaStockAsync(
        Guid ingredienteId,
        decimal stockActual,
        decimal stockMinimo)
    {
        try
        {
            if (ingredienteId == Guid.Empty) return false;
            if (stockMinimo < 0) return false;
            if (stockActual < 0) return false;

            // Si el stock actual está por debajo o igual al mínimo, hay alerta
            return stockActual <= stockMinimo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar alerta de stock para ingrediente {IngredienteId}", ingredienteId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<InfoAlertaStock?> EvaluarNecesidadAlertaAsync(
        Guid ingredienteId,
        decimal stockActual,
        decimal stockMinimo,
        decimal? stockCritico = null)
    {
        try
        {
            if (ingredienteId == Guid.Empty) return null;
            if (stockMinimo < 0 || stockActual < 0) return null;

            var stockCrit = stockCritico ?? stockMinimo * 0.5m; // Si no se especifica, es 50% del mínimo

            var tipoAlerta = DeterminarTipoAlerta(stockActual, stockMinimo, stockCrit);
            
            // Si no hay problema con el stock, no generar alerta
            if (tipoAlerta == TipoAlertaStock.StockExcesivo && stockActual <= stockMinimo * 10) // Límite arbitrario para "excesivo"
                return null;

            if (stockActual > stockMinimo && tipoAlerta != TipoAlertaStock.StockExcesivo)
                return null;

            var nivelPrioridad = CalcularNivelPrioridadAlerta(stockActual, stockMinimo, stockCrit);
            var mensaje = GenerarMensajeAlerta(tipoAlerta, stockActual, stockMinimo, stockCrit);

            return new InfoAlertaStock
            {
                IngredienteId = ingredienteId,
                TipoAlerta = tipoAlerta,
                NivelPrioridad = nivelPrioridad,
                Mensaje = mensaje,
                StockActual = stockActual,
                StockMinimo = stockMinimo,
                StockCritico = stockCrit,
                RequiereAccionInmediata = nivelPrioridad >= NivelPrioridadAlerta.Alta
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al evaluar necesidad de alerta para ingrediente {IngredienteId}", ingredienteId);
            return null;
        }
    }

    /// <inheritdoc />
    public NivelPrioridadAlerta CalcularNivelPrioridadAlerta(
        decimal stockActual,
        decimal stockMinimo,
        decimal stockCritico)
    {
        try
        {
            // Stock agotado = Crítica
            if (stockActual <= 0)
                return NivelPrioridadAlerta.Critica;

            // Por debajo del stock crítico = Crítica
            if (stockActual <= stockCritico)
                return NivelPrioridadAlerta.Critica;

            // Por debajo del stock mínimo = Alta
            if (stockActual <= stockMinimo)
                return NivelPrioridadAlerta.Alta;

            // Ligeramente por encima del mínimo (hasta 20% más) = Media
            if (stockActual <= stockMinimo * 1.2m)
                return NivelPrioridadAlerta.Media;

            // Stock excesivo (más de 10 veces el mínimo) = Media
            if (stockActual > stockMinimo * 10)
                return NivelPrioridadAlerta.Media;

            // En otros casos = Baja
            return NivelPrioridadAlerta.Baja;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular nivel de prioridad de alerta");
            return NivelPrioridadAlerta.Baja;
        }
    }

    /// <inheritdoc />
    public TipoAlertaStock DeterminarTipoAlerta(
        decimal stockActual,
        decimal stockMinimo,
        decimal stockCritico)
    {
        try
        {
            // Stock agotado
            if (stockActual <= 0)
                return TipoAlertaStock.StockAgotado;

            // Stock crítico
            if (stockActual <= stockCritico)
                return TipoAlertaStock.StockCritico;

            // Stock bajo
            if (stockActual <= stockMinimo)
                return TipoAlertaStock.StockBajo;

            // Stock excesivo (más de 10 veces el mínimo)
            if (stockActual > stockMinimo * 10)
                return TipoAlertaStock.StockExcesivo;

            // Por defecto, consideramos stock bajo si estamos cerca del límite
            return TipoAlertaStock.StockBajo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al determinar tipo de alerta");
            return TipoAlertaStock.StockBajo;
        }
    }

    /// <summary>
    /// Genera un mensaje descriptivo para la alerta
    /// </summary>
    private string GenerarMensajeAlerta(TipoAlertaStock tipoAlerta, decimal stockActual, decimal stockMinimo, decimal stockCritico)
    {
        return tipoAlerta switch
        {
            TipoAlertaStock.StockAgotado => $"¡STOCK AGOTADO! No hay existencias disponibles. Stock mínimo requerido: {stockMinimo}",
            TipoAlertaStock.StockCritico => $"Stock crítico: {stockActual} unidades (debajo de {stockCritico}). Reabastecer urgentemente.",
            TipoAlertaStock.StockBajo => $"Stock bajo: {stockActual} unidades (por debajo del mínimo {stockMinimo}). Considerar reabastecimiento.",
            TipoAlertaStock.StockExcesivo => $"Stock excesivo: {stockActual} unidades (muy por encima del mínimo {stockMinimo}). Revisar almacenamiento.",
            _ => $"Alerta de stock: {stockActual} unidades disponibles."
        };
    }
} 