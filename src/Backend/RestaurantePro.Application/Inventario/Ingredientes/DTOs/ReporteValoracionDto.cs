using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Application.Inventario.Ingredientes.DTOs;

public class ReporteValoracionDto
{
    public DateTime FechaGeneracion { get; set; }
    public int TotalIngredientes { get; set; }
    public decimal ValorTotalInventario { get; set; }
    public int IngredientesConStock { get; set; }
    public int IngredientesSinStock { get; set; }
    public int IngredientesBajoStock { get; set; }
    public List<ValoracionIngredienteDto> DetallePorIngrediente { get; set; } = new();
    
    // Propiedades calculadas
    public decimal PorcentajeConStock => TotalIngredientes > 0 ? (decimal)IngredientesConStock / TotalIngredientes * 100 : 0;
    public decimal PorcentajeSinStock => TotalIngredientes > 0 ? (decimal)IngredientesSinStock / TotalIngredientes * 100 : 0;
    public decimal PorcentajeBajoStock => TotalIngredientes > 0 ? (decimal)IngredientesBajoStock / TotalIngredientes * 100 : 0;
    
    // Propiedades formateadas para UI
    public string ValorTotalFormateado => $"${ValorTotalInventario:N2}";
    public string FechaGeneracionFormateada => FechaGeneracion.ToString("dd/MM/yyyy HH:mm");
    public string PorcentajeConStockFormateado => $"{PorcentajeConStock:N1}%";
    public string PorcentajeSinStockFormateado => $"{PorcentajeSinStock:N1}%";
    public string PorcentajeBajoStockFormateado => $"{PorcentajeBajoStock:N1}%";
}

public class ValoracionIngredienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Stock { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public UnidadMedida UnidadMedida { get; set; }
    public string UnidadMedidaTexto => UnidadMedida.ToString();
    public decimal StockMinimo { get; set; }
    public decimal StockMaximo { get; set; }
    public string EstadoStock { get; set; } = string.Empty;
    
    // Propiedades formateadas para UI
    public string StockFormateado => $"{Stock:N2} {UnidadMedidaTexto}";
    public string PrecioUnitarioFormateado => $"${PrecioUnitario:N2}";
    public string ValorTotalFormateado => $"${ValorTotal:N2}";
    public string StockMinimoFormateado => $"{StockMinimo:N2} {UnidadMedidaTexto}";
    public string StockMaximoFormateado => $"{StockMaximo:N2} {UnidadMedidaTexto}";
} 