using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Application.Common.Services;

namespace RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;

public class ActualizarProductoHandler : IRequestHandler<ActualizarProductoCommand, Result<ProductoDto>>
{
    private readonly IProductoRepository _repository;
    private readonly IProductoCategoriaRepository _categoriaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarProductoHandler> _logger;
    private readonly ICacheService _cache;
    private readonly IHtmlSanitizerService _sanitizer;

    public ActualizarProductoHandler(
        IProductoRepository repository,
        IProductoCategoriaRepository categoriaRepository,
        IMapper mapper,
        ILogger<ActualizarProductoHandler> logger,
        ICacheService cache,
        IHtmlSanitizerService sanitizer)
    {
        _repository = repository;
        _categoriaRepository = categoriaRepository;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _sanitizer = sanitizer;
    }

    public async Task<Result<ProductoDto>> Handle(ActualizarProductoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando actualización de producto: {Id}", request.Id);

        try
        {
            // Verificar que el producto existe
            var producto = await _repository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (producto == null)
            {
                _logger.LogWarning("⚠️ Producto no encontrado: {Id}", request.Id);
                return Result.Failure<ProductoDto>($"Producto con ID {request.Id} no encontrado");
            }

            // Verificar que la categoría existe
            var categoria = await _categoriaRepository.ObtenerPorIdAsync(request.CategoriaId, cancellationToken);
            if (categoria == null)
            {
                _logger.LogWarning("⚠️ Categoría no encontrada: {CategoriaId}", request.CategoriaId);
                return Result.Failure<ProductoDto>($"Categoría con ID {request.CategoriaId} no encontrada");
            }

            // Sanitizar datos de entrada para prevenir XSS
            var nombreSanitizado = _sanitizer.SanitizeText(request.Nombre);
            var descripcionSanitizada = string.IsNullOrEmpty(request.Descripcion) 
                ? request.Descripcion 
                : _sanitizer.SanitizeText(request.Descripcion);

            // Actualizar los datos básicos del producto
            var nuevoPrecio = new PrecioProducto(request.Precio);
            producto.Actualizar(nombreSanitizado, descripcionSanitizada, nuevoPrecio);

            // Actualizar la categoría si ha cambiado
            if (producto.CategoriaId != request.CategoriaId)
            {
                producto.ActualizarCategoria(request.CategoriaId, categoria.Nombre);
            }

            // Actualizar el estado del producto
            if (request.Activo && !producto.EstaActivo)
            {
                producto.Activar();
            }
            else if (!request.Activo && producto.EstaActivo)
            {
                producto.Desactivar();
            }

            // Actualizar la imagen si se proporciona
            if (producto.ImagenUrl != request.ImagenUrl)
            {
                producto.ActualizarImagen(request.ImagenUrl);
            }

            // Persistir los cambios
            await _repository.ActualizarAsync(producto, cancellationToken);

            // Invalidar caché relacionada a listados y a este producto
            try
            {
                _cache.InvalidatePattern("productos:lista:*");
                _cache.Remove($"productos:detalle:{producto.Id}");
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "No se pudo invalidar caché de productos tras actualizar {Id}", producto.Id);
            }

            _logger.LogInformation("✅ Producto actualizado exitosamente: {Id}", request.Id);
            return Result.Success(_mapper.Map<ProductoDto>(producto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al actualizar producto: {Id}", request.Id);
            return Result.Failure<ProductoDto>($"Error interno al actualizar el producto: {ex.Message}");
        }
    }
} 