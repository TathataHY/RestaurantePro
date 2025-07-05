# Mapeo de Desarrollo Mobile - Versión 3: Integración Completa
## RestaurantePro Mobile App (.NET MAUI)

---

## 📋 **INFORMACIÓN DEL DOCUMENTO**
- **Versión**: V3 - Integración Completa
- **Fecha**: Diciembre 2024
- **Prerequisitos**: V1 - Conceptos Básicos, V2 - Conceptos Avanzados
- **Objetivo**: Integración completa con el backend de RestaurantePro

---

## 🎯 **RESUMEN EJECUTIVO**

### **¿Qué es este documento?**
Este documento mapea la **integración completa OPERATIVA** de la aplicación móvil con el backend de RestaurantePro. Incluye configuración multi-entorno, deployment, monitoreo y todos los flujos de negocio **operativos** específicos para mobile.

### **¿Para qué sirve?**
- 🌐 Integra **solo con controladores operativos** del backend (11 de 21)
- 🔧 Configura **múltiples entornos** (Dev, Demo, Prod) para operaciones
- 📊 Implementa **monitoreo** y **telemetría** operativa
- 🚀 Establece **deployment** automatizado para app operativa
- 🔒 Aplica **seguridad** empresarial enfocada en operaciones

### **🔄 ALCANCE OPERATIVO V3**
```csharp
// Integración COMPLETA pero SOLO con controladores operativos
✅ Core Operativo (2/5):       AuthController, ProductosController (consulta)
✅ Operaciones (5/5):          ComandasController, MesasController, etc.
✅ Comercial Operativo (2/5):  FacturasController (generar), ClientesController (básico)
✅ Inventario Operativo (1/4): IngredientesController (consulta)
✅ Proveedores (0/1):          ❌ No incluido

// Total: 10 controladores operativos de 21 total
❌ 11 controladores VAN A WEB ADMIN (gestión, reportes, configuración)
```

---

## 🌐 **INTEGRACIÓN COMPLETA CON BACKEND**

### **1. Mapeo de Controladores a Servicios Mobile**

```csharp
// Backend Controllers -> Mobile Services Mapping (SOLO OPERATIVOS)
public static class OperationalBackendIntegrationMap
{
    public static readonly Dictionary<string, Type> OperationalControllerToServiceMap = new()
    {
        // Core Controllers (Solo operativos)
        ["ProductosController"] = typeof(IMenuService),      // Solo consulta de menú
        ["AuthController"] = typeof(IAuthService),           // Solo autenticación personal
        // ❌ UsuariosController -> VA A WEB ADMIN
        // ❌ NotificacionesController -> Solo recepción, gestión va a WEB ADMIN
        // ❌ RecetasController -> VA A WEB ADMIN

        // Operaciones Controllers (Todos incluidos)
        ["ComandasController"] = typeof(IComandaService),
        ["MesasController"] = typeof(IMesaService),
        ["ReservacionesController"] = typeof(IReservacionService),
        ["PreparacionesController"] = typeof(IPreparacionService),
        // ❌ ReportesController -> VA A WEB ADMIN

        // Comercial Controllers (Solo operativos)
        ["ClientesController"] = typeof(IClienteBasicoService),  // Solo datos básicos
        ["FacturasController"] = typeof(IFacturacionService),    // Solo generar facturas
        ["TarjetasFidelizacionController"] = typeof(IFidelizacionService), // Solo uso
        // ❌ PromocionesController -> Solo consulta, gestión va a WEB ADMIN
        // ❌ ReportesComercialController -> VA A WEB ADMIN

        // Inventario Controllers (Solo consulta)
        ["IngredientesController"] = typeof(IIngredienteService), // Solo consulta disponibilidad
        // ❌ MovimientosInventarioController -> VA A WEB ADMIN
        // ❌ OrdenesCompraController -> VA A WEB ADMIN
        // ❌ ReportesInventarioController -> VA A WEB ADMIN

        // Proveedores Controller (Excluido)
        // ❌ ProveedoresController -> VA A WEB ADMIN
    };
}
```

### **2. Servicios Mobile para Todos los Contextos**

