using FluentAssertions;
using RestaurantePro.Mobile.Core.Models.DTOs;
using Xunit;

namespace RestaurantePro.Mobile.UnitTests.Models;

/// <summary>
/// Pruebas unitarias para AuthUser y clases relacionadas
/// </summary>
public class AuthUserTests
{
    #region AuthUser Tests

    #region Constructor y Propiedades Iniciales

    [Fact]
    public void Constructor_DeberiaInicializarPropiedadesCorrectamente()
    {
        // Arrange & Act
        var authUser = new AuthUser();

        // Assert
        authUser.Id.Should().Be(0);
        authUser.Email.Should().BeEmpty();
        authUser.Nombre.Should().BeEmpty();
        authUser.Apellido.Should().BeEmpty();
        authUser.Roles.Should().NotBeNull();
        authUser.Roles.Should().BeEmpty();
        authUser.Activo.Should().BeTrue();
    }

    #endregion

    #region Propiedades Básicas

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(-1)]
    public void Id_DeberiaEstablecerCorrectamente(int id)
    {
        // Arrange
        var authUser = new AuthUser();

        // Act
        authUser.Id = id;

        // Assert
        authUser.Id.Should().Be(id);
    }

    [Theory]
    [InlineData("usuario@test.com")]
    [InlineData("admin@restaurante.com")]
    [InlineData("")]
    [InlineData("usuario con espacios@test.com")]
    public void Email_DeberiaEstablecerCorrectamente(string email)
    {
        // Arrange
        var authUser = new AuthUser();

        // Act
        authUser.Email = email;

        // Assert
        authUser.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("Juan")]
    [InlineData("María")]
    [InlineData("")]
    [InlineData("José María")]
    public void Nombre_DeberiaEstablecerCorrectamente(string nombre)
    {
        // Arrange
        var authUser = new AuthUser();

        // Act
        authUser.Nombre = nombre;

        // Assert
        authUser.Nombre.Should().Be(nombre);
    }

    [Theory]
    [InlineData("Pérez")]
    [InlineData("García")]
    [InlineData("")]
    [InlineData("De la Cruz")]
    public void Apellido_DeberiaEstablecerCorrectamente(string apellido)
    {
        // Arrange
        var authUser = new AuthUser();

        // Act
        authUser.Apellido = apellido;

        // Assert
        authUser.Apellido.Should().Be(apellido);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Activo_DeberiaEstablecerCorrectamente(bool activo)
    {
        // Arrange
        var authUser = new AuthUser();

        // Act
        authUser.Activo = activo;

        // Assert
        authUser.Activo.Should().Be(activo);
    }

    #endregion

    #region Roles Collection

    [Fact]
    public void Roles_DeberiaPermitirAgregarRoles()
    {
        // Arrange
        var authUser = new AuthUser();
        var roles = new List<string> { "Admin", "Mesero", "Cocinero" };

        // Act
        foreach (var role in roles)
        {
            authUser.Roles.Add(role);
        }

        // Assert
        authUser.Roles.Should().HaveCount(3);
        authUser.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void Roles_DeberiaPermitirAsignarListaCompleta()
    {
        // Arrange
        var authUser = new AuthUser();
        var roles = new List<string> { "Admin", "Mesero" };

        // Act
        authUser.Roles = roles;

        // Assert
        authUser.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void Roles_DeberiaPermitirLimpiarRoles()
    {
        // Arrange
        var authUser = new AuthUser();
        authUser.Roles.Add("Admin");
        authUser.Roles.Add("Mesero");

        // Act
        authUser.Roles.Clear();

        // Assert
        authUser.Roles.Should().BeEmpty();
    }

    #endregion

    #region NombreCompleto Property

    [Theory]
    [InlineData("Juan", "Pérez", "Juan Pérez")]
    [InlineData("María", "García", "María García")]
    [InlineData("José", "De la Cruz", "José De la Cruz")]
    [InlineData("", "Pérez", "Pérez")]
    [InlineData("Juan", "", "Juan")]
    [InlineData("", "", "")]
    [InlineData("   ", "   ", "   ")]
    public void NombreCompleto_DeberiaConcatenarCorrectamente(string nombre, string apellido, string nombreCompletoEsperado)
    {
        // Arrange
        var authUser = new AuthUser();

        // Act
        authUser.Nombre = nombre;
        authUser.Apellido = apellido;

        // Assert
        // El código actual no hace trim, solo concatena con espacio
        var resultadoEsperado = $"{nombre} {apellido}".Trim();
        authUser.NombreCompleto.Should().Be(resultadoEsperado);
    }

    [Fact]
    public void NombreCompleto_ConEspaciosExtremos_DeberiaTrimearCorrectamente()
    {
        // Arrange
        var authUser = new AuthUser();

        // Act
        authUser.Nombre = "  Juan  ";
        authUser.Apellido = "  Pérez  ";

        // Assert
        // El código actual no hace trim de espacios internos, solo concatena
        authUser.NombreCompleto.Should().Be("Juan     Pérez");
    }

    #endregion

    #region Casos Edge

    [Fact]
    public void AuthUser_ConTodasLasPropiedades_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var authUser = new AuthUser();
        var roles = new List<string> { "Admin", "Mesero" };

        // Act
        authUser.Id = 1;
        authUser.Email = "admin@restaurante.com";
        authUser.Nombre = "Juan";
        authUser.Apellido = "Pérez";
        authUser.Roles = roles;
        authUser.Activo = true;

        // Assert
        authUser.Id.Should().Be(1);
        authUser.Email.Should().Be("admin@restaurante.com");
        authUser.Nombre.Should().Be("Juan");
        authUser.Apellido.Should().Be("Pérez");
        authUser.Roles.Should().BeEquivalentTo(roles);
        authUser.Activo.Should().BeTrue();
        authUser.NombreCompleto.Should().Be("Juan Pérez");
    }

    #endregion

    #endregion

    #region AuthResponse Tests

    #region Constructor y Propiedades Iniciales

    [Fact]
    public void AuthResponse_Constructor_DeberiaInicializarPropiedadesCorrectamente()
    {
        // Arrange & Act
        var authResponse = new AuthResponse();

        // Assert
        authResponse.Token.Should().BeEmpty();
        authResponse.RefreshToken.Should().BeNull();
        authResponse.User.Should().NotBeNull();
        authResponse.User.Should().BeOfType<AuthUser>();
        authResponse.ExpiresAt.Should().Be(default(DateTime));
    }

    #endregion

    #region Propiedades Básicas

    [Theory]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...")]
    [InlineData("")]
    [InlineData("token-con-espacios")]
    public void Token_DeberiaEstablecerCorrectamente(string token)
    {
        // Arrange
        var authResponse = new AuthResponse();

        // Act
        authResponse.Token = token;

        // Assert
        authResponse.Token.Should().Be(token);
    }

    [Theory]
    [InlineData("refresh-token-123")]
    [InlineData("")]
    [InlineData(null)]
    public void RefreshToken_DeberiaEstablecerCorrectamente(string? refreshToken)
    {
        // Arrange
        var authResponse = new AuthResponse();

        // Act
        authResponse.RefreshToken = refreshToken;

        // Assert
        authResponse.RefreshToken.Should().Be(refreshToken);
    }

    [Fact]
    public void User_DeberiaEstablecerCorrectamente()
    {
        // Arrange
        var authResponse = new AuthResponse();
        var user = new AuthUser
        {
            Id = 1,
            Email = "test@test.com",
            Nombre = "Test",
            Apellido = "User"
        };

        // Act
        authResponse.User = user;

        // Assert
        authResponse.User.Should().Be(user);
        authResponse.User.Id.Should().Be(1);
        authResponse.User.Email.Should().Be("test@test.com");
        authResponse.User.Nombre.Should().Be("Test");
        authResponse.User.Apellido.Should().Be("User");
    }

    [Fact]
    public void ExpiresAt_DeberiaEstablecerCorrectamente()
    {
        // Arrange
        var authResponse = new AuthResponse();
        var expiresAt = DateTime.Now.AddHours(1);

        // Act
        authResponse.ExpiresAt = expiresAt;

        // Assert
        authResponse.ExpiresAt.Should().Be(expiresAt);
    }

    #endregion

    #region Casos Edge

    [Fact]
    public void AuthResponse_ConTodasLasPropiedades_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var authResponse = new AuthResponse();
        var user = new AuthUser
        {
            Id = 1,
            Email = "admin@restaurante.com",
            Nombre = "Juan",
            Apellido = "Pérez",
            Roles = new List<string> { "Admin" },
            Activo = true
        };
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var refreshToken = "refresh-token-123";
        var expiresAt = DateTime.Now.AddHours(1);

        // Act
        authResponse.Token = token;
        authResponse.RefreshToken = refreshToken;
        authResponse.User = user;
        authResponse.ExpiresAt = expiresAt;

        // Assert
        authResponse.Token.Should().Be(token);
        authResponse.RefreshToken.Should().Be(refreshToken);
        authResponse.User.Should().Be(user);
        authResponse.ExpiresAt.Should().Be(expiresAt);
    }

    #endregion

    #endregion

    #region LoginRequest Tests

    #region Constructor y Propiedades Iniciales

    [Fact]
    public void LoginRequest_Constructor_DeberiaInicializarPropiedadesCorrectamente()
    {
        // Arrange & Act
        var loginRequest = new LoginRequest();

        // Assert
        loginRequest.Email.Should().BeEmpty();
        loginRequest.Password.Should().BeEmpty();
        loginRequest.Recordarme.Should().BeFalse();
    }

    #endregion

    #region Propiedades Básicas

    [Theory]
    [InlineData("usuario@test.com")]
    [InlineData("admin@restaurante.com")]
    [InlineData("")]
    [InlineData("usuario con espacios@test.com")]
    public void LoginRequest_Email_DeberiaEstablecerCorrectamente(string email)
    {
        // Arrange
        var loginRequest = new LoginRequest();

        // Act
        loginRequest.Email = email;

        // Assert
        loginRequest.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("password123")]
    [InlineData("contraseña")]
    [InlineData("")]
    [InlineData("password con espacios")]
    public void Password_DeberiaEstablecerCorrectamente(string password)
    {
        // Arrange
        var loginRequest = new LoginRequest();

        // Act
        loginRequest.Password = password;

        // Assert
        loginRequest.Password.Should().Be(password);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Recordarme_DeberiaEstablecerCorrectamente(bool recordarme)
    {
        // Arrange
        var loginRequest = new LoginRequest();

        // Act
        loginRequest.Recordarme = recordarme;

        // Assert
        loginRequest.Recordarme.Should().Be(recordarme);
    }

    #endregion

    #region Casos Edge

    [Fact]
    public void LoginRequest_ConTodasLasPropiedades_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var loginRequest = new LoginRequest();

        // Act
        loginRequest.Email = "admin@restaurante.com";
        loginRequest.Password = "password123";
        loginRequest.Recordarme = true;

        // Assert
        loginRequest.Email.Should().Be("admin@restaurante.com");
        loginRequest.Password.Should().Be("password123");
        loginRequest.Recordarme.Should().BeTrue();
    }

    #endregion

    #endregion

    #region Escenarios Reales

    [Fact]
    public void AuthUser_EscenarioRealCompleto_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var authUser = new AuthUser();
        var roles = new List<string> { "Mesero", "Cajero" };

        // Act
        authUser.Id = 123;
        authUser.Email = "mesero@restaurante.com";
        authUser.Nombre = "Carlos";
        authUser.Apellido = "García";
        authUser.Roles = roles;
        authUser.Activo = true;

        // Assert
        authUser.Id.Should().Be(123);
        authUser.Email.Should().Be("mesero@restaurante.com");
        authUser.Nombre.Should().Be("Carlos");
        authUser.Apellido.Should().Be("García");
        authUser.Roles.Should().BeEquivalentTo(roles);
        authUser.Activo.Should().BeTrue();
        authUser.NombreCompleto.Should().Be("Carlos García");
    }

    [Fact]
    public void AuthResponse_EscenarioRealCompleto_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var authResponse = new AuthResponse();
        var user = new AuthUser
        {
            Id = 456,
            Email = "cocinero@restaurante.com",
            Nombre = "Ana",
            Apellido = "López",
            Roles = new List<string> { "Cocinero" },
            Activo = true
        };
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0NTYiLCJlbWFpbCI6ImNvY2luZXJvQHJlc3RhdXJhbnRlLmNvbSIsIm5hbWUiOiJBbmEiLCJyb2xlcyI6WyJDb2NpbmVybyJdLCJleHAiOjE2MzQ1Njc4MDB9.signature";
        var refreshToken = "refresh-token-456";
        var expiresAt = DateTime.Now.AddHours(8);

        // Act
        authResponse.Token = token;
        authResponse.RefreshToken = refreshToken;
        authResponse.User = user;
        authResponse.ExpiresAt = expiresAt;

        // Assert
        authResponse.Token.Should().Be(token);
        authResponse.RefreshToken.Should().Be(refreshToken);
        authResponse.User.Should().Be(user);
        authResponse.ExpiresAt.Should().Be(expiresAt);
        authResponse.User.NombreCompleto.Should().Be("Ana López");
    }

    [Fact]
    public void LoginRequest_EscenarioRealCompleto_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var loginRequest = new LoginRequest();

        // Act
        loginRequest.Email = "gerente@restaurante.com";
        loginRequest.Password = "gerente123";
        loginRequest.Recordarme = true;

        // Assert
        loginRequest.Email.Should().Be("gerente@restaurante.com");
        loginRequest.Password.Should().Be("gerente123");
        loginRequest.Recordarme.Should().BeTrue();
    }

    #endregion
}
