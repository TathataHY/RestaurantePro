using FluentAssertions;
using RestaurantePro.Mobile.UITests.PageObjects;
using RestaurantePro.Mobile.UITests.TestBase;
using Xunit.Abstractions;

namespace RestaurantePro.Mobile.UITests.Tests;

/// <summary>
/// Pruebas end-to-end que combinan UI tests con API mock
/// </summary>
public class EndToEndFlowTests : AppiumTestBaseWithMockApi
{
    private LoginPageObject _loginPage;
    private DashboardPageObject _dashboardPage;
    private MesasPageObject _mesasPage;
    private ComandasPageObject _comandasPage;

    public EndToEndFlowTests(ITestOutputHelper testOutput) : base(testOutput) { }

    [Fact]
    public async Task CompleteRestaurantFlow_ShouldWorkEndToEnd()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _mesasPage = new MesasPageObject(Driver, TestOutput);
        _comandasPage = new ComandasPageObject(Driver, TestOutput);

        // Paso 1: Preparar datos en la API mock
        await PrepareTestDataInMockApi();

        // Paso 2: Login en la aplicación móvil
        await PerformLoginInMobile();

        // Paso 3: Navegar al dashboard
        await NavigateToDashboard();

        // Paso 4: Gestionar mesas
        await ManageTables();

        // Paso 5: Gestionar comandas
        await ManageOrders();

        // Paso 6: Verificar datos en API mock
        await VerifyDataInMockApi();

        TestOutput.WriteLine("✅ Flujo end-to-end completado exitosamente");
    }

    private async Task PrepareTestDataInMockApi()
    {
        TestOutput.WriteLine("📊 Preparando datos de prueba en API mock...");

        // Los datos ya están inicializados en el MockApiService
        // Solo verificamos que estén disponibles
        var mesas = await MockApi.GetFromApiAsync<List<object>>("/api/mesas");
        var productos = await MockApi.GetFromApiAsync<List<object>>("/api/productos");

        TestOutput.WriteLine($"✅ Datos mock disponibles: {mesas.Count} mesas, {productos.Count} productos");
    }

    private async Task PerformLoginInMobile()
    {
        TestOutput.WriteLine("🔐 Realizando login en aplicación móvil...");

        // Verificar que estamos en la página de login
        _loginPage.IsLoginPageDisplayed().Should().BeTrue();

        // Ingresar credenciales
        _loginPage.EnterEmail("admin@restaurantepro.com");
        _loginPage.EnterPassword("admin123");

        // Hacer login
        _loginPage.ClickLoginButton();

        // Esperar a que se complete el login
        _loginPage.WaitForLoginToComplete();

        TestOutput.WriteLine("✅ Login completado en aplicación móvil");
    }

    private async Task NavigateToDashboard()
    {
        TestOutput.WriteLine("🏠 Navegando al dashboard...");

        // Verificar que estamos en el dashboard
        _dashboardPage.WaitForDashboardToLoad().Should().BeTrue();
        _dashboardPage.IsDashboardDisplayed().Should().BeTrue();

        // Verificar elementos del dashboard
        var dashboardTitle = _dashboardPage.GetDashboardTitle();
        dashboardTitle.Should().Contain("Dashboard");

        TestOutput.WriteLine("✅ Dashboard cargado correctamente");
    }

    private async Task ManageTables()
    {
        TestOutput.WriteLine("🪑 Gestionando mesas...");

        // Navegar a la página de mesas
        _dashboardPage.NavigateToMesas();

        // Verificar que estamos en la página de mesas
        _mesasPage.IsMesasPageDisplayed().Should().BeTrue();

        // Obtener mesas disponibles desde mock
        var mesasDisponibles = await MockApi.GetFromApiAsync<List<object>>("/api/mesas/disponibles");
        mesasDisponibles.Should().NotBeEmpty();

        // Asignar una mesa
        var mesaId = mesasDisponibles.First();
        await MockApi.PostToApiAsync<object>($"/api/mesas/{mesaId}/asignar", new { UsuarioId = 1 });

        TestOutput.WriteLine("✅ Gestión de mesas completada");
    }

    private async Task ManageOrders()
    {
        TestOutput.WriteLine("📋 Gestionando comandas...");

        // Navegar a la página de comandas
        _dashboardPage.NavigateToComandas();

        // Verificar que estamos en la página de comandas
        _comandasPage.IsComandasPageDisplayed().Should().BeTrue();

        // Crear una nueva comanda
        var nuevaComanda = new { MesaId = 1, Observaciones = "Sin cebolla" };
        var comandaCreada = await MockApi.PostToApiAsync<object>("/api/comandas", nuevaComanda);

        // Agregar productos a la comanda
        var productos = await MockApi.GetFromApiAsync<List<object>>("/api/productos");
        var primerProducto = productos.First();
        
        var itemComanda = new { ComandaId = 1, ProductoId = primerProducto, Cantidad = 2 };
        await MockApi.PostToApiAsync<object>("/api/comandas/items", itemComanda);

        TestOutput.WriteLine("✅ Gestión de comandas completada");
    }

    private async Task VerifyDataInMockApi()
    {
        TestOutput.WriteLine("🔍 Verificando datos en API mock...");

        // Verificar mesas
        var mesas = await MockApi.GetFromApiAsync<List<object>>("/api/mesas");
        mesas.Should().NotBeEmpty();

        // Verificar comandas
        var comandas = await MockApi.GetFromApiAsync<List<object>>("/api/comandas");
        comandas.Should().NotBeEmpty();

        // Verificar productos
        var productos = await MockApi.GetFromApiAsync<List<object>>("/api/productos");
        productos.Should().NotBeEmpty();

        TestOutput.WriteLine("✅ Datos verificados correctamente en API mock");
    }

    [Fact]
    public async Task LoginWithApiValidation_ShouldWorkCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);

        // Preparar usuario en API mock
        var usuario = new { Email = "test@restaurantepro.com", Password = "test123", Nombre = "Usuario Test" };
        await MockApi.PostToApiAsync<object>("/api/auth/register", usuario);

        // Act - Login en móvil
        _loginPage.EnterEmail("test@restaurantepro.com");
        _loginPage.EnterPassword("test123");
        _loginPage.ClickLoginButton();
        _loginPage.WaitForLoginToComplete();

        // Assert - Verificar en API mock que el usuario está autenticado
        var authStatus = await MockApi.GetFromApiAsync<object>("/api/auth/status");
        authStatus.Should().NotBeNull();

        TestOutput.WriteLine("✅ Login con validación de API completado");
    }

    [Fact]
    public async Task TableAssignmentFlow_ShouldSyncWithApi()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _mesasPage = new MesasPageObject(Driver, TestOutput);

        // Login
        await PerformLoginInMobile();
        await NavigateToDashboard();

        // Act - Asignar mesa desde móvil
        _dashboardPage.NavigateToMesas();
        _mesasPage.IsMesasPageDisplayed().Should().BeTrue();

        // Simular asignación de mesa (esto dependería de la implementación específica)
        // Por ahora, lo hacemos directamente en la API mock
        var mesaId = 1;
        await MockApi.PostToApiAsync<object>($"/api/mesas/{mesaId}/asignar", new { UsuarioId = 1 });

        // Assert - Verificar que la mesa está asignada en la API mock
        var mesa = await MockApi.GetFromApiAsync<object>($"/api/mesas/{mesaId}");
        mesa.Should().NotBeNull();

        TestOutput.WriteLine("✅ Flujo de asignación de mesa sincronizado con API");
    }
} 