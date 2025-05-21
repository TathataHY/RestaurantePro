using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Application.Proveedores.DTOs;
using RestaurantePro.Application.Proveedores.Mappers;
using RestaurantePro.Domain.Proveedores.Interfaces;

namespace RestaurantePro.Application.Proveedores.Services
{
    /// <summary>
    /// Servicio de aplicación para proveedores
    /// </summary>
    public class ProveedoresAppService : IProveedoresAppService
    {
        private readonly IProveedorRepository _proveedorRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="proveedorRepository">Repositorio de proveedores</param>
        public ProveedoresAppService(IProveedorRepository proveedorRepository)
        {
            _proveedorRepository = proveedorRepository ?? throw new ArgumentNullException(nameof(proveedorRepository));
        }

        /// <summary>
        /// Obtiene estadísticas de proveedores
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>DTO con estadísticas de proveedores</returns>
        public async Task<EstadisticasProveedoresDto> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
        {
            var estadisticasDominio = await _proveedorRepository.ObtenerEstadisticasAsync(cancellationToken);
            return estadisticasDominio.ToDto();
        }
    }

    /// <summary>
    /// Interfaz para el servicio de aplicación de proveedores
    /// </summary>
    public interface IProveedoresAppService
    {
        /// <summary>
        /// Obtiene estadísticas de proveedores
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>DTO con estadísticas de proveedores</returns>
        Task<EstadisticasProveedoresDto> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default);
    }
} 