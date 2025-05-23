namespace RestaurantePro.Domain.Operaciones.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que actualiza el inventario cuando se modifica una comanda.
    /// Este handler maneja eventos de tipo ProductoAgregadoAComanda y ProductoEliminadoDeComanda.
    /// </summary>
    public class ComandaModificada_ActualizarInventarioHandler : 
        IEventHandler<Comandas.Events.ItemComanda.ProductoAgregadoAComanda>,
        IEventHandler<Comandas.Events.ItemComanda.ProductoEliminadoDeComanda>
    {
        private readonly IComandaRepository _comandaRepository;
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IDomainEventRegistry _eventRegistry;
        private readonly IDateTimeService _dateTimeService;
        private readonly IRecetaService _recetaService;

        public ComandaModificada_ActualizarInventarioHandler(
            IComandaRepository comandaRepository,
            IIngredienteRepository ingredienteRepository,
            IDomainEventRegistry eventRegistry,
            IDateTimeService dateTimeService,
            IRecetaService recetaService = null) // Opcional para permitir inyección en tests
        {
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _recetaService = recetaService; // Puede ser null en tests
        }

        /// <summary>
        /// Maneja el evento ProductoAgregadoAComanda y decrementa el stock de los ingredientes correspondientes.
        /// </summary>
        public async Task Handle(Comandas.Events.ItemComanda.ProductoAgregadoAComanda evento, CancellationToken cancellationToken)
        {
            // Registrar el evento para trazabilidad
            await _eventRegistry.RegisterAsync(evento, cancellationToken);

            // Obtener la comanda
            var comanda = await _comandaRepository.ObtenerPorIdAsync(evento.ComandaId, cancellationToken);
            if (comanda == null)
            {
                // No se encontró la comanda, esto no debería ocurrir
                // En un caso real, se debería loggear este error
                return;
            }

            // Obtener los ingredientes necesarios para el producto
            var ingredientesProducto = await _recetaService.ObtenerIngredientesParaProductoAsync(
                evento.ProductoId, cancellationToken);

            if (ingredientesProducto == null || !ingredientesProducto.Any())
            {
                // Este producto no tiene ingredientes registrados
                return;
            }

            // Decrementar el stock de cada ingrediente
            foreach (var ingrediente in ingredientesProducto)
            {
                var ingredienteId = ingrediente.Key;
                var cantidadPorUnidad = ingrediente.Value;
                var cantidadTotal = cantidadPorUnidad * evento.Cantidad;

                // Obtener el ingrediente
                var ingredienteEntity = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
                if (ingredienteEntity == null)
                {
                    // Ingrediente no encontrado, esto es un error pero continuamos con los demás
                    continue;
                }

                // Decrementar el stock
                ingredienteEntity.DecrementarStock(cantidadTotal, $"Comanda #{evento.ComandaId}: {evento.Cantidad} x {evento.NombreProducto}");

                // Guardar el ingrediente actualizado
                await _ingredienteRepository.ActualizarAsync(ingredienteEntity, cancellationToken);
            }
        }

        /// <summary>
        /// Maneja el evento ProductoEliminadoDeComanda e incrementa el stock de los ingredientes correspondientes.
        /// </summary>
        public async Task Handle(Comandas.Events.ItemComanda.ProductoEliminadoDeComanda evento, CancellationToken cancellationToken)
        {
            // Registrar el evento para trazabilidad
            await _eventRegistry.RegisterAsync(evento, cancellationToken);

            // Obtener los ingredientes necesarios para el producto
            var ingredientesProducto = await _recetaService.ObtenerIngredientesParaProductoAsync(
                evento.ProductoId, cancellationToken);

            if (ingredientesProducto == null || !ingredientesProducto.Any())
            {
                // Este producto no tiene ingredientes registrados
                return;
            }

            // Incrementar el stock de cada ingrediente (reembolso por eliminación)
            foreach (var ingrediente in ingredientesProducto)
            {
                var ingredienteId = ingrediente.Key;
                var cantidadPorUnidad = ingrediente.Value;
                var cantidadTotal = cantidadPorUnidad * evento.Cantidad;

                // Obtener el ingrediente
                var ingredienteEntity = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
                if (ingredienteEntity == null)
                {
                    // Ingrediente no encontrado, esto es un error pero continuamos con los demás
                    continue;
                }

                // Incrementar el stock (devolver los ingredientes al inventario)
                ingredienteEntity.IncrementarStock(
                    cantidadTotal, 
                    $"Devolución por cancelación: Comanda #{evento.ComandaId}: {evento.Cantidad} x {evento.NombreProducto}. Motivo: {evento.Motivo}");

                // Guardar el ingrediente actualizado
                await _ingredienteRepository.ActualizarAsync(ingredienteEntity, cancellationToken);
            }
        }
    }
} 