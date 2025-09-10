using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO base para facturas del restaurante
/// </summary>
public class FacturaDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El número de factura es obligatorio")]
    [StringLength(50, ErrorMessage = "El número de factura no puede exceder 50 caracteres")]
    public string NumeroFactura { get; set; } = string.Empty;
    
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaPago { get; set; }
    
    [Required(ErrorMessage = "El cliente es obligatorio")]
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ClienteTelefono { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La mesa es obligatoria")]
    public Guid MesaId { get; set; }
    public string MesaNombre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El mesero es obligatorio")]
    public Guid MeseroId { get; set; }
    public string MeseroNombre { get; set; } = string.Empty;
    
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Pagada, Vencida, Cancelada
    public string TipoPago { get; set; } = "Efectivo"; // Efectivo, Tarjeta, Transferencia, Otro
    public string MetodoPago { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor o igual a 0")]
    public decimal Subtotal { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
    public decimal Descuento { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "El impuesto debe ser mayor o igual a 0")]
    public decimal Impuesto { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "El total debe ser mayor o igual a 0")]
    public decimal Total { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "El monto pagado debe ser mayor o igual a 0")]
    public decimal MontoPagado { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "El cambio debe ser mayor o igual a 0")]
    public decimal Cambio { get; set; } = 0;
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
    
    [StringLength(500, ErrorMessage = "Las notas internas no pueden exceder 500 caracteres")]
    public string? NotasInternas { get; set; }
    
    public bool EsFacturaElectronica { get; set; } = false;
    public string? CodigoQR { get; set; }
    public string? CodigoBarras { get; set; }
    
    // Información de la comanda asociada
    public Guid? ComandaId { get; set; }
    public string? ComandaNumero { get; set; }
    
    // Relaciones
    public List<FacturaDetalleDto> Detalles { get; set; } = new();
    public List<FacturaPagoDto> Pagos { get; set; } = new();
    
    // Propiedades calculadas
    public decimal SaldoPendiente => Total - MontoPagado;
    public bool EstaPagada => Estado == "Pagada";
    public bool EstaVencida => FechaVencimiento.HasValue && DateTime.Now > FechaVencimiento.Value && !EstaPagada;
    public bool EstaCancelada => Estado == "Cancelada";
    public string EstadoVisual => Estado switch
    {
        "Pagada" => "Pagada",
        "Vencida" => "Vencida",
        "Cancelada" => "Cancelada",
        _ => "Pendiente"
    };
    public string ClaseEstado => Estado switch
    {
        "Pagada" => "success",
        "Vencida" => "danger",
        "Cancelada" => "secondary",
        _ => "warning"
    };
    public int DiasVencimiento => FechaVencimiento.HasValue ? (DateTime.Now - FechaVencimiento.Value).Days : 0;
}

/// <summary>
/// DTO para crear una nueva factura
/// </summary>
public class CrearFacturaRequest
{
    [Required(ErrorMessage = "El cliente es obligatorio")]
    public Guid ClienteId { get; set; }
    
    [Required(ErrorMessage = "La mesa es obligatoria")]
    public Guid MesaId { get; set; }
    
    [Required(ErrorMessage = "El mesero es obligatorio")]
    public Guid MeseroId { get; set; }
    
    public string TipoPago { get; set; } = "Efectivo";
    public string MetodoPago { get; set; } = string.Empty;
    public decimal Descuento { get; set; } = 0;
    public string? Observaciones { get; set; }
    public string? NotasInternas { get; set; }
    public bool EsFacturaElectronica { get; set; } = false;
    public Guid? ComandaId { get; set; }
    
    [Required(ErrorMessage = "Los detalles son obligatorios")]
    public List<CrearFacturaDetalleRequest> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para actualizar una factura existente
/// </summary>
public class ActualizarFacturaRequest
{
    [Required(ErrorMessage = "El ID es obligatorio")]
    public Guid Id { get; set; }
    
    public string Estado { get; set; } = "Pendiente";
    public string TipoPago { get; set; } = "Efectivo";
    public string MetodoPago { get; set; } = string.Empty;
    public decimal Descuento { get; set; } = 0;
    public string? Observaciones { get; set; }
    public string? NotasInternas { get; set; }
    public bool EsFacturaElectronica { get; set; } = false;
}

/// <summary>
/// DTO para detalles de factura
/// </summary>
public class FacturaDetalleDto
{
    public Guid Id { get; set; }
    public Guid FacturaId { get; set; }
    
