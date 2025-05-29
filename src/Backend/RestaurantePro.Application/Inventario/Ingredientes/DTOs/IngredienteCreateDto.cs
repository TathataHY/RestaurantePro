namespace RestaurantePro.Application.Inventario.Ingredientes.DTOs;

/// <summary>
/// DTO para crear un nuevo ingrediente
/// Contiene solo las propiedades requeridas para la creación
/// </summary>
public class IngredienteCreateDto
{
    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Código único del ingrediente
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del ingrediente
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Unidad de medida
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;

    /// <summary>
    /// Stock inicial
    /// </summary>
    public decimal StockInicial { get; set; } = 0;

    /// <summary>
    /// Stock mínimo permitido
    /// </summary>
    public decimal StockMinimo { get; set; } = 0;

    /// <summary>
    /// Nivel de rotación del ingrediente
    /// </summary>
    public string Rotacion { get; set; } = "Media";

    /// <summary>
    /// Temporada principal del ingrediente
    /// </summary>
    public string Temporada { get; set; } = "TodoElAño";

    /// <summary>
    /// ID del proveedor principal (opcional)
    /// </summary>
    public Guid? ProveedorPrincipalId { get; set; }

    /// <summary>
    /// Costo inicial del ingrediente
    /// </summary>
    public decimal CostoInicial { get; set; } = 0;

    /// <summary>
    /// Indica si el ingrediente debe estar activo al crearse
    /// </summary>
    public bool EstaActivo { get; set; } = true;
} 