namespace RestaurantePro.Domain.Operaciones.Services
{
    /// <summary>
    /// Implementación del servicio de integración entre los contextos de Operaciones e Inventario.
    /// </summary>
    public class OperacionesInventarioIntegrationService : IOperacionesInventarioIntegrationService
    {
        private readonly IComandaRepository _comandaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IMovimientoInventarioRepository _movimientoInventarioRepository;
        private readonly ILogger<OperacionesInventarioIntegrationService> _logger;
        private readonly INotificationManager _notificationManager;
        private readonly IDateTimeService _dateTimeService;

        public OperacionesInventarioIntegrationService(
            IComandaRepository comandaRepository,
            IProductoRepository productoRepository,
            IIngredienteRepository ingredienteRepository,
            IMovimientoInventarioRepository movimientoInventarioRepository,
            ILogger<OperacionesInventarioIntegrationService> logger,
            INotificationManager notificationManager,
            IDateTimeService dateTimeService)
        {
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _movimientoInventarioRepository = movimientoInventarioRepository ?? throw new ArgumentNullException(nameof(movimientoInventarioRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        public async Task<Result<bool>> DescontarInventarioPorComandaAsync(Guid comandaId, CancellationToken cancellationToken = default)
        {
            var comanda = await _comandaRepository.ObtenerPorIdConItemsAsync(comandaId, cancellationToken);
            if (comanda == null)
            {
                _notificationManager.AddError("Comanda no encontrada");
                return Result.Failure<bool>("Comanda no encontrada");
            }

            foreach (var item in comanda.Items)
            {
                var producto = await _productoRepository.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
                if (producto == null)
                {
                     _logger.LogWarning("Producto con ID {ProductoId} no encontrado al descontar inventario.", item.ProductoId);
                    continue;
                }

                if (producto.Recetas.Any())
                {
                    var receta = producto.Recetas.FirstOrDefault();
                    if (receta == null) continue;

                    foreach (var ingredienteReceta in receta.Ingredientes)
                    {
                        var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteReceta.IngredienteId, false, cancellationToken);
                        if (ingrediente == null)
                        {
                            // Log o manejar el caso de ingrediente no encontrado
                            continue;
                        }

                        var cantidadNecesaria = ingredienteReceta.Cantidad * item.Cantidad;
                        
                        ingrediente.DecrementarStock(
                            cantidadNecesaria,
                            $"Consumo por comanda {comanda.NumeroComanda}"
                        );

                        await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
                    }
                }
            }
            
            _logger.LogInformation("Inventario descontado para comanda {ComandaId}", comandaId);
            return Result.Success(true);
        }
        
        // Los otros métodos como VerificarDisponibilidad, Reservar, etc. se simplifican o eliminan
        // ya que la lógica ahora es más directa. Se pueden añadir de nuevo si se necesita una
        // lógica de negocio más compleja (ej. reservas explícitas de stock).
    }
} 