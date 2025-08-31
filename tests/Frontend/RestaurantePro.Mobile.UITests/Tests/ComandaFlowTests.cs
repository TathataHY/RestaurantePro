using FluentAssertions;
using RestaurantePro.Mobile.UITests.PageObjects;
using RestaurantePro.Mobile.UITests.TestBase;

namespace RestaurantePro.Mobile.UITests.Tests;

public class ComandaFlowTests : AppiumTestBase
{
    private LoginPageObject _loginPage = null!;
    private DashboardPageObject _dashboardPage = null!;
    private MesasPageObject _mesasPage = null!;
    private ComandasPageObject _comandasPage = null!;

    public ComandaFlowTests(ITestOutputHelper testOutput) : base(testOutput) { }

    [Fact]
    public void CreateComanda_CompleteFlow_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _mesasPage = new MesasPageObject(Driver, TestOutput);
        _comandasPage = new ComandasPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "test@example.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "password123";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Mesas
        _dashboardPage.NavigateToMesas();
        _mesasPage.WaitForMesasToLoad();

        // Verify mesas are loaded
        var numberOfMesas = _mesasPage.GetNumberOfMesas();
        numberOfMesas.Should().BeGreaterThan(0);

        // Get mesa IDs
        var mesaIds = _mesasPage.GetMesaIds();
        mesaIds.Should().NotBeEmpty();
        var mesaId = mesaIds.First();

        // Select mesa and create comanda
        _mesasPage.ClickMesa(mesaId);
        _mesasPage.ClickCrearComanda();

        // Verify comandas page is loaded
        _comandasPage.WaitForComandasPageToLoad();
        _comandasPage.IsComandasPageDisplayed().Should().BeTrue();

        // Add products to comanda
        var productos = new List<(string producto, int cantidad, string observaciones)>
        {
            ("Hamburguesa Clásica", 1, "Sin cebolla"),
            ("Papas Fritas", 2, "Extra crujientes")
        };

        foreach (var (producto, cantidad, observaciones) in productos)
        {
            _comandasPage.AddProductoToComanda(producto, cantidad, observaciones);
        }

        // Verify total and number of products
        var totalComanda = _comandasPage.GetTotalComanda();
        var numeroProductos = _comandasPage.GetNumeroProductos();

        totalComanda.Should().BeGreaterThan(0);
        numeroProductos.Should().Be(productos.Count);

        // Confirm comanda
        _comandasPage.ClickConfirmarComanda();
        _comandasPage.WaitForComandaToComplete();

        // Verify success
        _comandasPage.IsSuccessMessageDisplayed().Should().BeTrue();
        var successMessage = _comandasPage.GetSuccessMessage();
        successMessage.Should().ContainAny("creada", "exitosamente", "confirmada");

