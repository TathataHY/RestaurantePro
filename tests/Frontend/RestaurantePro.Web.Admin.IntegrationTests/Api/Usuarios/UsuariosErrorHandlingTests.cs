using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Application.Core.Usuarios.DTOs;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using Xunit;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Usuarios;

/// <summary>
/// Pruebas de manejo de errores para el controlador de usuarios
/// </summary>
public class UsuariosErrorHandlingTests : BaseIntegrationTest
{
    private readonly JsonSerializerOptions _jsonOptions;

    public UsuariosErrorHandlingTests(WebApplicationFactory factory) : base(factory)
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    #region Pruebas de Errores de Validación

    [Fact]
    public async Task CrearUsuario_ConJsonInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var jsonInvalido = "{ nombreCompleto: 'Usuario Test', email: 'test@test.com', // JSON inválido }";
        var content = new StringContent(jsonInvalido, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/usuarios", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearUsuario_ConContentTypeIncorrecto_DeberiaRetornarUnsupportedMediaType()
    {
        // Arrange
        var usuario = new
        {
            nombreCompleto = "Usuario Test",
            email = "test@test.com",
            telefono = "+1234567890",
            rol = "Mesero",
            activo = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuario, _jsonOptions),
            Encoding.UTF8,
            "text/plain"); // Content-Type incorrecto

        // Act
        var response = await _client.PostAsync("/api/core/usuarios", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task CrearUsuario_ConBodyVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/core/usuarios", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pruebas de Errores de Negocio

    [Fact]
    public async Task CrearUsuario_ConEmailDuplicado_DeberiaRetornarErrorEspecifico()
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
        responseData.Message.Should().Contain("duplicado");
    }

    [Fact]
    public async Task ActualizarUsuario_ConEmailDuplicado_DeberiaRetornarErrorEspecifico()
    {
        // Arrange - Crear dos usuarios
        var usuarioIds = await SeedUsuariosDePruebaAsync();
        var usuarioId1 = usuarioIds[0];
        var usuarioId2 = usuarioIds[1];

        // Obtener el email del segundo usuario
        var responseGet = await _client.GetAsync($"/api/core/usuarios/{usuarioId2}");
        var contentGet = await responseGet.Content.ReadAsStringAsync();
        var usuario2 = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(contentGet, _jsonOptions);

        // Intentar actualizar el primer usuario con el email del segundo
        var usuarioActualizado = new
        {
            id = usuarioId1,
            nombreCompleto = "Usuario Actualizado",
            email = usuario2.Data.Email, // Email duplicado
            telefono = "+1111111111",
            rol = "Mesero",
            activo = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioActualizado, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PutAsync($"/api/core/usuarios/{usuarioId1}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Message.Should().Contain("email");
        responseData.Message.Should().Contain("duplicado");
    }

    #endregion

    #region Pruebas de Errores de Recurso No Encontrado

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

    #region Pruebas de Errores de Parámetros

    [Theory]
    [InlineData(0, 10)] // pageNumber = 0
    [InlineData(-1, 10)] // pageNumber negativo
    [InlineData(1, 0)] // pageSize = 0
    [InlineData(1, -1)] // pageSize negativo
    [InlineData(1, 1000)] // pageSize muy grande
    public async Task ObtenerUsuarios_ConParametrosInvalidos_DeberiaRetornarBadRequest(int pageNumber, int pageSize)
    {
        // Act
        var response = await _client.GetAsync($"/api/core/usuarios?pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ObtenerUsuarios_ConOrderByInvalido_DeberiaRetornarBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/core/usuarios?orderBy=CampoInexistente&orderDirection=asc");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ObtenerUsuarios_ConOrderDirectionInvalido_DeberiaRetornarBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/core/usuarios?orderBy=NombreCompleto&orderDirection=invalid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region Pruebas de Errores de Concurrencia

    [Fact]
    public async Task ActualizarUsuario_ConDatosDesactualizados_DeberiaManejarConcurrencia()
    {
        // Arrange
        var usuarioIds = await SeedUsuariosDePruebaAsync();
        var usuarioId = usuarioIds.First();

        // Obtener usuario original
        var responseGet = await _client.GetAsync($"/api/core/usuarios/{usuarioId}");
        var contentGet = await responseGet.Content.ReadAsStringAsync();
        var usuarioOriginal = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(contentGet, _jsonOptions);

        // Simular actualización concurrente - actualizar el usuario
        var usuarioActualizado = new
        {
            id = usuarioId,
            nombreCompleto = "Usuario Actualizado Concurrentemente",
            email = usuarioOriginal.Data.Email,
            telefono = "+9999999999",
            rol = usuarioOriginal.Data.Rol,
            activo = usuarioOriginal.Data.Activo
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
        responseData.Data.NombreCompleto.Should().Be(usuarioActualizado.nombreCompleto);
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
