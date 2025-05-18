namespace RestaurantePro.Domain.Operaciones.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que cancela todas las reservaciones pendientes
    /// de un cliente cuando éste es desactivado del sistema.
    /// 
    /// Este manejador representa un ejemplo de comunicación entre contextos delimitados:
    /// - Comercial (Cliente) → Operaciones (Reservaciones)
    /// 
    /// El contexto Comercial emite el evento ClienteDesactivado cuando un cliente
    /// es desactivado, y este manejador en el contexto Operaciones reacciona
    /// cancelando automáticamente las reservaciones pendientes, manteniendo
    /// así la consistencia del sistema sin acoplar directamente ambos contextos.
    /// </summary>
    public class ClienteDesactivado_CancelarReservacionesPendientesHandler : IDomainEventHandler<Comercial.Clientes.Events.Cliente.ClienteDesactivado>
    {
        private readonly IReservacionRepository _reservacionRepository;
        private readonly IDomainEventLog _eventLog;
        
        public ClienteDesactivado_CancelarReservacionesPendientesHandler(
            IReservacionRepository reservacionRepository,
            IDomainEventLog eventLog)
        {
            _reservacionRepository = reservacionRepository ?? throw new ArgumentNullException(nameof(reservacionRepository));
            _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
        }
        
        /// <summary>
        /// Maneja el evento ClienteDesactivado, buscando y cancelando todas las reservaciones
        /// pendientes del cliente que ha sido desactivado.
        /// 
        /// Flujo de ejecución:
        /// 1. Obtiene todas las reservaciones pendientes o confirmadas del cliente
        /// 2. Para cada reservación, aplica la cancelación con el motivo "Cliente desactivado"
        /// 3. Actualiza las reservaciones en el repositorio
        /// 4. Registra los resultados en el log de eventos
        /// 
        /// Esta operación es idempotente y segura ante fallos, ya que verifica
        /// el estado de cada reservación antes de cancelarla.
        /// </summary>
        public async Task Handle(Comercial.Clientes.Events.Cliente.ClienteDesactivado evento, CancellationToken cancellationToken = default)
        {
            try
            {
                // Buscar todas las reservaciones pendientes del cliente
                var reservacionesPendientes = await _reservacionRepository.ObtenerReservacionesPendientesPorClienteIdAsync(
                    evento.ClienteId, cancellationToken);
                    
                if (reservacionesPendientes == null || !reservacionesPendientes.Any())
                {
                    // No hay reservaciones pendientes, no es necesario hacer nada
                    await _eventLog.LogEvent(evento, 
                        $"No se encontraron reservaciones pendientes para el cliente desactivado {evento.NombreCompleto} (ID: {evento.ClienteId})",
                        cancellationToken);
                    return;
                }
                
                // Registrar cuántas reservaciones se van a cancelar
                await _eventLog.LogEvent(evento, 
                    $"Se encontraron {reservacionesPendientes.Count()} reservaciones pendientes para el cliente " +
                    $"{evento.NombreCompleto} (ID: {evento.ClienteId}) que serán canceladas",
                    cancellationToken);
                
                int canceladas = 0;
                
                // Procesar cada reservación pendiente
                foreach (var reservacion in reservacionesPendientes)
                {
                    // Solo cancelar si está en estado pendiente o confirmada
                    if (reservacion.Estado == EstadoReservacion.Pendiente || 
                        reservacion.Estado == EstadoReservacion.Confirmada)
                    {
                        // Cancelar la reservación
                        reservacion.Cancelar("Cliente desactivado del sistema");
                        
                        // Actualizar la reservación en la base de datos
                        await _reservacionRepository.ActualizarAsync(reservacion);
                        
                        canceladas++;
                    }
                }
                
                // Registrar el resultado final
                await _eventLog.LogEvent(evento, 
                    $"Se cancelaron {canceladas} reservaciones pendientes del cliente {evento.NombreCompleto} (ID: {evento.ClienteId})",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventLog.LogEvent(evento, 
                    $"Error al cancelar reservaciones pendientes: {ex.Message}",
                    cancellationToken);
            }
        }
    }
} 