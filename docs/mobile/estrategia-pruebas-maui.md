# Estrategia de Pruebas para MAUI - RestaurantePro Mobile

---

## 📋 **INFORMACIÓN DEL DOCUMENTO**
- **Objetivo**: Estrategia completa de pruebas para la aplicación móvil **operativa**
- **Cobertura**: Unitarias, Integración, UI, Rendimiento, Seguridad
- **Frameworks**: **XUnit** (moderno), Appium, .NET MAUI UI Testing

---

## 🎯 **RESUMEN DE ESTRATEGIA**

### **Pirámide de Pruebas**
```
        🔺 UI Tests (10%)
       🔺🔺 Integration Tests (30%)
      🔺🔺🔺 Unit Tests (60%)
```

### **Cobertura Objetivo**
- **Unitarias**: 80% de cobertura de código
- **Integración**: 100% de servicios críticos
- **UI**: 100% de flujos principales
- **Rendimiento**: Todas las operaciones críticas
- **Seguridad**: Todos los puntos de entrada

---

## 🧪 **PRUEBAS UNITARIAS**

### **1. Estructura de Pruebas Unitarias**

```csharp
// Tests/RestaurantePro.Mobile.UnitTests/ViewModels/AuthViewModelTests.cs
public class AuthViewModelTests : IDisposable
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<ITelemetryService> _mockTelemetryService;
    private readonly AuthViewModel _viewModel;

    public AuthViewModelTests()
    {
        // XUnit - Constructor serves as Setup
        _mockAuthService = new Mock<IAuthService>();
        _mockNavigationService = new Mock<INavigationService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockTelemetryService = new Mock<ITelemetryService>();
        
        _viewModel = new AuthViewModel(
            _mockAuthService.Object,
            _mockNavigationService.Object,
            _mockDialogService.Object,
            _mockTelemetryService.Object);
    }

    [Fact]
    public async Task LoginCommand_WithValidCredentials_ShouldNavigateToDashboard()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(Result<AuthResponse>.Success(new AuthResponse { Token = "valid_token" }));

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(x => x.NavigateToAsync("//dashboard"), Times.Once);
        _mockTelemetryService.Verify(x => x.TrackEventAsync("Login", It.IsAny<Dictionary<string, string>>()), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoginCommand_WithInvalidCredentials_ShouldShowErrorMessage()
    {
        // Arrange
        _viewModel.Email = "invalid@example.com";
        _viewModel.Password = "wrongpassword";
        
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(Result<AuthResponse>.Failure("Credenciales inválidas"));

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync("Credenciales inválidas"), Times.Once);
        _mockNavigationService.Verify(x => x.NavigateToAsync(It.IsAny<string>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoginCommand_WithNetworkError_ShouldShowNetworkErrorMessage()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        await _viewModel.LoginCommand.ExecuteAsync(null);

        // Assert
        _mockDialogService.Verify(x => x.ShowErrorAsync(It.Is<string>(s => s.Contains("Network error"))), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public void LoginCommand_WithEmptyCredentials_ShouldNotExecute()
    {
        // Arrange
        _viewModel.Email = "";
        _viewModel.Password = "";

        // Act & Assert
        Assert.False(_viewModel.LoginCommand.CanExecute(null));
    }

    public void Dispose()
    {
        // XUnit - Dispose serves as TearDown
        _viewModel?.Dispose();
    }
}
```

### **2. Pruebas de Servicios**

