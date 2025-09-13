using System.Net;
using System.Net.Http.Headers;
using Moq.Protected;

namespace RestaurantePro.Web.Admin.UnitTests.Services;

public class AuthTokenHandlerTests
{
    private readonly Mock<HttpMessageHandler> _innerHandlerMock;
    private readonly TokenStore _tokenStore;
    private readonly HttpClient _httpClient;

    public AuthTokenHandlerTests()
    {
        _innerHandlerMock = new Mock<HttpMessageHandler>();
        _tokenStore = new TokenStore();
        
        var handler = new AuthTokenHandler()
        {
            InnerHandler = _innerHandlerMock.Object
        };
        
        _httpClient = new HttpClient(handler);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task SendAsync_ConTokenValido_DeberiaAgregarAutorizacion()
    {
        // Arrange
        _tokenStore.Token = "test-token-123";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/test");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization.Should().NotBeNull();
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("test-token-123");
        request.Headers.Contains("X-Bearer-Token").Should().BeTrue();
        request.Headers.GetValues("X-Bearer-Token").First().Should().Be("test-token-123");
    }

    [Fact]
    public async Task SendAsync_ConTokenVacio_NoDeberiaAgregarAutorizacion()
    {
        // Arrange
        _tokenStore.Token = string.Empty;

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/test");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization.Should().BeNull();
        request.Headers.Contains("X-Bearer-Token").Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_ConTokenNull_NoDeberiaAgregarAutorizacion()
    {
        // Arrange
        _tokenStore.Token = null!;

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/test");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization.Should().BeNull();
        request.Headers.Contains("X-Bearer-Token").Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_ConTokenSoloEspacios_NoDeberiaAgregarAutorizacion()
    {
        // Arrange
        _tokenStore.Token = "   ";

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/test");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization.Should().BeNull();
        request.Headers.Contains("X-Bearer-Token").Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_ConAutorizacionExistente_NoDeberiaSobrescribir()
    {
        // Arrange
        _tokenStore.Token = "test-token-123";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/test");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", "dGVzdDp0ZXN0");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization!.Scheme.Should().Be("Basic");
        request.Headers.Authorization.Parameter.Should().Be("dGVzdDp0ZXN0");
        request.Headers.Contains("X-Bearer-Token").Should().BeTrue();
        request.Headers.GetValues("X-Bearer-Token").First().Should().Be("test-token-123");
    }

    [Fact]
    public async Task SendAsync_ConAutorizacionNull_DeberiaAgregarBearer()
    {
        // Arrange
        _tokenStore.Token = "test-token-456";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/test");
        // No establecer Authorization (será null)
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("test-token-456");
        request.Headers.Contains("X-Bearer-Token").Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_ConXBearerTokenExistente_NoDeberiaDuplicar()
    {
        // Arrange
        _tokenStore.Token = "test-token-789";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/test");
        request.Headers.Add("X-Bearer-Token", "existing-token");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.GetValues("X-Bearer-Token").Should().HaveCount(1);
        request.Headers.GetValues("X-Bearer-Token").First().Should().Be("existing-token");
    }

    [Fact]
    public async Task SendAsync_ConTokenValido_DeberiaPasarRequestAlInnerHandler()
    {
        // Arrange
        _tokenStore.Token = "test-token-abc";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test.com/create");
        request.Content = new StringContent("test data");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _innerHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r == request),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_ConErrorDelInnerHandler_DeberiaPropagarError()
    {
        // Arrange
        _tokenStore.Token = "test-token-error";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/error");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.SendAsync(request));
    }

    [Fact]
    public async Task SendAsync_ConTimeout_DeberiaPropagarTimeout()
    {
        // Arrange
        _tokenStore.Token = "test-token-timeout";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/slow");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => _httpClient.SendAsync(request));
    }

    // ===== PRUEBAS DE DIFERENTES MÉTODOS HTTP =====

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task SendAsync_ConDiferentesMetodos_DeberiaAgregarToken(string methodName)
    {
        // Arrange
        var method = new HttpMethod(methodName);
        _tokenStore.Token = $"token-{methodName}";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        var request = new HttpRequestMessage(method, "https://api.test.com/test");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be($"token-{methodName}");
    }

    // ===== PRUEBAS DE TOKENSTORE =====

    [Fact]
    public void TokenStore_ConTokenValido_DeberiaEstarAutenticado()
    {
        // Arrange & Act
        _tokenStore.Token = "valid-token";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        // Assert
        _tokenStore.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void TokenStore_ConTokenExpirado_NoDeberiaEstarAutenticado()
    {
        // Arrange & Act
        _tokenStore.Token = "expired-token";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(-1);

        // Assert
        _tokenStore.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void TokenStore_ConTokenVacio_NoDeberiaEstarAutenticado()
    {
        // Arrange & Act
        _tokenStore.Token = string.Empty;

        // Assert
        _tokenStore.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void TokenStore_ConTokenNull_NoDeberiaEstarAutenticado()
    {
        // Arrange & Act
        _tokenStore.Token = null!;

        // Assert
        _tokenStore.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void TokenStore_Clear_DeberiaLimpiarTodosLosValores()
    {
        // Arrange
        _tokenStore.Token = "test-token";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);
        _tokenStore.RefreshToken = "refresh-token";
        _tokenStore.UserName = "testuser";
        _tokenStore.Roles.Add("Admin");
        _tokenStore.Roles.Add("User");

        // Act
        _tokenStore.Clear();

        // Assert
        _tokenStore.Token.Should().BeEmpty();
        _tokenStore.Expiration.Should().Be(DateTime.MinValue);
        _tokenStore.RefreshToken.Should().BeNull();
        _tokenStore.UserName.Should().BeEmpty();
        _tokenStore.Roles.Should().BeEmpty();
        _tokenStore.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void TokenStore_ConRoles_DeberiaMantenerLista()
    {
        // Arrange & Act
        _tokenStore.Roles.Add("Admin");
        _tokenStore.Roles.Add("Manager");
        _tokenStore.Roles.Add("User");

        // Assert
        _tokenStore.Roles.Should().HaveCount(3);
        _tokenStore.Roles.Should().Contain("Admin");
        _tokenStore.Roles.Should().Contain("Manager");
        _tokenStore.Roles.Should().Contain("User");
    }

    [Fact]
    public void TokenStore_ConRefreshToken_DeberiaAlmacenarCorrectamente()
    {
        // Arrange & Act
        _tokenStore.RefreshToken = "refresh-token-123";

        // Assert
        _tokenStore.RefreshToken.Should().Be("refresh-token-123");
    }

    [Fact]
    public void TokenStore_ConUserName_DeberiaAlmacenarCorrectamente()
    {
        // Arrange & Act
        _tokenStore.UserName = "john.doe";

        // Assert
        _tokenStore.UserName.Should().Be("john.doe");
    }

    // ===== PRUEBAS DE CASOS EDGE =====

    [Fact]
    public async Task SendAsync_ConTokenMuyLargo_DeberiaManejarCorrectamente()
    {
        // Arrange
        var longToken = new string('A', 10000); // Token muy largo
        _tokenStore.Token = longToken;
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/test");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization!.Parameter.Should().Be(longToken);
        request.Headers.GetValues("X-Bearer-Token").First().Should().Be(longToken);
    }

    [Fact]
    public async Task SendAsync_ConTokenConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange
        var specialToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        _tokenStore.Token = specialToken;
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/test");
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        request.Headers.Authorization!.Parameter.Should().Be(specialToken);
        request.Headers.GetValues("X-Bearer-Token").First().Should().Be(specialToken);
    }

    [Fact]
    public async Task SendAsync_ConCancellationToken_DeberiaPasarCorrectamente()
    {
        // Arrange
        _tokenStore.Token = "test-token";
        _tokenStore.Expiration = DateTime.UtcNow.AddHours(1);

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test.com/test");
        var cancellationToken = new CancellationToken();
        
        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var response = await _httpClient.SendAsync(request, cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _innerHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }
}