    [Required(ErrorMessage = "El producto es obligatorio")]
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoCategoria { get; set; } = string.Empty;
    
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0")]
    public decimal PrecioUnitario { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
    public decimal Descuento { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "El impuesto debe ser mayor o igual a 0")]
    public decimal Impuesto { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor o igual a 0")]
    public decimal Subtotal { get; set; }
    
    [StringLength(200, ErrorMessage = "Las observaciones no pueden exceder 200 caracteres")]
    public string? Observaciones { get; set; }
    
    // Propiedades calculadas
    public decimal Total => Subtotal - Descuento + Impuesto;
}

/// <summary>
/// DTO para crear detalle de factura
/// </summary>
public class CrearFacturaDetalleRequest
{
    [Required(ErrorMessage = "El producto es obligatorio")]
    public Guid ProductoId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0")]
    public decimal PrecioUnitario { get; set; }
    
    public decimal Descuento { get; set; } = 0;
    public decimal Impuesto { get; set; } = 0;
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para pagos de factura
/// </summary>
public class FacturaPagoDto
{
    public Guid Id { get; set; }
    public Guid FacturaId { get; set; }
    
    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }
    
    [Required(ErrorMessage = "El método de pago es obligatorio")]
    public string MetodoPago { get; set; } = string.Empty;
    
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
    
    public DateTime FechaPago { get; set; }
    public Guid UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    
    public string Estado { get; set; } = "Procesado"; // Procesado, Pendiente, Rechazado
}

/// <summary>
/// DTO para registrar un pago
/// </summary>
public class RegistrarPagoRequest
{
    [Required(ErrorMessage = "El ID de factura es obligatorio")]
    public Guid FacturaId { get; set; }
    
    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }
    
    [Required(ErrorMessage = "El método de pago es obligatorio")]
    public string MetodoPago { get; set; } = string.Empty;
    
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para estadísticas de facturas
/// </summary>
public class FacturaEstadisticasDto
{
    public int TotalFacturas { get; set; }
    public int FacturasPagadas { get; set; }
    public int FacturasPendientes { get; set; }
    public int FacturasVencidas { get; set; }
    public int FacturasCanceladas { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalCobrado { get; set; }
    public decimal TotalPendiente { get; set; }
    public decimal PromedioFactura { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TotalImpuestos { get; set; }
    public List<FacturaMetodoPagoDto> MetodosPago { get; set; } = new();
    public List<FacturaEstadoDto> Estados { get; set; } = new();
    public List<FacturaVentasDiariasDto> VentasDiarias { get; set; } = new();
}

/// <summary>
/// DTO para métodos de pago
/// </summary>
public class FacturaMetodoPagoDto
{
    public string MetodoPago { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
    public decimal Porcentaje { get; set; }
}

/// <summary>
/// DTO para estados de factura
/// </summary>
public class FacturaEstadoDto
{
    public string Estado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
    public decimal Porcentaje { get; set; }
}

/// <summary>
/// DTO para ventas diarias
/// </summary>
public class FacturaVentasDiariasDto
{
    public DateTime Fecha { get; set; }
    public int CantidadFacturas { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalCobrado { get; set; }
    public decimal PromedioFactura { get; set; }
}

/// <summary>
/// DTO para filtros de facturas
/// </summary>
public class FacturaFiltrosDto
{
    public string? Busqueda { get; set; }
    public string? Estado { get; set; }
    public string? TipoPago { get; set; }
    public string? MetodoPago { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? MesaId { get; set; }
    public Guid? MeseroId { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public decimal? MontoMinimo { get; set; }
    public decimal? MontoMaximo { get; set; }
    public bool? EsFacturaElectronica { get; set; }
    public string? OrdenarPor { get; set; } = "FechaEmision";
    public string? DireccionOrden { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO para reimprimir factura
/// </summary>
public class ReimprimirFacturaRequest
{
    [Required(ErrorMessage = "El ID de factura es obligatorio")]
    public Guid FacturaId { get; set; }
    
    public string Motivo { get; set; } = "Reimpresión";
    public bool IncluirQR { get; set; } = true;
    public bool IncluirCodigoBarras { get; set; } = true;
}

/// <summary>
/// DTO para cancelar factura
/// </summary>
public class CancelarFacturaRequest
{
    [Required(ErrorMessage = "El ID de factura es obligatorio")]
    public Guid FacturaId { get; set; }
    
    [Required(ErrorMessage = "El motivo es obligatorio")]
    [StringLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
    public string Motivo { get; set; } = string.Empty;
    
    public bool Reembolsar { get; set; } = false;
    public string? MetodoReembolso { get; set; }
}

/// <summary>
/// DTO para generar factura electrónica
/// </summary>
public class GenerarFacturaElectronicaRequest
{
    [Required(ErrorMessage = "El ID de factura es obligatorio")]
    public Guid FacturaId { get; set; }
    
    [Required(ErrorMessage = "El RUC es obligatorio")]
    [StringLength(11, ErrorMessage = "El RUC debe tener 11 caracteres")]
    public string RUC { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La razón social es obligatoria")]
    [StringLength(200, ErrorMessage = "La razón social no puede exceder 200 caracteres")]
    public string RazonSocial { get; set; } = string.Empty;
    
    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Direccion { get; set; }
    
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    public string? Email { get; set; }
}
