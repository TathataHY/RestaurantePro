namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientePorId;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;

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
            // 1. Buscar el ingrediente en el repositorio con recarga forzada
            var ingrediente = await _repository.ObtenerPorIdAsync(request.Id, request.IncluirMovimientos, cancellationToken);
            if (ingrediente == null)
            {
                _logger.LogWarning("⚠️ Ingrediente no encontrado: {Id}", request.Id);
                return Result.Failure<IngredienteDto>($"No se encontró el ingrediente con ID {request.Id}");
            }

            // 2. Forzar recarga desde la base de datos para asegurar datos actualizados
            _logger.LogInformation("🔄 Forzando recarga de ingrediente desde BD: {Id}", request.Id);
            await _repository.RecargarEntidadAsync(ingrediente, cancellationToken);

            // 2. Mapear a DTO
            var ingredienteDto = _mapper.Map<IngredienteDto>(ingrediente);

            // 3. Enriquecer DTO con información calculada
            EnriquecerIngredienteDto(ingredienteDto, ingrediente);

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
    private void EnriquecerIngredienteDto(IngredienteDto dto, Ingrediente ingrediente)
    {
        // Asignar propiedades que están marcadas como Ignore() en el mapeo
        dto.StockActual = ingrediente.Stock;
        dto.StockMaximo = ingrediente.StockMinimo * 3; // Stock máximo = 3x stock mínimo
        dto.CostoUnitario = ingrediente.CostoPromedio; // Usar costo promedio como costo unitario
        dto.Activo = ingrediente.EstaActivo; // Activo = estado real de actividad
        dto.RequiereRefrigeracion = false; // Por defecto no requiere refrigeración
        dto.DiasVencimiento = 0; // Por defecto 0 días de vencimiento
        
        // Las propiedades calculadas ya están disponibles en IngredienteDto:
        // - EstadoStock (propiedad calculada)
        // - ColorEstado (propiedad calculada) 
        // - PorcentajeStock (propiedad calculada)
        // - ValorTotalStock (propiedad calculada)
        // - MensajeAlerta (propiedad calculada)
        // - DiasStockDisponible (propiedad calculada)
        
        _logger.LogDebug("Ingrediente enriquecido: {Nombre} - Stock: {StockActual} - {Estado} - {Alerta}", 
            dto.Nombre, dto.StockActual, dto.EstadoStock, dto.MensajeAlerta);
    }

    /// <summary>
    /// Enriquece el DTO del movimiento con información de UI
    /// </summary>
    private void EnriquecerMovimientoDto(MovimientoInventarioDto dto)
    {
        // Según mejores prácticas, el backend solo debe devolver datos crudos
        // El formateo debe hacerse en el frontend/UI
        // Por eso eliminamos las propiedades formateadas del DTO
        
        _logger.LogDebug("Movimiento enriquecido: {Tipo} - {Cantidad} - {Fecha}", 
            dto.TipoMovimientoTexto, dto.Cantidad, dto.FechaCreacion);
    }
} 