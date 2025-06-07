using System;

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
    /// ID de la factura generada
    /// </summary>
    public Guid FacturaId { get; set; }
    
    /// <summary>
    /// Total del pedido
    /// </summary>
    public decimal Total { get; set; }
    
    /// <summary>
    /// Puntos acumulados por el cliente
    /// </summary>
    public int PuntosAcumulados { get; set; }
    
    /// <summary>
    /// Indica si se liberó la mesa
    /// </summary>
    public bool MesaLiberada { get; set; }
    
    /// <summary>
    /// ID de la mesa liberada
    /// </summary>
    public Guid? MesaId { get; set; }
    
    /// <summary>
    /// Información detallada sobre la mesa liberada
    /// </summary>
    public MesaLiberadaDto? MesaLiberadaDto { get; set; }
    
    /// <summary>
    /// Fecha y hora del procesamiento
    /// </summary>
    public DateTime FechaHora { get; set; }
    
    /// <summary>
    /// Estado final de la comanda
    /// </summary>
    public string EstadoComanda { get; set; }
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

/// <summary>
/// DTO con información de un item del pedido
/// </summary>
public class ItemPedidoDto
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string NombreProducto { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad del producto
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario del producto
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Precio total del producto
    /// </summary>
    public decimal PrecioTotal { get; set; }

    /// <summary>
    /// Lista de personalizaciones del producto
    /// </summary>
    public List<string> Personalizaciones { get; set; } = new();

    /// <summary>
    /// Categoría del producto
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el producto tiene descuento
    /// </summary>
    public bool TieneDescuento { get; set; }

    /// <summary>
    /// Monto del descuento aplicado al producto
    /// </summary>
    public decimal MontoDescuento { get; set; }
} 