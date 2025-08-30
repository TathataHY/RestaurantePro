using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium;
using Xunit.Abstractions;
using System.Net.Http;
using System.Text.Json;

namespace RestaurantePro.Mobile.UITests.TestBase;

/// <summary>
/// Base class para UI tests que incluye API externa real para pruebas end-to-end completas
/// </summary>
public abstract class AppiumTestBaseWithRealApi : IDisposable
{
    protected IWebDriver Driver { get; private set; }
    protected ITestOutputHelper TestOutput { get; }
    protected IConfiguration Configuration { get; }
    protected HttpClient ApiClient { get; private set; }

    protected AppiumTestBaseWithRealApi(ITestOutputHelper testOutput)
    {
        TestOutput = testOutput;
        Configuration = LoadConfiguration();
        
        // Inicializar API externa real
        InitializeRealApi();
        
        // Inicializar Appium
        InitializeAppium();
    }

    private void InitializeRealApi()
    {
        try
        {
            TestOutput.WriteLine("🚀 Inicializando conexión con API externa real...");
            
            // Crear HttpClient para conectar a la API externa
            ApiClient = new HttpClient();
            
            // Configurar base URL de la API
            var apiBaseUrl = Configuration["ApiConfig:BaseUrl"] ?? "https://localhost:7001";
            ApiClient.BaseAddress = new Uri(apiBaseUrl);
            
            // Configurar headers por defecto
            ApiClient.DefaultRequestHeaders.Add("Accept", "application/json");
            
            // Verificar que la API esté disponible
            TestApiConnection();
            
            TestOutput.WriteLine($"✅ Conexión con API externa establecida: {apiBaseUrl}");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error conectando con API externa: {ex.Message}");
            TestOutput.WriteLine("⚠️ Asegúrate de que la API esté ejecutándose en: https://localhost:7001");
            throw;
        }
    }

