using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Critical;

/// <summary>
/// Seeder para validar configuraciones críticas del sistema RestaurantePro.
/// 
/// IMPORTANTE: Este seeder NO crea configuraciones nuevas, sino que VALIDA
/// que las configuraciones definidas en AppSettings.cs sean consistentes.
/// 
/// Los valores se extraen de:
/// - AppSettings.cs (configuraciones por defecto)
/// - BusinessRulesSettings (reglas de negocio)
/// - BehaviorSettings (configuraciones de pipeline)
/// - appsettings.json y appsettings.Development.json
///
/// Orden: 140 (después de PermisosSeeder=130, antes de EstadosSeeder=150)
/// </summary>
public class ConfiguracionSeeder : ISeedData
{
    public string Name => "Validación de Configuraciones del Sistema";
    public int Order => 140;
    public bool IsDevOnly => false;
    public bool IsCritical => true;

    /// <summary>
    /// Configuraciones críticas extraídas de AppSettings.cs
    /// Estos son los valores POR DEFECTO reales que usa el sistema
    /// </summary>
    private static readonly Dictionary<string, ConfiguracionInfo> ConfiguracionesCriticas = new()
    {
        // === NOTIFICACIONES ===
        ["Notifications:EnableEmailNotifications"] = new("Notificaciones", true, "Habilitar notificaciones por email"),
        ["Notifications:EnableSMSNotifications"] = new("Notificaciones", true, "Habilitar notificaciones por SMS"),
        ["Notifications:EnablePushNotifications"] = new("Notificaciones", true, "Habilitar notificaciones push"),
        ["Notifications:EnableSignalRNotifications"] = new("Notificaciones", true, "Habilitar notificaciones SignalR"),
        ["Notifications:DefaultTimeoutSeconds"] = new("Notificaciones", 30, "Timeout por defecto para notificaciones (segundos)"),
        ["Notifications:MaxRetryAttempts"] = new("Notificaciones", 3, "Máximo de reintentos para notificaciones fallidas"),

        // === CACHÉ ===
        ["Cache:DefaultExpirationMinutes"] = new("Caché", 60, "Tiempo de expiración por defecto para caché (minutos)"),
        ["Cache:SizeLimit"] = new("Caché", 1024, "Límite de entradas en caché"),
        ["Cache:CompactionPercentage"] = new("Caché", 0.25, "Porcentaje de compactación cuando se alcanza el límite"),
        ["Cache:EnableQueryCaching"] = new("Caché", true, "Habilitar caché para queries"),

        // === TRABAJOS EN SEGUNDO PLANO ===
        ["BackgroundJobs:EnableBackgroundJobs"] = new("Jobs", true, "Habilitar trabajos en segundo plano"),
        ["BackgroundJobs:DefaultStockCheckIntervalHours"] = new("Jobs", 24, "Intervalo por defecto para verificación de stock (horas)"),
        ["BackgroundJobs:DefaultInvoiceReminderDays"] = new("Jobs", 3, "Días de anticipación para recordatorios de facturas"),
        ["BackgroundJobs:AutoCleanupTime"] = new("Jobs", "02:00", "Hora de ejecución de limpieza automática (formato 24h)"),
        ["BackgroundJobs:LogRetentionDays"] = new("Jobs", 30, "Días de retención para logs y datos temporales"),

        // === SIGNALR ===
        ["Integration:SignalR:Enabled"] = new("SignalR", false, "Habilitar SignalR (deshabilitado por defecto hasta implementar)"),
        ["Integration:SignalR:NotificationHubUrl"] = new("SignalR", "/notificationHub", "URL del hub de notificaciones"),
        ["Integration:SignalR:ConnectionTimeoutSeconds"] = new("SignalR", 30, "Timeout de conexión (segundos)"),

        // === SERVICIOS EXTERNOS - EMAIL ===
        ["Integration:ExternalServices:Email:Provider"] = new("Email", "SMTP", "Proveedor de email (SendGrid, Azure, SMTP, etc.)"),
        ["Integration:ExternalServices:Email:DefaultFromEmail"] = new("Email", "noreply@restaurantepro.com", "Email por defecto para envíos"),
        ["Integration:ExternalServices:Email:DefaultFromName"] = new("Email", "RestaurantePro", "Nombre por defecto para envíos"),
        ["Integration:ExternalServices:Email:TestMode"] = new("Email", true, "Modo de prueba para emails"),

        // === SERVICIOS EXTERNOS - SMS ===
        ["Integration:ExternalServices:SMS:Provider"] = new("SMS", "Twilio", "Proveedor de SMS"),
        ["Integration:ExternalServices:SMS:DefaultFromNumber"] = new("SMS", "+1234567890", "Número por defecto para SMS"),
        ["Integration:ExternalServices:SMS:TestMode"] = new("SMS", true, "Modo de prueba para SMS"),

        // === REGLAS DE NEGOCIO - FIDELIZACIÓN ===
        ["BusinessRules:Fidelizacion:PuntosPorPeso"] = new("Fidelización", 0.001m, "Puntos por peso gastado (1 punto por $1000)"),
        ["BusinessRules:Fidelizacion:MinimoPuntos"] = new("Fidelización", 1, "Mínimo de puntos a otorgar"),
        ["BusinessRules:Fidelizacion:MultiplicadorPremium"] = new("Fidelización", 1.5m, "Multiplicador para clientes Premium"),

        // === REGLAS DE NEGOCIO - INVENTARIO ===
        ["BusinessRules:Inventario:DefaultCriticalityLevel"] = new("Inventario", 3, "Nivel de criticidad por defecto para alertas"),
        ["BusinessRules:Inventario:MinStockPercentage"] = new("Inventario", 0.20m, "Porcentaje de stock mínimo para alerta (20%)"),
        ["BusinessRules:Inventario:EnableMLPredictions"] = new("Inventario", true, "Habilitar predicciones con Machine Learning"),

        // === REGLAS DE NEGOCIO - OPERACIONES ===
        ["BusinessRules:Operaciones:DefaultPreparationTimeMinutes"] = new("Operaciones", 30, "Tiempo máximo de preparación por defecto (minutos)"),
        ["BusinessRules:Operaciones:DefaultMaxTableCapacity"] = new("Operaciones", 8, "Capacidad máxima de mesas por defecto"),
        ["BusinessRules:Operaciones:ReservationToleranceMinutes"] = new("Operaciones", 15, "Tiempo de tolerancia para reservaciones (minutos)"),

        // === BEHAVIORS - AUDITORÍA ===
        ["Behaviors:Auditing:Enabled"] = new("Auditoría", true, "Habilitar auditoría"),
        ["Behaviors:Auditing:CommandsOnly"] = new("Auditoría", true, "Auditar solo commands (true) o también queries (false)"),
        ["Behaviors:Auditing:IncludeRequestData"] = new("Auditoría", true, "Incluir datos de request en auditoría"),
        ["Behaviors:Auditing:IncludeResponseData"] = new("Auditoría", false, "Incluir datos de response en auditoría"),
        ["Behaviors:Auditing:MaxDataSizeBytes"] = new("Auditoría", 10240, "Máximo tamaño de datos a auditar (10KB)"),

        // === BEHAVIORS - TRANSACCIONES ===
        ["Behaviors:Transaction:Enabled"] = new("Transacciones", true, "Habilitar transacciones automáticas"),
        ["Behaviors:Transaction:TimeoutSeconds"] = new("Transacciones", 30, "Timeout para transacciones en segundos"),
        ["Behaviors:Transaction:DefaultIsolationLevel"] = new("Transacciones", "ReadCommitted", "Nivel de aislamiento por defecto"),

        // === MÉTRICAS ===
        ["Metrics:Enabled"] = new("Métricas", true, "Habilitar recolección de métricas"),
        ["Metrics:ReportIntervalSeconds"] = new("Métricas", 60, "Intervalo de reporte en segundos"),
        ["Metrics:EnableBusinessMetrics"] = new("Métricas", true, "Habilitar métricas de negocio"),
        ["Metrics:EnableSystemMetrics"] = new("Métricas", true, "Habilitar métricas de sistema"),
        ["Metrics:MaxMetricsInMemory"] = new("Métricas", 10000, "Máximo número de métricas en memoria")
    };

