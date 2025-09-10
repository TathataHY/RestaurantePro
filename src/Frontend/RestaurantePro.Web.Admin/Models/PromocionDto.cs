using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para promociones del restaurante
/// </summary>
public class PromocionDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "El código es obligatorio")]
    [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
    public string Codigo { get; set; } = string.Empty;
    
    public TipoPromocion Tipo { get; set; }
    
    public decimal ValorDescuento { get; set; }
    
    public decimal? ValorMinimoCompra { get; set; }
    
    public int? CantidadMaximaUsos { get; set; }
    
    public int UsosRealizados { get; set; }
    
    public DateTime FechaInicio { get; set; }
    
    public DateTime FechaFin { get; set; }
    
    public bool EstaActiva { get; set; }
    
    public bool EstaEliminada { get; set; }
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime? FechaActualizacion { get; set; }
    
    public string? CreatedBy { get; set; }
    
    public string? LastModifiedBy { get; set; }
    
    // Propiedades calculadas
    public bool EsValida => EstaActiva && !EstaEliminada && DateTime.Now >= FechaInicio && DateTime.Now <= FechaFin;
    
    public bool TieneUsosDisponibles => !CantidadMaximaUsos.HasValue || UsosRealizados < CantidadMaximaUsos.Value;
    
    public int UsosDisponibles => CantidadMaximaUsos.HasValue ? CantidadMaximaUsos.Value - UsosRealizados : int.MaxValue;
    
    public string Estado => !EstaActiva ? "Inactiva" : 
                           EstaEliminada ? "Eliminada" : 
                           DateTime.Now < FechaInicio ? "Pendiente" : 
                           DateTime.Now > FechaFin ? "Expirada" : 
                           !TieneUsosDisponibles ? "Agotada" : "Activa";
}

/// <summary>
/// DTO para crear una nueva promoción
/// </summary>
public class CrearPromocionRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "El código es obligatorio")]
    [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
    public string Codigo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El tipo es obligatorio")]
    public TipoPromocion Tipo { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El valor del descuento debe ser mayor o igual a 0")]
    public decimal ValorDescuento { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El valor mínimo de compra debe ser mayor o igual a 0")]
    public decimal? ValorMinimoCompra { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad máxima de usos debe ser mayor a 0")]
    public int? CantidadMaximaUsos { get; set; }
    
    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    public DateTime FechaInicio { get; set; } = DateTime.Today;
    
    [Required(ErrorMessage = "La fecha de fin es obligatoria")]
    public DateTime FechaFin { get; set; } = DateTime.Today.AddDays(30);
    
    public bool EstaActiva { get; set; } = true;
    
    public List<Guid> ProductosIds { get; set; } = new();
}

/// <summary>
/// DTO para actualizar una promoción existente
/// </summary>
public class ActualizarPromocionRequest
{
    [Required(ErrorMessage = "El ID es obligatorio")]
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "El código es obligatorio")]
    [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
    public string Codigo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El tipo es obligatorio")]
    public TipoPromocion Tipo { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El valor del descuento debe ser mayor o igual a 0")]
    public decimal ValorDescuento { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El valor mínimo de compra debe ser mayor o igual a 0")]
    public decimal? ValorMinimoCompra { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad máxima de usos debe ser mayor a 0")]
    public int? CantidadMaximaUsos { get; set; }
    
    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    public DateTime FechaInicio { get; set; }
    
    [Required(ErrorMessage = "La fecha de fin es obligatoria")]
    public DateTime FechaFin { get; set; }
    
    public bool EstaActiva { get; set; }
    
    public List<Guid> ProductosIds { get; set; } = new();
}

/// <summary>
/// DTO para productos asociados a una promoción
/// </summary>
public class PromocionProductoDto
{
    public Guid Id { get; set; }
    public Guid PromocionId { get; set; }
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}

/// <summary>
/// DTO para estadísticas de promociones
/// </summary>
public class PromocionEstadisticasDto
{
    public int TotalPromociones { get; set; }
    public int PromocionesActivas { get; set; }
    public int PromocionesExpiradas { get; set; }
    public int PromocionesPendientes { get; set; }
    public int TotalUsos { get; set; }
    public decimal DescuentoTotalAplicado { get; set; }
    public PromocionDto? PromocionMasUsada { get; set; }
}

/// <summary>
/// Tipos de promociones disponibles
/// </summary>
public enum TipoPromocion
{
    Porcentaje = 1,
    MontoFijo = 2,
    DescuentoPorCantidad = 3,
    DescuentoPorCompraMinima = 4,
    DescuentoPorProducto = 5
}

/// <summary>
/// DTO para filtros de promociones
/// </summary>
public class PromocionFiltrosDto
{
    public string? Busqueda { get; set; }
    public TipoPromocion? Tipo { get; set; }
    public bool? EstaActiva { get; set; }
    public DateTime? FechaInicioDesde { get; set; }
    public DateTime? FechaInicioHasta { get; set; }
    public DateTime? FechaFinDesde { get; set; }
    public DateTime? FechaFinHasta { get; set; }
    public string? OrdenarPor { get; set; } = "FechaCreacion";
    public string? DireccionOrden { get; set; } = "desc";
}
