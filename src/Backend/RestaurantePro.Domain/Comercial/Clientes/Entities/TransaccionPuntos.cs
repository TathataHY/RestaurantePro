namespace RestaurantePro.Domain.Comercial.Clientes.Entities;

/// <summary>
/// Entidad para representar transacciones de puntos de fidelización
/// </summary>
public class TransaccionPuntos : EntityBase
{
    /// <summary>
    /// ID de la tarjeta de fidelización
    /// </summary>
    public Guid TarjetaFidelizacionId { get; private set; }

    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid ClienteId { get; private set; }

    /// <summary>
    /// Tipo de transacción
    /// </summary>
    public TipoTransaccionPuntos Tipo { get; private set; }

    /// <summary>
    /// Puntos de la transacción (positivo para acumulación, negativo para canje)
    /// </summary>
    public int Puntos { get; private set; }

    /// <summary>
    /// Saldo de puntos después de la transacción
    /// </summary>
    public int SaldoResultante { get; private set; }

    /// <summary>
    /// Monto asociado a la transacción (si aplica)
    /// </summary>
    public decimal? MontoAsociado { get; private set; }

    /// <summary>
    /// Descripción de la transacción
    /// </summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>
    /// Referencia externa (ID de factura, promoción, etc.)
    /// </summary>
    public string? ReferenciaExterna { get; private set; }

    /// <summary>
    /// ID de la promoción asociada (si aplica)
    /// </summary>
    public Guid? PromocionId { get; private set; }

    /// <summary>
    /// ID de la factura asociada (si aplica)
    /// </summary>
    public Guid? FacturaId { get; private set; }

    /// <summary>
    /// Fecha de la transacción
    /// </summary>
    public DateTime Fecha { get; private set; }

    /// <summary>
    /// Fecha de vencimiento de los puntos (si aplica)
    /// </summary>
    public DateTime? FechaVencimiento { get; private set; }

    /// <summary>
    /// Usuario que procesó la transacción
    /// </summary>
    public Guid UsuarioId { get; private set; }

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? Observaciones { get; private set; }

    /// <summary>
    /// Estado de la transacción
    /// </summary>
    public EstadoTransaccionPuntos Estado { get; private set; }

    /// <summary>
    /// Metadatos adicionales en formato JSON
    /// </summary>
    public string? Metadata { get; private set; }

    // Propiedades de navegación
    public virtual TarjetaFidelizacion TarjetaFidelizacion { get; set; } = null!;
    public virtual Cliente Cliente { get; set; } = null!;

    // Constructor privado para EF Core
    private TransaccionPuntos() { }

    // Constructor para crear nueva transacción
    public TransaccionPuntos(
        Guid tarjetaFidelizacionId,
        Guid clienteId,
        TipoTransaccionPuntos tipo,
        int puntos,
        int saldoResultante,
        string descripcion,
        Guid usuarioId,
        decimal? montoAsociado = null,
        string? referenciaExterna = null,
        Guid? promocionId = null,
        Guid? facturaId = null,
        DateTime? fechaVencimiento = null,
        string? observaciones = null,
        string? metadata = null)
    {
        TarjetaFidelizacionId = tarjetaFidelizacionId;
        ClienteId = clienteId;
        Tipo = tipo;
        Puntos = puntos;
        SaldoResultante = saldoResultante;
        Descripcion = descripcion;
        UsuarioId = usuarioId;
        MontoAsociado = montoAsociado;
        ReferenciaExterna = referenciaExterna;
        PromocionId = promocionId;
        FacturaId = facturaId;
        FechaVencimiento = fechaVencimiento;
        Observaciones = observaciones;
        Metadata = metadata;
        Fecha = DateTime.Now;
        Estado = EstadoTransaccionPuntos.Pendiente;
    }

    // Métodos de dominio
    public void Completar()
    {
        Estado = EstadoTransaccionPuntos.Completada;
    }

    public void Cancelar()
    {
        Estado = EstadoTransaccionPuntos.Cancelada;
    }

    public void Revertir()
    {
        Estado = EstadoTransaccionPuntos.Revertida;
    }

    public void ActualizarObservaciones(string observaciones)
    {
        Observaciones = observaciones;
    }
} 