namespace RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de reservaciones
    /// </summary>
    public interface IReservacionRepository : IRepository<Reservacion>
    {
        /// <summary>
        /// Obtiene todas las reservaciones
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de todas las reservaciones</returns>
        Task<IEnumerable<Reservacion>> ObtenerTodasAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una reservación por su ID
        /// </summary>
        /// <param name="id">ID de la reservación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Reservación encontrada o null si no existe</returns>
        Task<Reservacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las reservaciones para una fecha específica
        /// </summary>
        /// <param name="fecha">Fecha de las reservaciones</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de reservaciones para la fecha especificada</returns>
        Task<IEnumerable<Reservacion>> ObtenerPorFechaAsync(DateTime fecha, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las reservaciones activas (confirmadas o pendientes) para una mesa en una fecha específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="fecha">Fecha de las reservaciones</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de reservaciones activas para la mesa y fecha</returns>
        Task<IEnumerable<Reservacion>> ObtenerReservacionesActivasPorMesaYFechaAsync(Guid mesaId, DateTime fecha, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las reservaciones por cliente
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de reservaciones del cliente</returns>
        Task<IEnumerable<Reservacion>> ObtenerPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las reservaciones pendientes y confirmadas de un cliente específico
        /// </summary>
        /// <param name="clienteId">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de reservaciones pendientes y confirmadas del cliente</returns>
        Task<IEnumerable<Reservacion>> ObtenerReservacionesPendientesPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene reservaciones por estado
        /// </summary>
        /// <param name="estado">Estado de la reservación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de reservaciones en el estado especificado</returns>
        Task<IEnumerable<Reservacion>> ObtenerPorEstadoAsync(EstadoReservacion estado, CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega una nueva reservación
        /// </summary>
        /// <param name="reservacion">Reservación a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task AgregarAsync(Reservacion reservacion, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una reservación existente
        /// </summary>
        /// <param name="reservacion">Reservación a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task ActualizarAsync(Reservacion reservacion, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina una reservación
        /// </summary>
        /// <param name="id">ID de la reservación a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe una reservación para una mesa en un rango de tiempo específico
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="fecha">Fecha de la reservación</param>
        /// <param name="horaInicio">Hora de inicio</param>
        /// <param name="horaFin">Hora de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe una reservación en el rango especificado, False en caso contrario</returns>
        Task<bool> ExisteReservacionEnRangoHorarioAsync(Guid mesaId, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene reservaciones por mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de reservaciones para la mesa especificada</returns>
        Task<IEnumerable<Reservacion>> ObtenerPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene reservaciones en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de reservaciones en el rango de fechas especificado</returns>
        Task<IEnumerable<Reservacion>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si una mesa está disponible en una fecha y hora específicas
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="fecha">Fecha de la reservación</param>
        /// <param name="hora">Hora de la reservación</param>
        /// <param name="duracionMinutos">Duración estimada en minutos (por defecto 90)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la mesa está disponible, False si está ocupada</returns>
        Task<bool> VerificarDisponibilidadMesaAsync(Guid mesaId, DateTime fecha, TimeSpan hora, int duracionMinutos = 90, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene mesas disponibles para una fecha, hora y cantidad de personas
        /// </summary>
        /// <param name="fecha">Fecha de la reservación</param>
        /// <param name="hora">Hora de la reservación</param>
        /// <param name="cantidadPersonas">Cantidad de personas</param>
        /// <param name="duracionMinutos">Duración estimada en minutos (por defecto 90)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de IDs de mesas disponibles</returns>
        Task<IEnumerable<Guid>> ObtenerMesasDisponiblesAsync(DateTime fecha, TimeSpan hora, int cantidadPersonas, int duracionMinutos = 90, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene reservaciones con paginación
        /// </summary>
        /// <param name="pagina">Número de página (base 0)</param>
        /// <param name="elementosPorPagina">Elementos por página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tupla con reservaciones y total de elementos</returns>
        Task<(IEnumerable<Reservacion> Reservaciones, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene estadísticas de reservaciones por día
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Diccionario con estadísticas por día</returns>
        Task<Dictionary<DateTime, int>> ObtenerEstadisticasPorDiaAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
    }
}
