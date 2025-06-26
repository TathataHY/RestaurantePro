namespace RestaurantePro.Application.Comercial.Promociones.Commands.PausarPromocion;

/// <summary>
/// Command para pausar una promoción
/// </summary>
public class PausarPromocionCommand : IRequest<Result<PromocionDto>>
{
    /// <summary>
    /// ID de la promoción a pausar
    /// </summary>
    public Guid Id { get; set; }
} 