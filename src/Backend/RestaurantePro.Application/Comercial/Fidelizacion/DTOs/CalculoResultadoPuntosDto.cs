namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

/// <summary>
/// Resultado del cálculo de puntos
/// </summary>
public class CalculoResultadoPuntosDto
{
    /// <summary>
    /// ID de la tarjeta de fidelización
    /// </summary>
    public Guid TarjetaFidelizacionId { get; set; }

    /// <summary>
    /// Puntos base calculados
    /// </summary>
    public int PuntosBase { get; set; }

    /// <summary>
    /// Bonificación aplicada
    /// </summary>
    public int Bonificacion { get; set; }

    /// <summary>
    /// Total de puntos a acumular
    /// </summary>
    public int TotalPuntos => PuntosBase + Bonificacion;

    /// <summary>
    /// Multiplicador aplicado
    /// </summary>
    public decimal Multiplicador { get; set; } = 1.0m;

    /// <summary>
    /// Monto base para el cálculo
    /// </summary>
    public decimal MontoBase { get; set; }

    /// <summary>
    /// Tasa de conversión aplicada (puntos por peso)
    /// </summary>
    public decimal TasaConversion { get; set; }

    /// <summary>
    /// Promociones aplicadas
    /// </summary>
    public List<PromocionAplicadaDto> PromocionesAplicadas { get; set; } = new();

    /// <summary>
    /// Detalles del cálculo
    /// </summary>
    public string DetalleCalculo { get; set; } = string.Empty;

    /// <summary>
    /// Fecha del cálculo
    /// </summary>
    public DateTime FechaCalculo { get; set; } = DateTime.Now;

    /// <summary>
    /// Indica si el cálculo fue exitoso
    /// </summary>
    public bool EsExitoso { get; set; } = true;

    /// <summary>
    /// Mensajes de error si los hay
    /// </summary>
    public List<string> Errores { get; set; } = new();
}

/// <summary>
/// Promoción aplicada en el cálculo
/// </summary>
public class PromocionAplicadaDto
{
    /// <summary>
    /// ID de la promoción
    /// </summary>
    public Guid PromocionId { get; set; }

    /// <summary>
    /// Nombre de la promoción
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Puntos otorgados por la promoción
    /// </summary>
    public int PuntosOtorgados { get; set; }

    /// <summary>
    /// Tipo de promoción
    /// </summary>
    public string TipoPromocion { get; set; } = string.Empty;
} 