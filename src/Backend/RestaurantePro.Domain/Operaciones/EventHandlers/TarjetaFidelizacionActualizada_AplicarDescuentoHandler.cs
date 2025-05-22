namespace RestaurantePro.Domain.Operaciones.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que se encarga de aplicar descuentos en comandas pendientes
    /// cuando un cliente mejora su nivel de fidelización.
    /// Este es un ejemplo de comunicación entre contextos: Comercial → Operaciones
    /// </summary>
    public class TarjetaFidelizacionActualizada_AplicarDescuentoHandler : IDomainEventHandler<NivelFidelizacionActualizado>
    {
        private readonly IComandaRepository _comandaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IServicioFidelizacion _servicioFidelizacion;
        private readonly IDomainEventRegistry _eventRegistry;

        /// <summary>
        /// Constructor
        /// </summary>
        public TarjetaFidelizacionActualizada_AplicarDescuentoHandler(
            IComandaRepository comandaRepository,
            IClienteRepository clienteRepository,
            IServicioFidelizacion servicioFidelizacion,
            IDomainEventRegistry eventRegistry)
        {
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _servicioFidelizacion = servicioFidelizacion ?? throw new ArgumentNullException(nameof(servicioFidelizacion));
            _eventRegistry = eventRegistry ?? throw new ArgumentNullException(nameof(eventRegistry));
        }

        /// <summary>
        /// Maneja el evento de actualización de nivel de fidelización
        /// </summary>
        public async Task Handle(NivelFidelizacionActualizado evento, CancellationToken cancellationToken = default)
        {
            try
            {
                // Solo aplicamos descuentos automáticos si el nivel mejoró a Oro o Platino
                if (evento.NuevoNivel != NivelFidelizacion.Oro && evento.NuevoNivel != NivelFidelizacion.Platino)
                {
                    await _eventRegistry.RegisterAsync(evento, cancellationToken);
                    return;
                }

                // Obtener el cliente asociado a la tarjeta
                var cliente = await _clienteRepository.ObtenerPorIdAsync(evento.ClienteId, cancellationToken);
                if (cliente == null)
                {
                    await _eventRegistry.RegisterAsync(evento, cancellationToken);
                    return;
                }

                // Obtener comandas abiertas del cliente
                var comandasAbiertas = await _comandaRepository.ObtenerPorClienteAsync(cliente.Id, true, cancellationToken);
                if (!comandasAbiertas.Any())
                {
                    await _eventRegistry.RegisterAsync(evento, cancellationToken);
                    return;
                }

                int comandasActualizadas = 0;
                decimal descuentoAplicado = 0;

                // Aplicar descuentos en todas las comandas abiertas
                foreach (var comanda in comandasAbiertas)
                {
                    if (comanda.TieneDescuentoFidelizacion())
                    {
                        continue; // Ya tiene descuento aplicado
                    }

                    // Calcular descuento según nivel
                    decimal porcentajeDescuento = evento.NuevoNivel == NivelFidelizacion.Platino ? 0.15m : 0.10m;

                    // Aplicar descuento
                    comanda.AplicarDescuentoFidelizacion(porcentajeDescuento);
                    await _comandaRepository.ActualizarAsync(comanda, cancellationToken);

                    comandasActualizadas++;
                    descuentoAplicado += comanda.DescuentoFidelizacion ?? 0;
                }

                // Registrar éxito
                await _eventRegistry.RegisterAsync(evento, cancellationToken);
            }
            catch (Exception ex)
            {
                // Registrar error
                Console.WriteLine($"Error al aplicar descuento automático: {ex.Message}");
                await _eventRegistry.RegisterAsync(evento, cancellationToken);
                throw;
            }
        }
    }
} 