namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados;

public class ObtenerProductosPaginadosHandler : IRequestHandler<ObtenerProductosPaginadosQuery, Result<PaginatedList<ProductoDto>>>
{
    private readonly IProductoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerProductosPaginadosHandler> _logger;

    public ObtenerProductosPaginadosHandler(
        IProductoRepository repository,
        IMapper mapper,
        ILogger<ObtenerProductosPaginadosHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<ProductoDto>>> Handle(ObtenerProductosPaginadosQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("📄 Obteniendo productos paginados - Página: {PageNumber}, Tamaño: {PageSize}", 
            request.PageNumber, request.PageSize);

        try
        {
            // Obtener productos según los filtros
            IEnumerable<Producto> productos;

            if (request.CategoriaId.HasValue)
            {
                productos = await _repository.ObtenerPorCategoriaAsync(request.CategoriaId.Value, request.SoloActivos, cancellationToken);
            }
            else
            {
                productos = await _repository.ObtenerTodosAsync(request.SoloActivos, cancellationToken);
            }

            // Aplicar filtro de texto si se proporciona
            if (!string.IsNullOrEmpty(request.Filtro))
            {
                var filtroLower = request.Filtro.ToLowerInvariant();
                productos = productos.Where(p => 
                    (p.Nombre?.ToLowerInvariant().Contains(filtroLower) ?? false) ||
                    (p.Descripcion?.ToLowerInvariant().Contains(filtroLower) ?? false));
            }

            // Aplicar ordenamiento
            productos = ApplyOrdering(productos, request.OrderBy, request.OrderDirection);

            // Calcular paginación
            var totalCount = productos.Count();
            var productosArray = productos.ToArray();
            
            var itemsToSkip = (request.PageNumber - 1) * request.PageSize;
            var productosPagina = productosArray
                .Skip(itemsToSkip)
                .Take(request.PageSize)
                .ToList();

            // Mapear a DTOs
            var productosDto = _mapper.Map<List<ProductoDto>>(productosPagina);

            // Crear lista paginada
            var resultado = new PaginatedList<ProductoDto>(
                productosDto,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("✅ Productos obtenidos: {Count} de {Total} - Página {PageNumber}/{TotalPages}", 
                resultado.Items.Count, resultado.TotalCount, resultado.PageNumber, resultado.TotalPages);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener productos paginados");
            return Result.Failure<PaginatedList<ProductoDto>>($"Error interno al obtener productos: {ex.Message}");
        }
    }

    private static IEnumerable<Producto> ApplyOrdering(IEnumerable<Producto> productos, string orderBy, string orderDirection)
    {
        var isDescending = orderDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return orderBy.ToLowerInvariant() switch
        {
            "nombre" => isDescending 
                ? productos.OrderByDescending(p => p.Nombre)
                : productos.OrderBy(p => p.Nombre),
            
            "precio" => isDescending 
                ? productos.OrderByDescending(p => p.Precio?.Valor ?? 0)
                : productos.OrderBy(p => p.Precio?.Valor ?? 0),
            
            "fechacreacion" => isDescending 
                ? productos.OrderByDescending(p => p.FechaCreacion)
                : productos.OrderBy(p => p.FechaCreacion),
                
            "popularidad" => isDescending 
                ? productos.OrderByDescending(p => p.Popularidad)
                : productos.OrderBy(p => p.Popularidad),
            
            _ => productos.OrderBy(p => p.Nombre) // Default por nombre
        };
    }
} 