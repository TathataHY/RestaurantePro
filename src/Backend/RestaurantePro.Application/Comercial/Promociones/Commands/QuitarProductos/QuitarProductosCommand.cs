namespace RestaurantePro.Application.Comercial.Promociones.Commands.QuitarProductos;

/// <summary>
/// Command para quitar productos de una promoción
/// </summary>
public class QuitarProductosCommand : IRequest<Result<PromocionDto>>
{
    /// <summary>
    /// ID de la promoción
    /// </summary>
    public Guid PromocionId { get; set; }

    /// <summary>
    /// IDs de los productos a quitar
    /// </summary>
    public List<Guid> ProductosIds { get; set; } = new();
} 