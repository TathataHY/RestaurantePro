using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using RestaurantePro.Web.Admin.Auth;
using RestaurantePro.Web.Admin.Models;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Auth;

public class JwtAuthenticationStateProviderTests
{
    private readonly TokenStore _tokenStore;
    private readonly JwtAuthenticationStateProvider _provider;

    public JwtAuthenticationStateProviderTests()
    {
        _tokenStore = new TokenStore();
        _provider = new JwtAuthenticationStateProvider(_tokenStore);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task GetAuthenticationStateAsync_ConTokenValido_DeberiaRetornarUsuarioAutenticado()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "valid-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = "testuser",
            Roles = new List<string> { "Admin", "Manager" }
        };

        _provider.SetAuth(authResponse);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeTrue();
        authState.User.Identity.Name.Should().Be("testuser");
        authState.User.IsInRole("Admin").Should().BeTrue();
        authState.User.IsInRole("Manager").Should().BeTrue();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ConTokenExpirado_DeberiaRetornarUsuarioNoAutenticado()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "expired-token",
            Expiration = DateTime.UtcNow.AddHours(-1), // Token expirado
            UserName = "testuser",
            Roles = new List<string> { "Admin" }
        };

        _provider.SetAuth(authResponse);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_SinToken_DeberiaRetornarUsuarioNoAutenticado()
    {
        // Arrange - TokenStore vacío

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void SetAuth_ConDatosValidos_DeberiaConfigurarTokenStore()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "new-token",
            Expiration = DateTime.UtcNow.AddHours(2),
            RefreshToken = "refresh-token",
            UserName = "newuser",
            Roles = new List<string> { "User", "Editor" }
        };

        // Act
        _provider.SetAuth(authResponse);

        // Assert
        _tokenStore.Token.Should().Be("new-token");
        _tokenStore.Expiration.Should().Be(authResponse.Expiration);
        _tokenStore.RefreshToken.Should().Be("refresh-token");
        _tokenStore.UserName.Should().Be("newuser");
        _tokenStore.Roles.Should().BeEquivalentTo(new List<string> { "User", "Editor" });
    }

    [Fact]
    public void Logout_DeberiaLimpiarTokenStore()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "token",
            Expiration = DateTime.UtcNow.AddHours(1),
            RefreshToken = "refresh",
            UserName = "user",
            Roles = new List<string> { "Admin" }
        };

        _provider.SetAuth(authResponse);

        // Act
        _provider.Logout();

        // Assert
        _tokenStore.Token.Should().BeEmpty();
        _tokenStore.Expiration.Should().Be(DateTime.MinValue);
        _tokenStore.RefreshToken.Should().BeNull();
        _tokenStore.UserName.Should().BeEmpty();
        _tokenStore.Roles.Should().BeEmpty();
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public async Task GetAuthenticationStateAsync_ConRolesVacios_DeberiaManejarCorrectamente()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "valid-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = "testuser",
            Roles = new List<string>() // Lista vacía
        };

        _provider.SetAuth(authResponse);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeTrue();
        authState.User.Identity.Name.Should().Be("testuser");
        authState.User.IsInRole("Admin").Should().BeFalse();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ConRolesNulos_DeberiaLanzarExcepcion()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "valid-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = "testuser",
            Roles = null // Roles nulos
        };

        // Act & Assert
        var action = () => _provider.SetAuth(authResponse);
        action.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void SetAuth_ConDatosNulos_DeberiaLanzarExcepcion()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = null,
            Expiration = DateTime.MinValue,
            RefreshToken = null,
            UserName = null,
            Roles = null
        };

        // Act & Assert
        var action = () => _provider.SetAuth(authResponse);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ConTokenEnBlanco_DeberiaRetornarUsuarioNoAutenticado()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "", // Token vacío
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = "testuser",
            Roles = new List<string> { "Admin" }
        };

        _provider.SetAuth(authResponse);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ConTokenSoloEspacios_DeberiaRetornarUsuarioNoAutenticado()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "   ", // Token solo espacios
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = "testuser",
            Roles = new List<string> { "Admin" }
        };

        _provider.SetAuth(authResponse);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeFalse();
    }

    // ===== PRUEBAS ROBUSTAS - SEGURIDAD =====

    [Fact]
    public async Task GetAuthenticationStateAsync_ConRolesMaliciosos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "valid-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = "testuser",
            Roles = new List<string> { "<script>alert('xss')</script>", "Admin", "'; DROP TABLE Users; --" }
        };

        _provider.SetAuth(authResponse);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeTrue();
        authState.User.IsInRole("<script>alert('xss')</script>").Should().BeTrue();
        authState.User.IsInRole("'; DROP TABLE Users; --").Should().BeTrue();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ConUserNameMalicioso_DeberiaManejarCorrectamente()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "valid-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = "<script>alert('xss')</script>",
            Roles = new List<string> { "Admin" }
        };

        _provider.SetAuth(authResponse);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeTrue();
        authState.User.Identity.Name.Should().Be("<script>alert('xss')</script>");
    }

    // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

    [Fact]
    public async Task GetAuthenticationStateAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = "valid-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = "testuser",
            Roles = new List<string> { "Admin" }
        };

        _provider.SetAuth(authResponse);

        // Act
        var tasks = Enumerable.Range(1, 10).Select(_ => _provider.GetAuthenticationStateAsync()).ToArray();
        var authStates = await Task.WhenAll(tasks);

        // Assert
        authStates.Should().HaveCount(10);
        foreach (var authState in authStates)
        {
            authState.User.Identity.Should().NotBeNull();
            authState.User.Identity!.IsAuthenticated.Should().BeTrue();
            authState.User.Identity.Name.Should().Be("testuser");
        }
    }

    [Fact]
    public void SetAuth_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var authResponses = Enumerable.Range(1, 5).Select(i => new AuthResponse
        {
            Token = $"token-{i}",
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = $"user-{i}",
            Roles = new List<string> { $"Role-{i}" }
        }).ToArray();

        // Act
        Parallel.ForEach(authResponses, authResponse => _provider.SetAuth(authResponse));

        // Assert
        _tokenStore.Token.Should().NotBeEmpty();
        _tokenStore.UserName.Should().NotBeEmpty();
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO Y LÍMITES =====

    [Fact]
    public async Task GetAuthenticationStateAsync_ConMuchosRoles_DeberiaManejarCorrectamente()
    {
        // Arrange
        var roles = Enumerable.Range(1, 1000).Select(i => $"Role{i}").ToList();
        var authResponse = new AuthResponse
        {
            Token = "valid-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = "testuser",
            Roles = roles
        };

        _provider.SetAuth(authResponse);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeTrue();
        authState.User.Identity.Name.Should().Be("testuser");
        authState.User.IsInRole("Role1").Should().BeTrue();
        authState.User.IsInRole("Role1000").Should().BeTrue();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ConUserNameMuyLargo_DeberiaManejarCorrectamente()
    {
        // Arrange
        var longUserName = new string('A', 10000);
        var authResponse = new AuthResponse
        {
            Token = "valid-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = longUserName,
            Roles = new List<string> { "Admin" }
        };

        _provider.SetAuth(authResponse);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeTrue();
        authState.User.Identity.Name.Should().Be(longUserName);
    }

    [Fact]
    public void SetAuth_ConDatosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange
        var authResponse = new AuthResponse
        {
            Token = new string('A', 10000), // Token muy largo
            Expiration = DateTime.MaxValue, // Fecha máxima
            RefreshToken = new string('B', 10000), // Refresh token muy largo
            UserName = new string('C', 10000), // Username muy largo
            Roles = Enumerable.Range(1, 1000).Select(i => new string('D', 100)).ToList() // Muchos roles largos
        };

        // Act
        _provider.SetAuth(authResponse);

        // Assert
        _tokenStore.Token.Should().HaveLength(10000);
        _tokenStore.Expiration.Should().Be(DateTime.MaxValue);
        _tokenStore.RefreshToken.Should().HaveLength(10000);
        _tokenStore.UserName.Should().HaveLength(10000);
        _tokenStore.Roles.Should().HaveCount(1000);
    }
}
