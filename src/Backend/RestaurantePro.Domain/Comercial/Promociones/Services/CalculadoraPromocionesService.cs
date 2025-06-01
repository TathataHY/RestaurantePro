namespace RestaurantePro.Domain.Comercial.Promociones.Services;

/// <summary>
/// Implementación del servicio de dominio para calcular promociones aplicables
/// </summary>
public class CalculadoraPromocionesService : ICalculadoraPromocionesService
{
    private readonly IServicioPromociones _servicioPromociones;
    private readonly ILogger<CalculadoraPromocionesService> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public CalculadoraPromocionesService(
        IServicioPromociones servicioPromociones,
        ILogger<CalculadoraPromocionesService> logger)
    {
        _servicioPromociones = servicioPromociones ?? throw new ArgumentNullException(nameof(servicioPromociones));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calcula promociones aplicables para una compra
    /// </summary>
    public async Task<List<PromocionAplicable>> CalcularPromocionesAplicablesAsync(
        ParametrosCompra parametros, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (parametros == null)
                throw new ArgumentNullException(nameof(parametros));

            var promocionesAplicables = new List<PromocionAplicable>();

            // Obtener promociones válidas para el cliente y monto
            var promocionesValidas = await _servicioPromociones.ObtenerPromocionesValidasParaClienteAsync(
                parametros.ClienteId, 
                parametros.MontoTotal, 
                cancellationToken);

            foreach (var promocion in promocionesValidas)
            {
                var promocionAplicable = await EvaluarPromocionAsync(promocion, parametros, cancellationToken);
                if (promocionAplicable != null)
                {
                    promocionesAplicables.Add(promocionAplicable);
                }
            }

            // Obtener promociones por productos
            foreach (var item in parametros.Items)
            {
                var promocionesProducto = await _servicioPromociones.ObtenerPromocionesParaProductoAsync(
                    item.ProductoId, 
                    null, // TODO: Agregar categoriaId si está disponible
                    cancellationToken);

                foreach (var promocion in promocionesProducto)
                {
                    var promocionAplicable = await EvaluarPromocionAsync(promocion, parametros, cancellationToken);
                    if (promocionAplicable != null && !promocionesAplicables.Any(p => p.Id == promocionAplicable.Id))
                    {
                        promocionesAplicables.Add(promocionAplicable);
                    }
                }
            }

            // Ordenar por prioridad y ahorro estimado
            var promocionesOrdenadas = promocionesAplicables
                .OrderBy(p => p.Prioridad)
                .ThenByDescending(p => p.AhorroEstimado)
                .ToList();

            _logger.LogInformation("Se encontraron {Cantidad} promociones aplicables para cliente {ClienteId}", 
                promocionesOrdenadas.Count, parametros.ClienteId);

            return promocionesOrdenadas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando promociones aplicables para cliente {ClienteId}", parametros.ClienteId);
            return new List<PromocionAplicable>();
        }
    }

