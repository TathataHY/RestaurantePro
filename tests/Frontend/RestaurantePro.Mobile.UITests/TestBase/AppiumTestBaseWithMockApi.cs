using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Interfaces;
using Xunit.Abstractions;

namespace RestaurantePro.Mobile.UITests.TestBase;

/// <summary>
/// Base class para UI tests que incluye API mock para pruebas end-to-end completas
/// </summary>
public abstract class AppiumTestBaseWithMockApi : IDisposable
{
    protected IWebDriver Driver { get; private set; }
    protected ITestOutputHelper TestOutput { get; }
    protected IConfiguration Configuration { get; }
    protected MockApiService MockApi { get; private set; }

    protected AppiumTestBaseWithMockApi(ITestOutputHelper testOutput)
    {
        TestOutput = testOutput;
        Configuration = LoadConfiguration();
        
        // Inicializar API mock
        InitializeMockApi();
        
        // Inicializar Appium
        InitializeAppium();
    }

    private void InitializeMockApi()
    {
        try
        {
            TestOutput.WriteLine("🚀 Inicializando API mock...");
            
            // Crear servicio mock de API
            MockApi = new MockApiService(TestOutput);
            
            TestOutput.WriteLine("✅ API mock inicializada correctamente");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error inicializando API mock: {ex.Message}");
            throw;
        }
    }

    private void InitializeAppium()
    {
        try
        {
            TestOutput.WriteLine("📱 Inicializando Appium...");
            
            var appiumOptions = new AppiumOptions();
            
            // Configuración básica
            appiumOptions.PlatformName = Configuration["AppiumConfig:PlatformName"];
            appiumOptions.PlatformVersion = Configuration["AppiumConfig:PlatformVersion"];
            appiumOptions.DeviceName = Configuration["AppiumConfig:DeviceName"];
            appiumOptions.AutomationName = "UiAutomator2";

            // Configuración específica para WSA
            if (bool.Parse(Configuration["WSAConfig:Enabled"] ?? "true"))
            {
                appiumOptions.AddAdditionalAppiumOption("wsaEnabled", true);
                appiumOptions.AddAdditionalAppiumOption("wsaConnectionTimeout", int.Parse(Configuration["WSAConfig:ConnectionTimeout"] ?? "30"));
                
                // Configuración optimizada para WSA
                appiumOptions.AddAdditionalAppiumOption("skipServerInstallation", true);
                appiumOptions.AddAdditionalAppiumOption("skipDeviceInitialization", true);
                appiumOptions.AddAdditionalAppiumOption("dontStopAppOnReset", true);
                appiumOptions.AddAdditionalAppiumOption("autoLaunch", false);
                
                // Configuración de espera de actividades
                appiumOptions.AddAdditionalAppiumOption("appWaitActivity", "crc64492cedc7810ceddb.MainActivity");
                appiumOptions.AddAdditionalAppiumOption("appActivity", "crc64492cedc7810ceddb.MainActivity");
                appiumOptions.AddAdditionalAppiumOption("appWaitDuration", 30000);
                
                // Configuración de timeouts optimizada
                appiumOptions.AddAdditionalAppiumOption("androidDeviceReadyTimeout", 30);
                appiumOptions.AddAdditionalAppiumOption("androidInstallTimeout", 60000);
                appiumOptions.AddAdditionalAppiumOption("adbExecTimeout", 60000);
                appiumOptions.AddAdditionalAppiumOption("uiautomator2ServerLaunchTimeout", 60000);
                appiumOptions.AddAdditionalAppiumOption("uiautomator2ServerInstallTimeout", 60000);
                
                // Configuración adicional
                appiumOptions.AddAdditionalAppiumOption("noReset", true);
                appiumOptions.AddAdditionalAppiumOption("fullReset", false);
                appiumOptions.AddAdditionalAppiumOption("newCommandTimeout", 60);
                appiumOptions.AddAdditionalAppiumOption("autoGrantPermissions", true);
                appiumOptions.AddAdditionalAppiumOption("disableWindowAnimation", true);
                appiumOptions.AddAdditionalAppiumOption("disableSuppressAccessibilityService", true);
            }

            // Configurar la aplicación
            var appPath = Configuration["AppiumConfig:AppPath"];
            if (!string.IsNullOrEmpty(appPath) && File.Exists(appPath))
            {
                appiumOptions.App = appPath;
            }
            else
            {
                appiumOptions.AddAdditionalAppiumOption("appPackage", Configuration["AppiumConfig:AppPackage"]);
                appiumOptions.AddAdditionalAppiumOption("appActivity", Configuration["AppiumConfig:AppActivity"]);
            }

            // Conectar a Appium Server
            var appiumUrl = new Uri(Configuration["AppiumConfig:ServerUrl"] ?? "http://localhost:4723/wd/hub");
            Driver = new AndroidDriver(appiumUrl, appiumOptions, TimeSpan.FromSeconds(60));
            
            TestOutput.WriteLine("✅ Appium inicializado correctamente");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error inicializando Appium: {ex.Message}");
            throw;
        }
    }

