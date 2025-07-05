A# Mapeo de Desarrollo Mobile - Versión 2: Conceptos Avanzados
## RestaurantePro Mobile App (.NET MAUI)

---

## 📋 **INFORMACIÓN DEL DOCUMENTO**
- **Versión**: V2 - Conceptos Avanzados
- **Fecha**: Diciembre 2024
- **Prerequisito**: V1 - Conceptos Básicos
- **Siguiente**: V3 - Integración Completa

---

## 🎯 **RESUMEN EJECUTIVO**

### **¿Qué es este documento?**
Este documento mapea los **conceptos avanzados** para la aplicación móvil **operativa** de RestaurantePro usando .NET MAUI. Incluye arquitectura compleja, patrones avanzados, sincronización offline y optimizaciones de rendimiento **enfocadas en operaciones críticas**.

### **¿Para qué sirve?**
- 🏗️ Implementa **arquitectura avanzada** con DI completo para operaciones
- 🔄 Establece **sincronización offline** robusta para flujos operativos
- 📈 Optimiza **rendimiento** y **memoria** para operaciones críticas
- 🌐 Integra **servicios externos** (SignalR, Push) para notificaciones operativas
- 🧪 Implementa **pruebas UI** automatizadas para flujos operativos

### **🔄 ENFOQUE OPERATIVO V2**
```csharp
// Funcionalidades AVANZADAS pero solo para operaciones críticas
✅ Sincronización offline de operaciones (Mesas, Comandas, Preparaciones)
✅ Notificaciones en tiempo real para cocina/meseros
✅ Optimizaciones de rendimiento para flujos críticos
✅ Gestión avanzada de cache operativo
✅ Patrones avanzados (Repository, Command, Observer)
✅ Integración SignalR para estados en tiempo real

❌ Sincronización de datos administrativos (Va en Web Admin)
❌ Reportes y analytics avanzados (Va en Web Admin)
❌ Gestión avanzada de usuarios (Va en Web Admin)
❌ Configuración avanzada del sistema (Va en Web Admin)
```

---

## 🏗️ **ARQUITECTURA AVANZADA**

### **1. Dependency Injection Avanzado**

```csharp
// Program.cs - Configuración completa de DI
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Registrar servicios por capas
        builder.Services.RegisterCoreServices();
        builder.Services.RegisterBusinessServices();
        builder.Services.RegisterInfrastructureServices();
        builder.Services.RegisterViewModels();
        builder.Services.RegisterViews();

        return builder.Build();
    }
}

// ServiceCollectionExtensions.cs - Registro organizado
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IApiService, ApiService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<IConnectivityService, ConnectivityService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        return services;
    }

    public static IServiceCollection RegisterOperationalServices(this IServiceCollection services)
    {
        // Servicios OPERATIVOS únicamente
        services.AddTransient<IMesaService, MesaService>();
        services.AddTransient<IComandaService, ComandaService>();
        services.AddTransient<IPreparacionService, PreparacionService>();
        services.AddTransient<IFacturacionService, FacturacionService>();
        services.AddTransient<IMenuService, MenuService>();
        services.AddTransient<IClienteBasicoService, ClienteBasicoService>();
        services.AddTransient<ISyncOperationalService, SyncOperationalService>();
        return services;
    }

    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<ILocalDatabase, LocalDatabase>();
        services.AddSingleton<ISignalRService, SignalRService>();
        services.AddSingleton<IPushNotificationService, PushNotificationService>();
        services.AddSingleton<ICacheService, CacheService>();
        return services;
    }
}
```

### **2. Patrón Repository Local**

```csharp
// ILocalRepository.cs - Patrón Repository para datos locales
public interface ILocalRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T> GetByIdAsync(Guid id);
    Task<List<T>> GetByFilterAsync(Expression<Func<T, bool>> filter);
    Task<bool> InsertAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpsertAsync(T entity);
    Task<bool> BulkInsertAsync(List<T> entities);
}

// LocalRepository.cs - Implementación base
public class LocalRepository<T> : ILocalRepository<T> where T : class, new()
{
    private readonly ILocalDatabase _database;
    private readonly AsyncTableQuery<T> _table;

    public LocalRepository(ILocalDatabase database)
    {
        _database = database;
        _table = _database.Connection.Table<T>();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _table.ToListAsync();
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        return await _table.Where(x => ((IEntity)x).Id == id).FirstOrDefaultAsync();
    }

    public async Task<bool> UpsertAsync(T entity)
    {
        try
        {
            var existing = await GetByIdAsync(((IEntity)entity).Id);
            if (existing != null)
            {
                await _database.Connection.UpdateAsync(entity);
            }
            else
            {
                await _database.Connection.InsertAsync(entity);
            }
            return true;
        }
        catch (Exception ex)
        {
            // Log error
            return false;
        }
    }
}
```

