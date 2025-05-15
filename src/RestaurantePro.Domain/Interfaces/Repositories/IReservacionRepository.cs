using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones relacionadas con reservaciones
    /// </summary>
    public interface IReservacionRepository : IBaseRepository<Reservacion>
    {
        /// <summary>
        /// Obtiene reservaciones por estado
        /// </summary>
        /// <param name="estado">Estado de las reservaciones a buscar</param>
        /// <returns>Lista de reservaciones con el estado especificado</returns>
        Task<IReadOnlyList<Reservacion>> GetByEstadoAsync(EstadoReservacion estado);

        /// <summary>
        /// Obtiene reservaciones por mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Lista de reservaciones para la mesa</returns>
        Task<IReadOnlyList<Reservacion>> GetByMesaIdAsync(int mesaId);

        /// <summary>
        /// Obtiene reservaciones para un período de tiempo específico
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Lista de reservaciones en el período especificado</returns>
        Task<IReadOnlyList<Reservacion>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene reservaciones para una fecha específica
        /// </summary>
        /// <param name="fecha">Fecha a consultar</param>
        /// <returns>Lista de reservaciones para la fecha</returns>
        Task<IReadOnlyList<Reservacion>> GetByFechaAsync(DateTime fecha);

        /// <summary>
        /// Verifica si una mesa está disponible para una fecha y hora específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="fecha">Fecha y hora para verificar</param>
        /// <param name="duracionMinutos">Duración en minutos de la reservación</param>
        /// <returns>True si la mesa está disponible, false en caso contrario</returns>
        Task<bool> VerificarDisponibilidadMesaAsync(int mesaId, DateTime fecha, int duracionMinutos);

        /// <summary>
        /// Actualiza el estado de una reservación
        /// </summary>
        /// <param name="reservacionId">ID de la reservación</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <returns>True si se actualizó correctamente, false si no se encontró la reservación</returns>
        Task<bool> ActualizarEstadoAsync(int reservacionId, EstadoReservacion nuevoEstado);

        /// <summary>
        /// Busca reservaciones por nombre de cliente o email
        /// </summary>
        /// <param name="busqueda">Texto a buscar</param>
        /// <returns>Lista de reservaciones que coinciden con la búsqueda</returns>
        Task<IReadOnlyList<Reservacion>> BuscarAsync(string busqueda);
    }
} 