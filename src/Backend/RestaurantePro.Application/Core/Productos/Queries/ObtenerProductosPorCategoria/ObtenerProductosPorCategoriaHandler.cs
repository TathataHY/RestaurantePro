namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPorCategoria;

public class ObtenerProductosPorCategoriaHandler : IRequestHandler<ObtenerProductosPorCategoriaQuery, Result<List<ProductoDto>>>
{
    private readonly IProductoRepository _repository;
    private readonly IProductoCategoriaRepository _categoriaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerProductosPorCategoriaHandler> _logger;

    public ObtenerProductosPorCategoriaHandler(
        IProductoRepository repository,
        IProductoCategoriaRepository categoriaRepository,
        IMapper mapper,
        ILogger<ObtenerProductosPorCategoriaHandler> logger)
    {
        _repository = repository;
        _categoriaRepository = categoriaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<ProductoDto>>> Handle(ObtenerProductosPorCategoriaQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🏷️ Obteniendo productos por categoría: {CategoriaId}", request.CategoriaId);

        try
        {
            // Verificar que la categoría existe
            var categoria = await _categoriaRepository.ObtenerPorIdAsync(request.CategoriaId, cancellationToken);
            if (categoria == null)
            {
                _logger.LogWarning("⚠️ Categoría no encontrada: {CategoriaId}", request.CategoriaId);
                return Result.Failure<List<ProductoDto>>($"La categoría con ID {request.CategoriaId} no fue encontrada");
            }

            // Obtener productos de la categoría
            var productos = await _repository.ObtenerPorCategoriaAsync(request.CategoriaId, request.SoloActivos, cancellationToken);

            // Aplicar ordenamiento si se requiere
            if (request.OrdenarPorPopularidad)
            {
                productos = productos.OrderByDescending(p => p.Popularidad).ThenBy(p => p.Nombre).ToList();
            }
            else
            {
                productos = productos.OrderBy(p => p.Nombre).ToList();
            }

            // Mapear a DTOs
            var productosDto = _mapper.Map<List<ProductoDto>>(productos);

            _logger.LogInformation("✅ Productos obtenidos para categoría '{CategoriaNombre}': {Count}", 
                categoria.Nombre, productos.Count);

            return Result.Success(productosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener productos por categoría: {CategoriaId}", request.CategoriaId);
            return Result.Failure<List<ProductoDto>>($"Error interno al obtener productos: {ex.Message}");
        }
    }
} 