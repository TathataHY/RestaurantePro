namespace RestaurantePro.Application.Comercial.Promociones.Commands.ActivarPromocion;

/// <summary>
/// Command para activar una promoción
/// </summary>
public class ActivarPromocionCommand : IRequest<Result<PromocionDto>>
{
    /// <summary>
    /// ID de la promoción a activar
    /// </summary>
    public Guid Id { get; set; }
} 