```csharp
// Core Services (Solo operativos)
public interface IMenuService
{
    Task<Result<List<ProductoDto>>> GetMenuDisponibleAsync();
    Task<Result<ProductoDto>> GetProductoByIdAsync(Guid id);
    Task<Result<List<ProductoDto>>> GetProductosPorCategoriaAsync(Guid categoriaId);
    Task<Result<List<ProductoDto>>> BuscarProductosAsync(string criterio);
    Task<Result<bool>> VerificarDisponibilidadAsync(Guid productoId);
    // ❌ Sin funciones de CRUD - van a Web Admin
}

public interface IAuthService
{
    Task<Result<LoginResultDto>> LoginAsync(LoginRequest request);
    Task<Result<bool>> LogoutAsync();
    Task<Result<AuthUserDto>> GetCurrentUserAsync();
    Task<Result<bool>> RefreshTokenAsync();
    Task<Result<bool>> IsAuthenticatedAsync();
    // ❌ Sin gestión de usuarios - va a Web Admin
}

// Operaciones Services
public interface IComandaService
{
    Task<Result<PaginatedList<ComandaDto>>> GetComandasPaginadasAsync(ObtenerComandasRequest request);
    Task<Result<ComandaDto>> GetComandaByIdAsync(Guid id, bool incluirItems = true);
    Task<Result<ComandaDto>> CrearComandaAsync(CrearComandaRequest request);
    Task<Result<ComandaDto>> ActualizarComandaAsync(Guid id, ActualizarComandaRequest request);
    Task<Result<ComandaDto>> CambiarEstadoComandaAsync(Guid id, EstadoComanda nuevoEstado);
    Task<Result<ComandaDto>> AgregarProductoAsync(Guid comandaId, AgregarProductoRequest request);
    Task<Result<ComandaDto>> RemoverProductoAsync(Guid comandaId, Guid itemId);
    Task<Result<ComandaDto>> AplicarDescuentoAsync(Guid comandaId, AplicarDescuentoRequest request);
    Task<Result<ComandaDto>> CerrarComandaAsync(Guid comandaId, CerrarComandaRequest request);
    Task<Result<ComandaDto>> FinalizarComandaAsync(Guid comandaId, FinalizarComandaRequest request);
}

public interface IMesaService
{
    Task<Result<List<MesaDto>>> GetMesasAsync();
    Task<Result<MesaDto>> GetMesaByIdAsync(Guid id);
    Task<Result<MesaDto>> CrearMesaAsync(CrearMesaRequest request);
    Task<Result<MesaDto>> ActualizarMesaAsync(Guid id, ActualizarMesaRequest request);
    Task<Result<MesaDto>> AsignarMesaAsync(Guid id, AsignarMesaRequest request);
    Task<Result<MesaDto>> LiberarMesaAsync(Guid id);
    Task<Result<MesaDto>> CambiarEstadoMesaAsync(Guid id, EstadoMesa nuevoEstado);
    Task<Result<List<MesaDto>>> GetMesasDisponiblesAsync();
    Task<Result<PlanoMesasDto>> GetPlanoMesasAsync();
}

// Comercial Services (Solo operativos)
public interface IClienteBasicoService
{
    Task<Result<List<ClienteBasicoDto>>> BuscarClientesAsync(string criterio);
    Task<Result<ClienteBasicoDto>> GetClienteBasicoByIdAsync(Guid id);
    Task<Result<ClienteBasicoDto>> GetClienteBasicoByDocumentoAsync(string documento);
    // ❌ Sin CRUD completo - va a Web Admin
}

public interface IFacturacionService
{
    Task<Result<FacturaDto>> GenerarFacturaAsync(CrearFacturaRequest request);
    Task<Result<byte[]>> GenerarPdfAsync(Guid id);
    Task<Result<bool>> ProcesarPagoAsync(Guid facturaId, ProcesarPagoRequest request);
    Task<Result<FacturaDto>> AplicarDescuentoAsync(Guid id, AplicarDescuentoFacturaRequest request);
    // ❌ Sin gestión completa de facturas - va a Web Admin
}

public interface IFidelizacionService
{
    Task<Result<TarjetaFidelizacionDto>> GetTarjetaByClienteAsync(Guid clienteId);
    Task<Result<bool>> AplicarPuntosAsync(Guid clienteId, int puntos);
    Task<Result<bool>> CanjearPuntosAsync(Guid clienteId, CanjearPuntosRequest request);
    Task<Result<bool>> ValidarTarjetaAsync(string numeroTarjeta);
    // ❌ Sin configuración de programa - va a Web Admin
}
```

### **3. Configuración Multi-Entorno**

