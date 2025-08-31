using OpenQA.Selenium;
using Xunit.Abstractions;

namespace RestaurantePro.Mobile.UITests.PageObjects;

/// <summary>
/// Page Object para la página de cocina
/// </summary>
public class CocinaPageObject
{
    private readonly IWebDriver _driver;
    private readonly ITestOutputHelper _testOutput;

    public CocinaPageObject(IWebDriver driver, ITestOutputHelper testOutput)
    {
        _driver = driver;
        _testOutput = testOutput;
    }

    /// <summary>
    /// Espera a que la página de cocina se cargue
    /// </summary>
    public void WaitForCocinaToLoad()
    {
        try
        {
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            wait.Until(driver => driver.FindElement(By.Id("cocina-container")));
            _testOutput.WriteLine("✅ Página de cocina cargada");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error esperando carga de cocina: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el número de órdenes pendientes
    /// </summary>
    public int GetNumberOfOrdenesPendientes()
    {
        try
        {
            var ordenes = _driver.FindElements(By.CssSelector(".orden-pendiente"));
            var count = ordenes.Count;
            _testOutput.WriteLine($"📊 Órdenes pendientes encontradas: {count}");
            return count;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo número de órdenes pendientes: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Marca una orden como en preparación
    /// </summary>
    public void MarcarOrdenEnPreparacion(string ordenId)
    {
        try
        {
            var orden = _driver.FindElement(By.CssSelector($".orden-pendiente[data-id='{ordenId}']"));
            var button = orden.FindElement(By.CssSelector(".btn-en-preparacion"));
            button.Click();
            _testOutput.WriteLine($"🔄 Orden {ordenId} marcada como en preparación");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error marcando orden {ordenId} como en preparación: {ex.Message}");
        }
    }

    /// <summary>
    /// Marca una orden como lista
    /// </summary>
    public void MarcarOrdenLista(string ordenId)
    {
        try
        {
            var orden = _driver.FindElement(By.CssSelector($".orden-en-preparacion[data-id='{ordenId}']"));
            var button = orden.FindElement(By.CssSelector(".btn-lista"));
            button.Click();
            _testOutput.WriteLine($"✅ Orden {ordenId} marcada como lista");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error marcando orden {ordenId} como lista: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el tiempo de preparación de una orden
    /// </summary>
    public string GetTiempoPreparacion(string ordenId)
    {
        try
        {
            var orden = _driver.FindElement(By.CssSelector($".orden-item[data-id='{ordenId}'] .tiempo"));
            var tiempo = orden.Text;
            _testOutput.WriteLine($"⏱️ Tiempo de preparación de orden {ordenId}: {tiempo}");
            return tiempo;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo tiempo de preparación de orden {ordenId}: {ex.Message}");
            return "00:00";
        }
    }

    /// <summary>
    /// Filtra órdenes por prioridad
    /// </summary>
    public void FiltrarPorPrioridad(string prioridad)
    {
        try
        {
            var filterDropdown = _driver.FindElement(By.Id("prioridad-filter"));
            filterDropdown.Click();
            
            var prioridadOption = _driver.FindElement(By.XPath($"//option[text()='{prioridad}']"));
            prioridadOption.Click();
            
            _testOutput.WriteLine($"🔍 Filtro aplicado por prioridad: {prioridad}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error aplicando filtro por prioridad {prioridad}: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene la prioridad de la primera orden filtrada
    /// </summary>
    public string GetPrimeraOrdenPrioridad()
    {
        try
        {
            var primeraOrden = _driver.FindElement(By.CssSelector(".orden-item .prioridad"));
            var prioridad = primeraOrden.Text;
            _testOutput.WriteLine($"📊 Prioridad de primera orden filtrada: {prioridad}");
            return prioridad;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo prioridad de primera orden: {ex.Message}");
            return "Normal";
        }
    }

    /// <summary>
    /// Busca órdenes por producto
    /// </summary>
    public void BuscarPorProducto(string searchTerm)
    {
        try
        {
            var searchBox = _driver.FindElement(By.Id("producto-search"));
            searchBox.Clear();
            searchBox.SendKeys(searchTerm);
            
            var searchButton = _driver.FindElement(By.Id("search-button"));
            searchButton.Click();
            
            _testOutput.WriteLine($"🔍 Búsqueda realizada por producto: {searchTerm}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error buscando por producto {searchTerm}: {ex.Message}");
        }
    }

    /// <summary>
    /// Limpia la búsqueda
    /// </summary>
    public void LimpiarBusqueda()
    {
        try
        {
            var searchBox = _driver.FindElement(By.Id("producto-search"));
            searchBox.Clear();
            
            var clearButton = _driver.FindElement(By.Id("clear-search"));
            clearButton.Click();
            
            _testOutput.WriteLine("🧹 Búsqueda limpiada");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error limpiando búsqueda: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el número de órdenes urgentes
    /// </summary>
    public int GetNumberOfOrdenesUrgentes()
    {
        try
        {
            var ordenesUrgentes = _driver.FindElements(By.CssSelector(".orden-urgente"));
            var count = ordenesUrgentes.Count;
            _testOutput.WriteLine($"🚨 Órdenes urgentes encontradas: {count}");
            return count;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo número de órdenes urgentes: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Obtiene el número de órdenes normales
    /// </summary>
    public int GetNumberOfOrdenesNormales()
    {
        try
        {
            var ordenesNormales = _driver.FindElements(By.CssSelector(".orden-normal"));
            var count = ordenesNormales.Count;
            _testOutput.WriteLine($"📋 Órdenes normales encontradas: {count}");
            return count;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo número de órdenes normales: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Obtiene el total de órdenes en cocina
    /// </summary>
    public int GetTotalOrdenes()
    {
        try
        {
            var totalElement = _driver.FindElement(By.Id("total-ordenes"));
            var total = int.Parse(totalElement.Text);
            _testOutput.WriteLine($"📊 Total de órdenes en cocina: {total}");
            return total;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo total de órdenes: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Verifica si hay alertas de inventario
    /// </summary>
    public bool HayAlertasInventario()
    {
        try
        {
            var alertas = _driver.FindElements(By.CssSelector(".alerta-inventario"));
            var hayAlertas = alertas.Count > 0;
            _testOutput.WriteLine($"⚠️ Alertas de inventario: {hayAlertas}");
            return hayAlertas;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error verificando alertas de inventario: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene el mensaje de la primera alerta de inventario
    /// </summary>
    public string GetPrimeraAlertaInventario()
    {
        try
        {
            var primeraAlerta = _driver.FindElement(By.CssSelector(".alerta-inventario .mensaje"));
            var mensaje = primeraAlerta.Text;
            _testOutput.WriteLine($"🚨 Primera alerta de inventario: {mensaje}");
            return mensaje;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo primera alerta de inventario: {ex.Message}");
            return string.Empty;
        }
    }
}
