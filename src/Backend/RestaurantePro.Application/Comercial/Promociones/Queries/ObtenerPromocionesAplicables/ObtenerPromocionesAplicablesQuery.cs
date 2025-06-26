using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.Queries.ObtenerPromocionesAplicables;

/// <summary>
/// Query para obtener promociones aplicables a un cliente, monto, productos, etc.
/// </summary>
public class ObtenerPromocionesAplicablesQuery : IRequest<Result<List<PromocionDto>>>
{
    public Guid? ClienteId { get; set; }
    public decimal? Monto { get; set; }
    public List<Guid>? ProductosIds { get; set; }
    public DateTime? Fecha { get; set; }
    public bool? SoloVigentes { get; set; }
    public bool? SoloAcumulables { get; set; }
    public TipoPromocion? Tipo { get; set; }
} 