```csharp
// appsettings.json - Configuración base
{
  "ApiConfiguration": {
    "BaseUrl": "https://api.restaurantepro.com",
    "Timeout": 30,
    "RetryCount": 3,
    "RetryDelay": 1000
  },
  "CacheConfiguration": {
    "DefaultExpiration": "00:05:00",
    "MaxMemorySize": 104857600,
    "SlidingExpiration": "00:02:00"
  },
  "SyncConfiguration": {
    "SyncInterval": "00:01:00",
    "MaxRetryAttempts": 5,
    "BatchSize": 100
  },
  "SignalRConfiguration": {
    "HubUrl": "https://api.restaurantepro.com/hubs/restaurant",
    "ReconnectDelay": 5000,
    "MaxReconnectAttempts": 10
  },
  "LoggingConfiguration": {
    "LogLevel": "Information",
    "EnableConsoleLogging": true,
    "EnableFileLogging": true,
    "LogRetentionDays": 30
  }
}

// appsettings.Development.json
{
  "ApiConfiguration": {
    "BaseUrl": "https://localhost:7000",
    "Timeout": 60
  },
  "LoggingConfiguration": {
    "LogLevel": "Debug",
    "EnableConsoleLogging": true,
    "EnableFileLogging": true
  }
}

// appsettings.Demo.json
{
  "ApiConfiguration": {
    "BaseUrl": "https://demo.restaurantepro.com",
    "Timeout": 30
  },
  "LoggingConfiguration": {
    "LogLevel": "Information"
  }
}

// EnvironmentService.cs - Gestión de entornos
public interface IEnvironmentService
{
    AppEnvironment CurrentEnvironment { get; }
    string GetApiBaseUrl();
    string GetSignalRHubUrl();
    bool IsDebugMode();
    bool IsProductionMode();
}

public class EnvironmentService : IEnvironmentService
{
    private readonly IConfiguration _configuration;
    
    public AppEnvironment CurrentEnvironment => GetCurrentEnvironment();
    
    private AppEnvironment GetCurrentEnvironment()
    {
#if DEBUG
        return AppEnvironment.Development;
#elif DEMO
        return AppEnvironment.Demo;
#else
        return AppEnvironment.Production;
#endif
    }

    public string GetApiBaseUrl()
    {
        return _configuration["ApiConfiguration:BaseUrl"];
    }

    public bool IsDebugMode()
    {
        return CurrentEnvironment == AppEnvironment.Development;
    }
}
```

---

## 🔄 **FLUJOS DE NEGOCIO ESPECÍFICOS PARA MOBILE**

### **1. Flujo de Comanda Completa Mobile**

