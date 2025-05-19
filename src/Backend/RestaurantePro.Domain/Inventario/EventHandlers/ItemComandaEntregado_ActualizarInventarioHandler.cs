namespace RestaurantePro.Domain.Inventario.EventHandlers
{
    /// <summary>
    /// Manejador que actualiza el inventario cuando un ítem de comanda se marca como entregado
    /// Este enfoque permite actualizar el inventario en tiempo real, cuando el ítem se entrega al cliente
    /// en lugar de esperar a que toda la comanda sea finalizada
    /// </summary>
    public class ItemComandaEntregado_ActualizarInventarioHandler : IDomainEventHandler<ItemComandaEntregado>
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IMovimientoInventarioRepository _movimientoRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IProductoIngredienteRepository _productoIngredienteRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDomainEventRegistry _eventRegistry;
        
        public ItemComandaEntregado_ActualizarInventarioHandler(
            IIngredienteRepository ingredienteRepository,
            IMovimientoInventarioRepository movimientoRepository,
            IProductoRepository productoRepository,
            IProductoIngredienteRepository productoIngredienteRepository,
            IDateTimeService dateTimeService,
            IDomainEventRegistry eventRegistry)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _movimientoRepository = movimientoRepository ?? throw new ArgumentNullException(nameof(movimientoRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _productoIngredienteRepository = productoIngredienteRepository ?? throw new ArgumentNullException(nameof(productoIngredienteRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
        }
        
        /// <summary>
        /// Maneja el evento actualizando el inventario cuando se entrega un ítem
        /// </summary>
        public async Task Handle(ItemComandaEntregado evento, CancellationToken cancellationToken = default)
        {
            try
            {
                // Obtener el producto
                var producto = await _productoRepository.ObtenerPorIdAsync(evento.ProductoId, cancellationToken);
                if (producto == null)
                {
                    await _eventRegistry.RegisterAsync(evento, cancellationToken);
                    return;
                }
                
                // Obtener los ingredientes asociados al producto
                var ingredientes = await _ingredienteRepository.ObtenerIngredientesPorProductoAsync(producto.Id, cancellationToken);
                if (ingredientes == null || !ingredientes.Any())
                {
                    // Si no hay ingredientes, no hay nada que actualizar
                    return;
                }
                
                // Por cada ingrediente, actualizar el inventario
                foreach (var ingrediente in ingredientes)
                {
                    // Obtener la relación entre producto e ingrediente
                    var productoIngrediente = await _productoIngredienteRepository.ObtenerPorProductoEIngredienteAsync(
                        producto.Id, ingrediente.Id, cancellationToken);
                        
                    if (productoIngrediente == null)
                    {
                        await _eventRegistry.RegisterAsync(evento, cancellationToken);
                        continue;
                    }
                    
                    // Calcular la cantidad total a decrementar
                    decimal cantidadTotal = productoIngrediente.Cantidad * evento.Cantidad;
                    
                    // Crear movimiento para registrar el consumo del ingrediente
                    var movimiento = MovimientoInventario.CrearEgreso(
                        ingrediente.Id,
                        cantidadTotal,
                        $"Entrega de ítem #{evento.ItemComandaId} en comanda #{evento.ComandaId}",
                        _dateTimeService.Now);
                    
                    // Registrar el movimiento
                    await _movimientoRepository.AgregarAsync(movimiento);
                    
                    // Decrementar el stock del ingrediente
                    ingrediente.DecrementarStock(cantidadTotal, $"Entrega ítem #{evento.ItemComandaId}");
                    
                    // Actualizar el ingrediente con su nuevo stock
                    await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
                    
                    // Verificar si se necesita emitir una alerta de stock bajo
                    if (ingrediente.Stock < ingrediente.StockMinimo)
                    {
                        await _eventRegistry.RegisterAsync(evento, cancellationToken);
                    }
                }
                
                // Registrar el éxito de la operación
                await _eventRegistry.RegisterAsync(evento, cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventRegistry.RegisterAsync(evento, cancellationToken);
            }
        }
    }
}