namespace RestaurantePro.Domain.Core.Base.Events.Handlers
{
    /// <summary>
    /// Interfaz genérica para manejar eventos de dominio específicos
    /// </summary>
    /// <typeparam name="T">Tipo de evento de dominio a manejar</typeparam>
    public interface IDomainEventHandler<in T> where T : DomainEvent
    {
        /// <summary>
        /// Maneja el evento de dominio especificado
        /// </summary>
        /// <param name="evento">Evento a manejar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task Handle(T evento, CancellationToken cancellationToken = default);
    }
} 