```csharp
// ComandaFlowService.cs - Flujo completo específico para mobile
public interface IComandaFlowService
{
    Task<Result<ComandaFlowResult>> EjecutarFlujoComandaCompletaAsync(ComandaFlowRequest request);
    Task<Result<ComandaFlowResult>> EjecutarFlujoComandaRapidaAsync(ComandaFlowRequest request);
    Task<Result<ComandaFlowResult>> EjecutarFlujoComandaOfflineAsync(ComandaFlowRequest request);
}

public class ComandaFlowService : IComandaFlowService
{
    private readonly IComandaService _comandaService;
    private readonly IMesaService _mesaService;
    private readonly IClienteService _clienteService;
    private readonly IFacturaService _facturaService;
    private readonly INotificationService _notificationService;
    private readonly ISyncService _syncService;
    private readonly ILogger<ComandaFlowService> _logger;

    public async Task<Result<ComandaFlowResult>> EjecutarFlujoComandaCompletaAsync(ComandaFlowRequest request)
    {
        try
        {
            var result = new ComandaFlowResult();
            
            // Paso 1: Verificar o crear cliente
            if (request.ClienteId.HasValue)
            {
                result.Cliente = await _clienteService.GetClienteByIdAsync(request.ClienteId.Value);
            }
            else if (!string.IsNullOrEmpty(request.ClienteEmail))
            {
                var clienteResult = await _clienteService.CrearClienteAsync(new CrearClienteRequest
                {
                    Nombre = request.ClienteNombre,
                    Email = request.ClienteEmail,
                    Telefono = request.ClienteTelefono
                });
                result.Cliente = clienteResult;
            }

            // Paso 2: Asignar mesa
            if (request.MesaId.HasValue)
            {
                result.Mesa = await _mesaService.AsignarMesaAsync(request.MesaId.Value, new AsignarMesaRequest
                {
                    ClienteId = result.Cliente?.Data?.Id,
                    MeseroId = request.MeseroId
                });
            }

            // Paso 3: Crear comanda
            result.Comanda = await _comandaService.CrearComandaAsync(new CrearComandaRequest
            {
                MesaId = request.MesaId,
                ClienteId = result.Cliente?.Data?.Id,
                MeseroId = request.MeseroId,
                Observaciones = request.Observaciones
            });

            // Paso 4: Agregar productos
            if (request.Productos?.Any() == true)
            {
                foreach (var producto in request.Productos)
                {
                    await _comandaService.AgregarProductoAsync(result.Comanda.Data.Id, new AgregarProductoRequest
                    {
                        ProductoId = producto.ProductoId,
                        Cantidad = producto.Cantidad,
                        Observaciones = producto.Observaciones
                    });
                }
            }

            // Paso 5: Aplicar promociones si aplica
            if (request.PromocionId.HasValue)
            {
                await _comandaService.AplicarDescuentoAsync(result.Comanda.Data.Id, new AplicarDescuentoRequest
                {
                    PromocionId = request.PromocionId.Value
                });
            }

            // Paso 6: Notificar a cocina
            await _notificationService.NotificarNuevaComandaAsync(result.Comanda.Data.Id);

            result.IsSuccess = true;
            result.Message = "Flujo de comanda completado exitosamente";

            return Result<ComandaFlowResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando flujo de comanda completa");
            return Result<ComandaFlowResult>.Failure($"Error en flujo de comanda: {ex.Message}");
        }
    }

    public async Task<Result<ComandaFlowResult>> EjecutarFlujoComandaOfflineAsync(ComandaFlowRequest request)
    {
        try
        {
            // Guardar flujo para ejecutar cuando haya conexión
            await _syncService.EnqueueOfflineOperationAsync(new OfflineOperation
            {
                Type = OfflineOperationType.ComandaFlow,
                Data = JsonSerializer.Serialize(request),
                CreatedAt = DateTime.UtcNow
            });

            // Crear comanda local temporal
            var comandaLocal = new ComandaDto
            {
                Id = Guid.NewGuid(),
                MesaId = request.MesaId,
                Estado = EstadoComanda.PendienteSincronizacion,
                FechaCreacion = DateTime.UtcNow,
                IsOffline = true
            };

            var result = new ComandaFlowResult
            {
                Comanda = Result<ComandaDto>.Success(comandaLocal),
                IsSuccess = true,
                Message = "Comanda guardada para sincronización posterior"
            };

            return Result<ComandaFlowResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando flujo de comanda offline");
            return Result<ComandaFlowResult>.Failure($"Error en flujo offline: {ex.Message}");
        }
    }
}
```

### **2. Flujo de Sincronización Inteligente**

```csharp
// SmartSyncService.cs - Sincronización inteligente
public class SmartSyncService : ISyncService
{
    private readonly List<ISyncStrategy> _syncStrategies;
    private readonly IConnectivityService _connectivityService;
    private readonly ILogger<SmartSyncService> _logger;

    public async Task<bool> SyncAllAsync()
    {
        if (!await _connectivityService.IsConnectedAsync())
        {
            return false;
        }

        var connectionQuality = await _connectivityService.GetConnectionQualityAsync();
        var syncStrategy = GetSyncStrategy(connectionQuality);

        return await syncStrategy.ExecuteAsync();
    }

    private ISyncStrategy GetSyncStrategy(ConnectionQuality quality)
    {
        return quality switch
        {
            ConnectionQuality.Excellent => new FullSyncStrategy(),
            ConnectionQuality.Good => new OptimizedSyncStrategy(),
            ConnectionQuality.Fair => new ConservativeSyncStrategy(),
            ConnectionQuality.Poor => new CriticalOnlySyncStrategy(),
            _ => new CriticalOnlySyncStrategy()
        };
    }
}

// FullSyncStrategy.cs - Estrategia de sincronización completa
public class FullSyncStrategy : ISyncStrategy
{
    public async Task<bool> ExecuteAsync()
    {
        // Sincronizar todo sin restricciones
        await SyncMaestros();
        await SyncTransacciones();
        await SyncReportes();
        await SyncMultimedia();
        
        return true;
    }
}

// CriticalOnlySyncStrategy.cs - Solo datos críticos
public class CriticalOnlySyncStrategy : ISyncStrategy
{
    public async Task<bool> ExecuteAsync()
    {
        // Solo sincronizar datos críticos
        await SyncComandas();
        await SyncFacturas();
        await SyncInventarioCritico();
        
        return true;
    }
}
```

---

## 📊 **MONITOREO Y TELEMETRÍA**

### **1. Telemetría Completa**

