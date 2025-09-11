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
        var resultado = await _service.ObtenerProductosPaginadosAsync(1, 10, null, null, true, "Nombre", "asc");

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
}
