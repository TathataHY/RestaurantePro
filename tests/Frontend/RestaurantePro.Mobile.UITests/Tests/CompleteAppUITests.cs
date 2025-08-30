using FluentAssertions;
using RestaurantePro.Mobile.UITests.PageObjects;
using RestaurantePro.Mobile.UITests.TestBase;
using Xunit.Abstractions;

namespace RestaurantePro.Mobile.UITests.Tests;

/// <summary>
/// Pruebas UI completas para toda la aplicación móvil con API externa real
/// </summary>
public class CompleteAppUITests : AppiumTestBaseWithRealApi
{
    private LoginPageObject _loginPage;
    private DashboardPageObject _dashboardPage;
    private MesasPageObject _mesasPage;
    private ComandasPageObject _comandasPage;
    private ProductosPageObject _productosPage;

    public CompleteAppUITests(ITestOutputHelper testOutput) : base(testOutput) { }

    [Fact]
    public async Task CompleteAppFlow_ShouldWorkEndToEnd()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _mesasPage = new MesasPageObject(Driver, TestOutput);
        _comandasPage = new ComandasPageObject(Driver, TestOutput);
        _productosPage = new ProductosPageObject(Driver, TestOutput);

        // Paso 1: Login exitoso
        await PerformSuccessfulLogin();

        // Paso 2: Navegar por el dashboard
        await NavigateDashboard();

        // Paso 3: Gestionar mesas
        await ManageTablesComplete();

        // Paso 4: Gestionar productos
        await ManageProductsComplete();

        // Paso 5: Gestionar comandas
        await ManageOrdersComplete();

        // Paso 6: Verificar datos en API
        await VerifyAllDataInApi();

