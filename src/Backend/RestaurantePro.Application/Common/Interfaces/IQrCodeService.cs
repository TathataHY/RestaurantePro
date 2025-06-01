namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Servicio para generar códigos QR
/// </summary>
public interface IQrCodeService
{
    /// <summary>
    /// Genera un código QR para una tarjeta de fidelización
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta</param>
    /// <param name="parametros">Parámetros adicionales</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Código QR generado</returns>
    Task<Result<CodigoQrResultado>> GenerarQrTarjetaFidelizacionAsync(Guid tarjetaId, ParametrosQr? parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un código QR genérico
    /// </summary>
    /// <param name="contenido">Contenido del QR</param>
    /// <param name="parametros">Parámetros de configuración</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Código QR generado</returns>
    Task<Result<CodigoQrResultado>> GenerarQrAsync(string contenido, ParametrosQr? parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida un código QR
    /// </summary>
    /// <param name="codigoQr">Código QR a validar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del QR validado</returns>
    Task<Result<InformacionQr>> ValidarQrAsync(string codigoQr, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decodifica un código QR
    /// </summary>
    /// <param name="imagenQr">Imagen del código QR en base64</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Contenido decodificado</returns>
    Task<Result<string>> DecodificarQrAsync(string imagenQr, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado de la generación de código QR
/// </summary>
public class CodigoQrResultado
{
    /// <summary>
    /// Código único del QR
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Imagen del QR en base64
    /// </summary>
    public string ImagenBase64 { get; set; } = string.Empty;

    /// <summary>
    /// URL del QR (si aplica)
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Fecha de generación
    /// </summary>
    public DateTime FechaGeneracion { get; set; } = DateTime.Now;

    /// <summary>
    /// Fecha de vencimiento (si aplica)
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Contenido del QR
    /// </summary>
    public string Contenido { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de QR
    /// </summary>
    public string Tipo { get; set; } = string.Empty;
}

/// <summary>
/// Parámetros para la generación de códigos QR
/// </summary>
public class ParametrosQr
{
    /// <summary>
    /// Tamaño del QR en píxeles
    /// </summary>
    public int Tamaño { get; set; } = 300;

    /// <summary>
    /// Formato de la imagen (PNG, JPEG, SVG)
    /// </summary>
    public string Formato { get; set; } = "PNG";

    /// <summary>
    /// Nivel de corrección de errores
    /// </summary>
    public NivelCorreccionQr NivelCorreccion { get; set; } = NivelCorreccionQr.Medio;

    /// <summary>
    /// Color de primer plano (hex)
    /// </summary>
    public string ColorFrente { get; set; } = "#000000";

    /// <summary>
    /// Color de fondo (hex)
    /// </summary>
    public string ColorFondo { get; set; } = "#FFFFFF";

    /// <summary>
    /// Incluir logo (si aplica)
    /// </summary>
    public bool IncluirLogo { get; set; } = false;

    /// <summary>
    /// Logo en base64 (si se incluye)
    /// </summary>
    public string? LogoBase64 { get; set; }

    /// <summary>
    /// Margen alrededor del QR
    /// </summary>
    public int Margen { get; set; } = 10;

    /// <summary>
    /// Fecha de vencimiento del QR
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Datos adicionales para incluir
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }
}

/// <summary>
/// Información de un código QR validado
/// </summary>
public class InformacionQr
{
    /// <summary>
    /// Código del QR
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de QR
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Contenido decodificado
    /// </summary>
    public string Contenido { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de generación
    /// </summary>
    public DateTime FechaGeneracion { get; set; }

    /// <summary>
    /// Fecha de vencimiento
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Indica si el QR está vigente
    /// </summary>
    public bool EsVigente { get; set; }

    /// <summary>
    /// ID de la entidad asociada (tarjeta, promoción, etc.)
    /// </summary>
    public Guid? EntidadId { get; set; }

    /// <summary>
    /// Datos adicionales decodificados
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }
}

/// <summary>
/// Niveles de corrección de errores para códigos QR
/// </summary>
public enum NivelCorreccionQr
{
    /// <summary>
    /// Bajo (~7% de corrección)
    /// </summary>
    Bajo = 1,

    /// <summary>
    /// Medio (~15% de corrección)
    /// </summary>
    Medio = 2,

    /// <summary>
    /// Cuartil (~25% de corrección)
    /// </summary>
    Cuartil = 3,

    /// <summary>
    /// Alto (~30% de corrección)
    /// </summary>
    Alto = 4
} 