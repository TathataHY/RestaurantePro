namespace RestaurantePro.Application.Inventario.Reportes.DTOs;

public class TendenciaInventarioDto
{
    public DateTime Fecha { get; set; }
    public decimal ValorTotal { get; set; }
    public int CantidadIngredientes { get; set; }
}

public class CategoriaAnalisisDto
{
    public string Categoria { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public int CantidadIngredientes { get; set; }
    public decimal PorcentajeDelTotal { get; set; }
} 