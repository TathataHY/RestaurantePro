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
        var apiResponse = await DeserializarResponse<List<UsuarioDto>>(response);
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

    [Fact(DisplayName = "Auth_ConsultaUsuarios_DeberiaRetornarUsuariosSembrados")]
    public async Task Auth_ConsultaUsuarios_DeberiaRetornarUsuariosSembrados()
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

        // Act: Consultar usuarios (endpoint que sabemos que existe)
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await DeserializarResponse<List<UsuarioDto>>(response);
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Should().Contain(u => u.Email == "admin@restaurantepro.com");
        apiResponse.Data.Should().Contain(u => u.Roles.Contains("Administrador"));
    }

    [Fact(DisplayName = "Auth_ConsultaPerfil_DeberiaRetornarPerfilUsuarioSembrado")]
    public async Task Auth_ConsultaPerfil_DeberiaRetornarPerfilUsuarioSembrado()
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

        // Act: Consultar perfil del usuario autenticado
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await DeserializarResponse<UserDto>(response);
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Email.Should().Be("admin@restaurantepro.com");
        apiResponse.Data.UserName.Should().Be("admin");
    }

    #region Refresh Token Tests

    [Fact(DisplayName = "Login_ConRecordarme_DebeDevolverRefreshToken")]
    public async Task Login_ConRecordarme_DebeDevolverRefreshToken()
    {
        // Arrange - Crear un usuario de prueba
        var registerRequest = new
        {
            Nombre = "Refresh",
            Apellidos = "Test",
            Email = "refresh@test.com",
            Username = "refreshtest",
            Password = "Test123!",
            Rol = "Empleado"
        };

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        var registerResponse = await HttpClient.PostAsync("/api/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Login con Recordarme activado
        var loginRequest = new
        {
            Email = "refresh@test.com",
            Password = "Test123!",
            Recordarme = true
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await HttpClient.PostAsync("/api/auth/login", loginContent);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        loginResult.Should().Contain("Success");
        loginResult.Should().Contain("Token");
        loginResult.Should().Contain("RefreshToken");
    }

    [Fact(DisplayName = "Login_SinRecordarme_NoDebeDevolverRefreshToken")]
    public async Task Login_SinRecordarme_NoDebeDevolverRefreshToken()
    {
        // Arrange - Crear un usuario de prueba
        var registerRequest = new
        {
            Nombre = "NoRefresh",
            Apellidos = "Test",
            Email = "norefresh@test.com",
            Username = "norefreshtest",
            Password = "Test123!",
            Rol = "Empleado"
        };

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        var registerResponse = await HttpClient.PostAsync("/api/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Login sin Recordarme
        var loginRequest = new
        {
            Email = "norefresh@test.com",
            Password = "Test123!",
            Recordarme = false
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await HttpClient.PostAsync("/api/auth/login", loginContent);

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        loginResult.Should().Contain("Success");
        loginResult.Should().Contain("Token");
        loginResult.Should().NotContain("RefreshToken");
    }

    [Fact(DisplayName = "RefreshToken_ConTokenValido_DebeRenovarToken")]
    public async Task RefreshToken_ConTokenValido_DebeRenovarToken()
    {
        // Arrange - Crear usuario y hacer login con Recordarme
        var registerRequest = new
        {
            Nombre = "RefreshValid",
            Apellidos = "Test",
            Email = "refreshvalid@test.com",
            Username = "refreshvalidtest",
            Password = "Test123!",
            Rol = "Empleado"
        };

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        var registerResponse = await HttpClient.PostAsync("/api/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Login con Recordarme
        var loginRequest = new
        {
            Email = "refreshvalid@test.com",
            Password = "Test123!",
            Recordarme = true
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await HttpClient.PostAsync("/api/auth/login", loginContent);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var originalToken = loginApiResponse.Data.Token;
        var refreshToken = loginApiResponse.Data.RefreshToken;

        // Act - Usar refresh token para renovar
        var refreshRequest = new
        {
            Token = originalToken,
            RefreshToken = refreshToken
        };

        var refreshContent = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");
        var refreshResponse = await HttpClient.PostAsync("/api/auth/refresh", refreshContent);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshResult = await refreshResponse.Content.ReadAsStringAsync();
        refreshResult.Should().Contain("Success");
        refreshResult.Should().Contain("Token");
        refreshResult.Should().Contain("RefreshToken");

        // Verificar que el nuevo token es diferente al original
        var refreshApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(refreshResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        refreshApiResponse.Data.Token.Should().NotBe(originalToken);
        refreshApiResponse.Data.RefreshToken.Should().NotBe(refreshToken);
    }

    [Fact(DisplayName = "RefreshToken_ConTokenInvalido_DebeDevolver401")]
    public async Task RefreshToken_ConTokenInvalido_DebeDevolver401()
    {
        // Act - Intentar refresh con token inválido
        var refreshRequest = new
        {
            Token = "token_invalido",
            RefreshToken = "invalid-refresh-token"
        };

        var refreshContent = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");
        var refreshResponse = await HttpClient.PostAsync("/api/auth/refresh", refreshContent);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "RefreshToken_ConRefreshTokenExpirado_DebeDevolver401")]
    public async Task RefreshToken_ConRefreshTokenExpirado_DebeDevolver401()
    {
        // Arrange - Crear usuario y hacer login con Recordarme
        var registerRequest = new
        {
            Nombre = "RefreshExpired",
            Apellidos = "Test",
            Email = "refreshexpired@test.com",
            Username = "refreshexpiredtest",
            Password = "Test123!",
            Rol = "Empleado"
        };

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        var registerResponse = await HttpClient.PostAsync("/api/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Login con Recordarme
        var loginRequest = new
        {
            Email = "refreshexpired@test.com",
            Password = "Test123!",
            Recordarme = true
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await HttpClient.PostAsync("/api/auth/login", loginContent);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var originalToken = loginApiResponse.Data.Token;
        var refreshToken = loginApiResponse.Data.RefreshToken;

        // Simular refresh token expirado modificando la base de datos
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync("refreshexpired@test.com");
        if (user != null)
        {
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1); // Hacer que expire ayer
            await userManager.UpdateAsync(user);
        }

        // Act - Intentar refresh con token expirado
        var refreshRequest = new
        {
            Token = originalToken,
            RefreshToken = "expired-refresh-token"
        };

        var refreshContent = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");
        var refreshResponse = await HttpClient.PostAsync("/api/auth/refresh", refreshContent);

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "FlujoCompleto_LoginConRecordarmeYRefresh_DebeFuncionarCorrectamente")]
    public async Task FlujoCompleto_LoginConRecordarmeYRefresh_DebeFuncionarCorrectamente()
    {
        // Arrange - Crear usuario
        var registerRequest = new
        {
            Nombre = "FlujoCompleto",
            Apellidos = "Test",
            Email = "flujocompleto@test.com",
            Username = "flujocompletotest",
            Password = "Test123!",
            Rol = "Empleado"
        };

        var registerContent = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
        var registerResponse = await HttpClient.PostAsync("/api/auth/register", registerContent);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act 1 - Login con Recordarme
        var loginRequest = new
        {
            Email = "flujocompleto@test.com",
            Password = "Test123!",
            Recordarme = true
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await HttpClient.PostAsync("/api/auth/login", loginContent);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token1 = loginApiResponse.Data.Token;
        var refreshToken1 = loginApiResponse.Data.RefreshToken;

        // Act 2 - Usar el token para acceder a endpoint protegido
        var request1 = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
        request1.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);
        var profileResponse1 = await HttpClient.SendAsync(request1);
        profileResponse1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act 3 - Refresh del token
        var refreshRequest = new
        {
            Token = token1,
            RefreshToken = refreshToken1
        };

        var refreshContent = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");
        var refreshResponse = await HttpClient.PostAsync("/api/auth/refresh", refreshContent);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResult = await refreshResponse.Content.ReadAsStringAsync();
        var refreshApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(refreshResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var token2 = refreshApiResponse.Data.Token;
        var refreshToken2 = refreshApiResponse.Data.RefreshToken;

        // Act 4 - Usar el nuevo token para acceder a endpoint protegido
        var request2 = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
        request2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);
        var profileResponse2 = await HttpClient.SendAsync(request2);
        profileResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert
        token2.Should().NotBe(token1, "El nuevo token debe ser diferente al original");
        refreshToken2.Should().NotBe(refreshToken1, "El nuevo refresh token debe ser diferente al original");
        profileResponse1.StatusCode.Should().Be(HttpStatusCode.OK, "El token original debe funcionar");
        profileResponse2.StatusCode.Should().Be(HttpStatusCode.OK, "El token renovado debe funcionar");
    }

    #endregion

    #region Métodos Helper

    private static async Task<ApiResponse<T>> DeserializarResponse<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsyncApiResponse<T>() ?? new ApiResponse<T> { Success = false, Data = default };
    }

    #endregion
} 