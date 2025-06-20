using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Queries.ObtenerOrdenesCompraPaginadas;

/// <summary>
/// Query para obtener órdenes de compra con paginación
/// </summary>
public class ObtenerOrdenesCompraPaginadasQuery : IRequest<Result<PaginatedList<OrdenCompraDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public EstadoOrdenCompra? Estado { get; set; }
    public Guid? ProveedorId { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
} 