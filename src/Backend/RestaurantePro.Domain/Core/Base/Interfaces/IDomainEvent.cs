
namespace RestaurantePro.Domain.Core.Base.Interfaces
{
    /// <summary>
    /// Interfaz base para todos los eventos de dominio
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        DateTime OccurredOn { get; }
    }
} 