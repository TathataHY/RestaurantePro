using OpenQA.Selenium.Appium.Android;
using RestaurantePro.Mobile.UITests.TestBase;

namespace RestaurantePro.Mobile.UITests.PageObjects;

public class DashboardPageObject
{
    private readonly IWebDriver _driver;
    private readonly ITestOutputHelper _testOutput;

    // Elementos del dashboard
    private By DashboardTitle => By.Id("DashboardTitle");
    private By MesasTab => By.Id("MesasTab");
    private By ComandasTab => By.Id("ComandasTab");
    private By ProductosTab => By.Id("ProductosTab");
    private By PreparacionesTab => By.Id("PreparacionesTab");
    private By ReservacionesTab => By.Id("ReservacionesTab");
    private By FacturasTab => By.Id("FacturasTab");
    private By ClientesTab => By.Id("ClientesTab");
    private By LogoutButton => By.Id("LogoutButton");
    private By UserProfileButton => By.Id("UserProfileButton");
    private By NotificationsButton => By.Id("NotificationsButton");

    public DashboardPageObject(IWebDriver driver, ITestOutputHelper testOutput)
    {
        _driver = driver;
        _testOutput = testOutput;
    }

    public bool IsDashboardDisplayed()
    {
        try
        {
            var dashboardTitle = _driver.FindElement(DashboardTitle);
            return dashboardTitle.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public string GetDashboardTitle()
    {
        try
        {
            var dashboardTitle = _driver.FindElement(DashboardTitle);
            return dashboardTitle.Text;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al obtener título del dashboard: {ex.Message}");
            return string.Empty;
        }
    }

    public void NavigateToMesas()
    {
        try
        {
            var mesasTab = _driver.FindElement(MesasTab);
            mesasTab.Click();
            _testOutput.WriteLine("Navegado a la sección de Mesas");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al navegar a Mesas: {ex.Message}");
            throw;
        }
    }

    public void NavigateToComandas()
    {
        try
        {
            var comandasTab = _driver.FindElement(ComandasTab);
            comandasTab.Click();
            _testOutput.WriteLine("Navegado a la sección de Comandas");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al navegar a Comandas: {ex.Message}");
            throw;
        }
    }

    public void NavigateToProductos()
    {
        try
        {
            var productosTab = _driver.FindElement(ProductosTab);
            productosTab.Click();
            _testOutput.WriteLine("Navegado a la sección de Productos");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al navegar a Productos: {ex.Message}");
            throw;
        }
    }

    public void NavigateToPreparaciones()
    {
        try
        {
            var preparacionesTab = _driver.FindElement(PreparacionesTab);
            preparacionesTab.Click();
            _testOutput.WriteLine("Navegado a la sección de Preparaciones");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al navegar a Preparaciones: {ex.Message}");
            throw;
        }
    }

    public void NavigateToReservaciones()
    {
        try
        {
            var reservacionesTab = _driver.FindElement(ReservacionesTab);
            reservacionesTab.Click();
            _testOutput.WriteLine("Navegado a la sección de Reservaciones");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al navegar a Reservaciones: {ex.Message}");
            throw;
        }
    }

    public void NavigateToFacturas()
    {
        try
        {
            var facturasTab = _driver.FindElement(FacturasTab);
            facturasTab.Click();
            _testOutput.WriteLine("Navegado a la sección de Facturas");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al navegar a Facturas: {ex.Message}");
            throw;
        }
    }

    public void NavigateToClientes()
    {
        try
        {
            var clientesTab = _driver.FindElement(ClientesTab);
            clientesTab.Click();
            _testOutput.WriteLine("Navegado a la sección de Clientes");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al navegar a Clientes: {ex.Message}");
            throw;
        }
    }

    public void ClickLogout()
    {
        try
        {
            var logoutButton = _driver.FindElement(LogoutButton);
            logoutButton.Click();
            _testOutput.WriteLine("Botón de logout clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al hacer logout: {ex.Message}");
            throw;
        }
    }

    public void ClickUserProfile()
    {
        try
        {
            var userProfileButton = _driver.FindElement(UserProfileButton);
            userProfileButton.Click();
            _testOutput.WriteLine("Perfil de usuario clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al abrir perfil de usuario: {ex.Message}");
        }
    }

    public void ClickNotifications()
    {
        try
        {
            var notificationsButton = _driver.FindElement(NotificationsButton);
            notificationsButton.Click();
            _testOutput.WriteLine("Notificaciones clickeadas");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al abrir notificaciones: {ex.Message}");
        }
    }

    public bool WaitForDashboardToLoad()
    {
        try
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => IsDashboardDisplayed());
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsTabVisible(string tabName)
    {
        try
        {
            By tabSelector = tabName.ToLower() switch
            {
                "mesas" => MesasTab,
                "comandas" => ComandasTab,
                "productos" => ProductosTab,
                "preparaciones" => PreparacionesTab,
                "reservaciones" => ReservacionesTab,
                "facturas" => FacturasTab,
                "clientes" => ClientesTab,
                _ => throw new ArgumentException($"Tab '{tabName}' no reconocido")
            };

            var tab = _driver.FindElement(tabSelector);
            return tab.Displayed;
        }
        catch
        {
            return false;
        }
    }
} 