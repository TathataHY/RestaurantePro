namespace RestaurantePro.Domain.Comercial.EventHandlers
{
    /// <summary>
    /// Manejador de eventos para acumular puntos de fidelización cuando se finaliza una comanda.
    /// Este es el componente que coordina la interacción entre los agregados Cliente, Comanda y TarjetaFidelizacion.
    /// </summary>
    public class ComandaFinalizada_AcumularPuntosHandler : IDomainEventHandler<ComandaFinalizada>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IComandaRepository _comandaRepository;
        private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
        private readonly IServicioFidelizacion _servicioFidelizacion;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDomainEventLog _eventLog;
        
        public ComandaFinalizada_AcumularPuntosHandler(
            IClienteRepository clienteRepository,
            IComandaRepository comandaRepository,
            ITarjetaFidelizacionRepository tarjetaRepository,
            IServicioFidelizacion servicioFidelizacion,
            IDateTimeService dateTimeService,
            IDomainEventLog eventLog)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
            _servicioFidelizacion = servicioFidelizacion ?? throw new ArgumentNullException(nameof(servicioFidelizacion));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
        }
        
        /// <summary>
        /// Maneja el evento ComandaFinalizada actualizando los puntos de fidelización del cliente
        /// </summary>
        public async Task Handle(ComandaFinalizada evento, CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Obtener la comanda completa
                var comanda = await _comandaRepository.ObtenerPorIdAsync(evento.ComandaId, cancellationToken);
                if (comanda == null || !comanda.ClienteId.HasValue || comanda.Estado != EstadoComanda.Finalizada)
                {
                    await _eventLog.LogEvent(evento, 
                        $"No se acumularon puntos: Comanda con ID {evento.ComandaId} no encontrada, sin cliente asociado o no finalizada",
                        cancellationToken);
                    return;
                }
                    
                // 2. Obtener el cliente
                var cliente = await _clienteRepository.ObtenerPorIdAsync(comanda.ClienteId.Value, cancellationToken);
                if (cliente == null || !cliente.EstaActivo)
                {
                    await _eventLog.LogEvent(evento, 
                        $"No se acumularon puntos: Cliente con ID {comanda.ClienteId.Value} no encontrado o inactivo",
                        cancellationToken);
                    return;
                }
                
                // 3. Usar el servicio de fidelización para acumular puntos
                // Este servicio se encarga de obtener la tarjeta y acumular los puntos basados en los criterios de negocio
                await _servicioFidelizacion.AcumularPuntosAsync(
                    cliente.Id, 
                    comanda.Id, 
                    evento.Total);
                
                await _eventLog.LogEvent(evento, 
                    $"Se procesó acumulación de puntos para el cliente {cliente.Nombre.NombreCompleto} por su comanda de {evento.Total:C2}",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventLog.LogEvent(evento, 
                    $"Error al acumular puntos: {ex.Message}",
                    cancellationToken);
            }
        }
    }
} 