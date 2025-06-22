using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;

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
    public string UnidadMedidaTexto { get; set; } = string.Empty;

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
    public new bool Activo { get; set; }

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

    // 🔥 PROPIEDADES BÁSICAS AGREGADAS para mejor UX
    /// <summary>
    /// Código de barras o SKU formateado para mostrar
    /// </summary>
    public string CodigoInventario => $"ING-{Id.ToString().Substring(0, 8).ToUpper()}";

    /// <summary>
    /// Stock formateado para mostrar en UI
    /// </summary>
    public string StockFormateado => $"{StockActual:N2} {UnidadMedidaTexto}";

    /// <summary>
    /// Costo formateado para mostrar
    /// </summary>
    public string CostoFormateado => $"${CostoUnitario:N2}";

    /// <summary>
    /// Valor de inventario formateado
    /// </summary>
    public string ValorInventarioFormateado => $"${ValorInventario:N2}";

    /// <summary>
    /// Nivel de criticidad de stock (1-5)
    /// </summary>
    public int NivelCriticidad => SinStock ? 5 : BajoStock ? 4 : PorcentajeStock < 30 ? 3 : PorcentajeStock < 60 ? 2 : 1;

    /// <summary>
    /// Mensaje de alerta basado en el estado del stock
    /// </summary>
    public string MensajeAlerta => NivelCriticidad switch
    {
        5 => "¡SIN STOCK! Reponer urgentemente",
        4 => "Stock bajo - Considerar reposición",
        3 => "Stock moderado - Monitorear",
        2 => "Stock normal",
        1 => "Stock óptimo",
        _ => "Estado desconocido"
    };

    /// <summary>
    /// Icono recomendado para mostrar el estado
    /// </summary>
    public string IconoEstado => NivelCriticidad switch
    {
        5 => "alert-triangle",
        4 => "alert-circle", 
        3 => "clock",
        2 => "check-circle",
        1 => "check-circle-2",
        _ => "help-circle"
    };

    /// <summary>
    /// Indica si necesita reposición urgente
    /// </summary>
    public bool NecesitaReposicionUrgente => NivelCriticidad >= 4;

    /// <summary>
    /// Cantidad sugerida para reposición
    /// </summary>
    public decimal CantidadSugeridaReposicion => StockMaximo - StockActual;

    /// <summary>
    /// Días desde el último movimiento
    /// </summary>
    public int? DiasSinMovimiento => MovimientosRecientes.Count > 0 ? 
        (int?)(DateTime.Now - MovimientosRecientes.First().FechaCreacion).TotalDays : null;

    /// <summary>
    /// Indica si el ingrediente está inactivo por mucho tiempo
    /// </summary>
    public bool EstaInactivo => DiasSinMovimiento > 30;
} 