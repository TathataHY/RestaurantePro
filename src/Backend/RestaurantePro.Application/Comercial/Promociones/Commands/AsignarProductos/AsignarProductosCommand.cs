namespace RestaurantePro.Application.Comercial.Promociones.Commands.AsignarProductos;

/// <summary>
/// Command para asignar productos a una promoción
/// </summary>
public class AsignarProductosCommand : IRequest<Result<PromocionDto>>
{
    /// <summary>
    /// ID de la promoción
    /// </summary>
    public Guid PromocionId { get; set; }

    /// <summary>
    /// IDs de los productos a asignar
    /// </summary>
    public List<Guid> ProductosIds { get; set; } = new();
} 