namespace RestaurantePro.Application.Inventario.Reportes.DTOs;

public class ItemInventarioFisicoDto
{
    public int IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public decimal CantidadContada { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
}

public class ResultadoInventarioFisicoDto
{
    public int InventarioId { get; set; }
    public DateTime FechaRealizacion { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public List<DiferenciaInventarioDto> Diferencias { get; set; } = new();
    public decimal ValorTotalDiferencias { get; set; }
    public int TotalItemsContados { get; set; }
    public int ItemsConDiferencias { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class DiferenciaInventarioDto
{
    public int IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public decimal StockSistema { get; set; }
    public decimal StockContado { get; set; }
    public decimal Diferencia { get; set; }
    public decimal ValorDiferencia { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
} 