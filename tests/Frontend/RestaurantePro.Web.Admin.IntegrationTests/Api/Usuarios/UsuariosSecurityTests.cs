using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Application.Core.Usuarios.DTOs;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using Xunit;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Usuarios;

/// <summary>
/// Pruebas de seguridad para el controlador de usuarios
/// </summary>
public class UsuariosSecurityTests : BaseIntegrationTest
{
    private readonly JsonSerializerOptions _jsonOptions;

    public UsuariosSecurityTests(WebApplicationFactory factory) : base(factory)
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    #region Pruebas de Autorización

    [Fact]
    public async Task ObtenerUsuarios_SinAutorizacion_DeberiaRetornarUnauthorized()
    {
        // Arrange - Crear cliente sin autorización
        var clientNoAuth = _factory.CreateClient();

        // Act
        var response = await clientNoAuth.GetAsync("/api/core/usuarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CrearUsuario_SinAutorizacion_DeberiaRetornarUnauthorized()
    {
        // Arrange
        var clientNoAuth = _factory.CreateClient();
        var nuevoUsuario = new
        {
            nombreCompleto = "Usuario No Autorizado",
            email = "noauth@restaurantepro.com",
            telefono = "+1234567890",
            rol = "Mesero",
            activo = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(nuevoUsuario, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await clientNoAuth.PostAsync("/api/core/usuarios", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Pruebas de SQL Injection

    [Theory]
    [InlineData("'; DROP TABLE Usuarios; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("'; INSERT INTO Usuarios VALUES ('hacker', 'hacker@evil.com'); --")]
    [InlineData("' UNION SELECT * FROM Usuarios --")]
    [InlineData("'; UPDATE Usuarios SET Rol='Administrador' WHERE Email='admin@test.com'; --")]
    public async Task BuscarUsuarios_ConSQLInjection_DeberiaEscaparCorrectamente(string sqlInjection)
    {
        // Arrange
        await SeedUsuariosDePruebaAsync();

        // Act
        var response = await _client.GetAsync($"/api/core/usuarios?filtro={Uri.EscapeDataString(sqlInjection)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        
        // Verificar que no se ejecutó código SQL malicioso
        content.Should().NotContain("SQL");
        content.Should().NotContain("exception");
        content.Should().NotContain("syntax error");
        content.Should().NotContain("database error");
        content.Should().NotContain("DROP TABLE");
        content.Should().NotContain("INSERT INTO");
        content.Should().NotContain("UPDATE");
        content.Should().NotContain("UNION SELECT");
    }

    [Theory]
    [InlineData("'; DROP TABLE Usuarios; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("'; INSERT INTO Usuarios VALUES ('hacker', 'hacker@evil.com'); --")]
    public async Task CrearUsuario_ConSQLInjectionEnEmail_DeberiaValidarCorrectamente(string sqlInjection)
    {
        // Arrange
        var usuarioConSQLInjection = new
        {
            nombreCompleto = "Usuario SQL Injection",
            email = sqlInjection,
            telefono = "+1234567890",
            rol = "Mesero",
            activo = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioConSQLInjection, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/usuarios", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region Pruebas de XSS (Cross-Site Scripting)

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("javascript:alert('XSS')")]
    [InlineData("<img src=x onerror=alert('XSS')>")]
    [InlineData("';alert('XSS');//")]
    public async Task CrearUsuario_ConXSSEnNombre_DeberiaEscaparCorrectamente(string xssPayload)
    {
        // Arrange
        var usuarioConXSS = new
        {
            nombreCompleto = xssPayload,
            email = "xss@restaurantepro.com",
            telefono = "+1234567890",
            rol = "Mesero",
            activo = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioConXSS, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/usuarios", content);

        // Assert
        // Debería crear el usuario pero con el contenido escapado
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        
        // Verificar que el contenido XSS está escapado
        responseData.Data.NombreCompleto.Should().NotContain("<script>");
        responseData.Data.NombreCompleto.Should().NotContain("javascript:");
        responseData.Data.NombreCompleto.Should().NotContain("onerror=");
    }

    #endregion

    #region Pruebas de Validación de Entrada

    [Theory]
    [InlineData("", "email@test.com", "+1234567890", "Mesero")] // Nombre vacío
    [InlineData("Usuario Test", "", "+1234567890", "Mesero")] // Email vacío
    [InlineData("Usuario Test", "email-invalido", "+1234567890", "Mesero")] // Email inválido
    [InlineData("Usuario Test", "email@test.com", "", "Mesero")] // Teléfono vacío
    [InlineData("Usuario Test", "email@test.com", "+1234567890", "")] // Rol vacío
    [InlineData("Usuario Test", "email@test.com", "+1234567890", "RolInexistente")] // Rol inválido
    public async Task CrearUsuario_ConDatosInvalidos_DeberiaRetornarBadRequest(
        string nombreCompleto, string email, string telefono, string rol)
    {
        // Arrange
        var usuarioInvalido = new
        {
            nombreCompleto,
            email,
            telefono,
            rol,
            activo = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioInvalido, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/usuarios", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region Pruebas de Límites de Entrada

    [Fact]
    public async Task CrearUsuario_ConNombreMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var nombreMuyLargo = new string('A', 1000); // Nombre de 1000 caracteres
        var usuarioConNombreLargo = new
        {
            nombreCompleto = nombreMuyLargo,
            email = "nombre-largo@restaurantepro.com",
            telefono = "+1234567890",
            rol = "Mesero",
            activo = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioConNombreLargo, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/usuarios", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CrearUsuario_ConEmailMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var emailMuyLargo = new string('a', 300) + "@restaurantepro.com"; // Email muy largo
        var usuarioConEmailLargo = new
        {
            nombreCompleto = "Usuario Email Largo",
            email = emailMuyLargo,
            telefono = "+1234567890",
            rol = "Mesero",
            activo = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioConEmailLargo, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/usuarios", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region Métodos de Ayuda

    private async Task<List<Guid>> SeedUsuariosDePruebaAsync()
    {
        var usuarios = new[]
        {
            new
            {
                nombreCompleto = "Usuario Prueba 1",
                email = "usuario1@restaurantepro.com",
                telefono = "+1111111111",
                rol = "Mesero",
                activo = true
            },
            new
            {
                nombreCompleto = "Usuario Prueba 2",
                email = "usuario2@restaurantepro.com",
                telefono = "+2222222222",
                rol = "Cocinero",
                activo = true
            }
        };

        var usuarioIds = new List<Guid>();

        foreach (var usuario in usuarios)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(usuario, _jsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/core/usuarios", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(responseContent, _jsonOptions);
            usuarioIds.Add(responseData.Data.Id);
        }

        return usuarioIds;
    }

    #endregion
}
