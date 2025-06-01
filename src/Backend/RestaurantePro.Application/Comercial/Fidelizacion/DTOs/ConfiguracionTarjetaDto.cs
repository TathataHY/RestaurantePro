namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

/// <summary>
/// Configuración para la creación de tarjetas de fidelización
/// </summary>
public class ConfiguracionTarjetaDto
{
    /// <summary>
    /// Nivel de fidelización de la tarjeta
    /// </summary>
    public NivelFidelizacion Nivel { get; set; } = NivelFidelizacion.Basico;

    /// <summary>
    /// Puntos iniciales a otorgar
    /// </summary>
    public int PuntosIniciales { get; set; } = 0;

    /// <summary>
    /// Multiplicador de puntos por defecto
    /// </summary>
    public decimal MultiplicadorPorDefecto { get; set; } = 1.0m;

    /// <summary>
    /// Días de vigencia de la tarjeta (null = sin vencimiento)
    /// </summary>
    public int? DiasVigencia { get; set; }

    /// <summary>
    /// Requiere activación manual
    /// </summary>
    public bool RequiereActivacion { get; set; } = false;

    /// <summary>
    /// Genera código QR automáticamente
    /// </summary>
    public bool GenerarQR { get; set; } = true;

    /// <summary>
    /// Envía tarjeta por email automáticamente
    /// </summary>
    public bool EnviarPorEmail { get; set; } = true;

    /// <summary>
    /// Envía tarjeta por SMS automáticamente
    /// </summary>
    public bool EnviarPorSMS { get; set; } = false;

    /// <summary>
    /// Template de email a utilizar
    /// </summary>
    public string? TemplateEmail { get; set; }

    /// <summary>
    /// Template de SMS a utilizar
    /// </summary>
    public string? TemplateSMS { get; set; }

    /// <summary>
    /// Beneficios predeterminados para este tipo
    /// </summary>
    public List<Guid> BeneficiosPredeterminados { get; set; } = new();

    /// <summary>
    /// Límite diario de acumulación de puntos
    /// </summary>
    public int? LimiteDiarioAcumulacion { get; set; }

    /// <summary>
    /// Límite mensual de acumulación de puntos
    /// </summary>
    public int? LimiteMensualAcumulacion { get; set; }

    /// <summary>
    /// Configura auditoría automática
    /// </summary>
    public bool AuditoriaAutomatica { get; set; } = true;

    /// <summary>
    /// Nivel de fidelización inicial
    /// </summary>
    public Guid? NivelInicialId { get; set; }

    /// <summary>
    /// Observaciones sobre la configuración
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Datos adicionales de configuración
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    /// <summary>
    /// Configuración específica para el diseño de la tarjeta
    /// </summary>
    public ConfiguracionDiseñoDto? DiseñoTarjeta { get; set; }
}

/// <summary>
/// Configuración del diseño visual de la tarjeta
/// </summary>
public class ConfiguracionDiseñoDto
{
    /// <summary>
    /// Color principal (hex)
    /// </summary>
    public string ColorPrincipal { get; set; } = "#003366";

    /// <summary>
    /// Color secundario (hex)
    /// </summary>
    public string ColorSecundario { get; set; } = "#FFFFFF";

    /// <summary>
    /// Logo de la empresa (base64 o URL)
    /// </summary>
    public string? Logo { get; set; }

    /// <summary>
    /// Imagen de fondo (base64 o URL)
    /// </summary>
    public string? ImagenFondo { get; set; }

    /// <summary>
    /// Fuente para el texto
    /// </summary>
    public string Fuente { get; set; } = "Arial";

    /// <summary>
    /// Tamaño de fuente
    /// </summary>
    public int TamañoFuente { get; set; } = 12;

    /// <summary>
    /// Incluir código QR en el diseño
    /// </summary>
    public bool IncluirQR { get; set; } = true;

    /// <summary>
    /// Posición del QR en la tarjeta
    /// </summary>
    public string PosicionQR { get; set; } = "inferior-derecha";

    /// <summary>
    /// Tamaño del QR en la tarjeta
    /// </summary>
    public int TamañoQR { get; set; } = 80;
} 