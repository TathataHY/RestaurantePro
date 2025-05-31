namespace RestaurantePro.Application.Config.Settings;

/// <summary>
/// Configuración principal de la aplicación
/// </summary>
public class AppSettings
{
    public const string SectionName = "AppSettings";
    
    /// <summary>
    /// Configuraciones de notificaciones
    /// </summary>
    public NotificationSettings Notifications { get; set; } = new();
    
    /// <summary>
    /// Configuraciones de caché
    /// </summary>
    public CacheSettings Cache { get; set; } = new();
    
    /// <summary>
    /// Configuraciones de trabajos en segundo plano
    /// </summary>
    public BackgroundJobSettings BackgroundJobs { get; set; } = new();
    
    /// <summary>
    /// Configuraciones de integración
    /// </summary>
    public IntegrationSettings Integration { get; set; } = new();
    
    /// <summary>
    /// Configuraciones de business rules
    /// </summary>
    public BusinessRulesSettings BusinessRules { get; set; } = new();

    /// <summary>
    /// 🆕 Configuración de behaviors del pipeline MediatR
    /// </summary>
    public BehaviorSettings Behaviors { get; set; } = new();

    /// <summary>
    /// 🆕 Configuración de métricas y monitoreo
    /// </summary>
    public MetricsSettings Metrics { get; set; } = new();
}

/// <summary>
/// Configuración de notificaciones
/// </summary>
public class NotificationSettings
{
    /// <summary>
    /// Habilitar notificaciones por email
    /// </summary>
    public bool EnableEmailNotifications { get; set; } = true;
    
    /// <summary>
    /// Habilitar notificaciones por SMS
    /// </summary>
    public bool EnableSMSNotifications { get; set; } = true;
    
    /// <summary>
    /// Habilitar notificaciones push
    /// </summary>
    public bool EnablePushNotifications { get; set; } = true;
    
    /// <summary>
    /// Habilitar notificaciones SignalR
    /// </summary>
    public bool EnableSignalRNotifications { get; set; } = true;
    
    /// <summary>
    /// Timeout por defecto para notificaciones (en segundos)
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Máximo de reintentos para notificaciones fallidas
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}

/// <summary>
/// Configuración de caché
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// Tiempo de expiración por defecto para caché (en minutos)
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 60;
    
    /// <summary>
    /// Límite de entradas en caché
    /// </summary>
    public int SizeLimit { get; set; } = 1024;
    
    /// <summary>
    /// Porcentaje de compactación cuando se alcanza el límite
    /// </summary>
    public double CompactionPercentage { get; set; } = 0.25;
    
    /// <summary>
    /// Habilitar caché para queries
    /// </summary>
    public bool EnableQueryCaching { get; set; } = true;
}

/// <summary>
/// Configuración de trabajos en segundo plano
/// </summary>
public class BackgroundJobSettings
{
    /// <summary>
    /// Habilitar trabajos en segundo plano
    /// </summary>
    public bool EnableBackgroundJobs { get; set; } = true;
    
    /// <summary>
    /// Intervalo por defecto para verificación de stock (en horas)
    /// </summary>
    public int DefaultStockCheckIntervalHours { get; set; } = 24;
    
    /// <summary>
    /// Días de anticipación para recordatorios de facturas
    /// </summary>
    public int DefaultInvoiceReminderDays { get; set; } = 3;
    
    /// <summary>
    /// Hora de ejecución de limpieza automática (formato 24h)
    /// </summary>
    public string AutoCleanupTime { get; set; } = "02:00";
    
    /// <summary>
    /// Días de retención para logs y datos temporales
    /// </summary>
    public int LogRetentionDays { get; set; } = 30;
}

/// <summary>
/// Configuración de integraciones
/// </summary>
public class IntegrationSettings
{
    /// <summary>
    /// Configuración de SignalR
    /// </summary>
    public SignalRSettings SignalR { get; set; } = new();
    
    /// <summary>
    /// Configuración de servicios externos
    /// </summary>
    public ExternalServicesSettings ExternalServices { get; set; } = new();
}

