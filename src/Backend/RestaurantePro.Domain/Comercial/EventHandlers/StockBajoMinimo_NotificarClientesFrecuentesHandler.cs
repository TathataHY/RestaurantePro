namespace RestaurantePro.Domain.Comercial.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que notifica a clientes frecuentes cuando un producto está por agotarse
    /// Este es un ejemplo de comunicación entre contextos: Inventario → Comercial
    /// </summary>
    public class StockBajoMinimo_NotificarClientesFrecuentesHandler : IDomainEventHandler<RestaurantePro.Domain.Inventario.Ingredientes.Events.StockBajoMinimo>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly Core.Notificaciones.Services.IServicioNotificaciones _notificacionService;
        private readonly IDomainEventLog _eventLog;
        private readonly IIngredienteRepository _ingredienteRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        public StockBajoMinimo_NotificarClientesFrecuentesHandler(
            IClienteRepository clienteRepository,
            ITarjetaFidelizacionRepository tarjetaRepository,
            IProductoRepository productoRepository,
            Core.Notificaciones.Services.IServicioNotificaciones notificacionService,
            IDomainEventLog eventLog,
            IIngredienteRepository ingredienteRepository)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
            _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
            _notificacionService = notificacionService ?? throw new ArgumentNullException(nameof(notificacionService));
            _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
        }

        /// <summary>
        /// Maneja el evento de stock bajo notificando a clientes frecuentes
        /// </summary>
        public async Task Handle(RestaurantePro.Domain.Inventario.Ingredientes.Events.StockBajoMinimo evento, CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Obtener el producto y el ingrediente relacionado
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(evento.IngredienteId, cancellationToken);
                if (ingrediente == null)
                {
                    await _eventLog.LogEvent(evento, 
                        $"No se encontró el ingrediente {evento.IngredienteId}", 
                        cancellationToken);
                    return;
                }

                // 2. Obtener productos que contienen este ingrediente
                var productos = await _productoRepository.ObtenerProductosPorIngredienteAsync(
                    ingrediente.Id, cancellationToken);

                if (!productos.Any())
                {
                    await _eventLog.LogEvent(evento,
                        $"No se encontraron productos con el ingrediente {ingrediente.Nombre}",
                        cancellationToken);
                    return;
                }

                // 3. Obtener clientes frecuentes de nivel oro o platino
                var nivelesAltos = new[] { NivelFidelizacion.Oro, NivelFidelizacion.Platino };
                var tarjetasClientesFrecuentes = await _tarjetaRepository.ObtenerTarjetasPorNivelesAsync(
                    nivelesAltos, cancellationToken);

                // 4. Crear lista de clientes a notificar
                var clientesIds = tarjetasClientesFrecuentes.Select(t => t.ClienteId).Distinct().ToList();
                var clientes = await _clienteRepository.ObtenerClientesPorIdsAsync(clientesIds, cancellationToken);

                // 5. Preparar el mensaje de notificación
                var productosNombres = string.Join(", ", productos.Select(p => p.Nombre));
                var mensaje = $"Estimado cliente, le informamos que algunos de sus productos favoritos ({productosNombres}) " +
                              $"están próximos a agotarse. ¡Visítenos pronto para disfrutarlos antes que se acaben!";

                // 6. Enviar notificación a cada cliente
                int clientesNotificados = 0;
                foreach (var cliente in clientes.Where(c => c.EstaActivo))
                {
                    await _notificacionService.EnviarNotificacionAsync(
                        "Productos próximos a agotarse",
                        mensaje,
                        TipoNotificacion.Personalizada,
                        cliente.Id,
                        null,
                        cancellationToken);
                    
                    clientesNotificados++;
                }

                // 7. Registrar éxito
                await _eventLog.LogEvent(evento, 
                    $"Se notificaron {clientesNotificados} clientes frecuentes sobre productos próximos a agotarse", 
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Registrar error
                await _eventLog.LogEvent(evento, 
                    $"Error al notificar a clientes frecuentes: {ex.Message}", 
                    cancellationToken);
                throw;
            }
        }
    }
} 