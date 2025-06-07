using System;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de gestión de mesas
    /// </summary>
    public interface IMesaService
    {
        /// <summary>
        /// Libera una mesa específica
        /// </summary>
        /// <param name="mesaId">ID de la mesa a liberar</param>
        /// <param name="usuarioId">ID del usuario que realiza la operación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información de la mesa liberada</returns>
        Task<Result<LiberarMesaResult>> LiberarMesaAsync(Guid mesaId, Guid usuarioId, CancellationToken cancellationToken);

        /// <summary>
        /// Cambia el estado de una mesa
        /// </summary>
        /// <param name="mesaId">ID de la mesa</param>
        /// <param name="nuevoEstado">Nuevo estado para la mesa</param>
        /// <param name="usuarioId">ID del usuario que realiza la operación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información de la mesa actualizada</returns>
        Task<Result<MesaDto>> CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, Guid usuarioId, CancellationToken cancellationToken);

        /// <summary>
        /// Asigna una mesa a un cliente o grupo
        /// </summary>
        /// <param name="mesaId">ID de la mesa a asignar</param>
        /// <param name="clienteId">ID del cliente (opcional)</param>
        /// <param name="usuarioId">ID del usuario que realiza la operación</param>
        /// <param name="numeroPersonas">Número de personas en la mesa</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado con información de la mesa asignada</returns>
        Task<Result<MesaDto>> AsignarMesaAsync(Guid mesaId, Guid? clienteId, Guid usuarioId, int numeroPersonas, CancellationToken cancellationToken);
    }
} 