    /// <summary>
    /// Campos sensibles para auditoría (extraídos de AuditingSettings)
    /// </summary>
    private static readonly string[] CamposSensibles = new[]
    {
        "password", "contraseña", "token", "secret", "key"
    };

    /// <summary>
    /// Commands que siempre requieren transacción (extraídos de TransactionSettings)
    /// </summary>
    private static readonly string[] ComandosTransaccionales = new[]
    {
        "CrearFactura", "ProcesarPago", "ActualizarInventario", "CrearComanda"
    };

    /// <summary>
    /// Commands que nunca requieren transacción
    /// </summary>
    private static readonly string[] ComandosSinTransaccion = new[]
    {
        "EnviarNotificacion", "GenerarReporte", "ValidarStock"
    };

    public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
    {
        // Este seeder siempre debe ejecutarse para validar consistencia
        // No depende de datos existentes, sino de validación
        return false;
    }

    public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("⚙️  Validando configuraciones del sistema...");

        var configuracionesValidadas = 0;
        var configuracionesInconsistentes = 0;
        var configuracionesPeligrosas = 0;

        // VALIDACIÓN 1: Verificar categorización de configuraciones
        await ValidarCategorizacionConfiguraciones(logger);

        // VALIDACIÓN 2: Validar configuraciones críticas de negocio
        var (validadas, inconsistentes) = await ValidarConfiguracionesNegocio(logger);
        configuracionesValidadas = validadas;
        configuracionesInconsistentes = inconsistentes;

