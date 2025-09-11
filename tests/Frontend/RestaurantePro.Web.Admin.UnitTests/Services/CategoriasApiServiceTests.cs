using System.Net;
using System.Text;
using System.Text.Json;
using Moq.Protected;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class CategoriasApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly CategoriasApiService _service;

    public CategoriasApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStoreMock = new Mock<TokenStore>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api")).Returns(httpClient);

        _service = new CategoriasApiService(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerAsync_ConRespuestaExitosa_DeberiaRetornarCategorias()
    {
        // Arrange
        var categoriasEsperadas = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Entradas", Descripcion = "Platos de entrada", Activa = true },
            new() { Id = Guid.NewGuid(), Nombre = "Platos Principales", Descripcion = "Platos principales", Activa = true }
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
        var resultado = await _service.ObtenerAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.First().Nombre.Should().Be("Entradas");
        resultado.Last().Nombre.Should().Be("Platos Principales");
    }

    [Fact]
    public async Task ObtenerAsync_ConFiltros_DeberiaRetornarCategoriasFiltradas()
    {
        // Arrange
        var categoriasEsperadas = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Entradas", Activa = true }
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
        var resultado = await _service.ObtenerAsync(soloActivas: true, ocultarVacias: true);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
        resultado.First().Nombre.Should().Be("Entradas");
    }

    [Fact]
    public async Task ObtenerAsync_ConErrorEnApi_DeberiaLanzarExcepcion()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ObtenerAsync());
    }

    [Fact]
    public async Task BuscarAsync_ConNombreValido_DeberiaRetornarCategorias()
    {
        // Arrange
        var categoriasEsperadas = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Pizzas", Descripcion = "Variedad de pizzas", Activa = true }
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
        var resultado = await _service.BuscarAsync("pizza");

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
        var categoriaEsperada = new CategoriaProductoDto
        {
            Id = id,
            Nombre = "Postres",
            Descripcion = "Dulces y postres",
            Activa = true
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<CategoriaProductoDto>
        {
            Success = true,
            Data = categoriaEsperada
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
        resultado!.Id.Should().Be(id);
        resultado.Nombre.Should().Be("Postres");
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
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Categoría no encontrada", Encoding.UTF8, "application/json")
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
            Nombre = "Bebidas",
            Descripcion = "Bebidas y refrescos",
            Activa = true
        };

        var categoriaCreada = new CategoriaProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Activa = request.Activa
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
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Be("Bebidas");
    }

    [Fact]
    public async Task CrearAsync_ConDatosInvalidos_DeberiaRetornarNull()
    {
        // Arrange
        var request = new CreateCategoriaRequest
        {
            Nombre = "", // Nombre vacío
            Descripcion = "Descripción válida",
            Activa = true
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Datos inválidos", Encoding.UTF8, "application/json")
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
            Nombre = "Bebidas Actualizadas",
            Descripcion = "Descripción actualizada",
            Activa = false
        };

        var categoriaActualizada = new CategoriaProductoDto
        {
            Id = id,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
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
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Be("Bebidas Actualizadas");
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
    public async Task ValidarNombreUnicoAsync_ConNombreDisponible_DeberiaRetornarTrue()
    {
        // Arrange
        var nombre = "Categoría Nueva";

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
    public async Task ValidarNombreUnicoAsync_ConNombreExistente_DeberiaRetornarFalse()
    {
        // Arrange
        var nombre = "Categoría Existente";

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

    // ===== PRUEBAS ROBUSTAS ADICIONALES =====

    [Fact]
    public async Task ObtenerAsync_ConJsonMalformado_DeberiaLanzarExcepcion()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ json malformado }", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => _service.ObtenerAsync());
    }

    [Fact]
    public async Task BuscarAsync_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriasEsperadas = new List<CategoriaProductoDto>();

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

        // Act - Buscar con caracteres especiales
        var resultado = await _service.BuscarAsync("categoría@#$%^&*()");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearAsync_ConTimeout_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new CreateCategoriaRequest
        {
            Nombre = "Categoría Test",
            Descripcion = "Descripción test",
            Activa = true
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _service.CrearAsync(request));
    }

    [Fact]
    public async Task ValidarNombreUnicoAsync_ConIdExcluir_DeberiaIncluirParametro()
    {
        // Arrange
        var nombre = "Categoría Test";
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

    [Fact]
    public async Task ObtenerAsync_ConConexionPerdida_DeberiaLanzarExcepcion()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ObtenerAsync());
    }

    // ===== PRUEBAS DE SEGURIDAD =====

    [Fact]
    public async Task BuscarAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriasEsperadas = new List<CategoriaProductoDto>();

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

        // Act - Buscar con inyección SQL
        var resultado = await _service.BuscarAsync("'; DROP TABLE categorias; --");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearAsync_ConXSS_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CreateCategoriaRequest
        {
            Nombre = "<script>alert('xss')</script>",
            Descripcion = "Descripción <img src=x onerror=alert('xss')>",
            Activa = true
        };

        var categoriaCreada = new CategoriaProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Activa = request.Activa
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
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Contain("<script>");
    }

    [Fact]
    public async Task ValidarNombreUnicoAsync_ConCaracteresEspecialesExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
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

        // Act - Validar con caracteres especiales extremos
        var resultado = await _service.ValidarNombreUnicoAsync("Categoría@#$%^&*()_+{}|:<>?[]\\;'\",./");

        // Assert
        resultado.Should().BeTrue();
    }

    // ===== PRUEBAS DE CONCURRENCIA =====

    [Fact]
    public async Task ObtenerAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriasEsperadas = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Entradas", Activa = true }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>>
        {
            Success = true,
            Data = categoriasEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Simular concurrencia con múltiples tareas
        var tasks = new List<Task<List<CategoriaProductoDto>>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_service.ObtenerAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(10);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r.Should().HaveCount(1));
    }

    [Fact]
    public async Task CrearAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CreateCategoriaRequest
        {
            Nombre = "Categoría Concurrencia",
            Descripcion = "Descripción de concurrencia",
            Activa = true
        };

        var categoriaCreada = new CategoriaProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Activa = request.Activa
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<CategoriaProductoDto>
        {
            Success = true,
            Data = categoriaCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Simular concurrencia con múltiples tareas
        var tasks = new List<Task<ApiResponse<CategoriaProductoDto>?>>();
        for (int i = 0; i < 8; i++)
        {
            tasks.Add(_service.CrearAsync(request));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(8);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Success.Should().BeTrue());
    }

    // ===== PRUEBAS DE LÍMITES =====

    [Fact]
    public async Task ObtenerAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriasMasivas = new List<CategoriaProductoDto>();
        for (int i = 0; i < 1000; i++)
        {
            categoriasMasivas.Add(new CategoriaProductoDto
            {
                Id = Guid.NewGuid(),
                Nombre = $"Categoría {i}",
                Descripcion = $"Descripción muy larga para la categoría {i} con muchos caracteres para probar el límite de datos masivos",
                Activa = i % 2 == 0
            });
        }

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<CategoriaProductoDto>>
        {
            Success = true,
            Data = categoriasMasivas
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
        resultado.Should().HaveCount(1000);
        resultado.First().Nombre.Should().Be("Categoría 0");
        resultado.Last().Nombre.Should().Be("Categoría 999");
    }

    [Fact]
    public async Task BuscarAsync_ConTerminoMuyLargo_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriasEsperadas = new List<CategoriaProductoDto>();

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

        // Act - Buscar con término muy largo
        var terminoLargo = new string('A', 10000);
        var resultado = await _service.BuscarAsync(terminoLargo);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidarNombreUnicoAsync_ConRateLimiting_DeberiaManejarCorrectamente()
    {
        // Arrange
        var responseContent = JsonSerializer.Serialize(new ApiResponse<bool>
        {
            Success = true,
            Data = true
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Simular múltiples validaciones rápidas (rate limiting)
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_service.ValidarNombreUnicoAsync($"Categoría{i}"));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(20);
        resultados.Should().AllSatisfy(r => r.Should().BeTrue());
    }
}