```csharp
// Tests/RestaurantePro.Mobile.UnitTests/Services/ComandaServiceTests.cs
public class ComandaServiceTests : IDisposable
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<ILocalRepository<Comanda>> _mockLocalRepository;
    private readonly Mock<ISyncOperationalService> _mockSyncService;
    private readonly Mock<IConnectivityService> _mockConnectivityService;
    private readonly ComandaService _service;

    public ComandaServiceTests()
    {
        // XUnit - Constructor serves as Setup
        _mockApiService = new Mock<IApiService>();
        _mockLocalRepository = new Mock<ILocalRepository<Comanda>>();
        _mockSyncService = new Mock<ISyncOperationalService>();
        _mockConnectivityService = new Mock<IConnectivityService>();
        
        _service = new ComandaService(
            _mockApiService.Object,
            _mockLocalRepository.Object,
            _mockSyncService.Object,
            _mockConnectivityService.Object);
    }

    [Fact]
    public async Task CrearComandaAsync_WithConnection_ShouldCreateOnlineAndSync()
    {
        // Arrange
        var request = new CrearComandaRequest { MesaId = Guid.NewGuid(), ClienteId = Guid.NewGuid() };
        var expectedComanda = new ComandaDto { Id = Guid.NewGuid(), MesaId = request.MesaId };
        
        _mockConnectivityService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(true);
        _mockApiService.Setup(x => x.PostAsync<ComandaDto>(It.IsAny<string>(), It.IsAny<object>()))
                      .ReturnsAsync(Result<ComandaDto>.Success(expectedComanda));

        // Act
        var result = await _service.CrearComandaAsync(request);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(expectedComanda.Id, result.Data.Id);
        _mockApiService.Verify(x => x.PostAsync<ComandaDto>("/api/operaciones/comandas", request), Times.Once);
        _mockLocalRepository.Verify(x => x.UpsertAsync(It.IsAny<Comanda>()), Times.Once);
    }

    [Fact]
    public async Task CrearComandaAsync_WithoutConnection_ShouldCreateOfflineAndMarkForSync()
    {
        // Arrange
        var request = new CrearComandaRequest { MesaId = Guid.NewGuid(), ClienteId = Guid.NewGuid() };
        
        _mockConnectivityService.Setup(x => x.IsConnectedAsync()).ReturnsAsync(false);
        _mockLocalRepository.Setup(x => x.InsertAsync(It.IsAny<Comanda>())).ReturnsAsync(true);

        // Act
        var result = await _service.CrearComandaAsync(request);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data.IsOffline);
        _mockLocalRepository.Verify(x => x.InsertAsync(It.IsAny<Comanda>()), Times.Once);
        _mockSyncService.Verify(x => x.EnqueueOfflineOperationAsync(It.IsAny<OfflineOperation>()), Times.Once);
    }

    public void Dispose()
    {
        // XUnit - Dispose serves as TearDown
        _service?.Dispose();
    }
}
```

---

## 🔗 **PRUEBAS DE INTEGRACIÓN**

### **1. Pruebas de Integración con API**

```csharp
// Tests/RestaurantePro.Mobile.IntegrationTests/ApiIntegrationTests.cs
[TestFixture]
public class ApiIntegrationTests
{
    private TestServer _testServer;
    private HttpClient _httpClient;
    private ApiService _apiService;
    private IAuthService _authService;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Configurar servidor de pruebas
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllers();
        // ... configuración adicional
        
        var app = builder.Build();
        _testServer = new TestServer(app);
        _httpClient = _testServer.CreateClient();
        
        // Configurar servicios
        _authService = new AuthService(_httpClient);
        _apiService = new ApiService(_httpClient, _authService);
        
        // Autenticar
        await _authService.LoginAsync("test@example.com", "password123");
    }

    [Test]
    public async Task GetComandas_WithValidAuth_ShouldReturnCommandas()
    {
        // Act
        var result = await _apiService.GetAsync<List<ComandaDto>>("/api/operaciones/comandas");

        // Assert
        Assert.IsTrue(result.Succeeded);
        Assert.IsNotNull(result.Data);
        Assert.IsInstanceOf<List<ComandaDto>>(result.Data);
    }

    [Test]
    public async Task CreateComanda_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var request = new CrearComandaRequest
        {
            MesaId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            Observaciones = "Prueba integración"
        };

        // Act
        var result = await _apiService.PostAsync<ComandaDto>("/api/operaciones/comandas", request);

        // Assert
        Assert.IsTrue(result.Succeeded);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(request.MesaId, result.Data.MesaId);
    }
}
```

### **2. Pruebas de Flujos Completos**

