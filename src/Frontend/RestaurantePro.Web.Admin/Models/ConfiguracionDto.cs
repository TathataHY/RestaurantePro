namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para configuración del sistema
/// </summary>
public class ConfiguracionDto
{
    public Guid Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string TipoDato { get; set; } = string.Empty;
    public bool EsEditable { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

/// <summary>
/// DTO para configuración de notificaciones
/// </summary>
public class ConfiguracionNotificacionesDto
{
    public bool NotificacionesEmail { get; set; }
    public bool NotificacionesSistema { get; set; }
    public bool NotificacionesStockBajo { get; set; }
    public bool NotificacionesVencimiento { get; set; }
    public bool NotificacionesReservaciones { get; set; }
    public bool NotificacionesComandas { get; set; }
    public int DiasAntesVencimiento { get; set; } = 7;
    public int StockMinimoAlerta { get; set; } = 10;
}

/// <summary>
/// DTO para configuración de tarjetas de fidelización
/// </summary>
public class ConfiguracionFidelizacionDto
{
    public bool SistemaActivo { get; set; }
    public int PuntosPorPeso { get; set; } = 1;
    public decimal PesoPorPunto { get; set; } = 100;
    public int PuntosMinimosCanje { get; set; } = 1000;
    public decimal DescuentoMaximo { get; set; } = 50;
    public int DiasVencimientoPuntos { get; set; } = 365;
    public bool CanjeAutomatico { get; set; }
}

/// <summary>
/// DTO para parámetros del sistema
/// </summary>
public class ParametroSistemaDto
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string TipoDato { get; set; } = string.Empty;
}

/// <summary>
/// Request para actualizar configuración
/// </summary>
public class ActualizarConfiguracionRequest
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}