    /// <summary>
    /// Aplica una promoción específica
    /// </summary>
    public async Task<Result<ResultadoAplicacionPromocion>> AplicarPromocionAsync(
        Guid promocionId, 
        ParametrosCompra parametros, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (parametros == null)
                return Result<ResultadoAplicacionPromocion>.Failure("Los parámetros de compra son requeridos");

            // Validar aplicabilidad
            var esAplicable = await ValidarAplicabilidadAsync(promocionId, parametros, cancellationToken);
            if (!esAplicable.IsSuccess || !esAplicable.Value)
                return Result<ResultadoAplicacionPromocion>.Failure("La promoción no es aplicable a esta compra");

            // TODO: Obtener promoción desde repositorio
            // Por ahora simulamos la aplicación
            var descuentoAplicado = CalcularDescuentoSimulado(parametros.MontoTotal);
            var montoFinal = parametros.MontoTotal - descuentoAplicado;
            var porcentajeDescuento = (descuentoAplicado / parametros.MontoTotal) * 100;

            var resultado = new ResultadoAplicacionPromocion
            {
                PromocionId = promocionId,
                CodigoPromocion = $"PROMO-{promocionId.ToString()[..8].ToUpper()}",
                DescuentoAplicado = descuentoAplicado,
                PuntosOtorgados = CalcularPuntosPromocion(parametros.MontoTotal),
                MontoOriginal = parametros.MontoTotal,
                MontoFinal = montoFinal,
                PorcentajeDescuento = porcentajeDescuento,
                DetalleCalculo = $"Descuento del {porcentajeDescuento:F1}% aplicado sobre ${parametros.MontoTotal:F2}",
                ArticulosAfectados = parametros.Items.Select(i => i.Nombre).ToList()
            };

            _logger.LogInformation("Promoción {PromocionId} aplicada exitosamente. Descuento: ${Descuento:F2}", 
                promocionId, descuentoAplicado);

            return Result<ResultadoAplicacionPromocion>.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aplicando promoción {PromocionId}", promocionId);
            return Result<ResultadoAplicacionPromocion>.Failure($"Error aplicando promoción: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida si una promoción es aplicable
    /// </summary>
    public async Task<Result<bool>> ValidarAplicabilidadAsync(
        Guid promocionId, 
        ParametrosCompra parametros, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (parametros == null)
                return Result<bool>.Failure("Los parámetros de compra son requeridos");

            // Validaciones básicas
            if (parametros.MontoTotal <= 0)
                return Result<bool>.Failure("El monto total debe ser mayor a cero");

            if (!parametros.Items.Any())
                return Result<bool>.Failure("La compra debe tener al menos un item");

            // TODO: Implementar validaciones específicas de promoción
            // Por ahora retornamos true para promociones válidas
            
            // Validar monto mínimo (ejemplo)
            var montoMinimo = 50m; // Esto debería venir de la configuración de la promoción
            if (parametros.MontoTotal < montoMinimo)
                return Result<bool>.Failure($"El monto mínimo requerido es ${montoMinimo:F2}");

            _logger.LogDebug("Promoción {PromocionId} es aplicable para monto ${Monto:F2}", 
                promocionId, parametros.MontoTotal);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando aplicabilidad de promoción {PromocionId}", promocionId);
            return Result<bool>.Failure($"Error validando promoción: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el mejor combo de promociones
    /// </summary>
    public async Task<Result<ComboPromociones>> ObtenerMejorComboAsync(
        ParametrosCompra parametros, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var promocionesAplicables = await CalcularPromocionesAplicablesAsync(parametros, cancellationToken);
            
            if (!promocionesAplicables.Any())
                return Result<ComboPromociones>.Failure("No hay promociones aplicables");

            // Algoritmo simple: seleccionar promociones compatibles con mayor ahorro
            var mejorCombo = await CalcularMejorComboSimple(promocionesAplicables, parametros, cancellationToken);

            _logger.LogInformation("Mejor combo calculado con {Cantidad} promociones y ahorro total de ${Ahorro:F2}", 
                mejorCombo.Promociones.Count, mejorCombo.AhorroTotal);

            return Result<ComboPromociones>.Success(mejorCombo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando mejor combo de promociones");
            return Result<ComboPromociones>.Failure($"Error calculando combo: {ex.Message}");
        }
    }

    #region Métodos Privados de Lógica de Negocio

    /// <summary>
    /// Evalúa si una promoción es aplicable y calcula sus valores
    /// </summary>
    private async Task<PromocionAplicable?> EvaluarPromocionAsync(
        Promocion promocion, 
        ParametrosCompra parametros, 
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implementar lógica específica basada en el tipo de promoción
            // Por ahora simulamos la evaluación
            
            var ahorro = CalcularAhorroEstimado(promocion, parametros.MontoTotal);
            
            if (ahorro <= 0)
                return null;

            return new PromocionAplicable
            {
                Id = promocion.Id,
                Codigo = promocion.Codigo ?? $"PROMO-{promocion.Id.ToString()[..8].ToUpper()}",
                Nombre = promocion.Nombre,
                Descripcion = promocion.Descripcion ?? string.Empty,
                Tipo = promocion.TipoPromocion?.ToString() ?? "General",
                DescuentoPesos = promocion.TipoPromocion == TipoPromocion.DescuentoPesos ? promocion.ValorDescuento : 0,
                DescuentoPorcentaje = promocion.TipoPromocion == TipoPromocion.DescuentoPorcentaje ? promocion.ValorDescuento : 0,
                PuntosOtorgados = promocion.TipoPromocion == TipoPromocion.PuntosExtra ? (int)promocion.ValorDescuento : 0,
                Prioridad = promocion.Prioridad,
                Condiciones = promocion.Condiciones ?? string.Empty,
                FechaVencimiento = promocion.FechaFin,
                AhorroEstimado = ahorro
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluando promoción {PromocionId}", promocion.Id);
            return null;
        }
    }

    /// <summary>
    /// Calcula el ahorro estimado de una promoción
    /// </summary>
    private decimal CalcularAhorroEstimado(Promocion promocion, decimal montoTotal)
    {
        return promocion.TipoPromocion switch
        {
            TipoPromocion.DescuentoPesos => Math.Min(promocion.ValorDescuento, montoTotal),
            TipoPromocion.DescuentoPorcentaje => montoTotal * (promocion.ValorDescuento / 100),
            TipoPromocion.PuntosExtra => promocion.ValorDescuento * 0.5m, // Valor estimado de puntos
            _ => 0m
        };
    }

    /// <summary>
    /// Calcula un descuento simulado (placeholder)
    /// </summary>
    private decimal CalcularDescuentoSimulado(decimal montoTotal)
    {
        // Lógica temporal: 10% de descuento
        return montoTotal * 0.10m;
    }

    /// <summary>
    /// Calcula puntos de promoción (placeholder)
    /// </summary>
    private int CalcularPuntosPromocion(decimal montoTotal)
    {
        // Lógica temporal: 1 punto por cada $10
        return (int)(montoTotal / 10);
    }

    /// <summary>
    /// Calcula el mejor combo simple de promociones
    /// </summary>
    private async Task<ComboPromociones> CalcularMejorComboSimple(
        List<PromocionAplicable> promociones, 
        ParametrosCompra parametros, 
        CancellationToken cancellationToken)
    {
        var combo = new ComboPromociones();

        foreach (var promocion in promociones.Take(3)) // Máximo 3 promociones por simplicidad
        {
            // Verificar compatibilidad con promociones ya seleccionadas
            var promocionesIds = combo.Promociones.Select(p => p.Id).Append(promocion.Id).ToList();
            var sonCompatibles = await _servicioPromociones.ValidarCompatibilidadPromocionesAsync(
                promocionesIds, 
                cancellationToken);

            if (sonCompatibles)
            {
                combo.Promociones.Add(promocion);
                combo.DescuentoTotal += promocion.DescuentoPesos + (parametros.MontoTotal * promocion.DescuentoPorcentaje / 100);
                combo.PuntosTotales += promocion.PuntosOtorgados;
                combo.AhorroTotal += promocion.AhorroEstimado;
            }
        }

        combo.DescripcionCombo = combo.Promociones.Count switch
        {
            1 => $"Promoción individual: {combo.Promociones[0].Nombre}",
            2 => $"Combo de 2 promociones",
            3 => $"Combo triple de promociones",
            _ => "Sin promociones aplicables"
        };

        return combo;
    }

    #endregion
} 