namespace RestaurantePro.Domain.Inventario.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que actualiza el inventario cuando se crea una comanda
    /// Este es un ejemplo de comunicación entre contextos: Operaciones → Inventario
    /// </summary>
    public class ComandaCreada_ActualizarInventarioHandler : IDomainEventHandler<ComandaCreada>
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IMovimientoInventarioRepository _movimientoRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDomainEventLog _eventLog;

        /// <summary>
        /// Constructor
        /// </summary>
        public ComandaCreada_ActualizarInventarioHandler(
            IIngredienteRepository ingredienteRepository,
            IMovimientoInventarioRepository movimientoRepository,
            IProductoRepository productoRepository,
            IDateTimeService dateTimeService,
            IDomainEventLog eventLog)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _movimientoRepository = movimientoRepository ?? throw new ArgumentNullException(nameof(movimientoRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
        }

        /// <summary>
        /// Maneja el evento de creación de comanda actualizando el inventario
        /// </summary>
        public async Task Handle(ComandaCreada evento, CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Obtener los productos pedidos en la comanda
                foreach (var item in evento.Items)
                {
                    // 2. Obtener el producto
                    var producto = await _productoRepository.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
                    if (producto == null)
                    {
                        await _eventLog.LogEvent(evento, 
                            $"No se encontró el producto {item.ProductoId} al procesar la comanda {evento.ComandaId}", 
                            cancellationToken);
                        continue;
                    }

                    // 3. Obtener los ingredientes asociados al producto
                    var ingredientesProducto = await _ingredienteRepository.ObtenerIngredientesPorProductoAsync(
                        producto.Id, cancellationToken);

                    // 4. Disminuir el stock de cada ingrediente
                    foreach (var ingrediente in ingredientesProducto)
                    {
                        // Calcular cantidad a reducir del inventario (cantidad del ítem * cantidad requerida del ingrediente)
                        decimal cantidadReducir = item.Cantidad * ingrediente.CantidadPorProducto;

                        // Crear movimiento de inventario
                        var movimiento = MovimientoInventario.Crear(
                            ingrediente.Id,
                            TipoMovimientoInventario.Salida,
                            cantidadReducir,
                            $"Venta en comanda #{evento.ComandaId}",
                            evento.ComandaId);

                        // Guardar el movimiento
                        await _movimientoRepository.AgregarAsync(movimiento, cancellationToken);

                        // Actualizar stock del ingrediente
                        ingrediente.ActualizarStock(
                            ingrediente.StockActual - cantidadReducir,
                            _dateTimeService.Now);

                        // Guardar el ingrediente actualizado
                        await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
                    }
                }

                // Registrar éxito
                await _eventLog.LogEvent(evento, "Inventario actualizado correctamente", cancellationToken);
            }
            catch (Exception ex)
            {
                // Registrar error
                await _eventLog.LogEvent(evento, $"Error al actualizar inventario: {ex.Message}", cancellationToken);
                throw; // Re-lanzar la excepción para que pueda ser manejada en niveles superiores
            }
        }
    }
} 