using System.Net;
using System.Text;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class AuthApiServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly AuthApiService _service;

    public AuthApiServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        httpClient.BaseAddress = new Uri("http://localhost:8080");
        
        _httpClientFactoryMock.Setup(x => x.CreateClient("Api"))
            .Returns(httpClient);

        _service = new AuthApiService(_httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ConCredencialesValidas_DeberiaRetornarAuthResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurante.com",
            Password = "password123"
        };

        var authResponseEsperado = new AuthResponse
        {
            Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            RefreshToken = "refresh_token_123",
            Expiration = DateTime.UtcNow.AddHours(1)
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<AuthResponse>
        {
            Success = true,
            Data = authResponseEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.LoginAsync(loginRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Token.Should().Be("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");
        resultado.RefreshToken.Should().Be("refresh_token_123");
        resultado.Expiration.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task LoginAsync_ConCredencialesInvalidas_DeberiaRetornarNull()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurante.com",
            Password = "password_incorrecto"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Credenciales inválidas", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.LoginAsync(loginRequest);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ConErrorEnServidor_DeberiaRetornarNull()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurante.com",
            Password = "password123"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno del servidor", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.LoginAsync(loginRequest);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetProfileAsync_ConTokenValido_DeberiaRetornarPerfilUsuario()
    {
        // Arrange
        var perfilEsperado = new AuthUserDto
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@restaurante.com",
            UserName = "admin",
            Roles = new List<string> { "Administrador" }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<AuthUserDto>
        {
            Success = true,
            Data = perfilEsperado
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.GetProfileAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(perfilEsperado.Id);
        resultado.Email.Should().Be("admin@restaurante.com");
        resultado.UserName.Should().Be("admin");
        resultado.Roles.Should().Contain("Administrador");
    }

    [Fact]
    public async Task GetProfileAsync_ConTokenInvalido_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Token inválido", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.GetProfileAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetProfileAsync_ConErrorEnServidor_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno del servidor", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.GetProfileAsync();

        // Assert
        resultado.Should().BeNull();
    }

    // ===== PRUEBAS ROBUSTAS ADICIONALES =====

    [Fact]
    public async Task LoginAsync_ConCredencialesExtremas_DeberiaManejarCorrectamente()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "A".PadRight(100, 'A') + "@example.com", // Email muy largo
            Password = "B".PadRight(50, 'B'), // Contraseña muy larga
            Recordarme = true
        };

        var authResponse = new AuthResponse
        {
            Success = true,
            Token = "C".PadRight(1000, 'C'), // Token muy largo
            RefreshToken = "D".PadRight(1000, 'D'), // Refresh token muy largo
            Expiration = DateTime.UtcNow.AddHours(24),
            UserId = Guid.NewGuid().ToString(),
            UserName = "E".PadRight(50, 'E'), // Username muy largo
            Roles = new List<string> { "Administrador", "SuperUsuario", "Moderador" }
        };

        var responseContent = JsonSerializer.Serialize(new ApiResponse<AuthResponse>
        {
            Success = true,
            Data = authResponse
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.LoginAsync(loginRequest);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Token.Should().HaveLength(1000);
        resultado.Roles.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoginAsync_ConTimeout_DeberiaLanzarExcepcion()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurante.com",
            Password = "password123"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _service.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task GetProfileAsync_ConConexionPerdida_DeberiaLanzarExcepcion()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection lost"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetProfileAsync());
    }

    [Fact]
    public async Task LoginAsync_ConJsonMalformado_DeberiaLanzarExcepcion()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurante.com",
            Password = "password123"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ json malformado }", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => _service.LoginAsync(loginRequest));
    }

    [Fact]
    public async Task GetProfileAsync_ConJsonMalformado_DeberiaLanzarExcepcion()
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
        await Assert.ThrowsAsync<JsonException>(() => _service.GetProfileAsync());
    }

    [Fact]
    public async Task LoginAsync_ConDatosInvalidosExtremos_DeberiaRetornarError()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "", // Email vacío
            Password = "", // Contraseña vacía
            Recordarme = false
        };

        var authResponse = new AuthResponse
        {
            Success = false,
            Message = "Credenciales inválidas",
            Token = "",
            Expiration = DateTime.MinValue,
            UserId = "",
            UserName = "",
            Roles = new List<string>()
        };

        var responseContent = JsonSerializer.Serialize(authResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.LoginAsync(loginRequest);

        // Assert
        resultado.Should().BeNull(); // El servicio retorna null en caso de error HTTP
    }

    [Fact]
    public async Task LoginAsync_ConErrorDeServidor_DeberiaRetornarError()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurante.com",
            Password = "password123"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error interno del servidor", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.LoginAsync(loginRequest);

        // Assert
        resultado.Should().BeNull(); // El servicio retorna null en caso de error HTTP
    }

    [Fact]
    public async Task GetProfileAsync_ConTokenExpirado_DeberiaRetornarNull()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Token expirado", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.GetProfileAsync();

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ConRespuestaNula_DeberiaRetornarNull()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurante.com",
            Password = "password123"
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            });

        // Act
        var resultado = await _service.LoginAsync(loginRequest);

        // Assert
        resultado.Should().BeNull();
    }
}
