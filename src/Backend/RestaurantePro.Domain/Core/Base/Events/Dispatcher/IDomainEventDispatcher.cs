namespace RestaurantePro.Domain.Core.Base.Events.Dispatcher
{
    /// <summary>
    /// Interfaz para el servicio que distribuye eventos de dominio a sus manejadores
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Distribuye un evento de dominio a todos sus manejadores registrados
        /// </summary>
        /// <param name="evento">Evento a distribuir</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task Dispatch(DomainEvent evento, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Distribuye una colección de eventos de dominio a sus respectivos manejadores
        /// </summary>
        /// <param name="eventos">Colección de eventos a distribuir</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task DispatchAll(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default);
    }
} 