```csharp
// Tests/RestaurantePro.Mobile.IntegrationTests/FlowIntegrationTests.cs
[TestFixture]
public class FlowIntegrationTests
{
    private IServiceProvider _serviceProvider;
    private IComandaFlowService _comandaFlowService;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IComandaFlowService, ComandaFlowService>();
        // ... registrar todos los servicios necesarios
        
        _serviceProvider = services.BuildServiceProvider();
        _comandaFlowService = _serviceProvider.GetRequiredService<IComandaFlowService>();
    }

    [Test]
    public async Task ComandaCompleteFlow_WithValidData_ShouldCompleteSuccessfully()
    {
        // Arrange
        var request = new ComandaFlowRequest
        {
            ClienteEmail = "cliente@test.com",
            ClienteNombre = "Cliente Test",
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            Productos = new List<ProductoComandaRequest>
            {
                new() { ProductoId = Guid.NewGuid(), Cantidad = 2 },
                new() { ProductoId = Guid.NewGuid(), Cantidad = 1 }
            }
        };

        // Act
        var result = await _comandaFlowService.EjecutarFlujoComandaCompletaAsync(request);

        // Assert
        Assert.IsTrue(result.Succeeded);
        Assert.IsNotNull(result.Data.Cliente);
        Assert.IsNotNull(result.Data.Mesa);
        Assert.IsNotNull(result.Data.Comanda);
        Assert.IsTrue(result.Data.IsSuccess);
    }
}
```

---

## 📱 **PRUEBAS DE UI**

### **1. Configuración de Appium**

```csharp
// Tests/RestaurantePro.Mobile.UITests/AppiumTestBase.cs
public abstract class AppiumTestBase
{
    protected AndroidDriver<AndroidElement> AndroidDriver;
    protected IOSDriver<IOSElement> IOSDriver;
    protected AppiumDriver<AppiumWebElement> Driver;
    protected AppiumOptions Options;

    [SetUp]
    public virtual void SetUp()
    {
        Options = new AppiumOptions();
        
        if (TestContext.Parameters["Platform"] == "Android")
        {
            SetupAndroid();
        }
        else if (TestContext.Parameters["Platform"] == "iOS")
        {
            SetupIOS();
        }
    }

    private void SetupAndroid()
    {
        Options.AddAdditionalCapability("platformName", "Android");
        Options.AddAdditionalCapability("platformVersion", "11.0");
        Options.AddAdditionalCapability("deviceName", "Android Emulator");
        Options.AddAdditionalCapability("app", GetAndroidAppPath());
        Options.AddAdditionalCapability("automationName", "UiAutomator2");
        Options.AddAdditionalCapability("newCommandTimeout", 300);
        Options.AddAdditionalCapability("appWaitActivity", "crc64*");

        AndroidDriver = new AndroidDriver<AndroidElement>(new Uri("http://localhost:4723/wd/hub"), Options);
        Driver = AndroidDriver;
    }

    private void SetupIOS()
    {
        Options.AddAdditionalCapability("platformName", "iOS");
        Options.AddAdditionalCapability("platformVersion", "15.0");
        Options.AddAdditionalCapability("deviceName", "iPhone 13");
        Options.AddAdditionalCapability("app", GetIOSAppPath());
        Options.AddAdditionalCapability("automationName", "XCUITest");

        IOSDriver = new IOSDriver<IOSElement>(new Uri("http://localhost:4723/wd/hub"), Options);
        Driver = IOSDriver;
    }

    [TearDown]
    public virtual void TearDown()
    {
        Driver?.Quit();
    }

    protected void WaitForElement(By locator, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(d => d.FindElement(locator));
    }

    protected void WaitForElementToBeClickable(By locator, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(ExpectedConditions.ElementToBeClickable(locator));
    }

    protected abstract string GetAndroidAppPath();
    protected abstract string GetIOSAppPath();
}
```

### **2. Pruebas de Páginas UI**

