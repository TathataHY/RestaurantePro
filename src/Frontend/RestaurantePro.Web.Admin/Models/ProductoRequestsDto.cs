using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// Request para crear un nuevo producto
/// </summary>
public class CrearProductoRequest
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    public Guid CategoriaId { get; set; }

    [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
    public string? Codigo { get; set; }

    [StringLength(100, ErrorMessage = "La imagen URL no puede exceder 100 caracteres")]
    public string? ImagenUrl { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El tiempo de preparación debe ser mayor o igual a 0")]
    public int TiempoPreparacionMinutos { get; set; }

    public bool Disponible { get; set; } = true;

    public bool EsPromocional { get; set; } = false;

    [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
    public decimal? Descuento { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }

    public List<IngredienteProductoRequest> Ingredientes { get; set; } = new();
}

/// <summary>
/// Request para actualizar un producto existente
/// </summary>
public class ActualizarProductoRequest
{
    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public Guid Id { get; set; }

    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string? Nombre { get; set; }

    [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
    public string? Descripcion { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal? Precio { get; set; }

    public Guid? CategoriaId { get; set; }

    [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
    public string? Codigo { get; set; }

    [StringLength(100, ErrorMessage = "La imagen URL no puede exceder 100 caracteres")]
    public string? ImagenUrl { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El tiempo de preparación debe ser mayor o igual a 0")]
    public int? TiempoPreparacionMinutos { get; set; }

    public bool? Disponible { get; set; }

    public bool? EsPromocional { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
    public decimal? Descuento { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }

    public List<IngredienteProductoRequest>? Ingredientes { get; set; }
}

/// <summary>
/// Request para ingrediente de producto
/// </summary>
public class IngredienteProductoRequest
{
    [Required(ErrorMessage = "El ID del ingrediente es obligatorio")]
    public Guid IngredienteId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria")]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal Cantidad { get; set; }

    [Required(ErrorMessage = "La unidad de medida es obligatoria")]
    [StringLength(20, ErrorMessage = "La unidad no puede exceder 20 caracteres")]
    public string UnidadMedida { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Las observaciones no pueden exceder 200 caracteres")]
    public string? Observaciones { get; set; }

    public bool EsOpcional { get; set; } = false;
}

/// <summary>
/// Request para cambiar el estado de un producto
/// </summary>
public class CambiarEstadoProductoRequest
{
    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio")]
    public bool Activo { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// Request para actualizar precio de producto
/// </summary>
public class ActualizarPrecioProductoRequest
{
    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// Request para actualizar disponibilidad de producto
/// </summary>
public class ActualizarDisponibilidadProductoRequest
{
    [Required(ErrorMessage = "El ID del producto es obligatorio")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "La disponibilidad es obligatoria")]
    public bool Disponible { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

/// <summary>
/// Request para filtros de productos
/// </summary>
public class FiltroProductosRequest
{
    public string? Busqueda { get; set; }
    public Guid? CategoriaId { get; set; }
    public bool? Disponible { get; set; }
    public bool? EsPromocional { get; set; }
    public decimal? PrecioMinimo { get; set; }
    public decimal? PrecioMaximo { get; set; }
    public int? TiempoPreparacionMaximo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? OrdenarPor { get; set; }
    public bool OrdenDescendente { get; set; } = false;
}
