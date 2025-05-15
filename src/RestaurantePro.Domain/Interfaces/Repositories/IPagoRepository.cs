using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones relacionadas con pagos
    /// </summary>
    public interface IPagoRepository : IBaseRepository<Pago>
    {
        /// <summary>
        /// Obtiene pagos por comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <returns>Lista de pagos asociados a la comanda</returns>
        Task<IReadOnlyList<Pago>> GetByComandaIdAsync(int comandaId);

        /// <summary>
        /// Obtiene pagos por estado
        /// </summary>
        /// <param name="estado">Estado de los pagos a buscar</param>
        /// <returns>Lista de pagos con el estado especificado</returns>
        Task<IReadOnlyList<Pago>> GetByEstadoAsync(EstadoPago estado);

        /// <summary>
        /// Obtiene pagos por método de pago
        /// </summary>
        /// <param name="metodoPago">Método de pago a buscar</param>
        /// <returns>Lista de pagos con el método especificado</returns>
        Task<IReadOnlyList<Pago>> GetByMetodoPagoAsync(MetodoPago metodoPago);

        /// <summary>
        /// Obtiene pagos para un período de tiempo específico
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Lista de pagos en el período especificado</returns>
        Task<IReadOnlyList<Pago>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Calcula el total pagado de una comanda
        /// </summary>
        /// <param name="comandaId">ID de la comanda</param>
        /// <returns>Total pagado</returns>
        Task<decimal> CalcularTotalPagadoAsync(int comandaId);

        /// <summary>
        /// Actualiza el estado de un pago
        /// </summary>
        /// <param name="pagoId">ID del pago</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <returns>True si se actualizó correctamente, false si no se encontró el pago</returns>
        Task<bool> ActualizarEstadoAsync(int pagoId, EstadoPago nuevoEstado);
    }
} 