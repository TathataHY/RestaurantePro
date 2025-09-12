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
        resultado!.Clave.Should().Be("nombre_restaurante");
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
        resultado.Should().BeNull();
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
        resultado.Should().BeNull();
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
        resultado!.Clave.Should().Be("test_key");
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
        resultado.Should().BeNull();
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
        resultado.Should().BeNull();
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
        var tareas = new List<Task<ConfiguracionDto?>>();
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
        resultado.Should().BeNull();
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

    // ===== PRUEBAS ROBUSTAS AVANZADAS - CASOS EDGE EXTREMOS =====

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConDatosMasivos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var configuracionesMasivas = Enumerable.Range(1, 10000).Select(i => new ConfiguracionDto
        {
            Id = Guid.NewGuid(),
            Clave = $"config_{i}",
            Valor = $"valor_{i}",
            Categoria = $"categoria_{i % 100}",
            TipoDato = "string",
            EsEditable = i % 2 == 0,
            FechaCreacion = DateTime.Now.AddDays(-i),
            FechaActualizacion = DateTime.Now.AddDays(-i + 1)
        }).ToList();

        var responseContent = JsonSerializer.Serialize(new ApiResponse<List<ConfiguracionDto>>
        {
            Success = true,
            Data = configuracionesMasivas
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
        resultado!.Clave.Should().Be("test_key");
    }

    [Fact]
    public async Task ObtenerPorCategoriaAsync_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoriaEspecial = "categoría-con-ñ-y-acentos-áéíóú-🚀-emoji-特殊字符-中文";
        var configuraciones = new List<ConfiguracionDto>
        {
            new() { Id = Guid.NewGuid(), Clave = "test_especial", Valor = "valor_especial", Categoria = categoriaEspecial }
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
        var resultado = await _service.ObtenerPorCategoriaAsync(categoriaEspecial);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Should().HaveCount(1);
    }

    [Fact]
    public async Task ActualizarParametroAsync_ConValoresNumericosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var requestNumerico = new ActualizarConfiguracionRequest
        {
            Clave = "valor_numerico_extremo",
            Valor = decimal.MaxValue.ToString() // Valor numérico máximo
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var resultado = await _service.ActualizarParametroAsync(requestNumerico);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerConfiguracionNotificacionesAsync_ConValoresExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var configuracionExtrema = new ConfiguracionNotificacionesDto
        {
            NotificacionesEmail = true,
            NotificacionesSistema = true,
            NotificacionesStockBajo = true,
            NotificacionesVencimiento = true,
            NotificacionesReservaciones = true,
            NotificacionesComandas = true,
            DiasAntesVencimiento = int.MaxValue,
            StockMinimoAlerta = int.MaxValue
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<ConfiguracionNotificacionesDto>
        {
            Success = true,
            Data = configuracionExtrema
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
        resultado!.DiasAntesVencimiento.Should().Be(int.MaxValue);
        resultado.StockMinimoAlerta.Should().Be(int.MaxValue);
    }

    // ===== PRUEBAS ROBUSTAS AVANZADAS - SEGURIDAD EXTREMA =====

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConInyeccionSQLCompleja_DeberiaManejarCorrectamente()
    {
        // Arrange
        var payloadsSQL = new[]
        {
            "'; DROP TABLE configuracion; --",
            "' OR '1'='1",
            "'; INSERT INTO configuracion VALUES ('hack', 'hack'); --",
            "' UNION SELECT * FROM usuarios --",
            "'; EXEC xp_cmdshell('dir'); --"
        };

        foreach (var payload in payloadsSQL)
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent($"Error de validación SQL: {payload}", Encoding.UTF8, "text/plain")
                });

            // Act
            var resultado = await _service.ObtenerConfiguracionAsync();

            // Assert
            resultado.Should().BeNull();
        }
    }

    [Fact]
    public async Task ObtenerPorCategoriaAsync_ConXSSAvanzado_DeberiaManejarCorrectamente()
    {
        // Arrange
        var payloadsXSS = new[]
        {
            "<script>alert('xss')</script>",
            "javascript:alert('xss')",
            "<img src=x onerror=alert('xss')>",
            "<svg onload=alert('xss')>",
            "';alert('xss');//",
            "<iframe src=javascript:alert('xss')></iframe>"
        };

        foreach (var payload in payloadsXSS)
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Datos inválidos detectados", Encoding.UTF8, "text/plain")
                });

            // Act
            var resultado = await _service.ObtenerPorCategoriaAsync(payload);

            // Assert
            resultado.Should().BeNull();
        }
    }

    [Fact]
    public async Task ActualizarParametroAsync_ConPayloadsMaliciosos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var payloadsMaliciosos = new[]
        {
            new ActualizarConfiguracionRequest { Clave = "../../../etc/passwd", Valor = "hack" },
            new ActualizarConfiguracionRequest { Clave = "config", Valor = "<?xml version='1.0'?><root><hack>true</hack></root>" },
            new ActualizarConfiguracionRequest { Clave = "config", Valor = "{\"hack\": true, \"malicious\": \"payload\"}" },
            new ActualizarConfiguracionRequest { Clave = "config", Valor = "command|whoami" },
            new ActualizarConfiguracionRequest { Clave = "config", Valor = "eval('malicious code')" }
        };

        foreach (var payload in payloadsMaliciosos)
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Parámetros inválidos", Encoding.UTF8, "text/plain")
                });

            // Act
            var resultado = await _service.ActualizarParametroAsync(payload);

            // Assert
            resultado.Should().BeFalse();
        }
    }

    [Fact]
    public async Task ObtenerConfiguracionFidelizacionAsync_ConValidacionExtrema_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Configuración de fidelización inválida", Encoding.UTF8, "text/plain")
            });

        // Act
        var resultado = await _service.ObtenerConfiguracionFidelizacionAsync();

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS AVANZADAS - CONCURRENCIA EXTREMA =====

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConConcurrenciaMasiva_DeberiaManejarCorrectamente()
    {
        // Arrange
        var configuraciones = new List<ConfiguracionDto>
        {
            new() { Id = Guid.NewGuid(), Clave = "concurrent_test", Valor = "valor", Categoria = "test" }
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

        // Act - Ejecutar 100 operaciones simultáneas
        var tareas = new List<Task<ConfiguracionDto?>>();
        for (int i = 0; i < 100; i++)
        {
            tareas.Add(_service.ObtenerConfiguracionAsync());
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().HaveCount(100);
    }

    [Fact]
    public async Task ActualizarParametroAsync_ConConcurrenciaMasiva_DeberiaManejarCorrectamente()
    {
        // Arrange
        var request = new ActualizarConfiguracionRequest
        {
            Clave = "concurrent_update",
            Valor = "valor_concurrent"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act - Ejecutar 150 actualizaciones simultáneas
        var tareas = new List<Task<bool>>();
        for (int i = 0; i < 150; i++)
        {
            tareas.Add(_service.ActualizarParametroAsync(request));
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().BeTrue());
        resultados.Should().HaveCount(150);
    }

    [Fact]
    public async Task ObtenerConfiguracionNotificacionesAsync_ConConcurrenciaMasiva_DeberiaManejarCorrectamente()
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

        // Act - Ejecutar 200 consultas simultáneas
        var tareas = new List<Task<ConfiguracionNotificacionesDto?>>();
        for (int i = 0; i < 200; i++)
        {
            tareas.Add(_service.ObtenerConfiguracionNotificacionesAsync());
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().HaveCount(200);
    }

    [Fact]
    public async Task ObtenerConfiguracionFidelizacionAsync_ConConcurrenciaMasiva_DeberiaManejarCorrectamente()
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

        // Act - Ejecutar 250 consultas simultáneas
        var tareas = new List<Task<ConfiguracionFidelizacionDto?>>();
        for (int i = 0; i < 250; i++)
        {
            tareas.Add(_service.ObtenerConfiguracionFidelizacionAsync());
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().NotBeNull());
        resultados.Should().HaveCount(250);
    }

    // ===== PRUEBAS ROBUSTAS AVANZADAS - RENDIMIENTO Y LÍMITES EXTREMOS =====

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConTimeoutExtremo_DeberiaManejarCorrectamente()
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
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConError500_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var resultado = await _service.ObtenerConfiguracionAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerConfiguracionAsync_ConError503_DeberiaManejarCorrectamente()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        // Act
        var resultado = await _service.ObtenerConfiguracionAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ResetearConfiguracionAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var categoria = "general";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act - Ejecutar múltiples resets simultáneos
        var tareas = new List<Task<bool>>();
        for (int i = 0; i < 50; i++)
        {
            tareas.Add(_service.ResetearConfiguracionAsync(categoria));
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().BeTrue());
        resultados.Should().HaveCount(50);
    }

    [Fact]
    public async Task ActualizarConfiguracionNotificacionesAsync_ConConcurrencia_DeberiaManejarCorrectamente()
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

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act - Ejecutar múltiples actualizaciones simultáneas
        var tareas = new List<Task<bool>>();
        for (int i = 0; i < 75; i++)
        {
            tareas.Add(_service.ActualizarConfiguracionNotificacionesAsync(configuracion));
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().BeTrue());
        resultados.Should().HaveCount(75);
    }

    [Fact]
    public async Task ActualizarConfiguracionFidelizacionAsync_ConConcurrencia_DeberiaManejarCorrectamente()
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

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act - Ejecutar múltiples actualizaciones simultáneas
        var tareas = new List<Task<bool>>();
        for (int i = 0; i < 100; i++)
        {
            tareas.Add(_service.ActualizarConfiguracionFidelizacionAsync(configuracion));
        }

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.Should().BeTrue());
        resultados.Should().HaveCount(100);
    }
}