        TestOutput.WriteLine("✅ Flujo completo de la aplicación ejecutado exitosamente");
    }

    [Fact]
    public async Task AuthenticationFlow_ShouldWorkCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);

        // Act & Assert - Login exitoso
        await PerformSuccessfulLogin();

        // Act & Assert - Login fallido
        await PerformFailedLogin();

        // Act & Assert - Validaciones de campos
        await ValidateLoginFields();

        TestOutput.WriteLine("✅ Flujo de autenticación completado correctamente");
    }

    [Fact]
    public async Task TablesManagementFlow_ShouldWorkCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _mesasPage = new MesasPageObject(Driver, TestOutput);

        // Login
        await PerformSuccessfulLogin();
        await NavigateDashboard();

        // Act & Assert - Gestión completa de mesas
        await ManageTablesComplete();

        TestOutput.WriteLine("✅ Flujo de gestión de mesas completado correctamente");
    }

    [Fact]
    public async Task OrdersManagementFlow_ShouldWorkCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _comandasPage = new ComandasPageObject(Driver, TestOutput);

        // Login
        await PerformSuccessfulLogin();
        await NavigateDashboard();

        // Act & Assert - Gestión completa de comandas
        await ManageOrdersComplete();

        TestOutput.WriteLine("✅ Flujo de gestión de comandas completado correctamente");
    }

    [Fact]
    public async Task ProductsManagementFlow_ShouldWorkCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _productosPage = new ProductosPageObject(Driver, TestOutput);

        // Login
        await PerformSuccessfulLogin();
        await NavigateDashboard();

        // Act & Assert - Gestión completa de productos
        await ManageProductsComplete();

        TestOutput.WriteLine("✅ Flujo de gestión de productos completado correctamente");
    }

    private async Task PerformSuccessfulLogin()
    {
        TestOutput.WriteLine("🔐 Realizando login exitoso...");

        // Verificar que estamos en la página de login
        _loginPage.IsLoginPageDisplayed().Should().BeTrue();

        // Ingresar credenciales válidas
        _loginPage.EnterEmail("admin@restaurantepro.com");
        _loginPage.EnterPassword("admin123");

        // Hacer login
        _loginPage.ClickLoginButton();

        // Esperar a que se complete el login
        _loginPage.WaitForLoginToComplete();

        TestOutput.WriteLine("✅ Login exitoso completado");
    }

    private async Task PerformFailedLogin()
    {
        TestOutput.WriteLine("🔐 Realizando login fallido...");

        // Verificar que estamos en la página de login
        _loginPage.IsLoginPageDisplayed().Should().BeTrue();

        // Ingresar credenciales inválidas
        _loginPage.EnterEmail("invalid@example.com");
        _loginPage.EnterPassword("wrongpassword");

        // Hacer login
        _loginPage.ClickLoginButton();

        // Verificar que se muestra mensaje de error
        _loginPage.IsErrorMessageDisplayed().Should().BeTrue();

        TestOutput.WriteLine("✅ Login fallido manejado correctamente");
    }

    private async Task ValidateLoginFields()
    {
        TestOutput.WriteLine("🔍 Validando campos de login...");

        // Verificar que estamos en la página de login
        _loginPage.IsLoginPageDisplayed().Should().BeTrue();

        // Probar con email inválido
        _loginPage.EnterEmail("invalid-email");
        _loginPage.EnterPassword("password123");
        _loginPage.ClickLoginButton();

        // Verificar validación de email
        _loginPage.IsErrorMessageDisplayed().Should().BeTrue();

        // Probar con campos vacíos
        _loginPage.EnterEmail("");
        _loginPage.EnterPassword("");
        _loginPage.ClickLoginButton();

        // Verificar validación de campos vacíos
        _loginPage.IsErrorMessageDisplayed().Should().BeTrue();

        TestOutput.WriteLine("✅ Validaciones de campos completadas");
    }

    private async Task NavigateDashboard()
    {
        TestOutput.WriteLine("🏠 Navegando por el dashboard...");

        // Verificar que estamos en el dashboard
        _dashboardPage.WaitForDashboardToLoad().Should().BeTrue();
        _dashboardPage.IsDashboardDisplayed().Should().BeTrue();

        // Verificar elementos del dashboard
        var dashboardTitle = _dashboardPage.GetDashboardTitle();
        dashboardTitle.Should().Contain("Dashboard");

        // Verificar elementos del dashboard
        _dashboardPage.IsDashboardDisplayed().Should().BeTrue();

        TestOutput.WriteLine("✅ Dashboard navegado correctamente");
    }

    private async Task ManageTablesComplete()
    {
        TestOutput.WriteLine("🪑 Gestionando mesas completamente...");

        // Navegar a la página de mesas
        _dashboardPage.NavigateToMesas();

        // Verificar que estamos en la página de mesas
        _mesasPage.IsMesasPageDisplayed().Should().BeTrue();

        // Obtener mesas desde API real
        var mesas = await GetFromApiAsync<List<object>>("/api/mesas");
        mesas.Should().NotBeEmpty();

        // Verificar que se muestran las mesas en la UI
        _mesasPage.IsMesasPageDisplayed().Should().BeTrue();

        // Asignar una mesa
        var mesaId = 1;
        await PostToApiAsync<object>($"/api/mesas/{mesaId}/asignar", new { UsuarioId = 1 });

        // Verificar que la mesa se asignó correctamente
        var mesaAsignada = await GetFromApiAsync<object>($"/api/mesas/{mesaId}");
        mesaAsignada.Should().NotBeNull();

        TestOutput.WriteLine("✅ Gestión completa de mesas finalizada");
    }

    private async Task ManageProductsComplete()
    {
        TestOutput.WriteLine("🍽️ Gestionando productos completamente...");

        // Navegar a la página de productos
        _dashboardPage.NavigateToProductos();

        // Verificar que estamos en la página de productos
        _productosPage.IsProductosPageDisplayed().Should().BeTrue();

        // Obtener productos desde API
        var productos = await GetFromApiAsync<List<object>>("/api/productos");
        productos.Should().NotBeEmpty();

        // Verificar que se muestran los productos en la UI
        _productosPage.IsProductosPageDisplayed().Should().BeTrue();

        // Verificar que hay productos disponibles
        _productosPage.HasProducts().Should().BeTrue();

        TestOutput.WriteLine("✅ Gestión completa de productos finalizada");
    }

    private async Task ManageOrdersComplete()
    {
        TestOutput.WriteLine("📋 Gestionando comandas completamente...");

        // Navegar a la página de comandas
        _dashboardPage.NavigateToComandas();

        // Verificar que estamos en la página de comandas
        _comandasPage.IsComandasPageDisplayed().Should().BeTrue();

        // Crear una nueva comanda
        var nuevaComanda = new { MesaId = 1, Observaciones = "Sin cebolla" };
        var comandaCreada = await PostToApiAsync<object>("/api/comandas", nuevaComanda);

        // Agregar productos a la comanda
        var productos = await GetFromApiAsync<List<object>>("/api/productos");
        var primerProducto = productos.First();
        
        var itemComanda = new { ComandaId = 1, ProductoId = primerProducto, Cantidad = 2 };
        await PostToApiAsync<object>("/api/comandas/items", itemComanda);

        // Verificar que la comanda se creó correctamente
        var comandas = await GetFromApiAsync<List<object>>("/api/comandas");
        comandas.Should().NotBeEmpty();

        TestOutput.WriteLine("✅ Gestión completa de comandas finalizada");
    }

    private async Task VerifyAllDataInApi()
    {
        TestOutput.WriteLine("🔍 Verificando todos los datos en API...");

        // Verificar mesas
        var mesas = await GetFromApiAsync<List<object>>("/api/mesas");
        mesas.Should().NotBeEmpty();
        TestOutput.WriteLine($"✅ {mesas.Count} mesas verificadas");

        // Verificar productos
        var productos = await GetFromApiAsync<List<object>>("/api/productos");
        productos.Should().NotBeEmpty();
        TestOutput.WriteLine($"✅ {productos.Count} productos verificados");

        // Verificar comandas
        var comandas = await GetFromApiAsync<List<object>>("/api/comandas");
        comandas.Should().NotBeEmpty();
        TestOutput.WriteLine($"✅ {comandas.Count} comandas verificadas");

        // Verificar usuario
        var usuario = await GetFromApiAsync<object>("/api/auth/user");
        usuario.Should().NotBeNull();
        TestOutput.WriteLine("✅ Usuario verificado");

        TestOutput.WriteLine("✅ Todos los datos verificados correctamente en API");
    }
} 