using System.Linq;
using RestaurantePro.Application.Proveedores.DTOs;
using RestaurantePro.Domain.Proveedores.Results;

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
        /// <param name="resultado">Resultado de estadísticas de proveedores del dominio</param>
        /// <returns>DTO de estadísticas de proveedores</returns>
        public static EstadisticasProveedoresDto ToDto(this ResultadoEstadisticasProveedores resultado)
        {
            return new EstadisticasProveedoresDto
            {
                TotalProveedores = resultado.TotalProveedores,
                ProveedoresActivos = resultado.ProveedoresActivos,
                OrdenesEnPeriodo = resultado.OrdenesEnPeriodo,
                ValorTotalOrdenesEnPeriodo = resultado.ValorTotalOrdenesEnPeriodo,
                TiempoPromedioEntrega = resultado.TiempoPromedioEntrega,
                ProveedoresConOrdenesPendientes = resultado.ProveedoresConOrdenesPendientes,
                FechaCalculo = resultado.FechaCalculo,
                TopProveedoresPorVolumen = resultado.TopProveedoresPorVolumen.Select(p => new ProveedorEstadisticaDto
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