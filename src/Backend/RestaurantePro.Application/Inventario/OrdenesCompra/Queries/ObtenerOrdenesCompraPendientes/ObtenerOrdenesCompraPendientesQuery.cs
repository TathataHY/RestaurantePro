using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Queries.ObtenerOrdenesCompraPendientes;

/// <summary>
/// Query para obtener órdenes de compra pendientes
/// </summary>
public class ObtenerOrdenesCompraPendientesQuery : IRequest<Result<List<OrdenCompraDto>>>
{
    public Guid? ProveedorId { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? Limite { get; set; }
} 