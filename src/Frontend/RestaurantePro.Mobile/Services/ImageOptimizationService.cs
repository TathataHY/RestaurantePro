using System.Diagnostics;

namespace RestaurantePro.Mobile.Services;

/// <summary>
/// Servicio para optimización de imágenes - V4
/// </summary>
public class ImageOptimizationService
{
    private static ImageOptimizationService _instance;
    private static readonly object _lock = new object();

    public static ImageOptimizationService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ImageOptimizationService();
                    }
                }
            }
            return _instance;
        }
    }

    private ImageOptimizationService()
    {
    }

    /// <summary>
    /// Obtiene una imagen optimizada con lazy loading
    /// </summary>
    public async Task<ImageSource> GetOptimizedImageAsync(string imageUrl, int? maxWidth = null, int? maxHeight = null)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return GetDefaultImage();
            }

            // Verificar si la imagen está en caché
            var cachedImage = await GetCachedImageAsync(imageUrl);
            if (cachedImage != null)
            {
                return cachedImage;
            }

            // Descargar y optimizar la imagen
            var optimizedImage = await DownloadAndOptimizeImageAsync(imageUrl, maxWidth, maxHeight);
            
            // Guardar en caché
            await CacheImageAsync(imageUrl, optimizedImage);
            
            return optimizedImage;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error optimizing image {imageUrl}: {ex.Message}");
            return GetDefaultImage();
        }
    }

    /// <summary>
    /// Descarga y optimiza una imagen
    /// </summary>
    private async Task<ImageSource> DownloadAndOptimizeImageAsync(string imageUrl, int? maxWidth, int? maxHeight)
    {
        try
        {
            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            
            // Aquí podríamos implementar optimización real de imagen
            // Por ahora, simplemente creamos el ImageSource
            return ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error downloading image {imageUrl}: {ex.Message}");
            return GetDefaultImage();
        }
    }

    /// <summary>
    /// Obtiene una imagen del caché
    /// </summary>
    private async Task<ImageSource> GetCachedImageAsync(string imageUrl)
    {
        try
        {
            var cacheKey = GetCacheKey(imageUrl);
            var cachedPath = Path.Combine(FileSystem.CacheDirectory, cacheKey);
            
            if (File.Exists(cachedPath))
            {
                return ImageSource.FromFile(cachedPath);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting cached image: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Guarda una imagen en caché
    /// </summary>
    private async Task CacheImageAsync(string imageUrl, ImageSource imageSource)
    {
        try
        {
            var cacheKey = GetCacheKey(imageUrl);
            var cachePath = Path.Combine(FileSystem.CacheDirectory, cacheKey);
            
            // Implementar guardado en caché
            // Por ahora, solo registramos la intención
            Debug.WriteLine($"Caching image: {cacheKey}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error caching image: {ex.Message}");
        }
    }

    /// <summary>
    /// Genera una clave de caché para la URL de la imagen
    /// </summary>
    private string GetCacheKey(string imageUrl)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(imageUrl));
        return Convert.ToBase64String(hashBytes).Replace("/", "_").Replace("+", "-").Substring(0, 16) + ".jpg";
    }

    /// <summary>
    /// Obtiene una imagen por defecto
    /// </summary>
    private ImageSource GetDefaultImage()
    {
        return ImageSource.FromFile("placeholder.png");
    }

    /// <summary>
    /// Limpia el caché de imágenes
    /// </summary>
    public async Task ClearImageCacheAsync()
    {
        try
        {
            var cacheDirectory = FileSystem.CacheDirectory;
            var cacheFiles = Directory.GetFiles(cacheDirectory, "*.jpg");
            
            foreach (var file in cacheFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deleting cache file {file}: {ex.Message}");
                }
            }
            
            Debug.WriteLine($"Cleared {cacheFiles.Length} cached images");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error clearing image cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el tamaño del caché de imágenes
    /// </summary>
    public async Task<long> GetImageCacheSizeAsync()
    {
        try
        {
            var cacheDirectory = FileSystem.CacheDirectory;
            var cacheFiles = Directory.GetFiles(cacheDirectory, "*.jpg");
            
            long totalSize = 0;
            foreach (var file in cacheFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting file size for {file}: {ex.Message}");
                }
            }
            
            return totalSize;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting cache size: {ex.Message}");
            return 0;
        }
    }
} 