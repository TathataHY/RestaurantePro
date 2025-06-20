namespace RestaurantePro.Api.IntegrationTests.Controllers.Core;

/// <summary>
/// Tests de integración para RecetasController
/// Valida todos los endpoints REST del controlador de recetas
/// </summary>
[Collection("Sequential")]
public class RecetasControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public RecetasControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetRecetas_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/core/recetas";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Endpoint no implementado aún");
        content.Should().Contain("Funcionalidad en desarrollo");
    }

    [Fact]
    public async Task GetRecetas_ConParametroSoloActivas_DebeRetornar501NotImplemented()
    {
        // Arrange
        var url = "/api/core/recetas?soloActivas=true";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Endpoint no implementado aún");
        content.Should().Contain("Funcionalidad en desarrollo");
    }

    [Fact]
    public async Task GetReceta_ConIdEspecifico_DebeRetornar501NotImplemented()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var url = $"/api/core/recetas/{recetaId}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Endpoint no implementado aún");
        content.Should().Contain("Funcionalidad en desarrollo");
    }

    [Fact]
    public async Task PostReceta_DebeRetornar501NotImplemented()
    {
        // Arrange
        var command = new
        {
            ProductoId = Guid.NewGuid(),
            Preparacion = "Mezclar todos los ingredientes y cocinar por 15 minutos",
            TiempoPreparacionMinutos = 15,
            Ingredientes = new[]
            {
                new { IngredienteId = Guid.NewGuid(), Cantidad = 100m, EsOpcional = false }
            }
        };
        var url = "/api/core/recetas";

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Endpoint no implementado aún");
        content.Should().Contain("Funcionalidad en desarrollo");
    }

    [Fact]
    public async Task PutReceta_DebeRetornar501NotImplemented()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var command = new
        {
            Id = recetaId,
            Preparacion = "Preparación actualizada",
            TiempoPreparacionMinutos = 20,
            Ingredientes = new[]
            {
                new { IngredienteId = Guid.NewGuid(), Cantidad = 150m, EsOpcional = false }
            }
        };
        var url = $"/api/core/recetas/{recetaId}";

        // Act
        var response = await HttpClient.PutAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Endpoint no implementado aún");
        content.Should().Contain("Funcionalidad en desarrollo");
    }

    [Fact]
    public async Task DeleteReceta_DebeRetornar501NotImplemented()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var url = $"/api/core/recetas/{recetaId}";

        // Act
        var response = await HttpClient.DeleteAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Endpoint no implementado aún");
        content.Should().Contain("Funcionalidad en desarrollo");
    }

    [Fact]
    public async Task GetRecetasPorProducto_DebeRetornar501NotImplemented()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var url = $"/api/core/recetas/producto/{productoId}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Endpoint no implementado aún");
        content.Should().Contain("Funcionalidad en desarrollo");
    }

    [Fact]
    public async Task CalcularCostoReceta_DebeRetornar501NotImplemented()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var url = $"/api/core/recetas/{recetaId}/costo";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Endpoint no implementado aún");
        content.Should().Contain("Funcionalidad en desarrollo");
    }

    [Fact]
    public async Task VerificarDisponibilidad_DebeRetornar501NotImplemented()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var url = $"/api/core/recetas/{recetaId}/disponibilidad?cantidad=2";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Endpoint no implementado aún");
        content.Should().Contain("Funcionalidad en desarrollo");
    }

    new public void Dispose()
    {
        _factory?.Dispose();
        base.Dispose();
    }
} 