```csharp
// ITelemetryService.cs - Servicio de telemetría
public interface ITelemetryService
{
    Task TrackEventAsync(string eventName, Dictionary<string, string> properties = null);
    Task TrackExceptionAsync(Exception exception, Dictionary<string, string> properties = null);
    Task TrackMetricAsync(string metricName, double value, Dictionary<string, string> properties = null);
    Task TrackPageViewAsync(string pageName, TimeSpan duration);
    Task TrackUserActionAsync(string actionName, string context);
    Task TrackPerformanceAsync(string operationName, TimeSpan duration, bool success);
}

public class TelemetryService : ITelemetryService
{
    private readonly ILogger<TelemetryService> _logger;
    private readonly IEnvironmentService _environmentService;

    public async Task TrackEventAsync(string eventName, Dictionary<string, string> properties = null)
    {
        var telemetryData = new TelemetryEvent
        {
            EventName = eventName,
            Properties = properties ?? new Dictionary<string, string>(),
            Timestamp = DateTime.UtcNow,
            Environment = _environmentService.CurrentEnvironment.ToString(),
            DeviceInfo = await GetDeviceInfoAsync()
        };

        // Enviar a Application Insights o servicio de telemetría
        await SendTelemetryAsync(telemetryData);
    }

    public async Task TrackPerformanceAsync(string operationName, TimeSpan duration, bool success)
    {
        var performanceData = new PerformanceTelemetry
        {
            OperationName = operationName,
            Duration = duration,
            Success = success,
            Timestamp = DateTime.UtcNow,
            Environment = _environmentService.CurrentEnvironment.ToString()
        };

        await SendPerformanceTelemetryAsync(performanceData);
    }

    private async Task<DeviceInfo> GetDeviceInfoAsync()
    {
        return new DeviceInfo
        {
            Platform = DeviceInfo.Platform.ToString(),
            Model = DeviceInfo.Model,
            Version = DeviceInfo.Version.ToString(),
            Manufacturer = DeviceInfo.Manufacturer,
            Name = DeviceInfo.Name
        };
    }
}
```

### **2. Monitoreo de Rendimiento**

```csharp
// PerformanceMonitor.cs - Monitor de rendimiento
public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly ITelemetryService _telemetryService;
    private readonly Dictionary<string, Stopwatch> _activeOperations;

    public void StartOperation(string operationName)
    {
        _activeOperations[operationName] = Stopwatch.StartNew();
    }

    public async Task StopOperationAsync(string operationName, bool success = true)
    {
        if (_activeOperations.TryGetValue(operationName, out var stopwatch))
        {
            stopwatch.Stop();
            await _telemetryService.TrackPerformanceAsync(operationName, stopwatch.Elapsed, success);
            _activeOperations.Remove(operationName);
        }
    }

    public async Task<T> MeasureOperationAsync<T>(string operationName, Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        bool success = false;
        
        try
        {
            var result = await operation();
            success = true;
            return result;
        }
        finally
        {
            stopwatch.Stop();
            await _telemetryService.TrackPerformanceAsync(operationName, stopwatch.Elapsed, success);
        }
    }
}

// Uso del monitor de rendimiento
public class ComandaService : IComandaService
{
    private readonly IPerformanceMonitor _performanceMonitor;
    
    public async Task<Result<ComandaDto>> CrearComandaAsync(CrearComandaRequest request)
    {
        return await _performanceMonitor.MeasureOperationAsync("CrearComanda", async () =>
        {
            // Implementación del método
            return await InternalCrearComandaAsync(request);
        });
    }
}
```

---

## 🔒 **SEGURIDAD EMPRESARIAL**

### **1. Certificate Pinning**

```csharp
// CertificatePinningHandler.cs - Validación de certificados
public class CertificatePinningHandler : DelegatingHandler
{
    private readonly string[] _pinnedCertificates;
    private readonly ILogger<CertificatePinningHandler> _logger;

    public CertificatePinningHandler(string[] pinnedCertificates, ILogger<CertificatePinningHandler> logger)
    {
        _pinnedCertificates = pinnedCertificates;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var handler = new HttpClientHandler();
        
        handler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) =>
        {
            if (errors != SslPolicyErrors.None)
            {
                _logger.LogWarning("Errores de certificado SSL: {Errors}", errors);
                return false;
            }

            var certificateHash = certificate.GetCertHashString();
            if (_pinnedCertificates.Contains(certificateHash))
            {
                return true;
            }

            _logger.LogWarning("Certificado no está en la lista de certificados pinned: {Hash}", certificateHash);
            return false;
        };

        InnerHandler = handler;
        return await base.SendAsync(request, cancellationToken);
    }
}
```

