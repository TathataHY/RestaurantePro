namespace RestaurantePro.Domain.Inventario.EventHandlers
{
    /// <summary>
    /// Manejador de evento que actualiza el historial del cliente cuando se finaliza una comanda
    /// Demuestra la comunicación entre agregados a través de eventos de dominio
    /// </summary>
    public class ComandaFinalizada_ActualizarHistorialClienteHandler : IDomainEventHandler<ComandaFinalizada>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IComandaRepository _comandaRepository;
        
        public ComandaFinalizada_ActualizarHistorialClienteHandler(
            IClienteRepository clienteRepository,
            IComandaRepository comandaRepository)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
        }
        
        /// <summary>
        /// Maneja el evento ComandaFinalizada registrando una visita en el cliente asociado
        /// </summary>
        public async Task Handle(ComandaFinalizada evento, CancellationToken cancellationToken)
        {
            // Obtener la comanda completa a partir del ID en el evento
            var comanda = await _comandaRepository.ObtenerPorIdAsync(evento.ComandaId, cancellationToken);
            
            // Si la comanda no existe o no tiene cliente asociado, no hacer nada
            if (comanda == null || !comanda.ClienteId.HasValue || comanda.ClienteId.Value == Guid.Empty)
                return;
                
            // Obtener el cliente por ID
            var cliente = await _clienteRepository.ObtenerPorIdAsync(comanda.ClienteId.Value, cancellationToken);
            
            // Si el cliente no existe o no está activo, no hacer nada
            if (cliente == null || !cliente.EstaActivo)
                return;
                
            // Actualizar el cliente registrando la visita
            cliente.RegistrarVisita();
            
            // Persistir los cambios
            await _clienteRepository.ActualizarAsync(cliente, cancellationToken);
            
            // Nota: Este evento es un ejemplo de comunicación entre agregados
            // Cliente y Comanda son agregados distintos que pertenecen a contextos diferentes
            // La comunicación se realiza a través de IDs y eventos de dominio
        }
    }
} 