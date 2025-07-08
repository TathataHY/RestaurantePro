using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Api.Models.Requests;
using Microsoft.AspNetCore.Identity;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;
using RestaurantePro.Infrastructure.Persistence.SeedData.Critical;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Core.Usuarios.DTOs;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Core;

/// <summary>
/// Tests de integración para AuthController
/// Prueba los endpoints de autenticación (login, registro, etc.)
/// </summary>
[Collection("ApiTestCollection")]
public class AuthControllerTests : AuthorizationTestBase, IAsyncLifetime
{
    public AuthControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        // Ejecutar seeders críticos manualmente antes de los tests
        using var scope = Factory.Services.CreateScope();
        var seedDataRunner = scope.ServiceProvider.GetRequiredService<RestaurantePro.Infrastructure.Persistence.SeedData.Extensions.SeedDataRunner>();
        await seedDataRunner.RunCriticalOnlyAsync();
    }

    public async Task DisposeAsync() { /* No-op, cleanup handled by base */ }

    [Fact]
    public async Task Login_ConCredencialesValidas_DebeDevolverToken()
    {
        // Arrange - Crear un usuario de prueba
        var registerRequest = new
        {
            Nombre = "Test",
            Apellidos = "User",
            Email = "test@test.com",
            Username = "testuser",
            Password = "Test123!",
            Rol = "Empleado"
        };

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        var registerResponse = await HttpClient.PostAsync("/api/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Intentar login
        var loginRequest = new
        {
            Email = "test@test.com",
            Password = "Test123!"
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await HttpClient.PostAsync("/api/auth/login", loginContent);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        loginResult.Should().Contain("Success");
        loginResult.Should().Contain("Token");
    }

    [Fact]
    public async Task Login_ConCredencialesInvalidas_DebeDevolver401()
    {
        // Act - Intentar login con credenciales inválidas
        var loginRequest = new
        {
            Email = "invalid@test.com",
            Password = "WrongPassword"
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await HttpClient.PostAsync("/api/auth/login", loginContent);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_ConDatosValidos_DebeCrearUsuario()
    {
        // Act - Registrar un nuevo usuario
        var registerRequest = new
        {
            Nombre = "Nuevo",
            Apellidos = "Usuario",
            Email = "nuevo@test.com",
            Username = "nuevousuario",
            Password = "Nuevo123!",
            Rol = "Empleado"
        };

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        var registerResponse = await HttpClient.PostAsync("/api/auth/register", registerContent);

        // Assert
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerResult = await registerResponse.Content.ReadAsStringAsync();
        registerResult.Should().Contain("Success");
    }

    [Fact]
    public async Task Register_ConEmailDuplicado_DebeDevolver400()
    {
        // Arrange - Crear un usuario
        var registerRequest = new
        {
            Nombre = "Test",
            Apellidos = "User",
            Email = "duplicado@test.com",
            Username = "testuser1",
            Password = "Test123!",
            Rol = "Empleado"
        };

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        await HttpClient.PostAsync("/api/auth/register", registerContent);

        // Act - Intentar registrar otro usuario con el mismo email
        var duplicateRequest = new
        {
            Nombre = "Otro",
            Apellidos = "Usuario",
            Email = "duplicado@test.com", // Mismo email
            Username = "testuser2",
            Password = "Test123!",
            Rol = "Empleado"
        };

        var duplicateContent = new StringContent(JsonSerializer.Serialize(duplicateRequest), Encoding.UTF8, "application/json");
        var duplicateResponse = await HttpClient.PostAsync("/api/auth/register", duplicateContent);

        // Assert
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProfile_SinAutenticacion_DebeDevolver401()
    {
        // Act - Intentar obtener perfil sin autenticación
        var response = await HttpClient.GetAsync("/api/auth/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_ConAutenticacion_DebeDevolverPerfil()
    {
        // Arrange: Registrar y loguear usuario
        var registerRequest = new RegisterRequest
        {
            Nombre = "Perfil",
            Apellidos = "Test",
            Email = "perfil@test.com",
            Username = "perfiltest",
            Password = "Password123!",
            Rol = "Empleado"
        };
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token = loginApiResponse.Data.Token;

        // Act: Llamar endpoint protegido con token
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var profileResponse = await HttpClient.SendAsync(request);

        // Assert
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_ConAutenticacion_DebeCambiarContraseña()
    {
        // Arrange: Registrar y loguear usuario
        var registerRequest = new RegisterRequest
        {
            Nombre = "Password",
            Apellidos = "Test",
            Email = "password@test.com",
            Username = "passwordtest",
            Password = "Password123!",
            Rol = "Empleado"
        };
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token = loginApiResponse.Data.Token;

        // Act: Cambiar contraseña autenticado
        var changePasswordRequest = new ChangePasswordRequest
        {
            CurrentPassword = registerRequest.Password,
            NewPassword = "NuevaPassword123!",
            ConfirmNewPassword = "NuevaPassword123!"
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/change-password");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(changePasswordRequest);
        var changePasswordResponse = await HttpClient.SendAsync(request);

        // Assert
        changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_ConAutenticacion_DebeCerrarSesion()
    {
        // Arrange: Registrar y loguear usuario
        var registerRequest = new RegisterRequest
        {
            Nombre = "Logout",
            Apellidos = "Test",
            Email = "logout@test.com",
            Username = "logouttest",
            Password = "Password123!",
            Rol = "Empleado"
        };
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token = loginApiResponse.Data.Token;

        // Act: Logout autenticado
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var logoutResponse = await HttpClient.SendAsync(request);

        // Assert
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Login_AdminSembrado_DebeDevolverToken")]
    public async Task Login_AdminSembrado_DebeDevolverToken()
    {
        // Arrange: Limpiar completamente Identity y forzar re-ejecución de seeders
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        
        // Limpiar todos los usuarios y roles de Identity
        var allUsers = await userManager.Users.ToListAsync();
        foreach (var user in allUsers)
        {
            await userManager.DeleteAsync(user);
        }
        
        var allRoles = await roleManager.Roles.ToListAsync();
        foreach (var role in allRoles)
        {
            await roleManager.DeleteAsync(role);
        }
        
        Console.WriteLine("🧹 Identity limpiado completamente");
        
        // Forzar re-ejecución de seeders críticos
        var seedDataRunner = scope.ServiceProvider.GetRequiredService<SeedDataRunner>();
        await seedDataRunner.RunCriticalOnlyAsync();
        Console.WriteLine("✅ Seeders críticos re-ejecutados");
        
        // Verificar que el usuario admin existe en Identity
        var adminIdentity = await userManager.FindByEmailAsync("admin@restaurantepro.com");
        if (adminIdentity == null)
        {
            Assert.Fail("El usuario admin no existe en Identity después de ejecutar los seeders");
        }
        
        var roles = await userManager.GetRolesAsync(adminIdentity);
        Console.WriteLine($"👤 Usuario admin en Identity: {adminIdentity.Email}, Roles: [{string.Join(", ", roles)}]");
        
        // Preparar request de login
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };
        var loginContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(loginRequest), System.Text.Encoding.UTF8, "application/json");

        // Act
        var client = HttpClient;
        var loginResponse = await client.PostAsync("/api/auth/login", loginContent);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, "El login debe ser exitoso");
        
        var responseContent = await loginResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"📄 Respuesta del login: {responseContent}");
        
        responseContent.Should().Contain("Success", "La respuesta debe contener 'Success'");
        responseContent.Should().Contain("Token", "La respuesta debe contener un token JWT");
    }

    [Fact(DisplayName = "Authz_AdminPuedeAcceder_EndpointProtegido")]
    public async Task Authz_AdminPuedeAcceder_EndpointProtegido()
    {
        // Arrange: Login con usuario admin sembrado
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginJson = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token = loginApiResponse.Data.Token;

        // Act: Acceder a endpoint protegido con el token
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "El admin debe poder acceder al endpoint protegido");
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UsuarioDto>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNullOrEmpty();
        apiResponse.Data.Should().Contain(u => u.Email == "admin@restaurantepro.com");
    }

    [Fact(DisplayName = "Authz_CajeroNoPuedeAcceder_EndpointProtegido")]
    public async Task Authz_CajeroNoPuedeAcceder_EndpointProtegido()
    {
        // Arrange: Registrar y loguear usuario con rol Cajero
        var registerRequest = new RegisterRequest
        {
            Nombre = "Cajero",
            Apellidos = "Test",
            Email = "cajero@test.com",
            Username = "cajerotest",
            Password = "Cajero123!",
            Rol = "Cajero"
        };
        var registerResponse = await HttpClient.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token = loginApiResponse.Data.Token;

        // Act: Intentar acceder a endpoint protegido con token de Cajero
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "Un cajero no debe poder acceder al endpoint protegido solo para administradores");
    }

    [Fact(DisplayName = "Auth_ConsultaRoles_DeberiaRetornarRolesSembrados")]
    public async Task Auth_ConsultaRoles_DeberiaRetornarRolesSembrados()
    {
        // Arrange: Login como admin
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token = loginApiResponse.Data.Token;

        // Act: Consultar roles
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/roles");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<string>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().Contain("Administrador");
        apiResponse.Data.Should().Contain("Cajero");
        apiResponse.Data.Should().Contain("Mesero");
        apiResponse.Data.Should().Contain("Gerente");
    }

    [Fact(DisplayName = "Auth_ConsultaPermisos_DeberiaRetornarPermisosSembrados")]
    public async Task Auth_ConsultaPermisos_DeberiaRetornarPermisosSembrados()
    {
        // Arrange: Login como admin
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token = loginApiResponse.Data.Token;

        // Act: Consultar permisos
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/permissions");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<string>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().Contain("sistema.full_access");
        apiResponse.Data.Should().Contain("usuarios.read");
        apiResponse.Data.Should().Contain("productos.read");
    }
} 