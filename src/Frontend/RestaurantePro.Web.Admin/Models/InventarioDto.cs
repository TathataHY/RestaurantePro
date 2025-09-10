using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para representar un ingrediente en el inventario
/// </summary>
public class IngredienteDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El nombre del ingrediente es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "La unidad de medida es obligatoria")]
    [StringLength(20, ErrorMessage = "La unidad no puede exceder 20 caracteres")]
    public string UnidadMedida { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "El stock mínimo debe ser mayor o igual a 0")]
    public decimal StockMinimo { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El stock máximo debe ser mayor o igual a 0")]
    public decimal StockMaximo { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El stock actual debe ser mayor o igual a 0")]
    public decimal StockActual { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor o igual a 0")]
    public decimal CostoUnitario { get; set; }
    
    public DateTime FechaVencimiento { get; set; }
    
    public bool EstaActivo { get; set; } = true;
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime? FechaActualizacion { get; set; }
    
    // Propiedades de navegación
    public string? Categoria { get; set; }
    public string? Proveedor { get; set; }
    public Guid? ProveedorId { get; set; }
}

/// <summary>
/// DTO para representar un movimiento de inventario
/// </summary>
public class MovimientoInventarioDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El ingrediente es obligatorio")]
    public Guid IngredienteId { get; set; }
    
    public string IngredienteNombre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
    public TipoMovimientoInventario TipoMovimiento { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal Cantidad { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor o igual a 0")]
    public decimal CostoUnitario { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El costo total debe ser mayor o igual a 0")]
    public decimal CostoTotal { get; set; }
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
    
    public DateTime FechaMovimiento { get; set; }
    
    public string UsuarioResponsable { get; set; } = string.Empty;
    
    public Guid? OrdenCompraId { get; set; }
    
    public Guid? ComandaId { get; set; }
    
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// DTO para filtros de inventario
/// </summary>
public class InventarioFiltrosDto
{
    public string? Nombre { get; set; }
    public string? Categoria { get; set; }
    public string? Proveedor { get; set; }
    public bool EstaActivo { get; set; }
    public bool StockBajo { get; set; }
    public bool VencimientoProximo { get; set; }
    public DateTime? FechaVencimientoDesde { get; set; }
    public DateTime? FechaVencimientoHasta { get; set; }
    public decimal? StockMinimoDesde { get; set; }
    public decimal? StockMinimoHasta { get; set; }
    public decimal? CostoDesde { get; set; }
    public decimal? CostoHasta { get; set; }
}

/// <summary>
/// DTO para filtros de movimientos de inventario
/// </summary>
public class MovimientoInventarioFiltrosDto
{
    public Guid? IngredienteId { get; set; }
    public TipoMovimientoInventario? TipoMovimiento { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? UsuarioResponsable { get; set; }
    public Guid? OrdenCompraId { get; set; }
    public Guid? ComandaId { get; set; }
}

/// <summary>
/// DTO para estadísticas de inventario
/// </summary>
public class InventarioEstadisticasDto
{
    public int TotalIngredientes { get; set; }
    public int IngredientesActivos { get; set; }
    public int IngredientesInactivos { get; set; }
    public int StockBajo { get; set; }
    public int VencimientoProximo { get; set; }
    public int Vencidos { get; set; }
    public decimal ValorTotalInventario { get; set; }
    public decimal ValorStockBajo { get; set; }
    public decimal ValorVencidos { get; set; }
    public int MovimientosHoy { get; set; }
    public int MovimientosMes { get; set; }
    public decimal CostoTotalMovimientosHoy { get; set; }
    public decimal CostoTotalMovimientosMes { get; set; }
}

/// <summary>
/// DTO para alertas de inventario
/// </summary>
public class AlertaInventarioDto
{
    public Guid IngredienteId { get; set; }
    public string IngredienteNombre { get; set; } = string.Empty;
    public TipoAlertaInventario TipoAlerta { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string Severidad { get; set; } = string.Empty;
    public DateTime FechaAlerta { get; set; }
    public bool EstaResuelta { get; set; }
}

/// <summary>
/// DTO para orden de compra
/// </summary>
public class OrdenCompraDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El proveedor es obligatorio")]
    public Guid ProveedorId { get; set; }
    
    public string ProveedorNombre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El número de orden es obligatorio")]
    [StringLength(50, ErrorMessage = "El número de orden no puede exceder 50 caracteres")]
    public string NumeroOrden { get; set; } = string.Empty;
    
    public EstadoOrdenCompra Estado { get; set; }
    
    public DateTime FechaOrden { get; set; }
    
    public DateTime? FechaEsperadaEntrega { get; set; }
    
    public DateTime? FechaEntrega { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor o igual a 0")]
    public decimal Subtotal { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El impuesto debe ser mayor o igual a 0")]
    public decimal Impuesto { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El total debe ser mayor o igual a 0")]
    public decimal Total { get; set; }
    
    [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
    public string? Observaciones { get; set; }
    
    public string UsuarioResponsable { get; set; } = string.Empty;
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime? FechaActualizacion { get; set; }
    
    public List<DetalleOrdenCompraDto> Detalles { get; set; } = new();
}

/// <summary>
/// DTO para detalle de orden de compra
/// </summary>
public class DetalleOrdenCompraDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El ingrediente es obligatorio")]
    public Guid IngredienteId { get; set; }
    
    public string IngredienteNombre { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal Cantidad { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0")]
    public decimal PrecioUnitario { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El total debe ser mayor o igual a 0")]
    public decimal Total { get; set; }
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para filtros de órdenes de compra
/// </summary>
public class OrdenCompraFiltrosDto
{
    public string? NumeroOrden { get; set; }
    public Guid? ProveedorId { get; set; }
    public EstadoOrdenCompra? Estado { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? UsuarioResponsable { get; set; }
    public decimal? TotalDesde { get; set; }
    public decimal? TotalHasta { get; set; }
}

/// <summary>
/// DTO para estadísticas de órdenes de compra
/// </summary>
public class OrdenCompraEstadisticasDto
{
    public int TotalOrdenes { get; set; }
    public int OrdenesPendientes { get; set; }
    public int OrdenesEnProceso { get; set; }
    public int OrdenesCompletadas { get; set; }
    public int OrdenesCanceladas { get; set; }
    public decimal ValorTotalOrdenes { get; set; }
    public decimal ValorOrdenesPendientes { get; set; }
    public decimal ValorOrdenesCompletadas { get; set; }
    public int OrdenesHoy { get; set; }
    public int OrdenesMes { get; set; }
    public decimal ValorOrdenesHoy { get; set; }
    public decimal ValorOrdenesMes { get; set; }
}

/// <summary>
/// Enumeración para tipos de movimiento de inventario
/// </summary>
public enum TipoMovimientoInventario
{
    Entrada = 1,
    Salida = 2,
    Ajuste = 3,
    Transferencia = 4,
    Vencimiento = 5,
    Perdida = 6
}

/// <summary>
/// Enumeración para tipos de alerta de inventario
/// </summary>
public enum TipoAlertaInventario
{
    StockBajo = 1,
    StockCritico = 2,
    VencimientoProximo = 3,
    Vencido = 4,
    StockExcesivo = 5
}

/// <summary>
/// Enumeración para estados de orden de compra
/// </summary>
public enum EstadoOrdenCompra
{
    Pendiente = 1,
    EnProceso = 2,
    Completada = 3,
    Cancelada = 4,
    ParcialmenteRecibida = 5
}
