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
        // Las propiedades ya están disponibles como calculadas en IngredienteDto
        // No necesitamos calcular manualmente porque el DTO ya tiene:
        // - EstadoStock (propiedad calculada)
        // - ColorEstado (propiedad calculada) 
        // - PorcentajeStock (propiedad calculada)
        // - ValorTotalStock (propiedad calculada)
        // - MensajeAlerta (propiedad calculada)
        // - DiasStockDisponible (propiedad calculada)
        
        _logger.LogDebug("Ingrediente enriquecido: {Nombre} - {Estado} - {Alerta}", 
            dto.Nombre, dto.EstadoStock, dto.MensajeAlerta);
    }

    /// <summary>
    /// Enriquece el DTO del movimiento con información de UI
    /// </summary>
    private void EnriquecerMovimientoDto(MovimientoInventarioDto dto)
    {
        // Las propiedades de UI ya están disponibles como calculadas en MovimientoInventarioDto:
        // - CantidadFormateada (propiedad calculada)
        // - StockAnteriorFormateado (propiedad calculada)
        // - StockPosteriorFormateado (propiedad calculada)
        // - FechaFormateada (propiedad calculada)
        // - EsEntrada (propiedad calculada)
        // - EsSalida (propiedad calculada)
        
        _logger.LogDebug("Movimiento enriquecido: {Tipo} - {Cantidad} - {Fecha}", 
            dto.TipoMovimientoTexto, dto.CantidadFormateada, dto.FechaFormateada);
    }
} 