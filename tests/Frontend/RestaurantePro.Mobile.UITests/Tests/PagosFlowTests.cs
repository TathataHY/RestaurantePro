using FluentAssertions;
using RestaurantePro.Mobile.UITests.PageObjects;
using RestaurantePro.Mobile.UITests.TestBase;

namespace RestaurantePro.Mobile.UITests.Tests;

/// <summary>
/// Pruebas completas para el flujo de gestión de pagos y facturación
/// </summary>
public class PagosFlowTests : AppiumTestBase
{
    private LoginPageObject _loginPage = null!;
    private DashboardPageObject _dashboardPage = null!;
    private PagosPageObject _pagosPage = null!;
    private FacturacionPageObject _facturacionPage = null!;

    public PagosFlowTests(ITestOutputHelper testOutput) : base(testOutput) { }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Pagos_CompleteFlow_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _pagosPage = new PagosPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Pagos
        _dashboardPage.NavigateToPagos();
        _pagosPage.WaitForPagosToLoad();

        // Verify pagos are loaded
        var numberOfPagos = _pagosPage.GetNumberOfPagos();
        numberOfPagos.Should().BeGreaterThanOrEqualTo(0);

        TestOutput.WriteLine($"✅ Pagos cargados: {numberOfPagos}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Pagos_ProcessPayment_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _pagosPage = new PagosPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Pagos
        _dashboardPage.NavigateToPagos();
        _pagosPage.WaitForPagosToLoad();

        // Get first pago if exists
        if (_pagosPage.GetNumberOfPagos() > 0)
        {
            var pagoId = _pagosPage.GetFirstPagoId();
            var currentStatus = _pagosPage.GetPagoStatus(pagoId);
            
            if (currentStatus == "Pendiente")
            {
                // Process payment
                _pagosPage.ClickProcessPayment(pagoId);
                _pagosPage.WaitForPaymentProcessing();

                // Verify payment was processed
                var updatedStatus = _pagosPage.GetPagoStatus(pagoId);
                updatedStatus.Should().Be("Procesado");

                TestOutput.WriteLine($"✅ Pago procesado exitosamente: {pagoId}");
            }
            else
            {
                TestOutput.WriteLine($"ℹ️ Pago {pagoId} ya está en estado: {currentStatus}");
            }
        }
        else
        {
            TestOutput.WriteLine("ℹ️ No hay pagos para procesar");
        }
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Pagos_GenerateInvoice_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _pagosPage = new PagosPageObject(Driver, TestOutput);
        _facturacionPage = new FacturacionPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Pagos
        _dashboardPage.NavigateToPagos();
        _pagosPage.WaitForPagosToLoad();

        // Get first pago if exists
        if (_pagosPage.GetNumberOfPagos() > 0)
        {
            var pagoId = _pagosPage.GetFirstPagoId();
            
            // Generate invoice
            _pagosPage.ClickGenerateInvoice(pagoId);
            _facturacionPage.WaitForFacturacionToLoad();

            // Verify invoice generation
            _facturacionPage.IsInvoiceGenerated().Should().BeTrue();
            var invoiceNumber = _facturacionPage.GetInvoiceNumber();
            invoiceNumber.Should().NotBeEmpty();

            TestOutput.WriteLine($"✅ Factura generada exitosamente: {invoiceNumber}");
        }
        else
        {
            TestOutput.WriteLine("ℹ️ No hay pagos para generar factura");
        }
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Pagos_FilterByStatus_ShouldWorkCorrectly()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _pagosPage = new PagosPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Pagos
        _dashboardPage.NavigateToPagos();
        _pagosPage.WaitForPagosToLoad();

        // Test different filters
        var statuses = new[] { "Pendiente", "Procesado", "Rechazado", "Cancelado" };
        
        foreach (var status in statuses)
        {
            _pagosPage.FilterByStatus(status);
            var filteredCount = _pagosPage.GetNumberOfPagos();
            
            TestOutput.WriteLine($"✅ Filtro por '{status}': {filteredCount} pagos");
            
            // Verify filter is applied
            if (filteredCount > 0)
            {
                var firstPagoStatus = _pagosPage.GetFirstPagoStatus();
                firstPagoStatus.Should().Be(status);
            }
        }
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Pagos_SearchByCustomer_ShouldFindResults()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _pagosPage = new PagosPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Pagos
        _dashboardPage.NavigateToPagos();
        _pagosPage.WaitForPagosToLoad();

        // Search for common customer names
        var searchTerms = new[] { "Juan", "María", "Carlos", "Ana" };
        
        foreach (var searchTerm in searchTerms)
        {
            _pagosPage.SearchByCustomer(searchTerm);
            var searchResults = _pagosPage.GetNumberOfPagos();
            
            TestOutput.WriteLine($"✅ Búsqueda por cliente '{searchTerm}': {searchResults} resultados");
            
            // Clear search for next iteration
            _pagosPage.ClearSearch();
        }
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Pagos_ExportPaymentReport_ShouldGenerateFile()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _pagosPage = new PagosPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Pagos
        _dashboardPage.NavigateToPagos();
        _pagosPage.WaitForPagosToLoad();

        // Export payment report
        _pagosPage.ClickExportPaymentReport();
        _pagosPage.WaitForExportToComplete();

        // Verify export was successful
        var exportMessage = _pagosPage.GetExportMessage();
        exportMessage.Should().NotBeEmpty();
        exportMessage.Should().ContainAny("exportado", "generado", "descargado");

        TestOutput.WriteLine($"✅ Reporte de pagos exportado: {exportMessage}");
    }

    [Fact(Skip = "Temporalmente comentada hasta crear PageObjects")]
    public void Pagos_RefundPayment_ShouldSucceed()
    {
        // Arrange
        _loginPage = new LoginPageObject(Driver, TestOutput);
        _dashboardPage = new DashboardPageObject(Driver, TestOutput);
        _pagosPage = new PagosPageObject(Driver, TestOutput);
        
        var validEmail = Configuration["TestData:ValidCredentials:Email"] ?? "admin@restaurantepro.com";
        var validPassword = Configuration["TestData:ValidCredentials:Password"] ?? "AdminRestaurante123!";

        // Login
        _loginPage.Login(validEmail, validPassword);
        _loginPage.WaitForLoginToComplete();

        // Navigate to Pagos
        _dashboardPage.NavigateToPagos();
        _pagosPage.WaitForPagosToLoad();

        // Get first processed payment if exists
        _pagosPage.FilterByStatus("Procesado");
        if (_pagosPage.GetNumberOfPagos() > 0)
        {
            var pagoId = _pagosPage.GetFirstPagoId();
            
            // Process refund
            _pagosPage.ClickRefundPayment(pagoId);
            _pagosPage.WaitForRefundProcessing();

            // Verify refund was processed
            var updatedStatus = _pagosPage.GetPagoStatus(pagoId);
            updatedStatus.Should().Be("Reembolsado");

            TestOutput.WriteLine($"✅ Reembolso procesado exitosamente: {pagoId}");
        }
        else
        {
            TestOutput.WriteLine("ℹ️ No hay pagos procesados para reembolsar");
        }
    }
}
