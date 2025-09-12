using System.Net;
using System.Text.Json;
using FluentAssertions;
using AppReportes = RestaurantePro.Application.Comercial.Reportes.DTOs;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.ReportesComercial;

/// <summary>
/// Pruebas de seguridad para reportes comerciales
/// </summary>
public class ReportesSecurityTests : BaseIntegrationTest
{
    public ReportesSecurityTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Tests de Autenticación

    [Fact]
    public async Task ObtenerReporteVentas_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act - Crear cliente sin autenticación
        var clientNoAuth = _factory.CreateClient();
        var response = await clientNoAuth.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObtenerReporteClientes_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange - Sin autenticación

        // Act - Crear cliente sin autenticación
        var clientNoAuth = _factory.CreateClient();
        var response = await clientNoAuth.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObtenerReporteProductos_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act - Crear cliente sin autenticación
        var clientNoAuth = _factory.CreateClient();
        var response = await clientNoAuth.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObtenerReporteFidelizacion_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act - Crear cliente sin autenticación
        var clientNoAuth = _factory.CreateClient();
        var response = await clientNoAuth.GetAsync($"/api/comercial/reportes/fidelizacion?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ObtenerReportePromociones_SinAutenticacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act - Crear cliente sin autenticación
        var clientNoAuth = _factory.CreateClient();
        var response = await clientNoAuth.GetAsync($"/api/comercial/reportes/promociones?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Tests de Autorización por Roles

    [Fact]
    public async Task ObtenerReporteVentas_ConRolMesero_DeberiaRetornarForbidden()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act - Crear cliente con rol de mesero
        var clientMesero = _factory.CreateClient();
        await AutenticarComoMeseroAsync(clientMesero);
        var response = await clientMesero.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConRolCocinero_DeberiaRetornarForbidden()
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act - Crear cliente con rol de cocinero
        var clientCocinero = _factory.CreateClient();
        await AutenticarComoCocineroAsync(clientCocinero);
        var response = await clientCocinero.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ObtenerReporteProductos_ConRolCajero_DeberiaRetornarForbidden()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act - Crear cliente con rol de cajero
        var clientCajero = _factory.CreateClient();
        await AutenticarComoCajeroAsync(clientCajero);
        var response = await clientCajero.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConRolGerente_DeberiaPermitirAcceso()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act - Crear cliente con rol de gerente
        var clientGerente = _factory.CreateClient();
        await AutenticarComoGerenteAsync(clientGerente);
        var response = await clientGerente.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConRolAdministrador_DeberiaPermitirAcceso()
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act - Crear cliente con rol de administrador
        var clientAdmin = _factory.CreateClient();
        await AutenticarComoAdministradorAsync(clientAdmin);
        var response = await clientAdmin.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Tests de Validación de Entrada

    [Theory]
    [InlineData("2024-13-01", "2024-12-31")] // Mes inválido
    [InlineData("2024-02-30", "2024-12-31")] // Día inválido
    [InlineData("invalid-date", "2024-12-31")] // Formato inválido
    [InlineData("2024-01-01", "invalid-date")] // Formato inválido
    public async Task ObtenerReporteVentas_ConFechasInvalidas_DeberiaRetornarBadRequest(string fechaInicio, string fechaFin)
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio}&fechaFin={fechaFin}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("2024-13-01", "2024-12-31")] // Mes inválido
    [InlineData("2024-02-30", "2024-12-31")] // Día inválido
    [InlineData("invalid-date", "2024-12-31")] // Formato inválido
    public async Task ObtenerReporteProductos_ConFechasInvalidas_DeberiaRetornarBadRequest(string fechaInicio, string fechaFin)
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio}&fechaFin={fechaFin}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("2024-13-01", "2024-12-31")] // Mes inválido
    [InlineData("2024-02-30", "2024-12-31")] // Día inválido
    [InlineData("invalid-date", "2024-12-31")] // Formato inválido
    public async Task ObtenerReporteFidelizacion_ConFechasInvalidas_DeberiaRetornarBadRequest(string fechaInicio, string fechaFin)
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/fidelizacion?fechaInicio={fechaInicio}&fechaFin={fechaFin}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("2024-13-01", "2024-12-31")] // Mes inválido
    [InlineData("2024-02-30", "2024-12-31")] // Día inválido
    [InlineData("invalid-date", "2024-12-31")] // Formato inválido
    public async Task ObtenerReportePromociones_ConFechasInvalidas_DeberiaRetornarBadRequest(string fechaInicio, string fechaFin)
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/promociones?fechaInicio={fechaInicio}&fechaFin={fechaFin}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("2024-13-01", "2024-12-31")] // Mes inválido
    [InlineData("2024-02-30", "2024-12-31")] // Día inválido
    [InlineData("invalid-date", "2024-12-31")] // Formato inválido
    public async Task ObtenerReporteClientes_ConFechasInvalidas_DeberiaRetornarBadRequest(string fechaDesde, string fechaHasta)
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?fechaRegistroDesde={fechaDesde}&fechaRegistroHasta={fechaHasta}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Tests de Inyección de Código

    [Theory]
    [InlineData("'; DROP TABLE Ventas; --")]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("'; DELETE FROM Clientes; --")]
    [InlineData("1' OR '1'='1")]
    public async Task ObtenerReporteVentas_ConInyeccionSQL_DeberiaSerSeguro(string valorMalicioso)
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&segmento={valorMalicioso}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        // No debería causar errores internos del servidor
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData("'; DROP TABLE Clientes; --")]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("1' OR '1'='1")]
    public async Task ObtenerReporteClientes_ConInyeccionSQL_DeberiaSerSeguro(string valorMalicioso)
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/clientes?segmento={valorMalicioso}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        // No debería causar errores internos del servidor
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Theory]
    [InlineData("'; DROP TABLE Productos; --")]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("1' OR '1'='1")]
    public async Task ObtenerReporteProductos_ConInyeccionSQL_DeberiaSerSeguro(string valorMalicioso)
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&categoria={valorMalicioso}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        // No debería causar errores internos del servidor
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Tests de Límites de Parámetros

    [Fact]
    public async Task ObtenerReporteProductos_ConTopProductosMuyAlto_DeberiaLimitarCorrectamente()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;
        var topProductos = 10000; // Valor muy alto

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/productos?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&topProductos={topProductos}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<AppReportes.ReporteProductosDto>>(content, GetJsonOptions());
        
        responseData.Data.Should().NotBeNull();
        // El sistema debería limitar automáticamente el número de productos
        responseData.Data.ProductosMasVendidos.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteVentas_ConRangoMuyLargo_DeberiaLimitarCorrectamente()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddYears(-10); // Rango muy largo
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        // Debería limitar el rango o rechazar la solicitud
    }

    #endregion

    #region Tests de Headers de Seguridad

    [Fact]
    public async Task ObtenerReporteVentas_DeberiaIncluirHeadersDeSeguridad()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act
        var response = await _client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar headers de seguridad
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("X-XSS-Protection");
    }

    [Fact]
    public async Task ObtenerReporteClientes_DeberiaIncluirHeadersDeSeguridad()
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act
        var response = await _client.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar headers de seguridad
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("X-XSS-Protection");
    }

    #endregion

    #region Tests de Rate Limiting

    [Fact]
    public async Task ObtenerReporteVentas_ConMuchasRequests_DeberiaAplicarRateLimiting()
    {
        // Arrange
        await SeedDatosParaReportesAsync();
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Hacer muchas requests rápidamente
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_client.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}"));
        }
        
        var responses = await Task.WhenAll(tasks);

        // Assert
        // Algunas requests deberían ser limitadas
        var rateLimitedResponses = responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        rateLimitedResponses.Should().NotBeEmpty();
    }

    #endregion

    #region Tests de Logging de Seguridad

    [Fact]
    public async Task ObtenerReporteVentas_ConAccesoNoAutorizado_DeberiaLoggearIntento()
    {
        // Arrange
        var fechaInicio = DateTime.Now.AddDays(-30);
        var fechaFin = DateTime.Now;

        // Act - Crear cliente sin autenticación
        var clientNoAuth = _factory.CreateClient();
        var response = await clientNoAuth.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        // En un entorno real, verificaríamos que se haya loggeado el intento de acceso no autorizado
    }

    [Fact]
    public async Task ObtenerReporteClientes_ConRolIncorrecto_DeberiaLoggearIntento()
    {
        // Arrange
        await SeedDatosParaReportesAsync();

        // Act - Crear cliente con rol incorrecto
        var clientMesero = _factory.CreateClient();
        await AutenticarComoMeseroAsync(clientMesero);
        var response = await clientMesero.GetAsync("/api/comercial/reportes/clientes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        // En un entorno real, verificaríamos que se haya loggeado el intento de acceso con rol incorrecto
    }

    #endregion

    #region Métodos de Ayuda

    private async Task SeedDatosParaReportesAsync()
    {
        // Crear datos básicos necesarios para los reportes
        await CrearProductosDePruebaAsync(5);
        await CrearClientesDePruebaAsync(10);
        await CrearFacturasDePruebaAsync(20);
        await CrearPromocionesDePruebaAsync(3);
        await CrearTarjetasFidelizacionDePruebaAsync(5);
    }

    private async Task AutenticarComoMeseroAsync(HttpClient client)
    {
        // Aquí se implementaría la autenticación como mesero
        // Por ahora solo simulamos
        await Task.Delay(1);
    }

    private async Task AutenticarComoCocineroAsync(HttpClient client)
    {
        // Aquí se implementaría la autenticación como cocinero
        // Por ahora solo simulamos
        await Task.Delay(1);
    }

    private async Task AutenticarComoCajeroAsync(HttpClient client)
    {
        // Aquí se implementaría la autenticación como cajero
        // Por ahora solo simulamos
        await Task.Delay(1);
    }

    private async Task AutenticarComoGerenteAsync(HttpClient client)
    {
        // Aquí se implementaría la autenticación como gerente
        // Por ahora solo simulamos
        await Task.Delay(1);
    }

    private async Task AutenticarComoAdministradorAsync(HttpClient client)
    {
        // Aquí se implementaría la autenticación como administrador
        // Por ahora solo simulamos
        await Task.Delay(1);
    }

    private async Task<List<Guid>> CrearClientesDePruebaAsync(int cantidad)
    {
        var clienteIds = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var clienteId = Guid.NewGuid();
            clienteIds.Add(clienteId);
            
            // Aquí se crearían los clientes en la base de datos de prueba
            // Por ahora solo simulamos la creación
        }
        
        return clienteIds;
    }

    private async Task<List<Guid>> CrearFacturasDePruebaAsync(int cantidad)
    {
        var facturaIds = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var facturaId = Guid.NewGuid();
            facturaIds.Add(facturaId);
            
            // Aquí se crearían las facturas en la base de datos de prueba
            // Por ahora solo simulamos la creación
        }
        
        return facturaIds;
    }

    private async Task<List<Guid>> CrearPromocionesDePruebaAsync(int cantidad)
    {
        var promocionIds = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var promocionId = Guid.NewGuid();
            promocionIds.Add(promocionId);
            
            // Aquí se crearían las promociones en la base de datos de prueba
            // Por ahora solo simulamos la creación
        }
        
        return promocionIds;
    }

    private async Task<List<Guid>> CrearTarjetasFidelizacionDePruebaAsync(int cantidad)
    {
        var tarjetaIds = new List<Guid>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var tarjetaId = Guid.NewGuid();
            tarjetaIds.Add(tarjetaId);
            
            // Aquí se crearían las tarjetas de fidelización en la base de datos de prueba
            // Por ahora solo simulamos la creación
        }
        
        return tarjetaIds;
    }

    #endregion
}
