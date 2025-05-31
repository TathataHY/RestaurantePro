namespace RestaurantePro.Application.Inventario.Ingredientes.DTOs;

/// <summary>
/// DTO principal para Ingrediente con información completa
/// Incluye stock, movimientos recientes y propiedades calculadas
/// </summary>
public class IngredienteDto : BaseDto
{
    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del ingrediente
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Categoría del ingrediente
    /// </summary>
    public RotacionIngrediente Rotacion { get; set; }

    /// <summary>
    /// Texto de la categoría del ingrediente
    /// </summary>
    public string RotacionTexto => Rotacion.ToString();

    /// <summary>
    /// Unidad de medida del ingrediente
    /// </summary>
    public UnidadMedida UnidadMedida { get; set; }

    /// <summary>
    /// Texto de la unidad de medida del ingrediente
    /// </summary>
    public string UnidadMedidaTexto => UnidadMedida.ToString();

    /// <summary>
    /// Stock actual disponible
    /// </summary>
    public decimal StockActual { get; set; }

    /// <summary>
    /// Stock mínimo configurado
    /// </summary>
    public decimal StockMinimo { get; set; }

    /// <summary>
    /// Stock máximo configurado
    /// </summary>
    public decimal StockMaximo { get; set; }

    /// <summary>
    /// Costo unitario del ingrediente
    /// </summary>
    public decimal CostoUnitario { get; set; }

    /// <summary>
    /// Costo promedio en el inventario
    /// </summary>
    public decimal CostoPromedio { get; set; }

    /// <summary>
    /// Valor del inventario (stock actual * costo promedio)
    /// </summary>
    public decimal ValorInventario => StockActual * CostoPromedio;

    /// <summary>
    /// ID del proveedor principal (si tiene)
    /// </summary>
    public Guid? ProveedorPrincipalId { get; set; }

    /// <summary>
    /// Nombre del proveedor principal (si tiene)
    /// </summary>
    public string? NombreProveedorPrincipal { get; set; }

    /// <summary>
    /// Indica si está activo
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Indica si requiere refrigeración
    /// </summary>
    public bool RequiereRefrigeracion { get; set; }

    /// <summary>
    /// Días de vencimiento del ingrediente
    /// </summary>
    public int DiasVencimiento { get; set; }

    /// <summary>
    /// Indica si está bajo el stock mínimo
    /// </summary>
    public bool BajoStock => StockActual <= StockMinimo;

    /// <summary>
    /// Indica si está sin stock
    /// </summary>
    public bool SinStock => StockActual <= 0;

    /// <summary>
    /// Porcentaje del stock actual respecto al máximo
    /// </summary>
    public decimal PorcentajeStock => StockMaximo > 0 ? (StockActual / StockMaximo) * 100 : 0;

    // === PROPIEDADES CALCULADAS ===

    /// <summary>
    /// Estado del stock (Crítico, Bajo, Normal, Alto)
    /// </summary>
    public string EstadoStock { get; set; } = string.Empty;

    /// <summary>
    /// Color del estado para mostrar en UI
    /// </summary>
    public string ColorEstado { get; set; } = string.Empty;

    /// <summary>
    /// Indica si está bajo el stock mínimo
    /// </summary>
    public bool EstaBajoMinimo { get; set; }

    /// <summary>
    /// Días estimados de duración con el stock actual
    /// </summary>
    public int? DiasEstimadosDuracion { get; set; }

    /// <summary>
    /// Valor total del stock (cantidad * costo promedio)
    /// </summary>
    public decimal ValorTotalStock { get; set; }

    /// <summary>
    /// Resumen del estado para tooltips
    /// </summary>
    public string ResumenEstado { get; set; } = string.Empty;

    /// <summary>
    /// Lista de movimientos recientes (últimos 5)
    /// </summary>
    public List<MovimientoInventarioDto> MovimientosRecientes { get; set; } = new();
} 