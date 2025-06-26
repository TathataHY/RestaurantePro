using RestaurantePro.Domain.Comercial.Promociones;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
namespace RestaurantePro.Application.Comercial.Promociones.Commands.CrearPromocion;

/// <summary>
/// Command para crear una nueva promoción
/// </summary>
public class CrearPromocionCommand : IRequest<Result<PromocionDto>>
{
    /// <summary>
    /// Código único de la promoción
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo de la promoción
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada de la promoción
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de promoción
    /// </summary>
    public TipoPromocion Tipo { get; set; }

    /// <summary>
    /// Valor del descuento o beneficio
    /// </summary>
    public decimal ValorDescuento { get; set; }

    /// <summary>
    /// Monto mínimo de compra para aplicar la promoción
    /// </summary>
    public decimal MontoMinimo { get; set; } = 0;

    /// <summary>
    /// Puntos de fidelización requeridos para canjear la promoción
    /// </summary>
    public int PuntosRequeridos { get; set; } = 0;

    /// <summary>
    /// Fecha de inicio de la promoción
    /// </summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>
    /// Fecha de fin de la promoción
    /// </summary>
    public DateTime FechaFin { get; set; }

    /// <summary>
    /// Número máximo de veces que se puede usar la promoción (null = ilimitado)
    /// </summary>
    public int? MaximoUsos { get; set; }

    /// <summary>
    /// Indica si la promoción es acumulable con otras
    /// </summary>
    public bool EsAcumulable { get; set; } = false;

    /// <summary>
    /// Días de la semana en que es válida la promoción
    /// </summary>
    public List<DayOfWeek>? DiasValidos { get; set; }

    /// <summary>
    /// Prioridad de la promoción (menor número = mayor prioridad)
    /// </summary>
    public int Prioridad { get; set; } = 0;

    /// <summary>
    /// Condiciones específicas de la promoción
    /// </summary>
    public string? Condiciones { get; set; }

    /// <summary>
    /// IDs de los productos a los que aplica la promoción
    /// </summary>
    public List<Guid>? ProductosAplicablesIds { get; set; }

    /// <summary>
    /// IDs de las categorías a las que aplica la promoción
    /// </summary>
    public List<Guid>? CategoriasAplicablesIds { get; set; }
} 