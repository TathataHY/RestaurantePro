namespace RestaurantePro.Application.Core.Recetas.DTOs;

/// <summary>
/// DTO para representar una receta de producto
/// </summary>
public class RecetaDto
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string Preparacion { get; set; } = string.Empty;
    public int TiempoPreparacionMinutos { get; set; }
    public List<IngredienteRecetaDto> Ingredientes { get; set; } = new();
    public decimal CostoTotal { get; set; }
    public bool EstaActiva { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// DTO para representar un ingrediente en una receta
/// </summary>
public class IngredienteRecetaDto
{
    public Guid IngredienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public bool EsOpcional { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal CostoTotal { get; set; }
}

/// <summary>
/// DTO para agregar un ingrediente a una receta
/// </summary>
public class AgregarIngredienteDto
{
    public Guid IngredienteId { get; set; }
    public decimal Cantidad { get; set; }
    public bool EsOpcional { get; set; }
}

/// <summary>
/// DTO para verificar disponibilidad de ingredientes en una receta
/// </summary>
public class DisponibilidadRecetaDto
{
    public bool EstaDisponible { get; set; }
    public List<string> IngredientesFaltantes { get; set; } = new();
    public int MaximaPorcionesDisponibles { get; set; }
} 
