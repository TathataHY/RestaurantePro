using OpenQA.Selenium;
using Xunit.Abstractions;

namespace RestaurantePro.Mobile.UITests.PageObjects;

/// <summary>
/// Page Object para la página de productos
/// </summary>
public class ProductosPageObject
{
    private readonly IWebDriver _driver;
    private readonly ITestOutputHelper _testOutput;

    // Selectores XPath para elementos de la página de productos
    private readonly string _productosPageTitle = "//android.widget.TextView[@content-desc='Productos']";
    private readonly string _productosList = "//android.widget.RecyclerView[@content-desc='ProductosList']";
    private readonly string _productoItem = "//android.widget.FrameLayout[@content-desc='ProductoItem']";
    private readonly string _searchBox = "//android.widget.EditText[@content-desc='SearchBox']";
    private readonly string _addProductButton = "//android.widget.Button[@content-desc='AddProductButton']";
    private readonly string _productName = "//android.widget.TextView[@content-desc='ProductName']";
    private readonly string _productPrice = "//android.widget.TextView[@content-desc='ProductPrice']";
    private readonly string _productCategory = "//android.widget.TextView[@content-desc='ProductCategory']";

    public ProductosPageObject(IWebDriver driver, ITestOutputHelper testOutput)
    {
        _driver = driver;
        _testOutput = testOutput;
    }

    /// <summary>
    /// Verifica si la página de productos está visible
    /// </summary>
    public bool IsProductosPageDisplayed()
    {
        try
        {
            _testOutput.WriteLine("🔍 Verificando si la página de productos está visible...");
            
            var titleElement = _driver.FindElement(By.XPath(_productosPageTitle));
            var isDisplayed = titleElement.Displayed;
            
            _testOutput.WriteLine($"✅ Página de productos visible: {isDisplayed}");
            return isDisplayed;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error verificando página de productos: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Espera a que la página de productos cargue completamente
    /// </summary>
    public void WaitForProductosToLoad()
    {
        try
        {
            _testOutput.WriteLine("⏳ Esperando a que la página de productos cargue...");
            
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => driver.FindElement(By.XPath(_productosPageTitle)).Displayed);
            
            _testOutput.WriteLine("✅ Página de productos cargada correctamente");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error esperando carga de productos: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Obtiene el número de productos mostrados en la lista
    /// </summary>
    public int GetProductosCount()
    {
        try
        {
            _testOutput.WriteLine("🔢 Contando productos en la lista...");
            
            var productos = _driver.FindElements(By.XPath(_productoItem));
            var count = productos.Count;
            
            _testOutput.WriteLine($"✅ Número de productos encontrados: {count}");
            return count;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error contando productos: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Busca un producto específico
    /// </summary>
    public void SearchProduct(string searchTerm)
    {
        try
        {
            _testOutput.WriteLine($"🔍 Buscando producto: {searchTerm}");
            
            var searchBox = _driver.FindElement(By.XPath(_searchBox));
            searchBox.Clear();
            searchBox.SendKeys(searchTerm);
            
            // Simular presionar Enter
            searchBox.SendKeys(Keys.Enter);
            
            // Esperar un momento para que se procese la búsqueda
            Thread.Sleep(1000);
            
            _testOutput.WriteLine($"✅ Búsqueda completada para: {searchTerm}");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error buscando producto: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Obtiene el nombre del primer producto en la lista
    /// </summary>
    public string GetFirstProductName()
    {
        try
        {
            _testOutput.WriteLine("📝 Obteniendo nombre del primer producto...");
            
            var productNameElement = _driver.FindElement(By.XPath(_productName));
            var productName = productNameElement.Text;
            
            _testOutput.WriteLine($"✅ Nombre del primer producto: {productName}");
            return productName;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error obteniendo nombre del producto: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Obtiene el precio del primer producto en la lista
    /// </summary>
    public string GetFirstProductPrice()
    {
        try
        {
            _testOutput.WriteLine("💰 Obteniendo precio del primer producto...");
            
            var productPriceElement = _driver.FindElement(By.XPath(_productPrice));
            var productPrice = productPriceElement.Text;
            
            _testOutput.WriteLine($"✅ Precio del primer producto: {productPrice}");
            return productPrice;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error obteniendo precio del producto: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Obtiene la categoría del primer producto en la lista
    /// </summary>
    public string GetFirstProductCategory()
    {
        try
        {
            _testOutput.WriteLine("🏷️ Obteniendo categoría del primer producto...");
            
            var productCategoryElement = _driver.FindElement(By.XPath(_productCategory));
            var productCategory = productCategoryElement.Text;
            
            _testOutput.WriteLine($"✅ Categoría del primer producto: {productCategory}");
            return productCategory;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error obteniendo categoría del producto: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Hace clic en el botón de agregar producto
    /// </summary>
    public void ClickAddProductButton()
    {
        try
        {
            _testOutput.WriteLine("➕ Haciendo clic en botón agregar producto...");
            
            var addButton = _driver.FindElement(By.XPath(_addProductButton));
            addButton.Click();
            
            _testOutput.WriteLine("✅ Botón agregar producto clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error haciendo clic en botón agregar: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Hace clic en el primer producto de la lista
    /// </summary>
    public void ClickFirstProduct()
    {
        try
        {
            _testOutput.WriteLine("👆 Haciendo clic en el primer producto...");
            
            var firstProduct = _driver.FindElement(By.XPath(_productoItem));
            firstProduct.Click();
            
            _testOutput.WriteLine("✅ Primer producto clickeado");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error haciendo clic en producto: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Verifica si hay productos disponibles
    /// </summary>
    public bool HasProducts()
    {
        try
        {
            var count = GetProductosCount();
            var hasProducts = count > 0;
            
            _testOutput.WriteLine($"📊 Productos disponibles: {hasProducts} ({count} productos)");
            return hasProducts;
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error verificando productos: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Limpia la búsqueda
    /// </summary>
    public void ClearSearch()
    {
        try
        {
            _testOutput.WriteLine("🧹 Limpiando búsqueda...");
            
            var searchBox = _driver.FindElement(By.XPath(_searchBox));
            searchBox.Clear();
            
            // Simular presionar Enter para actualizar la lista
            searchBox.SendKeys(Keys.Enter);
            
            _testOutput.WriteLine("✅ Búsqueda limpiada");
        }
        catch (Exception ex)
        {
            _testOutput.WriteLine($"❌ Error limpiando búsqueda: {ex.Message}");
            throw;
        }
    }
} 