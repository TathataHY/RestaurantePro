namespace RestaurantePro.Domain.Comercial.Services;

/// <summary>
/// Implementación del servicio de dominio para cálculo de puntos de fidelización
/// </summary>
public class CalculadoraPuntosService : ICalculadoraPuntosService
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly ILogger<CalculadoraPuntosService> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public CalculadoraPuntosService(
        ITarjetaFidelizacionRepository tarjetaRepository,
        ILogger<CalculadoraPuntosService> logger)
    {
        _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calcula los puntos a acumular por una compra
    /// </summary>
            public async Task<Result<CalculoResultadoPuntos>> CalcularPuntosPorCompraAsync(
        Guid tarjetaId, 
        decimal montoCompra, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (montoCompra <= 0)
                return Result.Failure<CalculoResultadoPuntos>("El monto de compra debe ser mayor que cero");

            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(tarjetaId, cancellationToken, false);
            if (tarjeta == null)
                return Result.Failure<CalculoResultadoPuntos>("Tarjeta de fidelización no encontrada");

            if (tarjeta.Estado != EstadoTarjeta.Activa)
                return Result.Failure<CalculoResultadoPuntos>("La tarjeta de fidelización está inactiva");

            // Obtener tasa de conversión según el nivel de la tarjeta
            var tasaConversion = ObtenerTasaConversionPorNivel(tarjeta.NivelFidelizacion);
            
            // Calcular puntos base
            var puntosBase = (int)Math.Floor(montoCompra * tasaConversion);
            
            // Calcular bonificación por nivel
            var bonificacion = CalcularBonificacionPorNivel(tarjeta.NivelFidelizacion, puntosBase);
            
            var detalleCalculo = $"Monto: ${montoCompra:F2} x Tasa: {tasaConversion:F4} = {puntosBase} puntos base + {bonificacion} bonificación";

            var resultado = new CalculoResultadoPuntos(puntosBase, tasaConversion, bonificacion, detalleCalculo);

            _logger.LogInformation("Puntos calculados para tarjeta {TarjetaId}: {TotalPuntos} puntos", 
                tarjetaId, resultado.TotalPuntos);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando puntos para tarjeta {TarjetaId}", tarjetaId);
            return Result.Failure<CalculoResultadoPuntos>($"Error calculando puntos: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula los puntos por una promoción específica
    /// </summary>
            public async Task<Result<CalculoResultadoPuntos>> CalcularPuntosPorPromocionAsync(
        Guid tarjetaId, 
        Guid promocionId, 
        Dictionary<string, object>? parametros = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(tarjetaId, cancellationToken, false);
            if (tarjeta == null)
                return Result.Failure<CalculoResultadoPuntos>("Tarjeta de fidelización no encontrada");

            // TODO: Implementar lógica específica de promociones
            // Por ahora retornamos puntos fijos como ejemplo
            var puntosPromocion = 100; // Esto debería venir de la configuración de la promoción
            var detalleCalculo = $"Promoción {promocionId}: {puntosPromocion} puntos";

            var resultado = new CalculoResultadoPuntos(puntosPromocion, 0, 0, detalleCalculo);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando puntos por promoción {PromocionId} para tarjeta {TarjetaId}", 
                promocionId, tarjetaId);
            return Result.Failure<CalculoResultadoPuntos>($"Error calculando puntos por promoción: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula el valor en dinero de una cantidad de puntos
    /// </summary>
            public async Task<Result<decimal>> CalcularValorPuntosAsync(
        Guid tarjetaId, 
        int puntos, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (puntos <= 0)
                return Result.Failure<decimal>("La cantidad de puntos debe ser mayor a cero");

            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(tarjetaId, cancellationToken, false);
            if (tarjeta == null)
                return Result.Failure<decimal>("Tarjeta de fidelización no encontrada");

            // Valor por punto según el nivel (ejemplo: 1 punto = $0.50 para nivel básico)
            var valorPorPunto = ObtenerValorPorPuntoPorNivel(tarjeta.NivelFidelizacion);
            var valorTotal = puntos * valorPorPunto;

            return Result.Success(valorTotal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando valor de puntos para tarjeta {TarjetaId}", tarjetaId);
            return Result.Failure<decimal>($"Error calculando valor de puntos: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene la tasa de conversión actual para una tarjeta
    /// </summary>
            public async Task<Result<decimal>> ObtenerTasaConversionAsync(
        Guid tarjetaId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(tarjetaId, cancellationToken, false);
            if (tarjeta == null)
                return Result.Failure<decimal>("Tarjeta de fidelización no encontrada");

            var tasa = ObtenerTasaConversionPorNivel(tarjeta.NivelFidelizacion);
            return Result.Success(tasa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo tasa de conversión para tarjeta {TarjetaId}", tarjetaId);
            return Result.Failure<decimal>($"Error obteniendo tasa de conversión: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida si se pueden canjear una cantidad específica de puntos
    /// </summary>
            public async Task<Result<bool>> ValidarCanjePuntosAsync(
        Guid tarjetaId, 
        int puntos, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (puntos <= 0)
                return Result.Failure<bool>("La cantidad de puntos debe ser mayor a cero");

            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(tarjetaId, cancellationToken, false);
            if (tarjeta == null)
                return Result.Failure<bool>("Tarjeta de fidelización no encontrada");

            if (tarjeta.Estado != EstadoTarjeta.Activa)
                return Result.Failure<bool>("La tarjeta está inactiva");

            var puedeCanear = tarjeta.PuntosDisponibles >= puntos;
            
            if (!puedeCanear)
                return Result.Failure<bool>($"Puntos insuficientes. Disponibles: {tarjeta.PuntosDisponibles}, Requeridos: {puntos}");

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando canje de puntos para tarjeta {TarjetaId}", tarjetaId);
            return Result.Failure<bool>($"Error validando canje: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula los puntos de bonificación por nivel de cliente
    /// </summary>
            public async Task<Result<int>> CalcularBonificacionPorNivelAsync(
        Guid tarjetaId, 
        int puntosBase, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tarjeta = await _tarjetaRepository.ObtenerPorIdAsync(tarjetaId, cancellationToken, false);
            if (tarjeta == null)
                return Result.Failure<int>("Tarjeta de fidelización no encontrada");

            var bonificacion = CalcularBonificacionPorNivel(tarjeta.NivelFidelizacion, puntosBase);
            return Result.Success(bonificacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando bonificación para tarjeta {TarjetaId}", tarjetaId);
            return Result.Failure<int>($"Error calculando bonificación: {ex.Message}");
        }
    }

    #region Métodos Privados de Lógica de Negocio

    /// <summary>
    /// Obtiene la tasa de conversión según el nivel de fidelización
    /// </summary>
    private decimal ObtenerTasaConversionPorNivel(NivelFidelizacion nivel)
    {
        return nivel switch
        {
            NivelFidelizacion.Basico => 0.01m,   // 1 punto por cada $100
            NivelFidelizacion.Plata => 0.015m,   // 1.5 puntos por cada $100
            NivelFidelizacion.Oro => 0.02m,      // 2 puntos por cada $100
            NivelFidelizacion.Platino => 0.025m, // 2.5 puntos por cada $100
            _ => 0.01m
        };
    }

    /// <summary>
    /// Calcula la bonificación según el nivel
    /// </summary>
    private int CalcularBonificacionPorNivel(NivelFidelizacion nivel, int puntosBase)
    {
        var porcentajeBonificacion = nivel switch
        {
            NivelFidelizacion.Basico => 0.0m,   // Sin bonificación
            NivelFidelizacion.Plata => 0.05m,   // 5% extra
            NivelFidelizacion.Oro => 0.10m,     // 10% extra
            NivelFidelizacion.Platino => 0.15m, // 15% extra
            _ => 0.0m
        };

        return (int)Math.Floor(puntosBase * porcentajeBonificacion);
    }

    /// <summary>
    /// Obtiene el valor por punto según el nivel
    /// </summary>
    private decimal ObtenerValorPorPuntoPorNivel(NivelFidelizacion nivel)
    {
        return nivel switch
        {
            NivelFidelizacion.Basico => 0.50m,   // $0.50 por punto
            NivelFidelizacion.Plata => 0.60m,    // $0.60 por punto
            NivelFidelizacion.Oro => 0.70m,      // $0.70 por punto
            NivelFidelizacion.Platino => 0.80m,  // $0.80 por punto
            _ => 0.50m
        };
    }

    #endregion
} 