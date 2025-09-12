using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Application.Core.Usuarios.DTOs;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using Xunit;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Usuarios;

/// <summary>
/// Pruebas de integración para operaciones CRUD de usuarios
/// </summary>
public class UsuariosCrudTests : BaseIntegrationTest
{
    private readonly JsonSerializerOptions _jsonOptions;

    public UsuariosCrudTests(WebApplicationFactory factory) : base(factory)
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    #region Pruebas de Creación

    [Fact]
    public async Task CrearUsuario_ConDatosValidos_DeberiaRetornarUsuarioCreado()
    {
        // Arrange
        var nuevoUsuario = new
        {
            nombreCompleto = "Juan Pérez",
            email = "juan.perez@restaurantepro.com",
            telefono = "+1234567890",
            rol = "Mesero",
            activo = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(nuevoUsuario, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/usuarios", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.NombreCompleto.Should().Be(nuevoUsuario.nombreCompleto);
        responseData.Data.Email.Should().Be(nuevoUsuario.email);
        responseData.Data.Rol.Should().Be(nuevoUsuario.rol);
    }

    [Fact]
    public async Task CrearUsuario_ConEmailDuplicado_DeberiaRetornarError()
    {
        // Arrange - Crear primer usuario
        var primerUsuario = new
        {
            nombreCompleto = "Usuario Original",
            email = "duplicado@restaurantepro.com",
            telefono = "+1234567890",
            rol = "Mesero",
            activo = true
        };

        var content1 = new StringContent(
            JsonSerializer.Serialize(primerUsuario, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        await _client.PostAsync("/api/core/usuarios", content1);

        // Arrange - Intentar crear segundo usuario con mismo email
        var segundoUsuario = new
        {
            nombreCompleto = "Usuario Duplicado",
            email = "duplicado@restaurantepro.com",
            telefono = "+0987654321",
            rol = "Cocinero",
            activo = true
        };

        var content2 = new StringContent(
            JsonSerializer.Serialize(segundoUsuario, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/usuarios", content2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("email");
    }

    [Fact]
    public async Task CrearUsuario_ConDatosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var usuarioInvalido = new
        {
            nombreCompleto = "", // Nombre vacío
            email = "email-invalido", // Email inválido
            telefono = "", // Teléfono vacío
            rol = "RolInexistente", // Rol inválido
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

    #region Pruebas de Consulta

    [Fact]
    public async Task ObtenerUsuarios_ConParametrosValidos_DeberiaRetornarListaPaginada()
    {
        // Arrange - Crear usuarios de prueba
        await SeedUsuariosDePruebaAsync();

        // Act
        var response = await _client.GetAsync("/api/core/usuarios?pageNumber=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<List<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task ObtenerUsuario_ConIdValido_DeberiaRetornarUsuario()
    {
        // Arrange
        var usuarioIds = await SeedUsuariosDePruebaAsync();
        var usuarioId = usuarioIds.First();

        // Act
        var response = await _client.GetAsync($"/api/core/usuarios/{usuarioId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Id.Should().Be(usuarioId);
    }

    [Fact]
    public async Task ObtenerUsuario_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/core/usuarios/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("no encontrado");
    }

    #endregion

    #region Pruebas de Actualización

    [Fact]
    public async Task ActualizarUsuario_ConDatosValidos_DeberiaRetornarUsuarioActualizado()
    {
        // Arrange
        var usuarioIds = await SeedUsuariosDePruebaAsync();
        var usuarioId = usuarioIds.First();

        var usuarioActualizado = new
        {
            id = usuarioId,
            nombreCompleto = "Juan Pérez Actualizado",
            email = "juan.actualizado@restaurantepro.com",
            telefono = "+1111111111",
            rol = "Supervisor",
            activo = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioActualizado, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/usuarios/{usuarioId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.NombreCompleto.Should().Be(usuarioActualizado.nombreCompleto);
        responseData.Data.Email.Should().Be(usuarioActualizado.email);
    }

    [Fact]
    public async Task ActualizarUsuario_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var usuarioActualizado = new
        {
            id = idInexistente,
            nombreCompleto = "Usuario Inexistente",
            email = "inexistente@restaurantepro.com",
            telefono = "+1234567890",
            rol = "Mesero",
            activo = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioActualizado, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/usuarios/{idInexistente}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("no encontrado");
    }

    #endregion

    #region Pruebas de Eliminación

    [Fact]
    public async Task EliminarUsuario_ConIdValido_DeberiaRetornarOk()
    {
        // Arrange
        var usuarioIds = await SeedUsuariosDePruebaAsync();
        var usuarioId = usuarioIds.First();

        // Act
        var response = await _client.DeleteAsync($"/api/core/usuarios/{usuarioId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<bool>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().BeTrue();
    }

    [Fact]
    public async Task EliminarUsuario_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/core/usuarios/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("no encontrado");
    }

    #endregion

    #region Pruebas de Cambio de Rol

    [Fact]
    public async Task CambiarRol_ConRolValido_DeberiaRetornarUsuarioActualizado()
    {
        // Arrange
        var usuarioIds = await SeedUsuariosDePruebaAsync();
        var usuarioId = usuarioIds.First();

        var cambioRol = new
        {
            nuevoRol = "Supervisor"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(cambioRol, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/usuarios/{usuarioId}/rol", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.Rol.Should().Be(cambioRol.nuevoRol);
    }

    #endregion

    #region Métodos de Ayuda

    private async Task<List<Guid>> SeedUsuariosDePruebaAsync()
    {
        var usuarios = new[]
        {
            new
            {
                nombreCompleto = "María García",
                email = "maria.garcia@restaurantepro.com",
                telefono = "+1111111111",
                rol = "Mesero",
                activo = true
            },
            new
            {
                nombreCompleto = "Carlos López",
                email = "carlos.lopez@restaurantepro.com",
                telefono = "+2222222222",
                rol = "Cocinero",
                activo = true
            },
            new
            {
                nombreCompleto = "Ana Martínez",
                email = "ana.martinez@restaurantepro.com",
                telefono = "+3333333333",
                rol = "Cajero",
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
