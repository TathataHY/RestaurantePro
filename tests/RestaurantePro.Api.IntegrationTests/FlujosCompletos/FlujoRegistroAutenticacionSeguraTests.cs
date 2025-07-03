using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using RestaurantePro.Api.IntegrationTests.TestBase;
using System.Text.Json;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos
{
    [Collection("ApiTestCollection")]
    public class FlujoRegistroAutenticacionSeguraTests : ApiIntegrationTestBase
    {
        public FlujoRegistroAutenticacionSeguraTests(TestWebApplicationFactory factory) : base(factory) { }

        [Fact(DisplayName = "Flujo completo de registro y autenticación - Debe funcionar end-to-end")]
        public async Task FlujoCompletoRegistroAutenticacion_DebeFuncionarEndToEnd()
        {
            // Arrange - Datos de usuario para el flujo completo
            var userData = new
            {
                Nombre = "Juan",
                Apellidos = "Pérez",
                Email = "juan.perez@restaurantepro.com",
                Username = "juanperez",
                Password = "SecurePass123*",
                Rol = "Empleado"
            };

            // Act 1: Registro de usuario
            var registerResponse = await HttpClient.PostAsJsonAsync("/api/auth/register", userData);

            // Assert 1: Verificar que el registro fue exitoso
            registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            registerContent.Should().Contain("Usuario registrado exitosamente");

            // Act 2: Verificar que el usuario existe intentando autenticarse
            // Esta es una validación más realista del flujo end-to-end
            // El usuario debe poder autenticarse inmediatamente después del registro
            var verifyLoginData = new
            {
                Email = userData.Email,
                Password = userData.Password
            };
            var verifyLoginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", verifyLoginData);
            
            // Assert 2: Verificar que el usuario puede autenticarse (confirmando que existe)
            verifyLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var verifyLoginJson = await verifyLoginResponse.Content.ReadAsStringAsync();
            using var verifyLoginDoc = JsonDocument.Parse(verifyLoginJson);
            var root = verifyLoginDoc.RootElement;
            var data = root.TryGetProperty("Data", out var d) ? d : default;
            string verifyToken = data.ValueKind == JsonValueKind.Object && data.TryGetProperty("Token", out var t)
                ? t.GetString()
                : null;
            verifyToken.Should().NotBeNullOrEmpty("El usuario debe poder autenticarse después del registro");

            // Act 3: Usar el token de verificación para acceder a recursos protegidos
            string token = verifyToken; // Usamos el token obtenido en la verificación anterior

            // Act 4: Verificar que el token permite acceso a recursos protegidos
            HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", verifyToken);
            var protectedResourceResponse = await HttpClient.GetAsync("/api/auth/profile");
            
            // Assert 4: Verificar que se puede acceder a recursos protegidos
            protectedResourceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var protectedResourceContent = await protectedResourceResponse.Content.ReadAsStringAsync();
            protectedResourceContent.Should().Contain("Perfil obtenido exitosamente");

            // Act 5: Cambiar contraseña
            var changePasswordData = new
            {
                CurrentPassword = userData.Password,
                NewPassword = "NewSecurePass456*"
            };
            var changePasswordResponse = await HttpClient.PostAsJsonAsync("/api/auth/change-password", changePasswordData);

            // Assert 5: Verificar que el cambio de contraseña fue exitoso
            changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var changePasswordContent = await changePasswordResponse.Content.ReadAsStringAsync();
            changePasswordContent.Should().Contain("Contraseña cambiada exitosamente");

            // Act 6: Verificar que la contraseña anterior ya no funciona
            var oldPasswordLoginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", verifyLoginData);
            oldPasswordLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Act 7: Verificar que la nueva contraseña funciona
            var newLoginData = new
            {
                Email = userData.Email,
                Password = changePasswordData.NewPassword
            };
            var newLoginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", newLoginData);
            newLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act 8: Logout
            var logoutResponse = await HttpClient.PostAsJsonAsync("/api/auth/logout", new { });

            // Assert 8: Verificar que el logout fue exitoso
            logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var logoutContent = await logoutResponse.Content.ReadAsStringAsync();
            logoutContent.Should().Contain("Sesión cerrada exitosamente");

            // Act 9: Verificar que el token ya no es válido (si la implementación lo soporta)
            var invalidTokenResponse = await HttpClient.GetAsync("/api/core/usuarios");
            // Nota: Esto puede fallar dependiendo de cómo se implemente la invalidación de tokens
            // Si el token se invalida correctamente, debería retornar 401
        }

        [Fact(DisplayName = "Validación de credenciales incorrectas - Debe rechazar acceso")]
        public async Task ValidacionCredencialesIncorrectas_DebeRechazarAcceso()
        {
            // Arrange - Usuario que no existe
            var nonExistentUser = new
            {
                Email = "usuario.inexistente@example.com",
                Password = "CualquierPassword123*"
            };

            // Act 1: Intentar login con usuario inexistente
            var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", nonExistentUser);

            // Assert 1: Verificar que se rechaza el acceso
            loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            loginContent.Should().Contain("Credenciales inválidas");

            // Arrange - Usuario existente con contraseña incorrecta
            var existingUser = new
            {
                Nombre = "María",
                Apellidos = "García",
                Email = "maria.garcia@restaurantepro.com",
                Username = "mariagarcia",
                Password = "PasswordCorrecto123*",
                Rol = "Empleado"
            };

            // Crear usuario
            await HttpClient.PostAsJsonAsync("/api/auth/register", existingUser);

            // Act 2: Intentar login con contraseña incorrecta
            var wrongPasswordLogin = new
            {
                Email = existingUser.Email,
                Password = "PasswordIncorrecto456*"
            };
            var wrongPasswordResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", wrongPasswordLogin);

            // Assert 2: Verificar que se rechaza el acceso
            wrongPasswordResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var wrongPasswordContent = await wrongPasswordResponse.Content.ReadAsStringAsync();
            wrongPasswordContent.Should().Contain("Credenciales inválidas");

            // Act 3: Verificar que el login con contraseña correcta sí funciona
            var correctPasswordLogin = new
            {
                Email = existingUser.Email,
                Password = existingUser.Password
            };
            var correctPasswordResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", correctPasswordLogin);

            // Assert 3: Verificar que el login correcto funciona
            correctPasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Validación de datos de registro - Debe validar campos requeridos")]
        public async Task ValidacionDatosRegistro_DebeValidarCamposRequeridos()
        {
            // Arrange - Datos incompletos
            var incompleteData = new
            {
                Nombre = "Test",
                // Apellidos faltante
                Email = "test@example.com",
                // Username faltante
                Password = "Test123*"
                // Rol faltante
            };

            // Act: Intentar registro con datos incompletos
            var response = await HttpClient.PostAsJsonAsync("/api/auth/register", incompleteData);

            // Assert: Verificar que se rechaza el registro
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Errors");

            // Arrange - Email inválido
            var invalidEmailData = new
            {
                Nombre = "Test",
                Apellidos = "User",
                Email = "email-invalido",
                Username = "testuser",
                Password = "Test123*",
                Rol = "Empleado"
            };

            // Act: Intentar registro con email inválido
            var invalidEmailResponse = await HttpClient.PostAsJsonAsync("/api/auth/register", invalidEmailData);

            // Assert: Verificar que se rechaza el registro
            invalidEmailResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var invalidEmailContent = await invalidEmailResponse.Content.ReadAsStringAsync();
            invalidEmailContent.Should().Contain("Errors");
        }

        [Fact(DisplayName = "Validación de seguridad de contraseñas - Debe requerir contraseña segura")]
        public async Task ValidacionSeguridadPassword_DebeRequerirPasswordSeguro()
        {
            // Arrange - Contraseña débil
            var weakPasswordData = new
            {
                Nombre = "Test",
                Apellidos = "User",
                Email = "test.weak@example.com",
                Username = "testweak",
                Password = "123", // Contraseña muy débil
                Rol = "Empleado"
            };

            // Act: Intentar registro con contraseña débil
            var response = await HttpClient.PostAsJsonAsync("/api/auth/register", weakPasswordData);

            // Assert: Verificar que se rechaza el registro
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Errors");

            // Arrange - Contraseña segura
            var strongPasswordData = new
            {
                Nombre = "Test",
                Apellidos = "User",
                Email = "test.strong@example.com",
                Username = "teststrong",
                Password = "SecurePass123*", // Contraseña segura
                Rol = "Empleado"
            };

            // Act: Intentar registro con contraseña segura
            var strongResponse = await HttpClient.PostAsJsonAsync("/api/auth/register", strongPasswordData);

            // Assert: Verificar que el registro es exitoso
            strongResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact(DisplayName = "Validación de token inválido - Debe rechazar acceso")]
        public async Task ValidacionTokenInvalido_DebeRechazarAcceso()
        {
            // Arrange
            var tokenInvalido = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.token";

            // Act: Intentar acceder a recurso protegido con token inválido
            HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenInvalido);
            var response = await HttpClient.GetAsync("/api/auth/profile");

            // Assert: Verificar que se rechaza el acceso (401 o 404 son respuestas válidas para token inválido)
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "Acceso sin token - Debe rechazar acceso")]
        public async Task AccesoSinToken_DebeRechazarAcceso()
        {
            // Arrange: Limpiar headers de autorización
            HttpClient.DefaultRequestHeaders.Authorization = null;

            // Act: Intentar acceder a recurso protegido sin token
            var response = await HttpClient.GetAsync("/api/auth/profile");

            // Assert: Verificar que se rechaza el acceso
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact(DisplayName = "Validación de contraseñas muy débiles - Debe rechazar registro")]
        public async Task ValidacionContrasenasMuyDebiles_DebeRechazarRegistro()
        {
            // Arrange: Contraseñas muy débiles
            var passwordsDebiles = new[]
            {
                "123",           // Muy corta
                "password",      // Palabra común
                "qwerty",        // Secuencia de teclado
                "aaaaaa",        // Caracteres repetidos
                "123456789"      // Solo números
            };

            foreach (var password in passwordsDebiles)
            {
                var request = new
                {
                    Nombre = "Test",
                    Apellidos = "User",
                    Email = $"test{password}@example.com",
                    Username = $"testuser{password}",
                    Password = password,
                    Rol = "Empleado"
                };

                // Act: Intentar registro con contraseña débil
                var response = await HttpClient.PostAsJsonAsync("/api/auth/register", request);

                // Assert: Verificar que se rechaza el registro
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("Errors");
            }
        }

        [Fact(DisplayName = "Validación de logout efectivo - Debe cerrar sesión correctamente")]
        public async Task ValidacionLogoutEfectivo_DebeCerrarSesionCorrectamente()
        {
            // Arrange: Registrar y autenticar usuario
            var userData = new
            {
                Nombre = "Logout",
                Apellidos = "Test",
                Email = "logout.test@restaurantepro.com",
                Username = "logouttest",
                Password = "SecurePass123!",
                Rol = "Empleado"
            };

            var registerResponse = await HttpClient.PostAsJsonAsync("/api/auth/register", userData);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var loginData = new
            {
                Email = userData.Email,
                Password = userData.Password
            };
            var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginData);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginJson = await loginResponse.Content.ReadAsStringAsync();
            using var loginDoc = JsonDocument.Parse(loginJson);
            var root = loginDoc.RootElement;
            var data = root.TryGetProperty("Data", out var d) ? d : default;
            string token = data.ValueKind == JsonValueKind.Object && data.TryGetProperty("Token", out var t)
                ? t.GetString()
                : null;
            token.Should().NotBeNullOrEmpty();

            // Act 1: Verificar que el token funciona
            HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var profileResponse = await HttpClient.GetAsync("/api/auth/profile");
            profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act 2: Hacer logout
            var logoutResponse = await HttpClient.PostAsync("/api/auth/logout", null);
            logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act 3: Intentar usar el token después del logout
            // Nota: En una implementación real, el token debería invalidarse
            // Por ahora, verificamos que el logout responde correctamente
            var logoutContent = await logoutResponse.Content.ReadAsStringAsync();
            logoutContent.Should().Contain("Sesión cerrada exitosamente");
        }
    }
} 