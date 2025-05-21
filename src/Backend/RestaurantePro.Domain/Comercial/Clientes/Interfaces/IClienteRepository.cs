using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;

namespace RestaurantePro.Domain.Comercial.Clientes.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de clientes
    /// </summary>
    public interface IClienteRepository : IRepository<Cliente>
    {
        /// <summary>
        /// Obtiene un cliente por su ID
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cliente encontrado o null si no existe</returns>
        new Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene un cliente por su email
        /// </summary>
        /// <param name="email">Email del cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Cliente encontrado o null si no existe</returns>
        Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes por nombre (búsqueda parcial)
        /// </summary>
        /// <param name="nombre">Nombre completo o parcial</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes que coinciden con el criterio</returns>
        Task<IEnumerable<Cliente>> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes por segmento
        /// </summary>
        /// <param name="segmento">Segmento de cliente</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes del segmento especificado</returns>
        Task<IEnumerable<Cliente>> ObtenerPorSegmentoAsync(SegmentoCliente segmento, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes activos o inactivos
        /// </summary>
        /// <param name="activos">Indica si se deben obtener clientes activos o inactivos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes según el criterio</returns>
        Task<IEnumerable<Cliente>> ObtenerPorEstadoActivoAsync(bool activos, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes con tarjeta de fidelización
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes con tarjeta de fidelización</returns>
        Task<IEnumerable<Cliente>> ObtenerConTarjetaFidelizacionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los clientes más frecuentes (por número de visitas)
        /// </summary>
        /// <param name="cantidad">Cantidad de clientes a obtener</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes más frecuentes</returns>
        Task<IEnumerable<Cliente>> ObtenerClientesMasFrecuentesAsync(int cantidad, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes con una cantidad mínima de puntos acumulados
        /// </summary>
        /// <param name="puntosMinimos">Cantidad mínima de puntos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes con los puntos especificados o más</returns>
        Task<IEnumerable<Cliente>> ObtenerPorPuntosMinimosAsync(int puntosMinimos, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes con paginación
        /// </summary>
        /// <param name="pagina">Número de página (base 0)</param>
        /// <param name="elementosPorPagina">Elementos por página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tupla con clientes y total de elementos</returns>
        new Task<(IEnumerable<Cliente> Clientes, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene clientes registrados en un rango de fechas
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial</param>
        /// <param name="fechaFin">Fecha final</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de clientes registrados en el rango de fechas</returns>
        Task<IEnumerable<Cliente>> ObtenerPorRangoFechasRegistroAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
    }
}
