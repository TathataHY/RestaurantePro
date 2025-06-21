using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RestaurantePro.Api.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Comercial;

/// <summary>
/// Tests de integración para PromocionesController
/// Valida todos los endpoints REST del controlador de gestión de promociones
/// </summary>
[Collection("Sequential")]
public class PromocionesControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public PromocionesControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task ObtenerPromociones_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/comercial/promociones";

        // Act
        var response = await HttpClient.GetAsync(url);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        apiResponse.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerPromocion_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = $"/api/comercial/promociones/{Guid.NewGuid()}";

        // Act
        var response = await HttpClient.GetAsync(url);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        apiResponse.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CrearPromocion_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/comercial/promociones";
        var command = new { Nombre = "Test" };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        apiResponse.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ActualizarPromocion_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = $"/api/comercial/promociones/{Guid.NewGuid()}";
        var command = new { Nombre = "Test" };

        // Act
        var response = await HttpClient.PutAsJsonAsync(url, command);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        apiResponse.Success.Should().BeFalse();
    }

    [Fact]
    public async Task EliminarPromocion_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = $"/api/comercial/promociones/{Guid.NewGuid()}";

        // Act
        var response = await HttpClient.DeleteAsync(url);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        apiResponse.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ActivarPromocion_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = $"/api/comercial/promociones/{Guid.NewGuid()}/activar";

        // Act
        var response = await HttpClient.PatchAsync(url, null);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        apiResponse.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PausarPromocion_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = $"/api/comercial/promociones/{Guid.NewGuid()}/pausar";

        // Act
        var response = await HttpClient.PatchAsync(url, null);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        apiResponse.Success.Should().BeFalse();
    }

    [Fact]
    public async Task AplicarPromocion_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/comercial/promociones/aplicar";
        var command = new { PromocionId = Guid.NewGuid() };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        apiResponse.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerPromocionesAplicables_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = $"/api/comercial/promociones/aplicables?clienteId={Guid.NewGuid()}&monto=100";

        // Act
        var response = await HttpClient.GetAsync(url);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        apiResponse.Success.Should().BeFalse();
    }

    [Fact]
    public async Task AsignarProductos_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = $"/api/comercial/promociones/{Guid.NewGuid()}/productos";
        var command = new List<Guid> { Guid.NewGuid() };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        apiResponse.Success.Should().BeFalse();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
} 