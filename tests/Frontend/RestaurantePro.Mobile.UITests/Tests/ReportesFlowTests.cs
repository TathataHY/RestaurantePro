using FluentAssertions;
using RestaurantePro.Mobile.UITests.PageObjects;
using RestaurantePro.Mobile.UITests.TestBase;

namespace RestaurantePro.Mobile.UITests.Tests;

/// <summary>
/// Pruebas completas para el flujo de reportes y estadísticas
/// </summary>
public class ReportesFlowTests : AppiumTestBase
{
    private LoginPageObject _loginPage = null!;
    private DashboardPageObject _dashboardPage = null!;
    private ReportesPageObject _reportesPage = null!;
    private EstadisticasPageObject _estadisticasPage = null!;

    public ReportesFlowTests(ITestOutputHelper testOutput) : base(testOutput) { }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Reportes_CompleteFlow_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _reportesPage = new ReportesPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Reportes
        _dashboardPage.NavigateToReportes();
        _reportesPage.WaitForReportesToLoad();

        // Verify reportes are loaded
        var numberOfReportes = _reportesPage.GetNumberOfReportes();
        numberOfReportes.Should().BeGreaterThanOrEqualTo(0);

        TestOutput.WriteLine($"✅ Reportes cargados: {numberOfReportes}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Reportes_GenerateSalesReport_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _reportesPage = new ReportesPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Reportes
        _dashboardPage.NavigateToReportes();
        _reportesPage.WaitForReportesToLoad();

        // Generate sales report
        _reportesPage.ClickGenerateSalesReport();
        _reportesPage.WaitForReportGeneration();

        // Verify report was generated
        var reportData = _reportesPage.GetSalesReportData();
        reportData.Should().NotBeEmpty();

        TestOutput.WriteLine($"✅ Reporte de ventas generado: {reportData}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Reportes_GenerateInventoryReport_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _reportesPage = new ReportesPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Reportes
        _dashboardPage.NavigateToReportes();
        _reportesPage.WaitForReportesToLoad();

        // Generate inventory report
        _reportesPage.ClickGenerateInventoryReport();
        _reportesPage.WaitForReportGeneration();

        // Verify report was generated
        var reportData = _reportesPage.GetInventoryReportData();
        reportData.Should().NotBeEmpty();

        TestOutput.WriteLine($"✅ Reporte de inventario generado: {reportData}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Reportes_FilterByDateRange_ShouldWorkCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _reportesPage = new ReportesPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Reportes
        _dashboardPage.NavigateToReportes();
        _reportesPage.WaitForReportesToLoad();

        // Test different date ranges
        var dateRanges = new[]
        {
            ("Hoy", DateTime.Today, DateTime.Today),
            ("Esta Semana", DateTime.Today.AddDays(-7), DateTime.Today),
            ("Este Mes", DateTime.Today.AddMonths(-1), DateTime.Today),
            ("Este Año", DateTime.Today.AddYears(-1), DateTime.Today)
        };
        
        foreach (var (rangeName, startDate, endDate) in dateRanges)
        {
            _reportesPage.FilterByDateRange(startDate, endDate);
            var filteredCount = _reportesPage.GetNumberOfReportes();
            
            TestOutput.WriteLine($"✅ Filtro por '{rangeName}': {filteredCount} reportes");
            
            // Verify filter is applied
            if (filteredCount > 0)
            {
                var firstReportDate = _reportesPage.GetFirstReportDate();
                firstReportDate.Should().BeOnOrAfter(startDate);
                firstReportDate.Should().BeOnOrBefore(endDate);
            }
        }
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Reportes_ExportReport_ShouldGenerateFile()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _reportesPage = new ReportesPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Reportes
        _dashboardPage.NavigateToReportes();
        _reportesPage.WaitForReportesToLoad();

        // Export report
        _reportesPage.ClickExportReport();
        _reportesPage.WaitForExportToComplete();

        // Verify export was successful
        var exportMessage = _reportesPage.GetExportMessage();
        exportMessage.Should().NotBeEmpty();
        exportMessage.Should().ContainAny("exportado", "generado", "descargado");

        TestOutput.WriteLine($"✅ Reporte exportado: {exportMessage}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Estadisticas_ViewDashboardStats_ShouldDisplayCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _estadisticasPage = new EstadisticasPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Estadisticas
        _dashboardPage.NavigateToEstadisticas();
        _estadisticasPage.WaitForEstadisticasToLoad();

        // Verify statistics are displayed
        var ventasHoy = _estadisticasPage.GetVentasHoy();
        var ventasSemana = _estadisticasPage.GetVentasSemana();
        var ventasMes = _estadisticasPage.GetVentasMes();

        ventasHoy.Should().BeGreaterThanOrEqualTo(0);
        ventasSemana.Should().BeGreaterThanOrEqualTo(0);
        ventasMes.Should().BeGreaterThanOrEqualTo(0);

        TestOutput.WriteLine($"✅ Estadísticas mostradas - Hoy: {ventasHoy}, Semana: {ventasSemana}, Mes: {ventasMes}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Estadisticas_ViewProductStats_ShouldDisplayCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _estadisticasPage = new EstadisticasPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Estadisticas
        _dashboardPage.NavigateToEstadisticas();
        _estadisticasPage.WaitForEstadisticasToLoad();

        // View product statistics
        _estadisticasPage.ClickProductStats();
        _estadisticasPage.WaitForProductStatsToLoad();

        // Verify product stats are displayed
        var topProducts = _estadisticasPage.GetTopProducts();
        topProducts.Should().NotBeEmpty();

        TestOutput.WriteLine($"✅ Estadísticas de productos mostradas: {topProducts}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Estadisticas_ViewCustomerStats_ShouldDisplayCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _estadisticasPage = new EstadisticasPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Estadisticas
        _dashboardPage.NavigateToEstadisticas();
        _estadisticasPage.WaitForEstadisticasToLoad();

        // View customer statistics
        _estadisticasPage.ClickCustomerStats();
        _estadisticasPage.WaitForCustomerStatsToLoad();

        // Verify customer stats are displayed
        var topCustomers = _estadisticasPage.GetTopCustomers();
        topCustomers.Should().NotBeEmpty();

        TestOutput.WriteLine($"✅ Estadísticas de clientes mostradas: {topCustomers}");
    }
}
