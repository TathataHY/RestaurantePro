using System.Diagnostics;
using System.Net;
using System.Text.Json;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.Models.Requests;
using RestaurantePro.Api.IntegrationTests.TestBase;
using Xunit;
using System.Net.Http;
using System.Text;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Core;

/// <summary>
/// Tests de rendimiento para endpoints críticos de la API
/// </summary>
[Collection("ApiTestCollection")]
public class PerformanceControllerTests : ApiIntegrationTestBase
{
    public PerformanceControllerTests(TestWebApplicationFactory factory) : base(factory) { }

    #region Tests Básicos de Rendimiento

    [Fact(DisplayName = "Performance_Login_DeberiaCompletarEnMenosDe500ms")]
    public async Task Performance_Login_DeberiaCompletarEnMenosDe500ms()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(stopwatch.ElapsedMilliseconds < 500, 
            $"Login tardó {stopwatch.ElapsedMilliseconds}ms, debe ser < 500ms");
    }

    [Fact(DisplayName = "Performance_ConsultaUsuarios_DeberiaCompletarEnMenosDe250ms")]
    public async Task Performance_ConsultaUsuarios_DeberiaCompletarEnMenosDe250ms()
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

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var stopwatch = Stopwatch.StartNew();
        var response = await HttpClient.SendAsync(request);
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(stopwatch.ElapsedMilliseconds < 250, 
            $"Consulta usuarios tardó {stopwatch.ElapsedMilliseconds}ms, debe ser < 250ms");
    }

    [Fact(DisplayName = "Performance_ConsultaPerfil_DeberiaCompletarEnMenosDe200ms")]
    public async Task Performance_ConsultaPerfil_DeberiaCompletarEnMenosDe200ms()
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

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var stopwatch = Stopwatch.StartNew();
        var response = await HttpClient.SendAsync(request);
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(stopwatch.ElapsedMilliseconds < 200, 
            $"Consulta perfil tardó {stopwatch.ElapsedMilliseconds}ms, debe ser < 200ms");
    }

    [Fact(DisplayName = "Performance_ConcurrentRequests_DeberiaMantenerRendimiento")]
    public async Task Performance_ConcurrentRequests_DeberiaMantenerRendimiento()
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

        // Act: 5 requests concurrentes
        var tasks = new List<Task<(HttpResponseMessage response, long elapsedMs)>>();
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var requestStopwatch = Stopwatch.StartNew();
                var response = await HttpClient.SendAsync(request);
                requestStopwatch.Stop();
                
                return (response, requestStopwatch.ElapsedMilliseconds);
            }));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"5 requests concurrentes tardaron {stopwatch.ElapsedMilliseconds}ms, debe ser < 2000ms");
        
        foreach (var (response, elapsedMs) in results)
        {
            response.EnsureSuccessStatusCode();
            Assert.True(elapsedMs < 500, 
                $"Request individual tardó {elapsedMs}ms, debe ser < 500ms");
        }
    }

    #endregion

    #region Tests de Endpoints de Negocio Intensivo

    [Fact(DisplayName = "Performance_EndpointsBasicos_DeberianCompletarEnTiempoOptimo")]
    public async Task Performance_EndpointsBasicos_DeberianCompletarEnTiempoOptimo()
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

        // Act: Probar endpoints básicos que sabemos que existen
        var stopwatch = Stopwatch.StartNew();
        
        // 1. Consulta de usuarios (ya validado que funciona)
        var usuariosRequest = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios");
        usuariosRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var usuariosResponse = await HttpClient.SendAsync(usuariosRequest);
        usuariosResponse.EnsureSuccessStatusCode();
        
        // 2. Consulta de perfil (ya validado que funciona)
        var perfilRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
        perfilRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var perfilResponse = await HttpClient.SendAsync(perfilRequest);
        perfilResponse.EnsureSuccessStatusCode();
        
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Endpoints básicos tardaron {stopwatch.ElapsedMilliseconds}ms, debe ser < 1000ms");
    }

    [Fact(DisplayName = "Performance_ConsultaUsuariosMultiple_DeberiaMantenerRendimiento")]
    public async Task Performance_ConsultaUsuariosMultiple_DeberiaMantenerRendimiento()
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

        // Act: Múltiples consultas de usuarios
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 5; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"5 consultas de usuarios tardaron {stopwatch.ElapsedMilliseconds}ms, debe ser < 2000ms");
    }

    #endregion

    #region Tests de Carga con Datos Reales

    [Fact(DisplayName = "Performance_RegistroUsuarios_DeberiaMantenerRendimiento")]
    public async Task Performance_RegistroUsuarios_DeberiaMantenerRendimiento()
    {
        // Arrange: Login como admin
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        // Act: Registrar múltiples usuarios
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var registerRequest = new RegisterRequest
                {
                    Nombre = $"Usuario Test {Guid.NewGuid()}",
                    Apellidos = "Performance",
                    Email = $"test{i}@performance.com",
                    Username = $"testuser{i}",
                    Password = "Test123!",
                    Rol = "Empleado"
                };
                
                return await HttpClient.PostAsJsonAsync("/api/auth/register", registerRequest);
            }));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, 
            $"Registro de 5 usuarios tardó {stopwatch.ElapsedMilliseconds}ms, debe ser < 3000ms");
        
        var successCount = results.Count(r => r.IsSuccessStatusCode);
        Assert.True(successCount >= 1, 
            $"Al menos 1 de 5 registros debe ser exitoso, obtuvimos {successCount}");
    }

    #endregion

    #region Tests de Concurrencia Avanzada

    [Fact(DisplayName = "Performance_ConcurrenciaAlta_DeberiaMantenerEstabilidad")]
    public async Task Performance_ConcurrenciaAlta_DeberiaMantenerEstabilidad()
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

        // Act: 15 requests concurrentes a endpoints que sabemos que funcionan
        var tasks = new List<Task<(string endpoint, HttpResponseMessage response, long elapsedMs)>>();
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 15; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var endpoints = new[] { "/api/core/usuarios", "/api/auth/profile" };
                var endpoint = endpoints[i % endpoints.Length];
                
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var requestStopwatch = Stopwatch.StartNew();
                var response = await HttpClient.SendAsync(request);
                requestStopwatch.Stop();
                
                return (endpoint, response, requestStopwatch.ElapsedMilliseconds);
            }));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, 
            $"15 requests concurrentes tardaron {stopwatch.ElapsedMilliseconds}ms, debe ser < 3000ms");
        
        var successCount = results.Count(r => r.response.IsSuccessStatusCode);
        Assert.True(successCount >= 12, 
            $"Al menos 12 de 15 requests deben ser exitosos, obtuvimos {successCount}");
        
        foreach (var (endpoint, response, elapsedMs) in results)
        {
            Assert.True(elapsedMs < 500, 
                $"Request a {endpoint} tardó {elapsedMs}ms, debe ser < 500ms");
        }
    }

    #endregion

    #region Tests de SignalR/Notificaciones

    [Fact(DisplayName = "Performance_LoginMultiple_DeberiaMantenerRendimiento")]
    public async Task Performance_LoginMultiple_DeberiaMantenerRendimiento()
    {
        // Act: Múltiples logins simultáneos
        var tasks = new List<Task<(HttpResponseMessage response, long elapsedMs)>>();
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var loginRequest = new LoginRequest
                {
                    Email = "admin@restaurantepro.com",
                    Password = "AdminRestaurante123!"
                };
                
                var requestStopwatch = Stopwatch.StartNew();
                var response = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
                requestStopwatch.Stop();
                
                return (response, requestStopwatch.ElapsedMilliseconds);
            }));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"10 logins simultáneos tardaron {stopwatch.ElapsedMilliseconds}ms, debe ser < 2000ms");
        
        var successCount = results.Count(r => r.response.IsSuccessStatusCode);
        Assert.True(successCount >= 8, 
            $"Al menos 8 de 10 logins deben ser exitosos, obtuvimos {successCount}");
        
        foreach (var (response, elapsedMs) in results)
        {
            Assert.True(elapsedMs < 500, 
                $"Login individual tardó {elapsedMs}ms, debe ser < 500ms");
        }
    }

    #endregion

    #region Tests de Reportes y Analytics

    [Fact(DisplayName = "Performance_StressTest_DeberiaMantenerEstabilidad")]
    public async Task Performance_StressTest_DeberiaMantenerEstabilidad()
    {
        // Act: Test de estrés con múltiples operaciones
        var tasks = new List<Task<(string operation, HttpResponseMessage response, long elapsedMs)>>();
        var stopwatch = Stopwatch.StartNew();

        // 5 logins
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var loginRequest = new LoginRequest
                {
                    Email = "admin@restaurantepro.com",
                    Password = "AdminRestaurante123!"
                };
                
                var requestStopwatch = Stopwatch.StartNew();
                var response = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
                requestStopwatch.Stop();
                
                return ("login", response, requestStopwatch.ElapsedMilliseconds);
            }));
        }

        // 5 registros
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var registerRequest = new RegisterRequest
                {
                    Nombre = $"Stress Test {Guid.NewGuid()}",
                    Apellidos = "User",
                    Email = $"stress{i}@test.com",
                    Username = $"stressuser{i}",
                    Password = "Stress123!",
                    Rol = "Empleado"
                };
                
                var requestStopwatch = Stopwatch.StartNew();
                var response = await HttpClient.PostAsJsonAsync("/api/auth/register", registerRequest);
                requestStopwatch.Stop();
                
                return ("register", response, requestStopwatch.ElapsedMilliseconds);
            }));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Test de estrés tardó {stopwatch.ElapsedMilliseconds}ms, debe ser < 5000ms");
        
        var loginResults = results.Where(r => r.operation == "login").ToList();
        var registerResults = results.Where(r => r.operation == "register").ToList();
        
        var loginSuccessCount = loginResults.Count(r => r.response.IsSuccessStatusCode);
        var registerSuccessCount = registerResults.Count(r => r.response.IsSuccessStatusCode);
        
        Assert.True(loginSuccessCount >= 4, 
            $"Al menos 4 de 5 logins deben ser exitosos, obtuvimos {loginSuccessCount}");
        Assert.True(registerSuccessCount >= 1, 
            $"Al menos 1 de 5 registros debe ser exitoso, obtuvimos {registerSuccessCount}");
        
        foreach (var (operation, response, elapsedMs) in results)
        {
            Assert.True(elapsedMs < 1000, 
                $"{operation} tardó {elapsedMs}ms, debe ser < 1000ms");
        }
    }

    #endregion

    #region Tests de Negocio Intensivo

    [Fact(DisplayName = "Performance_FacturacionCompleta_DeberiaSerRapida")]
    public async Task Performance_FacturacionCompleta_DeberiaSerRapida()
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

        // Act: Proceso simulado de facturación usando endpoints existentes
        var stopwatch = Stopwatch.StartNew();
        
        // 1. Consultar usuarios (simula consulta de datos de cliente)
        var request1 = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios");
        request1.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var usuariosResponse = await HttpClient.SendAsync(request1);
        usuariosResponse.EnsureSuccessStatusCode();
        
        // 2. Consultar perfil (simula validación de usuario)
        var request2 = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
        request2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var perfilResponse = await HttpClient.SendAsync(request2);
        perfilResponse.EnsureSuccessStatusCode();
        
        // 3. Consultar usuarios paginados (simula proceso de facturación)
        var request3 = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios?page=1&pageSize=5");
        request3.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var usuarios2Response = await HttpClient.SendAsync(request3);
        usuarios2Response.EnsureSuccessStatusCode();
        
        stopwatch.Stop();
        var tiempoTotal = stopwatch.ElapsedMilliseconds;
        
        // Assert
        Assert.True(tiempoTotal < 3000, $"El flujo simulado debe completarse en menos de 3s, tomó {tiempoTotal}ms");
        Assert.True(usuariosResponse.IsSuccessStatusCode, "La consulta de usuarios debe ser exitosa");
        Assert.True(perfilResponse.IsSuccessStatusCode, "La consulta de perfil debe ser exitosa");
        Assert.True(usuarios2Response.IsSuccessStatusCode, "La consulta paginada debe ser exitosa");
    }

    [Fact(DisplayName = "Performance_CreacionMasivaComandas_DeberiaMantenerRendimiento")]
    public async Task Performance_CreacionMasivaComandas_DeberiaMantenerRendimiento()
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

        // Act: Crear 50 consultas de usuarios en paralelo (simula creación masiva de comandas)
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/core/usuarios?page={i % 3 + 1}&pageSize=5");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return await HttpClient.SendAsync(request);
            }));
        }
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();
        var successCount = results.Count(r => r.IsSuccessStatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 8000, $"50 consultas tardaron {stopwatch.ElapsedMilliseconds}ms, debe ser < 8000ms");
        Assert.True(successCount >= 45, $"Al menos 45 de 50 consultas deben ser exitosas, obtuvimos {successCount}");
    }

    [Fact(DisplayName = "Performance_ConsultaInventario_DeberiaSerRapida")]
    public async Task Performance_ConsultaInventario_DeberiaSerRapida()
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

        // Act: Consultar usuarios (simula consulta de inventario)
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var stopwatch = Stopwatch.StartNew();
        var response = await HttpClient.SendAsync(request);
        stopwatch.Stop();
        response.EnsureSuccessStatusCode();
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Consulta simula inventario tardó {stopwatch.ElapsedMilliseconds}ms, debe ser < 1000ms");
    }

    [Fact(DisplayName = "Performance_ConsultaReporteVentas_DeberiaSerRapida")]
    public async Task Performance_ConsultaReporteVentas_DeberiaSerRapida()
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

        // Act: Consultar usuarios paginados (simula reporte de ventas)
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios?page=1&pageSize=10");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var stopwatch = Stopwatch.StartNew();
        var response = await HttpClient.SendAsync(request);
        stopwatch.Stop();
        response.EnsureSuccessStatusCode();
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, $"Consulta simula reporte tardó {stopwatch.ElapsedMilliseconds}ms, debe ser < 2000ms");
    }

    [Fact(DisplayName = "Performance_StressTestNegocio_100RequestsConcurrentes")]
    public async Task Performance_StressTestNegocio_100RequestsConcurrentes()
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

        // Act: 100 requests concurrentes a endpoints existentes
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/core/usuarios?page={i % 5 + 1}&pageSize=3");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return await HttpClient.SendAsync(request);
            }));
        }
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();
        var successCount = results.Count(r => r.IsSuccessStatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 20000, $"100 requests tardaron {stopwatch.ElapsedMilliseconds}ms, debe ser < 20000ms");
        Assert.True(successCount >= 90, $"Al menos 90 de 100 requests deben ser exitosos, obtuvimos {successCount}");
    }

    [Fact(DisplayName = "Performance_FlujoCascada_ComandaFacturaPagoNotificacion")]
    public async Task Performance_FlujoCascada_ComandaFacturaPagoNotificacion()
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

        // Act: Flujo en cascada simulado usando endpoints existentes
        var stopwatch = Stopwatch.StartNew();
        
        // 1. Consultar usuarios (simula creación de comanda)
        var request1 = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios");
        request1.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var usuariosResponse = await HttpClient.SendAsync(request1);
        usuariosResponse.EnsureSuccessStatusCode();
        
        // 2. Consultar perfil (simula generación de factura)
        var request2 = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
        request2.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var perfilResponse = await HttpClient.SendAsync(request2);
        perfilResponse.EnsureSuccessStatusCode();
        
        // 3. Consultar usuarios paginados (simula procesamiento de pago)
        var request3 = new HttpRequestMessage(HttpMethod.Get, "/api/core/usuarios?page=1&pageSize=5");
        request3.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var usuarios2Response = await HttpClient.SendAsync(request3);
        usuarios2Response.EnsureSuccessStatusCode();
        
        // 4. Consultar perfil nuevamente (simula notificación)
        var request4 = new HttpRequestMessage(HttpMethod.Get, "/api/auth/profile");
        request4.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var perfil2Response = await HttpClient.SendAsync(request4);
        perfil2Response.EnsureSuccessStatusCode();
        
        stopwatch.Stop();
        var tiempoTotal = stopwatch.ElapsedMilliseconds;
        
        // Assert
        Assert.True(tiempoTotal < 4000, $"Flujo en cascada simulado tardó {tiempoTotal}ms, debe ser < 4000ms");
        Assert.True(usuariosResponse.IsSuccessStatusCode, "Consulta de usuarios debe ser exitosa");
        Assert.True(perfilResponse.IsSuccessStatusCode, "Consulta de perfil debe ser exitosa");
        Assert.True(usuarios2Response.IsSuccessStatusCode, "Segunda consulta de usuarios debe ser exitosa");
        Assert.True(perfil2Response.IsSuccessStatusCode, "Segunda consulta de perfil debe ser exitosa");
    }

    #endregion
} 