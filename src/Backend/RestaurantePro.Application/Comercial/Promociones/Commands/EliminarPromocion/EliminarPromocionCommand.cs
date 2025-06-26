namespace RestaurantePro.Application.Comercial.Promociones.Commands.EliminarPromocion;

/// <summary>
/// Command para eliminar (cancelar) una promoción
/// </summary>
public class EliminarPromocionCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID de la promoción a eliminar
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Motivo de la cancelación
    /// </summary>
    public string Motivo { get; set; } = string.Empty;
} 