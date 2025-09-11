using System.Net;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Categorias;

/// <summary>
/// Pruebas de seguridad para el controlador de Categorías
/// </summary>
public class CategoriasSecurityTests : BaseIntegrationTest
{
    public CategoriasSecurityTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Pruebas de Inyección SQL

    [Theory]
    [InlineData("'; DROP TABLE Categorias; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("' UNION SELECT * FROM Usuarios --")]
    [InlineData("'; INSERT INTO Categorias VALUES ('hack', 'hack'); --")]
    [InlineData("' OR 1=1 --")]
    public async Task BuscarCategorias_ConSQLInjection_DeberiaEscaparCorrectamente(string sqlInjection)
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={Uri.EscapeDataString(sqlInjection)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<object>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        
        // Verificar que no se ejecutó código SQL malicioso
        content.Should().NotContain("error");
        content.Should().NotContain("exception");
    }

    [Theory]
    [InlineData("'; DROP TABLE Categorias; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("' UNION SELECT * FROM Usuarios --")]
    public async Task ObtenerCategoriaPorId_ConSQLInjection_DeberiaRechazarCorrectamente(string sqlInjection)
    {
        // Arrange & Act
        var response = await _client.GetAsync($"/api/core/categorias/{sqlInjection}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Pruebas de XSS (Cross-Site Scripting)

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("javascript:alert('xss')")]
    [InlineData("<img src=x onerror=alert('xss')>")]
    [InlineData("<svg onload=alert('xss')>")]
    [InlineData("';alert('xss');//")]
    public async Task BuscarCategorias_ConXSS_DeberiaEscaparCorrectamente(string xssPayload)
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={Uri.EscapeDataString(xssPayload)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Verificar que el contenido no contiene el payload XSS
        content.Should().NotContain("<script>");
        content.Should().NotContain("javascript:");
        content.Should().NotContain("onerror=");
        content.Should().NotContain("onload=");
        content.Should().NotContain("alert(");
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("javascript:alert('xss')")]
    [InlineData("<img src=x onerror=alert('xss')>")]
    public async Task ObtenerCategoriaPorId_ConXSS_DeberiaRechazarCorrectamente(string xssPayload)
    {
        // Arrange & Act
        var response = await _client.GetAsync($"/api/core/categorias/{xssPayload}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Pruebas de Headers Maliciosos

    [Fact]
    public async Task ObtenerCategorias_ConHeadersMaliciosos_DeberiaIgnorar()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");
        _client.DefaultRequestHeaders.Add("X-Real-IP", "192.168.1.1");
        _client.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
        _client.DefaultRequestHeaders.Add("X-Original-URL", "/admin");
        _client.DefaultRequestHeaders.Add("X-Rewrite-URL", "/admin");

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ObtenerCategorias_ConUserAgentMalicioso_DeberiaIgnorar()
    {
        // Arrange
        _client.DefaultRequestHeaders.UserAgent.Clear();
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; EvilBot/1.0; +http://evil.com/bot)");

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ObtenerCategorias_ConRefererMalicioso_DeberiaIgnorar()
    {
        // Arrange
        _client.DefaultRequestHeaders.Referrer = new Uri("http://evil.com/malicious-page");

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Pruebas de Límites de Request

    [Fact]
    public async Task ObtenerCategorias_ConQueryMuyLarga_DeberiaManejarCorrectamente()
    {
        // Arrange
        var queryLarga = string.Join("&", Enumerable.Range(1, 1000).Select(i => $"param{i}=value{i}"));

        // Act
        var response = await _client.GetAsync($"/api/core/categorias?{queryLarga}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BuscarCategorias_ConTerminoMuyLargo_DeberiaManejarCorrectamente()
    {
        // Arrange
        var terminoMuyLargo = new string('a', 10000);

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={terminoMuyLargo}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Pruebas de Rate Limiting

    [Fact]
    public async Task ObtenerCategorias_ConRequestsRapidos_DeberiaManejarCorrectamente()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_client.GetAsync("/api/core/categorias"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.TooManyRequests));
    }

    [Fact]
    public async Task BuscarCategorias_ConRequestsRapidos_DeberiaManejarCorrectamente()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_client.GetAsync($"/api/core/categorias/buscar?nombre=test{i}"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.TooManyRequests));
    }

    #endregion

    #region Pruebas de Encoding

    [Fact]
    public async Task BuscarCategorias_ConCaracteresUnicode_DeberiaFuncionar()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();
        var terminoUnicode = "categoría_español_ñáéíóú_中文_العربية_русский";

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={Uri.EscapeDataString(terminoUnicode)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BuscarCategorias_ConEmojis_DeberiaFuncionar()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();
        var terminoConEmojis = "categoría🍽️🥘🍕🍔🍟🥗";

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={Uri.EscapeDataString(terminoConEmojis)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Pruebas de Métodos HTTP

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task ObtenerCategorias_ConMetodosNoPermitidos_DeberiaRetornarMethodNotAllowed(string metodo)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(metodo), "/api/core/categorias");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task BuscarCategorias_ConMetodosNoPermitidos_DeberiaRetornarMethodNotAllowed(string metodo)
    {
        // Arrange
        var request = new HttpRequestMessage(new HttpMethod(metodo), "/api/core/categorias/buscar?nombre=test");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    #endregion

    #region Pruebas de Content-Type

    [Fact]
    public async Task ObtenerCategorias_ConContentTypeInvalido_DeberiaRetornarJSON()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/core/categorias");
        request.Content = new StringContent("invalid content", System.Text.Encoding.UTF8, "text/plain");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    #endregion

    #region Pruebas de Parámetros Maliciosos

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("..\\..\\windows\\system32\\drivers\\etc\\hosts")]
    [InlineData("file:///etc/passwd")]
    [InlineData("ftp://evil.com/malicious")]
    public async Task BuscarCategorias_ConPathTraversal_DeberiaEscaparCorrectamente(string pathTraversal)
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={Uri.EscapeDataString(pathTraversal)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("root:");
        content.Should().NotContain("127.0.0.1");
    }

    #endregion

    #region Métodos de Ayuda

    private async Task SeedCategoriasDePruebaAsync(int cantidad = 5)
    {
        await CrearCategoriasDePruebaAsync();
    }

    #endregion
}