```csharp
// Tests/RestaurantePro.Mobile.UITests/Pages/LoginPageTests.cs
[TestFixture]
public class LoginPageTests : AppiumTestBase
{
    [Test]
    public async Task Login_WithValidCredentials_ShouldNavigateToDashboard()
    {
        // Arrange
        WaitForElement(By.Id("LoginPage"));
        
        var emailField = Driver.FindElement(By.Id("EmailEntry"));
        var passwordField = Driver.FindElement(By.Id("PasswordEntry"));
        var loginButton = Driver.FindElement(By.Id("LoginButton"));

        // Act
        emailField.Clear();
        emailField.SendKeys("mesero@test.com");
        passwordField.Clear();
        passwordField.SendKeys("password123");
        loginButton.Click();

        // Assert
        WaitForElement(By.Id("DashboardPage"), 15);
        var dashboardTitle = Driver.FindElement(By.Id("DashboardTitle"));
        Assert.That(dashboardTitle.Text, Is.EqualTo("Dashboard"));
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ShouldShowErrorMessage()
    {
        // Arrange
        WaitForElement(By.Id("LoginPage"));
        
        var emailField = Driver.FindElement(By.Id("EmailEntry"));
        var passwordField = Driver.FindElement(By.Id("PasswordEntry"));
        var loginButton = Driver.FindElement(By.Id("LoginButton"));

        // Act
        emailField.Clear();
        emailField.SendKeys("invalid@example.com");
        passwordField.Clear();
        passwordField.SendKeys("wrongpassword");
        loginButton.Click();

        // Assert
        WaitForElement(By.Id("ErrorMessage"), 10);
        var errorMessage = Driver.FindElement(By.Id("ErrorMessage"));
        Assert.That(errorMessage.Text, Does.Contain("Credenciales inválidas"));
    }

    [Test]
    public async Task Login_WithEmptyFields_ShouldDisableLoginButton()
    {
        // Arrange
        WaitForElement(By.Id("LoginPage"));
        
        var emailField = Driver.FindElement(By.Id("EmailEntry"));
        var passwordField = Driver.FindElement(By.Id("PasswordEntry"));
        var loginButton = Driver.FindElement(By.Id("LoginButton"));

        // Act
        emailField.Clear();
        passwordField.Clear();

        // Assert
        Assert.IsFalse(loginButton.Enabled);
    }

    protected override string GetAndroidAppPath()
    {
        return Path.Combine(TestContext.CurrentContext.TestDirectory, "RestaurantePro.Mobile.apk");
    }

    protected override string GetIOSAppPath()
    {
        return Path.Combine(TestContext.CurrentContext.TestDirectory, "RestaurantePro.Mobile.app");
    }
}
```

### **3. Pruebas de Flujos UI Completos**

```csharp
// Tests/RestaurantePro.Mobile.UITests/Flows/ComandaFlowUITests.cs
[TestFixture]
public class ComandaFlowUITests : AppiumTestBase
{
    [Test]
    public async Task CreateComanda_CompleteFlow_ShouldSucceed()
    {
        // Step 1: Login
        await LoginAsync("mesero@test.com", "password123");

        // Step 2: Navigate to Mesas
        Driver.FindElement(By.Id("MesasTab")).Click();
        WaitForElement(By.Id("MesasPage"));

        // Step 3: Select available mesa
        var mesasGrid = Driver.FindElement(By.Id("MesasGrid"));
        var mesaDisponible = mesasGrid.FindElement(By.XPath("//android.widget.Button[contains(@text, 'Disponible')]"));
        mesaDisponible.Click();

        // Step 4: Create nueva comanda
        WaitForElementToBeClickable(By.Id("CrearComandaButton"));
        Driver.FindElement(By.Id("CrearComandaButton")).Click();
        WaitForElement(By.Id("ComandaDetallePage"));

        // Step 5: Add productos
        Driver.FindElement(By.Id("AgregarProductoButton")).Click();
        WaitForElement(By.Id("ProductosPage"));
        
        var productosGrid = Driver.FindElement(By.Id("ProductosGrid"));
        var primerProducto = productosGrid.FindElement(By.XPath("//android.widget.Button[1]"));
        primerProducto.Click();
        
        WaitForElementToBeClickable(By.Id("AgregarAlCarritoButton"));
        Driver.FindElement(By.Id("AgregarAlCarritoButton")).Click();

        // Step 6: Confirm comanda
        WaitForElementToBeClickable(By.Id("ConfirmarComandaButton"));
        Driver.FindElement(By.Id("ConfirmarComandaButton")).Click();

        // Step 7: Verify success
        WaitForElement(By.Id("SuccessMessage"), 15);
        var successMessage = Driver.FindElement(By.Id("SuccessMessage"));
        Assert.That(successMessage.Text, Does.Contain("Comanda creada exitosamente"));

        // Step 8: Verify mesa status changed
        Driver.FindElement(By.Id("MesasTab")).Click();
        WaitForElement(By.Id("MesasPage"));
        
        var mesasGridUpdated = Driver.FindElement(By.Id("MesasGrid"));
        var mesaOcupada = mesasGridUpdated.FindElement(By.XPath("//android.widget.Button[contains(@text, 'Ocupada')]"));
        Assert.That(mesaOcupada, Is.Not.Null);
    }

    private async Task LoginAsync(string email, string password)
    {
        WaitForElement(By.Id("LoginPage"));
        
        Driver.FindElement(By.Id("EmailEntry")).SendKeys(email);
        Driver.FindElement(By.Id("PasswordEntry")).SendKeys(password);
        Driver.FindElement(By.Id("LoginButton")).Click();
        
        WaitForElement(By.Id("DashboardPage"), 15);
    }

    [Test]
    public async Task UpdateComanda_AddRemoveProducts_ShouldUpdateCorrectly()
    {
        // Complete flow to update an existing comanda
        await LoginAsync("mesero@test.com", "password123");
        
        // Navigate to comandas
        Driver.FindElement(By.Id("ComandasTab")).Click();
        WaitForElement(By.Id("ComandasPage"));
        
        // Select first comanda
        var comandasList = Driver.FindElement(By.Id("ComandasList"));
        var primeraComanda = comandasList.FindElement(By.XPath("//android.widget.Button[1]"));
        primeraComanda.Click();
        
        WaitForElement(By.Id("ComandaDetallePage"));
        
        // Add another product
        Driver.FindElement(By.Id("AgregarProductoButton")).Click();
        // ... implementation continues
    }
}
```

