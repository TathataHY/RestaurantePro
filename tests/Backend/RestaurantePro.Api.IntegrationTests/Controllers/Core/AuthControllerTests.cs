using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Api.Models.Requests;

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
} 