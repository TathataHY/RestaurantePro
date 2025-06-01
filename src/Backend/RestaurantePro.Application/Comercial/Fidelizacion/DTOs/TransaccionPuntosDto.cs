namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

/// <summary>
/// DTO para representar transacciones de puntos
/// </summary>
public class TransaccionPuntosDto
{
    /// <summary>
    /// ID único de la transacción
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID de la tarjeta de fidelización
    /// </summary>
    public Guid TarjetaFidelizacionId { get; set; }

    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Tipo de transacción
    /// </summary>
    public TipoMovimientoPuntos Tipo { get; set; }

    /// <summary>
    /// Puntos de la transacción (positivo para acumulación, negativo para canje)
    /// </summary>
    public int Puntos { get; set; }

    /// <summary>
    /// Saldo de puntos después de la transacción
    /// </summary>
    public int SaldoResultante { get; set; }

    /// <summary>
    /// Monto asociado a la transacción (si aplica)
    /// </summary>
    public decimal? MontoAsociado { get; set; }

    /// <summary>
    /// Descripción de la transacción
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Referencia externa (ID de factura, promoción, etc.)
    /// </summary>
    public string? ReferenciaExterna { get; set; }

    /// <summary>
    /// ID de la promoción asociada (si aplica)
    /// </summary>
    public Guid? PromocionId { get; set; }

    /// <summary>
    /// ID de la factura asociada (si aplica)
    /// </summary>
    public Guid? FacturaId { get; set; }

    /// <summary>
    /// Fecha de la transacción
    /// </summary>
    public DateTime Fecha { get; set; } = DateTime.Now;

    /// <summary>
    /// Fecha de vencimiento de los puntos (si aplica)
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Usuario que procesó la transacción
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Estado de la transacción
    /// </summary>
    public EstadoTransaccionPuntos Estado { get; set; } = EstadoTransaccionPuntos.Completada;

    /// <summary>
    /// Metadatos adicionales en formato JSON
    /// </summary>
    public string? Metadata { get; set; }
}

/// <summary>
/// Tipos de transacción de puntos
/// </summary>
public enum TipoTransaccionPuntos
{
    /// <summary>
    /// Acumulación por compra
    /// </summary>
    AcumulacionCompra = 1,

    /// <summary>
    /// Acumulación por promoción
    /// </summary>
    AcumulacionPromocion = 2,

    /// <summary>
    /// Acumulación manual
    /// </summary>
    AcumulacionManual = 3,

    /// <summary>
    /// Canje de puntos
    /// </summary>
    Canje = 4,

    /// <summary>
    /// Ajuste positivo
    /// </summary>
    AjustePositivo = 5,

    /// <summary>
    /// Ajuste negativo
    /// </summary>
    AjusteNegativo = 6,

    /// <summary>
    /// Vencimiento de puntos
    /// </summary>
    Vencimiento = 7,

    /// <summary>
    /// Transferencia entre tarjetas
    /// </summary>
    Transferencia = 8
}

/// <summary>
/// Estados de transacción de puntos
/// </summary>
public enum EstadoTransaccionPuntos
{
    /// <summary>
    /// Transacción pendiente
    /// </summary>
    Pendiente = 1,

    /// <summary>
    /// Transacción completada
    /// </summary>
    Completada = 2,

    /// <summary>
    /// Transacción cancelada
    /// </summary>
    Cancelada = 3,

    /// <summary>
    /// Transacción revertida
    /// </summary>
    Revertida = 4
} 