---

## ⚡ **PRUEBAS DE RENDIMIENTO**

### **1. Pruebas de Carga y Rendimiento**

```csharp
// Tests/RestaurantePro.Mobile.PerformanceTests/LoadTests.cs
[TestFixture]
public class LoadTests
{
    private IComandaService _comandaService;
    private IPerformanceMonitor _performanceMonitor;

    [SetUp]
    public void Setup()
    {
        // Setup services
        _comandaService = ServiceProvider.GetRequiredService<IComandaService>();
        _performanceMonitor = ServiceProvider.GetRequiredService<IPerformanceMonitor>();
    }

    [Test]
    public async Task LoadComandas_With100Items_ShouldCompleteInUnder2Seconds()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var expectedMaxTime = TimeSpan.FromSeconds(2);

        // Act
        var result = await _comandaService.GetComandasPaginadasAsync(new ObtenerComandasRequest
        {
            PageNumber = 1,
            PageSize = 100
        });

        // Assert
        stopwatch.Stop();
        Assert.IsTrue(result.Succeeded);
        Assert.LessOrEqual(stopwatch.Elapsed, expectedMaxTime, 
            $"LoadComandas took {stopwatch.Elapsed.TotalSeconds:F2} seconds, expected under {expectedMaxTime.TotalSeconds} seconds");
    }

    [Test]
    public async Task CreateMultipleComandas_Concurrent_ShouldHandleLoad()
    {
        // Arrange
        var tasks = new List<Task<Result<ComandaDto>>>();
        var numberOfConcurrentRequests = 10;

        // Act
        for (int i = 0; i < numberOfConcurrentRequests; i++)
        {
            var request = new CrearComandaRequest
            {
                MesaId = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                Observaciones = $"Prueba concurrente {i}"
            };
            
            tasks.Add(_comandaService.CrearComandaAsync(request));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(numberOfConcurrentRequests, results.Length);
        Assert.IsTrue(results.All(r => r.Succeeded), "All concurrent requests should succeed");
    }

    [Test]
    public async Task SyncLargeDataset_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var syncService = ServiceProvider.GetRequiredService<ISyncService>();
        var stopwatch = Stopwatch.StartNew();
        var maxSyncTime = TimeSpan.FromMinutes(5);

        // Act
        var result = await syncService.SyncAllAsync();

        // Assert
        stopwatch.Stop();
        Assert.IsTrue(result);
        Assert.LessOrEqual(stopwatch.Elapsed, maxSyncTime,
            $"Sync took {stopwatch.Elapsed.TotalMinutes:F2} minutes, expected under {maxSyncTime.TotalMinutes} minutes");
    }
}
```

