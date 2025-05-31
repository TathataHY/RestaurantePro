using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientePorId;

/// <summary>
/// Handler para obtener un ingrediente por ID
/// Enriquece el DTO con información calculada y movimientos recientes
/// </summary>
public class ObtenerIngredientePorIdHandler : IRequestHandler<ObtenerIngredientePorIdQuery, Result<IngredienteDto>>
{
    private readonly IIngredienteRepository _repository;
    private readonly IMovimientoInventarioRepository _movimientoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerIngredientePorIdHandler> _logger;

    public ObtenerIngredientePorIdHandler(
        IIngredienteRepository repository,
        IMovimientoInventarioRepository movimientoRepository,
        IMapper mapper,
        ILogger<ObtenerIngredientePorIdHandler> logger)
    {
        _repository = repository;
        _movimientoRepository = movimientoRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IngredienteDto>> Handle(
        ObtenerIngredientePorIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Obteniendo ingrediente por ID: {Id}", request.Id);

        try
        {
            // 1. Buscar el ingrediente en el repositorio
            var ingrediente = await _repository.ObtenerPorIdAsync(request.Id, request.IncluirMovimientos, cancellationToken);
            if (ingrediente == null)
            {
                _logger.LogWarning("⚠️ Ingrediente no encontrado: {Id}", request.Id);
                return Result.Failure<IngredienteDto>($"No se encontró el ingrediente con ID {request.Id}");
            }

            // 2. Mapear a DTO
            var ingredienteDto = _mapper.Map<IngredienteDto>(ingrediente);

            // 3. Enriquecer DTO con información calculada
            EnriquecerIngredienteDto(ingredienteDto);

            // 4. Cargar movimientos recientes si se solicita
            if (request.IncluirMovimientos)
            {
                var movimientos = await _movimientoRepository.ObtenerPorIngredienteAsync(request.Id, cancellationToken);
                var movimientosRecientes = movimientos.OrderByDescending(m => m.Fecha).Take(10);
                ingredienteDto.MovimientosRecientes = _mapper.Map<List<MovimientoInventarioDto>>(movimientosRecientes);

                // Enriquecer cada movimiento
                foreach (var movimiento in ingredienteDto.MovimientosRecientes)
                {
                    EnriquecerMovimientoDto(movimiento);
                }

                _logger.LogInformation("📋 Movimientos recientes cargados: {Count}", ingredienteDto.MovimientosRecientes.Count);
            }

            _logger.LogInformation("✅ Ingrediente obtenido: {Nombre} - Stock: {Stock}", 
                ingrediente.Nombre, ingrediente.Stock);

            return Result.Success(ingredienteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener ingrediente: {Id}", request.Id);
            return Result.Failure<IngredienteDto>("Error interno al obtener el ingrediente");
        }
    }

    /// <summary>
    /// Enriquece el DTO del ingrediente con información calculada
    /// </summary>
    private void EnriquecerIngredienteDto(IngredienteDto dto)
    {
        // TODO: Implementar cuando estén disponibles todas las propiedades en IngredienteDto
        /*
        // Calcular estado del stock
        var porcentajeStock = dto.StockMinimo > 0 ? (dto.StockActual / dto.StockMinimo) * 100 : 100;
        dto.PorcentajeStock = porcentajeStock;
        dto.EstaBajoMinimo = dto.StockActual < dto.StockMinimo;

        // Determinar estado y color
        (dto.EstadoStock, dto.ColorEstado) = porcentajeStock switch
        {
            <= 25 => ("Crítico", "#F44336"),
            <= 50 => ("Bajo", "#FF9800"),
            <= 100 => ("Normal", "#4CAF50"),
            _ => ("Alto", "#2196F3")
        };

        // Calcular valor total del stock
        dto.ValorTotalStock = dto.StockActual * dto.CostoPromedio;

        // Estimar días de duración (simplificado - en producción sería más complejo)
        // Basado en rotación: Alta = 7 días, Media = 14 días, Baja = 30 días
        var consumoDiario = dto.Rotacion.ToLower() switch
        {
            "alta" => dto.StockActual / 7,
            "media" => dto.StockActual / 14,
            "baja" => dto.StockActual / 30,
            _ => dto.StockActual / 14
        };

        dto.DiasEstimadosDuracion = consumoDiario > 0 ? (int)(dto.StockActual / consumoDiario) : null;

        // Crear resumen del estado
        dto.ResumenEstado = dto.EstaBajoMinimo 
            ? $"🚨 Stock crítico: {dto.StockActual:F2} {dto.UnidadMedida} (Mín: {dto.StockMinimo:F2})"
            : $"✅ Stock normal: {dto.StockActual:F2} {dto.UnidadMedida} - Valor: ${dto.ValorTotalStock:F2}";
        */
    }

    /// <summary>
    /// Enriquece el DTO del movimiento con información de UI
    /// </summary>
    private void EnriquecerMovimientoDto(MovimientoInventarioDto dto)
    {
        // Determinar color e icono según tipo de movimiento
        (dto.ColorTipo, dto.IconoTipo) = dto.TipoMovimiento.ToLower() switch
        {
            "ingreso" => ("#4CAF50", "arrow_upward"),
            "egreso" => ("#F44336", "arrow_downward"),
            "ajuste" => ("#FF9800", "tune"),
            _ => ("#9E9E9E", "sync")
        };

        // Formatear fecha
        dto.FechaTexto = dto.Fecha.ToString("dd/MM/yyyy HH:mm");

        // Formatear cantidad con signo
        var signo = dto.TipoMovimiento.ToLower() == "egreso" ? "-" : "+";
        dto.CantidadTexto = $"{signo}{dto.Cantidad:F2}";
    }
} 