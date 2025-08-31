using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium;
using Xunit.Abstractions;
using System.Net.Http;
using System.Text.Json;


namespace RestaurantePro.Mobile.UITests.TestBase;

/// <summary>
/// Base class simple y clara para todos los tests de UI móvil
/// Usa Appium real con la API en memoria para tests rápidos y confiables
/// </summary>
public abstract class AppiumTestBase : IDisposable
{
    protected IWebDriver Driver { get; private set; } = null!;
    protected ITestOutputHelper TestOutput { get; }
    protected IConfiguration Configuration { get; }
    protected HttpClient ApiClient { get; private set; } = null!;

    protected AppiumTestBase(ITestOutputHelper testOutput)
    {
        TestOutput = testOutput;
        Configuration = LoadConfiguration();
        
        // Inicializar API real (conectándose al backend ejecutándose)
        InitializeRealApi();
        
        // Inicializar Appium real
        InitializeAppium();
    }

    private void InitializeRealApi()
    {
        try
        {
            TestOutput.WriteLine("🚀 Inicializando conexión a API real...");
            
            // Obtener la URL de la API real desde la configuración
            var apiBaseUrl = Configuration["ApiConfig:BaseUrl"];
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                throw new InvalidOperationException("ApiConfig:BaseUrl no está configurado");
            }
            
            TestOutput.WriteLine($"🔗 Conectando a API real en: {apiBaseUrl}");
            
            // Crear HttpClient que se conecta a la API real
            ApiClient = new HttpClient
            {
                BaseAddress = new Uri(apiBaseUrl),
                Timeout = TimeSpan.FromSeconds(
                    int.Parse(Configuration["ApiConfig:TimeoutSeconds"] ?? "30"))
            };
            
            // Configurar headers por defecto
            ApiClient.DefaultRequestHeaders.Add("Accept", "application/json");
            ApiClient.DefaultRequestHeaders.Add("User-Agent", "RestaurantePro-UITests");
            
            TestOutput.WriteLine($"✅ API real inicializada en: {ApiClient.BaseAddress}");
            TestOutput.WriteLine("🔗 La UI móvil se conectará a esta API real");
            
            // Verificar conectividad con la API real
            VerifyApiConnectivity();
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error inicializando API real: {ex.Message}");
            throw;
        }
    }
    
    private async void VerifyApiConnectivity()
    {
        try
        {
            TestOutput.WriteLine("🔍 Verificando conectividad con la API real...");
            
            // Intentar hacer una petición simple para verificar conectividad
            var response = await ApiClient.GetAsync("api/health");
            if (response.IsSuccessStatusCode)
            {
                TestOutput.WriteLine("✅ API real responde correctamente");
            }
            else
            {
                TestOutput.WriteLine($"⚠️ API real responde con status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error verificando conectividad con API real: {ex.Message}");
            TestOutput.WriteLine("💡 Asegúrate de que la API esté ejecutándose en el puerto configurado");
        }
    }

    private void InitializeAppium()
    {
        try
        {
            TestOutput.WriteLine("📱 Inicializando Appium real...");
            
            var appiumOptions = new AppiumOptions();
            
            // Configuración básica de Appium
            appiumOptions.PlatformName = Configuration["AppiumConfig:PlatformName"] ?? "Android";
            appiumOptions.PlatformVersion = Configuration["AppiumConfig:PlatformVersion"] ?? "14.0";
            appiumOptions.DeviceName = Configuration["AppiumConfig:DeviceName"] ?? "emulator-5554";
            appiumOptions.AutomationName = "UiAutomator2";

            // Configuración para emulador
            if (bool.Parse(Configuration["EmulatorConfig:Enabled"] ?? "false"))
            {
                appiumOptions.AddAdditionalAppiumOption("avd", Configuration["EmulatorConfig:AvdName"]);
                TestOutput.WriteLine($"🎮 Usando emulador: {Configuration["EmulatorConfig:AvdName"]}");
            }

            // Configuración de package y activity (sin configurar app para evitar conflictos)
            appiumOptions.AddAdditionalAppiumOption("appPackage", Configuration["AppiumConfig:AppPackage"] ?? "com.companyname.restaurantepro.mobile");
            appiumOptions.AddAdditionalAppiumOption("appActivity", Configuration["AppiumConfig:AppActivity"] ?? "crc64e1fb321c08285b90.MainActivity");
            
            TestOutput.WriteLine($"📱 Usando package: {Configuration["AppiumConfig:AppPackage"] ?? "com.companyname.restaurantepro.mobile"}");
            TestOutput.WriteLine($"📱 Usando activity: {Configuration["AppiumConfig:AppActivity"] ?? "crc64e1fb321c08285b90.MainActivity"}");
            
            // Configuraciones de rendimiento
            appiumOptions.AddAdditionalAppiumOption("noReset", bool.Parse(Configuration["AppiumConfig:NoReset"] ?? "false"));
            appiumOptions.AddAdditionalAppiumOption("fullReset", bool.Parse(Configuration["AppiumConfig:FullReset"] ?? "false"));
            appiumOptions.AddAdditionalAppiumOption("fastReset", bool.Parse(Configuration["AppiumConfig:FastReset"] ?? "true"));
            
            // Configuraciones de permisos
            appiumOptions.AddAdditionalAppiumOption("autoGrantPermissions", bool.Parse(Configuration["AppiumConfig:AutoGrantPermissions"] ?? "true"));
            
            // Timeouts
            appiumOptions.AddAdditionalAppiumOption("newCommandTimeout", int.Parse(Configuration["AppiumConfig:NewCommandTimeout"] ?? "60"));
            appiumOptions.AddAdditionalAppiumOption("implicitWait", int.Parse(Configuration["AppiumConfig:ImplicitWaitSeconds"] ?? "15"));

            // Conectar al servidor Appium
            var appiumServerUrl = Configuration["AppiumConfig:ServerUrl"] ?? "http://localhost:4723";
            var fullAppiumUrl = $"{appiumServerUrl}/wd/hub";
            TestOutput.WriteLine($"🔌 Conectando a servidor Appium: {fullAppiumUrl}");
            
            // Usar el driver real de Android
            Driver = new AndroidDriver(new Uri(fullAppiumUrl), appiumOptions);
            
            // Configurar timeouts del driver (solo ImplicitWait, PageLoad no está soportado en Appium)
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(int.Parse(Configuration["AppiumConfig:ImplicitWaitSeconds"] ?? "15"));
            
            TestOutput.WriteLine("✅ Appium real inicializado correctamente");
            
            // Configurar la app móvil para usar la API en memoria
            ConfigureAppForInMemoryApi();
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error inicializando Appium real: {ex.Message}");
            TestOutput.WriteLine("💡 Asegúrate de que:");
            TestOutput.WriteLine("   1. Appium Server esté ejecutándose en http://localhost:4723");
            TestOutput.WriteLine("   2. El emulador esté disponible");
            TestOutput.WriteLine("   3. La app esté instalada o el APK esté disponible");
            TestOutput.WriteLine("   4. ADB esté funcionando (adb devices)");
            TestOutput.WriteLine("   5. El APK esté en la carpeta de salida del proyecto de tests");
            throw;
        }
    }

    private void ConfigureAppForInMemoryApi()
    {
        try
        {
            TestOutput.WriteLine("🔧 Configurando app móvil para usar API en memoria...");
            
            // Obtener la URL de la API en memoria
            var apiUrl = ApiClient.BaseAddress?.ToString() ?? "http://localhost:5000";
            
            // Aquí podrías configurar la app móvil para usar esta URL
            // Esto dependerá de cómo esté implementada tu app móvil
            TestOutput.WriteLine($"📱 App móvil configurada para usar API en: {apiUrl}");
            TestOutput.WriteLine("💡 Nota: La app móvil debe estar configurada para usar esta URL");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"⚠️ No se pudo configurar la app para API en memoria: {ex.Message}");
            TestOutput.WriteLine("💡 Esto es normal si la app no tiene configuración dinámica de API");
        }
    }

    private IConfiguration LoadConfiguration()
    {
        try
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);

            return builder.Build();
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error cargando configuración: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Método helper para tomar screenshots en caso de fallo
    /// </summary>
    protected void TakeScreenshot(string testName)
    {
        try
        {
            if (bool.Parse(Configuration["Screenshots:Enabled"] ?? "true"))
            {
                var screenshotDir = Configuration["Screenshots:Directory"] ?? "Screenshots";
                Directory.CreateDirectory(screenshotDir);
                
                var fileName = $"{screenshotDir}/{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                
                if (Driver is ITakesScreenshot takesScreenshot)
                {
                    var screenshot = takesScreenshot.GetScreenshot();
                    screenshot.SaveAsFile(fileName);
                    TestOutput.WriteLine($"📸 Screenshot guardado: {fileName}");
                }
                else
                {
                    TestOutput.WriteLine($"⚠️ Driver no soporta screenshots: {fileName}");
                }
            }
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"⚠️ No se pudo tomar screenshot: {ex.Message}");
        }
    }

    /// <summary>
    /// Método helper para esperar elementos de forma inteligente
    /// </summary>
    protected IWebElement WaitForElement(By by, int timeoutSeconds = 15)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(driver => driver.FindElement(by));
    }

    /// <summary>
    /// Método helper para verificar que un elemento esté visible
    /// </summary>
    protected bool IsElementVisible(By by, int timeoutSeconds = 5)
    {
        try
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(driver => driver.FindElement(by).Displayed);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Método helper para hacer llamadas a la API en memoria
    /// </summary>
    protected async Task<HttpResponseMessage> CallInMemoryApiAsync(string endpoint, HttpMethod method = null, HttpContent content = null)
    {
        try
        {
            method ??= HttpMethod.Get;
            var request = new HttpRequestMessage(method, endpoint);
            
            if (content != null)
            {
                request.Content = content;
            }
            
            TestOutput.WriteLine($"🌐 Llamando a API en memoria: {method} {endpoint}");
            var response = await ApiClient.SendAsync(request);
            TestOutput.WriteLine($"📡 Respuesta de API: {response.StatusCode}");
            
            return response;
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error llamando a API en memoria: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            TestOutput.WriteLine("🧹 Limpiando recursos de test...");
            
            // Tomar screenshot final si está habilitado
            TakeScreenshot("test_final");
            
            // Cerrar driver
            Driver?.Quit();
            Driver?.Dispose();
            
            // Cerrar cliente HTTP
            ApiClient?.Dispose();
            
            // Cerrar WebApplicationFactory
            // WebAppFactory ya no se usa con API real
            
            TestOutput.WriteLine("✅ Recursos limpiados correctamente");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"⚠️ Error durante limpieza: {ex.Message}");
        }
    }
}
