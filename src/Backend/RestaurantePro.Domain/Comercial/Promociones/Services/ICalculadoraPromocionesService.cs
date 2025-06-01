namespace RestaurantePro.Domain.Comercial.Promociones.Services;

/// <summary>
/// Servicio de dominio para calcular promociones aplicables
/// </summary>
public interface ICalculadoraPromocionesService
{
    /// <summary>
    /// Calcula promociones aplicables para una compra
    /// </summary>
    /// <param name="parametros">Parámetros de la compra</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de promociones aplicables</returns>
    Task<List<PromocionAplicable>> CalcularPromocionesAplicablesAsync(ParametrosCompra parametros, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aplica una promoción específica
    /// </summary>
    /// <param name="promocionId">ID de la promoción</param>
    /// <param name="parametros">Parámetros de la compra</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la aplicación</returns>
    Task<Result<ResultadoAplicacionPromocion>> AplicarPromocionAsync(Guid promocionId, ParametrosCompra parametros, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida si una promoción es aplicable
    /// </summary>
    /// <param name="promocionId">ID de la promoción</param>
    /// <param name="parametros">Parámetros de la compra</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si es aplicable</returns>
    Task<Result<bool>> ValidarAplicabilidadAsync(Guid promocionId, ParametrosCompra parametros, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el mejor combo de promociones
    /// </summary>
    /// <param name="parametros">Parámetros de la compra</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Combo óptimo de promociones</returns>
    Task<Result<ComboPromociones>> ObtenerMejorComboAsync(ParametrosCompra parametros, CancellationToken cancellationToken = default);
}

/// <summary>
/// Promoción aplicable a una compra
/// </summary>
public class PromocionAplicable
{
    /// <summary>
    /// ID de la promoción
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Código de la promoción
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la promoción
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la promoción
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de promoción
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Descuento en pesos
    /// </summary>
    public decimal DescuentoPesos { get; set; }

    /// <summary>
    /// Descuento en porcentaje
    /// </summary>
    public decimal DescuentoPorcentaje { get; set; }

    /// <summary>
    /// Puntos otorgados
    /// </summary>
    public int PuntosOtorgados { get; set; }

    /// <summary>
    /// Prioridad de aplicación
    /// </summary>
    public int Prioridad { get; set; }

    /// <summary>
    /// Condiciones de aplicación
    /// </summary>
    public string Condiciones { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de vencimiento
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Ahorro estimado
    /// </summary>
    public decimal AhorroEstimado { get; set; }
}

/// <summary>
/// Resultado de aplicar una promoción
/// </summary>
public class ResultadoAplicacionPromocion
{
    /// <summary>
    /// ID de la promoción aplicada
    /// </summary>
    public Guid PromocionId { get; set; }

    /// <summary>
    /// Código de la promoción
    /// </summary>
    public string CodigoPromocion { get; set; } = string.Empty;

    /// <summary>
    /// Descuento aplicado en pesos
    /// </summary>
    public decimal DescuentoAplicado { get; set; }

    /// <summary>
    /// Puntos otorgados
    /// </summary>
    public int PuntosOtorgados { get; set; }

    /// <summary>
    /// Monto original
    /// </summary>
    public decimal MontoOriginal { get; set; }

    /// <summary>
    /// Monto final después del descuento
    /// </summary>
    public decimal MontoFinal { get; set; }

    /// <summary>
    /// Porcentaje de descuento aplicado
    /// </summary>
    public decimal PorcentajeDescuento { get; set; }

    /// <summary>
    /// Detalle de los cálculos
    /// </summary>
    public string DetalleCalculo { get; set; } = string.Empty;

    /// <summary>
    /// Artículos afectados por la promoción
    /// </summary>
    public List<string> ArticulosAfectados { get; set; } = new();
}

/// <summary>
/// Parámetros para cálculo de promociones
/// </summary>
public class ParametrosCompra
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Monto total de la compra
    /// </summary>
    public decimal MontoTotal { get; set; }

    /// <summary>
    /// Items de la compra
    /// </summary>
    public List<ItemCompra> Items { get; set; } = new();

    /// <summary>
    /// Fecha de la compra
    /// </summary>
    public DateTime FechaCompra { get; set; } = DateTime.Now;

    /// <summary>
    /// Canal de venta
    /// </summary>
    public string Canal { get; set; } = string.Empty;

    /// <summary>
    /// Sucursal
    /// </summary>
    public string Sucursal { get; set; } = string.Empty;

    /// <summary>
    /// Métodos de pago
    /// </summary>
    public List<string> MetodosPago { get; set; } = new();

    /// <summary>
    /// Códigos de promoción aplicados
    /// </summary>
    public List<string> CodigosPromocion { get; set; } = new();
}

/// <summary>
/// Item de compra para cálculo de promociones
/// </summary>
public class ItemCompra
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// SKU del producto
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Categoría del producto
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Subtotal del item
    /// </summary>
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

/// <summary>
/// Combo de promociones optimizado
/// </summary>
public class ComboPromociones
{
    /// <summary>
    /// Promociones en el combo
    /// </summary>
    public List<PromocionAplicable> Promociones { get; set; } = new();

    /// <summary>
    /// Descuento total
    /// </summary>
    public decimal DescuentoTotal { get; set; }

    /// <summary>
    /// Puntos totales
    /// </summary>
    public int PuntosTotales { get; set; }

    /// <summary>
    /// Ahorro total
    /// </summary>
    public decimal AhorroTotal { get; set; }

    /// <summary>
    /// Descripción del combo
    /// </summary>
    public string DescripcionCombo { get; set; } = string.Empty;
} 