    private IConfiguration LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: true)
            .Build();
    }

    public void Dispose()
    {
        try
        {
            // Cerrar Appium
            Driver?.Quit();
            Driver?.Dispose();
            
            TestOutput.WriteLine("✅ Recursos liberados correctamente");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"⚠️ Error liberando recursos: {ex.Message}");
        }
    }
}

/// <summary>
/// Servicio mock de API para simular respuestas del backend
/// </summary>
public class MockApiService
{
    private readonly ITestOutputHelper _testOutput;
    private readonly Dictionary<string, object> _mockData;

    public MockApiService(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
        _mockData = new Dictionary<string, object>();
        InitializeMockData();
    }

    private void InitializeMockData()
    {
        // Datos mock para mesas
        _mockData["mesas"] = new List<object>
        {
            new { Id = 1, Numero = "1", Capacidad = 4, Estado = "Libre" },
            new { Id = 2, Numero = "2", Capacidad = 2, Estado = "Libre" },
            new { Id = 3, Numero = "3", Capacidad = 6, Estado = "Ocupada" }
        };

        // Datos mock para productos
        _mockData["productos"] = new List<object>
        {
            new { Id = 1, Nombre = "Hamburguesa", Precio = 15.99m, Categoria = "Platos Principales" },
            new { Id = 2, Nombre = "Coca Cola", Precio = 2.50m, Categoria = "Bebidas" },
            new { Id = 3, Nombre = "Ensalada César", Precio = 12.99m, Categoria = "Ensaladas" }
        };

        // Datos mock para comandas
        _mockData["comandas"] = new List<object>
        {
            new { Id = 1, MesaId = 3, Estado = "En Preparación", Total = 28.98m }
        };

        _testOutput.WriteLine("✅ Datos mock inicializados");
    }

    public async Task<T> GetFromApiAsync<T>(string endpoint)
    {
        _testOutput.WriteLine($"🔍 Mock API GET: {endpoint}");
        
        // Simular delay de red
        await Task.Delay(100);
        
        if (endpoint.Contains("/mesas"))
        {
            return (T)_mockData["mesas"];
        }
        else if (endpoint.Contains("/productos"))
        {
            return (T)_mockData["productos"];
        }
        else if (endpoint.Contains("/comandas"))
        {
            return (T)_mockData["comandas"];
        }
        
        return default(T)!;
    }

    public async Task<T> PostToApiAsync<T>(string endpoint, object data)
    {
        _testOutput.WriteLine($"📤 Mock API POST: {endpoint}");
        
        // Simular delay de red
        await Task.Delay(200);
        
        // Simular respuesta exitosa
        var response = new { Success = true, Message = "Operación completada exitosamente", Data = data };
        
        return (T)(object)response;
    }

    public async Task<object> CallApiAsync(string method, string endpoint, object content = null)
    {
        _testOutput.WriteLine($"🔄 Mock API {method}: {endpoint}");
        
        // Simular delay de red
        await Task.Delay(150);
        
        // Simular respuesta exitosa
        return new { Success = true, Message = "Operación completada exitosamente" };
    }
} 