using OpenQA.Selenium;
using Xunit.Abstractions;

namespace RestaurantePro.Mobile.UITests.PageObjects;

/// <summary>
/// Page Object para la página de preparaciones en cocina
/// </summary>
public class PreparacionesPageObject
{
    private readonly IWebDriver _driver;
    private readonly ITestOutputHelper _testOutput;

    public PreparacionesPageObject(IWebDriver driver, ITestOutputHelper testOutput)
    {
        _driver = driver;
        _testOutput = testOutput;
    }

    /// <summary>
    /// Espera a que la página de preparaciones se cargue
    /// </summary>
    public void WaitForPreparacionesToLoad()
    {
        try
        {
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            wait.Until(driver => driver.FindElement(By.Id("preparaciones-container")));
            _testOutput.WriteLine("✅ Página de preparaciones cargada");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error esperando carga de preparaciones: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el número de preparaciones mostradas
    /// </summary>
    public int GetNumberOfPreparaciones()
    {
        try
        {
            var preparaciones = _driver.FindElements(By.CssSelector(".preparacion-item"));
            var count = preparaciones.Count;
            _testOutput.WriteLine($"📊 Preparaciones encontradas: {count}");
            return count;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo número de preparaciones: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Obtiene el ID de la primera preparación
    /// </summary>
    public string GetFirstPreparacionId()
    {
        try
        {
            var firstPreparacion = _driver.FindElement(By.CssSelector(".preparacion-item"));
            var id = firstPreparacion.GetAttribute("data-id");
            _testOutput.WriteLine($"🆔 ID de primera preparación: {id}");
            return id ?? "default";
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo ID de preparación: {ex.Message}");
            return "default";
        }
    }

    /// <summary>
    /// Hace clic en una preparación específica
    /// </summary>
    public void ClickPreparacion(string preparacionId)
    {
        try
        {
            var preparacion = _driver.FindElement(By.CssSelector($".preparacion-item[data-id='{preparacionId}']"));
            preparacion.Click();
            _testOutput.WriteLine($"👆 Preparación {preparacionId} seleccionada");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error haciendo clic en preparación {preparacionId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si los detalles de preparación están mostrados
    /// </summary>
    public bool IsPreparacionDetailsDisplayed()
    {
        try
        {
            var details = _driver.FindElement(By.Id("preparacion-details"));
            var isDisplayed = details.Displayed;
            _testOutput.WriteLine($"📋 Detalles de preparación mostrados: {isDisplayed}");
            return isDisplayed;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error verificando detalles de preparación: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene los detalles de la preparación
    /// </summary>
    public string GetPreparacionDetails()
    {
        try
        {
            var details = _driver.FindElement(By.Id("preparacion-details"));
            var text = details.Text;
            _testOutput.WriteLine($"📋 Detalles obtenidos: {text}");
            return text;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo detalles de preparación: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Obtiene el estado de una preparación
    /// </summary>
    public string GetPreparacionStatus(string preparacionId)
    {
        try
        {
            var preparacion = _driver.FindElement(By.CssSelector($".preparacion-item[data-id='{preparacionId}'] .status"));
            var status = preparacion.Text;
            _testOutput.WriteLine($"📊 Estado de preparación {preparacionId}: {status}");
            return status;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo estado de preparación {preparacionId}: {ex.Message}");
            return "Desconocido";
        }
    }

    /// <summary>
    /// Obtiene el siguiente estado en el flujo de preparación
    /// </summary>
    public string GetNextStatus(string currentStatus)
    {
        return currentStatus switch
        {
            "Pendiente" => "En Preparación",
            "En Preparación" => "Listo",
            "Listo" => "Entregado",
            _ => "Pendiente"
        };
    }

    /// <summary>
    /// Actualiza el estado de una preparación
    /// </summary>
    public void UpdatePreparacionStatus(string preparacionId, string newStatus)
    {
        try
        {
            var preparacion = _driver.FindElement(By.CssSelector($".preparacion-item[data-id='{preparacionId}']"));
            var statusButton = preparacion.FindElement(By.CssSelector(".status-button"));
            statusButton.Click();
            
            var newStatusOption = _driver.FindElement(By.XPath($"//option[text()='{newStatus}']"));
            newStatusOption.Click();
            
            _testOutput.WriteLine($"🔄 Estado de preparación {preparacionId} actualizado a: {newStatus}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error actualizando estado de preparación {preparacionId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Filtra preparaciones por estado
    /// </summary>
    public void FilterByStatus(string status)
    {
        try
        {
            var filterDropdown = _driver.FindElement(By.Id("status-filter"));
            filterDropdown.Click();
            
            var statusOption = _driver.FindElement(By.XPath($"//option[text()='{status}']"));
            statusOption.Click();
            
            _testOutput.WriteLine($"🔍 Filtro aplicado por estado: {status}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error aplicando filtro por estado {status}: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el estado de la primera preparación filtrada
    /// </summary>
    public string GetFirstPreparacionStatus()
    {
        try
        {
            var firstPreparacion = _driver.FindElement(By.CssSelector(".preparacion-item .status"));
            var status = firstPreparacion.Text;
            _testOutput.WriteLine($"📊 Estado de primera preparación filtrada: {status}");
            return status;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo estado de primera preparación: {ex.Message}");
            return "Desconocido";
        }
    }

    /// <summary>
    /// Busca preparaciones por producto
    /// </summary>
    public void SearchByProduct(string searchTerm)
    {
        try
        {
            var searchBox = _driver.FindElement(By.Id("product-search"));
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
    public void ClearSearch()
    {
        try
        {
            var searchBox = _driver.FindElement(By.Id("product-search"));
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
    /// Hace clic en exportar reporte
    /// </summary>
    public void ClickExportReport()
    {
        try
        {
            var exportButton = _driver.FindElement(By.Id("export-report"));
            exportButton.Click();
            _testOutput.WriteLine("📤 Botón de exportar reporte clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error haciendo clic en exportar reporte: {ex.Message}");
        }
    }

    /// <summary>
    /// Espera a que la exportación se complete
    /// </summary>
    public void WaitForExportToComplete()
    {
        try
        {
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(driver => driver.FindElement(By.Id("export-success")));
            _testOutput.WriteLine("✅ Exportación completada");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error esperando exportación: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el mensaje de exportación
    /// </summary>
    public string GetExportMessage()
    {
        try
        {
            var message = _driver.FindElement(By.Id("export-success"));
            var text = message.Text;
            _testOutput.WriteLine($"📤 Mensaje de exportación: {text}");
            return text;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"⚠️ Error obteniendo mensaje de exportación: {ex.Message}");
            return string.Empty;
        }
    }
}