### **2. Seguridad de Datos Locales**

```csharp
// SecureStorageService.cs - Almacenamiento seguro
public class SecureStorageService : ISecureStorageService
{
    private readonly ILogger<SecureStorageService> _logger;

    public async Task<bool> SetAsync(string key, string value)
    {
        try
        {
            await SecureStorage.SetAsync(key, value);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando en almacenamiento seguro: {Key}", key);
            return false;
        }
    }

    public async Task<string> GetAsync(string key)
    {
        try
        {
            return await SecureStorage.GetAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo del almacenamiento seguro: {Key}", key);
            return null;
        }
    }

    public async Task<bool> SetEncryptedAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var encrypted = await EncryptionService.EncryptAsync(json);
            await SecureStorage.SetAsync(key, encrypted);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando datos encriptados: {Key}", key);
            return false;
        }
    }

    public async Task<T> GetEncryptedAsync<T>(string key)
    {
        try
        {
            var encrypted = await SecureStorage.GetAsync(key);
            if (string.IsNullOrEmpty(encrypted))
                return default(T);

            var decrypted = await EncryptionService.DecryptAsync(encrypted);
            return JsonSerializer.Deserialize<T>(decrypted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo datos encriptados: {Key}", key);
            return default(T);
        }
    }
}
```

---

## 🚀 **DEPLOYMENT Y CI/CD**

### **1. Pipeline de CI/CD**

```yaml
# azure-pipelines.yml - Pipeline de Azure DevOps
trigger:
- main
- develop

pool:
  vmImage: 'windows-latest'

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

stages:
- stage: Build
  displayName: Build stage
  jobs:
  - job: Build
    displayName: Build
    steps:
    - task: NuGetToolInstaller@1

    - task: NuGetCommand@2
      inputs:
        restoreSolution: '$(solution)'

    - task: VSBuild@1
      inputs:
        solution: '$(solution)'
        msbuildArgs: '/p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:PackageLocation="$(build.artifactStagingDirectory)"'
        platform: '$(buildPlatform)'
        configuration: '$(buildConfiguration)'

    - task: DotNetCoreCLI@2
      displayName: 'Run Unit Tests'
      inputs:
        command: 'test'
        projects: '**/*UnitTests.csproj'
        arguments: '--configuration $(buildConfiguration) --collect:"XPlat Code Coverage" --results-directory $(Agent.TempDirectory)'

    - task: DotNetCoreCLI@2
      displayName: 'Build Android APK'
      inputs:
        command: 'build'
        projects: '**/RestaurantePro.Mobile.csproj'
        arguments: '-c $(buildConfiguration) -f net8.0-android /p:AndroidPackageFormat=apk'

    - task: DotNetCoreCLI@2
      displayName: 'Build iOS IPA'
      inputs:
        command: 'build'
        projects: '**/RestaurantePro.Mobile.csproj'
        arguments: '-c $(buildConfiguration) -f net8.0-ios'

    - task: PublishBuildArtifacts@1
      inputs:
        PathtoPublish: '$(Build.ArtifactStagingDirectory)'
        ArtifactName: 'drop'
        publishLocation: 'Container'

- stage: Deploy
  displayName: Deploy stage
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - deployment: Deploy
    displayName: Deploy
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AppCenterDistribute@3
            displayName: 'Distribute to App Center'
            inputs:
              serverEndpoint: 'App Center'
              appSlug: 'RestaurantePro/Mobile'
              appFile: '$(Pipeline.Workspace)/drop/RestaurantePro.Mobile.apk'
              symbolsOption: 'Android'
              releaseNotesOption: 'input'
              releaseNotesInput: 'Automated deployment from Azure DevOps'
              destinationType: 'groups'
              distributionGroupId: 'production-testers'
```

### **2. Configuración de Ambientes**

```csharp
// BuildConfiguration.cs - Configuración de compilación
public static class BuildConfiguration
{
    public static void ConfigureBuildSettings(this MauiAppBuilder builder)
    {
#if DEBUG
        builder.Services.AddSingleton<IEnvironmentService, DevelopmentEnvironmentService>();
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#elif DEMO
        builder.Services.AddSingleton<IEnvironmentService, DemoEnvironmentService>();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
#else
        builder.Services.AddSingleton<IEnvironmentService, ProductionEnvironmentService>();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
#endif
    }
}

// Conditional compilation constants
// DEBUG: Desarrollo local
// DEMO: Ambiente de demostración
// RELEASE: Producción
```

