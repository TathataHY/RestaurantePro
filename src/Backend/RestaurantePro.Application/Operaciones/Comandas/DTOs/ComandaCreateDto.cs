namespace RestaurantePro.Application.Operaciones.Comandas.DTOs;

/// <summary>
/// DTO para crear una nueva comanda
/// Datos de entrada requeridos para iniciar una comanda
/// </summary>
public class ComandaCreateDto
{
    /// <summary>
    /// ID del mesero responsable de la comanda
    /// </summary>
    public Guid MeseroId { get; set; }

    /// <summary>
    /// ID de la mesa donde se toma la comanda (opcional)
    /// </summary>
    public Guid? MesaId { get; set; }

    /// <summary>
    /// ID del cliente asociado (opcional)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Observaciones iniciales de la comanda
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Lista de productos iniciales para agregar a la comanda
    /// </summary>
    public List<AgregarProductoDto> ProductosIniciales { get; set; } = new();
}

/// <summary>
/// DTO para agregar un producto a una comanda
/// </summary>
public class AgregarProductoDto
{
    /// <summary>
    /// ID del producto a agregar
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Cantidad del producto
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario del producto (será validado contra el precio actual)
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Observaciones específicas para este producto
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Lista de personalizaciones para el producto
    /// </summary>
    public List<PersonalizacionCreateDto> Personalizaciones { get; set; } = new();
}

/// <summary>
/// DTO para crear personalizaciones en un producto
/// </summary>
public class PersonalizacionCreateDto
{
    /// <summary>
    /// Tipo de personalización: "Extra", "Quitar", "Sustituir"
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// ID del ingrediente a personalizar
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Cantidad para extras y sustituciones
    /// </summary>
    public decimal Cantidad { get; set; } = 1;

    /// <summary>
    /// ID del ingrediente de sustitución (solo para tipo "Sustituir")
    /// </summary>
    public Guid? IngredienteSustitucionId { get; set; }

    /// <summary>
    /// Precio adicional de la personalización
    /// </summary>
    public decimal PrecioAdicional { get; set; } = 0;

    /// <summary>
    /// Detalles adicionales de la personalización
    /// </summary>
    public string? Detalles { get; set; }
} 