using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Services;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class NotificacionesApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly TokenStore _tokenStore;
    private readonly NotificacionesApiService _service;

    public NotificacionesApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _tokenStore = new TokenStore();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost:7001/")
        };

        _httpClientFactoryMock
            .Setup(x => x.CreateClient("Api"))
            .Returns(httpClient);

        _tokenStore.Token = "test-token";

        _service = new NotificacionesApiService(_httpClientFactoryMock.Object, _tokenStore);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerNotificacionesAsync_ConRespuestaExitosa_DeberiaRetornarLista()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>
        {
            new() { Id = Guid.NewGuid(), Titulo = "Nueva Orden", Mensaje = "Se ha recibido una nueva orden", Tipo = "Orden", EstaLeida = false },
            new() { Id = Guid.NewGuid(), Titulo = "Stock Bajo", Mensaje = "El stock de tomates está bajo", Tipo = "Inventario", EstaLeida = true }
        };

        var apiResponse = new ApiResponse<List<NotificacionDto>>
        {
            Success = true,
            Data = notificaciones
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
        var resultado = await _service.ObtenerNotificacionesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(2);
        resultado.First().Titulo.Should().Be("Nueva Orden");
    }

    [Fact]
    public async Task ObtenerNotificacionesAsync_ConSoloNoLeidas_DeberiaRetornarSoloNoLeidas()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>
        {
            new() { Id = Guid.NewGuid(), Titulo = "Nueva Orden", Mensaje = "Se ha recibido una nueva orden", Tipo = "Orden", EstaLeida = false }
        };

        var apiResponse = new ApiResponse<List<NotificacionDto>>
        {
            Success = true,
            Data = notificaciones
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
        var resultado = await _service.ObtenerNotificacionesAsync(soloNoLeidas: true);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
        resultado.First().EstaLeida.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdValido_DeberiaRetornarNotificacion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var notificacion = new NotificacionDto
        {
            Id = id,
            Titulo = "Notificación Específica",
            Mensaje = "Esta es una notificación específica",
            Tipo = "Sistema",
            EstaLeida = false
        };

        var apiResponse = new ApiResponse<NotificacionDto>
        {
            Success = true,
            Data = notificacion
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
        var resultado = await _service.ObtenerPorIdAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Titulo.Should().Be("Notificación Específica");
        resultado.Id.Should().Be(id);
    }

    [Fact]
    public async Task CrearNotificacionAsync_ConRequestValido_DeberiaRetornarNotificacionCreada()
    {
        // Arrange
        var request = new CrearNotificacionRequest
        {
            Titulo = "Nueva Notificación",
            Mensaje = "Mensaje de la notificación",
            Tipo = "Sistema",
            EntidadRelacionadaId = Guid.NewGuid()
        };

        var notificacionCreada = new NotificacionDto
        {
            Id = Guid.NewGuid(),
            Titulo = request.Titulo,
            Mensaje = request.Mensaje,
            Tipo = request.Tipo,
            EntidadRelacionadaId = request.EntidadRelacionadaId,
            EstaLeida = false
        };

        var apiResponse = new ApiResponse<NotificacionDto>
        {
            Success = true,
            Data = notificacionCreada
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
        var resultado = await _service.CrearNotificacionAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Titulo.Should().Be("Nueva Notificación");
        resultado.Tipo.Should().Be("Sistema");
    }

    [Fact]
    public async Task MarcarComoLeidaAsync_ConIdValido_DeberiaRetornarTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var resultado = await _service.MarcarComoLeidaAsync(id);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task MarcarTodasComoLeidasAsync_ConRespuestaExitosa_DeberiaRetornarCantidad()
    {
        // Arrange
        var apiResponse = new ApiResponse<int>
        {
            Success = true,
            Data = 5
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
        var resultado = await _service.MarcarTodasComoLeidasAsync();

        // Assert
        resultado.Should().Be(5);
    }

    [Fact]
    public async Task EliminarNotificacionAsync_ConIdValido_DeberiaRetornarTrue()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var resultado = await _service.EliminarNotificacionAsync(id);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerContadorNoLeidasAsync_ConRespuestaExitosa_DeberiaRetornarContador()
    {
        // Arrange
        var apiResponse = new ApiResponse<int>
        {
            Success = true,
            Data = 3
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
        var resultado = await _service.ObtenerContadorNoLeidasAsync();

        // Assert
        resultado.Should().Be(3);
    }

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConRespuestaExitosa_DeberiaRetornarConfiguracion()
    {
        // Arrange
        var configuracion = new { NotificacionesEmail = true, NotificacionesPush = false };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = configuracion
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
        var resultado = await _service.ObtenerConfiguracionAsync();

        // Assert
        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ActualizarConfiguracionAsync_ConConfiguracionValida_DeberiaRetornarTrue()
    {
        // Arrange
        var configuracion = new { NotificacionesEmail = true, NotificacionesPush = true };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var resultado = await _service.ActualizarConfiguracionAsync(configuracion);

        // Assert
        resultado.Should().BeTrue();
    }

    // ===== PRUEBAS DE ERROR =====

    [Fact]
    public async Task ObtenerNotificacionesAsync_ConErrorDeRed_DeberiaRetornarListaVacia()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerNotificacionesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConError404_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.ObtenerPorIdAsync(Guid.NewGuid());

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearNotificacionAsync_ConError500_DeberiaRetornarNull()
    {
        // Arrange
        var request = new CrearNotificacionRequest { Titulo = "Test" };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.CrearNotificacionAsync(request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task MarcarComoLeidaAsync_ConError500_DeberiaRetornarFalse()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.MarcarComoLeidaAsync(Guid.NewGuid());

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task MarcarTodasComoLeidasAsync_ConErrorDeRed_DeberiaRetornarCero()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.MarcarTodasComoLeidasAsync();

        // Assert
        resultado.Should().Be(0);
    }

    [Fact]
    public async Task EliminarNotificacionAsync_ConErrorDeRed_DeberiaRetornarFalse()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.EliminarNotificacionAsync(Guid.NewGuid());

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerContadorNoLeidasAsync_ConErrorDeRed_DeberiaRetornarCero()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerContadorNoLeidasAsync();

        // Assert
        resultado.Should().Be(0);
    }

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerConfiguracionAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarConfiguracionAsync_ConErrorDeRed_DeberiaRetornarFalse()
    {
        // Arrange
        var configuracion = new { Test = "value" };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ActualizarConfiguracionAsync(configuracion);

        // Assert
        resultado.Should().BeFalse();
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public async Task ObtenerNotificacionesAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>();
        for (int i = 0; i < 10000; i++)
        {
            notificaciones.Add(new NotificacionDto
            {
                Id = Guid.NewGuid(),
                Titulo = $"Notificación {i}",
                Mensaje = $"Mensaje de la notificación número {i}",
                Tipo = i % 2 == 0 ? "Orden" : "Sistema",
                EstaLeida = i % 3 == 0,
                FechaCreacion = DateTime.UtcNow.AddMinutes(-i)
            });
        }

        var apiResponse = new ApiResponse<List<NotificacionDto>>
        {
            Success = true,
            Data = notificaciones
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
        var resultado = await _service.ObtenerNotificacionesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(10000);
        resultado.First().Titulo.Should().Be("Notificación 0");
    }

    [Fact]
    public async Task ObtenerNotificacionesAsync_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>
        {
            new() { Id = Guid.NewGuid(), Titulo = "Notificación con emoji 🚨", Mensaje = "Mensaje con acentos y ñoño", Tipo = "Sistema", EstaLeida = false },
            new() { Id = Guid.NewGuid(), Titulo = "Notificación con Unicode 中文", Mensaje = "Mensaje con caracteres especiales: @#$%^&*()", Tipo = "Orden", EstaLeida = true },
            new() { Id = Guid.NewGuid(), Titulo = "Notificación con HTML <b>bold</b>", Mensaje = "Mensaje con saltos de línea\nY tabulaciones\t", Tipo = "Inventario", EstaLeida = false }
        };

        var apiResponse = new ApiResponse<List<NotificacionDto>>
        {
            Success = true,
            Data = notificaciones
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
        var resultado = await _service.ObtenerNotificacionesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(3);
        resultado.First().Titulo.Should().Be("Notificación con emoji 🚨");
    }

    [Fact]
    public async Task CrearNotificacionAsync_ConValoresExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearNotificacionRequest
        {
            Titulo = new string('A', 1000), // Título muy largo
            Mensaje = new string('B', 5000), // Mensaje muy largo
            Tipo = new string('C', 100), // Tipo muy largo
            EntidadRelacionadaId = Guid.NewGuid()
        };

        var notificacionCreada = new NotificacionDto
        {
            Id = Guid.NewGuid(),
            Titulo = request.Titulo,
            Mensaje = request.Mensaje,
            Tipo = request.Tipo,
            EntidadRelacionadaId = request.EntidadRelacionadaId,
            EstaLeida = false
        };

        var apiResponse = new ApiResponse<NotificacionDto>
        {
            Success = true,
            Data = notificacionCreada
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
        var resultado = await _service.CrearNotificacionAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Titulo.Should().HaveLength(1000);
        resultado.Mensaje.Should().HaveLength(5000);
    }

    [Fact]
    public async Task ObtenerContadorNoLeidasAsync_ConValoresExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var apiResponse = new ApiResponse<int>
        {
            Success = true,
            Data = int.MaxValue
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
        var resultado = await _service.ObtenerContadorNoLeidasAsync();

        // Assert
        resultado.Should().Be(int.MaxValue);
    }

    // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====

    [Fact]
    public async Task CrearNotificacionAsync_ConXSS_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearNotificacionRequest
        {
            Titulo = "<script>alert('xss')</script>",
            Mensaje = "<img src=x onerror=alert('xss')>",
            Tipo = "<svg onload=alert('xss')>",
            EntidadRelacionadaId = Guid.NewGuid()
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.CrearNotificacionAsync(request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task CrearNotificacionAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearNotificacionRequest
        {
            Titulo = "'; DROP TABLE Notificaciones; --",
            Mensaje = "'; DELETE FROM Usuarios; --",
            Tipo = "'; UPDATE Configuracion SET Valor = 'hacked'; --",
            EntidadRelacionadaId = Guid.NewGuid()
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.CrearNotificacionAsync(request);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarConfiguracionAsync_ConPayloadsMaliciosos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var configuracion = new
        {
            NotificacionesEmail = true,
            NotificacionesPush = false,
            ConfiguracionMaliciosa = "'; DROP TABLE Configuracion; --",
            ScriptXSS = "<script>alert('xss')</script>",
            JsonInjection = "{\"malicious\": \"'; DROP TABLE Notificaciones; --\"}"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ActualizarConfiguracionAsync(configuracion);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerNotificacionesAsync_ConParametrosMaliciosos_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var resultado = await _service.ObtenerNotificacionesAsync(soloNoLeidas: true);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

    [Fact]
    public async Task ObtenerNotificacionesAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var notificaciones = new List<NotificacionDto>
        {
            new() { Id = Guid.NewGuid(), Titulo = "Notificación Concurrencia", Mensaje = "Mensaje de prueba", Tipo = "Sistema", EstaLeida = false }
        };

        var apiResponse = new ApiResponse<List<NotificacionDto>>
        {
            Success = true,
            Data = notificaciones
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var responseContent = JsonSerializer.Serialize(apiResponse);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                };
            });

        // Act
        var tasks = new List<Task<List<NotificacionDto>>>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(_service.ObtenerNotificacionesAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(50);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r.Should().HaveCount(1));
    }

    [Fact]
    public async Task CrearNotificacionAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new CrearNotificacionRequest
        {
            Titulo = "Notificación Concurrencia",
            Mensaje = "Mensaje de prueba",
            Tipo = "Sistema"
        };

        var notificacionCreada = new NotificacionDto
        {
            Id = Guid.NewGuid(),
            Titulo = request.Titulo,
            Mensaje = request.Mensaje,
            Tipo = request.Tipo,
            EstaLeida = false
        };

        var apiResponse = new ApiResponse<NotificacionDto>
        {
            Success = true,
            Data = notificacionCreada
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var responseContent = JsonSerializer.Serialize(apiResponse);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                };
            });

        // Act
        var tasks = new List<Task<NotificacionDto?>>();
        for (int i = 0; i < 30; i++)
        {
            tasks.Add(_service.CrearNotificacionAsync(request));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(30);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().AllSatisfy(r => r!.Titulo.Should().Be("Notificación Concurrencia"));
    }

    [Fact]
    public async Task MarcarComoLeidaAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 25; i++)
        {
            tasks.Add(_service.MarcarComoLeidaAsync(id));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(25);
        resultados.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    [Fact]
    public async Task ObtenerContadorNoLeidasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var apiResponse = new ApiResponse<int>
        {
            Success = true,
            Data = 5
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var responseContent = JsonSerializer.Serialize(apiResponse);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                };
            });

        // Act
        var tasks = new List<Task<int>>();
        for (int i = 0; i < 40; i++)
        {
            tasks.Add(_service.ObtenerContadorNoLeidasAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(40);
        resultados.Should().AllSatisfy(r => r.Should().Be(5));
    }

    [Fact]
    public async Task EliminarNotificacionAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_service.EliminarNotificacionAsync(id));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(20);
        resultados.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO Y LÍMITES =====

    [Fact]
    public async Task ObtenerNotificacionesAsync_ConTimeout_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var resultado = await _service.ObtenerNotificacionesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerNotificacionesAsync_ConError500_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ObtenerNotificacionesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerNotificacionesAsync_ConError503_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        // Act
        var resultado = await _service.ObtenerNotificacionesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task MarcarTodasComoLeidasAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var apiResponse = new ApiResponse<int>
        {
            Success = true,
            Data = 10
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var responseContent = JsonSerializer.Serialize(apiResponse);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                };
            });

        // Act
        var tasks = new List<Task<int>>();
        for (int i = 0; i < 15; i++)
        {
            tasks.Add(_service.MarcarTodasComoLeidasAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(15);
        resultados.Should().AllSatisfy(r => r.Should().Be(10));
    }

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var configuracion = new { NotificacionesEmail = true, NotificacionesPush = false };

        var apiResponse = new ApiResponse<object>
        {
            Success = true,
            Data = configuracion
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var responseContent = JsonSerializer.Serialize(apiResponse);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                };
            });

        // Act
        var tasks = new List<Task<object?>>();
        for (int i = 0; i < 35; i++)
        {
            tasks.Add(_service.ObtenerConfiguracionAsync());
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(35);
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
    }

    [Fact]
    public async Task ActualizarConfiguracionAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var configuracion = new { NotificacionesEmail = true, NotificacionesPush = true };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 25; i++)
        {
            tasks.Add(_service.ActualizarConfiguracionAsync(configuracion));
        }

        var resultados = await Task.WhenAll(tasks);

        // Assert
        resultados.Should().HaveCount(25);
        resultados.Should().AllSatisfy(r => r.Should().BeTrue());
    }
}
