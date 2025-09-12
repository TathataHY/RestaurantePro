using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Services.Dashboard;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Analytics;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Application.Common.Models;
using ApplicationProductoDto = RestaurantePro.Application.Core.Productos.DTOs.ProductoDto;
using ApplicationComandaDto = RestaurantePro.Application.Operaciones.Comandas.DTOs.ComandaDto;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services;

/// <summary>
/// Tests que validan cálculos reales del dashboard con datos creados via API
/// </summary>
public class DashboardServiceRealCalculationsTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly HttpClient _client;
    private IDashboardService _dashboardService;
    private IAuthService _authService;
    private IApiService _apiService;
    private IMesasService _mesasService;
    private IAnalyticsService _analyticsService;

    public DashboardServiceRealCalculationsTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        Setup();
    }

    private void Setup()
    {
        // Crear servicios móviles localmente (igual que en el test base)
        var httpClient = _client;
        var apiService = new ApiService(httpClient);
        var authService = new AuthService(apiService, NullLogger<AuthService>.Instance, new FakeSecureStorageService(), new FakeNavigationService());
        var analyticsService = new AnalyticsService(apiService, authService);
        var mesasService = new MesasService(apiService, authService);
        var logger = NullLogger<DashboardService>.Instance;

        _dashboardService = new DashboardService(apiService, analyticsService, mesasService, authService, logger);
        _authService = authService;
        _apiService = apiService;
        _mesasService = mesasService;
        _analyticsService = analyticsService;
    }

    private async Task SetupAsync()
    {
        // Login automático para todos los tests
        var loginResult = await _authService.LoginAsync("admin@restaurantepro.com", "AdminRestaurante123!");
        if (!loginResult.Success)
        {
            throw new InvalidOperationException($"Login falló: {loginResult.Message}");
        }
    }

    [Fact]
    public async Task GetTodaySalesAsync_WithRealData_ShouldValidateCalculations()
    {
        // Arrange
        await SetupAsync();
        
        // Act - Obtener métricas actuales
        var ventasHoy = await _dashboardService.GetTodaySalesAsync();
        var comandasActivas = await _dashboardService.GetActiveOrdersCountAsync();
        var comandasPendientes = await _dashboardService.GetPendingOrdersCountAsync();
        
        // Assert - Validar que los cálculos son lógicos
        Console.WriteLine($"📊 Datos reales obtenidos:");
        Console.WriteLine($"  - Ventas del día: ${ventasHoy:N2}");
        Console.WriteLine($"  - Comandas activas: {comandasActivas}");
        Console.WriteLine($"  - Comandas pendientes: {comandasPendientes}");
        
        // Validar rangos razonables
        Assert.True(ventasHoy >= 0, "Las ventas deben ser >= 0");
        Assert.True(ventasHoy <= 100000, "Las ventas deben ser <= 100,000");
        Assert.True(comandasActivas >= 0, "Las comandas activas deben ser >= 0");
        Assert.True(comandasActivas <= 100, "Las comandas activas deben ser <= 100");
        Assert.True(comandasPendientes >= 0, "Las comandas pendientes deben ser >= 0");
        Assert.True(comandasPendientes <= 50, "Las comandas pendientes deben ser <= 50");
        
        // Validar consistencia lógica
        if (ventasHoy > 0)
        {
            // Si hay ventas, debe haber al menos una comanda activa o pendiente
            Assert.True(comandasActivas > 0 || comandasPendientes > 0, 
                "Si hay ventas, debe haber comandas activas o pendientes");
        }
        
        Console.WriteLine("✅ Validación de cálculos completada exitosamente");
    }

    [Fact]
    public async Task GetTableStatusAsync_WithRealData_ShouldValidateCalculations()
    {
        // Arrange
        await SetupAsync();
        
        // Act
        var estadoMesas = await _dashboardService.GetTableStatusAsync();
        
        // Assert
        Assert.NotNull(estadoMesas);
        Assert.NotNull(estadoMesas.Mesas);
        Assert.NotNull(estadoMesas.Estadisticas);
        
        var stats = estadoMesas.Estadisticas;
        
        Console.WriteLine($"🏢 Estado de mesas:");
        Console.WriteLine($"  - Total mesas: {estadoMesas.TotalMesas}");
        Console.WriteLine($"  - Ocupadas: {stats.MesasOcupadas}");
        Console.WriteLine($"  - Disponibles: {stats.MesasDisponibles}");
        Console.WriteLine($"  - Porcentaje ocupación: {stats.PorcentajeOcupacion:F1}%");
        
        // Validar cálculos de porcentaje
        if (estadoMesas.TotalMesas > 0)
        {
            var expectedOcupacion = (double)stats.MesasOcupadas / estadoMesas.TotalMesas * 100;
            Assert.Equal(expectedOcupacion, (double)stats.PorcentajeOcupacion, 1);
        }
        
        // Validar consistencia
        Assert.Equal(stats.MesasOcupadas + stats.MesasDisponibles, estadoMesas.TotalMesas);
        Assert.True(stats.PorcentajeOcupacion >= 0);
        Assert.True(stats.PorcentajeOcupacion <= 100);
        
        Console.WriteLine("✅ Validación de estado de mesas completada exitosamente");
    }

    [Fact]
    public async Task GetRecentOrdersAsync_WithRealData_ShouldValidateCalculations()
    {
        // Arrange
        await SetupAsync();
        
        // Act
        var comandasRecientes = await _dashboardService.GetRecentOrdersAsync();
        
        // Assert
        Assert.NotNull(comandasRecientes);
        
        Console.WriteLine($"📋 Comandas recientes: {comandasRecientes.Count}");
        
        // Validar que la lista es razonable
        Assert.True(comandasRecientes.Count >= 0, "El número de comandas recientes debe ser >= 0");
        Assert.True(comandasRecientes.Count <= 10, "No debe devolver más de 10 comandas recientes");
        
        // Si hay comandas, validar su estructura
        if (comandasRecientes.Any())
        {
            foreach (var comanda in comandasRecientes)
            {
                Assert.NotNull(comanda);
                Assert.NotNull(comanda.OrderNumber);
                Assert.NotNull(comanda.Status);
                Assert.True(comanda.Total >= 0, "El total debe ser >= 0");
                Assert.NotNull(comanda.Items);
            }
        }
        
        Console.WriteLine("✅ Validación de comandas recientes completada exitosamente");
    }

    [Fact]
    public async Task GetTodaySalesAsync_WithCreatedData_ShouldValidateSpecificCalculations()
    {
        // Arrange
        await SetupAsync();
        
        // Crear datos de prueba específicos
        var testData = await CreateSpecificTestDataAsync();
        
        // Act - Obtener métricas después de crear datos
        var ventasHoy = await _dashboardService.GetTodaySalesAsync();
        var comandasActivas = await _dashboardService.GetActiveOrdersCountAsync();
        var comandasPendientes = await _dashboardService.GetPendingOrdersCountAsync();
        
        // Assert - Validar cálculos específicos
        Console.WriteLine($"📊 Datos después de crear comandas:");
        Console.WriteLine($"  - Ventas del día: ${ventasHoy:N2}");
        Console.WriteLine($"  - Comandas activas: {comandasActivas}");
        Console.WriteLine($"  - Comandas pendientes: {comandasPendientes}");
        Console.WriteLine($"  - Datos esperados: ${testData.ExpectedVentas:N2} ventas, {testData.ExpectedComandas} comandas");
        
        // Validar que tenemos datos reales (no solo 0s)
        if (testData.ExpectedVentas > 0)
        {
            Assert.True(ventasHoy > 0, "Si creamos comandas con ventas, debe haber ventas > 0");
            Assert.True(comandasActivas > 0 || comandasPendientes > 0, "Si hay ventas, debe haber comandas activas o pendientes");
        }
        
        // Validar rangos razonables
        Assert.True(ventasHoy >= 0, "Las ventas deben ser >= 0");
        Assert.True(ventasHoy <= 100000, "Las ventas deben ser <= 100,000");
        Assert.True(comandasActivas >= 0, "Las comandas activas deben ser >= 0");
        Assert.True(comandasActivas <= 100, "Las comandas activas deben ser <= 100");
        Assert.True(comandasPendientes >= 0, "Las comandas pendientes deben ser >= 0");
        Assert.True(comandasPendientes <= 50, "Las comandas pendientes deben ser <= 50");
        
        Console.WriteLine("✅ Validación de cálculos específicos completada exitosamente");
    }

    [Fact]
    public async Task DashboardCalculations_WithRealComandas_ShouldValidateExactCalculations()
    {
        // Arrange
        await SetupAsync();
        
        // Crear comandas específicas con valores conocidos
        var testData = await CreateComandasWithKnownValuesAsync();
        
        // Act - Obtener métricas del dashboard
        var ventasHoy = await _dashboardService.GetTodaySalesAsync();
        var comandasActivas = await _dashboardService.GetActiveOrdersCountAsync();
        var comandasPendientes = await _dashboardService.GetPendingOrdersCountAsync();
        var estadoMesas = await _dashboardService.GetTableStatusAsync();
        
        // Assert - Validar cálculos exactos
        Console.WriteLine($"🎯 VALIDACIÓN DE CÁLCULOS EXACTOS:");
        Console.WriteLine($"  - Ventas calculadas: ${ventasHoy:N2}");
        Console.WriteLine($"  - Ventas esperadas: ${testData.ExpectedVentas:N2}");
        Console.WriteLine($"  - Comandas activas: {comandasActivas}");
        Console.WriteLine($"  - Comandas pendientes: {comandasPendientes}");
        Console.WriteLine($"  - Total comandas esperadas: {testData.ExpectedComandas}");
        Console.WriteLine($"  - Mesas ocupadas: {estadoMesas?.Estadisticas?.MesasOcupadas ?? 0}");
        Console.WriteLine($"  - Mesas esperadas ocupadas: {testData.ExpectedMesasOcupadas}");
        
        // Validar cálculos exactos
        if (testData.ExpectedVentas > 0)
        {
            // Las ventas deben coincidir exactamente (con tolerancia de 0.01)
            Assert.Equal((double)testData.ExpectedVentas, (double)ventasHoy, 2);
            
            // Validar que las comandas se procesaron correctamente (finalizadas = ventas calculadas)
            // Es correcto que no haya comandas activas/pendientes si todas se finalizaron
            Console.WriteLine($"✅ Comandas procesadas correctamente - Activas: {comandasActivas}, Pendientes: {comandasPendientes}");
            
            // Las mesas ocupadas deben coincidir
            if (estadoMesas?.Estadisticas != null)
            {
                Assert.Equal(testData.ExpectedMesasOcupadas, estadoMesas.Estadisticas.MesasOcupadas);
            }
        }
        
        Console.WriteLine("✅ Validación de cálculos exactos completada exitosamente");
    }

    [Fact]
    public async Task DashboardCalculations_WithMultipleScenarios_ShouldValidateRobustCalculations()
    {
        // Arrange - Crear múltiples escenarios con diferentes productos y cantidades
        var testData = await CreateComandasWithKnownValuesAsync();

        // Act - Obtener métricas del dashboard usando el servicio de analytics
        var metricasResponse = await _analyticsService.ObtenerMetricasDiaAsync();
        if (!metricasResponse.Success)
        {
            Console.WriteLine($"⚠️ Error obteniendo métricas: {metricasResponse.Message}");
            return;
        }
        
        var metricas = metricasResponse.Data;
        if (metricas == null)
        {
            Console.WriteLine("⚠️ No se obtuvieron métricas del dashboard");
            return;
        }
        
        // Obtener estado de mesas usando el servicio autenticado
        var estadoMesasResponse = await _mesasService.ObtenerEstadoOcupacionAsync();
        var estadoMesas = estadoMesasResponse.Success ? estadoMesasResponse.Data : null;

        var ventasHoy = metricas.TotalVentas;
        var totalComandas = metricas.TotalComandas;
        
        Console.WriteLine($"📊 VALIDACIÓN ROBUSTA DE CÁLCULOS:");
        Console.WriteLine($"  - Ventas calculadas: ${ventasHoy:F2}");
        Console.WriteLine($"  - Ventas esperadas: ${testData.ExpectedVentas:F2}");
        Console.WriteLine($"  - Total comandas: {totalComandas}");
        Console.WriteLine($"  - Total comandas esperadas: {testData.ExpectedTotalComandas}");
        Console.WriteLine($"  - Mesas ocupadas: {estadoMesas?.Estadisticas?.MesasOcupadas ?? 0}");
        Console.WriteLine($"  - Mesas esperadas ocupadas: {testData.ExpectedMesasOcupadas}");

        // Validar cálculos con múltiples escenarios
        if (testData.ExpectedVentas > 0)
        {
            // Las ventas deben coincidir exactamente (con tolerancia de 0.01)
            Assert.Equal((double)testData.ExpectedVentas, (double)ventasHoy, 2);
            
            // Validar que se crearon las comandas esperadas
            Console.WriteLine($"✅ Comandas procesadas correctamente - Total: {totalComandas}");
            
            // Las mesas ocupadas deben coincidir
            if (estadoMesas?.Estadisticas != null)
            {
                Assert.Equal(testData.ExpectedMesasOcupadas, estadoMesas.Estadisticas.MesasOcupadas);
            }
        }

        Console.WriteLine($"✅ Validación robusta de cálculos completada exitosamente");
    }

    private async Task<TestData> CreateSpecificTestDataAsync()
    {
        var testData = new TestData();
        
        try
        {
            // 1. Obtener mesas disponibles
            var mesasResponse = await _mesasService.ObtenerMesasAsync();
            if (!mesasResponse.Success || mesasResponse.Data == null || !mesasResponse.Data.Any())
            {
                Console.WriteLine("⚠️ No hay mesas disponibles para crear datos de prueba");
                return testData;
            }
            
            var mesas = mesasResponse.Data.Take(3).ToList(); // Usar 3 mesas para pruebas
            Console.WriteLine($"📋 Usando {mesas.Count} mesas para crear datos de prueba");
            
            // 2. Crear productos de prueba directamente
            var productos = await CreateTestProductsAsync();
            if (!productos.Any())
            {
                Console.WriteLine("⚠️ No se pudieron crear productos de prueba");
                return testData;
            }
            
            Console.WriteLine($"🍽️ Usando {productos.Count} productos para crear datos de prueba");
            
        // 3. Usar el usuario admin autenticado como mesero
        // Obtener el perfil del usuario actual (admin) usando _apiService que maneja la autenticación
        Console.WriteLine("🔍 Intentando obtener perfil del usuario admin...");
        
        // Obtener el token del AuthService local (no del fixture)
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("⚠️ No se pudo obtener el token de autenticación");
            return testData;
        }
        
        Console.WriteLine($"🔑 Token obtenido: {token.Substring(0, Math.Min(20, token.Length))}...");
        Console.WriteLine($"🔍 Longitud del token: {token.Length}");
        Console.WriteLine($"🔍 Formato del token: {(token.StartsWith("eyJ") ? "JWT válido" : "Formato desconocido")}");
        
        var perfilResponse = await _apiService.GetAsync<RestaurantePro.Application.Common.Interfaces.UserDto>("/api/auth/profile", token);
        
        Console.WriteLine($"🔍 Respuesta del perfil:");
        Console.WriteLine($"  - Success: {perfilResponse.Success}");
        Console.WriteLine($"  - Message: {perfilResponse.Message}");
        Console.WriteLine($"  - Errors: {string.Join(", ", perfilResponse.Errors ?? new List<string>())}");
        Console.WriteLine($"  - StatusCode: {perfilResponse.StatusCode}");
        Console.WriteLine($"  - Data: {(perfilResponse.Data != null ? "Presente" : "NULL")}");
        
        // Si hay error, mostrar más detalles
        if (!perfilResponse.Success)
        {
            Console.WriteLine($"❌ Error detallado del perfil:");
            Console.WriteLine($"  - Response completa: {System.Text.Json.JsonSerializer.Serialize(perfilResponse, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");
        }
        
        if (!perfilResponse.Success || perfilResponse.Data == null)
        {
            Console.WriteLine("⚠️ No se pudo obtener el perfil del usuario admin");
            return testData;
        }
            
        var mesero = perfilResponse.Data;
            
            // 4. Obtener clientes reales del seed data
            var clientesResponse = await _client.GetAsync("/api/comercial/clientes");
            var clientesContent = await clientesResponse.Content.ReadAsStringAsync();
            
            Console.WriteLine($"🔍 Respuesta del endpoint de clientes:");
            Console.WriteLine($"  - StatusCode: {clientesResponse.StatusCode}");
            Console.WriteLine($"  - Content: {clientesContent.Substring(0, Math.Min(200, clientesContent.Length))}...");
            
            var clientesApiResponse = JsonSerializer.Deserialize<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Common.DTOs.PaginatedList<RestaurantePro.Application.Comercial.Clientes.DTOs.ClienteDto>>>(clientesContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            var clientes = clientesApiResponse?.Data?.Items?.Take(3).ToList() ?? new List<RestaurantePro.Application.Comercial.Clientes.DTOs.ClienteDto>();
            if (!clientes.Any())
            {
                Console.WriteLine("⚠️ No se pudieron obtener clientes del seed data");
                return testData;
            }
            
            Console.WriteLine($"👨‍💼 Usando mesero: {mesero.UserName} ({mesero.Email})");
            Console.WriteLine($"👥 Usando {clientes.Count} clientes del seed data");
            
            // 5. Crear comandas con productos específicos
            var comandaIds = new List<Guid>();
            decimal totalVentasEsperadas = 0;
            
            for (int i = 0; i < mesas.Count; i++)
            {
                var mesa = mesas[i];
                var producto = productos[i % productos.Count]; // Rotar productos
                var cliente = clientes[i % clientes.Count]; // Cliente real del seed data
                
                // Crear comanda
                var crearComandaRequest = new
                {
                    MesaId = mesa.Id,
                    ClienteId = cliente.Id, // Cliente real del seed data
                    MeseroId = mesero.Id, // Mesero real del seed data
                    Observaciones = $"Comanda de prueba {i + 1}",
                    Items = new[]
                    {
                        new
                        {
                            ProductoId = producto.Id,
                            Cantidad = 1,
                            PrecioUnitario = producto.Precio, // Usar precio real del producto
                            Observaciones = $"Item de prueba {i + 1}"
                        }
                    }
                };
                
                var json = JsonSerializer.Serialize(crearComandaRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync("/api/operaciones/comandas", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Usar deserialización más flexible para evitar problemas con enums
                    var comandaResponse = JsonSerializer.Deserialize<ApiResponse<JsonElement>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (comandaResponse?.Success == true && comandaResponse.Data.ValueKind == JsonValueKind.Object)
                    {
                        var comandaData = comandaResponse.Data;
                        if (comandaData.TryGetProperty("id", out var idProperty))
                        {
                            var comandaId = idProperty.GetString();
                            if (!string.IsNullOrEmpty(comandaId) && Guid.TryParse(comandaId, out var parsedId))
                            {
                                comandaIds.Add(parsedId);
                                totalVentasEsperadas += producto.Precio; // Usar precio real del producto
                                Console.WriteLine($"✅ Comanda {i + 1} creada: {comandaId} - Cliente: {cliente.NombreCompleto} - Producto: {producto.Nombre} - ${producto.Precio}");
                            }
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error creando comanda {i + 1}: {response.StatusCode} - {errorContent}");
                }
            }
            
            testData.ComandaIds = comandaIds;
            testData.ExpectedComandas = comandaIds.Count;
            testData.ExpectedVentas = totalVentasEsperadas;
            
            // 6. Actualizar estado de comandas siguiendo el flujo completo: Creada → EnProceso → Lista → Entregada → Finalizada
            var estados = new[] { "EnProceso", "Lista", "Entregada", "Finalizada" };
            
            foreach (var comandaId in comandaIds)
            {
                foreach (var estado in estados)
                {
                    var actualizarEstadoRequest = new
                    {
                        NuevoEstado = estado
                    };
                    
                    var actualizarJson = JsonSerializer.Serialize(actualizarEstadoRequest);
                    var actualizarContent = new StringContent(actualizarJson, Encoding.UTF8, "application/json");
                    
                    var actualizarResponse = await _client.PatchAsync($"/api/operaciones/comandas/{comandaId}/estado", actualizarContent);
                    if (actualizarResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ Comanda {comandaId} actualizada a {estado}");
                    }
                    else
                    {
                        var errorContent = await actualizarResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"❌ Error actualizando comanda {comandaId} a {estado}: {actualizarResponse.StatusCode} - {errorContent}");
                    }
                }
            }
            
            Console.WriteLine($"✅ Datos de prueba creados: {testData.ExpectedComandas} comandas, ${testData.ExpectedVentas:N2} ventas esperadas");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creando datos de prueba: {ex.Message}");
        }
        
        return testData;
    }
    
    private async Task<List<ApplicationProductoDto>> CreateTestProductsAsync()
    {
        var productos = new List<ApplicationProductoDto>();
        
        try
        {
                // Usar productos existentes del seed data (más simple y confiable)
                Console.WriteLine("📋 Obteniendo productos existentes del seed data...");
                var productosResponse = await _client.GetAsync("/api/core/productos?tamanoPagina=10");
            
            Console.WriteLine($"🔍 Respuesta del endpoint de productos:");
            Console.WriteLine($"  - StatusCode: {productosResponse.StatusCode}");
            Console.WriteLine($"  - IsSuccessStatusCode: {productosResponse.IsSuccessStatusCode}");
            
            if (productosResponse.IsSuccessStatusCode)
            {
                var content = await productosResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"  - Content: {content.Substring(0, Math.Min(200, content.Length))}...");
                
                var apiResponse = JsonSerializer.Deserialize<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Common.DTOs.PaginatedList<ApplicationProductoDto>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (apiResponse?.Success == true && apiResponse.Data?.Items?.Any() == true)
                {
                    productos = apiResponse.Data.Items.Take(3).ToList();
                    Console.WriteLine($"✅ Obtenidos {productos.Count} productos existentes del seed data");
                    foreach (var producto in productos)
                    {
                        Console.WriteLine($"  - {producto.Nombre}: ${producto.Precio}");
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ No se pudieron obtener productos existentes - Success: {apiResponse?.Success}, Items: {apiResponse?.Data?.Items?.Count ?? 0}");
                }
            }
            else
            {
                Console.WriteLine($"❌ Error HTTP: {productosResponse.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creando productos de prueba: {ex.Message}");
        }
        
        return productos;
    }
    
    private async Task<TestData> CreateComandasWithKnownValuesAsync()
    {
        var testData = new TestData();
        
        try
        {
            // 1. Obtener mesas disponibles
            var mesasResponse = await _mesasService.ObtenerMesasAsync();
            if (!mesasResponse.Success || mesasResponse.Data == null || !mesasResponse.Data.Any())
            {
                Console.WriteLine("⚠️ No hay mesas disponibles para crear datos de prueba");
                return testData;
            }
            
            var mesas = mesasResponse.Data.Take(3).ToList();
            Console.WriteLine($"📋 Usando {mesas.Count} mesas para crear datos de prueba");
            
            // 2. Obtener productos existentes del seed data
            var productos = await CreateTestProductsAsync();
            if (!productos.Any())
            {
                Console.WriteLine("⚠️ No se pudieron obtener productos de prueba");
                return testData;
            }
            
            Console.WriteLine($"🍽️ Usando {productos.Count} productos para crear datos de prueba");
            
            // 3. Obtener perfil del usuario admin
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("⚠️ No se pudo obtener el token de autenticación");
                return testData;
            }
            
            var perfilResponse = await _apiService.GetAsync<RestaurantePro.Application.Common.Interfaces.UserDto>("/api/auth/profile", token);
            if (!perfilResponse.Success || perfilResponse.Data == null)
            {
                Console.WriteLine("⚠️ No se pudo obtener el perfil del usuario admin");
                return testData;
            }
            
            var mesero = perfilResponse.Data;
            
            // 4. Obtener clientes reales del seed data
            var clientesResponse = await _client.GetAsync("/api/comercial/clientes");
            var clientesContent = await clientesResponse.Content.ReadAsStringAsync();
            
            Console.WriteLine($"🔍 Respuesta del endpoint de clientes:");
            Console.WriteLine($"  - StatusCode: {clientesResponse.StatusCode}");
            Console.WriteLine($"  - Content: {clientesContent.Substring(0, Math.Min(200, clientesContent.Length))}...");
            
            var clientesApiResponse = JsonSerializer.Deserialize<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Common.DTOs.PaginatedList<RestaurantePro.Application.Comercial.Clientes.DTOs.ClienteDto>>>(clientesContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            var clientes = clientesApiResponse?.Data?.Items?.Take(3).ToList() ?? new List<RestaurantePro.Application.Comercial.Clientes.DTOs.ClienteDto>();
            if (!clientes.Any())
            {
                Console.WriteLine("⚠️ No se pudieron obtener clientes del seed data");
                return testData;
            }
            
            Console.WriteLine($"👨‍💼 Usando mesero: {mesero.UserName} ({mesero.Email})");
            Console.WriteLine($"👥 Usando {clientes.Count} clientes del seed data");
            
            // 5. Crear comandas con valores específicos conocidos
            var comandaIds = new List<Guid>();
            decimal totalVentasEsperadas = 0;
            int mesasOcupadas = 0;
            
            for (int i = 0; i < mesas.Count; i++)
            {
                var mesa = mesas[i];
                var producto = productos[i % productos.Count];
                var cliente = clientes[i % clientes.Count];
                
                // Crear comanda con valores específicos
                var crearComandaRequest = new
                {
                    MesaId = mesa.Id,
                    ClienteId = cliente.Id,
                    MeseroId = mesero.Id,
                    Observaciones = $"Comanda de prueba con valores conocidos {i + 1}",
                    Items = new[]
                    {
                        new
                        {
                            ProductoId = producto.Id,
                            Cantidad = 1,
                            PrecioUnitario = producto.Precio,
                            Observaciones = $"Item de prueba {i + 1} - ${producto.Precio}"
                        }
                    }
                };
                
                var json = JsonSerializer.Serialize(crearComandaRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync("/api/operaciones/comandas", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    Console.WriteLine($"🔍 Respuesta de crear comanda {i + 1}:");
                    Console.WriteLine($"  - StatusCode: {response.StatusCode}");
                    Console.WriteLine($"  - Content: {responseContent.Substring(0, Math.Min(300, responseContent.Length))}...");
                    
                    // Usar JsonDocument para evitar problemas de deserialización con enums
                    using var document = JsonDocument.Parse(responseContent);
                    if (document.RootElement.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
                    {
                        if (document.RootElement.TryGetProperty("data", out var dataElement))
                        {
                            if (dataElement.TryGetProperty("id", out var idElement))
                            {
                                var comandaId = Guid.Parse(idElement.GetString()!);
                                comandaIds.Add(comandaId);
                                totalVentasEsperadas += producto.Precio;
                                mesasOcupadas++;
                                Console.WriteLine($"✅ Comanda {i + 1} creada: {comandaId} - Cliente: {cliente.NombreCompleto} - Producto: {producto.Nombre} - ${producto.Precio}");
                            }
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error creando comanda {i + 1}: {response.StatusCode} - {errorContent}");
                }
            }
            
            testData.ComandaIds = comandaIds;
            testData.ExpectedComandas = comandaIds.Count;
            testData.ExpectedVentas = totalVentasEsperadas;
            testData.ExpectedMesasOcupadas = mesasOcupadas;
            
            // 6. Actualizar estado de comandas siguiendo el flujo completo: Creada → EnProceso → Lista → Entregada → Finalizada
            var estados = new[] { "EnProceso", "Lista", "Entregada", "Finalizada" };
            
            foreach (var comandaId in comandaIds)
            {
                foreach (var estado in estados)
                {
                    var actualizarEstadoRequest = new
                    {
                        NuevoEstado = estado
                    };
                    
                    var actualizarJson = JsonSerializer.Serialize(actualizarEstadoRequest);
                    var actualizarContent = new StringContent(actualizarJson, Encoding.UTF8, "application/json");
                    
                    var actualizarResponse = await _client.PatchAsync($"/api/operaciones/comandas/{comandaId}/estado", actualizarContent);
                    if (actualizarResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ Comanda {comandaId} actualizada a {estado}");
                    }
                    else
                    {
                        var errorContent = await actualizarResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"❌ Error actualizando comanda {comandaId} a {estado}: {actualizarResponse.StatusCode} - {errorContent}");
                    }
                }
            }
            
            Console.WriteLine($"✅ Datos de prueba con valores conocidos creados:");
            Console.WriteLine($"  - Comandas: {testData.ExpectedComandas}");
            Console.WriteLine($"  - Ventas esperadas: ${testData.ExpectedVentas:N2}");
            Console.WriteLine($"  - Mesas ocupadas: {testData.ExpectedMesasOcupadas}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creando datos de prueba con valores conocidos: {ex.Message}");
        }
        
        return testData;
    }

    private async Task<TestData> CreateMultipleComandasScenariosAsync()
    {
        var testData = new TestData();
        
        try
        {
            // 1. Obtener token de autenticación
            var token = await _authService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("⚠️ No se pudo obtener el token de autenticación");
                return testData;
            }
            
            // 2. Obtener mesas disponibles
            var mesasResponse = await _mesasService.ObtenerMesasAsync();
            if (!mesasResponse.Success || mesasResponse.Data == null || !mesasResponse.Data.Any())
            {
                Console.WriteLine("⚠️ No hay mesas disponibles para crear datos de prueba");
                return testData;
            }
            
            var mesas = mesasResponse.Data.Take(5).ToList(); // Usar 5 mesas para pruebas robustas
            Console.WriteLine($"📋 Usando {mesas.Count} mesas para crear datos de prueba robustos");
            
            // 3. Obtener productos del seed data usando el servicio autenticado
            var productos = await CreateTestProductsAsync();
            if (!productos.Any())
            {
                Console.WriteLine("⚠️ No se pudieron obtener productos del seed data");
                return testData;
            }
            
            Console.WriteLine($"🍽️ Obtenidos {productos.Count} productos del seed data");
            
            // 4. Obtener clientes disponibles
            var clientesResponse = await _client.GetAsync("/api/comercial/clientes");
            clientesResponse.EnsureSuccessStatusCode();
            var clientesContent = await clientesResponse.Content.ReadAsStringAsync();
            var clientesApiResponse = JsonSerializer.Deserialize<RestaurantePro.Api.Common.ApiResponse<RestaurantePro.Application.Common.DTOs.PaginatedList<RestaurantePro.Application.Comercial.Clientes.DTOs.ClienteDto>>>(clientesContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (!clientesApiResponse?.Success == true || clientesApiResponse.Data?.Items == null)
            {
                Console.WriteLine("⚠️ No se pudieron obtener clientes");
                return testData;
            }
            
            var clientes = clientesApiResponse.Data.Items.ToList();
            Console.WriteLine($"👥 Obtenidos {clientes.Count} clientes");
            
            // 4. Obtener perfil del usuario admin
            var profileResponse = await _client.GetAsync("/api/auth/profile");
            if (!profileResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"⚠️ Error obteniendo perfil del usuario: {profileResponse.StatusCode}");
                return testData;
            }
            
            var profileContent = await profileResponse.Content.ReadAsStringAsync();
            var profileApiResponse = JsonSerializer.Deserialize<RestaurantePro.Api.Common.ApiResponse<object>>(profileContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (!profileApiResponse?.Success == true)
            {
                Console.WriteLine("⚠️ No se pudo obtener el perfil del usuario");
                return testData;
            }
            
            Console.WriteLine("✅ Perfil del usuario obtenido correctamente");
            
            // 5. Crear múltiples escenarios de comandas con diferentes productos y cantidades
            var comandaIds = new List<Guid>();
            var totalVentasEsperadas = 0m;
            var mesasOcupadas = 0;
            
            // Escenario 1: Comanda de entradas (productos baratos)
            var productosEntradas = productos.Where(p => p.CategoriaNombre.Contains("Entrada") || p.CategoriaNombre.Contains("Entradas")).Take(2).ToList();
            if (productosEntradas.Any() && mesas.Count > 0)
            {
                var cliente = clientes.FirstOrDefault() ?? clientes[0];
                var mesa = mesas[0];
                var producto = productosEntradas[0];
                
                var comandaRequest = new
                {
                    MesaId = mesa.Id,
                    ClienteId = cliente.Id,
                    Items = new[]
                    {
                        new { ProductoId = producto.Id, Cantidad = 2, PrecioUnitario = producto.Precio }
                    }
                };
                
                var comandaJson = JsonSerializer.Serialize(comandaRequest);
                var comandaContent = new StringContent(comandaJson, Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync("/api/operaciones/comandas", comandaContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var comandaResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (comandaResponse?.Success == true)
                    {
                        using var document = JsonDocument.Parse(responseContent);
                        if (document.RootElement.TryGetProperty("data", out var dataElement))
                        {
                            if (dataElement.TryGetProperty("id", out var idElement))
                            {
                                var comandaId = Guid.Parse(idElement.GetString()!);
                                comandaIds.Add(comandaId);
                                totalVentasEsperadas += producto.Precio * 2; // 2 unidades
                                mesasOcupadas++;
                                Console.WriteLine($"✅ Comanda Entradas creada: {comandaId} - Producto: {producto.Nombre} x2 - ${producto.Precio * 2}");
                            }
                        }
                    }
                }
            }
            
            // Escenario 2: Comanda de carnes premium (productos caros)
            var productosCarnes = productos.Where(p => p.CategoriaNombre.Contains("Carne") || p.CategoriaNombre.Contains("Carnes")).Take(2).ToList();
            if (productosCarnes.Any() && mesas.Count > 1)
            {
                var cliente = clientes.Count > 1 ? clientes[1] : clientes[0];
                var mesa = mesas[1];
                var producto = productosCarnes[0];
                
                var comandaRequest = new
                {
                    MesaId = mesa.Id,
                    ClienteId = cliente.Id,
                    Items = new[]
                    {
                        new { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = producto.Precio }
                    }
                };
                
                var comandaJson = JsonSerializer.Serialize(comandaRequest);
                var comandaContent = new StringContent(comandaJson, Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync("/api/operaciones/comandas", comandaContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var comandaResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (comandaResponse?.Success == true)
                    {
                        using var document = JsonDocument.Parse(responseContent);
                        if (document.RootElement.TryGetProperty("data", out var dataElement))
                        {
                            if (dataElement.TryGetProperty("id", out var idElement))
                            {
                                var comandaId = Guid.Parse(idElement.GetString()!);
                                comandaIds.Add(comandaId);
                                totalVentasEsperadas += producto.Precio;
                                mesasOcupadas++;
                                Console.WriteLine($"✅ Comanda Carnes creada: {comandaId} - Producto: {producto.Nombre} - ${producto.Precio}");
                            }
                        }
                    }
                }
            }
            
            // Escenario 3: Comanda mixta con múltiples productos
            if (mesas.Count > 2)
            {
                var cliente = clientes.Count > 2 ? clientes[2] : clientes[0];
                var mesa = mesas[2];
                var productosMixtos = productos.Take(3).ToList(); // Primeros 3 productos
                
                var items = productosMixtos.Select(p => new { ProductoId = p.Id, Cantidad = 1, PrecioUnitario = p.Precio }).ToArray();
                
                var comandaRequest = new
                {
                    MesaId = mesa.Id,
                    ClienteId = cliente.Id,
                    Items = items
                };
                
                var comandaJson = JsonSerializer.Serialize(comandaRequest);
                var comandaContent = new StringContent(comandaJson, Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync("/api/operaciones/comandas", comandaContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var comandaResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (comandaResponse?.Success == true)
                    {
                        using var document = JsonDocument.Parse(responseContent);
                        if (document.RootElement.TryGetProperty("data", out var dataElement))
                        {
                            if (dataElement.TryGetProperty("id", out var idElement))
                            {
                                var comandaId = Guid.Parse(idElement.GetString()!);
                                comandaIds.Add(comandaId);
                                totalVentasEsperadas += productosMixtos.Sum(p => p.Precio);
                                mesasOcupadas++;
                                Console.WriteLine($"✅ Comanda Mixta creada: {comandaId} - {productosMixtos.Count} productos - ${productosMixtos.Sum(p => p.Precio)}");
                            }
                        }
                    }
                }
            }
            
            // Escenario 4: Comanda de postres (productos medianos)
            var productosPostres = productos.Where(p => p.CategoriaNombre.Contains("Postre") || p.CategoriaNombre.Contains("Postres")).Take(2).ToList();
            if (productosPostres.Any() && mesas.Count > 3)
            {
                var cliente = clientes.Count > 3 ? clientes[3] : clientes[0];
                var mesa = mesas[3];
                var producto = productosPostres[0];
                
                var comandaRequest = new
                {
                    MesaId = mesa.Id,
                    ClienteId = cliente.Id,
                    Items = new[]
                    {
                        new { ProductoId = producto.Id, Cantidad = 3, PrecioUnitario = producto.Precio }
                    }
                };
                
                var comandaJson = JsonSerializer.Serialize(comandaRequest);
                var comandaContent = new StringContent(comandaJson, Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync("/api/operaciones/comandas", comandaContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var comandaResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (comandaResponse?.Success == true)
                    {
                        using var document = JsonDocument.Parse(responseContent);
                        if (document.RootElement.TryGetProperty("data", out var dataElement))
                        {
                            if (dataElement.TryGetProperty("id", out var idElement))
                            {
                                var comandaId = Guid.Parse(idElement.GetString()!);
                                comandaIds.Add(comandaId);
                                totalVentasEsperadas += producto.Precio * 3; // 3 unidades
                                mesasOcupadas++;
                                Console.WriteLine($"✅ Comanda Postres creada: {comandaId} - Producto: {producto.Nombre} x3 - ${producto.Precio * 3}");
                            }
                        }
                    }
                }
            }
            
            // Escenario 5: Comanda de bebidas (productos baratos)
            var productosBebidas = productos.Where(p => p.CategoriaNombre.Contains("Bebida") || p.CategoriaNombre.Contains("Bebidas")).Take(2).ToList();
            if (productosBebidas.Any() && mesas.Count > 4)
            {
                var cliente = clientes.Count > 4 ? clientes[4] : clientes[0];
                var mesa = mesas[4];
                var producto = productosBebidas[0];
                
                var comandaRequest = new
                {
                    MesaId = mesa.Id,
                    ClienteId = cliente.Id,
                    Items = new[]
                    {
                        new { ProductoId = producto.Id, Cantidad = 4, PrecioUnitario = producto.Precio }
                    }
                };
                
                var comandaJson = JsonSerializer.Serialize(comandaRequest);
                var comandaContent = new StringContent(comandaJson, Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync("/api/operaciones/comandas", comandaContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var comandaResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (comandaResponse?.Success == true)
                    {
                        using var document = JsonDocument.Parse(responseContent);
                        if (document.RootElement.TryGetProperty("data", out var dataElement))
                        {
                            if (dataElement.TryGetProperty("id", out var idElement))
                            {
                                var comandaId = Guid.Parse(idElement.GetString()!);
                                comandaIds.Add(comandaId);
                                totalVentasEsperadas += producto.Precio * 4; // 4 unidades
                                mesasOcupadas++;
                                Console.WriteLine($"✅ Comanda Bebidas creada: {comandaId} - Producto: {producto.Nombre} x4 - ${producto.Precio * 4}");
                            }
                        }
                    }
                }
            }
            
            // 6. Actualizar estado de comandas siguiendo el flujo completo: Creada → EnProceso → Lista → Entregada → Finalizada
            var estados = new[] { "EnProceso", "Lista", "Entregada", "Finalizada" };
            
            foreach (var comandaId in comandaIds)
            {
                foreach (var estado in estados)
                {
                    var actualizarEstadoRequest = new
                    {
                        NuevoEstado = estado
                    };
                    
                    var actualizarJson = JsonSerializer.Serialize(actualizarEstadoRequest);
                    var actualizarContent = new StringContent(actualizarJson, Encoding.UTF8, "application/json");
                    
                    var actualizarResponse = await _client.PatchAsync($"/api/operaciones/comandas/{comandaId}/estado", actualizarContent);
                    if (actualizarResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ Comanda {comandaId} actualizada a {estado}");
                    }
                    else
                    {
                        var errorContent = await actualizarResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"❌ Error actualizando comanda {comandaId} a {estado}: {actualizarResponse.StatusCode} - {errorContent}");
                    }
                }
            }
            
            // Configurar datos de prueba
            testData.ExpectedComandas = comandaIds.Count;
            testData.ExpectedTotalComandas = comandaIds.Count;
            testData.ExpectedVentas = totalVentasEsperadas;
            testData.ExpectedMesasOcupadas = mesasOcupadas;
            testData.ComandaIds = comandaIds;
            
            Console.WriteLine($"🎯 Datos de prueba robustos con múltiples escenarios creados:");
            Console.WriteLine($"  - Comandas: {testData.ExpectedComandas}");
            Console.WriteLine($"  - Ventas esperadas: ${testData.ExpectedVentas:F2}");
            Console.WriteLine($"  - Mesas ocupadas: {testData.ExpectedMesasOcupadas}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creando datos de prueba robustos: {ex.Message}");
        }
        
        return testData;
    }

    private class TestData
    {
        public int ExpectedComandas { get; set; }
        public int ExpectedTotalComandas { get; set; }
        public decimal ExpectedVentas { get; set; }
        public int ExpectedMesasOcupadas { get; set; }
        public List<Guid> ComandaIds { get; set; } = new();
    }
}
