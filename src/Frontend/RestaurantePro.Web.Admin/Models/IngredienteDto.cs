using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO principal para Ingrediente con información completa
/// Incluye stock, movimientos recientes y propiedades calculadas
/// </summary>
public class IngredienteDto
{
    /// <summary>
    /// Identificador único de la entidad
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Fecha y hora de creación de la entidad
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha y hora de la última modificación
    /// </summary>
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Fecha de actualización (alias)
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }

    /// <summary>
    /// Usuario que creó la entidad
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que realizó la última modificación
    /// </summary>
    public string? ModificadoPor { get; set; }

    /// <summary>
    /// Indica si la entidad está activa
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Indica si está activo (alias)
    /// </summary>
    public bool EstaActivo { get; set; } = true;

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    [Required(ErrorMessage = "El nombre del ingrediente es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del ingrediente
    /// </summary>
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
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
    /// Categoría del ingrediente (string)
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

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
    [Range(0, double.MaxValue, ErrorMessage = "El stock actual debe ser mayor o igual a 0")]
    public decimal StockActual { get; set; }

    /// <summary>
    /// Stock mínimo configurado
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El stock mínimo debe ser mayor o igual a 0")]
    public decimal StockMinimo { get; set; }

    /// <summary>
    /// Stock máximo configurado
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El stock máximo debe ser mayor o igual a 0")]
    public decimal StockMaximo { get; set; }

    /// <summary>
    /// Costo unitario del ingrediente
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor o igual a 0")]
    public decimal CostoUnitario { get; set; }

    /// <summary>
    /// Precio unitario del ingrediente
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Costo promedio en el inventario
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El costo promedio debe ser mayor o igual a 0")]
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
    /// ID del proveedor
    /// </summary>
    public Guid? ProveedorId { get; set; }

    /// <summary>
    /// Nombre del proveedor
    /// </summary>
    public string? ProveedorNombre { get; set; }

    /// <summary>
    /// Proveedor (string)
    /// </summary>
    public string? Proveedor { get; set; }

    /// <summary>
    /// Indica si requiere refrigeración
    /// </summary>
    public bool RequiereRefrigeracion { get; set; }

    /// <summary>
    /// Días de vencimiento del ingrediente
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Los días de vencimiento deben ser mayor o igual a 0")]
    public int DiasVencimiento { get; set; }

    /// <summary>
    /// Fecha de vencimiento del ingrediente
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

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

/// <summary>
/// Representa el nivel de rotación de un ingrediente en inventario.
/// Se utiliza para priorizar ingredientes en las políticas de stock
/// y para la generación de órdenes de compra.
/// </summary>
public enum RotacionIngrediente
{
    /// <summary>
    /// Ingrediente con rotación baja (movimiento lento en inventario)
    /// </summary>
    Baja = 0,
    
    /// <summary>
    /// Ingrediente con rotación media
    /// </summary>
    Media = 1,
    
    /// <summary>
    /// Ingrediente con rotación alta (movimiento rápido en inventario)
    /// </summary>
    Alta = 2,
    
    /// <summary>
    /// Ingrediente crítico con rotación muy alta
    /// </summary>
    Critica = 3
}

/// <summary>
/// Unidades de medida para ingredientes
/// </summary>
public enum UnidadMedida
{
    /// <summary>
    /// Unidad individual (pieza, unidad)
    /// </summary>
    Unidad = 0,
    
    /// <summary>
    /// Kilogramo
    /// </summary>
    Kilogramo = 1,
    
    /// <summary>
    /// Gramo
    /// </summary>
    Gramo = 2,
    
    /// <summary>
    /// Litro
    /// </summary>
    Litro = 3,
    
    /// <summary>
    /// Mililitro
    /// </summary>
    Mililitro = 4,
    
    /// <summary>
    /// Cucharadas
    /// </summary>
    Cucharada = 5,
    
    /// <summary>
    /// Cucharaditas
    /// </summary>
    Cucharadita = 6,
    
    /// <summary>
    /// Tazas
    /// </summary>
    Taza = 7,
    
    /// <summary>
    /// Paquete (cantidad predefinida)
    /// </summary>
    Paquete = 8,
    
    /// <summary>
    /// Piezas (unidades individuales)
    /// </summary>
    Piezas = 9,
    
    // Alias en plural para compatibilidad con tests
    /// <summary>
    /// Alias en plural para Kilogramo
    /// </summary>
    Kilogramos = Kilogramo,
    
    /// <summary>
    /// Alias en plural para Gramo
    /// </summary>
    Gramos = Gramo,
    
    /// <summary>
    /// Alias en plural para Litro
    /// </summary>
    Litros = Litro,
    
    /// <summary>
    /// Alias en plural para Mililitro
    /// </summary>
    Mililitros = Mililitro,
    
    /// <summary>
    /// Alias en plural para Unidad
    /// </summary>
    Unidades = Unidad
}
