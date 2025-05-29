namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

/// <summary>
/// DTO para la respuesta de creación de tarjeta de fidelización
/// Contiene información completa de la tarjeta creada
/// </summary>
public class TarjetaFidelizacionDto
{
    /// <summary>
    /// ID único de la tarjeta de fidelización
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del cliente propietario
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Información básica del cliente
    /// </summary>
    public ClienteBasicoDto Cliente { get; set; } = new();

    /// <summary>
    /// Número único de la tarjeta
    /// </summary>
    public string NumeroTarjeta { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de tarjeta de fidelización
    /// </summary>
    public string TipoTarjeta { get; set; } = string.Empty;

    /// <summary>
    /// Nivel actual de fidelización
    /// </summary>
    public string NivelFidelizacion { get; set; } = string.Empty;

    /// <summary>
    /// Saldo actual de puntos
    /// </summary>
    public int SaldoPuntos { get; set; }

    /// <summary>
    /// Total de puntos acumulados históricamente
    /// </summary>
    public int TotalPuntosAcumulados { get; set; }

    /// <summary>
    /// Total de puntos canjeados históricamente
    /// </summary>
    public int TotalPuntosCanjeados { get; set; }

    /// <summary>
    /// Estado actual de la tarjeta
    /// </summary>
    public EstadoTarjeta Estado { get; set; }

    /// <summary>
    /// Indica si es la tarjeta principal del cliente
    /// </summary>
    public bool EsPrincipal { get; set; }

    /// <summary>
    /// Fecha de emisión de la tarjeta
    /// </summary>
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Fecha de activación de la tarjeta
    /// </summary>
    public DateTime? FechaActivacion { get; set; }

    /// <summary>
    /// Fecha de vencimiento de la tarjeta
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Fecha de la última transacción
    /// </summary>
    public DateTime? UltimaTransaccion { get; set; }

    /// <summary>
    /// Configuración específica de la tarjeta
    /// </summary>
    public ConfiguracionTarjetaDto Configuracion { get; set; } = new();

    /// <summary>
    /// Personalización del diseño
    /// </summary>
    public PersonalizacionTarjetaDto? Personalizacion { get; set; }

    /// <summary>
    /// Beneficios activos de la tarjeta
    /// </summary>
    public List<BeneficioTarjetaDto> BeneficiosActivos { get; set; } = new();

    /// <summary>
    /// Información del siguiente nivel
    /// </summary>
    public SiguienteNivelDto? SiguienteNivel { get; set; }

    /// <summary>
    /// Historial reciente de transacciones
    /// </summary>
    public List<TransaccionRecienteDto> TransaccionesRecientes { get; set; } = new();

    /// <summary>
    /// Recompensas disponibles para canjear
    /// </summary>
    public List<RecompensaDisponibleDto> RecompensasDisponibles { get; set; } = new();

    /// <summary>
    /// Logros obtenidos
    /// </summary>
    public List<LogroObtenidoDto> LogrosObtenidos { get; set; } = new();

    /// <summary>
    /// Puntos que expiran próximamente
    /// </summary>
    public PuntosProximosVencerDto? PuntosProximosVencer { get; set; }

    /// <summary>
    /// Estadísticas de la tarjeta
    /// </summary>
    public EstadisticasTarjetaDto Estadisticas { get; set; } = new();

    /// <summary>
    /// Sucursal de emisión
    /// </summary>
    public string? SucursalEmision { get; set; }

    /// <summary>
    /// Canal de registro
    /// </summary>
    public string Canal { get; set; } = string.Empty;

    /// <summary>
    /// Motivo de emisión
    /// </summary>
    public string? MotivoEmision { get; set; }

    /// <summary>
    /// QR Code de la tarjeta para verificación rápida
    /// </summary>
    public string? QrCode { get; set; }

    /// <summary>
    /// Código de barras de la tarjeta
    /// </summary>
    public string? CodigoBarras { get; set; }

    /// <summary>
    /// URL para ver la tarjeta digital
    /// </summary>
    public string? UrlTarjetaDigital { get; set; }

    /// <summary>
    /// Datos adicionales de la tarjeta
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    /// <summary>
    /// Preferencias del cliente
    /// </summary>
    public Dictionary<string, string>? PreferenciasCliente { get; set; }
}

/// <summary>
/// Estados posibles de una tarjeta de fidelización
/// </summary>
public enum EstadoTarjeta
{
    /// <summary>
    /// Tarjeta activa y funcional
    /// </summary>
    Activa = 1,

    /// <summary>
    /// Tarjeta creada pero no activada
    /// </summary>
    Pendiente = 2,

    /// <summary>
    /// Tarjeta temporalmente suspendida
    /// </summary>
    Suspendida = 3,

    /// <summary>
    /// Tarjeta bloqueada por seguridad
    /// </summary>
    Bloqueada = 4,

    /// <summary>
    /// Tarjeta expirada
    /// </summary>
    Expirada = 5,

    /// <summary>
    /// Tarjeta cancelada permanentemente
    /// </summary>
    Cancelada = 6
}

/// <summary>
/// DTO para información básica del cliente
/// </summary>
public class ClienteBasicoDto
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Fecha de nacimiento
    /// </summary>
    public DateTime? FechaNacimiento { get; set; }

    /// <summary>
    /// Fecha de registro
    /// </summary>
    public DateTime FechaRegistro { get; set; }
}

/// <summary>
/// DTO para configuración de la tarjeta
/// </summary>
public class ConfiguracionTarjetaDto
{
    /// <summary>
    /// Multiplicador de puntos por compra
    /// </summary>
    public decimal MultiplicadorPuntos { get; set; }

