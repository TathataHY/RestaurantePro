using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.Productos.Services;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;

namespace RestaurantePro.Domain.Core.Services
{
    public class CoreServiceFacade : ICoreServiceFacade
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICalculoRecetaService _calculoRecetaService;
        private readonly INotificationManager _notificationManager;

        public CoreServiceFacade(
            IProductoRepository productoRepository,
            IUsuarioRepository usuarioRepository,
            ICalculoRecetaService calculoRecetaService,
            INotificationManager notificationManager)
        {
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _calculoRecetaService = calculoRecetaService ?? throw new ArgumentNullException(nameof(calculoRecetaService));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        }

        public async Task<Result<Producto>> CrearProducto(string nombre, string descripcion, decimal precio, Guid categoriaId, string? imagenUrl, CancellationToken cancellationToken)
        {
            _notificationManager.CreateNewNotification();

            var producto = Producto.Crear(nombre, descripcion, new PrecioProducto(precio), categoriaId, imagenUrl);

            await _productoRepository.AgregarAsync(producto, cancellationToken);
            
            return Result.Success(producto);
        }

        public async Task<Result<Producto>> ActualizarProducto(Guid id, string nombre, string descripcion, decimal precio, Guid categoriaId, string? imagenUrl, CancellationToken cancellationToken)
        {
            _notificationManager.CreateNewNotification();

            var producto = await _productoRepository.ObtenerPorIdAsync(id, cancellationToken);
            if (producto == null)
            {
                _notificationManager.AddError("El producto no existe");
                return _notificationManager.ToResult<Producto>(null);
                }

            producto.Actualizar(nombre, descripcion, new PrecioProducto(precio));
            
            if (producto.CategoriaId != categoriaId)
            {
                producto.ActualizarCategoria(categoriaId, "Nueva Categoría"); // Placeholder
            }
            
            await _productoRepository.ActualizarAsync(producto, cancellationToken);

            return Result.Success(producto);
        }

        public async Task<Result<Receta>> AsignarRecetaAProducto(Guid productoId, Dictionary<Guid, decimal> ingredientes, CancellationToken cancellationToken)
        {
            _notificationManager.CreateNewNotification();

            var producto = await _productoRepository.ObtenerPorIdAsync(productoId, cancellationToken);
            if (producto == null)
            {
                _notificationManager.AddError("El producto no existe");
                return _notificationManager.ToResult<Receta>(null);
            }

            var receta = Receta.Crear(productoId, "Receta principal", 15); // Placeholder
            
            producto.Recetas.Add(receta);
            
            await _productoRepository.ActualizarAsync(producto, cancellationToken);

            return Result.Success(receta);
        }

        public Task<Result<Dictionary<Guid, decimal>>> ObtenerIngredientesParaProductoAsync(Guid productoId, CancellationToken cancellationToken)
        {
            return _calculoRecetaService.ObtenerIngredientesParaProductoAsync(productoId, cancellationToken);
        }

        public Task<Result<bool>> VerificarDisponibilidadIngredientesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken)
        {
            return _calculoRecetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, cancellationToken);
        }

        public Task<Result<decimal>> CalcularCostoRecetaAsync(Guid productoId, CancellationToken cancellationToken)
        {
            return _calculoRecetaService.CalcularCostoRecetaAsync(productoId, cancellationToken);
        }

        public async Task<Result<Usuario>> CrearUsuario(string nombreUsuario, string email, string rol, CancellationToken cancellationToken)
        {
            _notificationManager.CreateNewNotification();
            
            if (!Enum.TryParse<RolUsuario>(rol, true, out var rolEnum))
            {
                _notificationManager.AddError("El rol especificado no es válido.");
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            var usuario = Usuario.Crear(nombreUsuario, nombreUsuario, email, rolEnum);

            var usuarioExistente = await _usuarioRepository.BuscarPorEmailAsync(email);
            if (usuarioExistente != null)
        {
                _notificationManager.AddError("El email ya está en uso.");
                return _notificationManager.ToResult<Usuario>(null);
            }
            
            await _usuarioRepository.AgregarAsync(usuario, cancellationToken);
                
                return Result.Success(usuario);
            }
    }
}