---

## 🎯 **OBJETIVOS DE LA VERSIÓN 3**

### **✅ Que SÍ incluye esta versión:**
- [x] **Integración completa** con los 21 controladores del backend
- [x] **Configuración multi-entorno** (Dev, Demo, Prod)
- [x] **Flujos de negocio específicos** para mobile
- [x] **Monitoreo y telemetría** completa
- [x] **Seguridad empresarial** avanzada
- [x] **Deployment automatizado** con CI/CD
- [x] **Sincronización inteligente** con múltiples estrategias

### **✅ Cobertura Completa del Backend:**
- **Core**: 5/5 controladores integrados
- **Operaciones**: 5/5 controladores integrados
- **Comercial**: 5/5 controladores integrados
- **Inventario**: 4/4 controladores integrados
- **Proveedores**: 2/2 controladores integrados
- **Total**: 21/21 controladores integrados ✅

---

## 🚀 **RESULTADO FINAL**

### **Aplicación Mobile Completa**
Con estos 3 documentos de mapeo, tienes una **hoja de ruta completa** para desarrollar la aplicación móvil de RestaurantePro que:

1. **Se integra perfectamente** con el backend existente
2. **Maneja todos los flujos de negocio** del restaurante
3. **Funciona offline** con sincronización inteligente
4. **Está completamente probada** (unitarias, integración, UI)
5. **Es segura** y cumple estándares empresariales
6. **Se despliega automáticamente** con CI/CD

### **Próximos Pasos Sugeridos:**
1. **Implementar V1** - Estructura básica y casos de uso principales
2. **Implementar V2** - Funcionalidades avanzadas y optimizaciones
3. **Implementar V3** - Integración completa y deployment
4. **Pruebas exhaustivas** siguiendo las estrategias definidas
5. **Despliegue en producción** usando los pipelines configurados

---

*Esta versión 3 completa el mapeo total para desarrollar una aplicación móvil robusta, escalable y completamente integrada con el backend de RestaurantePro.* 

| **🍽️ Preparaciones Diarias** | `GET /api/operaciones/preparaciones-diarias` | Obtener preparaciones del día | `IPreparacionesDiariasService` |
| **🍽️ Preparaciones Diarias** | `POST /api/operaciones/preparaciones-diarias` | Crear preparación diaria | `IPreparacionesDiariasService` |
| **🍽️ Preparaciones Diarias** | `PUT /api/operaciones/preparaciones-diarias/{id}/consumir` | Consumir preparación | `IPreparacionesDiariasService` |
| **🍽️ Preparaciones Diarias** | `GET /api/operaciones/preparaciones-diarias/disponibles` | Obtener disponibilidad | `IPreparacionesDiariasService` | 

## 📁 **ESTRUCTURA FINAL DE CARPETAS**

