using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones relacionadas con mesas
    /// </summary>
    public interface IMesaRepository : IBaseRepository<Mesa>
    {
        /// <summary>
        /// Obtiene mesas por estado
        /// </summary>
        /// <param name="estado">Estado de las mesas a buscar</param>
        /// <returns>Lista de mesas con el estado especificado</returns>
        Task<IReadOnlyList<Mesa>> GetByEstadoAsync(EstadoMesa estado);

        /// <summary>
        /// Obtiene mesas disponibles para una fecha y hora específica (considernado reservaciones)
        /// </summary>
        /// <param name="fecha">Fecha y hora para verificar disponibilidad</param>
        /// <param name="duracionMinutos">Duración en minutos para la reservación</param>
        /// <param name="numeroPersonas">Número de personas a acomodar</param>
        /// <returns>Lista de mesas disponibles</returns>
        Task<IReadOnlyList<Mesa>> GetDisponiblesParaReservacionAsync(System.DateTime fecha, int duracionMinutos, int numeroPersonas);

        /// <summary>
        /// Actualiza el estado de una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <returns>True si se actualizó correctamente, false si no se encontró la mesa</returns>
        Task<bool> ActualizarEstadoAsync(int mesaId, EstadoMesa nuevoEstado);
    }
} 