/// <summary>
/// Configuración de SignalR
/// </summary>
public class SignalRSettings
{
    /// <summary>
    /// Habilitar SignalR
    /// </summary>
    public bool Enabled { get; set; } = false; // Deshabilitado por defecto hasta implementar
    
    /// <summary>
    /// URL del hub de notificaciones
    /// </summary>
    public string NotificationHubUrl { get; set; } = "/notificationHub";
    
    /// <summary>
    /// Timeout de conexión (en segundos)
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// Configuración de servicios externos
/// </summary>
public class ExternalServicesSettings
{
    /// <summary>
    /// Configuración de servicios de email
    /// </summary>
    public EmailServiceSettings Email { get; set; } = new();
    
    /// <summary>
    /// Configuración de servicios de SMS
    /// </summary>
    public SMSServiceSettings SMS { get; set; } = new();
}

/// <summary>
/// Configuración de servicios de email
/// </summary>
public class EmailServiceSettings
{
    /// <summary>
    /// Proveedor de email (SendGrid, Azure, SMTP, etc.)
    /// </summary>
    public string Provider { get; set; } = "SMTP";
    
    /// <summary>
    /// Email remitente por defecto
    /// </summary>
    public string DefaultFromEmail { get; set; } = "noreply@restaurantepro.com";
    
    /// <summary>
    /// Nombre del remitente por defecto
    /// </summary>
    public string DefaultFromName { get; set; } = "RestaurantePro";
    
    /// <summary>
    /// Habilitar modo de prueba (no envía emails reales)
    /// </summary>
    public bool TestMode { get; set; } = true;
}

/// <summary>
/// Configuración de servicios de SMS
/// </summary>
public class SMSServiceSettings
{
    /// <summary>
    /// Proveedor de SMS (Twilio, Azure, etc.)
    /// </summary>
    public string Provider { get; set; } = "Twilio";
    
    /// <summary>
    /// Número de teléfono remitente por defecto
    /// </summary>
    public string DefaultFromNumber { get; set; } = "+1234567890";
    
    /// <summary>
    /// Habilitar modo de prueba (no envía SMS reales)
    /// </summary>
    public bool TestMode { get; set; } = true;
}

/// <summary>
/// Configuración de reglas de negocio
/// </summary>
public class BusinessRulesSettings
{
    /// <summary>
    /// Configuración de fidelización
    /// </summary>
    public FidelizacionSettings Fidelizacion { get; set; } = new();
    
    /// <summary>
    /// Configuración de inventario
    /// </summary>
    public InventarioSettings Inventario { get; set; } = new();
    
    /// <summary>
    /// Configuración de operaciones
    /// </summary>
    public OperacionesSettings Operaciones { get; set; } = new();
}

/// <summary>
/// Configuración de fidelización
/// </summary>
public class FidelizacionSettings
{
    /// <summary>
    /// Puntos por peso gastado (ej: 1 punto por $1000)
    /// </summary>
    public decimal PuntosPorPeso { get; set; } = 0.001m; // 1 punto por $1000
    
    /// <summary>
    /// Mínimo de puntos a otorgar
    /// </summary>
    public int MinimoPuntos { get; set; } = 1;
    
    /// <summary>
    /// Multiplicador para clientes Premium
    /// </summary>
    public decimal MultiplicadorPremium { get; set; } = 1.5m;
}

/// <summary>
/// Configuración de inventario
/// </summary>
public class InventarioSettings
{
    /// <summary>
    /// Nivel de criticidad por defecto para alertas
    /// </summary>
    public int DefaultCriticalityLevel { get; set; } = 3;
    
    /// <summary>
    /// Porcentaje de stock mínimo para alerta
    /// </summary>
    public decimal MinStockPercentage { get; set; } = 0.20m; // 20%
    
