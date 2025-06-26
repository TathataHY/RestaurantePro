namespace RestaurantePro.Application.Comercial.Promociones.Queries.ObtenerPromocionPorId;

/// <summary>
/// Query para obtener una promoción específica por ID
/// </summary>
public class ObtenerPromocionPorIdQuery : IRequest<Result<PromocionDto>>
{
    /// <summary>
    /// ID de la promoción a obtener
    /// </summary>
    public Guid Id { get; set; }
} 