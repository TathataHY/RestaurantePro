using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using System.Drawing;
using System.Drawing.Imaging;

namespace RestaurantePro.Mobile.UITests.TestBase;

public abstract class AppiumTestBase : IAsyncLifetime
{
    protected IWebDriver Driver { get; private set; } = null!;
    protected AppiumOptions Options { get; private set; } = null!;
    protected IConfiguration Configuration { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;
    protected ITestOutputHelper TestOutput { get; }

    protected AppiumTestBase(ITestOutputHelper testOutput)
    {
        TestOutput = testOutput;
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: false)
            .Build();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            builder.AddConsole();
        });
        Logger = loggerFactory.CreateLogger<AppiumTestBase>();
    }

    public async Task InitializeAsync()
    {
        try
        {
            Options = new AppiumOptions();
            
            // Configuración básica de Appium usando propiedades dedicadas
            Options.PlatformName = Configuration["AppiumConfig:PlatformName"];
                                Options.PlatformVersion = Configuration["AppiumConfig:PlatformVersion"];
                    Options.DeviceName = Configuration["AppiumConfig:DeviceName"];
                    Options.AutomationName = "UiAutomator2";

            // Configuración específica para Android/WSA
            Options.AddAdditionalAppiumOption("appPackage", Configuration["AppiumConfig:AppPackage"]);
            Options.AddAdditionalAppiumOption("appActivity", Configuration["AppiumConfig:AppActivity"]);
            Options.AddAdditionalAppiumOption("noReset", bool.Parse(Configuration["AppiumConfig:NoReset"] ?? "false"));
            Options.AddAdditionalAppiumOption("fullReset", bool.Parse(Configuration["AppiumConfig:FullReset"] ?? "false"));
            Options.AddAdditionalAppiumOption("fastReset", bool.Parse(Configuration["AppiumConfig:FastReset"] ?? "true"));
            Options.AddAdditionalAppiumOption("autoGrantPermissions", bool.Parse(Configuration["AppiumConfig:AutoGrantPermissions"] ?? "true"));
            Options.AddAdditionalAppiumOption("newCommandTimeout", int.Parse(Configuration["AppiumConfig:NewCommandTimeout"] ?? "60"));
            Options.AddAdditionalAppiumOption("unicodeKeyboard", bool.Parse(Configuration["AppiumConfig:UnicodeKeyboard"] ?? "true"));
            Options.AddAdditionalAppiumOption("resetKeyboard", bool.Parse(Configuration["AppiumConfig:ResetKeyboard"] ?? "true"));
            Options.AddAdditionalAppiumOption("uiautomator2ServerLaunchTimeout", 60000);
            Options.AddAdditionalAppiumOption("uiautomator2ServerInstallTimeout", 60000);
            Options.AddAdditionalAppiumOption("androidInstallTimeout", 90000);
            Options.AddAdditionalAppiumOption("adbExecTimeout", 60000);

                    // Configuración específica para WSA con UiAutomator2 optimizado
            if (bool.Parse(Configuration["WSAConfig:Enabled"] ?? "true"))
            {
                // Configuración básica para WSA
                Options.AddAdditionalAppiumOption("wsaEnabled", true);
                Options.AddAdditionalAppiumOption("wsaConnectionTimeout", int.Parse(Configuration["WSAConfig:ConnectionTimeout"] ?? "30"));
                
                                        // Configuración específica para UiAutomator2 en WSA - Optimizada para evitar crashes
                        Options.AddAdditionalAppiumOption("skipServerInstallation", false);
                        Options.AddAdditionalAppiumOption("skipDeviceInitialization", false);
                        Options.AddAdditionalAppiumOption("dontStopAppOnReset", false);
                        Options.AddAdditionalAppiumOption("autoLaunch", true);
                        
                        // Configuración de espera de actividades
                        Options.AddAdditionalAppiumOption("appWaitActivity", "crc64492cedc7810ceddb.MainActivity");
                        Options.AddAdditionalAppiumOption("appActivity", "crc64492cedc7810ceddb.MainActivity");
                        Options.AddAdditionalAppiumOption("appWaitDuration", 30000);
                        
                        // Configuración de timeouts optimizada para WSA con UiAutomator2
                        Options.AddAdditionalAppiumOption("androidDeviceReadyTimeout", 60);
                        Options.AddAdditionalAppiumOption("androidInstallTimeout", 120000);
                        Options.AddAdditionalAppiumOption("adbExecTimeout", 120000);
                        Options.AddAdditionalAppiumOption("uiautomator2ServerLaunchTimeout", 120000);
                        Options.AddAdditionalAppiumOption("uiautomator2ServerInstallTimeout", 120000);
                        
                        // Configuración adicional para estabilidad
                        Options.AddAdditionalAppiumOption("newCommandTimeout", 120);
                        Options.AddAdditionalAppiumOption("autoGrantPermissions", true);
                        Options.AddAdditionalAppiumOption("allowTestPackages", true);
                        Options.AddAdditionalAppiumOption("disableWindowAnimation", true);
                        Options.AddAdditionalAppiumOption("disableSuppressAccessibilityService", true);
                
                // Configuración adicional para WSA
                Options.AddAdditionalAppiumOption("noReset", true);
                Options.AddAdditionalAppiumOption("fullReset", false);
                Options.AddAdditionalAppiumOption("newCommandTimeout", 60);
                Options.AddAdditionalAppiumOption("autoGrantPermissions", true);
                Options.AddAdditionalAppiumOption("allowTestPackages", true);
                Options.AddAdditionalAppiumOption("disableWindowAnimation", true);
                Options.AddAdditionalAppiumOption("disableSuppressAccessibilityService", true);
            }

            var serverUrl = Configuration["AppiumConfig:ServerUrl"];
            if (string.IsNullOrEmpty(serverUrl))
            {
                throw new InvalidOperationException("ServerUrl no está configurado en appsettings.Test.json");
            }

            var implicitWaitSeconds = int.Parse(Configuration["AppiumConfig:ImplicitWaitSeconds"] ?? "15");
            var pageLoadTimeoutSeconds = int.Parse(Configuration["AppiumConfig:PageLoadTimeoutSeconds"] ?? "30");

            TestOutput.WriteLine($"Conectando a Appium Server: {serverUrl}");
            TestOutput.WriteLine($"Dispositivo: {Options.DeviceName}");
            TestOutput.WriteLine($"Plataforma: {Options.PlatformName} {Options.PlatformVersion}");

            Driver = new AndroidDriver(new Uri(serverUrl), Options);
                    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitWaitSeconds);
        // PageLoad timeout no es compatible con Appium en Android
        // Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeoutSeconds);

            TestOutput.WriteLine("Driver de Appium inicializado exitosamente");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"Error al inicializar el driver: {ex.Message}");
            Logger.LogError(ex, "Error al inicializar el driver de Appium");
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (Driver != null)
            {
                Driver.Quit();
                Driver.Dispose();
                TestOutput.WriteLine("Driver de Appium cerrado exitosamente");
            }
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"Error al cerrar el driver: {ex.Message}");
            Logger.LogError(ex, "Error al cerrar el driver de Appium");
        }
    }

    protected IWebElement WaitForElement(By by, int timeoutSeconds = 10)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(driver => driver.FindElement(by));
    }

    protected bool ElementExists(By by, int timeoutSeconds = 5)
    {
        try
        {
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(driver => driver.FindElement(by));
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected void TakeScreenshot(string testName)
    {
        try
        {
            var screenshotEnabled = bool.Parse(Configuration["Screenshots:Enabled"] ?? "true");
            if (!screenshotEnabled) return;

            var screenshotDir = Configuration["Screenshots:Directory"] ?? "Screenshots";
            var fullDir = Path.Combine(Directory.GetCurrentDirectory(), screenshotDir);
            
            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{testName}_{timestamp}.png";
            var fullPath = Path.Combine(fullDir, fileName);

            var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
            File.WriteAllBytes(fullPath, screenshot.AsByteArray);

            TestOutput.WriteLine($"Screenshot guardado: {fullPath}");
        }
        catch (Exception ex)
        {
            TestOutput.WriteLine($"Error al tomar screenshot: {ex.Message}");
            Logger.LogError(ex, "Error al tomar screenshot");
        }
    }

    protected void LogInfo(string message)
    {
        TestOutput.WriteLine($"[INFO] {message}");
        Logger.LogInformation(message);
    }

    protected void LogError(string message, Exception? ex = null)
    {
        TestOutput.WriteLine($"[ERROR] {message}");
        if (ex != null)
        {
            TestOutput.WriteLine($"[ERROR] Exception: {ex.Message}");
        }
        Logger.LogError(ex, message);
    }
} 