using MediatR;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;

/// <summary>
/// Command para crear una nueva tarjeta de fidelización
/// Gestiona la creación, asignación y configuración inicial de tarjetas para clientes
/// </summary>
public class CrearTarjetaFidelizacionCommand : IRequest<Result<TarjetaFidelizacionDto>>
{
    /// <summary>
    /// ID del cliente al que se asignará la tarjeta
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Tipo de tarjeta de fidelización
    /// </summary>
    public TipoTarjetaFidelizacion TipoTarjeta { get; set; } = TipoTarjetaFidelizacion.Estandar;

    /// <summary>
    /// Número de tarjeta personalizado (opcional)
    /// Si no se especifica, se genera automáticamente
    /// </summary>
    public string? NumeroTarjeta { get; set; }

    /// <summary>
    /// Nivel inicial de fidelización
    /// </summary>
    public NivelFidelizacion NivelInicial { get; set; } = NivelFidelizacion.Bronce;

    /// <summary>
    /// Puntos iniciales a otorgar (promoción de bienvenida)
    /// </summary>
    public int PuntosIniciales { get; set; } = 0;

    /// <summary>
    /// Código de promoción para tarjeta especial
    /// </summary>
    public string? CodigoPromocion { get; set; }

    /// <summary>
    /// Indica si esta tarjeta será la principal del cliente
    /// </summary>
    public bool EsPrincipal { get; set; } = true;

    /// <summary>
    /// Fecha de activación de la tarjeta
    /// Si no se especifica, se activa inmediatamente
    /// </summary>
    public DateTime? FechaActivacion { get; set; }

    /// <summary>
    /// Fecha de vencimiento de la tarjeta
    /// Si no se especifica, usa el vencimiento estándar según el tipo
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Configuraciones especiales de la tarjeta
    /// </summary>
    public ConfiguracionTarjeta? Configuracion { get; set; }

    /// <summary>
    /// Sucursal donde se emite la tarjeta
    /// </summary>
    public string? SucursalEmision { get; set; }

    /// <summary>
    /// Empleado que autoriza la emisión
    /// </summary>
    public Guid? EmpleadoAutorizador { get; set; }

    /// <summary>
    /// Canal a través del cual se solicita la tarjeta
    /// </summary>
    public string Canal { get; set; } = "Sucursal";

    /// <summary>
    /// Motivo de emisión de la tarjeta
    /// </summary>
    public string? MotivoEmision { get; set; }

    /// <summary>
    /// Beneficios especiales a activar
    /// </summary>
    public List<string>? BeneficiosEspeciales { get; set; }

    /// <summary>
    /// Notificaciones a enviar al cliente
    /// </summary>
    public bool NotificarCliente { get; set; } = true;

    /// <summary>
    /// Enviar tarjeta física por correo
    /// </summary>
    public bool EnviarTarjetaFisica { get; set; } = false;

    /// <summary>
    /// Dirección de envío para tarjeta física
    /// </summary>
    public string? DireccionEnvio { get; set; }

    /// <summary>
    /// Personalización de diseño de la tarjeta
    /// </summary>
    public PersonalizacionTarjeta? Personalizacion { get; set; }

    /// <summary>
    /// Datos adicionales para la tarjeta
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    /// <summary>
    /// Preferencias del cliente para la tarjeta
    /// </summary>
    public Dictionary<string, string>? PreferenciasCliente { get; set; }
}

/// <summary>
/// Tipos de tarjeta de fidelización disponibles
/// </summary>
public enum TipoTarjetaFidelizacion
{
    /// <summary>
    /// Tarjeta estándar básica
    /// </summary>
    Estandar = 1,

    /// <summary>
    /// Tarjeta premium con beneficios adicionales
    /// </summary>
    Premium = 2,

    /// <summary>
    /// Tarjeta VIP con máximos beneficios
    /// </summary>
    Vip = 3,

    /// <summary>
    /// Tarjeta corporativa para empresas
    /// </summary>
    Corporativa = 4,

    /// <summary>
    /// Tarjeta de empleado con descuentos especiales
    /// </summary>
    Empleado = 5,

    /// <summary>
    /// Tarjeta promocional temporal
    /// </summary>
    Promocional = 6
}

/// <summary>
/// Niveles de fidelización disponibles
/// </summary>
public enum NivelFidelizacion
{
    /// <summary>
    /// Nivel inicial - Bronce
    /// </summary>
    Bronce = 1,

    /// <summary>
    /// Nivel intermedio - Plata
    /// </summary>
    Plata = 2,

    /// <summary>
    /// Nivel avanzado - Oro
    /// </summary>
    Oro = 3,

    /// <summary>
    /// Nivel premium - Platino
    /// </summary>
    Platino = 4,

    /// <summary>
    /// Nivel exclusivo - Diamante
    /// </summary>
    Diamante = 5
}

/// <summary>
/// Configuración específica de la tarjeta
/// </summary>
public class ConfiguracionTarjeta
{
    /// <summary>
    /// Multiplicador de puntos por compra
    /// </summary>
    public decimal MultiplicadorPuntos { get; set; } = 1.0m;

    /// <summary>
    /// Descuento base en compras
    /// </summary>
    public decimal DescuentoBase { get; set; } = 0m;

    /// <summary>
    /// Límite máximo de puntos acumulables por día
    /// </summary>
    public int? LimitePuntosDiario { get; set; }

    /// <summary>
    /// Límite máximo de puntos acumulables por mes
    /// </summary>
    public int? LimitePuntosMensual { get; set; }

    /// <summary>
    /// Días hasta que expiren los puntos
    /// </summary>
    public int? DiasExpiracionPuntos { get; set; }

    /// <summary>
    /// Permite acumular puntos en promociones
    /// </summary>
    public bool AcumularEnPromociones { get; set; } = true;

    /// <summary>
    /// Permite canjear puntos por descuentos
    /// </summary>
    public bool PermiteCanjearDescuentos { get; set; } = true;

    /// <summary>
    /// Acceso a eventos exclusivos
    /// </summary>
    public bool AccesoEventosExclusivos { get; set; } = false;

    /// <summary>
    /// Notificaciones automáticas activadas
    /// </summary>
    public bool NotificacionesActivas { get; set; } = true;

    /// <summary>
    /// Configuraciones adicionales
    /// </summary>
    public Dictionary<string, object>? ConfiguracionesExtras { get; set; }
}

/// <summary>
/// Personalización del diseño de la tarjeta
/// </summary>
public class PersonalizacionTarjeta
{
    /// <summary>
    /// Color principal de la tarjeta
    /// </summary>
    public string? ColorPrincipal { get; set; }

    /// <summary>
    /// Color secundario de la tarjeta
    /// </summary>
    public string? ColorSecundario { get; set; }

    /// <summary>
    /// Diseño/template de la tarjeta
    /// </summary>
    public string? DisenyoTemplate { get; set; }

    /// <summary>
    /// Logo personalizado (URL o base64)
    /// </summary>
    public string? LogoPersonalizado { get; set; }

    /// <summary>
    /// Texto personalizado en la tarjeta
    /// </summary>
    public string? TextoPersonalizado { get; set; }

    /// <summary>
    /// Imagen de fondo
    /// </summary>
    public string? ImagenFondo { get; set; }

    /// <summary>
    /// Fuente del texto
    /// </summary>
    public string? TipoFuente { get; set; }

    /// <summary>
    /// Configuraciones adicionales de diseño
    /// </summary>
    public Dictionary<string, string>? ConfiguracionesDiseño { get; set; }
} 