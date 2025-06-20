using Microsoft.Extensions.Logging;
using RestaurantePro.Api.IntegrationTests.TestBase;
using System.Net;
using System.Linq;
using AutoMapper;
using RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario;
using RestaurantePro.Application.Core.Usuarios.DTOs;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Core;

[Collection("Sequential")]
public class UsuariosControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    
    public UsuariosControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }
    
    public new void Dispose()
    {
        _factory?.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// Método helper para crear un usuario creador válido directamente en la base de datos
    /// </summary>
    private async Task<Guid> CrearUsuarioCreadorValido()
    {
        // Crear usuario directamente en la base de datos para bypassear validaciones circulares
        var usuarioCreador = Usuario.Crear(
            "admin.sistema", 
            "Administrador Sistema", 
            "admin@sistema.com", 
            RolUsuario.Administrador);

        // Confirmar la cuenta para que esté activo (requerido por el validador)
        usuarioCreador.ConfirmarCuenta();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        
        await context.Usuarios.AddAsync(usuarioCreador);
        await context.SaveChangesAsync(CancellationToken.None);
        
        Logger.LogInformation($"✅ Usuario creador válido creado con ID: {usuarioCreador.Id} - Estado: {usuarioCreador.Estado}");
        return usuarioCreador.Id;
    }

    [Fact]
    public async Task GetUsuarios_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetUsuarios_DebeRetornar501NotImplemented");

        // Act
        var response = await HttpClient.GetAsync("/api/core/usuarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementado");
        
        Logger.LogInformation("✅ Test completado - endpoint correctamente marcado como no implementado");
    }

    [Fact]
    public async Task GetUsuario_ConIdExistente_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetUsuario_ConIdExistente_DebeRetornar501NotImplemented");
        var idUsuario = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/api/core/usuarios/{idUsuario}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementado");
    }

    [Fact]
    public async Task PostUsuario_ConDatosValidos_DebeCrearUsuario()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostUsuario_ConDatosValidos_DebeCrearUsuario");
        
        // Crear usuario creador válido primero
        var creadorId = await CrearUsuarioCreadorValido();
        
        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "juan.perez",
            NombreCompleto = "Juan Pérez García",
            Email = "juan.perez@test.com",
            Password = "Password123!",
            ConfirmarPassword = "Password123!",
            Rol = "Administrador", // Cambiar a un rol que definitivamente es válido
            NivelAcceso = 8, // Nivel compatible con rol Administrador (6-9)
            Telefono = "+56987654321", // Formato internacional válido
            UsuarioCreadorId = creadorId // Usuario creador válido y existente
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/core/usuarios", command);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Si hay error, mostrar detalles para debugging
        if (response.StatusCode != HttpStatusCode.Created)
        {
            Logger.LogError($"❌ Test falló. Detalles del error: {content}");
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            if (errorResponse?.Errors != null)
            {
                foreach (var error in errorResponse.Errors)
                {
                    Logger.LogError($"🔍 Error de validación: {error}");
                }
            }
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Nombre.Should().Be("Juan Pérez García");
        apiResponse.Data.Email.Should().Be("juan.perez@test.com");
        apiResponse.Data.Id.Should().NotBe(Guid.Empty);
        
        Logger.LogInformation("✅ Test completado exitosamente - usuario creado correctamente");
    }

    [Fact]
    public async Task PostUsuario_ConEmailDuplicado_DebeRetornar400()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostUsuario_ConEmailDuplicado_DebeRetornar400");
        
        var creadorId = await CrearUsuarioCreadorValido();
        
        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "maria.gonzalez",
            NombreCompleto = "María González López",
            Email = "maria.gonzalez@test.com",
            Password = "Password123!",
            ConfirmarPassword = "Password123!",
            Rol = "Administrador", // Cambiar a un rol que definitivamente es válido
            NivelAcceso = 8, // Nivel compatible con rol Administrador (6-9)
            Telefono = "+56912345678",
            UsuarioCreadorId = creadorId
        };

        // Crear primer usuario
        await HttpClient.PostAsJsonAsync("/api/core/usuarios", command);

        // Act - Intentar crear usuario con mismo email
        var response = await HttpClient.PostAsJsonAsync("/api/core/usuarios", command);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().NotBeEmpty();
        
        Logger.LogInformation("✅ Test completado - duplicado correctamente rechazado");
    }

    [Fact]
    public async Task PostUsuario_ConDatosInvalidos_DebeRetornar400()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PostUsuario_ConDatosInvalidos_DebeRetornar400");
        
        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "", // Nombre de usuario vacío - inválido
            NombreCompleto = "",
            Email = "email-invalido", // Email inválido
            Password = "123", // Password muy corto
            ConfirmarPassword = "456", // No coincide
            Rol = "RolInexistente", // Rol inválido
            UsuarioCreadorId = Guid.Empty // ID inválido
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/core/usuarios", command);
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        
        Logger.LogInformation("✅ Test completado - datos inválidos correctamente rechazados");
    }

    [Fact]
    public async Task PutUsuario_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: PutUsuario_DebeRetornar501NotImplemented");
        var idUsuario = Guid.NewGuid();

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/core/usuarios/{idUsuario}", new { });
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementado");
        
        Logger.LogInformation("✅ Test completado - endpoint correctamente marcado como no implementado");
    }

    [Fact]
    public async Task DeleteUsuario_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: DeleteUsuario_DebeRetornar501NotImplemented");
        var idUsuario = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"/api/core/usuarios/{idUsuario}");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementado");
        
        Logger.LogInformation("✅ Test completado - endpoint correctamente marcado como no implementado");
    }

    [Fact]
    public async Task GetPerfilActual_DebeRetornar501NotImplemented()
    {
        // Arrange
        Logger.LogInformation("🧪 Iniciando test: GetPerfilActual_DebeRetornar501NotImplemented");

        // Act
        var response = await HttpClient.GetAsync("/api/core/usuarios/perfil");
        var content = await response.Content.ReadAsStringAsync();
        Logger.LogInformation($"📋 Response: {response.StatusCode} - {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no implementado");
        
        Logger.LogInformation("✅ Test completado - endpoint correctamente marcado como no implementado");
    }
} 