### **3. Sincronización Offline Completa**

```csharp
Ya // ISyncOperationalService.cs - Servicio de sincronización OPERATIVA
public interface ISyncOperationalServiceUh vaya ¿Qué es una mujer sis género 
{
    Task<bool> SyncOperationalDataAsync();
    Task<bool> SyncMesasAsync();
    Task<bool> SyncComandasAsync();
    Task<bool> SyncPreparacionesAsync();
    Task<bool> SyncMenuAsync();
    Task<bool> HasPendingOperationalChangesAsync();
    Task<List<SyncConflict>> GetOperationalConflictsAsync();
    Task<bool> ResolveOperationalConflictAsync(SyncConflict conflict);
    event EventHandler<SyncStatusEventArgs> SyncStatusChanged;
}

// SyncOperationalService.cs - Implementación de sincronización OPERATIVA
public class SyncOperationalService : ISyncOperationalService
{
    private readonly IApiService _apiService;
    private readonly ILocalRepository<Comanda> _comandaRepository;
    private readonly ILocalRepository<Mesa> _mesaRepository;
    private readonly ILocalRepository<Preparacion> _preparacionRepository;
    private readonly ILocalRepository<Producto> _productoRepository;
    private readonly IConnectivityService _connectivityService;
    private readonly ILogger<SyncOperationalService> _logger;

    public event EventHandler<SyncStatusEventArgs> SyncStatusChanged;

    public async Task<bool> SyncOperationalDataAsync()
    {
        if (!await _connectivityService.IsConnectedAsync())
        {
            _logger.LogWarning("Sin conexión a internet. Sincronización operativa aplazada.");
            return false;
        }

        try
        {
            OnSyncStatusChanged(SyncStatus.Started);

            // Sincronizar SOLO datos operativos en orden específico
            await SyncMesasAsync();           // Estados de mesas
            await SyncMenuAsync();            // Menú disponible (solo lectura)
            await SyncComandasAsync();        // Comandas locales al servidor
            await SyncPreparacionesAsync();   // Estados de preparación

            OnSyncStatusChanged(SyncStatus.Completed);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la sincronización");
            OnSyncStatusChanged(SyncStatus.Failed);
            return false;
        }
    }

    public async Task<bool> SyncEntityAsync<T>(SyncDirection direction) where T : class
    {
        try
        {
            switch (direction)
            {
                case SyncDirection.FromServer:
                    return await SyncFromServerAsync<T>();
                case SyncDirection.ToServer:
                    return await SyncToServerAsync<T>();
                case SyncDirection.Bidirectional:
                    return await SyncFromServerAsync<T>() && await SyncToServerAsync<T>();
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sincronizando {EntityType}", typeof(T).Name);
            return false;
        }
    }

    private async Task<bool> SyncFromServerAsync<T>() where T : class
    {
        var repository = GetRepository<T>();
        var lastSync = await GetLastSyncTimeAsync<T>();
        
        var serverData = await _apiService.GetAsync<List<T>>(
            $"api/sync/{typeof(T).Name.ToLower()}?since={lastSync:yyyy-MM-ddTHH:mm:ss}");

        if (serverData.Succeeded)
        {
            await repository.BulkInsertAsync(serverData.Data);
            await SetLastSyncTimeAsync<T>(DateTime.UtcNow);
            return true;
        }

        return false;
    }
}
```

---

## 🔄 **PATRONES AVANZADOS**

### **1. Command Pattern para Operaciones Complejas**

