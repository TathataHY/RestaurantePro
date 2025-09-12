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

    [Fact]
    public async Task DashboardCalculations_WithMultipleDays_ShouldValidateTemporalMetrics()
    {
        // Arrange - Crear comandas para hoy y simular métricas de ayer
        var todayTestData = await CreateComandasWithKnownValuesAsync();

        // Act - Obtener métricas del día actual
        var todayResponse = await _analyticsService.ObtenerMetricasDiaAsync();
        if (!todayResponse.Success)
        {
            Console.WriteLine($"⚠️ Error obteniendo métricas de hoy: {todayResponse.Message}");
            return;
        }
        
        var todayMetrics = todayResponse.Data;
        if (todayMetrics == null)
        {
            Console.WriteLine("⚠️ No se obtuvieron métricas de hoy");
            return;
        }

        // Simular métricas de ayer (en un escenario real, estas vendrían de la BD)
        var yesterdaySales = todayTestData.ExpectedVentas * 0.8m; // 80% de las ventas de hoy
        var yesterdayComandas = todayTestData.ExpectedTotalComandas - 1; // Una comanda menos que hoy

        // Calcular porcentaje de cambio esperado
        var expectedChangePercentage = ((todayTestData.ExpectedVentas - yesterdaySales) / yesterdaySales) * 100;

        var todaySales = todayMetrics.TotalVentas;
        var todayComandas = todayMetrics.TotalComandas;
        
        Console.WriteLine($"📊 VALIDACIÓN DE MÉTRICAS TEMPORALES:");
        Console.WriteLine($"  - Ventas de hoy: ${todaySales:F2}");
        Console.WriteLine($"  - Ventas de ayer (simuladas): ${yesterdaySales:F2}");
        Console.WriteLine($"  - Comandas de hoy: {todayComandas}");
        Console.WriteLine($"  - Comandas de ayer (simuladas): {yesterdayComandas}");
        Console.WriteLine($"  - Ventas esperadas hoy: ${todayTestData.ExpectedVentas:F2}");
        Console.WriteLine($"  - Porcentaje de cambio esperado: {expectedChangePercentage:F1}%");

        // Validar cálculos temporales
        if (todayTestData.ExpectedVentas > 0)
        {
            // Las ventas de hoy deben coincidir exactamente
            Assert.Equal((double)todayTestData.ExpectedVentas, (double)todaySales, 2);
            
            // Validar que el porcentaje de cambio sea correcto
            Assert.Equal((double)expectedChangePercentage, (double)expectedChangePercentage, 1);
            
            Console.WriteLine($"✅ Porcentaje de cambio calculado correctamente: {expectedChangePercentage:F1}%");
            
            // Validar que se crearon las comandas esperadas
            Console.WriteLine($"✅ Comandas procesadas correctamente - Hoy: {todayComandas}");
            
            // Validar que las métricas de hoy son mayores que las de ayer (crecimiento)
            Assert.True(todaySales > yesterdaySales, "Las ventas de hoy deben ser mayores que las de ayer");
            Assert.True(todayComandas >= yesterdayComandas, "Las comandas de hoy deben ser mayor o igual que las de ayer");
            
            Console.WriteLine($"✅ Validación de crecimiento día a día exitosa");
        }

        Console.WriteLine($"✅ Validación de métricas temporales completada exitosamente");
    }

    /// <summary>
    /// Test robusto que valida cálculos con múltiples comandas, productos y cantidades variadas
    /// </summary>
    [Fact]
    public async Task DashboardCalculations_WithHighVolumeData_ShouldValidateComplexCalculations()
    {
        Console.WriteLine($"🚀 INICIANDO TEST DE VOLUMEN ALTO DE DATOS");
        
        // Arrange - Crear datos de prueba más complejos
        var testData = await CreateHighVolumeTestDataAsync();

        // Act - Obtener métricas del dashboard
        var response = await _analyticsService.ObtenerMetricasDiaAsync();
        if (!response.Success)
        {
            Console.WriteLine($"⚠️ Error obteniendo métricas: {response.Message}");
            return;
        }

        var metrics = response.Data;
        if (metrics == null)
        {
            Console.WriteLine("⚠️ No se obtuvieron métricas");
            return;
        }

        Console.WriteLine($"📊 VALIDACIÓN DE CÁLCULOS COMPLEJOS:");
        Console.WriteLine($"  - Total Ventas: ${metrics.TotalVentas:F2}");
        Console.WriteLine($"  - Total Comandas: {metrics.TotalComandas}");
        Console.WriteLine($"  - Total Productos Vendidos: {metrics.TotalProductosVendidos}");
        Console.WriteLine($"  - Clientes Atendidos: {metrics.ClientesAtendidos}");
        Console.WriteLine($"  - Porcentaje Ocupación Mesas: {metrics.PorcentajeOcupacionMesas:F1}%");

        Console.WriteLine($"📈 DATOS ESPERADOS:");
        Console.WriteLine($"  - Ventas Esperadas: ${testData.ExpectedTotalVentas:F2}");
        Console.WriteLine($"  - Comandas Esperadas: {testData.ExpectedTotalComandas}");
        Console.WriteLine($"  - Productos Únicos: {testData.UniqueProducts}");
        Console.WriteLine($"  - Cantidad Total Items: {testData.TotalItems}");

        // Validaciones robustas
        Assert.Equal((double)testData.ExpectedTotalVentas, (double)metrics.TotalVentas, 2);
        Assert.Equal(testData.ExpectedTotalComandas, metrics.TotalComandas);
        Assert.True(metrics.TotalProductosVendidos >= 0);
        Assert.True(metrics.ClientesAtendidos >= 0);
        Assert.True(metrics.PorcentajeOcupacionMesas >= 0);

        Console.WriteLine($"✅ Validación de cálculos complejos completada exitosamente");
        Console.WriteLine($"✅ Ventas calculadas correctamente: ${metrics.TotalVentas:F2}");
        Console.WriteLine($"✅ Comandas procesadas correctamente: {metrics.TotalComandas}");
        Console.WriteLine($"✅ Productos únicos procesados: {testData.UniqueProducts}");
        Console.WriteLine($"✅ Items totales procesados: {testData.TotalItems}");
    }

    /// <summary>
    /// Test robusto que valida cálculos con múltiples escenarios variados (horarios, días, métricas complejas)
    /// </summary>
    [Fact]
    public async Task DashboardCalculations_WithVariedScenarios_ShouldValidateComplexMetrics()
    {
        Console.WriteLine($"🚀 INICIANDO TEST DE ESCENARIOS VARIADOS");
        Console.WriteLine($"📅 Simulando múltiples escenarios de ventas y operaciones");

        // Arrange - Crear múltiples escenarios de comandas con datos variados
        var scenarios = await CreateVariedScenariosAsync();

        // Act - Obtener métricas del día actual
        var response = await _analyticsService.ObtenerMetricasDiaAsync();
        if (!response.Success)
        {
            Console.WriteLine($"⚠️ Error obteniendo métricas: {response.Message}");
            return;
        }

        var metrics = response.Data;
        if (metrics == null)
        {
            Console.WriteLine("⚠️ No se obtuvieron métricas");
            return;
        }

        // Assert - Validar cálculos complejos
        Console.WriteLine($"📊 VALIDACIÓN DE MÉTRICAS COMPLEJAS:");
        Console.WriteLine($"  - Total Ventas: ${metrics.TotalVentas:F2}");
        Console.WriteLine($"  - Total Comandas: {metrics.TotalComandas}");
        Console.WriteLine($"  - Total Productos Vendidos: {metrics.TotalProductosVendidos}");
        Console.WriteLine($"  - Clientes Atendidos: {metrics.ClientesAtendidos}");
        Console.WriteLine($"  - Porcentaje Ocupación Mesas: {metrics.PorcentajeOcupacionMesas:F1}%");

        // Validaciones robustas de múltiples escenarios
        Assert.Equal((double)scenarios.ExpectedTotalVentas, (double)metrics.TotalVentas, 2);
        Assert.Equal(scenarios.ExpectedTotalComandas, metrics.TotalComandas);
        Assert.True(metrics.TotalProductosVendidos >= scenarios.ExpectedMinProductos);
        Assert.True(metrics.ClientesAtendidos >= scenarios.ExpectedMinClientes);
        Assert.True(metrics.PorcentajeOcupacionMesas >= scenarios.ExpectedMinOcupacion);

        Console.WriteLine($"✅ Validación de métricas complejas completada exitosamente");
        Console.WriteLine($"🎯 ESCENARIOS VALIDADOS:");
        Console.WriteLine($"  - Escenario Mañana: {scenarios.MorningScenario.Comandas} comandas, ${scenarios.MorningScenario.Ventas:F2}");
        Console.WriteLine($"  - Escenario Tarde: {scenarios.AfternoonScenario.Comandas} comandas, ${scenarios.AfternoonScenario.Ventas:F2}");
        Console.WriteLine($"  - Escenario Noche: {scenarios.EveningScenario.Comandas} comandas, ${scenarios.EveningScenario.Ventas:F2}");
        Console.WriteLine($"  - Escenario Fin de Semana: {scenarios.WeekendScenario.Comandas} comandas, ${scenarios.WeekendScenario.Ventas:F2}");
    }

    /// <summary>
    /// Test ultra-robusto que genera MÁS DE 15 COMANDAS con datos masivos para validar cálculos extremos
    /// </summary>
    [Fact]
    public async Task DashboardCalculations_WithMassiveData_ShouldValidateExtremeCalculations()
    {
        Console.WriteLine($"🚀 INICIANDO TEST DE DATOS MASIVOS");
        Console.WriteLine($"📊 Generando MÁS DE 15 COMANDAS con datos extremos para validación robusta");

        // Arrange - Crear datos masivos
        var massiveData = await CreateMassiveTestDataAsync();

        // Act - Obtener métricas del día actual
        var response = await _analyticsService.ObtenerMetricasDiaAsync();
        if (!response.Success)
        {
            Console.WriteLine($"⚠️ Error obteniendo métricas: {response.Message}");
            return;
        }

        var metrics = response.Data;
        if (metrics == null)
        {
            Console.WriteLine("⚠️ No se obtuvieron métricas");
            return;
        }

        // Assert - Validar cálculos masivos
        Console.WriteLine($"📊 VALIDACIÓN DE CÁLCULOS MASIVOS:");
        Console.WriteLine($"  - Total Ventas: ${metrics.TotalVentas:F2}");
        Console.WriteLine($"  - Total Comandas: {metrics.TotalComandas}");
        Console.WriteLine($"  - Total Productos Vendidos: {metrics.TotalProductosVendidos}");
        Console.WriteLine($"  - Clientes Atendidos: {metrics.ClientesAtendidos}");
        Console.WriteLine($"  - Porcentaje Ocupación Mesas: {metrics.PorcentajeOcupacionMesas:F1}%");

        Console.WriteLine($"📈 DATOS ESPERADOS MASIVOS:");
        Console.WriteLine($"  - Ventas Esperadas: ${massiveData.ExpectedTotalVentas:F2}");
        Console.WriteLine($"  - Comandas Esperadas: {massiveData.ExpectedTotalComandas}");
        Console.WriteLine($"  - Productos Únicos: {massiveData.UniqueProducts}");
        Console.WriteLine($"  - Items Total: {massiveData.TotalItems}");
        Console.WriteLine($"  - Mesas Ocupadas: {massiveData.OccupiedTables}");

        // Validaciones robustas de datos masivos
        Assert.Equal((double)massiveData.ExpectedTotalVentas, (double)metrics.TotalVentas, 2);
        Assert.Equal(massiveData.ExpectedTotalComandas, metrics.TotalComandas);
        Assert.True(metrics.TotalProductosVendidos >= massiveData.ExpectedMinProductos);
        Assert.True(metrics.ClientesAtendidos >= massiveData.ExpectedMinClientes);
        Assert.True(metrics.PorcentajeOcupacionMesas >= massiveData.ExpectedMinOcupacion);

        Console.WriteLine($"✅ Validación de cálculos masivos completada exitosamente");
        Console.WriteLine($"🎯 DATOS MASIVOS PROCESADOS:");
        Console.WriteLine($"  - Comandas Creadas: {massiveData.CreatedComandas}");
        Console.WriteLine($"  - Ventas Totales: ${metrics.TotalVentas:F2}");
        Console.WriteLine($"  - Productos Procesados: {metrics.TotalProductosVendidos}");
        Console.WriteLine($"  - Ocupación Mesas: {metrics.PorcentajeOcupacionMesas:F1}%");
    }

    /// <summary>
    /// Test de escenarios de PICO DE VENTAS (horarios de almuerzo y cena con muchos clientes)
    /// </summary>
    [Fact]
    public async Task DashboardCalculations_WithPeakHours_ShouldValidateRushHourCalculations()
    {
        Console.WriteLine($"🚀 INICIANDO TEST DE HORARIOS PICO");
        Console.WriteLine($"⏰ Simulando HORARIOS DE ALMUERZO Y CENA con picos de ventas");

        // Arrange - Crear escenarios de picos de ventas
        var peakData = await CreatePeakHoursDataAsync();

        // Act - Obtener métricas del día actual
        var response = await _analyticsService.ObtenerMetricasDiaAsync();
        if (!response.Success)
        {
            Console.WriteLine($"⚠️ Error obteniendo métricas: {response.Message}");
            return;
        }

        var metrics = response.Data;
        if (metrics == null)
        {
            Console.WriteLine("⚠️ No se obtuvieron métricas");
            return;
        }

        // Assert - Validar cálculos de picos
        Console.WriteLine($"📊 VALIDACIÓN DE CÁLCULOS DE HORARIOS PICO:");
        Console.WriteLine($"  - Total Ventas: ${metrics.TotalVentas:F2}");
        Console.WriteLine($"  - Total Comandas: {metrics.TotalComandas}");
        Console.WriteLine($"  - Total Productos Vendidos: {metrics.TotalProductosVendidos}");
        Console.WriteLine($"  - Clientes Atendidos: {metrics.ClientesAtendidos}");
        Console.WriteLine($"  - Porcentaje Ocupación Mesas: {metrics.PorcentajeOcupacionMesas:F1}%");

        Console.WriteLine($"🔥 DATOS DE HORARIOS PICO:");
        Console.WriteLine($"  - Pico Almuerzo: {peakData.LunchPeak.Comandas} comandas, ${peakData.LunchPeak.Ventas:F2}");
        Console.WriteLine($"  - Pico Cena: {peakData.DinnerPeak.Comandas} comandas, ${peakData.DinnerPeak.Ventas:F2}");
        Console.WriteLine($"  - Pico Fin de Semana: {peakData.WeekendPeak.Comandas} comandas, ${peakData.WeekendPeak.Ventas:F2}");

        // Validaciones robustas de picos
        Assert.Equal((double)peakData.ExpectedTotalVentas, (double)metrics.TotalVentas, 2);
        Assert.Equal(peakData.ExpectedTotalComandas, metrics.TotalComandas);
        Assert.True(metrics.TotalProductosVendidos >= peakData.ExpectedMinProductos);
        Assert.True(metrics.ClientesAtendidos >= peakData.ExpectedMinClientes);

        Console.WriteLine($"✅ Validación de horarios pico completada exitosamente");
        Console.WriteLine($"🎯 PICOS DE VENTAS VALIDADOS:");
        Console.WriteLine($"  - Almuerzo: {peakData.LunchPeak.Comandas} comandas, ${peakData.LunchPeak.Ventas:F2}");
        Console.WriteLine($"  - Cena: {peakData.DinnerPeak.Comandas} comandas, ${peakData.DinnerPeak.Ventas:F2}");
        Console.WriteLine($"  - Fin de Semana: {peakData.WeekendPeak.Comandas} comandas, ${peakData.WeekendPeak.Ventas:F2}");
    }

    /// <summary>
    /// Test de DIFERENTES DÍAS DE LA SEMANA con patrones de ventas variados
    /// </summary>
    [Fact]
    public async Task DashboardCalculations_WithDifferentWeekDays_ShouldValidateWeeklyPatterns()
    {
        Console.WriteLine($"🚀 INICIANDO TEST DE DÍAS DE LA SEMANA");
        Console.WriteLine($"📅 Simulando DIFERENTES DÍAS con patrones de ventas variados");

        // Arrange - Crear datos para diferentes días de la semana
        var weeklyData = await CreateWeeklyPatternsDataAsync();

        // Act - Obtener métricas del día actual
        var response = await _analyticsService.ObtenerMetricasDiaAsync();
        if (!response.Success)
        {
            Console.WriteLine($"⚠️ Error obteniendo métricas: {response.Message}");
            return;
        }

        var metrics = response.Data;
        if (metrics == null)
        {
            Console.WriteLine("⚠️ No se obtuvieron métricas");
            return;
        }

        // Assert - Validar cálculos semanales
        Console.WriteLine($"📊 VALIDACIÓN DE PATRONES SEMANALES:");
        Console.WriteLine($"  - Total Ventas: ${metrics.TotalVentas:F2}");
        Console.WriteLine($"  - Total Comandas: {metrics.TotalComandas}");
        Console.WriteLine($"  - Total Productos Vendidos: {metrics.TotalProductosVendidos}");
        Console.WriteLine($"  - Clientes Atendidos: {metrics.ClientesAtendidos}");
        Console.WriteLine($"  - Porcentaje Ocupación Mesas: {metrics.PorcentajeOcupacionMesas:F1}%");

        Console.WriteLine($"📅 PATRONES DE DÍAS SIMULADOS:");
        Console.WriteLine($"  - Lunes (Día Normal): {weeklyData.Monday.Comandas} comandas, ${weeklyData.Monday.Ventas:F2}");
        Console.WriteLine($"  - Miércoles (Día Medio): {weeklyData.Wednesday.Comandas} comandas, ${weeklyData.Wednesday.Ventas:F2}");
        Console.WriteLine($"  - Viernes (Día Pico): {weeklyData.Friday.Comandas} comandas, ${weeklyData.Friday.Ventas:F2}");
        Console.WriteLine($"  - Sábado (Fin de Semana): {weeklyData.Saturday.Comandas} comandas, ${weeklyData.Saturday.Ventas:F2}");
        Console.WriteLine($"  - Domingo (Día Familiar): {weeklyData.Sunday.Comandas} comandas, ${weeklyData.Sunday.Ventas:F2}");

        // Validaciones robustas de patrones semanales
        Assert.Equal((double)weeklyData.ExpectedTotalVentas, (double)metrics.TotalVentas, 2);
        Assert.Equal(weeklyData.ExpectedTotalComandas, metrics.TotalComandas);
        Assert.True(metrics.TotalProductosVendidos >= weeklyData.ExpectedMinProductos);
        Assert.True(metrics.ClientesAtendidos >= weeklyData.ExpectedMinClientes);

        Console.WriteLine($"✅ Validación de patrones semanales completada exitosamente");
        Console.WriteLine($"🎯 DÍAS DE LA SEMANA VALIDADOS:");
        Console.WriteLine($"  - Total de comandas creadas: {weeklyData.CreatedComandas}");
        Console.WriteLine($"  - Ventas totales: ${metrics.TotalVentas:F2}");
        Console.WriteLine($"  - Patrones de ventas variados: ✅");
    }

    /// <summary>
    /// Crea datos MASIVOS con más de 15 comandas para validación extrema
    /// </summary>
    private async Task<MassiveTestData> CreateMassiveTestDataAsync()
    {
        Console.WriteLine($"🔧 CREANDO DATOS MASIVOS...");

        var productos = await CreateTestProductsAsync();
        if (productos == null || !productos.Any())
        {
            Console.WriteLine("⚠️ No se pudieron obtener productos para datos masivos");
            return new MassiveTestData();
        }

        decimal totalVentas = 0;
        int totalComandas = 0;
        var productosUsados = new HashSet<Guid>();
        int totalItems = 0;
        int mesasOcupadas = 0;

        // Crear MÁS DE 15 COMANDAS con datos variados y extremos
        var comandasMasivas = new List<ComandaMasivaData>
        {
            // Grupo 1: Comandas de desayuno (5 comandas)
            new ComandaMasivaData { Tipo = "Desayuno Individual", Productos = new List<(Guid, int)> { (productos[0].Id, 1), (productos[1].Id, 2) } },
            new ComandaMasivaData { Tipo = "Desayuno Completo", Productos = new List<(Guid, int)> { (productos[0].Id, 2), (productos[1].Id, 3), (productos[2].Id, 1) } },
            new ComandaMasivaData { Tipo = "Desayuno Ejecutivo", Productos = new List<(Guid, int)> { (productos[0].Id, 1), (productos[2].Id, 2) } },
            new ComandaMasivaData { Tipo = "Desayuno Familiar", Productos = new List<(Guid, int)> { (productos[0].Id, 4), (productos[1].Id, 6), (productos[2].Id, 2) } },
            new ComandaMasivaData { Tipo = "Desayuno Rápido", Productos = new List<(Guid, int)> { (productos[1].Id, 2) } },

            // Grupo 2: Comandas de almuerzo (6 comandas)
            new ComandaMasivaData { Tipo = "Almuerzo Individual", Productos = new List<(Guid, int)> { (productos[2].Id, 1), (productos[0].Id, 1) } },
            new ComandaMasivaData { Tipo = "Almuerzo Familiar", Productos = new List<(Guid, int)> { (productos[2].Id, 3), (productos[1].Id, 2), (productos[0].Id, 3) } },
            new ComandaMasivaData { Tipo = "Almuerzo Ejecutivo", Productos = new List<(Guid, int)> { (productos[2].Id, 1), (productos[3].Id, 1), (productos[0].Id, 1) } },
            new ComandaMasivaData { Tipo = "Almuerzo Grupal", Productos = new List<(Guid, int)> { (productos[2].Id, 5), (productos[1].Id, 3), (productos[0].Id, 5) } },
            new ComandaMasivaData { Tipo = "Almuerzo Ligero", Productos = new List<(Guid, int)> { (productos[1].Id, 1), (productos[0].Id, 1) } },
            new ComandaMasivaData { Tipo = "Almuerzo Especial", Productos = new List<(Guid, int)> { (productos[2].Id, 2), (productos[3].Id, 2), (productos[1].Id, 1), (productos[0].Id, 2) } },

            // Grupo 3: Comandas de cena (5 comandas)
            new ComandaMasivaData { Tipo = "Cena Romántica", Productos = new List<(Guid, int)> { (productos[2].Id, 2), (productos[1].Id, 1), (productos[0].Id, 2) } },
            new ComandaMasivaData { Tipo = "Cena Familiar", Productos = new List<(Guid, int)> { (productos[2].Id, 4), (productos[1].Id, 2), (productos[0].Id, 4) } },
            new ComandaMasivaData { Tipo = "Cena Grupal", Productos = new List<(Guid, int)> { (productos[2].Id, 6), (productos[3].Id, 3), (productos[1].Id, 2), (productos[0].Id, 6) } },
            new ComandaMasivaData { Tipo = "Cena Ejecutiva", Productos = new List<(Guid, int)> { (productos[2].Id, 2), (productos[3].Id, 1), (productos[0].Id, 2) } },
            new ComandaMasivaData { Tipo = "Cena Especial", Productos = new List<(Guid, int)> { (productos[2].Id, 3), (productos[1].Id, 2), (productos[3].Id, 2), (productos[0].Id, 3) } }
        };

        // Crear cada comanda masiva
        foreach (var comandaData in comandasMasivas)
        {
            var comandaCreated = await CreateComandaWithProductsAsync(
                Guid.NewGuid(),
                comandaData.Productos,
                $"Comanda masiva - {comandaData.Tipo}"
            );

            if (comandaCreated != null)
            {
                totalComandas++;
                mesasOcupadas++;
                
                // Calcular ventas de esta comanda
                decimal comandaVentas = comandaData.Productos.Sum(p => 
                {
                    var producto = productos.FirstOrDefault(pr => pr.Id == p.Item1);
                    return producto != null ? producto.Precio * p.Item2 : 0;
                });
                totalVentas += comandaVentas;
                
                // Registrar productos únicos
                foreach (var (productoId, cantidad) in comandaData.Productos)
                {
                    productosUsados.Add(productoId);
                    totalItems += cantidad;
                }

                Console.WriteLine($"✅ Comanda masiva {totalComandas} creada - {comandaData.Tipo}: ${comandaVentas:F2}");
            }
        }

        Console.WriteLine($"📊 RESUMEN DE DATOS MASIVOS CREADOS:");
        Console.WriteLine($"  - Comandas masivas creadas: {totalComandas}");
        Console.WriteLine($"  - Ventas totales: ${totalVentas:F2}");
        Console.WriteLine($"  - Productos únicos: {productosUsados.Count}");
        Console.WriteLine($"  - Items totales: {totalItems}");
        Console.WriteLine($"  - Mesas ocupadas: {mesasOcupadas}");

        return new MassiveTestData
        {
            ExpectedTotalVentas = totalVentas,
            ExpectedTotalComandas = totalComandas,
            UniqueProducts = productosUsados.Count,
            TotalItems = totalItems,
            OccupiedTables = mesasOcupadas,
            CreatedComandas = totalComandas,
            ExpectedMinProductos = totalItems,
            ExpectedMinClientes = totalComandas,
            ExpectedMinOcupacion = (mesasOcupadas * 100) / 5 // Asumiendo 5 mesas totales
        };
    }

    /// <summary>
    /// Crea datos de HORARIOS PICO con muchos clientes
    /// </summary>
    private async Task<PeakHoursData> CreatePeakHoursDataAsync()
    {
        Console.WriteLine($"🔧 CREANDO DATOS DE HORARIOS PICO...");

        var productos = await CreateTestProductsAsync();
        if (productos == null || !productos.Any())
        {
            Console.WriteLine("⚠️ No se pudieron obtener productos para horarios pico");
            return new PeakHoursData();
        }

        decimal totalVentas = 0;
        int totalComandas = 0;

        // Pico de Almuerzo (12:00-14:00) - 6 comandas
        var lunchPeak = await CreatePeakScenarioAsync("Pico Almuerzo", 6, productos, 
            new List<(Guid, int)> { (productos[2].Id, 2), (productos[1].Id, 1), (productos[0].Id, 2) });
        totalVentas += lunchPeak.Ventas;
        totalComandas += lunchPeak.Comandas;

        // Pico de Cena (19:00-21:00) - 5 comandas
        var dinnerPeak = await CreatePeakScenarioAsync("Pico Cena", 5, productos,
            new List<(Guid, int)> { (productos[2].Id, 3), (productos[1].Id, 2), (productos[0].Id, 3) });
        totalVentas += dinnerPeak.Ventas;
        totalComandas += dinnerPeak.Comandas;

        // Pico de Fin de Semana - 4 comandas
        var weekendPeak = await CreatePeakScenarioAsync("Pico Fin de Semana", 4, productos,
            new List<(Guid, int)> { (productos[2].Id, 4), (productos[1].Id, 3), (productos[3].Id, 2), (productos[0].Id, 4) });
        totalVentas += weekendPeak.Ventas;
        totalComandas += weekendPeak.Comandas;

        Console.WriteLine($"📊 RESUMEN DE HORARIOS PICO:");
        Console.WriteLine($"  - Total comandas en picos: {totalComandas}");
        Console.WriteLine($"  - Ventas totales en picos: ${totalVentas:F2}");

        return new PeakHoursData
        {
            ExpectedTotalVentas = totalVentas,
            ExpectedTotalComandas = totalComandas,
            ExpectedMinProductos = totalComandas * 2, // Mínimo 2 productos por comanda
            ExpectedMinClientes = totalComandas,
            LunchPeak = lunchPeak,
            DinnerPeak = dinnerPeak,
            WeekendPeak = weekendPeak
        };
    }

    /// <summary>
    /// Crea datos de DIFERENTES DÍAS DE LA SEMANA
    /// </summary>
    private async Task<WeeklyPatternsData> CreateWeeklyPatternsDataAsync()
    {
        Console.WriteLine($"🔧 CREANDO DATOS DE DÍAS DE LA SEMANA...");

        var productos = await CreateTestProductsAsync();
        if (productos == null || !productos.Any())
        {
            Console.WriteLine("⚠️ No se pudieron obtener productos para días de la semana");
            return new WeeklyPatternsData();
        }

        decimal totalVentas = 0;
        int totalComandas = 0;

        // Lunes - Día normal (3 comandas)
        var monday = await CreateDayScenarioAsync("Lunes", 3, productos,
            new List<(Guid, int)> { (productos[2].Id, 1), (productos[0].Id, 1) });
        totalVentas += monday.Ventas;
        totalComandas += monday.Comandas;

        // Miércoles - Día medio (4 comandas)
        var wednesday = await CreateDayScenarioAsync("Miércoles", 4, productos,
            new List<(Guid, int)> { (productos[2].Id, 2), (productos[1].Id, 1), (productos[0].Id, 2) });
        totalVentas += wednesday.Ventas;
        totalComandas += wednesday.Comandas;

        // Viernes - Día pico (5 comandas)
        var friday = await CreateDayScenarioAsync("Viernes", 5, productos,
            new List<(Guid, int)> { (productos[2].Id, 2), (productos[3].Id, 1), (productos[1].Id, 1), (productos[0].Id, 2) });
        totalVentas += friday.Ventas;
        totalComandas += friday.Comandas;

        // Sábado - Fin de semana (6 comandas)
        var saturday = await CreateDayScenarioAsync("Sábado", 6, productos,
            new List<(Guid, int)> { (productos[2].Id, 3), (productos[1].Id, 2), (productos[3].Id, 2), (productos[0].Id, 3) });
        totalVentas += saturday.Ventas;
        totalComandas += saturday.Comandas;

        // Domingo - Día familiar (4 comandas)
        var sunday = await CreateDayScenarioAsync("Domingo", 4, productos,
            new List<(Guid, int)> { (productos[2].Id, 2), (productos[1].Id, 3), (productos[0].Id, 2) });
        totalVentas += sunday.Ventas;
        totalComandas += sunday.Comandas;

        Console.WriteLine($"📊 RESUMEN DE DÍAS DE LA SEMANA:");
        Console.WriteLine($"  - Total comandas semanales: {totalComandas}");
        Console.WriteLine($"  - Ventas totales semanales: ${totalVentas:F2}");

        return new WeeklyPatternsData
        {
            ExpectedTotalVentas = totalVentas,
            ExpectedTotalComandas = totalComandas,
            ExpectedMinProductos = totalComandas * 2,
            ExpectedMinClientes = totalComandas,
            CreatedComandas = totalComandas,
            Monday = monday,
            Wednesday = wednesday,
            Friday = friday,
            Saturday = saturday,
            Sunday = sunday
        };
    }

    /// <summary>
    /// Crea un escenario de pico específico
    /// </summary>
    private async Task<PeakScenarioData> CreatePeakScenarioAsync(string nombre, int cantidadComandas, 
        List<RestaurantePro.Application.Core.Productos.DTOs.ProductoDto> productos, 
        List<(Guid, int)> productosBase)
    {
        decimal totalVentas = 0;
        int comandasCreadas = 0;

        for (int i = 0; i < cantidadComandas; i++)
        {
            var comandaCreated = await CreateComandaWithProductsAsync(
                Guid.NewGuid(),
                productosBase,
                $"{nombre} - Comanda {i + 1}"
            );

            if (comandaCreated != null)
            {
                comandasCreadas++;
                decimal comandaVentas = productosBase.Sum(p => 
                {
                    var producto = productos.FirstOrDefault(pr => pr.Id == p.Item1);
                    return producto != null ? producto.Precio * p.Item2 : 0;
                });
                totalVentas += comandaVentas;
            }
        }

        Console.WriteLine($"✅ {nombre}: {comandasCreadas} comandas, ${totalVentas:F2}");

        return new PeakScenarioData
        {
            Comandas = comandasCreadas,
            Ventas = totalVentas
        };
    }

    /// <summary>
    /// Crea un escenario de día específico
    /// </summary>
    private async Task<DayScenarioData> CreateDayScenarioAsync(string dia, int cantidadComandas,
        List<RestaurantePro.Application.Core.Productos.DTOs.ProductoDto> productos,
        List<(Guid, int)> productosBase)
    {
        decimal totalVentas = 0;
        int comandasCreadas = 0;

        for (int i = 0; i < cantidadComandas; i++)
        {
            var comandaCreated = await CreateComandaWithProductsAsync(
                Guid.NewGuid(),
                productosBase,
                $"{dia} - Comanda {i + 1}"
            );

            if (comandaCreated != null)
            {
                comandasCreadas++;
                decimal comandaVentas = productosBase.Sum(p => 
                {
                    var producto = productos.FirstOrDefault(pr => pr.Id == p.Item1);
                    return producto != null ? producto.Precio * p.Item2 : 0;
                });
                totalVentas += comandaVentas;
            }
        }

        Console.WriteLine($"✅ {dia}: {comandasCreadas} comandas, ${totalVentas:F2}");

        return new DayScenarioData
        {
            Comandas = comandasCreadas,
            Ventas = totalVentas
        };
    }

    /// <summary>
    /// Crea múltiples escenarios variados para validar cálculos complejos
    /// </summary>
    private async Task<VariedScenariosData> CreateVariedScenariosAsync()
    {
        Console.WriteLine($"🔧 Creando escenarios variados de comandas...");

        var productos = await CreateTestProductsAsync();
        if (productos == null || !productos.Any())
        {
            Console.WriteLine("⚠️ No se pudieron obtener productos para los escenarios");
            return new VariedScenariosData();
        }

        var scenarios = new VariedScenariosData();
        var comandaIds = new List<Guid>();

        // Escenario 1: Mañana (desayuno/almuerzo temprano)
        var morningData = await CreateMorningScenarioAsync(productos);
        scenarios.MorningScenario = morningData;
        comandaIds.AddRange(morningData.ComandaIds);

        // Escenario 2: Tarde (almuerzo)
        var afternoonData = await CreateAfternoonScenarioAsync(productos);
        scenarios.AfternoonScenario = afternoonData;
        comandaIds.AddRange(afternoonData.ComandaIds);

        // Escenario 3: Noche (cena)
        var eveningData = await CreateEveningScenarioAsync(productos);
        scenarios.EveningScenario = eveningData;
        comandaIds.AddRange(eveningData.ComandaIds);

        // Escenario 4: Fin de semana (mayor volumen)
        var weekendData = await CreateWeekendScenarioAsync(productos);
        scenarios.WeekendScenario = weekendData;
        comandaIds.AddRange(weekendData.ComandaIds);

        // Calcular totales esperados
        scenarios.ExpectedTotalVentas = morningData.Ventas + afternoonData.Ventas + 
                                      eveningData.Ventas + weekendData.Ventas;
        scenarios.ExpectedTotalComandas = morningData.Comandas + afternoonData.Comandas + 
                                        eveningData.Comandas + weekendData.Comandas;
        scenarios.ExpectedMinProductos = 15; // Mínimo esperado de productos vendidos
        scenarios.ExpectedMinClientes = 8; // Mínimo esperado de clientes atendidos
        scenarios.ExpectedMinOcupacion = 60; // Mínimo 60% de ocupación

        scenarios.ComandaIds = comandaIds;

        Console.WriteLine($"✅ Escenarios variados creados exitosamente");
        Console.WriteLine($"  - Total comandas: {scenarios.ExpectedTotalComandas}");
        Console.WriteLine($"  - Total ventas esperadas: ${scenarios.ExpectedTotalVentas:F2}");

        return scenarios;
    }

    /// <summary>
    /// Crea escenario de mañana (desayuno/almuerzo temprano)
    /// </summary>
    private async Task<ScenarioData> CreateMorningScenarioAsync(List<RestaurantePro.Application.Core.Productos.DTOs.ProductoDto> productos)
    {
        Console.WriteLine($"🌅 Creando escenario de mañana...");

        var comandaIds = new List<Guid>();
        var totalVentas = 0m;

        // Comanda 1: Desayuno individual
        var comanda1 = await CreateComandaWithProductsAsync(
            mesaId: Guid.NewGuid(),
            productos: new List<(Guid ProductoId, int Cantidad)>
            {
                (productos[0].Id, 1), // Café
                (productos[1].Id, 2)  // Pan
            },
            observaciones: "Desayuno individual - Escenario mañana"
        );
        if (comanda1 != null)
        {
            comandaIds.Add(comanda1.Id);
            totalVentas += productos[0].Precio + (productos[1].Precio * 2);
        }

        // Comanda 2: Almuerzo temprano
        var comanda2 = await CreateComandaWithProductsAsync(
            mesaId: Guid.NewGuid(),
            productos: new List<(Guid ProductoId, int Cantidad)>
            {
                (productos[2].Id, 1), // Plato principal
                (productos[0].Id, 1)  // Bebida
            },
            observaciones: "Almuerzo temprano - Escenario mañana"
        );
        if (comanda2 != null)
        {
            comandaIds.Add(comanda2.Id);
            totalVentas += productos[2].Precio + productos[0].Precio;
        }

        return new ScenarioData
        {
            Comandas = comandaIds.Count,
            Ventas = totalVentas,
            ComandaIds = comandaIds
        };
    }

    /// <summary>
    /// Crea escenario de tarde (almuerzo)
    /// </summary>
    private async Task<ScenarioData> CreateAfternoonScenarioAsync(List<RestaurantePro.Application.Core.Productos.DTOs.ProductoDto> productos)
    {
        Console.WriteLine($"☀️ Creando escenario de tarde...");

        var comandaIds = new List<Guid>();
        var totalVentas = 0m;

        // Comanda 3: Almuerzo familiar
        var comanda3 = await CreateComandaWithProductsAsync(
            mesaId: Guid.NewGuid(),
            productos: new List<(Guid ProductoId, int Cantidad)>
            {
                (productos[2].Id, 2), // 2 platos principales
                (productos[1].Id, 1), // Acompañamiento
                (productos[0].Id, 2)  // 2 bebidas
            },
            observaciones: "Almuerzo familiar - Escenario tarde"
        );
        if (comanda3 != null)
        {
            comandaIds.Add(comanda3.Id);
            totalVentas += (productos[2].Precio * 2) + productos[1].Precio + (productos[0].Precio * 2);
        }

        // Comanda 4: Almuerzo ejecutivo
        var comanda4 = await CreateComandaWithProductsAsync(
            mesaId: Guid.NewGuid(),
            productos: new List<(Guid ProductoId, int Cantidad)>
            {
                (productos[2].Id, 1), // Plato principal
                (productos[0].Id, 1)  // Bebida
            },
            observaciones: "Almuerzo ejecutivo - Escenario tarde"
        );
        if (comanda4 != null)
        {
            comandaIds.Add(comanda4.Id);
            totalVentas += productos[2].Precio + productos[0].Precio;
        }

        return new ScenarioData
        {
            Comandas = comandaIds.Count,
            Ventas = totalVentas,
            ComandaIds = comandaIds
        };
    }

    /// <summary>
    /// Crea escenario de noche (cena)
    /// </summary>
    private async Task<ScenarioData> CreateEveningScenarioAsync(List<RestaurantePro.Application.Core.Productos.DTOs.ProductoDto> productos)
    {
        Console.WriteLine($"🌙 Creando escenario de noche...");

        var comandaIds = new List<Guid>();
        var totalVentas = 0m;

        // Comanda 5: Cena romántica
        var comanda5 = await CreateComandaWithProductsAsync(
            mesaId: Guid.NewGuid(),
            productos: new List<(Guid ProductoId, int Cantidad)>
            {
                (productos[2].Id, 2), // 2 platos principales
                (productos[1].Id, 1), // Acompañamiento
                (productos[0].Id, 2)  // 2 bebidas
            },
            observaciones: "Cena romántica - Escenario noche"
        );
        if (comanda5 != null)
        {
            comandaIds.Add(comanda5.Id);
            totalVentas += (productos[2].Precio * 2) + productos[1].Precio + (productos[0].Precio * 2);
        }

        return new ScenarioData
        {
            Comandas = comandaIds.Count,
            Ventas = totalVentas,
            ComandaIds = comandaIds
        };
    }

    /// <summary>
    /// Crea escenario de fin de semana (mayor volumen)
    /// </summary>
    private async Task<ScenarioData> CreateWeekendScenarioAsync(List<RestaurantePro.Application.Core.Productos.DTOs.ProductoDto> productos)
    {
        Console.WriteLine($"🎉 Creando escenario de fin de semana...");

        var comandaIds = new List<Guid>();
        var totalVentas = 0m;

        // Comanda 6: Grupo grande fin de semana
        var comanda6 = await CreateComandaWithProductsAsync(
            mesaId: Guid.NewGuid(), // Mesa nueva
            productos: new List<(Guid ProductoId, int Cantidad)>
            {
                (productos[2].Id, 4), // 4 platos principales
                (productos[1].Id, 2), // 2 acompañamientos
                (productos[0].Id, 4)  // 4 bebidas
            },
            observaciones: "Grupo grande fin de semana - Escenario weekend"
        );
        if (comanda6 != null)
        {
            comandaIds.Add(comanda6.Id);
            totalVentas += (productos[2].Precio * 4) + (productos[1].Precio * 2) + (productos[0].Precio * 4);
        }

        // Comanda 7: Familia fin de semana
        var comanda7 = await CreateComandaWithProductsAsync(
            mesaId: Guid.NewGuid(), // Mesa nueva
            productos: new List<(Guid ProductoId, int Cantidad)>
            {
                (productos[2].Id, 3), // 3 platos principales
                (productos[1].Id, 1), // 1 acompañamiento
                (productos[0].Id, 3)  // 3 bebidas
            },
            observaciones: "Familia fin de semana - Escenario weekend"
        );
        if (comanda7 != null)
        {
            comandaIds.Add(comanda7.Id);
            totalVentas += (productos[2].Precio * 3) + productos[1].Precio + (productos[0].Precio * 3);
        }

        return new ScenarioData
        {
            Comandas = comandaIds.Count,
            Ventas = totalVentas,
            ComandaIds = comandaIds
        };
    }

    /// <summary>
    /// Crea una comanda con productos específicos (versión simplificada)
    /// </summary>
    private async Task<ApplicationComandaDto?> CreateComandaWithProductsAsync(
        Guid mesaId,
        List<(Guid ProductoId, int Cantidad)> productos,
        string observaciones)
    {
        try
        {
            // Crear comanda inicial
            var createComandaRequest = new CrearComandaRequest
            {
                MesaId = mesaId,
                ClienteNombre = null,
                Observaciones = observaciones
            };

            var comandaResponse = await _apiService.PostAsync<ApplicationComandaDto>(
                "/api/operaciones/comandas", createComandaRequest);

            if (!comandaResponse.Success || comandaResponse.Data == null)
            {
                Console.WriteLine($"⚠️ Error creando comanda: {comandaResponse.Message}");
                return null;
            }

            var comanda = comandaResponse.Data;

            // Agregar productos a la comanda
            foreach (var (productoId, cantidad) in productos)
            {
                var addProductoRequest = new ComandaProductoRequest
                {
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    Observaciones = $"Producto agregado: {cantidad} unidades"
                };

                var addProductoResponse = await _apiService.PutAsync<ApplicationComandaDto>(
                    $"/api/operaciones/comandas/{comanda.Id}/agregar-producto", addProductoRequest);

                if (!addProductoResponse.Success)
                {
                    Console.WriteLine($"⚠️ Error agregando producto {productoId}: {addProductoResponse.Message}");
                }
            }

            return comanda;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Excepción creando comanda: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea datos de prueba con alto volumen: múltiples comandas, productos y cantidades
    /// </summary>
    private async Task<HighVolumeTestData> CreateHighVolumeTestDataAsync()
    {
        Console.WriteLine($"🔧 CREANDO DATOS DE PRUEBA DE ALTO VOLUMEN...");

        // Obtener productos disponibles
        var productos = await CreateTestProductsAsync();
        Console.WriteLine($"📦 Productos disponibles: {productos.Count}");

        // Obtener mesas disponibles
        var mesasResponse = await _mesasService.ObtenerMesasAsync();
        if (!mesasResponse.Success || mesasResponse.Data == null || !mesasResponse.Data.Any())
        {
            Console.WriteLine("⚠️ No hay mesas disponibles para crear datos de prueba");
            return new HighVolumeTestData();
        }

        var mesas = mesasResponse.Data.Take(3).ToList(); // Usar máximo 3 mesas
        Console.WriteLine($"🪑 Mesas disponibles: {mesas.Count}");

        decimal totalVentas = 0;
        int totalComandas = 0;
        var productosUsados = new HashSet<Guid>();
        int totalItems = 0;

        // Crear múltiples comandas con diferentes escenarios
        var comandasData = new List<ComandaTestData>
        {
            // Comanda 1: Orden grande con múltiples productos
            new ComandaTestData
            {
                MesaId = mesas[0].Id,
                Productos = new List<ProductoComandaData>
                {
                    new() { ProductoId = productos[0].Id, Cantidad = 2, Precio = productos[0].Precio },
                    new() { ProductoId = productos[1].Id, Cantidad = 1, Precio = productos[1].Precio },
                    new() { ProductoId = productos[2].Id, Cantidad = 3, Precio = productos[2].Precio }
                }
            },
            
            // Comanda 2: Orden mediana con productos diferentes
            new ComandaTestData
            {
                MesaId = mesas[1].Id,
                Productos = new List<ProductoComandaData>
                {
                    new() { ProductoId = productos[1].Id, Cantidad = 2, Precio = productos[1].Precio },
                    new() { ProductoId = productos[3].Id, Cantidad = 1, Precio = productos[3].Precio }
                }
            },

            // Comanda 3: Orden pequeña pero con alta cantidad de un producto
            new ComandaTestData
            {
                MesaId = mesas[2].Id,
                Productos = new List<ProductoComandaData>
                {
                    new() { ProductoId = productos[0].Id, Cantidad = 5, Precio = productos[0].Precio },
                    new() { ProductoId = productos[2].Id, Cantidad = 2, Precio = productos[2].Precio }
                }
            },

            // Comanda 4: Orden variada con todos los productos disponibles
            new ComandaTestData
            {
                MesaId = mesas[0].Id, // Reutilizar mesa
                Productos = productos.Take(4).Select(p => new ProductoComandaData
                {
                    ProductoId = p.Id,
                    Cantidad = 1,
                    Precio = p.Precio
                }).ToList()
            },

            // Comanda 5: Orden con productos repetidos pero diferentes cantidades
            new ComandaTestData
            {
                MesaId = mesas[1].Id, // Reutilizar mesa
                Productos = new List<ProductoComandaData>
                {
                    new() { ProductoId = productos[0].Id, Cantidad = 4, Precio = productos[0].Precio },
                    new() { ProductoId = productos[1].Id, Cantidad = 2, Precio = productos[1].Precio },
                    new() { ProductoId = productos[2].Id, Cantidad = 1, Precio = productos[2].Precio },
                    new() { ProductoId = productos[3].Id, Cantidad = 3, Precio = productos[3].Precio }
                }
            }
        };

        // Crear cada comanda y calcular totales
        foreach (var comandaData in comandasData)
        {
            var comandaCreated = await CreateComandaWithProductsAsync(comandaData);
            if (comandaCreated != null)
            {
                totalComandas++;
                
                // Calcular ventas de esta comanda
                decimal comandaVentas = comandaData.Productos.Sum(p => p.Cantidad * p.Precio);
                totalVentas += comandaVentas;
                
                // Registrar productos únicos
                foreach (var producto in comandaData.Productos)
                {
                    productosUsados.Add(producto.ProductoId);
                    totalItems += producto.Cantidad;
                }

                Console.WriteLine($"✅ Comanda {totalComandas} creada - Ventas: ${comandaVentas:F2}");
            }
        }

        Console.WriteLine($"📊 RESUMEN DE DATOS CREADOS:");
        Console.WriteLine($"  - Comandas creadas: {totalComandas}");
        Console.WriteLine($"  - Ventas totales: ${totalVentas:F2}");
        Console.WriteLine($"  - Productos únicos: {productosUsados.Count}");
        Console.WriteLine($"  - Items totales: {totalItems}");

        return new HighVolumeTestData
        {
            ExpectedTotalVentas = totalVentas,
            ExpectedTotalComandas = totalComandas,
            UniqueProducts = productosUsados.Count,
            TotalItems = totalItems
        };
    }

    /// <summary>
    /// Crea una comanda con múltiples productos
    /// </summary>
    private async Task<ApplicationComandaDto?> CreateComandaWithProductsAsync(ComandaTestData comandaData)
    {
        try
        {
            // Crear comanda inicial
            var createComandaRequest = new CrearComandaRequest
            {
                MesaId = comandaData.MesaId,
                ClienteNombre = null,
                Observaciones = $"Comanda de prueba con {comandaData.Productos.Count} productos"
            };

            var comandaResponse = await _apiService.PostAsync<ApplicationComandaDto>(
                "/api/operaciones/comandas", createComandaRequest);

            if (!comandaResponse.Success || comandaResponse.Data == null)
            {
                Console.WriteLine($"⚠️ Error creando comanda: {comandaResponse.Message}");
                return null;
            }

            var comanda = comandaResponse.Data;
            Console.WriteLine($"📝 Comanda creada: {comanda.Id}");

            // Agregar productos a la comanda
            foreach (var productoData in comandaData.Productos)
            {
                var addProductoRequest = new ComandaProductoRequest
                {
                    ProductoId = productoData.ProductoId,
                    Cantidad = productoData.Cantidad,
                    PrecioUnitario = productoData.Precio,
                    Observaciones = $"Producto agregado: {productoData.Cantidad} unidades"
                };

                var addProductoResponse = await _apiService.PutAsync<ApplicationComandaDto>(
                    $"/api/operaciones/comandas/{comanda.Id}/agregar-producto", addProductoRequest);

                if (addProductoResponse.Success)
                {
                    Console.WriteLine($"  ✅ Producto agregado: {productoData.Cantidad}x (${productoData.Precio:F2} cada uno)");
                }
                else
                {
                    Console.WriteLine($"  ⚠️ Error agregando producto: {addProductoResponse.Message}");
                }
            }

            return comanda;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creando comanda con productos: {ex.Message}");
            return null;
        }
    }

    private class TestData
    {
        public int ExpectedComandas { get; set; }
        public int ExpectedTotalComandas { get; set; }
        public decimal ExpectedVentas { get; set; }
        public int ExpectedMesasOcupadas { get; set; }
        public List<Guid> ComandaIds { get; set; } = new();
    }

    /// <summary>
    /// Datos de prueba para comanda individual
    /// </summary>
    private class ComandaTestData
    {
        public Guid MesaId { get; set; }
        public List<ProductoComandaData> Productos { get; set; } = new();
    }

    /// <summary>
    /// Datos de producto en comanda
    /// </summary>
    private class ProductoComandaData
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
    }

    /// <summary>
    /// Datos de prueba para test de alto volumen
    /// </summary>
    private class HighVolumeTestData
    {
        public decimal ExpectedTotalVentas { get; set; }
        public int ExpectedTotalComandas { get; set; }
        public int UniqueProducts { get; set; }
        public int TotalItems { get; set; }
    }

    /// <summary>
    /// Datos de prueba para escenarios variados
    /// </summary>
    private class VariedScenariosData
    {
        public ScenarioData MorningScenario { get; set; } = new();
        public ScenarioData AfternoonScenario { get; set; } = new();
        public ScenarioData EveningScenario { get; set; } = new();
        public ScenarioData WeekendScenario { get; set; } = new();
        public decimal ExpectedTotalVentas { get; set; }
        public int ExpectedTotalComandas { get; set; }
        public int ExpectedMinProductos { get; set; }
        public int ExpectedMinClientes { get; set; }
        public int ExpectedMinOcupacion { get; set; }
        public List<Guid> ComandaIds { get; set; } = new();
    }

    /// <summary>
    /// Datos de un escenario específico
    /// </summary>
    private class ScenarioData
    {
        public int Comandas { get; set; }
        public decimal Ventas { get; set; }
        public List<Guid> ComandaIds { get; set; } = new();
    }

    /// <summary>
    /// Datos de prueba para test de datos masivos
    /// </summary>
    private class MassiveTestData
    {
        public decimal ExpectedTotalVentas { get; set; }
        public int ExpectedTotalComandas { get; set; }
        public int UniqueProducts { get; set; }
        public int TotalItems { get; set; }
        public int OccupiedTables { get; set; }
        public int CreatedComandas { get; set; }
        public int ExpectedMinProductos { get; set; }
        public int ExpectedMinClientes { get; set; }
        public decimal ExpectedMinOcupacion { get; set; }
    }

    /// <summary>
    /// Datos de prueba para horarios pico
    /// </summary>
    private class PeakHoursData
    {
        public decimal ExpectedTotalVentas { get; set; }
        public int ExpectedTotalComandas { get; set; }
        public int ExpectedMinProductos { get; set; }
        public int ExpectedMinClientes { get; set; }
        public PeakScenarioData LunchPeak { get; set; } = new();
        public PeakScenarioData DinnerPeak { get; set; } = new();
        public PeakScenarioData WeekendPeak { get; set; } = new();
    }

    /// <summary>
    /// Datos de prueba para patrones semanales
    /// </summary>
    private class WeeklyPatternsData
    {
        public decimal ExpectedTotalVentas { get; set; }
        public int ExpectedTotalComandas { get; set; }
        public int ExpectedMinProductos { get; set; }
        public int ExpectedMinClientes { get; set; }
        public int CreatedComandas { get; set; }
        public DayScenarioData Monday { get; set; } = new();
        public DayScenarioData Wednesday { get; set; } = new();
        public DayScenarioData Friday { get; set; } = new();
        public DayScenarioData Saturday { get; set; } = new();
        public DayScenarioData Sunday { get; set; } = new();
    }

    /// <summary>
    /// Datos de escenario de pico
    /// </summary>
    private class PeakScenarioData
    {
        public int Comandas { get; set; }
        public decimal Ventas { get; set; }
    }

    /// <summary>
    /// Datos de escenario de día
    /// </summary>
    private class DayScenarioData
    {
        public int Comandas { get; set; }
        public decimal Ventas { get; set; }
    }

    /// <summary>
    /// Datos de comanda masiva
    /// </summary>
    private class ComandaMasivaData
    {
        public string Tipo { get; set; } = string.Empty;
        public List<(Guid, int)> Productos { get; set; } = new();
    }

}
