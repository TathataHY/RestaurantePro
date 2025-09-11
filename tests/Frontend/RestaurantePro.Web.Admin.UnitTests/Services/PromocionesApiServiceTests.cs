using System.Net;
using System.Text;
using System.Text.Json;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class PromocionesApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<TokenStore> _tokenStoreMock;
    private readonly PromocionesApiService _service;

    public PromocionesApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStoreMock = new Mock<TokenStore>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api")).Returns(httpClient);

        _service = new PromocionesApiService(_httpClientFactoryMock.Object, _tokenStoreMock.Object);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerPromocionesAsync_ConParametrosValidos_DeberiaRetornarPromociones()
    {
        // Arrange
        var promocionesEsperadas = new PaginatedList<PromocionDto>
        {
            Items = new List<PromocionDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Promoción 1", EstaActiva = true },
                new() { Id = Guid.NewGuid(), Nombre = "Promoción 2", EstaActiva = true }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 20
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PaginatedList<PromocionDto>>
        {
            Success = true,
            Data = promocionesEsperadas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPromocionesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Items.Should().HaveCount(2);
        resultado.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task CrearPromocionAsync_ConDatosValidos_DeberiaCrearPromocion()
    {
        // Arrange
        var nuevaPromocion = new CrearPromocionRequest
        {
            Nombre = "Descuento 20%",
            Codigo = "DESC20",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 20,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(30),
            EstaActiva = true
        };

        var promocionCreada = new PromocionDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Descuento 20%",
            Codigo = "DESC20",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 20,
            EstaActiva = true
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PromocionDto>
        {
            Success = true,
            Data = promocionCreada
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.CrearPromocionAsync(nuevaPromocion);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Data!.Nombre.Should().Be("Descuento 20%");
        resultado.Data.ValorDescuento.Should().Be(20);
        resultado.Data.EstaActiva.Should().BeTrue();
    }

    [Fact]
    public async Task ActualizarPromocionAsync_ConDatosValidos_DeberiaActualizarPromocion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var promocionActualizada = new ActualizarPromocionRequest
        {
            Id = id,
            Nombre = "Descuento 30%",
            Codigo = "DESC30",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 30,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(30),
            EstaActiva = true
        };

        var promocionResultado = new PromocionDto
        {
            Id = id,
            Nombre = "Descuento 30%",
            Codigo = "DESC30",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 30,
            EstaActiva = true
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PromocionDto>
        {
            Success = true,
            Data = promocionResultado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ActualizarPromocionAsync(promocionActualizada);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Data!.Nombre.Should().Be("Descuento 30%");
        resultado.Data.ValorDescuento.Should().Be(30);
        resultado.Data.EstaActiva.Should().BeTrue();
    }

    [Fact]
    public async Task EliminarPromocionAsync_ConIdValido_DeberiaEliminarPromocion()
    {
        // Arrange
        var id = Guid.NewGuid();

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
        var resultado = await _service.EliminarPromocionAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Data.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_DeberiaRetornarEstadisticas()
    {
        // Arrange
        var estadisticas = new PromocionEstadisticasDto
        {
            TotalPromociones = 10,
            PromocionesActivas = 5,
            PromocionesExpiradas = 3,
            PromocionesPendientes = 2,
            TotalUsos = 150,
            DescuentoTotalAplicado = 5000
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PromocionEstadisticasDto>
        {
            Success = true,
            Data = estadisticas
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerEstadisticasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Data!.TotalPromociones.Should().Be(10);
        resultado.Data.PromocionesActivas.Should().Be(5);
        resultado.Data.TotalUsos.Should().Be(150);
    }

    // ===== PRUEBAS DE ERROR =====

    [Fact]
    public async Task ObtenerPromocionesAsync_ConErrorDeServidor_DeberiaLanzarExcepcion()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ObtenerPromocionesAsync());
    }

    [Fact]
    public async Task CrearPromocionAsync_ConDatosInvalidos_DeberiaLanzarExcepcion()
    {
        // Arrange
        var promocionInvalida = new CrearPromocionRequest
        {
            Nombre = "", // Nombre vacío
            Codigo = "", // Código vacío
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = -10 // Valor negativo
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<PromocionDto>
        {
            Success = false,
            Message = "Datos de promoción inválidos"
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.CrearPromocionAsync(promocionInvalida));
    }

    [Fact]
    public async Task ActualizarPromocionAsync_ConIdInexistente_DeberiaLanzarExcepcion()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var promocionActualizada = new ActualizarPromocionRequest
        {
            Id = idInexistente,
            Nombre = "Promoción Actualizada",
            Codigo = "UPDATED",
            Tipo = TipoPromocion.Porcentaje,
            ValorDescuento = 25,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today.AddDays(30),
            EstaActiva = true
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ActualizarPromocionAsync(promocionActualizada));
    }

    [Fact]
    public async Task EliminarPromocionAsync_ConIdInexistente_DeberiaLanzarExcepcion()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.EliminarPromocionAsync(idInexistente));
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConErrorDeServidor_DeberiaLanzarExcepcion()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.ObtenerEstadisticasAsync());
    }
}