```csharp
// ICommand.cs - Patrón Command
public interface ICommand<T>
{
    Task<Result<T>> ExecuteAsync();
    Task<Result<T>> UndoAsync();
    bool CanExecute();
}

// CrearComandaCommand.cs - Comando complejo con validación
public class CrearComandaCommand : ICommand<Comanda>
{
    private readonly Comanda _comanda;
    private readonly IComandaService _comandaService;
    private readonly IValidationService _validationService;
    private readonly ILocalRepository<Comanda> _repository;

    public CrearComandaCommand(
        Comanda comanda,
        IComandaService comandaService,
        IValidationService validationService,
        ILocalRepository<Comanda> repository)
    {
        _comanda = comanda;
        _comandaService = comandaService;
        _validationService = validationService;
        _repository = repository;
    }

    public async Task<Result<Comanda>> ExecuteAsync()
    {
        // Validar comando
        var validationResult = await _validationService.ValidateAsync(_comanda);
        if (!validationResult.IsValid)
        {
            return Result<Comanda>.Failure(validationResult.ErrorMessage);
        }

        // Ejecutar en local primero
        var localResult = await _repository.InsertAsync(_comanda);
        if (!localResult)
        {
            return Result<Comanda>.Failure("Error al guardar localmente");
        }

        // Marcar para sincronización
        _comanda.PendingSync = true;
        await _repository.UpdateAsync(_comanda);

        // Intentar sincronizar inmediatamente si hay conexión
        if (await _connectivityService.IsConnectedAsync())
        {
            await _comandaService.SyncComandaAsync(_comanda);
        }

        return Result<Comanda>.Success(_comanda);
    }

    public bool CanExecute()
    {
        return _comanda != null && !string.IsNullOrEmpty(_comanda.MesaId.ToString());
    }
}
```

### **2. Observer Pattern para Notificaciones en Tiempo Real**

```csharp
// ISignalRService.cs - Servicio de tiempo real
public interface ISignalRService
{
    Task ConnectAsync();
    Task DisconnectAsync();
    Task JoinGroupAsync(string groupName);
    Task LeaveGroupAsync(string groupName);
    
    event EventHandler<ComandaUpdatedEventArgs> ComandaUpdated;
    event EventHandler<MesaStatusChangedEventArgs> MesaStatusChanged;
    event EventHandler<NotificationReceivedEventArgs> NotificationReceived;
}

// SignalRService.cs - Implementación de SignalR
public class SignalRService : ISignalRService
{
    private HubConnection _hubConnection;
    private readonly IAuthService _authService;
    private readonly ILogger<SignalRService> _logger;

    public event EventHandler<ComandaUpdatedEventArgs> ComandaUpdated;
    public event EventHandler<MesaStatusChangedEventArgs> MesaStatusChanged;
    public event EventHandler<NotificationReceivedEventArgs> NotificationReceived;

    public async Task ConnectAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{Constants.ApiBaseUrl}/hubs/restaurant", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token);
                })
                .WithAutomaticReconnect()
                .Build();

            // Configurar eventos
            _hubConnection.On<ComandaDto>("ComandaUpdated", OnComandaUpdated);
            _hubConnection.On<MesaDto>("MesaStatusChanged", OnMesaStatusChanged);
            _hubConnection.On<NotificationDto>("NotificationReceived", OnNotificationReceived);

            await _hubConnection.StartAsync();
            _logger.LogInformation("Conexión SignalR establecida");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error conectando a SignalR");
        }
    }

    private void OnComandaUpdated(ComandaDto comanda)
    {
        ComandaUpdated?.Invoke(this, new ComandaUpdatedEventArgs(comanda));
    }
}
```

### **🍽️ Preparaciones Diarias - Patrones Avanzados**

