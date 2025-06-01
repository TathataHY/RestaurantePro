namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos;

/// <summary>
/// Command para acumular puntos en el programa de fidelización
/// Gestiona múltiples tipos de transacciones y esquemas de puntuación
/// </summary>
public class AcumularPuntosCommand : IRequest<Result<AcumulacionPuntosDto>>
{
    /// <summary>
    /// ID del cliente que acumula puntos
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// ID de la tarjeta de fidelización (opcional si el cliente solo tiene una)
    /// </summary>
    public Guid? TarjetaFidelizacionId { get; set; }

    /// <summary>
    /// Monto de la compra o transacción base
    /// </summary>
    public decimal MontoCompra { get; set; }

    /// <summary>
    /// Alias para MontoCompra (compatibilidad con tests)
    /// </summary>
    public decimal MontoVenta 
    { 
        get => MontoCompra; 
        set => MontoCompra = value; 
    }

    /// <summary>
    /// Puntos directos a acumular (sin cálculo basado en monto)
    /// </summary>
    public int? PuntosDirectos { get; set; }

    /// <summary>
    /// Tipo de acumulación de puntos
    /// </summary>
    public TipoAcumulacion TipoAcumulacion { get; set; } = TipoAcumulacion.PorCompra;

    /// <summary>
    /// Indica si es una acumulación manual por administrador
    /// </summary>
    public bool EsAcumulacionManual { get; set; } = false;

    /// <summary>
    /// Motivo de la acumulación manual
    /// </summary>
    public string? MotivoAcumulacionManual { get; set; }

    /// <summary>
    /// Usuario que realiza la acumulación manual
    /// </summary>
    public string? UsuarioQueAcumula { get; set; }

    /// <summary>
    /// Tipo de transacción para el cálculo de puntos
    /// </summary>
    public TipoTransaccionPuntos TipoTransaccion { get; set; } = TipoTransaccionPuntos.Compra;

    /// <summary>
    /// ID de la factura o transacción asociada
    /// </summary>
    public Guid? FacturaId { get; set; }

    /// <summary>
    /// ID de la comanda asociada
    /// </summary>
    public Guid? ComandaId { get; set; }

    /// <summary>
    /// Código de promoción aplicable (opcional)
    /// </summary>
    public string? CodigoPromocion { get; set; }

    /// <summary>
    /// Multiplicador especial de puntos (por promociones, eventos especiales, etc.)
    /// </summary>
    public decimal? MultiplicadorEspecial { get; set; }

    /// <summary>
    /// Puntos bonus adicionales por criterios especiales
    /// </summary>
    public int? PuntosBonus { get; set; }

    /// <summary>
    /// Categoría de productos para aplicar multiplicadores específicos
    /// </summary>
    public string? CategoriaProductos { get; set; }

    /// <summary>
    /// Indica si es una transacción en fecha especial (cumpleaños, aniversario, etc.)
    /// </summary>
    public bool EsFechaEspecial { get; set; } = false;

    /// <summary>
    /// Tipo de fecha especial si aplica
    /// </summary>
    public string? TipoFechaEspecial { get; set; }

    /// <summary>
    /// Canal de la transacción (Presencial, App, Web, etc.)
    /// </summary>
    public string Canal { get; set; } = "Presencial";

    /// <summary>
    /// Sucursal donde se realizó la transacción
    /// </summary>
    public string? Sucursal { get; set; }

    /// <summary>
    /// ID del empleado que procesó la transacción
    /// </summary>
    public Guid? EmpleadoId { get; set; }

    /// <summary>
    /// Referencia externa de la transacción
    /// </summary>
    public string? ReferenciaExterna { get; set; }

    /// <summary>
    /// Comentarios adicionales sobre la acumulación
    /// </summary>
    public string? Comentarios { get; set; }

    /// <summary>
    /// Datos adicionales de la transacción
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }
}

/// <summary>
/// Tipos de transacciones para acumulación de puntos
/// </summary>
public enum TipoTransaccionPuntos
{
    /// <summary>
    /// Compra regular en el restaurante
    /// </summary>
    Compra = 1,

    /// <summary>
    /// Bonus por referido
    /// </summary>
    Referido = 2,

    /// <summary>
    /// Bonus por cumpleaños
    /// </summary>
    Cumpleanos = 3,

    /// <summary>
    /// Bonus por aniversario de membresía
    /// </summary>
    Aniversario = 4,

    /// <summary>
    /// Promoción especial
    /// </summary>
    PromocionEspecial = 5,

    /// <summary>
    /// Recompensa por reseña o feedback
    /// </summary>
    Resena = 6,

    /// <summary>
    /// Bonus por check-in en redes sociales
    /// </summary>
    CheckInSocial = 7,

    /// <summary>
    /// Ajuste manual por administrador
    /// </summary>
    AjusteManual = 8,

    /// <summary>
    /// Bonus por participación en evento
    /// </summary>
    Evento = 9,

    /// <summary>
    /// Puntos por suscripción o membresía premium
    /// </summary>
    Suscripcion = 10
}

/// <summary>
/// Tipos de acumulación de puntos
/// </summary>
public enum TipoAcumulacion
{
    /// <summary>
    /// Acumulación por compra regular
    /// </summary>
    PorCompra = 1,

    /// <summary>
    /// Acumulación manual por administrador
    /// </summary>
    Manual = 2,

    /// <summary>
    /// Acumulación por promoción
    /// </summary>
    PorPromocion = 3,

    /// <summary>
    /// Acumulación por evento especial
    /// </summary>
    PorEvento = 4,

    /// <summary>
    /// Acumulación por referido
    /// </summary>
    PorReferido = 5
} 