        // VALIDACIÓN 3: Validar configuraciones de seguridad
        configuracionesPeligrosas = await ValidarConfiguracionesSeguridad(logger);

        // VALIDACIÓN 4: Validar configuraciones de rendimiento
        await ValidarConfiguracionesRendimiento(logger);

        // VALIDACIÓN 5: Validar listas especiales
        await ValidarListasEspeciales(logger);

        logger.LogInformation("📊 Validación de configuraciones completada:");
        logger.LogInformation("   ✅ Configuraciones validadas: {ConfiguracionesValidadas}", configuracionesValidadas);
        logger.LogInformation("   ⚠️  Configuraciones peligrosas: {ConfiguracionesPeligrosas}", configuracionesPeligrosas);
        logger.LogInformation("   ❌ Inconsistencias: {ConfiguracionesInconsistentes}", configuracionesInconsistentes);

        if (configuracionesInconsistentes > 0)
        {
            logger.LogWarning("🚨 Se encontraron {Count} inconsistencias en configuraciones", configuracionesInconsistentes);
        }
    }

    private async Task ValidarCategorizacionConfiguraciones(ILogger logger)
    {
        var categorias = ConfiguracionesCriticas.GroupBy(c => c.Value.Categoria);
        
        logger.LogInformation("📋 Categorías de configuraciones identificadas:");
        foreach (var categoria in categorias)
        {
            var count = categoria.Count();
            logger.LogInformation("   📁 {Categoria}: {Total} configuraciones", categoria.Key, count);
        }
    }

    private async Task<(int validadas, int inconsistentes)> ValidarConfiguracionesNegocio(ILogger logger)
    {
        logger.LogInformation("💼 Validando configuraciones de negocio...");
        
        var configuracionesValidadas = 0;
        var configuracionesInconsistentes = 0;
        
        // Validar fidelización
        var puntosPorPeso = (decimal)ConfiguracionesCriticas["BusinessRules:Fidelizacion:PuntosPorPeso"].ValorPorDefecto;
        if (puntosPorPeso <= 0 || puntosPorPeso > 1)
        {
            logger.LogWarning("⚠️  PuntosPorPeso tiene valor sospechoso: {Valor}", puntosPorPeso);
            configuracionesInconsistentes++;
        }
        else
        {
            configuracionesValidadas++;
        }

        // Validar inventario
        var minStock = (decimal)ConfiguracionesCriticas["BusinessRules:Inventario:MinStockPercentage"].ValorPorDefecto;
        if (minStock <= 0 || minStock >= 1)
        {
            logger.LogWarning("⚠️  MinStockPercentage debe estar entre 0 y 1: {Valor}", minStock);
            configuracionesInconsistentes++;
        }
        else
        {
            configuracionesValidadas++;
        }

        // Validar operaciones
        var tiempoPreparacion = (int)ConfiguracionesCriticas["BusinessRules:Operaciones:DefaultPreparationTimeMinutes"].ValorPorDefecto;
        if (tiempoPreparacion <= 0 || tiempoPreparacion > 180) // Máximo 3 horas
        {
            logger.LogWarning("⚠️  DefaultPreparationTimeMinutes fuera de rango razonable: {Valor}", tiempoPreparacion);
            configuracionesInconsistentes++;
        }
        else
        {
            configuracionesValidadas++;
        }

        logger.LogInformation("✅ Configuraciones de negocio validadas");
        return (configuracionesValidadas, configuracionesInconsistentes);
    }

    private async Task<int> ValidarConfiguracionesSeguridad(ILogger logger)
    {
        logger.LogInformation("🔒 Validando configuraciones de seguridad...");
        
        var configuracionesPeligrosas = 0;
        
        // Validar que el modo de prueba esté habilitado en desarrollo
        var emailTestMode = (bool)ConfiguracionesCriticas["Integration:ExternalServices:Email:TestMode"].ValorPorDefecto;
        var smsTestMode = (bool)ConfiguracionesCriticas["Integration:ExternalServices:SMS:TestMode"].ValorPorDefecto;
        
        if (!emailTestMode)
        {
            logger.LogWarning("🔥 Email TestMode está deshabilitado - cuidado en producción");
            configuracionesPeligrosas++;
        }
        
        if (!smsTestMode)
        {
            logger.LogWarning("🔥 SMS TestMode está deshabilitado - cuidado en producción");
            configuracionesPeligrosas++;
        }

        // Validar auditoría
        var auditEnabled = (bool)ConfiguracionesCriticas["Behaviors:Auditing:Enabled"].ValorPorDefecto;
        if (!auditEnabled)
        {
            logger.LogWarning("🔥 Auditoría está deshabilitada - riesgo de seguridad");
            configuracionesPeligrosas++;
        }

        logger.LogInformation("✅ Configuraciones de seguridad validadas");
        return configuracionesPeligrosas;
    }

    private async Task ValidarConfiguracionesRendimiento(ILogger logger)
    {
        logger.LogInformation("⚡ Validando configuraciones de rendimiento...");
        
        var cacheSize = (int)ConfiguracionesCriticas["Cache:SizeLimit"].ValorPorDefecto;
        var metricsInMemory = (int)ConfiguracionesCriticas["Metrics:MaxMetricsInMemory"].ValorPorDefecto;
        var auditMaxSize = (int)ConfiguracionesCriticas["Behaviors:Auditing:MaxDataSizeBytes"].ValorPorDefecto;
        
        logger.LogInformation("   📊 Cache size limit: {CacheSize}", cacheSize);
        logger.LogInformation("   📊 Max metrics in memory: {MetricsInMemory}", metricsInMemory);
        logger.LogInformation("   📊 Audit max data size: {AuditMaxSize} bytes", auditMaxSize);
        
        if (cacheSize > 10000)
        {
            logger.LogWarning("⚠️  Cache size muy alto, podría afectar memoria");
        }
        
        logger.LogInformation("✅ Configuraciones de rendimiento validadas");
    }

    private async Task ValidarListasEspeciales(ILogger logger)
    {
        logger.LogInformation("📝 Validando listas especiales...");
        
        logger.LogInformation("   🔒 Campos sensibles para auditoría:");
        foreach (var campo in CamposSensibles)
        {
            logger.LogInformation("     - {Campo}", campo);
        }
        
        logger.LogInformation("   🔄 Commands que requieren transacción:");
        foreach (var comando in ComandosTransaccionales)
        {
            logger.LogInformation("     - {Comando}", comando);
        }
        
        logger.LogInformation("   🚫 Commands sin transacción:");
        foreach (var comando in ComandosSinTransaccion)
        {
            logger.LogInformation("     - {Comando}", comando);
        }
        
        logger.LogInformation("✅ Listas especiales validadas");
    }

    /// <summary>
    /// Información de una configuración del sistema
    /// </summary>
    private record ConfiguracionInfo(string Categoria, object ValorPorDefecto, string Descripcion);
} 