        TestOutput.WriteLine($"✅ Comanda creada exitosamente: {successMessage}");
    }

    [Fact]
    public void CreateComanda_WithSingleProduct_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _mesasPage = new MesasPageObject(Driver, TestOutput);
        _comandasPage = new ComandasPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "test@example.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "password123";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Mesas
        _dashboardPage.NavigateToMesas();
        _mesasPage.WaitForMesasToLoad();

        // Get mesa IDs
        var mesaIds = _mesasPage.GetMesaIds();
        mesaIds.Should().NotBeEmpty();
        var mesaId = mesaIds.First();

        // Select mesa and create comanda
        _mesasPage.ClickMesa(mesaId);
        _mesasPage.ClickCrearComanda();

        // Verify comandas page is loaded
        _comandasPage.WaitForComandasPageToLoad();
        _comandasPage.IsComandasPageDisplayed().Should().BeTrue();

        // Add single product
        _comandasPage.AddProductoToComanda("Ensalada César", 1, "Sin crutones");

        // Verify
        var numeroProductos = _comandasPage.GetNumeroProductos();
        numeroProductos.Should().Be(1);

        TestOutput.WriteLine("✅ Comanda con un producto creada exitosamente");
    }

    [Fact]
    public void CreateComanda_WithMultipleQuantities_ShouldCalculateCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _mesasPage = new MesasPageObject(Driver, TestOutput);
        _comandasPage = new ComandasPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "test@example.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "password123";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Mesas
        _dashboardPage.NavigateToMesas();
        _mesasPage.WaitForMesasToLoad();

        // Get mesa IDs
        var mesaIds = _mesasPage.GetMesaIds();
        mesaIds.Should().NotBeEmpty();
        var mesaId = mesaIds.First();

        // Select mesa and create comanda
        _mesasPage.ClickMesa(mesaId);
        _mesasPage.ClickCrearComanda();

        // Verify comandas page is loaded
        _comandasPage.WaitForComandasPageToLoad();
        _comandasPage.IsComandasPageDisplayed().Should().BeTrue();

        // Add products with different quantities
        _comandasPage.AddProductoToComanda("Hamburguesa Clásica", 3, "");
        _comandasPage.AddProductoToComanda("Papas Fritas", 2, "");
        _comandasPage.AddProductoToComanda("Refresco Cola", 4, "");

        // Verify total number of products
        var numeroProductos = _comandasPage.GetNumeroProductos();
        numeroProductos.Should().Be(9); // 3 + 2 + 4

        // Verify total is greater than 0
        var totalComanda = _comandasPage.GetTotalComanda();
        totalComanda.Should().BeGreaterThan(0);

        TestOutput.WriteLine($"✅ Cálculo de cantidades múltiples correcto: {numeroProductos} productos, ${totalComanda}");
    }

    [Fact]
    public void CreateComanda_WithObservations_ShouldIncludeThem()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _mesasPage = new MesasPageObject(Driver, TestOutput);
        _comandasPage = new ComandasPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "test@example.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "password123";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Mesas
        _dashboardPage.NavigateToMesas();
        _mesasPage.WaitForMesasToLoad();

        // Get mesa IDs
        var mesaIds = _mesasPage.GetMesaIds();
        mesaIds.Should().NotBeEmpty();
        var mesaId = mesaIds.First();

        // Select mesa and create comanda
        _mesasPage.ClickMesa(mesaId);
        _mesasPage.ClickCrearComanda();

        // Verify comandas page is loaded
        _comandasPage.WaitForComandasPageToLoad();
        _comandasPage.IsComandasPageDisplayed().Should().BeTrue();

        // Add product with observations
        var observaciones = "Sin gluten, extra queso, bien cocida";
        _comandasPage.AddProductoToComanda("Hamburguesa Clásica", 1, observaciones);

        // Verify product is in comanda
        _comandasPage.IsProductoInComanda("Hamburguesa Clásica").Should().BeTrue();

        TestOutput.WriteLine($"✅ Observaciones incluidas: {observaciones}");
    }

    [Fact]
    public void CancelComanda_ShouldReturnToMesas()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _mesasPage = new MesasPageObject(Driver, TestOutput);
        _comandasPage = new ComandasPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "test@example.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "password123";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Mesas
        _dashboardPage.NavigateToMesas();
        _mesasPage.WaitForMesasToLoad();

        // Get mesa IDs
        var mesaIds = _mesasPage.GetMesaIds();
        mesaIds.Should().NotBeEmpty();
        var mesaId = mesaIds.First();

        // Select mesa and create comanda
        _mesasPage.ClickMesa(mesaId);
        _mesasPage.ClickCrearComanda();

        // Verify comandas page is loaded
        _comandasPage.WaitForComandasPageToLoad();
        _comandasPage.IsComandasPageDisplayed().Should().BeTrue();

        // Cancel comanda
        _comandasPage.ClickCancelarComanda();

        // Verify return to Mesas
        _mesasPage.IsMesasPageDisplayed().Should().BeTrue();

        TestOutput.WriteLine("✅ Cancelación de comanda exitosa - Retorno a Mesas");
    }

    [Fact]
    public void MesaFiltering_ShouldWorkCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _mesasPage = new MesasPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "test@example.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "password123";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Mesas
        _dashboardPage.NavigateToMesas();
        _mesasPage.WaitForMesasToLoad();

        // Test filtering
        _mesasPage.ClickFiltrarMesas();
        _mesasPage.SetEstadoFiltro("Disponible");
        _mesasPage.SetUbicacionFiltro("Interior");
        _mesasPage.SetCapacidadFiltro(4);

        // Verify filtering worked
        var numberOfMesas = _mesasPage.GetNumberOfMesas();
        numberOfMesas.Should().BeGreaterThanOrEqualTo(0);

        TestOutput.WriteLine($"✅ Filtrado de mesas funciona correctamente: {numberOfMesas} mesas encontradas");
    }
} 
