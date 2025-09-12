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
        _jsonOptions = GetJsonOptions();
    }

    #region Pruebas de Creación

    [Fact]
    public async Task CrearUsuario_ConDatosValidos_DeberiaRetornarUsuarioCreado()
    {
        // Arrange
        var usuarioCreadorId = await CrearUsuarioAdministradorDePruebaAsync();
        
        var nuevoUsuario = new
        {
            nombreUsuario = "juan.perez",
            nombreCompleto = "Juan Pérez",
            email = "juan.perez@restaurantepro.com",
            password = "TempPass123!",
            confirmarPassword = "TempPass123!",
            telefono = "+1234567890",
            rol = "Mesero",
            nivelAcceso = 1,
            activo = true,
            usuarioCreadorId = usuarioCreadorId
        };

        var content = new StringContent(
            JsonSerializer.Serialize(nuevoUsuario, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.PostAsync("/api/core/usuarios", content);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response Status: {response.StatusCode}");
        Console.WriteLine($"Response JSON: {responseContent}");
        
        if (response.StatusCode != HttpStatusCode.Created)
        {
            Console.WriteLine($"❌ Error: Expected 201 Created, got {response.StatusCode}");
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);

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

        var authenticatedClient1 = CreateAuthenticatedClient();
        await authenticatedClient1.PostAsync("/api/core/usuarios", content1);

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
        var authenticatedClient2 = CreateAuthenticatedClient();
        var response = await authenticatedClient2.PostAsync("/api/core/usuarios", content2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().Contain("El email ya está en uso.");
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
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.PostAsync("/api/core/usuarios", content);

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
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.GetAsync("/api/core/usuarios?pageNumber=1&pageSize=5");

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
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.GetAsync($"/api/core/usuarios/{usuarioId}");

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

    #endregion

    #region Pruebas de Actualización

    [Fact]
    public async Task ActualizarUsuario_ConDatosValidos_DeberiaRetornarUsuarioActualizado()
    {
        // Arrange
        var usuarioIds = await SeedUsuariosDePruebaAsync();
        var usuarioId = usuarioIds.First();
        var usuarioAutorizaId = usuarioIds.First(); // Usar el primer usuario como autorizador

        var usuarioActualizado = new
        {
            UsuarioId = usuarioId,
            Nombre = "Juan Pérez Actualizado",
            Email = "juan.actualizado@restaurantepro.com",
            Telefono = "1111111111",
            Rol = "Mesero", // Usar rol válido del enum
            Activo = true,
            UsuarioAutorizaId = usuarioAutorizaId,
            MotivoActualizacion = "Actualización de datos de prueba para integración",
            Prioridad = 2,
            RequiereAprobacion = true // Requerido para cambios críticos como cambio de rol
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioActualizado, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.PutAsync($"/api/core/usuarios/{usuarioId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<RestaurantePro.Application.Core.Usuarios.DTOs.UsuarioDto>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeTrue();
        responseData.Data.Should().NotBeNull();
        responseData.Data.NombreCompleto.Should().Be(usuarioActualizado.Nombre);
        responseData.Data.Email.Should().Be(usuarioActualizado.Email);
    }

    [Fact]
    public async Task ActualizarUsuario_ConIdInexistente_DeberiaRetornarNotFound()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var usuarioIds = await SeedUsuariosDePruebaAsync();
        var usuarioAutorizaId = usuarioIds.First(); // Usar un usuario existente como autorizador

        var usuarioActualizado = new
        {
            UsuarioId = idInexistente,
            Nombre = "Usuario Inexistente",
            Email = "inexistente@restaurantepro.com",
            Telefono = "1234567890",
            Rol = "Mesero",
            Activo = true,
            UsuarioAutorizaId = usuarioAutorizaId,
            MotivoActualizacion = "Prueba de actualización con ID inexistente",
            Prioridad = 2,
            RequiereAprobacion = true // Requerido para cambios críticos como cambio de rol
        };

        var content = new StringContent(
            JsonSerializer.Serialize(usuarioActualizado, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.PutAsync($"/api/core/usuarios/{idInexistente}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // El validador devuelve 400, no 404

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, _jsonOptions);
        
        responseData.Should().NotBeNull();
        responseData.Success.Should().BeFalse();
        responseData.Errors.Should().NotBeEmpty();
        responseData.Errors.Should().Contain(e => e.Contains("El usuario especificado no existe"));
    }

    #endregion

    #region Pruebas de Eliminación

    [Fact]
    public async Task EliminarUsuario_ConIdValido_DeberiaRetornarOk()
    {
        // Arrange
        var usuarioIds = await SeedUsuariosDePruebaAsync();
        var usuarioId = usuarioIds.First();
        var authenticatedClient = CreateAuthenticatedClient();

        // Act
        var response = await authenticatedClient.DeleteAsync($"/api/core/usuarios/{usuarioId}");

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

    #region Pruebas de Cambio de Rol

    [Fact]
    public async Task CambiarRol_ConRolValido_DeberiaRetornarUsuarioActualizado()
    {
        // Arrange
        var usuarioIds = await SeedUsuariosDePruebaAsync();
        var usuarioId = usuarioIds.First();
        var usuarioCambiadorId = usuarioIds.Last(); // Usar otro usuario como cambiador

        var cambioRol = new
        {
            nuevoRol = "Gerente",
            usuarioCambiadorId = usuarioCambiadorId,
            motivo = "Cambio de rol para prueba de integración"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(cambioRol, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        // Act
        var authenticatedClient = CreateAuthenticatedClient();
        var response = await authenticatedClient.PostAsync($"/api/core/usuarios/{usuarioId}/cambiar-rol", content);

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
        // Crear usuario administrador primero
        var usuarioCreadorId = await CrearUsuarioAdministradorDePruebaAsync();
        
        var usuarios = new[]
        {
            new
            {
                nombreUsuario = "maria.garcia",
                nombreCompleto = "María García",
                email = "maria.garcia@restaurantepro.com",
                password = "Password123!",
                confirmarPassword = "Password123!",
                telefono = "+1111111111",
                rol = "Mesero",
                nivelAcceso = 1,
                usuarioCreadorId = usuarioCreadorId,
                activo = true
            },
            new
            {
                nombreUsuario = "carlos.lopez",
                nombreCompleto = "Carlos López",
                email = "carlos.lopez@restaurantepro.com",
                password = "Password123!",
                confirmarPassword = "Password123!",
                telefono = "+2222222222",
                rol = "Cocinero",
                nivelAcceso = 1,
                usuarioCreadorId = usuarioCreadorId,
                activo = true
            },
            new
            {
                nombreUsuario = "ana.martinez",
                nombreCompleto = "Ana Martínez",
                email = "ana.martinez@restaurantepro.com",
                password = "Password123!",
                confirmarPassword = "Password123!",
                telefono = "+3333333333",
                rol = "Cajero",
                nivelAcceso = 1,
                usuarioCreadorId = usuarioCreadorId,
                activo = true
            }
        };

        var usuarioIds = new List<Guid>();

        var authenticatedClient = CreateAuthenticatedClient();
        foreach (var usuario in usuarios)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(usuario, _jsonOptions),
                Encoding.UTF8,
                "application/json");

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