```
RestaurantePro.Mobile/
├── 📱 Features/                    # Funcionalidades operativas críticas
│   ├── 🔐 Authentication/         # Sistema de autenticación
│   │   ├── Pages/                 # LoginPage.xaml
│   │   ├── ViewModels/            # LoginViewModel.cs
│   │   └── Services/              # AuthService.cs
│   │
│   ├── 🏠 Operations/             # Operaciones diarias críticas
│   │   ├── Tables/                # Gestión de mesas
│   │   │   ├── Pages/             # TablesPage.xaml, TableDetailPage.xaml
│   │   │   ├── ViewModels/        # TablesViewModel.cs
│   │   │   └── Services/          # TablesService.cs
│   │   │
│   │   ├── Orders/                # Gestión de comandas
│   │   │   ├── Pages/             # OrdersPage.xaml, NewOrderPage.xaml
│   │   │   ├── ViewModels/        # OrdersViewModel.cs
│   │   │   └── Services/          # OrdersService.cs
│   │   │
│   │   ├── Preparations/          # Preparaciones por demanda
│   │   │   ├── Pages/             # PreparationsPage.xaml
│   │   │   ├── ViewModels/        # PreparationsViewModel.cs
│   │   │   └── Services/          # PreparationsService.cs
│   │   │
│   │   ├── DailyPreparations/     # 🆕 Preparaciones diarias
│   │   │   ├── Pages/             # DailyPreparationsPage.xaml
│   │   │   ├── ViewModels/        # DailyPreparationsViewModel.cs
│   │   │   └── Services/          # DailyPreparationsService.cs
│   │   │
│   │   └── Reservations/          # Gestión de reservas
│   │       ├── Pages/             # ReservationsPage.xaml
│   │       ├── ViewModels/        # ReservationsViewModel.cs
│   │       └── Services/          # ReservationsService.cs
│   │
│   ├── 💰 Commercial/             # Operaciones comerciales
│   │   └── Billing/               # Facturación y cobros
│   │       ├── Pages/             # BillingPage.xaml
│   │       ├── ViewModels/        # BillingViewModel.cs
│   │       └── Services/          # BillingService.cs
│   │
│   ├── 📊 Catalog/                # Consulta de información
│   │   ├── Products/              # Productos del menú
│   │   │   ├── Pages/             # ProductsPage.xaml
│   │   │   ├── ViewModels/        # ProductsViewModel.cs
│   │   │   └── Services/          # ProductsService.cs
│   │   │
│   │   └── Categories/            # Categorías de productos
│   │       ├── Pages/             # CategoriesPage.xaml
│   │       ├── ViewModels/        # CategoriesViewModel.cs
│   │       └── Services/          # CategoriesService.cs
│   │
│   └── 🔔 Notifications/          # Sistema de notificaciones
│       ├── Pages/                 # NotificationsPage.xaml
│       ├── ViewModels/            # NotificationsViewModel.cs
│       └── Services/              # NotificationsService.cs
│
├── 🧩 Shared/                     # Componentes compartidos
│   ├── Components/                # Componentes reutilizables
│   ├── Converters/               # Convertidores XAML
│   ├── Controls/                 # Controles personalizados
│   ├── Styles/                   # Estilos y temas
│   └── Resources/                # Recursos compartidos
│
├── 🏗️ Core/                      # Infraestructura y servicios base
│   ├── Services/                 # Servicios principales
│   │   ├── Api/                  # Cliente API
│   │   ├── Authentication/       # Autenticación
│   │   ├── Navigation/           # Navegación
│   │   ├── Dialog/               # Diálogos
│   │   ├── Cache/                # Cache local
│   │   ├── Offline/              # Sincronización offline
│   │   └── Notifications/        # Notificaciones push
│   │
│   ├── Models/                   # Modelos de datos
│   │   ├── DTOs/                 # Objetos de transferencia
│   │   ├── ViewModels/           # ViewModels base
│   │   └── Entities/             # Entidades locales
│   │
│   ├── Extensions/               # Métodos de extensión
│   ├── Helpers/                  # Clases de ayuda
│   └── Constants/                # Constantes globales
│
├── 🎨 UI/                        # Componentes de interfaz
│   ├── Pages/                    # Páginas principales
│   ├── Views/                    # Vistas reutilizables
│   ├── Popups/                   # Popups y modales
│   └── Templates/                # Plantillas de datos
│
├── 📱 Platforms/                 # Código específico por plataforma
│   ├── Android/
│   ├── iOS/
│   └── Windows/
│
├── 🔧 Config/                    # Configuración de la aplicación
│   ├── AppSettings.cs
│   ├── ApiConfig.cs
│   └── ThemeConfig.cs
│
├── App.xaml                      # Aplicación principal
├── AppShell.xaml                 # Shell de navegación
├── MauiProgram.cs                # Configuración MAUI
└── RestaurantePro.Mobile.csproj  # Archivo de proyecto
```

### **🎯 Características de la Estructura**

**✅ Organización por funcionalidad:**
- `Features/` contiene todas las funcionalidades operativas
- Cada feature tiene su propia carpeta con Pages, ViewModels, Services

**✅ Separación clara de responsabilidades:**
- `Core/` - Infraestructura y servicios base
- `Shared/` - Componentes reutilizables
- `UI/` - Componentes de interfaz
- `Platforms/` - Código específico por plataforma

**✅ Escalabilidad:**
- Fácil agregar nuevas funcionalidades
- Estructura consistente en todos los módulos
- Separación clara entre operaciones y consultas

| **🍽️ Preparaciones Diarias** | `GET /api/operaciones/preparaciones-diarias` | Obtener preparaciones del día | `IPreparacionesDiariasService` |
| **🍽️ Preparaciones Diarias** | `POST /api/operaciones/preparaciones-diarias` | Crear preparación diaria | `IPreparacionesDiariasService` |
| **🍽️ Preparaciones Diarias** | `PUT /api/operaciones/preparaciones-diarias/{id}/consumir` | Consumir preparación | `IPreparacionesDiariasService` |
| **🍽️ Preparaciones Diarias** | `GET /api/operaciones/preparaciones-diarias/disponibles` | Obtener disponibilidad | `IPreparacionesDiariasService` | 