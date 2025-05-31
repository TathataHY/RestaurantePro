using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Domain.Inventario.Services;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarStock;

/// <summary>
/// Handler para el comando ActualizarStock
/// Gestiona movimientos de inventario: ingresos, egresos y ajustes
/// </summary>
public class ActualizarStockHandler : IRequestHandler<ActualizarStockCommand, Result<IngredienteDto>>
{
    private readonly IIngredienteRepository _repository;
    private readonly IInventarioServiceFacade _inventarioService;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarStockHandler> _logger;

    public ActualizarStockHandler(
        IIngredienteRepository repository,
        IInventarioServiceFacade inventarioService,
        IMapper mapper,
        ILogger<ActualizarStockHandler> logger)
    {
        _repository = repository;
        _inventarioService = inventarioService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IngredienteDto>> Handle(
        ActualizarStockCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📦 Iniciando actualización de stock: Ingrediente {IngredienteId} - Tipo {TipoMovimiento} - Cantidad {Cantidad}", 
            request.IngredienteId, request.TipoMovimiento, request.Cantidad);

        try
        {
            // 1. Verificar que el ingrediente existe
            var ingrediente = await _repository.ObtenerPorIdAsync(request.IngredienteId, false, cancellationToken);
            if (ingrediente == null)
            {
                _logger.LogWarning("⚠️ Ingrediente no encontrado: {IngredienteId}", request.IngredienteId);
                return Result.Failure<IngredienteDto>($"No se encontró el ingrediente con ID {request.IngredienteId}");
            }

            // 2. Verificar que el ingrediente está activo
            if (!ingrediente.EstaActivo)
            {
                _logger.LogWarning("⚠️ Ingrediente inactivo: {Nombre}", ingrediente.Nombre);
                return Result.Failure<IngredienteDto>($"No se puede actualizar stock de ingrediente inactivo: {ingrediente.Nombre}");
            }

            // 3. Verificar que no está bloqueado por control de calidad
            if (ingrediente.BloqueadoControlCalidad)
            {
                _logger.LogWarning("🚫 Ingrediente bloqueado por control de calidad: {Nombre}", ingrediente.Nombre);
                return Result.Failure<IngredienteDto>($"El ingrediente {ingrediente.Nombre} está bloqueado por control de calidad");
            }

            // 4. Parsear tipo de movimiento
            if (!Enum.TryParse<TipoMovimientoInventario>(request.TipoMovimiento, true, out var tipoMovimiento))
            {
                return Result.Failure<IngredienteDto>($"Tipo de movimiento no válido: {request.TipoMovimiento}");
            }

            // 5. Procesar según tipo de movimiento
            var resultadoMovimiento = ProcesarMovimiento(ingrediente, request, tipoMovimiento);
            if (!resultadoMovimiento.Succeeded)
            {
                _logger.LogWarning("❌ Error al procesar movimiento: {Error}", resultadoMovimiento.Error);
                return Result.Failure<IngredienteDto>(resultadoMovimiento.Error);
            }

            // 6. Actualizar costo promedio si se proporcionó nuevo costo
            if (request.NuevoCosto.HasValue && tipoMovimiento == TipoMovimientoInventario.Ingreso)
            {
                try
                {
                    ingrediente.ActualizarCostoPromedio(request.NuevoCosto.Value);
                    _logger.LogInformation("💰 Costo promedio actualizado: ${CostoNuevo}", request.NuevoCosto.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("⚠️ No se pudo actualizar costo promedio: {Error}", ex.Message);
                }
            }

            // 7. Actualizar en el repositorio
            await _repository.ActualizarAsync(ingrediente, cancellationToken);

            // 8. Mapear a DTO y enriquecer
            var ingredienteDto = _mapper.Map<IngredienteDto>(ingrediente);
            EnriquecerIngredienteDto(ingredienteDto);

            // 9. Log del resultado
            var stockNuevo = ingrediente.Stock;
            
            _logger.LogInformation("✅ Stock actualizado: {Nombre} - Stock: {StockNuevo} {UnidadMedida} ({TipoMovimiento}: {Cantidad})",
                ingrediente.Nombre, stockNuevo, ingrediente.UnidadMedida, request.TipoMovimiento, request.Cantidad);

            // 10. Alertas automáticas
            VerificarAlertas(ingrediente);

            return Result.Success(ingredienteDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning("📋 Violación de regla de negocio: {Message}", ex.Message);
            return Result.Failure<IngredienteDto>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado al actualizar stock: Ingrediente {IngredienteId}", request.IngredienteId);
            return Result.Failure<IngredienteDto>("Error interno del servidor al actualizar el stock");
        }
    }

    /// <summary>
    /// Procesa el movimiento según su tipo
    /// </summary>
    private Result<MovimientoResult> ProcesarMovimiento(
        Domain.Inventario.Ingredientes.Entities.Ingrediente ingrediente,
        ActualizarStockCommand request,
        TipoMovimientoInventario tipoMovimiento)
    {
        var stockAnterior = ingrediente.Stock;

        _logger.LogInformation("🔄 Procesando movimiento {TipoMovimiento}: {Cantidad} - Stock actual: {StockActual}", 
            tipoMovimiento, request.Cantidad, stockAnterior);

        try
        {
            return tipoMovimiento switch
            {
                TipoMovimientoInventario.Ingreso => ProcesarIngreso(ingrediente, request, stockAnterior),
                TipoMovimientoInventario.Egreso => ProcesarEgreso(ingrediente, request, stockAnterior),
                TipoMovimientoInventario.Ajuste => ProcesarAjuste(ingrediente, request, stockAnterior),
                _ => Result.Failure<MovimientoResult>($"Tipo de movimiento no soportado: {tipoMovimiento}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al procesar movimiento {TipoMovimiento}", tipoMovimiento);
            return Result.Failure<MovimientoResult>($"Error al procesar movimiento: {ex.Message}");
        }
    }

    /// <summary>
    /// Procesa ingreso de stock (compras, devoluciones)
    /// </summary>
    private Result<MovimientoResult> ProcesarIngreso(
        Domain.Inventario.Ingredientes.Entities.Ingrediente ingrediente,
        ActualizarStockCommand request,
        decimal stockAnterior)
    {
        _logger.LogInformation("⬆️ Procesando INGRESO: +{Cantidad} - Referencia: {Referencia}", 
            request.Cantidad, request.ReferenciaExterna);

        try
        {
            // Usar método del dominio para incrementar stock
            var movimiento = ingrediente.IncrementarStock(request.Cantidad, request.Motivo);
            
            _logger.LogInformation("📈 Ingreso procesado: Movimiento ID {MovimientoId}", movimiento.Id);

            return Result.Success(new MovimientoResult { StockAnterior = stockAnterior, StockNuevo = ingrediente.Stock });
        }
        catch (Exception ex)
        {
            return Result.Failure<MovimientoResult>($"Error al procesar ingreso: {ex.Message}");
        }
    }

    /// <summary>
    /// Procesa egreso de stock (consumo, mermas)
    /// </summary>
    private Result<MovimientoResult> ProcesarEgreso(
        Domain.Inventario.Ingredientes.Entities.Ingrediente ingrediente,
        ActualizarStockCommand request,
        decimal stockAnterior)
    {
        _logger.LogInformation("⬇️ Procesando EGRESO: -{Cantidad} - Motivo: {Motivo}", 
            request.Cantidad, request.Motivo);

        // Verificar que hay suficiente stock
        if (!ingrediente.TieneStockSuficiente(request.Cantidad))
        {
            var deficit = ingrediente.CalcularDeficitStock(request.Cantidad);
            _logger.LogWarning("📉 Stock insuficiente: Déficit de {Deficit}", deficit);
            return Result.Failure<MovimientoResult>(
                $"Stock insuficiente. Stock actual: {ingrediente.Stock:F2}, cantidad requerida: {request.Cantidad:F2}, déficit: {deficit:F2}");
        }

        try
        {
            // Usar método del dominio para decrementar stock
            var movimiento = ingrediente.DecrementarStock(request.Cantidad, request.Motivo);
            
            _logger.LogInformation("📉 Egreso procesado: Movimiento ID {MovimientoId}", movimiento.Id);

            return Result.Success(new MovimientoResult { StockAnterior = stockAnterior, StockNuevo = ingrediente.Stock });
        }
        catch (Exception ex)
        {
            return Result.Failure<MovimientoResult>($"Error al procesar egreso: {ex.Message}");
        }
    }

    /// <summary>
    /// Procesa ajuste de stock (inventarios físicos)
    /// </summary>
    private Result<MovimientoResult> ProcesarAjuste(
        Domain.Inventario.Ingredientes.Entities.Ingrediente ingrediente,
        ActualizarStockCommand request,
        decimal stockAnterior)
    {
        _logger.LogInformation("⚖️ Procesando AJUSTE: Stock {StockActual} → {CantidadReal} - Referencia: {Referencia}", 
            stockAnterior, request.Cantidad, request.ReferenciaExterna);

        var diferencia = request.Cantidad - stockAnterior;
        var tipoAjuste = diferencia > 0 ? "aumento" : "disminución";
        var motivoCompleto = $"{request.Motivo} - {tipoAjuste} de {Math.Abs(diferencia):F2}";

        try
        {
            // Para ajustes, determinamos si es incremento o decremento
            if (diferencia > 0)
            {
                // Incrementar al stock real
                ingrediente.IncrementarStock(diferencia, motivoCompleto);
            }
            else if (diferencia < 0)
            {
                // Decrementar al stock real
                var cantidadADecrementar = Math.Abs(diferencia);
                if (!ingrediente.TieneStockSuficiente(cantidadADecrementar))
                {
                    return Result.Failure<MovimientoResult>(
                        $"No se puede ajustar a {request.Cantidad:F2} porque el stock actual {stockAnterior:F2} sería insuficiente");
                }
                ingrediente.DecrementarStock(cantidadADecrementar, motivoCompleto);
            }
            // Si diferencia == 0, no hay ajuste necesario

            _logger.LogInformation("📊 Ajuste completado: Diferencia {Diferencia:F2} ({Tipo})", 
                Math.Abs(diferencia), tipoAjuste);

            return Result.Success(new MovimientoResult { StockAnterior = stockAnterior, StockNuevo = ingrediente.Stock });
        }
        catch (Exception ex)
        {
            return Result.Failure<MovimientoResult>($"Error al procesar ajuste: {ex.Message}");
        }
    }

    /// <summary>
    /// Enriquece el DTO con información calculada
    /// </summary>
    private void EnriquecerIngredienteDto(IngredienteDto dto)
    {
        // Calcular estado del stock
        // var porcentajeStock = ingrediente.StockMinimo > 0 ? (ingrediente.Stock / ingrediente.StockMinimo) * 100 : 100;
        // dto.PorcentajeStock = porcentajeStock; // TODO: Propiedad de solo lectura
        dto.EstaBajoMinimo = dto.StockActual < dto.StockMinimo;

        // Determinar estado y color
        (dto.EstadoStock, dto.ColorEstado) = dto.StockActual switch
        {
            <= 25 => ("Crítico", "#F44336"),
            <= 50 => ("Bajo", "#FF9800"),
            <= 100 => ("Normal", "#4CAF50"),
            _ => ("Alto", "#2196F3")
        };

        // Calcular valor total del stock
        dto.ValorTotalStock = dto.StockActual * dto.CostoPromedio;

        // Crear resumen del estado
        dto.ResumenEstado = dto.EstaBajoMinimo 
            ? $"🚨 Stock crítico: {dto.StockActual:F2} {dto.UnidadMedida} (Mín: {dto.StockMinimo:F2})"
            : $"✅ Stock normal: {dto.StockActual:F2} {dto.UnidadMedida} - Valor: ${dto.ValorTotalStock:F2}";
    }

    /// <summary>
    /// Verifica alertas automáticas después del movimiento
    /// </summary>
    private void VerificarAlertas(Domain.Inventario.Ingredientes.Entities.Ingrediente ingrediente)
    {
        if (ingrediente.Stock <= 0)
        {
            _logger.LogWarning("🚨 ALERTA CRÍTICA: Ingrediente {Nombre} SIN STOCK", ingrediente.Nombre);
        }
        else if (ingrediente.EstaEnEstadoCritico())
        {
            // var porcentaje = (ingrediente.Stock / ingrediente.StockMinimo) * 100;
            _logger.LogWarning("⚠️ ALERTA: Stock bajo en {Nombre} - {Porcentaje:F1}% del mínimo ({Stock}/{StockMinimo})", 
                ingrediente.Nombre, 0, ingrediente.Stock, ingrediente.StockMinimo);
        }
    }

    /// <summary>
    /// Resultado interno del movimiento
    /// </summary>
    private class MovimientoResult
    {
        public decimal StockAnterior { get; set; }
        public decimal StockNuevo { get; set; }
    }
} 