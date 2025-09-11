using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Services;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class RecetasApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly TokenStore _tokenStore;
    private readonly RecetasApiService _service;

    public RecetasApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStore = new TokenStore();
        _tokenStore.Token = "test-token";

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.restaurantepro.com/")
        };

        _httpClientFactoryMock
            .Setup(x => x.CreateClient("Api"))
            .Returns(httpClient);

        _service = new RecetasApiService(_httpClientFactoryMock.Object, _tokenStore);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerRecetasPaginadasAsync_ConParametrosValidos_DeberiaRetornarRecetas()
    {
        // Arrange
        var recetas = new List<RecetaDto>
        {
            new() { Id = Guid.NewGuid(), NombreProducto = "Pizza Margherita", TiempoPreparacionMinutos = 30, EstaActiva = true },
            new() { Id = Guid.NewGuid(), NombreProducto = "Pasta Carbonara", TiempoPreparacionMinutos = 25, EstaActiva = true }
        };

        var paginatedList = new PaginatedList<RecetaDto>
        {
            Items = recetas,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 1
        };

        var apiResponse = new ApiResponse<PaginatedList<RecetaDto>>
        {
            Success = true,
            Data = paginatedList
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerRecetasPaginadasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().HaveCount(2);
        resultado.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task ObtenerRecetasPaginadasAsync_ConFiltros_DeberiaRetornarRecetasFiltradas()
    {
        // Arrange
        var recetas = new List<RecetaDto>
        {
            new() { Id = Guid.NewGuid(), NombreProducto = "Pizza Margherita", EstaActiva = true }
        };

        var paginatedList = new PaginatedList<RecetaDto>
        {
            Items = recetas,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 1
        };

        var apiResponse = new ApiResponse<PaginatedList<RecetaDto>>
        {
            Success = true,
            Data = paginatedList
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerRecetasPaginadasAsync(1, 20, true, Guid.NewGuid(), "pizza");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObtenerRecetasPaginadasAsync_ConErrorDeRed_DeberiaRetornarListaVacia()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerRecetasPaginadasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdValido_DeberiaRetornarReceta()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var receta = new RecetaDto
        {
            Id = recetaId,
            NombreProducto = "Pizza Margherita",
            Preparacion = "Mezclar ingredientes y hornear",
            TiempoPreparacionMinutos = 30,
            EstaActiva = true,
            CostoTotal = 15.50m
        };

        var apiResponse = new ApiResponse<RecetaDto>
        {
            Success = true,
            Data = receta
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPorIdAsync(recetaId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(recetaId);
        resultado.NombreProducto.Should().Be("Pizza Margherita");
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdInexistente_DeberiaRetornarNull()
    {
        // Arrange
        var recetaId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.ObtenerPorIdAsync(recetaId);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearRecetaAsync_ConDatosValidos_DeberiaRetornarRecetaCreada()
    {
        // Arrange
        var request = new CrearRecetaRequest
        {
            ProductoId = Guid.NewGuid(),
            Preparacion = "Mezclar ingredientes y hornear",
            TiempoPreparacionMinutos = 30,
            Ingredientes = new List<AgregarIngredienteDto>
            {
                new() { IngredienteId = Guid.NewGuid(), Cantidad = 2, EsOpcional = false }
            },
            EstaActiva = true
        };

        var recetaCreada = new RecetaDto
        {
            Id = Guid.NewGuid(),
            ProductoId = request.ProductoId,
            Preparacion = request.Preparacion,
            TiempoPreparacionMinutos = request.TiempoPreparacionMinutos,
            EstaActiva = request.EstaActiva,
            FechaCreacion = DateTime.UtcNow
        };

        var apiResponse = new ApiResponse<RecetaDto>
        {
            Success = true,
            Data = recetaCreada
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearRecetaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.ProductoId.Should().Be(request.ProductoId);
        resultado.Preparacion.Should().Be(request.Preparacion);
    }

    [Fact]
    public async Task CrearRecetaAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var request = new CrearRecetaRequest
        {
            ProductoId = Guid.NewGuid(),
            Preparacion = "Test",
            TiempoPreparacionMinutos = 30
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.CrearRecetaAsync(request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarRecetaAsync_ConDatosValidos_DeberiaRetornarRecetaActualizada()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var request = new ActualizarRecetaRequest
        {
            Preparacion = "Preparación actualizada",
            TiempoPreparacionMinutos = 35,
            Ingredientes = new List<AgregarIngredienteDto>
            {
                new() { IngredienteId = Guid.NewGuid(), Cantidad = 3, EsOpcional = false }
            },
            EstaActiva = true
        };

        var recetaActualizada = new RecetaDto
        {
            Id = recetaId,
            Preparacion = request.Preparacion,
            TiempoPreparacionMinutos = request.TiempoPreparacionMinutos,
            EstaActiva = request.EstaActiva,
            FechaModificacion = DateTime.UtcNow
        };

        var apiResponse = new ApiResponse<RecetaDto>
        {
            Success = true,
            Data = recetaActualizada
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarRecetaAsync(recetaId, request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(recetaId);
        resultado.Preparacion.Should().Be(request.Preparacion);
    }

    [Fact]
    public async Task ActualizarRecetaAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var request = new ActualizarRecetaRequest
        {
            Preparacion = "Test",
            TiempoPreparacionMinutos = 30
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ActualizarRecetaAsync(recetaId, request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task EliminarRecetaAsync_ConIdValido_DeberiaRetornarTrue()
    {
        // Arrange
        var recetaId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var resultado = await _service.EliminarRecetaAsync(recetaId);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task EliminarRecetaAsync_ConErrorDeRed_DeberiaRetornarFalse()
    {
        // Arrange
        var recetaId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.EliminarRecetaAsync(recetaId);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerRecetasPorProductoAsync_ConProductoIdValido_DeberiaRetornarRecetas()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var recetas = new List<RecetaDto>
        {
            new() { Id = Guid.NewGuid(), ProductoId = productoId, NombreProducto = "Pizza Margherita", EstaActiva = true },
            new() { Id = Guid.NewGuid(), ProductoId = productoId, NombreProducto = "Pizza Margherita Grande", EstaActiva = true }
        };

        var apiResponse = new ApiResponse<List<RecetaDto>>
        {
            Success = true,
            Data = recetas
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerRecetasPorProductoAsync(productoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.All(r => r.ProductoId == productoId).Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerRecetasPorProductoAsync_ConErrorDeRed_DeberiaRetornarListaVacia()
    {
        // Arrange
        var productoId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerRecetasPorProductoAsync(productoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task CalcularCostoRecetaAsync_ConIdValido_DeberiaRetornarCosto()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var costo = 15.50m;

        var apiResponse = new ApiResponse<decimal>
        {
            Success = true,
            Data = costo
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CalcularCostoRecetaAsync(recetaId);

        // Assert
        resultado.Should().Be(costo);
    }

    [Fact]
    public async Task CalcularCostoRecetaAsync_ConErrorDeRed_DeberiaRetornarCero()
    {
        // Arrange
        var recetaId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.CalcularCostoRecetaAsync(recetaId);

        // Assert
        resultado.Should().Be(0);
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConIngredientesDisponibles_DeberiaRetornarDisponibilidad()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var disponibilidad = new DisponibilidadRecetaDto
        {
            EstaDisponible = true,
            IngredientesFaltantes = new List<string>(),
            MaximaPorcionesDisponibles = 10
        };

        var apiResponse = new ApiResponse<DisponibilidadRecetaDto>
        {
            Success = true,
            Data = disponibilidad
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.VerificarDisponibilidadAsync(recetaId, 2);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.EstaDisponible.Should().BeTrue();
        resultado.MaximaPorcionesDisponibles.Should().Be(10);
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConIngredientesFaltantes_DeberiaRetornarDisponibilidad()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var disponibilidad = new DisponibilidadRecetaDto
        {
            EstaDisponible = false,
            IngredientesFaltantes = new List<string> { "Queso Mozzarella", "Tomate" },
            MaximaPorcionesDisponibles = 0
        };

        var apiResponse = new ApiResponse<DisponibilidadRecetaDto>
        {
            Success = true,
            Data = disponibilidad
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.VerificarDisponibilidadAsync(recetaId, 1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.EstaDisponible.Should().BeFalse();
        resultado.IngredientesFaltantes.Should().HaveCount(2);
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        var recetaId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.VerificarDisponibilidadAsync(recetaId, 1);

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public async Task ObtenerRecetasPaginadasAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var recetas = Enumerable.Range(1, 1000).Select(i => new RecetaDto
        {
            Id = Guid.NewGuid(),
            NombreProducto = $"Receta {i}",
            TiempoPreparacionMinutos = i % 60,
            EstaActiva = i % 2 == 0
        }).ToList();

        var paginatedList = new PaginatedList<RecetaDto>
        {
            Items = recetas.Take(20).ToList(),
            TotalCount = 1000,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 50
        };

        var apiResponse = new ApiResponse<PaginatedList<RecetaDto>>
        {
            Success = true,
            Data = paginatedList
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerRecetasPaginadasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.TotalCount.Should().Be(1000);
        resultado.Items.Should().HaveCount(20);
    }

    [Fact]
    public async Task ObtenerRecetasPaginadasAsync_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var recetas = new List<RecetaDto>
        {
            new() { Id = Guid.NewGuid(), NombreProducto = "Pizza Margherita 🍕", TiempoPreparacionMinutos = 30, EstaActiva = true },
            new() { Id = Guid.NewGuid(), NombreProducto = "Pasta Carbonara 🍝", TiempoPreparacionMinutos = 25, EstaActiva = true },
            new() { Id = Guid.NewGuid(), NombreProducto = "Sushi Roll 🍣", TiempoPreparacionMinutos = 45, EstaActiva = true }
        };

        var paginatedList = new PaginatedList<RecetaDto>
        {
            Items = recetas,
            TotalCount = 3,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 1
        };

        var apiResponse = new ApiResponse<PaginatedList<RecetaDto>>
        {
            Success = true,
            Data = paginatedList
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerRecetasPaginadasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().HaveCount(3);
        resultado.Items.First().NombreProducto.Should().Contain("🍕");
    }

    [Fact]
    public async Task CrearRecetaAsync_ConValoresExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearRecetaRequest
        {
            ProductoId = Guid.NewGuid(),
            Preparacion = new string('A', 10000), // Preparación muy larga
            TiempoPreparacionMinutos = int.MaxValue,
            Ingredientes = Enumerable.Range(1, 100).Select(i => new AgregarIngredienteDto
            {
                IngredienteId = Guid.NewGuid(),
                Cantidad = decimal.MaxValue,
                EsOpcional = i % 2 == 0
            }).ToList(),
            EstaActiva = true
        };

        var recetaCreada = new RecetaDto
        {
            Id = Guid.NewGuid(),
            ProductoId = request.ProductoId,
            Preparacion = request.Preparacion,
            TiempoPreparacionMinutos = request.TiempoPreparacionMinutos,
            EstaActiva = request.EstaActiva,
            FechaCreacion = DateTime.UtcNow
        };

        var apiResponse = new ApiResponse<RecetaDto>
        {
            Success = true,
            Data = recetaCreada
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearRecetaAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Preparacion.Should().HaveLength(10000);
        resultado.TiempoPreparacionMinutos.Should().Be(int.MaxValue);
    }

    // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====

    [Fact]
    public async Task ObtenerRecetasPaginadasAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        var filtroMalicioso = "'; DROP TABLE Recetas; --";
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ObtenerRecetasPaginadasAsync(1, 20, true, Guid.NewGuid(), filtroMalicioso);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearRecetaAsync_ConXSS_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearRecetaRequest
        {
            ProductoId = Guid.NewGuid(),
            Preparacion = "<script>alert('xss')</script>",
            TiempoPreparacionMinutos = 30,
            Ingredientes = new List<AgregarIngredienteDto>(),
            EstaActiva = true
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.CrearRecetaAsync(request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarRecetaAsync_ConPayloadsMaliciosos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var request = new ActualizarRecetaRequest
        {
            Preparacion = "javascript:alert('xss')",
            TiempoPreparacionMinutos = -1,
            Ingredientes = new List<AgregarIngredienteDto>
            {
                new() { IngredienteId = Guid.Empty, Cantidad = -100, EsOpcional = false }
            },
            EstaActiva = true
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ActualizarRecetaAsync(recetaId, request);

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

    [Fact]
    public async Task ObtenerRecetasPaginadasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var recetas = new List<RecetaDto>
        {
            new() { Id = Guid.NewGuid(), NombreProducto = "Receta 1", EstaActiva = true },
            new() { Id = Guid.NewGuid(), NombreProducto = "Receta 2", EstaActiva = true }
        };

        var paginatedList = new PaginatedList<RecetaDto>
        {
            Items = recetas,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 20,
            TotalPages = 1
        };

        var apiResponse = new ApiResponse<PaginatedList<RecetaDto>>
        {
            Success = true,
            Data = paginatedList
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tareas = Enumerable.Range(1, 10).Select(_ => _service.ObtenerRecetasPaginadasAsync()).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(10);
        foreach (var resultado in resultados)
        {
            resultado.Items.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task CrearRecetaAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearRecetaRequest
        {
            ProductoId = Guid.NewGuid(),
            Preparacion = "Receta concurrente",
            TiempoPreparacionMinutos = 30,
            Ingredientes = new List<AgregarIngredienteDto>(),
            EstaActiva = true
        };

        var recetaCreada = new RecetaDto
        {
            Id = Guid.NewGuid(),
            ProductoId = request.ProductoId,
            Preparacion = request.Preparacion,
            TiempoPreparacionMinutos = request.TiempoPreparacionMinutos,
            EstaActiva = request.EstaActiva,
            FechaCreacion = DateTime.UtcNow
        };

        var apiResponse = new ApiResponse<RecetaDto>
        {
            Success = true,
            Data = recetaCreada
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tareas = Enumerable.Range(1, 5).Select(_ => _service.CrearRecetaAsync(request)).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(5);
        foreach (var resultado in resultados)
        {
            resultado.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CalcularCostoRecetaAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var costo = 15.50m;

        var apiResponse = new ApiResponse<decimal>
        {
            Success = true,
            Data = costo
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tareas = Enumerable.Range(1, 8).Select(_ => _service.CalcularCostoRecetaAsync(recetaId)).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(8);
        foreach (var resultado in resultados)
        {
            resultado.Should().Be(costo);
        }
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var disponibilidad = new DisponibilidadRecetaDto
        {
            EstaDisponible = true,
            IngredientesFaltantes = new List<string>(),
            MaximaPorcionesDisponibles = 10
        };

        var apiResponse = new ApiResponse<DisponibilidadRecetaDto>
        {
            Success = true,
            Data = disponibilidad
        };

        var responseContent = JsonSerializer.Serialize(apiResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var tareas = Enumerable.Range(1, 6).Select(_ => _service.VerificarDisponibilidadAsync(recetaId, 2)).ToArray();
        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().HaveCount(6);
        foreach (var resultado in resultados)
        {
            resultado!.EstaDisponible.Should().BeTrue();
        }
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO Y LÍMITES =====

    [Fact]
    public async Task ObtenerRecetasPaginadasAsync_ConTimeout_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var resultado = await _service.ObtenerRecetasPaginadasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearRecetaAsync_ConError500_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearRecetaRequest
        {
            ProductoId = Guid.NewGuid(),
            Preparacion = "Test",
            TiempoPreparacionMinutos = 30
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.CrearRecetaAsync(request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarRecetaAsync_ConError503_DeberiaManejarCorrectamente()
    {
        // Arrange
        var recetaId = Guid.NewGuid();
        var request = new ActualizarRecetaRequest
        {
            Preparacion = "Test",
            TiempoPreparacionMinutos = 30
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        // Act
        var resultado = await _service.ActualizarRecetaAsync(recetaId, request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task EliminarRecetaAsync_ConError404_DeberiaRetornarFalse()
    {
        // Arrange
        var recetaId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.EliminarRecetaAsync(recetaId);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerRecetasPorProductoAsync_ConProductoInexistente_DeberiaRetornarListaVacia()
    {
        // Arrange
        var productoId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.ObtenerRecetasPorProductoAsync(productoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task CalcularCostoRecetaAsync_ConRecetaInexistente_DeberiaRetornarCero()
    {
        // Arrange
        var recetaId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.CalcularCostoRecetaAsync(recetaId);

        // Assert
        resultado.Should().Be(0);
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConRecetaInexistente_DeberiaRetornarNull()
    {
        // Arrange
        var recetaId = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.VerificarDisponibilidadAsync(recetaId, 1);

        // Assert
        resultado.Should().BeNull();
    }
}
