using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;

namespace RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de preparaciones diarias
    /// </summary>
    public interface IPreparacionRepository : IRepository<PreparacionDiaria>
    {
        /// <summary>
        /// Obtiene todas las preparaciones del día actual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de todas las preparaciones del día</returns>
        Task<IEnumerable<PreparacionDiaria>> ObtenerPreparacionesDelDiaAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene preparaciones disponibles para un producto específico
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de preparaciones disponibles del producto</returns>
        Task<IEnumerable<PreparacionDiaria>> ObtenerPreparacionesDisponiblesPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todas las preparaciones por su estado
        /// </summary>
        /// <param name="estado">Estado de las preparaciones a buscar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de preparaciones con el estado especificado</returns>
        Task<IEnumerable<PreparacionDiaria>> ObtenerPorEstadoAsync(EstadoPreparacion estado, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene preparaciones que vencen en un rango de tiempo específico
        /// </summary>
        /// <param name="horasAnticipacion">Horas de anticipación para considerar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de preparaciones por vencer</returns>
        Task<IEnumerable<PreparacionDiaria>> ObtenerPorVencerAsync(int horasAnticipacion = 2, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene preparaciones por chef
        /// </summary>
        /// <param name="chefId">ID del chef</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de preparaciones realizadas por el chef</returns>
        Task<IEnumerable<PreparacionDiaria>> ObtenerPorChefAsync(Guid chefId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene estadísticas de preparaciones del día actual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Estadísticas de preparaciones</returns>
        Task<EstadisticasPreparaciones> ObtenerEstadisticasDelDiaAsync(CancellationToken cancellationToken = default);
    }
} 