```csharp
// Servicio avanzado con cache y sincronización
public class PreparacionesDiariasService : IPreparacionesDiariasService
{
    private readonly IRepository<PreparacionDiaria> _repository;
    private readonly ICacheService _cache;
    private readonly IOfflineService _offline;
    private readonly INotificationService _notifications;

    // Obtener preparaciones con cache inteligente
    public async Task<List<PreparacionDiariaDto>> ObtenerPreparacionesDelDiaAsync()
    {
        var cacheKey = $"preparaciones_diarias_{DateTime.Now:yyyy-MM-dd}";
        
        // Intentar obtener del cache local
        var cached = await _cache.GetAsync<List<PreparacionDiariaDto>>(cacheKey);
        if (cached != null && !_offline.IsOnline)
            return cached;

        try
        {
            // Obtener del servidor
            var preparaciones = await _repository.GetPreparacionesDelDiaAsync();
            
            // Actualizar cache
            await _cache.SetAsync(cacheKey, preparaciones, TimeSpan.FromHours(2));
            
            // Notificar cambios via SignalR
            await _notifications.NotificarCambiosPreparacionesAsync(preparaciones);
            
            return preparaciones;
        }
        catch (Exception ex) when (_offline.IsOffline)
        {
            // Fallback a cache en modo offline
            return cached ?? new List<PreparacionDiariaDto>();
        }
    }

    // Crear preparación con patrón Command
    public async Task<Result> CrearPreparacionDiariaAsync(CrearPreparacionDiariaCommand command)
    {
        var preparacion = new PreparacionDiaria
        {
            ProductoId = command.ProductoId,
            CantidadPlanificada = command.Cantidad,
            FechaPreparacion = DateTime.Now,
            ChefId = command.ChefId,
            Observaciones = command.Observaciones
        };

        // Validar disponibilidad de ingredientes
        var ingredientesDisponibles = await ValidarIngredientesAsync(command.ProductoId, command.Cantidad);
        if (!ingredientesDisponibles.IsSuccess)
            return Result.Failure(ingredientesDisponibles.Error);

        // Persistir con patrón UoW
        await _repository.AddAsync(preparacion);
        
        // Invalidar cache
        await _cache.RemoveAsync($"preparaciones_diarias_{DateTime.Now:yyyy-MM-dd}");
        
        // Notificar a meseros
        await _notifications.NotificarNuevaPreparacionAsync(preparacion);
        
        return Result.Success();
    }

    // Consumir preparación con actualización tiempo real
    public async Task<Result> ConsumirPreparacionAsync(Guid preparacionId, int cantidad)
    {
        var preparacion = await _repository.GetByIdAsync(preparacionId);
        if (preparacion == null)
            return Result.Failure("Preparación no encontrada");

        if (preparacion.CantidadDisponible < cantidad)
            return Result.Failure("Cantidad insuficiente");

        // Actualizar cantidad disponible
        preparacion.CantidadDisponible -= cantidad;
        
        // Notificar cambio via SignalR
        await _notifications.NotificarCambioDisponibilidadAsync(preparacion);
        
        // Si se agota, notificar a cocina
        if (preparacion.CantidadDisponible == 0)
        {
            await _notifications.NotificarPreparacionAgotadaAsync(preparacion);
        }

        await _repository.UpdateAsync(preparacion);
        
        return Result.Success();
    }
}
```

### **📊 Sincronización Offline - Preparaciones Diarias**

```csharp
// Estrategia específica para preparaciones diarias
public class PreparacionesDiariasSyncStrategy : ISyncStrategy
{
    public async Task<SyncResult> SyncAsync()
    {
        var pendingCreations = await _localDb.GetPendingPreparacionesAsync();
        var pendingUpdates = await _localDb.GetPendingUpdatesAsync();
        
        // Priorizar creaciones de preparaciones matutinas
        foreach (var creation in pendingCreations.Where(p => p.EsMatutina))
        {
            await SyncCreationAsync(creation);
        }
        
        // Sincronizar consumos (crítico para disponibilidad)
        foreach (var update in pendingUpdates.Where(u => u.TipoUpdate == "consumo"))
        {
            await SyncConsumoAsync(update);
        }
        
        return SyncResult.Success();
    }
}
```

---

## 📈 **OPTIMIZACIONES DE RENDIMIENTO**

### **1. Lazy Loading y Virtualización**

