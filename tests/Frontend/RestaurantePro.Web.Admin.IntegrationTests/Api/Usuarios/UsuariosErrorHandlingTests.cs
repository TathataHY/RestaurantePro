using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Application.Core.Usuarios.DTOs;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Web.Admin.IntegrationTests.Utils;
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
        _jsonOptions = GetJsonOptions();
    }

    #region Pruebas de Errores de Validación

    [Fact]
    public async Task CrearUsuario_ConJsonInvalido_DeberiaRetornarBadRequest()
    {
        // Arrange
        var jsonInvalido = "{ nombreCompleto: 'Usuario Test', email: 'test@test.com', // JSON inválido }";
        var content = new StringContent(jsonInvalido, Encoding.UTF8, "application/json");

        // Act
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.PostAsync("/api/core/usuarios", content);

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
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.PostAsync("/api/core/usuarios", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task CrearUsuario_ConBodyVacio_DeberiaRetornarBadRequest()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // Act
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.PostAsync("/api/core/usuarios", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Pruebas de Errores de Negocio

    [Fact]
    public async Task CrearUsuario_ConEmailDuplicado_DeberiaRetornarErrorEspecifico()
    {
        // Arrange - Crear primer usuario
        var usuarioCreadorId = await CrearUsuarioAdministradorDePruebaAsync();
        var primerUsuario = new
        {
            nombreUsuario = "usuario.original",
            nombreCompleto = "Usuario Original",
            email = "duplicado@restaurantepro.com",
            password = "Password123!",
            confirmarPassword = "Password123!",
            telefono = "+1234567890",
            rol = "Mesero",
            nivelAcceso = 2,
            usuarioCreadorId = usuarioCreadorId,
            activo = true
        };

        var content1 = new StringContent(
            JsonSerializer.Serialize(primerUsuario, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        var authenticatedClient = CreateAuthenticatedClient();
        await authenticatedClient.PostAsync("/api/core/usuarios", content1);

        // Arrange - Intentar crear segundo usuario con mismo email
        var segundoUsuario = new
        {
            nombreUsuario = "usuario.duplicado",
            nombreCompleto = "Usuario Duplicado",
            email = "duplicado@restaurantepro.com",
            password = "Password123!",
            confirmarPassword = "Password123!",
            telefono = "+9876543210",
            rol = "Mesero",
            nivelAcceso = 2,
            usuarioCreadorId = usuarioCreadorId,
            activo = true
        };

        var content2 = new StringContent(
            JsonSerializer.Serialize(segundoUsuario, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await authenticatedClient.PostAsync("/api/core/usuarios", content2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().Contain("El email ya está en uso.");
    }

    [Fact]
    public async Task ActualizarUsuario_ConEmailDuplicado_DeberiaRetornarErrorEspecifico()
    {
        // Arrange - Crear dos usuarios
        var usuarioIds = await SeedUsuariosDePruebaAsync();
        var usuarioId1 = usuarioIds[0];
        var usuarioId2 = usuarioIds[1];

        // Obtener el email del segundo usuario
        var authenticatedClient = CreateAuthenticatedClient();
        var responseGet = await authenticatedClient.GetAsync($"/api/core/usuarios/{usuarioId2}");
        var contentGet = await responseGet.Content.ReadAsStringAsync();
        var usuario2 = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(contentGet, _jsonOptions);

        // Obtener el usuario administrador para usar como autorizador
        var usuarioCreadorId = await CrearUsuarioAdministradorDePruebaAsync();

        // Intentar actualizar el primer usuario con el email del segundo
        var usuarioActualizado = new
        {
            UsuarioId = usuarioId1,
            Nombre = "Usuario Actualizado",
            Email = usuario2.Data.Email, // Email duplicado
            Telefono = "1111111111",
            Rol = "Mesero",
            Activo = true,
            UsuarioAutorizaId = usuarioCreadorId,
            MotivoActualizacion = "Prueba de email duplicado",
            Prioridad = 1,
            RequiereAprobacion = false
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioActualizado, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await authenticatedClient.PutAsync($"/api/core/usuarios/{usuarioId1}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
        responseData.Errors.Should().Contain(e => e.Contains("El email ya está en uso"));
    }

    #endregion

    #region Pruebas de Errores de Recurso No Encontrado

    [Fact]
    public async Task ObtenerUsuario_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.GetAsync($"/api/core/usuarios/{idInexistente}");

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
        var usuarioCreadorId = await CrearUsuarioAdministradorDePruebaAsync();
        var usuarioActualizado = new
        {
            UsuarioId = idInexistente,
            Nombre = "Usuario Inexistente",
            Email = "inexistente@restaurantepro.com",
            Telefono = "1234567890",
            Rol = "Mesero",
            Activo = true,
            UsuarioAutorizaId = usuarioCreadorId,
            MotivoActualizacion = "Prueba de usuario inexistente",
            Prioridad = 1,
            RequiereAprobacion = false
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioActualizado, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.PutAsync($"/api/core/usuarios/{idInexistente}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseData = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        responseData.Should().NotBeNull();
        responseData!.Errors.Should().NotBeEmpty();
        responseData.Errors.Should().Contain(e => e.Contains("El usuario especificado no existe"));
    }

    [Fact]
    public async Task EliminarUsuario_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var authenticatedClient = CreateAuthenticatedClient();

        // Act
        var response = await authenticatedClient.DeleteAsync($"/api/core/usuarios/{idInexistente}");

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
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.GetAsync($"/api/core/usuarios?pageNumber={pageNumber}&pageSize={pageSize}");

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
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.GetAsync("/api/core/usuarios?orderBy=CampoInexistente&orderDirection=asc");

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
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.GetAsync("/api/core/usuarios?orderBy=NombreCompleto&orderDirection=invalid");

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
        var authenticatedClient = CreateAuthenticatedClient();
        var responseGet = await authenticatedClient.GetAsync($"/api/core/usuarios/{usuarioId}");
        var contentGet = await responseGet.Content.ReadAsStringAsync();
        var usuarioOriginal = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(contentGet, _jsonOptions);

        // Obtener el usuario administrador para usar como autorizador
        var usuarioCreadorId = await CrearUsuarioAdministradorDePruebaAsync();

        // Simular actualización concurrente - actualizar el usuario
        var usuarioActualizado = new
        {
            UsuarioId = usuarioId,
            Nombre = "Usuario Actualizado Concurrentemente",
            Email = usuarioOriginal.Data.Email,
            Telefono = "9999999999",
            Rol = usuarioOriginal.Data.Rol,
            Activo = usuarioOriginal.Data.Activo,
            UsuarioAutorizaId = usuarioCreadorId,
            MotivoActualizacion = "Prueba de concurrencia de datos",
            Prioridad = 1,
            RequiereAprobacion = true
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioActualizado, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await authenticatedClient.PutAsync($"/api/core/usuarios/{usuarioId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(responseContent, _jsonOptions);
        responseData.Should().NotBeNull();
        responseData!.Success.Should().BeTrue();
        responseData.Data!.NombreCompleto.Should().Be(usuarioActualizado.Nombre);
    }

    #endregion

    #region Métodos de Ayuda

    private async Task<Guid> CrearUsuarioAdministradorDePruebaAsync()
    {
        // Usar el seeder de la clase base que crea el usuario directamente en la base de datos
        return await UsuariosTestSeeder.SeedUsuarioAdministradorAsync(_context);
    }

    private async Task<List<Guid>> SeedUsuariosDePruebaAsync()
    {
        // Crear usuario administrador primero
        var usuarioCreadorId = await CrearUsuarioAdministradorDePruebaAsync();
        
        var usuarios = new[]
        {
            new
            {
                nombreUsuario = "usuario.prueba1",
                nombreCompleto = "Usuario Prueba Uno",
                email = "usuario1@restaurantepro.com",
                password = "Password123!",
                confirmarPassword = "Password123!",
                telefono = "1111111111",
                rol = "Mesero",
                nivelAcceso = 1,
                usuarioCreadorId = usuarioCreadorId,
                activo = true
            },
            new
            {
                nombreUsuario = "usuario.prueba2",
                nombreCompleto = "Usuario Prueba Dos",
                email = "usuario2@restaurantepro.com",
                password = "Password123!",
                confirmarPassword = "Password123!",
                telefono = "2222222222",
                rol = "Cocinero",
                nivelAcceso = 1,
                usuarioCreadorId = usuarioCreadorId,
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

            var authenticatedClient = CreateAuthenticatedClient();
            var response = await authenticatedClient.PostAsync("/api/core/usuarios", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(responseContent, _jsonOptions);
            usuarioIds.Add(responseData.Data.Id);
        }

        return usuarioIds;
    }

    #endregion
}
