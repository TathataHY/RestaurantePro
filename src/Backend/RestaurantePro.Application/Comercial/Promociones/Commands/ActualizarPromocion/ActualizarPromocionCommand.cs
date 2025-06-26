using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.ActualizarPromocion;

/// <summary>
/// Command para actualizar una promoción existente
/// </summary>
public class ActualizarPromocionCommand : IRequest<Result<PromocionDto>>
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public TipoPromocion Tipo { get; set; }
    public decimal ValorDescuento { get; set; }
    public decimal MontoMinimo { get; set; } = 0;
    public int PuntosRequeridos { get; set; } = 0;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int? MaximoUsos { get; set; }
    public bool EsAcumulable { get; set; } = false;
    public List<DayOfWeek>? DiasValidos { get; set; }
    public int Prioridad { get; set; } = 0;
    public string Condiciones { get; set; } = string.Empty;
    public List<Guid>? ProductosAplicablesIds { get; set; }
    public List<Guid>? CategoriasAplicablesIds { get; set; }
} 