```csharp
// LazyObservableCollection.cs - Colección con carga perezosa
public class LazyObservableCollection<T> : ObservableCollection<T>
{
    private readonly Func<int, int, Task<List<T>>> _loadMore;
    private readonly int _pageSize;
    private int _currentPage = 0;
    private bool _isLoading = false;
    private bool _hasMore = true;

    public LazyObservableCollection(Func<int, int, Task<List<T>>> loadMore, int pageSize = 20)
    {
        _loadMore = loadMore;
        _pageSize = pageSize;
    }

    public async Task LoadMoreAsync()
    {
        if (_isLoading || !_hasMore) return;

        _isLoading = true;
        try
        {
            var items = await _loadMore(_currentPage * _pageSize, _pageSize);
            
            if (items.Count < _pageSize)
            {
                _hasMore = false;
            }

            foreach (var item in items)
            {
                Add(item);
            }

            _currentPage++;
        }
        finally
        {
            _isLoading = false;
        }
    }
}

// ProductosViewModel.cs - Uso de lazy loading
public partial class ProductosViewModel : BaseViewModel
{
    [ObservableProperty]
    private LazyObservableCollection<Producto> productos;

    public ProductosViewModel(IProductoService productoService)
    {
        _productoService = productoService;
        
        Productos = new LazyObservableCollection<Producto>(
            async (skip, take) => await _productoService.GetProductosPaginadosAsync(skip, take));
    }

    [RelayCommand]
    private async Task LoadMoreProductosAsync()
    {
        await Productos.LoadMoreAsync();
    }
}
```

### **2. Caching Inteligente**

```csharp
// ICacheService.cs - Servicio de cache
public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task ClearAsync();
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}

// CacheService.cs - Implementación con múltiples niveles
public class CacheService : ICacheService
{
    private readonly MemoryCache _memoryCache;
    private readonly IStorageService _storageService;
    private readonly ILogger<CacheService> _logger;

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        // Nivel 1: Memoria
        if (_memoryCache.TryGetValue(key, out T cachedValue))
        {
            return cachedValue;
        }

        // Nivel 2: Almacenamiento local
        var storedValue = await _storageService.GetAsync<T>(key);
        if (storedValue != null)
        {
            _memoryCache.Set(key, storedValue, expiration ?? TimeSpan.FromMinutes(5));
            return storedValue;
        }

        // Nivel 3: Factory (API)
        var value = await factory();
        if (value != null)
        {
            _memoryCache.Set(key, value, expiration ?? TimeSpan.FromMinutes(5));
            await _storageService.SetAsync(key, value);
        }

        return value;
    }
}
```

---

## 🧪 **PRUEBAS UI AUTOMATIZADAS**

### **1. Configuración de Appium**

```csharp
// AppiumTestBase.cs - Base para pruebas UI
public abstract class AppiumTestBase
{
    protected AndroidDriver<AndroidElement> Driver;
    protected AppiumOptions Options;

    [SetUp]
    public void SetUp()
    {
        Options = new AppiumOptions();
        Options.AddAdditionalCapability("platformName", "Android");
        Options.AddAdditionalCapability("platformVersion", "11.0");
        Options.AddAdditionalCapability("deviceName", "Android Emulator");
        Options.AddAdditionalCapability("app", GetAppPath());
        Options.AddAdditionalCapability("automationName", "UiAutomator2");

        Driver = new AndroidDriver<AndroidElement>(new Uri("http://localhost:4723/wd/hub"), Options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
    }

    [TearDown]
    public void TearDown()
    {
        Driver?.Quit();
    }

    protected abstract string GetAppPath();
}

// LoginPageTests.cs - Pruebas de la página de login
[TestFixture]
public class LoginPageTests : AppiumTestBase
{
    [Test]
    public async Task LoginWithValidCredentials_ShouldNavigateToDashboard()
    {
        // Arrange
        var emailField = Driver.FindElement(By.Id("EmailEntry"));
        var passwordField = Driver.FindElement(By.Id("PasswordEntry"));
        var loginButton = Driver.FindElement(By.Id("LoginButton"));

        // Act
        emailField.SendKeys("test@example.com");
        passwordField.SendKeys("password123");
        loginButton.Click();

        // Assert
        var dashboardTitle = Driver.FindElement(By.Id("DashboardTitle"));
        Assert.That(dashboardTitle.Text, Is.EqualTo("Dashboard"));
    }

    [Test]
    public async Task LoginWithInvalidCredentials_ShouldShowErrorMessage()
    {
        // Arrange
        var emailField = Driver.FindElement(By.Id("EmailEntry"));
        var passwordField = Driver.FindElement(By.Id("PasswordEntry"));
        var loginButton = Driver.FindElement(By.Id("LoginButton"));

        // Act
        emailField.SendKeys("invalid@example.com");
        passwordField.SendKeys("wrongpassword");
        loginButton.Click();

        // Assert
        var errorMessage = Driver.FindElement(By.Id("ErrorMessage"));
        Assert.That(errorMessage.Text, Does.Contain("Credenciales inválidas"));
    }

    protected override string GetAppPath()
    {
        return Path.Combine(TestContext.CurrentContext.TestDirectory, "RestaurantePro.Mobile.apk");
    }
}
```

