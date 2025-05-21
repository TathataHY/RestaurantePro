using System.Linq;
using RestaurantePro.Application.Proveedores.DTOs;
using RestaurantePro.Domain.Proveedores.DTOs;

namespace RestaurantePro.Application.Proveedores.Mappers
{
    /// <summary>
    /// Clase para mapear entre entidades de dominio y DTOs de proveedores
    /// </summary>
    public static class ProveedorMapper
    {
        /// <summary>
        /// Mapea de estadísticas de dominio a DTO
        /// </summary>
        /// <param name="estadisticas">Estadísticas de proveedores del dominio</param>
        /// <returns>DTO de estadísticas de proveedores</returns>
        public static EstadisticasProveedoresDto ToDto(this EstadisticasProveedores estadisticas)
        {
            return new EstadisticasProveedoresDto
            {
                TotalProveedores = estadisticas.TotalProveedores,
                ProveedoresActivos = estadisticas.ProveedoresActivos,
                OrdenesEnPeriodo = estadisticas.OrdenesEnPeriodo,
                ValorTotalOrdenesEnPeriodo = estadisticas.ValorTotalOrdenesEnPeriodo,
                TiempoPromedioEntrega = estadisticas.TiempoPromedioEntrega,
                ProveedoresConOrdenesPendientes = estadisticas.ProveedoresConOrdenesPendientes,
                FechaCalculo = estadisticas.FechaCalculo,
                TopProveedoresPorVolumen = estadisticas.TopProveedoresPorVolumen.Select(p => new ProveedorEstadisticaDto
                {
                    ProveedorId = p.ProveedorId,
                    NombreProveedor = p.NombreProveedor,
                    TotalOrdenes = p.TotalOrdenes,
                    ValorTotalOrdenes = p.ValorTotalOrdenes,
                    TiempoPromedioEntrega = p.TiempoPromedioEntrega
                }).ToList()
            };
        }
    }
} 