    /// <summary>
    /// Habilitar predicciones con Machine Learning
    /// </summary>
    public bool EnableMLPredictions { get; set; } = true;
}

/// <summary>
/// Configuración de operaciones
/// </summary>
public class OperacionesSettings
{
    /// <summary>
    /// Tiempo máximo de preparación por defecto (en minutos)
    /// </summary>
    public int DefaultPreparationTimeMinutes { get; set; } = 30;
    
    /// <summary>
    /// Capacidad máxima de mesas por defecto
    /// </summary>
    public int DefaultMaxTableCapacity { get; set; } = 8;
    
    /// <summary>
    /// Tiempo de tolerancia para reservaciones (en minutos)
    /// </summary>
    public int ReservationToleranceMinutes { get; set; } = 15;
}

/// <summary>
/// 🆕 Configuración de behaviors del pipeline MediatR
/// </summary>
public class BehaviorSettings
{
    /// <summary>
    /// Configuración de rendimiento
    /// </summary>
    public PerformanceSettings Performance { get; set; } = new();
    
    /// <summary>
    /// Configuración de reintentos
    /// </summary>
    public RetrySettings Retry { get; set; } = new();
    
    /// <summary>
    /// Configuración de auditoría
    /// </summary>
    public AuditingSettings Auditing { get; set; } = new();
    
    /// <summary>
    /// Configuración de transacciones
    /// </summary>
    public TransactionSettings Transaction { get; set; } = new();
}

/// <summary>
/// 🆕 Configuración de métricas y monitoreo
/// </summary>
public class MetricsSettings
{
    /// <summary>
    /// Habilitar recolección de métricas
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Intervalo de reporte en segundos
    /// </summary>
    public int ReportIntervalSeconds { get; set; } = 60;
    
    /// <summary>
    /// Habilitar métricas de negocio
    /// </summary>
    public bool EnableBusinessMetrics { get; set; } = true;
    
    /// <summary>
    /// Habilitar métricas de sistema
    /// </summary>
    public bool EnableSystemMetrics { get; set; } = true;
    
    /// <summary>
    /// Máximo número de métricas en memoria
    /// </summary>
    public int MaxMetricsInMemory { get; set; } = 10000;
    
    /// <summary>
    /// Endpoint para envío de métricas (opcional)
    /// </summary>
    public string? MetricsEndpoint { get; set; }
}

/// <summary>
/// Configuración de auditoría
/// </summary>
public class AuditingSettings
{
    /// <summary>
    /// Habilitar auditoría
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Auditar solo commands (true) o también queries (false)
    /// </summary>
    public bool CommandsOnly { get; set; } = true;
    
    /// <summary>
    /// Incluir datos de request en auditoría
    /// </summary>
    public bool IncludeRequestData { get; set; } = true;
    
    /// <summary>
    /// Incluir datos de response en auditoría
    /// </summary>
    public bool IncludeResponseData { get; set; } = false;
    
    /// <summary>
    /// Máximo tamaño de datos a auditar (bytes)
    /// </summary>
    public int MaxDataSizeBytes { get; set; } = 10240; // 10KB
    
    /// <summary>
    /// Campos sensibles a excluir de auditoría
    /// </summary>
    public List<string> SensitiveFields { get; set; } = new()
    {
        "password", "contraseña", "token", "secret", "key"
    };
}

/// <summary>
/// Configuración de transacciones
/// </summary>
public class TransactionSettings
{
    /// <summary>
    /// Habilitar transacciones automáticas
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Timeout para transacciones en segundos
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Nivel de aislamiento por defecto
    /// </summary>
    public string DefaultIsolationLevel { get; set; } = "ReadCommitted";
    
    /// <summary>
    /// Operaciones que siempre requieren transacción
    /// </summary>
    public List<string> AlwaysRequireTransaction { get; set; } = new()
    {
        "CrearFactura", "ProcesarPago", "FinalizarComanda",
        "ActualizarStock", "CanjearPuntos"
    };
    
    /// <summary>
    /// Operaciones que nunca requieren transacción
    /// </summary>
    public List<string> NeverRequireTransaction { get; set; } = new()
    {
        "ObtenerProducto", "BuscarCliente", "ConsultarStock"
    };
} 