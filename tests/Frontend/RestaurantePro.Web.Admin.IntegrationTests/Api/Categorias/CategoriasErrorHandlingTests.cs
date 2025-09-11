using System.Net;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Categorias;

/// <summary>
/// Pruebas de manejo de errores para el controlador de Categorías
/// </summary>
public class CategoriasErrorHandlingTests : BaseIntegrationTest
{
    public CategoriasErrorHandlingTests(WebApplicationFactory factory) : base(factory)
    {
    }

    #region Errores de Parámetros Inválidos

    [Fact]
    public async Task ObtenerCategorias_ConParametrosInvalidos_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/core/categorias?soloActivas=invalid&ocultarVacias=invalid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Los parámetros inválidos se tratan como valores por defecto
    }

    [Fact]
    public async Task BuscarCategorias_ConParametrosInvalidos_DeberiaManejarCorrectamente()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/core/categorias/buscar?nombre=<script>alert('xss')</script>");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<object>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
    }

    #endregion

    #region Errores de ID Inválido

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("123")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task ObtenerCategoriaPorId_ConIdInvalido_DeberiaRetornarNotFound(string idInvalido)
    {
        // Arrange & Act
        var response = await _client.GetAsync($"/api/core/categorias/{idInvalido}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObtenerCategoriaPorId_ConIdVacio_DeberiaRetornarOK()
    {
        // Arrange & Act
        var response = await _client.GetAsync($"/api/core/categorias/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // ASP.NET Core redirige /api/core/categorias/ a /api/core/categorias
    }

    #endregion

    #region Errores de Búsqueda

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task BuscarCategorias_ConTerminoVacio_DeberiaRetornarTodasLasCategorias(string termino)
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={termino}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<object>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BuscarCategorias_ConTerminoNull_DeberiaRetornarTodasLasCategorias()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/categorias/buscar");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<object>>>(content, GetJsonOptions());
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
    }

    [Fact]
    public async Task BuscarCategorias_ConTerminoMuyLargo_DeberiaManejarCorrectamente()
    {
        // Arrange
        var terminoMuyLargo = new string('a', 1000);

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={terminoMuyLargo}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BuscarCategorias_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var terminoConCaracteresEspeciales = "categoría-con_especiales@#$%";

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={terminoConCaracteresEspeciales}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Errores de Concurrencia

    [Fact]
    public async Task ObtenerCategorias_ConRequestsSimultaneos_DeberiaManejarCorrectamente()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync("/api/core/categorias"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
            response.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task BuscarCategorias_ConRequestsSimultaneos_DeberiaManejarCorrectamente()
    {
        // Arrange
        await SeedCategoriasDePruebaAsync();

        // Act
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_client.GetAsync($"/api/core/categorias/buscar?nombre=test{i}"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(response => 
            response.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    #endregion

    #region Errores de Headers

    [Fact]
    public async Task ObtenerCategorias_ConHeadersMaliciosos_DeberiaIgnorar()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");
        _client.DefaultRequestHeaders.Add("X-Real-IP", "192.168.1.1");
        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; TestBot/1.0)");

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ObtenerCategorias_ConAcceptHeaderInvalido_DeberiaRetornarJSON()
    {
        // Arrange
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    #endregion

    #region Errores de Timeout

    [Fact]
    public async Task ObtenerCategorias_ConTimeoutCorto_DeberiaManejarCorrectamente()
    {
        // Arrange
        _client.Timeout = TimeSpan.FromMilliseconds(100);

        // Act
        var response = await _client.GetAsync("/api/core/categorias");

        // Assert
        // El timeout puede causar una excepción, pero el endpoint debería responder normalmente
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.RequestTimeout);
    }

    #endregion

    #region Errores de Encoding

    [Fact]
    public async Task BuscarCategorias_ConEncodingUTF8_DeberiaFuncionar()
    {
        // Arrange
        var terminoConAcentos = "categoría_español_ñáéíóú";

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={terminoConAcentos}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BuscarCategorias_ConEmojis_DeberiaFuncionar()
    {
        // Arrange
        var terminoConEmojis = "categoría🍽️🥘🍕";

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={terminoConEmojis}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Errores de Límites

    [Fact]
    public async Task ObtenerCategorias_ConMuchosParametros_DeberiaManejarCorrectamente()
    {
        // Arrange
        var parametros = string.Join("&", Enumerable.Range(1, 100).Select(i => $"param{i}=value{i}"));

        // Act
        var response = await _client.GetAsync($"/api/core/categorias?{parametros}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BuscarCategorias_ConQueryMuyLarga_DeberiaManejarCorrectamente()
    {
        // Arrange
        var queryLarga = new string('a', 10000);

        // Act
        var response = await _client.GetAsync($"/api/core/categorias/buscar?nombre={queryLarga}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Métodos de Ayuda

    private async Task SeedCategoriasDePruebaAsync(int cantidad = 5)
    {
        await CrearCategoriasDePruebaAsync();
    }

    #endregion
}
