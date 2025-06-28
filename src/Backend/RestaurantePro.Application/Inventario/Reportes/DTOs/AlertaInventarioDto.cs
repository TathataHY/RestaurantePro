namespace RestaurantePro.Application.Inventario.Reportes.DTOs;

/// <summary>
/// DTO para alerta de inventario
/// </summary>
public class AlertaInventarioDto
{
    public Guid IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public string CodigoIngrediente { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public string TipoAlerta { get; set; } = string.Empty; // "Bajo", "Crítico"
    public string Severidad { get; set; } = string.Empty; // "Media", "Alta"
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaAlerta { get; set; }
    public decimal CantidadRecomendada { get; set; }
} 