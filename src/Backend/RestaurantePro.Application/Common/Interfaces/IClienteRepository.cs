using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Comercial.Clientes.Entities;

namespace RestaurantePro.Application.Common.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de clientes en la capa de aplicación
    /// </summary>
    public interface IClienteRepository
    {
        /// <summary>
        /// Obtiene un cliente por su ID
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cliente encontrado o null si no existe</returns>
        Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todos los clientes
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de todos los clientes</returns>
        Task<IEnumerable<Cliente>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los clientes activos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes activos</returns>
        Task<IEnumerable<Cliente>> ObtenerActivosAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene un cliente por su email
        /// </summary>
        /// <param name="email">Email del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cliente encontrado o null si no existe</returns>
        Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Agrega un nuevo cliente
        /// </summary>
        /// <param name="cliente">Cliente a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cliente agregado con su ID asignado</returns>
        Task<Cliente> AgregarAsync(Cliente cliente, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Actualiza un cliente existente
        /// </summary>
        /// <param name="cliente">Cliente a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task ActualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina un cliente por su ID
        /// </summary>
        /// <param name="id">ID del cliente a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
    }
} 