using System.Net;
using System.Text;
using System.Text.Json;
using Moq.Protected;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class CategoriasApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly TokenStore _tokenStore;
    private readonly CategoriasApiService _service;

    public CategoriasApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStore = new TokenStore();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api")).Returns(httpClient);

        _service = new CategoriasApiService(_httpClientFactoryMock.Object, _tokenStore);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerAsync_ConParametrosValidos_DeberiaRetornarCategorias()
    {
        // Arrange
        var categoriasEsperadas = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Entradas", Activa = true, CantidadProductos = 5 },
            new() { Id = Guid.NewGuid(), Nombre = "Platos Principales", Activa = true, CantidadProductos = 10 }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>>
        {
            Success = true,
            Data = categoriasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerAsync(soloActivas: true, ocultarVacias: false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.First().Nombre.Should().Be("Entradas");
    }

    [Fact]
    public async Task ObtenerAsync_ConRespuestaNula_DeberiaRetornarListaVacia()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task BuscarAsync_ConNombreValido_DeberiaRetornarCategorias()
    {
        // Arrange
        var categorias = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Pizzas", Activa = true }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>>
        {
            Success = true,
            Data = categorias
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.BuscarAsync("Pizza");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
        resultado.First().Nombre.Should().Be("Pizzas");
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdValido_DeberiaRetornarCategoria()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoria = new CategoriaProductoDto
        {
            Id = id,
            Nombre = "Bebidas",
            Descripcion = "Bebidas frías y calientes",
            Activa = true
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<CategoriaProductoDto>
        {
            Success = true,
            Data = categoria
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Bebidas");
        resultado.Id.Should().Be(id);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdInexistente_DeberiaLanzarExcepcion()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ObtenerPorIdAsync(id));
    }

    [Fact]
    public async Task CrearAsync_ConDatosValidos_DeberiaRetornarCategoriaCreada()
    {
        // Arrange
        var request = new CreateCategoriaRequest
        {
            Nombre = "Postres",
            Descripcion = "Dulces y postres",
            Color = "#FF6B6B",
            Icono = "🍰",
            Orden = 5,
            Activa = true
        };

        var categoriaCreada = new CategoriaProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Color = request.Color,
            Icono = request.Icono,
            Orden = request.Orden,
            Activa = request.Activa,
            FechaCreacion = DateTime.UtcNow
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<CategoriaProductoDto>
        {
            Success = true,
            Data = categoriaCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data!.Nombre.Should().Be("Postres");
    }

    [Fact]
    public async Task CrearAsync_ConErrorDeServidor_DeberiaRetornarNull()
    {
        // Arrange
        var request = new CreateCategoriaRequest
        {
            Nombre = "Test"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.CrearAsync(request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarAsync_ConDatosValidos_DeberiaRetornarCategoriaActualizada()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateCategoriaRequest
        {
            Id = id,
            Nombre = "Entradas Actualizadas",
            Descripcion = "Nueva descripción",
            Color = "#4CAF50",
            Icono = "🥗",
            Orden = 1,
            Activa = true
        };

        var categoriaActualizada = new CategoriaProductoDto
        {
            Id = id,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Color = request.Color,
            Icono = request.Icono,
            Orden = request.Orden,
            Activa = request.Activa
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<CategoriaProductoDto>
        {
            Success = true,
            Data = categoriaActualizada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarAsync(id, request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data!.Nombre.Should().Be("Entradas Actualizadas");
    }

    [Fact]
    public async Task ActualizarAsync_ConErrorDeServidor_DeberiaRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateCategoriaRequest
        {
            Id = id,
            Nombre = "Test"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ActualizarAsync(id, request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task EliminarAsync_ConIdValido_DeberiaRetornarTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        var resultado = await _service.EliminarAsync(id);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task EliminarAsync_ConIdInexistente_DeberiaRetornarFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.EliminarAsync(id);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ValidarNombreUnicoAsync_ConNombreUnico_DeberiaRetornarTrue()
    {
        // Arrange
        var nombre = "Categoria Nueva";

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = true
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ValidarNombreUnicoAsync(nombre);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task ValidarNombreUnicoAsync_ConNombreDuplicado_DeberiaRetornarFalse()
    {
        // Arrange
        var nombre = "Categoria Existente";

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = false
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ValidarNombreUnicoAsync(nombre);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ValidarNombreUnicoAsync_ConIdExcluir_DeberiaIncluirParametro()
    {
        // Arrange
        var nombre = "Categoria";
        var idExcluir = Guid.NewGuid();

        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = true
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ValidarNombreUnicoAsync(nombre, idExcluir);

        // Assert
        resultado.Should().BeTrue();
    }

    // ===== PRUEBAS DE ERRORES =====

    [Fact]
    public async Task ObtenerAsync_ConErrorDeConexion_DeberiaLanzarExcepcion()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ObtenerAsync());
    }

    [Fact]
    public async Task BuscarAsync_ConErrorDeServidor_DeberiaLanzarExcepcion()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.BuscarAsync("test"));
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConTimeout_DeberiaLanzarExcepcion()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _service.ObtenerPorIdAsync(id));
    }

    [Fact]
    public async Task CrearAsync_ConDatosInvalidos_DeberiaRetornarNull()
    {
        // Arrange
        var request = new CreateCategoriaRequest
        {
            Nombre = "" // Nombre vacío
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.CrearAsync(request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarAsync_ConIdInexistente_DeberiaRetornarNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateCategoriaRequest
        {
            Id = id,
            Nombre = "Test"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.ActualizarAsync(id, request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task EliminarAsync_ConErrorDeServidor_DeberiaRetornarFalse()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.EliminarAsync(id);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ValidarNombreUnicoAsync_ConErrorDeServidor_DeberiaLanzarExcepcion()
    {
        // Arrange
        var nombre = "Test";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ValidarNombreUnicoAsync(nombre));
    }

    // ===== PRUEBAS CON TOKEN =====

    [Fact]
    public async Task ObtenerAsync_ConTokenValido_DeberiaIncluirAutorizacion()
    {
        // Arrange
        _tokenStore.Token = "test-token-123";

        var categorias = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Test", Activa = true }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>>
        {
            Success = true,
            Data = categorias
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
    }

    [Fact]
    public async Task CrearAsync_ConTokenValido_DeberiaIncluirAutorizacion()
    {
        // Arrange
        _tokenStore.Token = "test-token-456";

        var request = new CreateCategoriaRequest
        {
            Nombre = "Test Category"
        };

        var categoriaCreada = new CategoriaProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Activa = true
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<CategoriaProductoDto>
        {
            Success = true,
            Data = categoriaCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
    }
}