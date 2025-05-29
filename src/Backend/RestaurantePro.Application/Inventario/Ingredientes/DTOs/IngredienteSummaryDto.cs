namespace RestaurantePro.Application.Inventario.Ingredientes.DTOs;

/// <summary>
/// DTO resumido para Ingrediente - Optimizado para listas y performance
/// Contiene solo los campos esenciales para mostrar en grids y listas
/// </summary>
public class IngredienteSummaryDto
{
    /// <summary>
    /// Identificador único del ingrediente
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Código interno del ingrediente (SKU)
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Categoría del ingrediente (texto legible)
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Unidad de medida
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;

    /// <summary>
    /// Stock actual en inventario
    /// </summary>
    public decimal StockActual { get; set; }

    /// <summary>
    /// Stock mínimo requerido
    /// </summary>
    public decimal StockMinimo { get; set; }

    /// <summary>
    /// Stock máximo permitido
    /// </summary>
    public decimal StockMaximo { get; set; }

    /// <summary>
    /// Costo unitario actual
    /// </summary>
    public decimal CostoUnitario { get; set; }

    /// <summary>
    /// Valor total del stock (cantidad * costo unitario)
    /// </summary>
    public decimal ValorStock => StockActual * CostoUnitario;

    /// <summary>
    /// Estado activo del ingrediente
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Fecha de registro
    /// </summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>
    /// Usuario que registró el ingrediente
    /// </summary>
    public string RegistradoPor { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de la última entrada/salida
    /// </summary>
    public DateTime? FechaUltimoMovimiento { get; set; }

    /// <summary>
    /// Tipo del último movimiento (Entrada, Salida, Ajuste)
    /// </summary>
    public string TipoUltimoMovimiento { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de vencimiento del lote más próximo
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Días hasta vencimiento del lote más próximo
    /// </summary>
    public int? DiasHastaVencimiento => FechaVencimiento.HasValue 
        ? Math.Max(0, (int)(FechaVencimiento.Value - DateTime.Now).TotalDays)
        : null;

    /// <summary>
    /// ID del proveedor principal
    /// </summary>
    public Guid? ProveedorPrincipalId { get; set; }

    /// <summary>
    /// Nombre del proveedor principal
    /// </summary>
    public string ProveedorPrincipal { get; set; } = string.Empty;

    /// <summary>
    /// Número de recetas que usan este ingrediente
    /// </summary>
    public int TotalRecetas { get; set; }

    /// <summary>
    /// Consumo promedio mensual
    /// </summary>
    public decimal ConsumoPromedioMensual { get; set; }

    /// <summary>
    /// Estado del stock (OK, Bajo, Crítico, Sin Stock)
    /// </summary>
    public string EstadoStock => StockActual switch
    {
        0 => "Sin Stock",
        var stock when stock <= StockMinimo * 0.5m => "Crítico", 
        var stock when stock <= StockMinimo => "Bajo",
        var stock when stock >= StockMaximo => "Exceso",
        _ => "Normal"
    };

    /// <summary>
    /// Porcentaje de stock disponible (respecto al máximo)
    /// </summary>
    public decimal PorcentajeStock => StockMaximo > 0 
        ? Math.Round((StockActual / StockMaximo) * 100, 1) 
        : 0;

    /// <summary>
    /// Color del estado para UI
    /// </summary>
    public string ColorEstado => EstadoStock switch
    {
        "Sin Stock" => "red",
        "Crítico" => "orange",
        "Bajo" => "yellow",
        "Exceso" => "purple",
        "Normal" => "green",
        _ => "gray"
    };

    /// <summary>
    /// Icono del estado para UI
    /// </summary>
    public string IconoEstado => EstadoStock switch
    {
        "Sin Stock" => "x-circle",
        "Crítico" => "alert-triangle",
        "Bajo" => "alert-circle",
        "Exceso" => "trending-up",
        "Normal" => "check-circle",
        _ => "help-circle"
    };

    /// <summary>
    /// Indicador de necesidad de reposición
    /// </summary>
    public bool NecesitaReposicion => StockActual <= StockMinimo;

    /// <summary>
    /// Indicador de proximidad a vencimiento (dentro de 7 días)
    /// </summary>
    public bool ProximoAVencer => DiasHastaVencimiento.HasValue && DiasHastaVencimiento <= 7;

    /// <summary>
    /// Indicador de ingrediente crítico (usado en muchas recetas)
    /// </summary>
    public bool EsCritico => TotalRecetas >= 10;

    /// <summary>
    /// Cantidad sugerida para compra
    /// </summary>
    public decimal CantidadSugeridaCompra => StockActual < StockMinimo 
        ? StockMaximo - StockActual 
        : 0;

    /// <summary>
    /// Días de stock disponible (basado en consumo promedio)
    /// </summary>
    public int? DiasStockDisponible => ConsumoPromedioMensual > 0 
        ? (int?)Math.Floor(StockActual / (ConsumoPromedioMensual / 30))
        : null;
} 