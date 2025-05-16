
namespace RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de reservaciones
    /// </summary>
    public interface IReservacionRepository
    {
        /// <summary>
        /// Obtiene todas las reservaciones
        /// </summary>
        Task<IEnumerable<Reservacion>> ObtenerTodasAsync();

        /// <summary>
        /// Obtiene una reservación por su ID
        /// </summary>
        Task<Reservacion> ObtenerPorIdAsync(Guid id);

        /// <summary>
        /// Obtiene las reservaciones para una fecha específica
        /// </summary>
        Task<IEnumerable<Reservacion>> ObtenerPorFechaAsync(DateTime fecha);

        /// <summary>
        /// Obtiene las reservaciones activas (confirmadas o pendientes) para una mesa en una fecha específica
        /// </summary>
        Task<IEnumerable<Reservacion>> ObtenerReservacionesActivasPorMesaYFechaAsync(Guid mesaId, DateTime fecha);

        /// <summary>
        /// Obtiene las reservaciones por cliente
        /// </summary>
        Task<IEnumerable<Reservacion>> ObtenerPorClienteAsync(Guid clienteId);

        /// <summary>
        /// Obtiene reservaciones por estado
        /// </summary>
        Task<IEnumerable<Reservacion>> ObtenerPorEstadoAsync(EstadoReservacion estado);

        /// <summary>
        /// Agrega una nueva reservación
        /// </summary>
        Task AgregarAsync(Reservacion reservacion);

        /// <summary>
        /// Actualiza una reservación existente
        /// </summary>
        Task ActualizarAsync(Reservacion reservacion);

        /// <summary>
        /// Elimina una reservación
        /// </summary>
        Task EliminarAsync(Guid id);

        /// <summary>
        /// Verifica si existe una reservación para una mesa en un rango de tiempo específico
        /// </summary>
        Task<bool> ExisteReservacionEnRangoHorarioAsync(Guid mesaId, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin);
    }
}
