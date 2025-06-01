namespace RestaurantePro.Application.Operaciones.Reportes.DTOs;

/// <summary>
/// DTO de respuesta para el procesamiento completo de pedido
/// </summary>
public class ProcesarPedidoCompletoDto
{
    /// <summary>
    /// ID de la comanda procesada
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// Número de la comanda
    /// </summary>
    public string NumeroComanda { get; set; } = string.Empty;

    /// <summary>
    /// Estado final de la comanda
    /// </summary>
    public string EstadoComanda { get; set; } = string.Empty;

    /// <summary>
    /// Información de la factura generada
    /// </summary>
    public FacturaProcesadaDto? Factura { get; set; }

    /// <summary>
    /// Información del pago procesado
    /// </summary>
    public PagoProcesadoDto? Pago { get; set; }

    /// <summary>
    /// Información de puntos de fidelización procesados
    /// </summary>
    public FidelizacionProcesadaDto? Fidelizacion { get; set; }

    /// <summary>
    /// Información de la mesa liberada
    /// </summary>
    public MesaLiberadaDto? Mesa { get; set; }

    /// <summary>
    /// Resumen del procesamiento
    /// </summary>
    public ResumenProcesamientoDto Resumen { get; set; } = new();

    /// <summary>
    /// Fecha y hora del procesamiento
    /// </summary>
    public DateTime FechaProcesamiento { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuario que procesó el pedido
    /// </summary>
    public string UsuarioProcesamiento { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones del procesamiento
    /// </summary>
    public string? ObservacionesProcesamiento { get; set; }

    /// <summary>
    /// Indica si el procesamiento fue exitoso
    /// </summary>
    public bool ProcesamientoExitoso { get; set; }

    /// <summary>
    /// Mensajes de advertencia o información
    /// </summary>
    public List<string> Mensajes { get; set; } = new();
}

/// <summary>
/// DTO con información de la factura procesada
/// </summary>
public class FacturaProcesadaDto
{
    /// <summary>
    /// ID de la factura
    /// </summary>
    public Guid FacturaId { get; set; }

    /// <summary>
    /// Número de la factura
    /// </summary>
    public string NumeroFactura { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de factura
    /// </summary>
    public string TipoFactura { get; set; } = string.Empty;

    /// <summary>
    /// Monto total de la factura
    /// </summary>
    public decimal MontoTotal { get; set; }

    /// <summary>
    /// Monto de impuestos
    /// </summary>
    public decimal MontoImpuestos { get; set; }

    /// <summary>
    /// Monto subtotal
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Estado de la factura
    /// </summary>
    public string EstadoFactura { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de emisión
    /// </summary>
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Información del cliente facturado
    /// </summary>
    public ClienteFacturadoDto Cliente { get; set; } = new();
}

/// <summary>
/// DTO con información del cliente facturado
/// </summary>
public class ClienteFacturadoDto
{
    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Identificación del cliente
    /// </summary>
    public string? Identificacion { get; set; }

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? Telefono { get; set; }
}

/// <summary>
/// DTO con información del pago procesado
/// </summary>
public class PagoProcesadoDto
{
    /// <summary>
    /// ID del pago
    /// </summary>
    public Guid PagoId { get; set; }

    /// <summary>
    /// Tipo de pago
    /// </summary>
    public string TipoPago { get; set; } = string.Empty;

    /// <summary>
    /// Monto del pago
    /// </summary>
    public decimal MontoPago { get; set; }

    /// <summary>
    /// Moneda del pago
    /// </summary>
    public string Moneda { get; set; } = string.Empty;

    /// <summary>
    /// Estado del pago
    /// </summary>
    public string EstadoPago { get; set; } = string.Empty;

    /// <summary>
    /// Referencia del pago
    /// </summary>
    public string? ReferenciaPago { get; set; }

    /// <summary>
    /// Número de autorización (para tarjetas)
    /// </summary>
    public string? NumeroAutorizacion { get; set; }

    /// <summary>
    /// Fecha del pago
    /// </summary>
    public DateTime FechaPago { get; set; }

    /// <summary>
    /// Observaciones del pago
    /// </summary>
    public string? ObservacionesPago { get; set; }
}

/// <summary>
/// DTO con información de fidelización procesada
/// </summary>
public class FidelizacionProcesadaDto
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Puntos acumulados en esta transacción
    /// </summary>
    public int PuntosAcumulados { get; set; }

    /// <summary>
    /// Total de puntos del cliente después de la transacción
    /// </summary>
    public int TotalPuntosCliente { get; set; }

    /// <summary>
    /// Nivel de fidelización del cliente
    /// </summary>
    public string? NivelFidelizacion { get; set; }

    /// <summary>
    /// Indica si hubo cambio de nivel
    /// </summary>
    public bool CambioNivel { get; set; }

    /// <summary>
    /// Nivel anterior (si hubo cambio)
    /// </summary>
    public string? NivelAnterior { get; set; }

    /// <summary>
    /// Beneficios obtenidos
    /// </summary>
    public List<string> BeneficiosObtenidos { get; set; } = new();
}

/// <summary>
/// DTO con información de la mesa liberada
/// </summary>
public class MesaLiberadaDto
{
    /// <summary>
    /// ID de la mesa
    /// </summary>
    public Guid MesaId { get; set; }

    /// <summary>
    /// Número de la mesa
    /// </summary>
    public string NumeroMesa { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual de la mesa
    /// </summary>
    public string EstadoMesa { get; set; } = string.Empty;

    /// <summary>
    /// Hora de liberación
    /// </summary>
    public DateTime HoraLiberacion { get; set; }

    /// <summary>
    /// Tiempo total de ocupación
    /// </summary>
    public TimeSpan TiempoOcupacion { get; set; }
}

/// <summary>
/// DTO con resumen del procesamiento
/// </summary>
public class ResumenProcesamientoDto
{
    /// <summary>
    /// Monto total procesado
    /// </summary>
    public decimal MontoTotal { get; set; }

    /// <summary>
    /// Número de items en la comanda
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Tiempo total de procesamiento
    /// </summary>
    public TimeSpan TiempoProcesamiento { get; set; }

    /// <summary>
    /// Pasos completados exitosamente
    /// </summary>
    public List<string> PasosCompletados { get; set; } = new();

    /// <summary>
    /// Pasos que tuvieron advertencias
    /// </summary>
    public List<string> PasosConAdvertencias { get; set; } = new();

    /// <summary>
    /// Indica si se requiere seguimiento
    /// </summary>
    public bool RequiereSeguimiento { get; set; }

    /// <summary>
    /// Motivo del seguimiento (si aplica)
    /// </summary>
    public string? MotivoSeguimiento { get; set; }
} 