### **2. Pruebas de Memoria**

```csharp
// Tests/RestaurantePro.Mobile.PerformanceTests/MemoryTests.cs
[TestFixture]
public class MemoryTests
{
    [Test]
    public async Task LoadLargeProductList_ShouldNotCauseMemoryLeak()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(true);
        var productoService = ServiceProvider.GetRequiredService<IProductoService>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var result = await productoService.GetProductosPaginadosAsync(i, 50);
            // Simulate processing
            await Task.Delay(10);
        }

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert
        Assert.LessOrEqual(memoryIncrease, 10 * 1024 * 1024, // 10MB limit
            $"Memory increased by {memoryIncrease / 1024 / 1024:F2} MB, expected under 10 MB");
    }

    [Test]
    public async Task NavigateBetweenPages_ShouldReleaseMemory()
    {
        // Test navigation memory management
        var navigationService = ServiceProvider.GetRequiredService<INavigationService>();
        var initialMemory = GC.GetTotalMemory(true);

        // Navigate through multiple pages
        await navigationService.NavigateToAsync("comandas");
        await navigationService.NavigateToAsync("productos");
        await navigationService.NavigateToAsync("mesas");
        await navigationService.NavigateToAsync("clientes");
        await navigationService.NavigateToAsync("dashboard");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;

        Assert.LessOrEqual(memoryIncrease, 5 * 1024 * 1024, // 5MB limit
            $"Navigation memory increased by {memoryIncrease / 1024 / 1024:F2} MB");
    }
}
```

---

## 🔒 **PRUEBAS DE SEGURIDAD**

### **1. Pruebas de Autenticación y Autorización**

```csharp
// Tests/RestaurantePro.Mobile.SecurityTests/AuthenticationTests.cs
[TestFixture]
public class AuthenticationTests
{
    [Test]
    public async Task ApiCall_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var httpClient = new HttpClient();
        var apiService = new ApiService(httpClient, null);

        // Act
        var result = await apiService.GetAsync<List<ComandaDto>>("/api/operaciones/comandas");

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(401, result.StatusCode);
    }

    [Test]
    public async Task ApiCall_WithExpiredToken_ShouldRefreshToken()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("expired_token");
        mockAuthService.Setup(x => x.RefreshTokenAsync()).ReturnsAsync("new_token");
        
        var httpClient = new HttpClient();
        var apiService = new ApiService(httpClient, mockAuthService.Object);

        // Act
        var result = await apiService.GetAsync<List<ComandaDto>>("/api/operaciones/comandas");

        // Assert
        mockAuthService.Verify(x => x.RefreshTokenAsync(), Times.Once);
    }

    [Test]
    public async Task StoreSecureData_ShouldBeEncrypted()
    {
        // Arrange
        var secureStorageService = ServiceProvider.GetRequiredService<ISecureStorageService>();
        var sensitiveData = new { Token = "sensitive_token", UserId = Guid.NewGuid() };

        // Act
        await secureStorageService.SetEncryptedAsync("user_data", sensitiveData);
        var retrievedData = await secureStorageService.GetEncryptedAsync<object>("user_data");

        // Assert
        Assert.IsNotNull(retrievedData);
        // Verify that data is actually encrypted in storage
        var rawData = await SecureStorage.GetAsync("user_data");
        Assert.IsFalse(rawData.Contains("sensitive_token"));
    }
}
```

### **2. Pruebas de Validación de Entrada**

