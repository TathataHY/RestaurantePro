using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Queries.ObtenerOrdenCompraPorId;

/// <summary>
/// Query para obtener una orden de compra específica por ID
/// </summary>
public class ObtenerOrdenCompraPorIdQuery : IRequest<Result<OrdenCompraDto>>
{
    public Guid Id { get; set; }
} 