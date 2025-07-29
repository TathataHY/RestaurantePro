namespace RestaurantePro.Application.Inventario.Ingredientes.DTOs;

/// <summary>
/// DTO para estadísticas de ingredientes
/// </summary>
public class EstadisticasIngredientesDto
{
    public int TotalIngredientes { get; set; }
    public int IngredientesActivos { get; set; }
    public int IngredientesBajoStock { get; set; }
    public int IngredientesSinStock { get; set; }
    public decimal ValorTotalInventario { get; set; }
    public Dictionary<string, int> IngredientesPorCategoria { get; set; } = new();
} 