namespace RestaurantePro.Domain.Inventario.Services;

/// <summary>
/// Servicio de dominio para validación de operaciones de inventario
/// </summary>
public interface IValidacionInventarioService
{
    /// <summary>
    /// Valida si un ajuste de inventario es válido según las reglas de negocio
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="tipoMovimiento">Tipo de movimiento</param>
    /// <param name="cantidad">Cantidad a ajustar</param>
    /// <param name="stockActual">Stock actual del ingrediente</param>
    /// <returns>Resultado de la validación</returns>
    Task<ResultadoValidacionInventario> ValidarAjusteInventarioAsync(
        Guid ingredienteId,
        RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario tipoMovimiento,
        decimal cantidad,
        decimal stockActual);

    /// <summary>
    /// Valida si es posible realizar una salida de inventario
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="cantidadSalida">Cantidad a retirar</param>
    /// <param name="stockActual">Stock actual</param>
    /// <returns>True si la salida es válida</returns>
    Task<bool> ValidarSalidaInventarioAsync(
        Guid ingredienteId,
        decimal cantidadSalida,
        decimal stockActual);

    /// <summary>
    /// Valida las reglas de negocio para un movimiento de inventario
    /// </summary>
    /// <param name="movimiento">Datos del movimiento a validar</param>
    /// <returns>Resultado de la validación</returns>
    Task<ResultadoValidacionInventario> ValidarReglasNegocioAsync(
        DatosMovimientoInventario movimiento);

    /// <summary>
    /// Verifica la integridad de datos de inventario
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <returns>Resultado de la verificación</returns>
    Task<ResultadoValidacionInventario> VerificarIntegridadDatosAsync(Guid ingredienteId);
}

/// <summary>
/// Resultado de una validación de inventario
/// </summary>
public class ResultadoValidacionInventario
{
    /// <summary>
    /// Indica si la validación fue exitosa
    /// </summary>
    public bool EsValido { get; private set; }

    /// <summary>
    /// Lista de errores encontrados
    /// </summary>
    public IReadOnlyList<string> Errores { get; private set; } = new List<string>();

    /// <summary>
    /// Lista de advertencias
    /// </summary>
    public IReadOnlyList<string> Advertencias { get; private set; } = new List<string>();

    /// <summary>
    /// Datos adicionales de la validación
    /// </summary>
    public IReadOnlyDictionary<string, object> DatosAdicionales { get; private set; } = new Dictionary<string, object>();

    private ResultadoValidacionInventario(bool esValido, List<string> errores, List<string> advertencias, Dictionary<string, object> datosAdicionales)
    {
        EsValido = esValido;
        Errores = errores.AsReadOnly();
        Advertencias = advertencias.AsReadOnly();
        DatosAdicionales = datosAdicionales.AsReadOnly();
    }

    /// <summary>
    /// Crea un resultado válido
    /// </summary>
    public static ResultadoValidacionInventario Exitoso() => 
        new(true, new List<string>(), new List<string>(), new Dictionary<string, object>());

    /// <summary>
    /// Crea un resultado con errores
    /// </summary>
    public static ResultadoValidacionInventario ConErrores(params string[] errores) => 
        new(false, errores.ToList(), new List<string>(), new Dictionary<string, object>());

    /// <summary>
    /// Crea un resultado con advertencias
    /// </summary>
    public static ResultadoValidacionInventario ConAdvertencias(params string[] advertencias) => 
        new(true, new List<string>(), advertencias.ToList(), new Dictionary<string, object>());
}

/// <summary>
/// Datos de un movimiento de inventario para validación
/// </summary>
public class DatosMovimientoInventario
{
    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Tipo de movimiento
    /// </summary>
    public string TipoMovimiento { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad del movimiento
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Motivo del movimiento
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que realiza el movimiento
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Fecha del movimiento
    /// </summary>
    public DateTime FechaMovimiento { get; set; }

    /// <summary>
    /// Stock actual antes del movimiento
    /// </summary>
    public decimal StockAnterior { get; set; }
} 