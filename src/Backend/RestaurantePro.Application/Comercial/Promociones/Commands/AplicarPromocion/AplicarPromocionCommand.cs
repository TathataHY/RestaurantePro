namespace RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;

/// <summary>
/// Command para aplicar una promoción a una factura o comanda
/// Permite aplicar descuentos promocionales con validaciones de negocio
/// </summary>
public class AplicarPromocionCommand : IRequest<Result<AplicarPromocionDto>>
{
    /// <summary>
    /// ID de la promoción a aplicar
    /// </summary>
    public Guid PromocionId { get; set; }

    /// <summary>
    /// Código de promoción (alternativo al ID)
    /// </summary>
    public string? CodigoPromocion { get; set; }

    /// <summary>
    /// ID de la factura donde aplicar la promoción
    /// </summary>
    public Guid? FacturaId { get; set; }

    /// <summary>
    /// ID de la comanda donde aplicar la promoción
    /// </summary>
    public Guid? ComandaId { get; set; }

    /// <summary>
    /// IDs de productos específicos para la promoción
    /// </summary>
    public List<Guid>? ProductosIds { get; set; }

    /// <summary>
    /// ID del cliente que aplica la promoción
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Tipo de aplicación de la promoción
    /// </summary>
    public TipoAplicacionPromocion TipoAplicacion { get; set; }

    /// <summary>
    /// Usuario que autoriza la aplicación
    /// </summary>
    public Guid? AutorizadoPor { get; set; }

    /// <summary>
    /// Notas adicionales sobre la aplicación
    /// </summary>
    public string? NotasAplicacion { get; set; }

    /// <summary>
    /// Indica si se debe validar restricciones de cliente
    /// </summary>
    public bool ValidarRestriccionesCliente { get; set; } = true;

    /// <summary>
    /// Indica si se debe validar límites de uso
    /// </summary>
    public bool ValidarLimitesUso { get; set; } = true;

    /// <summary>
    /// Datos adicionales de la aplicación
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    public AplicarPromocionCommand(Guid promocionId, TipoAplicacionPromocion tipoAplicacion)
    {
        PromocionId = promocionId;
        TipoAplicacion = tipoAplicacion;
    }

    public AplicarPromocionCommand(string codigoPromocion, TipoAplicacionPromocion tipoAplicacion)
    {
        CodigoPromocion = codigoPromocion;
        TipoAplicacion = tipoAplicacion;
    }

    public AplicarPromocionCommand() { }
}

/// <summary>
/// Tipos de aplicación de promoción
/// </summary>
public enum TipoAplicacionPromocion
{
    /// <summary>
    /// Aplicar a toda la factura
    /// </summary>
    FacturaCompleta = 1,

    /// <summary>
    /// Aplicar a productos específicos
    /// </summary>
    ProductosEspecificos = 2,

    /// <summary>
    /// Aplicar por categoría de productos
    /// </summary>
    PorCategoria = 3,

    /// <summary>
    /// Aplicar por cantidad mínima
    /// </summary>
    PorCantidadMinima = 4,

    /// <summary>
    /// Aplicar por monto mínimo
    /// </summary>
    PorMontoMinimo = 5
}

/// <summary>
/// DTO de respuesta para la aplicación de promoción
/// </summary>
public class AplicarPromocionDto
{
    public Guid PromocionId { get; set; }
    public string CodigoPromocion { get; set; } = string.Empty;
    public string NombrePromocion { get; set; } = string.Empty;
    public TipoAplicacionPromocion TipoAplicacion { get; set; }
    public Guid? FacturaId { get; set; }
    public Guid? ComandaId { get; set; }
    public decimal MontoDescuento { get; set; }
    public decimal PorcentajeDescuento { get; set; }
    public DateTime FechaAplicacion { get; set; }
    public Guid? AutorizadoPor { get; set; }
    public bool AplicacionExitosa { get; set; }
    public List<Guid> ProductosAfectados { get; set; } = new();
    public string? MensajeResultado { get; set; }
} 