namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;

/// <summary>
/// Command para crear una nueva tarjeta de fidelización
/// </summary>
public class CrearTarjetaFidelizacionCommand : IRequest<Result<TarjetaFidelizacionDto>>
{
    /// <summary>
    /// ID del cliente para quien se crea la tarjeta
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Código único de la tarjeta
    /// </summary>
    public string? CodigoTarjeta { get; set; }

    /// <summary>
    /// Puntos iniciales de bienvenida
    /// </summary>
    public int PuntosIniciales { get; set; } = 0;

    /// <summary>
    /// Tipo de tarjeta a crear
    /// </summary>
    public string TipoTarjeta { get; set; } = string.Empty;

    /// <summary>
    /// Configuración específica de la tarjeta
    /// </summary>
    public CrearTarjetaConfiguracion? Configuracion { get; set; }

    /// <summary>
    /// Activar inmediatamente la tarjeta
    /// </summary>
    public bool ActivarInmediatamente { get; set; } = true;

    /// <summary>
    /// Enviar tarjeta por email
    /// </summary>
    public bool EnviarPorEmail { get; set; } = true;

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// ID del usuario que crea la tarjeta
    /// </summary>
    public Guid UsuarioId { get; set; }
}

/// <summary>
/// Configuración para la creación de tarjeta
/// </summary>
public class CrearTarjetaConfiguracion
{
    /// <summary>
    /// Puntos iniciales de bienvenida
    /// </summary>
    public int PuntosIniciales { get; set; } = 0;

    /// <summary>
    /// Multiplicador de puntos
    /// </summary>
    public decimal MultiplicadorPuntos { get; set; } = 1.0m;

    /// <summary>
    /// Fecha de vencimiento personalizada
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Límite de puntos mensual
    /// </summary>
    public int? LimitePuntosMensual { get; set; }

    /// <summary>
    /// Configuraciones especiales
    /// </summary>
    public Dictionary<string, object>? ConfiguracionesEspeciales { get; set; }
} 