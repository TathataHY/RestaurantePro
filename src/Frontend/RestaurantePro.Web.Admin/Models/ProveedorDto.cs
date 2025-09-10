using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para representar un proveedor
/// </summary>
public class ProveedorDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(50, ErrorMessage = "El RUC no puede exceder 50 caracteres")]
    public string? Ruc { get; set; }
    
    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Direccion { get; set; }
    
    [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
    public string? Ciudad { get; set; }
    
    [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
    public string? Pais { get; set; }
    
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string? Email { get; set; }
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
    
    public bool EstaActivo { get; set; } = true;
    
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    
    // Propiedades calculadas
    public int TotalContactos { get; set; }
    public int TotalOrdenesCompra { get; set; }
    public decimal MontoTotalCompras { get; set; }
    public DateTime? UltimaCompra { get; set; }
    
    // Lista de contactos
    public List<ContactoProveedorDto> Contactos { get; set; } = new();
}

/// <summary>
/// DTO para representar un contacto de proveedor
/// </summary>
public class ContactoProveedorDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string? Apellido { get; set; }
    
    [StringLength(50, ErrorMessage = "El cargo no puede exceder 50 caracteres")]
    public string? Cargo { get; set; }
    
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string? Email { get; set; }
    
    [StringLength(20, ErrorMessage = "El celular no puede exceder 20 caracteres")]
    public string? Celular { get; set; }
    
    public bool EsContactoPrincipal { get; set; } = false;
    
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    
    // Propiedades de navegación
    public Guid ProveedorId { get; set; }
    public string? ProveedorNombre { get; set; }
}

/// <summary>
/// DTO para filtros de búsqueda de proveedores
/// </summary>
public class ProveedorFiltrosDto
{
    public string? Nombre { get; set; }
    public string? Ruc { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }
    public bool EstaActivo { get; set; } = true;
    public DateTime? FechaCreacionDesde { get; set; }
    public DateTime? FechaCreacionHasta { get; set; }
    public decimal? MontoMinimoCompras { get; set; }
    public decimal? MontoMaximoCompras { get; set; }
    public bool TieneContactos { get; set; } = false;
    public bool TieneOrdenesCompra { get; set; } = false;
}

/// <summary>
/// DTO para estadísticas de proveedores
/// </summary>
public class ProveedorEstadisticasDto
{
    public int TotalProveedores { get; set; }
    public int ProveedoresActivos { get; set; }
    public int ProveedoresInactivos { get; set; }
    public int TotalContactos { get; set; }
    public int TotalOrdenesCompra { get; set; }
    public decimal MontoTotalCompras { get; set; }
    public decimal MontoPromedioCompras { get; set; }
    public int ProveedoresConContactos { get; set; }
    public int ProveedoresSinContactos { get; set; }
    public int ProveedoresConOrdenes { get; set; }
    public int ProveedoresSinOrdenes { get; set; }
    public List<ProveedorTopDto> TopProveedores { get; set; } = new();
    public List<ProveedorPorCiudadDto> ProveedoresPorCiudad { get; set; } = new();
}

/// <summary>
/// DTO para top proveedores
/// </summary>
public class ProveedorTopDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal MontoTotalCompras { get; set; }
    public int TotalOrdenes { get; set; }
    public DateTime? UltimaCompra { get; set; }
}

/// <summary>
/// DTO para distribución de proveedores por ciudad
/// </summary>
public class ProveedorPorCiudadDto
{
    public string Ciudad { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal MontoTotal { get; set; }
}

/// <summary>
/// DTO para crear/actualizar proveedor
/// </summary>
public class CrearProveedorRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(50, ErrorMessage = "El RUC no puede exceder 50 caracteres")]
    public string? Ruc { get; set; }
    
    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Direccion { get; set; }
    
    [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
    public string? Ciudad { get; set; }
    
    [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
    public string? Pais { get; set; }
    
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string? Email { get; set; }
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
    
    public bool EstaActivo { get; set; } = true;
}

/// <summary>
/// DTO para crear/actualizar contacto de proveedor
/// </summary>
public class CrearContactoProveedorRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string? Apellido { get; set; }
    
    [StringLength(50, ErrorMessage = "El cargo no puede exceder 50 caracteres")]
    public string? Cargo { get; set; }
    
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string? Email { get; set; }
    
    [StringLength(20, ErrorMessage = "El celular no puede exceder 20 caracteres")]
    public string? Celular { get; set; }
    
    public bool EsContactoPrincipal { get; set; } = false;
    
    [Required(ErrorMessage = "El ID del proveedor es requerido")]
    public Guid ProveedorId { get; set; }
}

/// <summary>
/// DTO para exportar proveedores
/// </summary>
public class ExportarProveedoresRequest
{
    public ProveedorFiltrosDto Filtros { get; set; } = new();
    public List<string> Columnas { get; set; } = new();
    public string Formato { get; set; } = "Excel"; // Excel, PDF, CSV
}
