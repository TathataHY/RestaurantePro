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
}
