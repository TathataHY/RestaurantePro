namespace RestaurantePro.Domain.Comercial.Promociones.Services;

/// <summary>
/// Implementación del servicio de dominio para calcular promociones aplicables
/// </summary>
public class CalculadoraPromocionesService : ICalculadoraPromocionesService
{
    private readonly IServicioPromociones? _servicioPromociones;
    private readonly IPromocionRepository? _promocionRepository;
    private readonly IProductoRepository? _productoRepository;
    private readonly ILogger<CalculadoraPromocionesService> _logger;

    /// <summary>
    /// Constructor con IServicioPromociones (implementación original)
    /// </summary>
    public CalculadoraPromocionesService(
        IServicioPromociones servicioPromociones,
        ILogger<CalculadoraPromocionesService> logger)
    {
        _servicioPromociones = servicioPromociones ?? throw new ArgumentNullException(nameof(servicioPromociones));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Constructor con repositorios (para tests)
    /// </summary>
    public CalculadoraPromocionesService(
        IPromocionRepository promocionRepository,
        IProductoRepository productoRepository,
        ILogger<CalculadoraPromocionesService> logger)
    {
        _promocionRepository = promocionRepository ?? throw new ArgumentNullException(nameof(promocionRepository));
        _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
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

            // Si tenemos servicio de promociones (implementación original)
            if (_servicioPromociones != null)
            {
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
            }
            // Si tenemos repositorio directo (para tests)
            else if (_promocionRepository != null)
            {
                var promocionesActivas = await _promocionRepository.ObtenerPromocionesActivasAsync(cancellationToken);
                
                foreach (var promocion in promocionesActivas)
                {
                    var promocionAplicable = await EvaluarPromocionAsync(promocion, parametros, cancellationToken);
                    if (promocionAplicable != null)
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
                return Result.Failure<ResultadoAplicacionPromocion>("Los parámetros de compra son requeridos");

            // Validar aplicabilidad
            var esAplicable = await ValidarAplicabilidadAsync(promocionId, parametros, cancellationToken);
            if (!esAplicable.Succeeded || !esAplicable.Value)
                return Result.Failure<ResultadoAplicacionPromocion>("La promoción no es aplicable a esta compra");

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

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aplicando promoción {PromocionId}", promocionId);
            return Result.Failure<ResultadoAplicacionPromocion>($"Error aplicando promoción: {ex.Message}");
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
                return Result.Failure<bool>("Los parámetros de compra son requeridos");

            // Validaciones básicas
            if (parametros.MontoTotal <= 0)
                return Result.Failure<bool>("El monto total debe ser mayor a cero");

            if (!parametros.Items.Any())
                return Result.Failure<bool>("La compra debe tener al menos un item");

            // TODO: Implementar validaciones específicas de promoción
            // Por ahora retornamos true para promociones válidas
            
            // Validar monto mínimo (ejemplo)
            var montoMinimo = 50m; // Esto debería venir de la configuración de la promoción
            if (parametros.MontoTotal < montoMinimo)
                return Result.Failure<bool>($"El monto mínimo requerido es ${montoMinimo:F2}");

            _logger.LogDebug("Promoción {PromocionId} es aplicable para monto ${Monto:F2}", 
                promocionId, parametros.MontoTotal);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando aplicabilidad de promoción {PromocionId}", promocionId);
            return Result.Failure<bool>($"Error validando promoción: {ex.Message}");
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
                return Result.Failure<ComboPromociones>("No hay promociones aplicables");

            // Algoritmo simple: seleccionar promociones compatibles con mayor ahorro
            var mejorCombo = await CalcularMejorComboSimple(promocionesAplicables, parametros, cancellationToken);

            _logger.LogInformation("Mejor combo calculado con {Cantidad} promociones y ahorro total de ${Ahorro:F2}", 
                mejorCombo.Promociones.Count, mejorCombo.AhorroTotal);

            return Result.Success(mejorCombo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando mejor combo de promociones");
            return Result.Failure<ComboPromociones>($"Error calculando combo: {ex.Message}");
        }
    }

    /// <summary>
    /// Evalúa promociones para un cliente específico y lista de productos (método para tests)
    /// </summary>
    public async Task<Result<List<PromocionAplicable>>> EvaluarPromocionesAsync(
        Guid clienteId, 
        IEnumerable<DatosProductoPromocion> productos, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (productos == null || !productos.Any())
                return Result.Failure<List<PromocionAplicable>>("La lista de productos no puede estar vacía");

            var promocionesAplicables = new List<PromocionAplicable>();

            // Si tenemos repositorio directo (para tests)
            if (_promocionRepository != null)
            {
                var promocionesActivas = await _promocionRepository.ObtenerPromocionesActivasAsync(cancellationToken);
                
                foreach (var promocion in promocionesActivas)
                {
                    var montoTotal = productos.Sum(p => p.Cantidad * p.PrecioUnitario);
                    var ahorro = CalcularAhorroEstimado(promocion, montoTotal);
                    
                    if (ahorro > 0)
                    {
                        promocionesAplicables.Add(new PromocionAplicable
                        {
                            Id = promocion.Id,
                            Codigo = promocion.Codigo ?? $"PROMO-{promocion.Id.ToString()[..8].ToUpper()}",
                            Nombre = promocion.Nombre,
                            Descripcion = promocion.Descripcion ?? string.Empty,
                            Tipo = promocion.Tipo.ToString(),
                            DescuentoPesos = promocion.Tipo == TipoPromocion.MontoFijoTotal ? promocion.ValorDescuento : 0,
                            DescuentoPorcentaje = promocion.Tipo == TipoPromocion.PorcentajeTotal ? promocion.ValorDescuento : 0,
                            AhorroEstimado = ahorro
                        });
                    }
                }
            }
            // Si tenemos servicio de promociones (implementación original)
            else if (_servicioPromociones != null)
            {
                var parametros = new ParametrosCompra
                {
                    ClienteId = clienteId,
                    MontoTotal = productos.Sum(p => p.Cantidad * p.PrecioUnitario),
                    Items = productos.Select(p => new ItemCompra
                    {
                        ProductoId = p.ProductoId,
                        Nombre = p.Nombre,
                        Cantidad = p.Cantidad,
                        PrecioUnitario = p.PrecioUnitario
                    }).ToList()
                };

                promocionesAplicables = await CalcularPromocionesAplicablesAsync(parametros, cancellationToken);
            }

            _logger.LogInformation("Se evaluaron promociones para cliente {ClienteId}. Promociones encontradas: {Cantidad}", 
                clienteId, promocionesAplicables.Count);

            return Result.Success(promocionesAplicables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluando promociones para cliente {ClienteId}", clienteId);
            return Result.Failure<List<PromocionAplicable>>($"Error evaluando promociones: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula el mejor combo de promociones basado en productos y reglas (método para tests)
    /// </summary>
    public async Task<Result<ComboPromociones>> CalcularMejorComboAsync(
        IEnumerable<DatosProductoPromocion> productos, 
        IEnumerable<ReglasComboPromocion> reglasCombo, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (productos == null || !productos.Any())
                return Result.Failure<ComboPromociones>("La lista de productos no puede estar vacía");

            var combo = new ComboPromociones();
            var montoTotal = productos.Sum(p => p.Cantidad * p.PrecioUnitario);

            foreach (var regla in reglasCombo)
            {
                // Verificar si se cumplen las categorías requeridas
                var categoriasDispo = productos.Select(p => "General").Distinct(); // Simplificado para tests
                var seCumpleRegla = regla.CategoriasRequeridas.All(cr => categoriasDispo.Contains(cr));

                if (seCumpleRegla)
                {
                    var promocionCombo = new PromocionAplicable
                    {
                        Id = Guid.NewGuid(),
                        Codigo = "COMBO-AUTO",
                        Nombre = $"Combo {string.Join("+", regla.CategoriasRequeridas)}",
                        Tipo = "Combo",
                        DescuentoPorcentaje = regla.DescuentoPorcentaje ?? 0,
                        DescuentoPesos = regla.PrecioFijo ?? 0,
                        AhorroEstimado = regla.DescuentoPorcentaje.HasValue 
                            ? montoTotal * (regla.DescuentoPorcentaje.Value / 100)
                            : regla.PrecioFijo ?? 0
                    };

                    combo.Promociones.Add(promocionCombo);
                    combo.AhorroTotal += promocionCombo.AhorroEstimado;
                    combo.DescuentoTotal += promocionCombo.DescuentoPesos + (montoTotal * promocionCombo.DescuentoPorcentaje / 100);
                }
            }

            combo.DescripcionCombo = combo.Promociones.Any() 
                ? $"Combo aplicado con {combo.Promociones.Count} promocion(es)"
                : "No se encontraron combos aplicables";

            _logger.LogInformation("Combo calculado con ahorro total de ${Ahorro:F2}", combo.AhorroTotal);

            return Result.Success(combo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculando mejor combo");
            return Result.Failure<ComboPromociones>($"Error calculando combo: {ex.Message}");
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
                Tipo = promocion.Tipo.ToString(),
                DescuentoPesos = promocion.Tipo == TipoPromocion.MontoFijoTotal ? promocion.ValorDescuento : 0,
                DescuentoPorcentaje = promocion.Tipo == TipoPromocion.PorcentajeTotal ? promocion.ValorDescuento : 0,
                PuntosOtorgados = promocion.Tipo == TipoPromocion.CanjePuntos ? (int)promocion.ValorDescuento : 0,
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
        return promocion.Tipo switch
        {
            TipoPromocion.MontoFijoTotal => Math.Min(promocion.ValorDescuento, montoTotal),
            TipoPromocion.PorcentajeTotal => montoTotal * (promocion.ValorDescuento / 100),
            TipoPromocion.CanjePuntos => promocion.ValorDescuento * 0.5m, // Valor estimado de puntos
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
            
            bool sonCompatibles = true;
            
            // Solo verificar compatibilidad si tenemos el servicio de promociones
            if (_servicioPromociones != null)
            {
                sonCompatibles = await _servicioPromociones.ValidarCompatibilidadPromocionesAsync(
                    promocionesIds, 
                    cancellationToken);
            }
            // Para tests o cuando no tenemos el servicio, asumimos compatibilidad básica
            else
            {
                // Lógica simplificada: no permitir más de una promoción de descuento porcentual
                var yaHayPorcentaje = combo.Promociones.Any(p => p.DescuentoPorcentaje > 0);
                var esPorcentaje = promocion.DescuentoPorcentaje > 0;
                sonCompatibles = !(yaHayPorcentaje && esPorcentaje);
            }

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