namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Servicio de almacenamiento de archivos
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Sube un archivo
    /// </summary>
    /// <param name="archivo">Datos del archivo</param>
    /// <param name="ruta">Ruta donde guardar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>URL del archivo subido</returns>
    Task<Result<string>> SubirArchivoAsync(DatosArchivo archivo, string ruta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Descarga un archivo
    /// </summary>
    /// <param name="url">URL del archivo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Datos del archivo</returns>
    Task<Result<DatosArchivo>> DescargarArchivoAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un archivo
    /// </summary>
    /// <param name="url">URL del archivo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se eliminó correctamente</returns>
    Task<Result<bool>> EliminarArchivoAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una URL temporal para acceso directo
    /// </summary>
    /// <param name="url">URL del archivo</param>
    /// <param name="tiempoExpiracion">Tiempo de expiración</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>URL temporal</returns>
    Task<Result<string>> ObtenerUrlTemporalAsync(string url, TimeSpan tiempoExpiracion, CancellationToken cancellationToken = default);
}

/// <summary>
/// Datos de un archivo
/// </summary>
public class DatosArchivo
{
    public string NombreArchivo { get; set; } = string.Empty;
    public byte[] Contenido { get; set; } = Array.Empty<byte>();
    public string TipoMime { get; set; } = string.Empty;
    public long TamañoBytes { get; set; }
    public Dictionary<string, string>? Metadatos { get; set; }
} 