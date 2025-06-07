using System;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Mesas.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de mesas
    /// </summary>
    public interface IMesaService
    {
        /// <summary>
        /// Libera una mesa para que pueda ser utilizada por otros clientes
        /// </summary>
        /// <param name="mesaId">ID de la mesa a liberar</param>
        /// <param name="usuarioId">ID del usuario que realiza la operación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result> LiberarMesaAsync(Guid mesaId, Guid usuarioId, CancellationToken cancellationToken);
    }
} 