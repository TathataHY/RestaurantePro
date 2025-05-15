using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones relacionadas con comandas
    /// </summary>
    public interface IComandaRepository : IBaseRepository<Comanda>
    {
        /// <summary>
        /// Obtiene comandas por estado
        /// </summary>
        /// <param name="estado">Estado de las comandas a buscar</param>
        /// <returns>Lista de comandas con el estado especificado</returns>
        Task<IReadOnlyList<Comanda>> GetByEstadoAsync(EstadoComanda estado);

        /// <summary>
        /// Obtiene comandas activas de una mesa específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <returns>Lista de comandas activas para la mesa</returns>
        Task<IReadOnlyList<Comanda>> GetActivasByMesaIdAsync(int mesaId);

        /// <summary>
        /// Obtiene comandas para un período de tiempo específico
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Lista de comandas en el período especificado</returns>
        Task<IReadOnlyList<Comanda>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Actualiza el estado de una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <returns>True si se actualizó correctamente, false si no se encontró la comanda</returns>
        Task<bool> ActualizarEstadoAsync(int comandaId, EstadoComanda nuevoEstado);

        /// <summary>
        /// Obtiene el número siguiente de comanda para generar
        /// </summary>
        /// <returns>Número de comanda siguiente</returns>
        Task<string> GenerarNumeroComandaAsync();

        /// <summary>
        /// Calcula el total de una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <returns>Total calculado de la comanda</returns>
        Task<decimal> CalcularTotalAsync(int comandaId);

        /// <summary>
        /// Obtiene comandas por usuario
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>Lista de comandas creadas por el usuario</returns>
        Task<IReadOnlyList<Comanda>> GetByUsuarioIdAsync(int usuarioId);
    }
} 