### **2. Pruebas de Flujos Completos**

```csharp
// ComandaFlowTests.cs - Pruebas del flujo completo de comandas
[TestFixture]
public class ComandaFlowTests : AppiumTestBase
{
    [Test]
    public async Task CreateComanda_CompleteFlow_ShouldSucceed()
    {
        // 1. Login
        await LoginAsync("mesero@test.com", "password123");

        // 2. Navegar a mesas
        Driver.FindElement(By.Id("MesasTab")).Click();

        // 3. Seleccionar mesa disponible
        var mesaDisponible = Driver.FindElement(By.XPath("//android.widget.Button[@resource-id='Mesa1' and @text='Disponible']"));
        mesaDisponible.Click();

        // 4. Crear nueva comanda
        Driver.FindElement(By.Id("CrearComandaButton")).Click();

        // 5. Agregar productos
        Driver.FindElement(By.Id("AgregarProductoButton")).Click();
        Driver.FindElement(By.XPath("//android.widget.TextView[@text='Hamburguesa Clásica']")).Click();
        Driver.FindElement(By.Id("AgregarAlCarritoButton")).Click();

        // 6. Confirmar comanda
        Driver.FindElement(By.Id("ConfirmarComandaButton")).Click();

        // 7. Verificar éxito
        var successMessage = Driver.FindElement(By.Id("SuccessMessage"));
        Assert.That(successMessage.Text, Does.Contain("Comanda creada exitosamente"));

        // 8. Verificar que la mesa cambió de estado
        Driver.FindElement(By.Id("MesasTab")).Click();
        var mesaOcupada = Driver.FindElement(By.XPath("//android.widget.Button[@resource-id='Mesa1' and @text='Ocupada']"));
        Assert.That(mesaOcupada, Is.Not.Null);
    }

    private async Task LoginAsync(string email, string password)
    {
        Driver.FindElement(By.Id("EmailEntry")).SendKeys(email);
        Driver.FindElement(By.Id("PasswordEntry")).SendKeys(password);
        Driver.FindElement(By.Id("LoginButton")).Click();
        
        // Esperar a que aparezca el dashboard
        WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElement(By.Id("DashboardTitle")));
    }
}
```

---

## 🎯 **OBJETIVOS DE LA VERSIÓN 2**

### **✅ Que SÍ incluye esta versión:**
- [x] **Dependency Injection** avanzado y organizado
- [x] **Patrón Repository** para datos locales
- [x] **Sincronización offline** completa y robusta
- [x] **Patrones avanzados** (Command, Observer)
- [x] **Optimizaciones de rendimiento** (Lazy loading, Cache)
- [x] **Pruebas UI automatizadas** con Appium
- [x] **Integración SignalR** para tiempo real

### **❌ Que NO incluye esta versión:**
- [ ] **Configuración multi-entorno** completa
- [ ] **Análisis de rendimiento** detallado
- [ ] **Seguridad avanzada** (Certificate pinning, etc.)
- [ ] **Deployment automatizado**
- [ ] **Monitoreo y telemetría**

---

## 🚀 **SIGUIENTE PASO**

### **Próximo Documento: V3 - Integración Completa**
- **Configuración multi-entorno**
- **Deployment y CI/CD**
- **Monitoreo y telemetría**
- **Seguridad empresarial**
- **Integración completa con backend**

---

*Este documento proporciona las herramientas avanzadas necesarias para desarrollar una aplicación móvil robusta y escalable que se integre perfectamente con el backend de RestaurantePro.*

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

*Este documento proporciona las herramientas avanzadas necesarias para desarrollar una aplicación móvil robusta y escalable que se integre perfectamente con el backend de RestaurantePro.* 