    private async void TestApiConnection()
    {
        try
        {
            TestOutput.WriteLine("🔍 Verificando conectividad con API...");
            
            var response = await ApiClient.GetAsync("/api/health");
            if (response.IsSuccessStatusCode)
            {
                TestOutput.WriteLine("✅ API externa respondiendo correctamente");
            }
            else
            {
                TestOutput.WriteLine($"⚠️ API externa respondió con código: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"⚠️ No se pudo conectar con API externa: {ex.Message}");
            TestOutput.WriteLine("💡 Ejecuta: dotnet run --project src/Backend/RestaurantePro.Api");
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

            // Configuración específica para Emulador o WSA
            if (bool.Parse(Configuration["EmulatorConfig:Enabled"] ?? "false"))
            {
                // Configuración básica para emulador
                appiumOptions.AddAdditionalAppiumOption("avd", Configuration["EmulatorConfig:AvdName"]);
                appiumOptions.AddAdditionalAppiumOption("avdLaunchTimeout", 120000);
                appiumOptions.AddAdditionalAppiumOption("avdReadyTimeout", 120000);
                
                // Configuración simplificada para emulador
                appiumOptions.AddAdditionalAppiumOption("skipServerInstallation", false);
                appiumOptions.AddAdditionalAppiumOption("skipDeviceInitialization", false);
                appiumOptions.AddAdditionalAppiumOption("dontStopAppOnReset", false);
                appiumOptions.AddAdditionalAppiumOption("autoLaunch", false);
                
                // Configuración de espera de actividades
                appiumOptions.AddAdditionalAppiumOption("appWaitActivity", "crc64492cedc7810ceddb.MainActivity");
                appiumOptions.AddAdditionalAppiumOption("appWaitDuration", 60000);
                
                // Configuración de timeouts optimizada
                appiumOptions.AddAdditionalAppiumOption("androidDeviceReadyTimeout", 120);
                appiumOptions.AddAdditionalAppiumOption("androidInstallTimeout", 180000);
                appiumOptions.AddAdditionalAppiumOption("adbExecTimeout", 180000);
                appiumOptions.AddAdditionalAppiumOption("uiautomator2ServerLaunchTimeout", 180000);
                appiumOptions.AddAdditionalAppiumOption("uiautomator2ServerInstallTimeout", 180000);
                
                // Configuración adicional
                appiumOptions.AddAdditionalAppiumOption("noReset", true);
                appiumOptions.AddAdditionalAppiumOption("fullReset", false);
                appiumOptions.AddAdditionalAppiumOption("fastReset", false);
                appiumOptions.AddAdditionalAppiumOption("newCommandTimeout", 180);
                appiumOptions.AddAdditionalAppiumOption("autoGrantPermissions", true);
                appiumOptions.AddAdditionalAppiumOption("disableWindowAnimation", true);
                appiumOptions.AddAdditionalAppiumOption("disableSuppressAccessibilityService", true);
            }
            else if (bool.Parse(Configuration["WSAConfig:Enabled"] ?? "true"))
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
            var appiumUrl = new Uri(Configuration["AppiumConfig:ServerUrl"] ?? "http://localhost:4723");
            Driver = new AndroidDriver(appiumUrl, appiumOptions, TimeSpan.FromSeconds(120));
            
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

    /// <summary>
    /// Ejecuta una acción en la API externa real
    /// </summary>
    protected async Task<HttpResponseMessage> CallApiAsync(HttpMethod method, string endpoint, object content = null)
    {
        var request = new HttpRequestMessage(method, endpoint);
        
        if (content != null)
        {
            var json = JsonSerializer.Serialize(content);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }
        
        TestOutput.WriteLine($"🌐 API {method} {endpoint}");
        return await ApiClient.SendAsync(request);
    }

    /// <summary>
    /// Obtiene datos de la API externa real
    /// </summary>
    protected async Task<T> GetFromApiAsync<T>(string endpoint)
    {
        try
        {
            var response = await ApiClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            TestOutput.WriteLine($"✅ API GET {endpoint} - Respuesta: {content.Length} caracteres");
            
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error en API GET {endpoint}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Envía datos a la API externa real
    /// </summary>
    protected async Task<T> PostToApiAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await ApiClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            TestOutput.WriteLine($"✅ API POST {endpoint} - Respuesta: {responseContent.Length} caracteres");
            
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error en API POST {endpoint}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Actualiza datos en la API externa real
    /// </summary>
    protected async Task<T> PutToApiAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await ApiClient.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            TestOutput.WriteLine($"✅ API PUT {endpoint} - Respuesta: {responseContent.Length} caracteres");
            
            return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error en API PUT {endpoint}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Elimina datos en la API externa real
    /// </summary>
    protected async Task<bool> DeleteFromApiAsync(string endpoint)
    {
        try
        {
            var response = await ApiClient.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
            
            TestOutput.WriteLine($"✅ API DELETE {endpoint} - Exitosa");
            return true;
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error en API DELETE {endpoint}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Autentica un usuario en la API externa real
    /// </summary>
    protected async Task<string> AuthenticateUserAsync(string email, string password)
    {
        try
        {
            TestOutput.WriteLine($"🔐 Autenticando usuario: {email}");
            
            var loginData = new { Email = email, Password = password };
            var response = await PostToApiAsync<object>("/api/auth/login", loginData);
            
            // Extraer token de la respuesta (ajustar según la estructura real de la API)
            var responseJson = JsonSerializer.Serialize(response);
            TestOutput.WriteLine($"✅ Usuario autenticado: {email}");
            
            // Configurar token en headers para futuras requests
            // ApiClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            return "mock-token"; // Ajustar según la respuesta real
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"❌ Error autenticando usuario: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            // Cerrar Appium
            Driver?.Quit();
            Driver?.Dispose();
            
            // Cerrar API client
            ApiClient?.Dispose();
            
            TestOutput.WriteLine("✅ Recursos liberados correctamente");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"⚠️ Error liberando recursos: {ex.Message}");
        }
    }
} 