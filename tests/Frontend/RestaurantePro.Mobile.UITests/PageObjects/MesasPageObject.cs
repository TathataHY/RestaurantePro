using OpenQA.Selenium.Appium.Android;
using RestaurantePro.Mobile.UITests.TestBase;

namespace RestaurantePro.Mobile.UITests.PageObjects;

public class MesasPageObject
{
    private readonly IWebDriver _driver;
    private readonly ITestOutputHelper _testOutput;

    // Elementos de la página de mesas
    private By MesasTitle => By.Id("MesasTitle");
    private By MesaDisponible(string mesaId) => By.XPath($"//android.widget.Button[@resource-id='{mesaId}' and @text='Disponible']");
    private By MesaOcupada(string mesaId) => By.XPath($"//android.widget.Button[@resource-id='{mesaId}' and @text='Ocupada']");
    private By MesaReservada(string mesaId) => By.XPath($"//android.widget.Button[@resource-id='{mesaId}' and @text='Reservada']");
    private By CrearComandaButton => By.Id("CrearComandaButton");
    private By FiltrarMesasButton => By.Id("FiltrarMesasButton");
    private By EstadoFiltro => By.Id("EstadoFiltro");
    private By UbicacionFiltro => By.Id("UbicacionFiltro");
    private By CapacidadFiltro => By.Id("CapacidadFiltro");
    private By RefreshButton => By.Id("RefreshButton");
    private By LoadingIndicator => By.Id("LoadingIndicator");

    public MesasPageObject(IWebDriver driver, ITestOutputHelper testOutput)
    {
        _driver = driver;
        _testOutput = testOutput;
    }

    public bool IsMesasPageDisplayed()
    {
        try
        {
            var mesasTitle = _driver.FindElement(MesasTitle);
            return mesasTitle.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public string GetMesasTitle()
    {
        try
        {
            var mesasTitle = _driver.FindElement(MesasTitle);
            return mesasTitle.Text;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al obtener título de mesas: {ex.Message}");
            return string.Empty;
        }
    }

    public bool IsMesaDisponible(string mesaId)
    {
        try
        {
            var mesa = _driver.FindElement(MesaDisponible(mesaId));
            return mesa.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public bool IsMesaOcupada(string mesaId)
    {
        try
        {
            var mesa = _driver.FindElement(MesaOcupada(mesaId));
            return mesa.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public bool IsMesaReservada(string mesaId)
    {
        try
        {
            var mesa = _driver.FindElement(MesaReservada(mesaId));
            return mesa.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public void ClickMesa(string mesaId)
    {
        try
        {
            // Intentar encontrar la mesa en cualquier estado
            var mesa = _driver.FindElement(By.Id(mesaId));
            mesa.Click();
            _testOutput.WriteLine($"Mesa {mesaId} clickeada");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al hacer click en mesa {mesaId}: {ex.Message}");
            throw;
        }
    }

    public void ClickCrearComanda()
    {
        try
        {
            var crearComandaButton = _driver.FindElement(CrearComandaButton);
            crearComandaButton.Click();
            _testOutput.WriteLine("Botón 'Crear Comanda' clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al crear comanda: {ex.Message}");
            throw;
        }
    }

    public void ClickFiltrarMesas()
    {
        try
        {
            var filtrarButton = _driver.FindElement(FiltrarMesasButton);
            filtrarButton.Click();
            _testOutput.WriteLine("Botón 'Filtrar Mesas' clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al filtrar mesas: {ex.Message}");
        }
    }

    public void SetEstadoFiltro(string estado)
    {
        try
        {
            var estadoFiltro = _driver.FindElement(EstadoFiltro);
            estadoFiltro.Click();
            
            // Seleccionar el estado específico
            var estadoOption = _driver.FindElement(By.XPath($"//android.widget.TextView[@text='{estado}']"));
            estadoOption.Click();
            
            _testOutput.WriteLine($"Filtro de estado establecido: {estado}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al establecer filtro de estado: {ex.Message}");
        }
    }

    public void SetUbicacionFiltro(string ubicacion)
    {
        try
        {
            var ubicacionFiltro = _driver.FindElement(UbicacionFiltro);
            ubicacionFiltro.Click();
            
            // Seleccionar la ubicación específica
            var ubicacionOption = _driver.FindElement(By.XPath($"//android.widget.TextView[@text='{ubicacion}']"));
            ubicacionOption.Click();
            
            _testOutput.WriteLine($"Filtro de ubicación establecido: {ubicacion}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al establecer filtro de ubicación: {ex.Message}");
        }
    }

    public void SetCapacidadFiltro(int capacidad)
    {
        try
        {
            var capacidadFiltro = _driver.FindElement(CapacidadFiltro);
            capacidadFiltro.Click();
            
            // Seleccionar la capacidad específica
            var capacidadOption = _driver.FindElement(By.XPath($"//android.widget.TextView[@text='{capacidad}']"));
            capacidadOption.Click();
            
            _testOutput.WriteLine($"Filtro de capacidad establecido: {capacidad}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al establecer filtro de capacidad: {ex.Message}");
        }
    }

    public void ClickRefresh()
    {
        try
        {
            var refreshButton = _driver.FindElement(RefreshButton);
            refreshButton.Click();
            _testOutput.WriteLine("Botón 'Refresh' clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al refrescar: {ex.Message}");
        }
    }

    public bool IsLoadingIndicatorDisplayed()
    {
        try
        {
            var loadingElement = _driver.FindElement(LoadingIndicator);
            return loadingElement.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public void WaitForMesasToLoad()
    {
        try
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => !IsLoadingIndicatorDisplayed());
            _testOutput.WriteLine("Mesas cargadas correctamente");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error esperando carga de mesas: {ex.Message}");
        }
    }

    public int GetNumberOfMesas()
    {
        try
        {
            var mesas = _driver.FindElements(By.XPath("//android.widget.Button[contains(@resource-id, 'Mesa')]"));
            return mesas.Count;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al contar mesas: {ex.Message}");
            return 0;
        }
    }

    public List<string> GetMesaIds()
    {
        try
        {
            var mesas = _driver.FindElements(By.XPath("//android.widget.Button[contains(@resource-id, 'Mesa')]"));
            return mesas.Select(m => m.GetAttribute("resource-id")).ToList();
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al obtener IDs de mesas: {ex.Message}");
            return new List<string>();
        }
    }

    public string GetMesaEstado(string mesaId)
    {
        try
        {
            var mesa = _driver.FindElement(By.Id(mesaId));
            return mesa.Text;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al obtener estado de mesa {mesaId}: {ex.Message}");
            return string.Empty;
        }
    }
} 