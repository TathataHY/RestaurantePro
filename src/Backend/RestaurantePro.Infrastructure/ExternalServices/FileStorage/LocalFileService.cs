using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Infrastructure.ExternalServices.FileStorage
{
    /// <summary>
    /// Implementación del servicio de almacenamiento de archivos local
    /// </summary>
    public class LocalFileService : IFileStorageService
    {
        private readonly ILogger<LocalFileService> _logger;
        private readonly string _baseDirectory;
        private readonly string _baseUrl;
        
        /// <summary>
        /// Constructor para LocalFileService
        /// </summary>
        public LocalFileService(
            IConfiguration configuration,
            ILogger<LocalFileService> logger)
        {
            _logger = logger;
            _baseDirectory = configuration["FileStorage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            _baseUrl = configuration["FileStorage:BaseUrl"] ?? "/files";
            
            // Asegurar que el directorio base exista
            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
            }
        }
        
        /// <summary>
        /// Sube un archivo al sistema de archivos local
        /// </summary>
        public async Task<Result<string>> SubirArchivoAsync(
            DatosArchivo archivo, 
            string ruta, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var directorioDestino = Path.Combine(_baseDirectory, ruta.TrimStart('/'));
                
                if (!Directory.Exists(directorioDestino))
                {
                    Directory.CreateDirectory(directorioDestino);
                }
                
                var archivoRuta = Path.Combine(directorioDestino, archivo.NombreArchivo);
                
                await File.WriteAllBytesAsync(archivoRuta, archivo.Contenido, cancellationToken);
                
                _logger.LogInformation("Archivo guardado localmente: {RutaCompleta}", archivoRuta);
                
                string urlArchivo = Path.Combine(_baseUrl, ruta.TrimStart('/'), archivo.NombreArchivo).Replace("\\", "/");
                return Result.Success(urlArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar archivo local en {Ruta}", ruta);
                return Result.Failure<string>($"Error al guardar archivo local: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Descarga un archivo del sistema de archivos local
        /// </summary>
        public async Task<Result<DatosArchivo>> DescargarArchivoAsync(
            string url, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var rutaRelativa = url.Replace(_baseUrl, "").TrimStart('/');
                var rutaCompleta = Path.Combine(_baseDirectory, rutaRelativa);
                
                if (!File.Exists(rutaCompleta))
                {
                    _logger.LogWarning("Archivo local no encontrado: {RutaCompleta}", rutaCompleta);
                    return Result.Failure<DatosArchivo>($"Archivo no encontrado: {url}");
                }
                
                var contenido = await File.ReadAllBytesAsync(rutaCompleta, cancellationToken);
                var nombreArchivo = Path.GetFileName(rutaCompleta);
                
                var archivo = new DatosArchivo
                {
                    NombreArchivo = nombreArchivo,
                    Contenido = contenido,
                    TipoMime = ObtenerTipoMime(nombreArchivo),
                    TamañoBytes = contenido.Length
                };
                
                return Result.Success(archivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener archivo local de {Url}", url);
                return Result.Failure<DatosArchivo>($"Error al descargar archivo: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Elimina un archivo del sistema de archivos local
        /// </summary>
        public async Task<Result<bool>> EliminarArchivoAsync(
            string url, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var rutaRelativa = url.Replace(_baseUrl, "").TrimStart('/');
                var rutaCompleta = Path.Combine(_baseDirectory, rutaRelativa);
                
                if (!File.Exists(rutaCompleta))
                {
                    _logger.LogWarning("Intento de eliminar archivo local inexistente: {RutaCompleta}", rutaCompleta);
                    return Result.Success(false);
                }
                
                File.Delete(rutaCompleta);
                _logger.LogInformation("Archivo local eliminado: {RutaCompleta}", rutaCompleta);
                
                await Task.CompletedTask; // Para mantener la firma async
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar archivo local de {Url}", url);
                return Result.Failure<bool>($"Error al eliminar archivo: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Obtiene una URL temporal para un archivo local
        /// </summary>
        public Task<Result<string>> ObtenerUrlTemporalAsync(
            string url, 
            TimeSpan tiempoExpiracion, 
            CancellationToken cancellationToken = default)
        {
            // Para archivos locales, simplemente devolvemos la misma URL
            // En implementaciones avanzadas, podríamos generar tokens de acceso temporal
            return Task.FromResult(Result.Success(url));
        }
        
        /// <summary>
        /// Obtiene el tipo MIME basado en la extensión del archivo
        /// </summary>
        private string ObtenerTipoMime(string nombreArchivo)
        {
            var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
            
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".doc" or ".docx" => "application/msword",
                ".xls" or ".xlsx" => "application/vnd.ms-excel",
                ".zip" => "application/zip",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
} 
