namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Servicio para envío de tarjetas de fidelización
/// </summary>
public interface IEnvioTarjetaService
{
    /// <summary>
    /// Envía tarjeta por email
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta</param>
    /// <param name="email">Email destino</param>
    /// <param name="parametros">Parámetros del envío</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del envío</returns>
    Task<Result<ResultadoEnvio>> EnviarPorEmailAsync(Guid tarjetaId, string email, ParametrosEnvio? parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía tarjeta por SMS
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta</param>
    /// <param name="telefono">Teléfono destino</param>
    /// <param name="parametros">Parámetros del envío</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del envío</returns>
    Task<Result<ResultadoEnvio>> EnviarPorSmsAsync(Guid tarjetaId, string telefono, ParametrosEnvio? parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera archivo para impresión de tarjeta
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta</param>
    /// <param name="formato">Formato de salida (PDF, PNG, etc.)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Archivo generado</returns>
    Task<Result<ArchivoTarjeta>> GenerarParaImpresionAsync(Guid tarjetaId, string formato = "PDF", CancellationToken cancellationToken = default);

    /// <summary>
    /// Programa el envío físico de una tarjeta
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta</param>
    /// <param name="direccionEnvio">Dirección de envío</param>
    /// <param name="fechaEstimada">Fecha estimada de envío</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la programación</returns>
    Task<Result<ResultadoEnvio>> ProgramarEnvioAsync(Guid tarjetaId, string direccionEnvio, DateTime fechaEstimada, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado del envío de tarjeta
/// </summary>
public class ResultadoEnvio
{
    public bool Exitoso { get; set; }
    public string IdEnvio { get; set; } = string.Empty;
    public DateTime FechaEnvio { get; set; } = DateTime.Now;
    public string CanalEnvio { get; set; } = string.Empty;
    public string? MensajeError { get; set; }
}

/// <summary>
/// Parámetros para el envío
/// </summary>
public class ParametrosEnvio
{
    public string? Plantilla { get; set; }
    public Dictionary<string, object>? VariablesPersonalizacion { get; set; }
    public bool IncluirQR { get; set; } = true;
    public bool IncluirInstrucciones { get; set; } = true;
}

/// <summary>
/// Archivo de tarjeta generado
/// </summary>
public class ArchivoTarjeta
{
    public string NombreArchivo { get; set; } = string.Empty;
    public string ContenidoBase64 { get; set; } = string.Empty;
    public string TipoMime { get; set; } = string.Empty;
    public long TamañoBytes { get; set; }
} 