using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Application.Common.Services;

namespace RestaurantePro.Application.Core.Productos.Commands.CrearProducto;

/// <summary>
/// Handler para el comando CrearProducto
/// Vertical Slice completo: CrearProducto
/// </summary>
public class CrearProductoHandler : IRequestHandler<CrearProductoCommand, Result<ProductoDto>>
{
    private readonly IProductoRepository _repository;
    private readonly IProductoCategoriaRepository _categoriaRepository;
    private readonly ProductoBuilder _builder;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearProductoHandler> _logger;
    private readonly ICacheService _cache;
    private readonly IHtmlSanitizerService _sanitizer;

    public CrearProductoHandler(
        IProductoRepository repository,
        IProductoCategoriaRepository categoriaRepository,
        ProductoBuilder builder,
        IMapper mapper,
        ILogger<CrearProductoHandler> logger,
        ICacheService cache,
        IHtmlSanitizerService sanitizer)
    {
        _repository = repository;
        _categoriaRepository = categoriaRepository;
        _builder = builder;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _sanitizer = sanitizer;
    }

    public async Task<Result<ProductoDto>> Handle(CrearProductoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🍕 Iniciando creación de producto: {Nombre}", request.Nombre);

        try
        {
            // 0. Sanitizar datos de entrada para prevenir XSS
            var nombreSanitizado = _sanitizer.SanitizeText(request.Nombre);
            var descripcionSanitizada = string.IsNullOrEmpty(request.Descripcion) 
                ? request.Descripcion 
                : _sanitizer.SanitizeText(request.Descripcion);

            // 1. Validar que la categoría existe
            var categoria = await _categoriaRepository.ObtenerPorIdAsync(request.CategoriaId, cancellationToken);
            if (categoria == null)
            {
                _logger.LogWarning("❌ Categoría no encontrada: {CategoriaId}", request.CategoriaId);
                return Result.Failure<ProductoDto>($"Categoría con ID {request.CategoriaId} no encontrada");
            }

            // 2. Validar que no exista un producto con el mismo nombre
            var productoExistente = await _repository.ObtenerPorNombreAsync(nombreSanitizado);
            if (productoExistente != null)
            {
                _logger.LogWarning("❌ Ya existe un producto con el nombre: {Nombre}", nombreSanitizado);
                return Result.Failure<ProductoDto>($"Ya existe un producto con el nombre '{nombreSanitizado}'");
            }

            // 3. Usar builder del dominio para crear el producto
            var resultado = _builder
                .ConNombre(nombreSanitizado)
                .ConDescripcion(descripcionSanitizada)
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