```csharp
// Tests/RestaurantePro.Mobile.SecurityTests/InputValidationTests.cs
[TestFixture]
public class InputValidationTests
{
    [Test]
    public async Task CreateComanda_WithSQLInjection_ShouldBeSanitized()
    {
        // Arrange
        var comandaService = ServiceProvider.GetRequiredService<IComandaService>();
        var maliciousInput = "'; DROP TABLE Comandas; --";

        var request = new CrearComandaRequest
        {
            MesaId = Guid.NewGuid(),
            Observaciones = maliciousInput
        };

        // Act
        var result = await comandaService.CrearComandaAsync(request);

        // Assert
        Assert.IsTrue(result.Succeeded);
        Assert.AreNotEqual(maliciousInput, result.Data.Observaciones);
    }

    [Test]
    public async Task CreateClient_WithXSSAttack_ShouldBeSanitized()
    {
        // Arrange
        var clienteService = ServiceProvider.GetRequiredService<IClienteService>();
        var xssScript = "<script>alert('XSS')</script>";

        var request = new CrearClienteRequest
        {
            Nombre = xssScript,
            Email = "test@example.com"
        };

        // Act
        var result = await clienteService.CrearClienteAsync(request);

        // Assert
        Assert.IsTrue(result.Succeeded);
        Assert.IsFalse(result.Data.Nombre.Contains("<script>"));
    }
}
```

---

## 📊 **REPORTES Y MÉTRICAS**

### **1. Configuración de Reportes**

```xml
<!-- RestaurantePro.Mobile.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="NUnit" Version="3.14.0" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="ReportGenerator" Version="5.1.26" />
    <PackageReference Include="Appium.WebDriver" Version="4.4.0" />
    <PackageReference Include="Selenium.Support" Version="4.15.0" />
    <PackageReference Include="Moq" Version="4.20.69" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\RestaurantePro.Mobile\RestaurantePro.Mobile.csproj" />
  </ItemGroup>
</Project>
```

### **2. Scripts de Ejecución**

```bash
# Scripts/run-all-tests.sh
#!/bin/bash

echo "🧪 Ejecutando todas las pruebas de RestaurantePro Mobile..."

# Limpiar
dotnet clean

# Compilar
dotnet build

# Ejecutar pruebas unitarias
echo "📋 Ejecutando pruebas unitarias..."
dotnet test Tests/RestaurantePro.Mobile.UnitTests/RestaurantePro.Mobile.UnitTests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory TestResults/Unit \
  --logger trx

# Ejecutar pruebas de integración
echo "🔗 Ejecutando pruebas de integración..."
dotnet test Tests/RestaurantePro.Mobile.IntegrationTests/RestaurantePro.Mobile.IntegrationTests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory TestResults/Integration \
  --logger trx

# Generar reporte de cobertura
echo "📊 Generando reporte de cobertura..."
reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:"TestResults/Coverage" \
  -reporttypes:"Html;Badges"

# Ejecutar pruebas de rendimiento
echo "⚡ Ejecutando pruebas de rendimiento..."
dotnet test Tests/RestaurantePro.Mobile.PerformanceTests/RestaurantePro.Mobile.PerformanceTests.csproj \
  --logger trx \
  --results-directory TestResults/Performance

# Ejecutar pruebas de seguridad
echo "🔒 Ejecutando pruebas de seguridad..."
dotnet test Tests/RestaurantePro.Mobile.SecurityTests/RestaurantePro.Mobile.SecurityTests.csproj \
  --logger trx \
  --results-directory TestResults/Security

echo "✅ Todas las pruebas completadas. Reportes disponibles en TestResults/"
```

---

## 🎯 **CRITERIOS DE ACEPTACIÓN**

### **Definición de Terminado (DoD)**
- [ ] **80% cobertura** de código en pruebas unitarias
- [ ] **100% de servicios críticos** con pruebas de integración
- [ ] **Todos los flujos principales** con pruebas UI
- [ ] **Cero memory leaks** detectados
- [ ] **Tiempo de respuesta** < 2 segundos para operaciones críticas
- [ ] **Validación de seguridad** en todos los puntos de entrada
- [ ] **Documentación completa** de casos de prueba

### **Umbrales de Calidad**
- **Cobertura de código**: Mínimo 80%
- **Tiempo de ejecución**: Unitarias < 30 seg, Integración < 5 min
- **Tasa de éxito**: 100% en pruebas críticas
- **Rendimiento**: < 2 seg para operaciones críticas
- **Memoria**: < 100MB uso máximo

---

*Esta estrategia asegura que la aplicación móvil de RestaurantePro sea robusta, segura y performante, con cobertura completa de pruebas en todos los niveles.* 