    /// <summary>
    /// Descuento base en compras
    /// </summary>
    public decimal DescuentoBase { get; set; }

    /// <summary>
    /// Límite diario de puntos
    /// </summary>
    public int? LimitePuntosDiario { get; set; }

    /// <summary>
    /// Límite mensual de puntos
    /// </summary>
    public int? LimitePuntosMensual { get; set; }

    /// <summary>
    /// Días hasta que expiren los puntos
    /// </summary>
    public int? DiasExpiracionPuntos { get; set; }

    /// <summary>
    /// Permite acumular en promociones
    /// </summary>
    public bool AcumularEnPromociones { get; set; }

    /// <summary>
    /// Permite canjear por descuentos
    /// </summary>
    public bool PermiteCanjearDescuentos { get; set; }

    /// <summary>
    /// Acceso a eventos exclusivos
    /// </summary>
    public bool AccesoEventosExclusivos { get; set; }

    /// <summary>
    /// Notificaciones activas
    /// </summary>
    public bool NotificacionesActivas { get; set; }
}

/// <summary>
/// DTO para personalización de la tarjeta
/// </summary>
public class PersonalizacionTarjetaDto
{
    /// <summary>
    /// Color principal
    /// </summary>
    public string? ColorPrincipal { get; set; }

    /// <summary>
    /// Color secundario
    /// </summary>
    public string? ColorSecundario { get; set; }

    /// <summary>
    /// Template de diseño
    /// </summary>
    public string? DisenyoTemplate { get; set; }

    /// <summary>
    /// Logo personalizado
    /// </summary>
    public string? LogoPersonalizado { get; set; }

    /// <summary>
    /// Texto personalizado
    /// </summary>
    public string? TextoPersonalizado { get; set; }

    /// <summary>
    /// Imagen de fondo
    /// </summary>
    public string? ImagenFondo { get; set; }

    /// <summary>
    /// Tipo de fuente
    /// </summary>
    public string? TipoFuente { get; set; }
}

/// <summary>
/// DTO para beneficios de la tarjeta
/// </summary>
public class BeneficioTarjetaDto
{
    /// <summary>
    /// ID del beneficio
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del beneficio
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del beneficio
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de beneficio
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Valor del beneficio
    /// </summary>
    public string Valor { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de activación
    /// </summary>
    public DateTime FechaActivacion { get; set; }

    /// <summary>
    /// Fecha de vencimiento
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Está activo
    /// </summary>
    public bool EstaActivo { get; set; }
}

/// <summary>
/// DTO para información del siguiente nivel
/// </summary>
public class SiguienteNivelDto
{
    /// <summary>
    /// Nombre del siguiente nivel
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Puntos necesarios para alcanzar el siguiente nivel
    /// </summary>
    public int PuntosNecesarios { get; set; }

    /// <summary>
    /// Puntos que faltan
    /// </summary>
    public int PuntosFaltantes { get; set; }

    /// <summary>
    /// Porcentaje de progreso
    /// </summary>
    public decimal PorcentajeProgreso { get; set; }

    /// <summary>
    /// Beneficios del siguiente nivel
    /// </summary>
    public List<string> BeneficiosProximoNivel { get; set; } = new();

    /// <summary>
    /// Tiempo estimado para alcanzar el nivel
    /// </summary>
    public string? TiempoEstimado { get; set; }
}

/// <summary>
/// DTO para transacciones recientes
/// </summary>
public class TransaccionRecienteDto
{
    /// <summary>
    /// ID de la transacción
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tipo de transacción
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Puntos de la transacción
    /// </summary>
    public int Puntos { get; set; }

    /// <summary>
    /// Descripción
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de la transacción
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Saldo después de la transacción
    /// </summary>
    public int SaldoDespues { get; set; }
}

/// <summary>
/// DTO para puntos próximos a vencer
/// </summary>
public class PuntosProximosVencerDto
{
    /// <summary>
    /// Cantidad de puntos que vencen
    /// </summary>
    public int CantidadPuntos { get; set; }

    /// <summary>
    /// Fecha de vencimiento
    /// </summary>
    public DateTime FechaVencimiento { get; set; }

    /// <summary>
    /// Días restantes
    /// </summary>
    public int DiasRestantes { get; set; }

    /// <summary>
    /// Recomendaciones para usar los puntos
    /// </summary>
    public List<string> Recomendaciones { get; set; } = new();
}

/// <summary>
/// DTO para estadísticas de la tarjeta
/// </summary>
public class EstadisticasTarjetaDto
{
    /// <summary>
    /// Total de transacciones
    /// </summary>
    public int TotalTransacciones { get; set; }

    /// <summary>
    /// Compra promedio
    /// </summary>
    public decimal CompraPromedio { get; set; }

    /// <summary>
    /// Puntos promedio por mes
    /// </summary>
    public int PuntosPromedioMes { get; set; }

    /// <summary>
    /// Mes más activo
    /// </summary>
    public string? MesMasActivo { get; set; }

    /// <summary>
    /// Días desde la última visita
    /// </summary>
    public int DiasSinVisitas { get; set; }

    /// <summary>
    /// Frecuencia de visitas (visitas por mes)
    /// </summary>
    public decimal FrecuenciaVisitas { get; set; }

    /// <summary>
    /// Categoría de producto más comprada
    /// </summary>
    public string? CategoriaMasComprada { get; set; }

    /// <summary>
    /// Total gastado (en dinero)
    /// </summary>
    public decimal TotalGastado { get; set; }

    /// <summary>
    /// Total ahorrado con la tarjeta
    /// </summary>
    public decimal TotalAhorrado { get; set; }
} 