
namespace RestaurantePro.Domain.Comercial.Clientes.Interfaces
{
    /// <summary>
    /// Repositorio para la gestión del historial de puntos
    /// </summary>
    public interface IHistorialPuntosRepository
    {
        /// <summary>
        /// Obtiene un registro de historial por su ID
        /// </summary>
        /// <param name="id">ID del registro</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Registro encontrado o null si no existe</returns>
        Task<HistorialPuntos> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los registros de una tarjeta de fidelización
        /// </summary>
        /// <param name="tarjetaId">ID de la tarjeta</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de registros de la tarjeta</returns>
        Task<IEnumerable<HistorialPuntos>> ObtenerPorTarjetaIdAsync(Guid tarjetaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los registros filtrados por tipo de operación
        /// </summary>
        /// <param name="tarjetaId">ID de la tarjeta</param>
        /// <param name="tipoOperacion">Tipo de operación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de registros que cumplen con el filtro</returns>
        Task<IEnumerable<HistorialPuntos>> ObtenerPorTipoOperacionAsync(Guid tarjetaId, TipoOperacionPuntos tipoOperacion, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los registros de puntos en un rango de fechas
        /// </summary>
        /// <param name="tarjetaId">ID de la tarjeta</param>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de registros dentro del rango de fechas</returns>
        Task<IEnumerable<HistorialPuntos>> ObtenerPorRangoFechasAsync(Guid tarjetaId, DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el total de puntos por tipo de operación en un periodo
        /// </summary>
        /// <param name="tarjetaId">ID de la tarjeta</param>
        /// <param name="tipoOperacion">Tipo de operación</param>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Total de puntos del tipo de operación en el periodo</returns>
        Task<int> ObtenerTotalPuntosPorTipoAsync(Guid tarjetaId, TipoOperacionPuntos tipoOperacion, DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega un nuevo registro de historial
        /// </summary>
        /// <param name="historial">Registro a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task AgregarAsync(HistorialPuntos historial, CancellationToken cancellationToken = default);

        /// <summary>
        /// Guarda los cambios en la unidad de trabajo
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea asíncrona</returns>
        Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
    }
}
