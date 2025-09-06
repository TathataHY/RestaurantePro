using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;

namespace RestaurantePro.Application.Core.Productos.Commands.CrearProducto;

/// <summary>
/// Handler para el comando CrearProducto
/// Vertical Slice completo: CrearProducto
/// </summary>
public class CrearProductoHandler : IRequestHandler<CrearProductoCommand, Result<ProductoDto>>
{
    private readonly IProductoRepository _repository;
    private readonly ProductoBuilder _builder;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearProductoHandler> _logger;
    private readonly ICacheService _cache;

    public CrearProductoHandler(
        IProductoRepository repository,
        ProductoBuilder builder,
        IMapper mapper,
        ILogger<CrearProductoHandler> logger,
        ICacheService cache)
    {
        _repository = repository;
        _builder = builder;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<ProductoDto>> Handle(CrearProductoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🍕 Iniciando creación de producto: {Nombre}", request.Nombre);

        try
        {
            // 1. Usar builder del dominio para crear el producto
            var resultado = _builder
                .ConNombre(request.Nombre)
                .ConDescripcion(request.Descripcion)
                .ConPrecio(request.Precio)
                .EnCategoria(request.CategoriaId)
                .Construir();

            if (!resultado.Succeeded)
            {
                _logger.LogWarning("❌ Error al construir producto: {Error}", resultado.Error);
                return Result.Failure<ProductoDto>(resultado.Error ?? "Error desconocido al construir producto");
            }

            var producto = resultado.Value;
            
            // 2. Persistir en repositorio
            await _repository.AgregarAsync(producto);
            await _repository.GuardarCambiosAsync(); // ✅ AGREGAR: Guardar cambios en BD

            // 3. Mapear a DTO para respuesta
            var productoDto = _mapper.Map<ProductoDto>(producto);

            try
            {
                _cache.InvalidatePattern("productos:lista:*");
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "No se pudo invalidar caché de listados tras crear producto {Id}", producto.Id);
            }

            _logger.LogInformation("✅ Producto creado exitosamente: {Id} - {Nombre}", producto.Id, producto.Nombre);
            return Result.Success(productoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al crear producto: {Nombre}", request.Nombre);
            return Result.Failure<ProductoDto>($"Error interno del servidor: {ex.Message}");
        }
    }
} 