using System.Net;
using System.Text;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class ProductosApiServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly ProductosApiService _service;

    public ProductosApiServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _tokenStoreMock = new Mock<TokenStore>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri("http://localhost:8080");
        
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api"))
            .Returns(httpClient);

        _service = new ProductosApiService(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_ConRespuestaExitosa_DeberiaRetornarCategorias()
    {
        // Arrange
        var categoriasEsperadas = new List<CategoriaProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Entradas", Activa = true },
            new() { Id = Guid.NewGuid(), Nombre = "Platos Principales", Activa = true }
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
        var resultado = await _service.ObtenerCategoriasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.Should().BeEquivalentTo(categoriasEsperadas);
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_ConErrorEnApi_DeberiaLanzarExcepcion()
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
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ObtenerCategoriasAsync());
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ConParametrosValidos_DeberiaRetornarProductosPaginados()
    {
        // Arrange
        var productosEsperados = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Hamburguesa", Precio = 15.99m },
                new() { Id = Guid.NewGuid(), Nombre = "Pizza", Precio = 12.50m }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
        {
            Success = true,
            Data = productosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerProductosPaginadosAsync(1, 10, null, null, true, orderBy: "Nombre", orderDirection: "asc");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().HaveCount(2);
        resultado.TotalCount.Should().Be(2);
        resultado.PageNumber.Should().Be(1);
        resultado.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task CrearAsync_ConProductoValido_DeberiaRetornarProductoCreado()
    {
        // Arrange
        var productoRequest = new CreateProductoRequest
        {
            Nombre = "Nuevo Producto",
            Precio = 20.00m,
            CategoriaId = Guid.NewGuid()
        };

        var productoEsperado = new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Nuevo Producto",
            Precio = 20.00m
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ProductoDto>
        {
            Success = true,
            Data = productoEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearAsync(productoRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Nuevo Producto");
        resultado.Precio.Should().Be(20.00m);
    }

    [Fact]
    public async Task CrearAsync_ConErrorEnApi_DeberiaRetornarNull()
    {
        // Arrange
        var productoRequest = new CreateProductoRequest
        {
            Nombre = "Producto Inválido",
            Precio = -10.00m
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Error de validación", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearAsync(productoRequest);

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

    // ===== PRUEBAS ROBUSTAS ADICIONALES =====

    [Fact]
    public async Task ObtenerCategoriasAsync_ConRespuestaNula_DeberiaRetornarListaVacia()
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
        var resultado = await _service.ObtenerCategoriasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_ConJsonMalformado_DeberiaLanzarExcepcion()
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
        await Assert.ThrowsAsync<JsonException>(() => _service.ObtenerCategoriasAsync());
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ConFiltrosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var productosEsperados = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 1
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
        {
            Success = true,
            Data = productosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerProductosPaginadosAsync(
            pageNumber: 1, 
            pageSize: 1, 
            filtro: "test@#$%^&*()", 
            categoriaId: Guid.NewGuid(), 
            soloActivos: true, 
            orderBy: "Nombre", 
            orderDirection: "asc"
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().BeEmpty();
        resultado.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ConPaginacionExtrema_DeberiaManejarCorrectamente()
    {
        // Arrange
        var productosEsperados = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            TotalCount = 0,
            PageNumber = 999999,
            PageSize = 1000
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
        {
            Success = true,
            Data = productosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerProductosPaginadosAsync(
            pageNumber: 999999, 
            pageSize: 1000, 
            filtro: null, 
            categoriaId: null, 
            soloActivos: false, 
            orderBy: "Precio", 
            orderDirection: "desc"
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.PageNumber.Should().Be(999999);
        resultado.PageSize.Should().Be(1000);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdValido_DeberiaRetornarProducto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var productoEsperado = new ProductoDto
        {
            Id = id,
            Nombre = "Pizza Margherita",
            Descripcion = "Pizza clásica italiana",
            Precio = 15.99m,
            Activo = true
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ProductoDto>
        {
            Success = true,
            Data = productoEsperado
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
        resultado.Nombre.Should().Be("Pizza Margherita");
        resultado.Precio.Should().Be(15.99m);
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
                Content = new StringContent("Producto no encontrado", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ObtenerPorIdAsync(id));
    }

    [Fact]
    public async Task CrearAsync_ConTimeout_DeberiaLanzarExcepcion()
    {
        // Arrange
        var productoRequest = new CreateProductoRequest
        {
            Nombre = "Producto Test",
            Descripcion = "Descripción test",
            Precio = 10.99m
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _service.CrearAsync(productoRequest));
    }

    [Fact]
    public async Task ActualizarAsync_ConDatosInvalidos_DeberiaRetornarNull()
    {
        // Arrange
        var productoRequest = new UpdateProductoRequest
        {
            Id = Guid.NewGuid(),
            Nombre = "",
            Descripcion = "",
            Precio = -1.0m
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Datos inválidos", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarAsync(productoRequest);

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
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno del servidor", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.EliminarAsync(id);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ConConexionPerdida_DeberiaLanzarExcepcion()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.ObtenerProductosPaginadosAsync(1, 10, null, null, true, orderBy: "Nombre", orderDirection: "asc"));
    }

    // ===== PRUEBAS DE SEGURIDAD =====

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        var productosEsperados = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
        {
            Success = true,
            Data = productosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Intentar inyección SQL en filtros
        var resultado = await _service.ObtenerProductosPaginadosAsync(
            pageNumber: 1,
            pageSize: 10,
            filtro: "'; DROP TABLE productos; --",
            categoriaId: null,
            soloActivos: true,
            orderBy: "Nombre",
            orderDirection: "asc"
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task CrearAsync_ConXSS_DeberiaManejarCorrectamente()
    {
        // Arrange
        var productoRequest = new CreateProductoRequest
        {
            Nombre = "<script>alert('XSS')</script>Pizza",
            Descripcion = "Pizza con <img src=x onerror=alert('XSS')>",
            Precio = 15.99m
        };

        var productoEsperado = new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = productoRequest.Nombre,
            Descripcion = productoRequest.Descripcion,
            Precio = productoRequest.Precio
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ProductoDto>
        {
            Success = true,
            Data = productoEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearAsync(productoRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be(productoRequest.Nombre);
        resultado.Descripcion.Should().Be(productoRequest.Descripcion);
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ConCaracteresEspecialesExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var productosEsperados = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
        {
            Success = true,
            Data = productosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Caracteres especiales extremos
        var resultado = await _service.ObtenerProductosPaginadosAsync(
            pageNumber: 1,
            pageSize: 10,
            filtro: "!@#$%^&*()_+-=[]{}|;':\",./<>?`~",
            categoriaId: null,
            soloActivos: true,
            orderBy: "Nombre",
            orderDirection: "asc"
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().BeEmpty();
    }

    // ===== PRUEBAS DE CONCURRENCIA =====

    [Fact]
    public async Task CrearAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var productoRequest = new CreateProductoRequest
        {
            Nombre = "Producto Concurrencia",
            Descripcion = "Descripción de concurrencia",
            Precio = 10.99m
        };

        var productoEsperado = new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = productoRequest.Nombre,
            Descripcion = productoRequest.Descripcion,
            Precio = productoRequest.Precio
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ProductoDto>
        {
            Success = true,
            Data = productoEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Simular concurrencia con múltiples tareas
        var tasks = new List<Task<ProductoDto?>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_service.CrearAsync(productoRequest));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(10);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Nombre.Should().Be("Producto Concurrencia"));
    }

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var productosEsperados = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Producto 1", Precio = 10.99m },
                new() { Id = Guid.NewGuid(), Nombre = "Producto 2", Precio = 15.99m }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
        {
            Success = true,
            Data = productosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Simular concurrencia con múltiples consultas
        var tasks = new List<Task<PaginatedList<ProductoDto>?>>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_service.ObtenerProductosPaginadosAsync(1, 10, null, null, true, orderBy: "Nombre", orderDirection: "asc"));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(20);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Items.Should().HaveCount(2));
    }

    // ===== PRUEBAS DE LÍMITES Y RENDIMIENTO =====

    [Fact]
    public async Task ObtenerProductosPaginadosAsync_ConPaginacionMasiva_DeberiaManejarCorrectamente()
    {
        // Arrange
        var productosEsperados = new PaginatedList<ProductoDto>
        {
            Items = new List<ProductoDto>(),
            TotalCount = 1000000, // 1 millón de productos
            PageNumber = 500000,
            PageSize = 1000
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<ProductoDto>>
        {
            Success = true,
            Data = productosEsperados
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerProductosPaginadosAsync(
            pageNumber: 500000,
            pageSize: 1000,
            filtro: null,
            categoriaId: null,
            soloActivos: true,
            orderBy: "Nombre",
            orderDirection: "asc"
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado!.TotalCount.Should().Be(1000000);
        resultado.PageNumber.Should().Be(500000);
        resultado.PageSize.Should().Be(1000);
    }

    [Fact]
    public async Task CrearAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var productoRequest = new CreateProductoRequest
        {
            Nombre = "A".PadRight(1000, 'A'), // Nombre de 1000 caracteres
            Descripcion = "B".PadRight(5000, 'B'), // Descripción de 5000 caracteres
            Precio = 999999.99m // Precio máximo
        };

        var productoEsperado = new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = productoRequest.Nombre,
            Descripcion = productoRequest.Descripcion,
            Precio = productoRequest.Precio
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ProductoDto>
        {
            Success = true,
            Data = productoEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearAsync(productoRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().HaveLength(1000);
        resultado.Descripcion.Should().HaveLength(5000);
        resultado.Precio.Should().Be(999999.99m);
    }
}
