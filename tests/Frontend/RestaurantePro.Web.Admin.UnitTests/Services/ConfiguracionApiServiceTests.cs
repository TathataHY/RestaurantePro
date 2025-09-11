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

public class ConfiguracionApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly TokenStore _tokenStore;
    private readonly ConfiguracionApiService _service;

    public ConfiguracionApiServiceTests()
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

        _service = new ConfiguracionApiService(_httpClientFactoryMock.Object, _tokenStore);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConRespuestaExitosa_DeberiaRetornarLista()
    {
        // Arrange
        var configuraciones = new List<ConfiguracionDto>
        {
            new() { Id = Guid.NewGuid(), Clave = "nombre_restaurante", Valor = "Mi Restaurante", Categoria = "general" },
            new() { Id = Guid.NewGuid(), Clave = "telefono", Valor = "555-1234", Categoria = "contacto" }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<ConfiguracionDto>>
        {
            Success = true,
            Data = configuraciones
        });

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
        resultado.Should().HaveCount(2);
        resultado.First().Clave.Should().Be("nombre_restaurante");
    }

    [Fact]
    public async Task ObtenerPorCategoriaAsync_ConCategoriaValida_DeberiaRetornarConfiguraciones()
    {
        // Arrange
        var categoria = "general";
        var configuraciones = new List<ConfiguracionDto>
        {
            new() { Id = Guid.NewGuid(), Clave = "nombre_restaurante", Valor = "Mi Restaurante", Categoria = categoria }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<ConfiguracionDto>>
        {
            Success = true,
            Data = configuraciones
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPorCategoriaAsync(categoria);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
        resultado.First().Categoria.Should().Be(categoria);
    }

    [Fact]
    public async Task ActualizarParametroAsync_ConRequestValido_DeberiaRetornarTrue()
    {
        // Arrange
        var request = new ActualizarConfiguracionRequest
        {
            Clave = "nombre_restaurante",
            Valor = "Nuevo Nombre"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var resultado = await _service.ActualizarParametroAsync(request);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerConfiguracionNotificacionesAsync_ConRespuestaExitosa_DeberiaRetornarConfiguracion()
    {
        // Arrange
        var configuracion = new ConfiguracionNotificacionesDto
        {
            NotificacionesEmail = true,
            NotificacionesSistema = true,
            NotificacionesStockBajo = true,
            DiasAntesVencimiento = 7,
            StockMinimoAlerta = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ConfiguracionNotificacionesDto>
        {
            Success = true,
            Data = configuracion
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerConfiguracionNotificacionesAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.NotificacionesEmail.Should().BeTrue();
        resultado.StockMinimoAlerta.Should().Be(10);
    }

    [Fact]
    public async Task ObtenerConfiguracionFidelizacionAsync_ConRespuestaExitosa_DeberiaRetornarConfiguracion()
    {
        // Arrange
        var configuracion = new ConfiguracionFidelizacionDto
        {
            SistemaActivo = true,
            PuntosPorPeso = 1,
            PesoPorPunto = 100,
            PuntosMinimosCanje = 1000,
            DescuentoMaximo = 50
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ConfiguracionFidelizacionDto>
        {
            Success = true,
            Data = configuracion
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerConfiguracionFidelizacionAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.SistemaActivo.Should().BeTrue();
        resultado.PuntosPorPeso.Should().Be(1);
    }

    // ===== PRUEBAS DE ERROR =====

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConErrorDeRed_DeberiaRetornarListaVacia()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerConfiguracionAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorCategoriaAsync_ConError404_DeberiaRetornarListaVacia()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var resultado = await _service.ObtenerPorCategoriaAsync("inexistente");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ActualizarParametroAsync_ConError500_DeberiaRetornarFalse()
    {
        // Arrange
        var request = new ActualizarConfiguracionRequest
        {
            Clave = "clave_invalida",
            Valor = "valor"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ActualizarParametroAsync(request);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerConfiguracionNotificacionesAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerConfiguracionNotificacionesAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerEstadisticasAsync_ConErrorDeRed_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act
        var resultado = await _service.ObtenerEstadisticasAsync();

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConDatosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var configuracionesExtremas = new List<ConfiguracionDto>
        {
            new() { 
                Id = Guid.Empty, 
                Clave = new string('A', 1000), // Clave muy larga
                Valor = new string('B', 10000), // Valor muy largo
                Categoria = "extrema",
                TipoDato = "string",
                EsEditable = true,
                FechaCreacion = DateTime.MinValue,
                FechaActualizacion = DateTime.MaxValue
            }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<ConfiguracionDto>>
        {
            Success = true,
            Data = configuracionesExtremas
        });

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
        resultado.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObtenerPorCategoriaAsync_ConCategoriaExtrema_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaExtrema = new string('X', 500); // Categoria muy larga
        var configuraciones = new List<ConfiguracionDto>
        {
            new() { Id = Guid.NewGuid(), Clave = "test", Valor = "test", Categoria = categoriaExtrema }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<ConfiguracionDto>>
        {
            Success = true,
            Data = configuraciones
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.ObtenerPorCategoriaAsync(categoriaExtrema);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
    }

    [Fact]
    public async Task ActualizarParametroAsync_ConDatosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var requestExtremo = new ActualizarConfiguracionRequest
        {
            Clave = new string('K', 1000), // Clave muy larga
            Valor = new string('V', 10000) // Valor muy largo
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var resultado = await _service.ActualizarParametroAsync(requestExtremo);

        // Assert
        resultado.Should().BeTrue();
    }

    // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConInyeccionSQL_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Error de validación SQL", Encoding.UTF8, "text/plain")
            });

        // Act
        var resultado = await _service.ObtenerConfiguracionAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPorCategoriaAsync_ConXSS_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaConXSS = "<script>alert('xss')</script>";
        
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Datos inválidos detectados", Encoding.UTF8, "text/plain")
            });

        // Act
        var resultado = await _service.ObtenerPorCategoriaAsync(categoriaConXSS);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ActualizarParametroAsync_ConDatosMaliciosos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var requestMalicioso = new ActualizarConfiguracionRequest
        {
            Clave = "'; DROP TABLE configuracion; --",
            Valor = "<script>alert('hack')</script>"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Parámetros inválidos", Encoding.UTF8, "text/plain")
            });

        // Act
        var resultado = await _service.ActualizarParametroAsync(requestMalicioso);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerConfiguracionNotificacionesAsync_ConValidacionExtrema_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Configuración inválida", Encoding.UTF8, "text/plain")
            });

        // Act
        var resultado = await _service.ObtenerConfiguracionNotificacionesAsync();

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var configuraciones = new List<ConfiguracionDto>
        {
            new() { Id = Guid.NewGuid(), Clave = "test1", Valor = "valor1", Categoria = "general" },
            new() { Id = Guid.NewGuid(), Clave = "test2", Valor = "valor2", Categoria = "general" }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<ConfiguracionDto>>
        {
            Success = true,
            Data = configuraciones
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Ejecutar múltiples operaciones simultáneas
        var tareas = new List<Task<List<ConfiguracionDto>>>();
        for (int i = 0; i < 15; i++)
        {
            tareas.Add(_service.ObtenerConfiguracionAsync());
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().HaveCount(15);
    }

    [Fact]
    public async Task ActualizarParametroAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new ActualizarConfiguracionRequest
        {
            Clave = "test_concurrent",
            Valor = "valor_concurrent"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act - Ejecutar múltiples actualizaciones simultáneas
        var tareas = new List<Task<bool>>();
        for (int i = 0; i < 20; i++)
        {
            tareas.Add(_service.ActualizarParametroAsync(request));
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().BeTrue());
        resultados.Should().HaveCount(20);
    }

    [Fact]
    public async Task ObtenerConfiguracionNotificacionesAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var configuracion = new ConfiguracionNotificacionesDto
        {
            NotificacionesEmail = true,
            NotificacionesSistema = true,
            NotificacionesStockBajo = true,
            DiasAntesVencimiento = 7,
            StockMinimoAlerta = 10
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ConfiguracionNotificacionesDto>
        {
            Success = true,
            Data = configuracion
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Ejecutar múltiples consultas simultáneas
        var tareas = new List<Task<ConfiguracionNotificacionesDto?>>();
        for (int i = 0; i < 25; i++)
        {
            tareas.Add(_service.ObtenerConfiguracionNotificacionesAsync());
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().HaveCount(25);
    }

    [Fact]
    public async Task ObtenerConfiguracionFidelizacionAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var configuracion = new ConfiguracionFidelizacionDto
        {
            SistemaActivo = true,
            PuntosPorPeso = 1,
            PesoPorPunto = 100,
            PuntosMinimosCanje = 1000,
            DescuentoMaximo = 50
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ConfiguracionFidelizacionDto>
        {
            Success = true,
            Data = configuracion
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act - Ejecutar múltiples consultas simultáneas
        var tareas = new List<Task<ConfiguracionFidelizacionDto?>>();
        for (int i = 0; i < 30; i++)
        {
            tareas.Add(_service.ObtenerConfiguracionFidelizacionAsync());
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().HaveCount(30);
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO Y LÍMITES =====

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConTimeout_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.RequestTimeout
            });

        // Act
        var resultado = await _service.ObtenerConfiguracionAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetearConfiguracionAsync_ConCategoriaValida_DeberiaRetornarTrue()
    {
        // Arrange
        var categoria = "general";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var resultado = await _service.ResetearConfiguracionAsync(categoria);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task ResetearConfiguracionAsync_ConError500_DeberiaRetornarFalse()
    {
        // Arrange
        var categoria = "inexistente";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ResetearConfiguracionAsync(categoria);

        // Assert
        resultado.Should().BeFalse();
    }
}
