using OpenQA.Selenium.Appium.Android;
using RestaurantePro.Mobile.UITests.TestBase;

namespace RestaurantePro.Mobile.UITests.PageObjects;

public class ComandasPageObject
{
    private readonly IWebDriver _driver;
    private readonly ITestOutputHelper _testOutput;

    // Elementos de la página de comandas
    private By ComandasTitle => By.Id("ComandasTitle");
    private By AgregarProductoButton => By.Id("AgregarProductoButton");
    private By AgregarAlCarritoButton => By.Id("AgregarAlCarritoButton");
    private By ConfirmarComandaButton => By.Id("ConfirmarComandaButton");
    private By CancelarComandaButton => By.Id("CancelarComandaButton");
    private By SuccessMessage => By.Id("SuccessMessage");
    private By ErrorMessage => By.Id("ErrorMessage");
    private By LoadingIndicator => By.Id("LoadingIndicator");
    private By ProductoItem(string productoNombre) => By.XPath($"//android.widget.TextView[@text='{productoNombre}']");
    private By CantidadInput => By.Id("CantidadInput");
    private By ObservacionesInput => By.Id("ObservacionesInput");
    private By TotalComanda => By.Id("TotalComanda");
    private By NumeroProductos => By.Id("NumeroProductos");

    public ComandasPageObject(IWebDriver driver, ITestOutputHelper testOutput)
    {
        _driver = driver;
        _testOutput = testOutput;
    }

    public bool IsComandasPageDisplayed()
    {
        try
        {
            var comandasTitle = _driver.FindElement(ComandasTitle);
            return comandasTitle.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public string GetComandasTitle()
    {
        try
        {
            var comandasTitle = _driver.FindElement(ComandasTitle);
            return comandasTitle.Text;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al obtener título de comandas: {ex.Message}");
            return string.Empty;
        }
    }

    public void ClickAgregarProducto()
    {
        try
        {
            var agregarProductoButton = _driver.FindElement(AgregarProductoButton);
            agregarProductoButton.Click();
            _testOutput.WriteLine("Botón 'Agregar Producto' clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al agregar producto: {ex.Message}");
            throw;
        }
    }

    public void SelectProducto(string productoNombre)
    {
        try
        {
            var producto = _driver.FindElement(ProductoItem(productoNombre));
            producto.Click();
            _testOutput.WriteLine($"Producto seleccionado: {productoNombre}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al seleccionar producto {productoNombre}: {ex.Message}");
            throw;
        }
    }

    public void SetCantidad(int cantidad)
    {
        try
        {
            var cantidadInput = _driver.FindElement(CantidadInput);
            cantidadInput.Clear();
            cantidadInput.SendKeys(cantidad.ToString());
            _testOutput.WriteLine($"Cantidad establecida: {cantidad}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al establecer cantidad: {ex.Message}");
            throw;
        }
    }

    public void SetObservaciones(string observaciones)
    {
        try
        {
            var observacionesInput = _driver.FindElement(ObservacionesInput);
            observacionesInput.Clear();
            observacionesInput.SendKeys(observaciones);
            _testOutput.WriteLine($"Observaciones establecidas: {observaciones}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al establecer observaciones: {ex.Message}");
        }
    }

    public void ClickAgregarAlCarrito()
    {
        try
        {
            var agregarAlCarritoButton = _driver.FindElement(AgregarAlCarritoButton);
            agregarAlCarritoButton.Click();
            _testOutput.WriteLine("Botón 'Agregar al Carrito' clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al agregar al carrito: {ex.Message}");
            throw;
        }
    }

    public void ClickConfirmarComanda()
    {
        try
        {
            var confirmarComandaButton = _driver.FindElement(ConfirmarComandaButton);
            confirmarComandaButton.Click();
            _testOutput.WriteLine("Botón 'Confirmar Comanda' clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al confirmar comanda: {ex.Message}");
            throw;
        }
    }

    public void ClickCancelarComanda()
    {
        try
        {
            var cancelarComandaButton = _driver.FindElement(CancelarComandaButton);
            cancelarComandaButton.Click();
            _testOutput.WriteLine("Botón 'Cancelar Comanda' clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al cancelar comanda: {ex.Message}");
        }
    }

    public bool IsSuccessMessageDisplayed()
    {
        try
        {
            var successElement = _driver.FindElement(SuccessMessage);
            return successElement.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public string GetSuccessMessage()
    {
        try
        {
            var successElement = _driver.FindElement(SuccessMessage);
            return successElement.Text;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al obtener mensaje de éxito: {ex.Message}");
            return string.Empty;
        }
    }

    public bool IsErrorMessageDisplayed()
    {
        try
        {
            var errorElement = _driver.FindElement(ErrorMessage);
            return errorElement.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public string GetErrorMessage()
    {
        try
        {
            var errorElement = _driver.FindElement(ErrorMessage);
            return errorElement.Text;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al obtener mensaje de error: {ex.Message}");
            return string.Empty;
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

    public void WaitForComandaToComplete()
    {
        try
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(driver => !IsLoadingIndicatorDisplayed());
            _testOutput.WriteLine("Comanda completada");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error esperando completar comanda: {ex.Message}");
        }
    }

    public decimal GetTotalComanda()
    {
        try
        {
            var totalElement = _driver.FindElement(TotalComanda);
            var totalText = totalElement.Text.Replace("$", "").Replace(",", "");
            return decimal.Parse(totalText);
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al obtener total de comanda: {ex.Message}");
            return 0;
        }
    }

    public int GetNumeroProductos()
    {
        try
        {
            var numeroElement = _driver.FindElement(NumeroProductos);
            return int.Parse(numeroElement.Text);
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error al obtener número de productos: {ex.Message}");
            return 0;
        }
    }

    public void AddProductoToComanda(string productoNombre, int cantidad = 1, string observaciones = "")
    {
        ClickAgregarProducto();
        SelectProducto(productoNombre);
        
        if (cantidad > 1)
        {
            SetCantidad(cantidad);
        }
        
        if (!string.IsNullOrEmpty(observaciones))
        {
            SetObservaciones(observaciones);
        }
        
        ClickAgregarAlCarrito();
        _testOutput.WriteLine($"Producto agregado a comanda: {productoNombre} x{cantidad}");
    }

    public void CreateCompleteComanda(List<(string producto, int cantidad, string observaciones)> productos)
    {
        foreach (var (producto, cantidad, observaciones) in productos)
        {
            AddProductoToComanda(producto, cantidad, observaciones);
        }
        
        ClickConfirmarComanda();
        WaitForComandaToComplete();
        _testOutput.WriteLine("Comanda completa creada");
    }

    public bool IsProductoInComanda(string productoNombre)
    {
        try
        {
            var producto = _driver.FindElement(ProductoItem(productoNombre));
            return producto.Displayed;
        }
        catch
        {
            return false;
        }
    }

    public void WaitForComandasPageToLoad()
    {
        try
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => IsComandasPageDisplayed());
            _testOutput.WriteLine("Página de comandas cargada");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"Error esperando carga de página de comandas: {ex.Message}");
        }
    }
} 