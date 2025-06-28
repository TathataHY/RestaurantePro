namespace RestaurantePro.Application.Inventario.Reportes.DTOs;

public class ValorTotalInventarioDto
{
    public decimal ValorTotal { get; set; }
    public string Moneda { get; set; } = "USD";
    public DateTime FechaCalculo { get; set; }
    public List<ValorPorCategoriaDto> ValoresPorCategoria { get; set; } = new();
    public int TotalIngredientes { get; set; }
    public decimal PromedioValorPorIngrediente { get; set; }
}

public class ValorPorCategoriaDto
{
    public string Categoria { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public int CantidadIngredientes { get; set; }
    public decimal PorcentajeDelTotal { get; set; }
} 