using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Reportes.DTOs;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerRecomendacionesCompra;

/// <summary>
/// Query para obtener recomendaciones de compra basadas en el inventario
/// </summary>
public class ObtenerRecomendacionesCompraQuery : IRequest<Result<List<RecomendacionCompraDto>>>
{
    public int DiasProyeccion { get; set; } = 30;
} 