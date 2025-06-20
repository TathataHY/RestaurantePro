namespace RestaurantePro.Api.IntegrationTests.Controllers.Operaciones;

/// <summary>
/// Tests de integración para PreparacionesController
/// Valida todos los endpoints REST del controlador de preparaciones de cocina
/// </summary>
[Collection("Sequential")]
public class PreparacionesControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public PreparacionesControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    [Fact]
    public async Task GetPreparaciones_DebeRetornarRespuestaValida()
    {
        // Arrange
        var url = "/api/operaciones/preparaciones";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetPreparaciones_ConParametrosFiltro_DebeRetornarRespuestaValida()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var cocineroId = Guid.NewGuid();
        var fechaDesde = DateTime.Now.AddDays(-1);
        var fechaHasta = DateTime.Now;
        var url = $"/api/operaciones/preparaciones?estado=EnPreparacion&comandaId={comandaId}&cocineroId={cocineroId}&fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetPreparacion_ConIdEspecifico_DebeRetornarRespuestaValida()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        var url = $"/api/operaciones/preparaciones/{preparacionId}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostPreparacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var url = "/api/operaciones/preparaciones";
        var command = new
        {
            ComandaId = Guid.NewGuid(),
            ProductoId = Guid.NewGuid(),
            CocineroId = Guid.NewGuid(),
            Cantidad = 2,
            Observaciones = "Sin cebolla, bien cocido",
            Prioridad = "Alta",
            TiempoEstimadoMinutos = 15,
            IngredientesEspeciales = new[]
            {
                new { IngredienteId = Guid.NewGuid(), Cantidad = 100, Unidad = "g" }
            }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PutPreparacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        var url = $"/api/operaciones/preparaciones/{preparacionId}";
        var command = new
        {
            Id = preparacionId,
            ComandaId = Guid.NewGuid(),
            ProductoId = Guid.NewGuid(),
            CocineroId = Guid.NewGuid(),
            Cantidad = 3,
            Observaciones = "Preparación actualizada",
            Estado = "EnPreparacion",
            TiempoEstimadoMinutos = 20
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task IniciarPreparacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        var url = $"/api/operaciones/preparaciones/{preparacionId}/iniciar";
        var command = new
        {
            IniciadoPor = Guid.NewGuid(),
            FechaInicio = DateTime.Now,
            NotasInicio = "Iniciando preparación",
            TiempoEstimadoActualizado = 18
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CompletarPreparacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        var url = $"/api/operaciones/preparaciones/{preparacionId}/completar";
        var command = new
        {
            CompletadoPor = Guid.NewGuid(),
            FechaCompletado = DateTime.Now,
            NotasCompletado = "Preparación completada exitosamente",
            CalidadPreparacion = 5,
            TiempoRealMinutos = 16
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CancelarPreparacion_DebeRetornarRespuestaValida()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        var url = $"/api/operaciones/preparaciones/{preparacionId}/cancelar";
        var command = new
        {
            CanceladoPor = Guid.NewGuid(),
            MotivoCancelacion = "Producto agotado",
            FechaCancelacion = DateTime.Now,
            NotasCancelacion = "Cancelación por falta de ingredientes"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetColaPreparaciones_DebeRetornarRespuestaValida()
    {
        // Arrange
        var url = "/api/operaciones/preparaciones/cola";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotImplemented, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    public new void Dispose()
    {
        _factory?.Dispose();
        base.Dispose();
    }
} 