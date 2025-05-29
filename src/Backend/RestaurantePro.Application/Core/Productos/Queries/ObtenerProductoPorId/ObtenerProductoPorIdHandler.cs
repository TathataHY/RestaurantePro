namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerProductoPorId;

/// <summary>
/// Handler para obtener un producto por ID
/// Vertical Slice completo: ObtenerProductoPorId
/// </summary>
public class ObtenerProductoPorIdHandler : IRequestHandler<ObtenerProductoPorIdQuery, Result<ProductoDto>>
{
    private readonly IProductoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerProductoPorIdHandler> _logger;

    public ObtenerProductoPorIdHandler(
        IProductoRepository repository,
        IMapper mapper,
        ILogger<ObtenerProductoPorIdHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ProductoDto>> Handle(ObtenerProductoPorIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Obteniendo producto por ID: {ProductoId}", request.ProductoId);

        try
        {
            var producto = await _repository.ObtenerPorIdAsync(request.ProductoId);

            if (producto == null)
            {
                _logger.LogWarning("❌ Producto no encontrado: {ProductoId}", request.ProductoId);
                return Result.Failure<ProductoDto>($"Producto con ID {request.ProductoId} no encontrado");
            }

            var productoDto = _mapper.Map<ProductoDto>(producto);

            _logger.LogInformation("✅ Producto obtenido exitosamente: {ProductoId} - {Nombre}", 
                request.ProductoId, producto.Nombre);
            
            return Result.Success(productoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al obtener producto: {ProductoId}", request.ProductoId);
            return Result.Failure<ProductoDto>($"Error interno del servidor: {ex.Message}");
        }
    }
} 