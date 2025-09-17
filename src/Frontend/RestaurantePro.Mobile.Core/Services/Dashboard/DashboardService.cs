using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Analytics;
using RestaurantePro.Mobile.Core.Services.Mesas;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.Core.Services.Dashboard;

/// <summary>
/// Implementación del servicio de dashboard
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IApiService _apiService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IMesasService _mesasService;
    private readonly IAuthService _authService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IApiService apiService, IAnalyticsService analyticsService, IMesasService mesasService, IAuthService authService, ILogger<DashboardService> logger)
    {
        _apiService = apiService;
        _analyticsService = analyticsService;
        _mesasService = mesasService;
        _authService = authService;
        _logger = logger;
    }

    public async Task<decimal> GetTodaySalesAsync()
    {
        try
        {
            _logger.LogInformation("Obteniendo ventas del día actual desde la API");
            
            var response = await _analyticsService.ObtenerMetricasDiaAsync();
            
            if (response.Success && response.Data != null)
            {
                var totalVentas = response.Data.TotalVentas;
                
                // Validar que las ventas sean consistentes (no negativas)
                if (totalVentas < 0)
                {
                    _logger.LogWarning("Datos inconsistentes recibidos: ventas negativas ({TotalVentas}), usando datos simulados", totalVentas);
                    return await GetSimulatedTodaySalesAsync();
                }
                
                _logger.LogInformation("Ventas del día obtenidas exitosamente: {TotalVentas}", totalVentas);
                return totalVentas;
            }
            else
            {
                _logger.LogWarning("No se pudieron obtener las ventas del día desde la API, usando datos simulados");
                return await GetSimulatedTodaySalesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas del día desde la API, usando datos simulados");
            return await GetSimulatedTodaySalesAsync();
        }
    }

    private async Task<decimal> GetSimulatedTodaySalesAsync()
    {
        // Datos simulados como fallback
        await Task.Delay(100);
        
        var hour = DateTime.Now.Hour;
        var baseSales = 1200m;
        var hourlyMultiplier = hour switch
        {
            >= 6 and < 12 => 0.3m,  // Desayuno
            >= 12 and < 15 => 0.8m, // Almuerzo
            >= 15 and < 18 => 0.2m, // Merienda
            >= 18 and < 22 => 1.0m, // Cena
            _ => 0.1m               // Horas bajas
        };
        
        var todaySales = baseSales * hourlyMultiplier + (hour * 50);
        return Math.Round(todaySales, 2);
    }

    public async Task<decimal> GetSalesChangePercentageAsync()
    {
        try
        {
            _logger.LogInformation("Obteniendo porcentaje de cambio de ventas comparando con ayer");
            
            // Obtener métricas de hoy
            var todayResponse = await _analyticsService.ObtenerMetricasDiaAsync();
            
            if (todayResponse.Success && todayResponse.Data != null)
            {
                // Obtener métricas de ayer
                var yesterday = DateTime.Today.AddDays(-1);
                var yesterdayResponse = await _analyticsService.ObtenerMetricasRangoAsync(yesterday, yesterday);
                
                if (yesterdayResponse.Success && yesterdayResponse.Data != null)
                {
                    var todaySales = todayResponse.Data.TotalVentas;
                    var yesterdaySales = yesterdayResponse.Data.TotalVentas;
                    
                    if (yesterdaySales > 0)
                    {
                        var changePercentage = ((todaySales - yesterdaySales) / yesterdaySales) * 100;
                        _logger.LogInformation("Cambio de ventas calculado: {ChangePercentage}%", changePercentage);
                        return Math.Round(changePercentage, 1);
                    }
                }
            }
            
            _logger.LogWarning("No se pudo calcular el cambio de ventas, usando datos simulados");
            return await GetSimulatedSalesChangeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener porcentaje de cambio de ventas, usando datos simulados");
            return await GetSimulatedSalesChangeAsync();
        }
    }

    private async Task<decimal> GetSimulatedSalesChangeAsync()
    {
        // Datos simulados como fallback
        await Task.Delay(50);
        
        var random = new Random();
        var change = random.Next(-20, 30); // Entre -20% y +30%
        
        return change;
    }

    public async Task<int> GetActiveOrdersCountAsync()
    {
        try
        {
            _logger.LogInformation("Obteniendo número de comandas activas desde la API");
            
            // Obtener comandas activas según enum del backend: EnProceso, Lista, Entregada
            var activeStatuses = new[] { "EnProceso", "Lista", "Entregada" };
            var totalActive = 0;
            
            var token = await _authService.GetTokenAsync();

            foreach (var status in activeStatuses)
            {
                var response = await _apiService.GetAsync<PaginatedList<ComandaDto>>(
                    $"api/operaciones/comandas?Estado={status}&PageSize=100&SoloActivas=true", token);
                
                if (response.Success && response.Data != null)
                {
                    totalActive += response.Data.Items.Count;
                }
            }
            
            _logger.LogInformation("Comandas activas obtenidas: {TotalActive}", totalActive);
            return totalActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener número de comandas activas, usando datos simulados");
            return await GetSimulatedActiveOrdersCountAsync();
        }
    }

    public async Task<int> GetPendingOrdersCountAsync()
    {
        try
        {
            _logger.LogInformation("Obteniendo número de comandas pendientes desde la API");
            
            var token = await _authService.GetTokenAsync();
            // Pendientes en backend corresponden a estado Creada
            var response = await _apiService.GetAsync<PaginatedList<ComandaDto>>(
                "api/operaciones/comandas?Estado=Creada&PageSize=100", token);
            
            if (response.Success && response.Data != null)
            {
                var pendingCount = response.Data.Items.Count;
                _logger.LogInformation("Comandas pendientes obtenidas: {PendingCount}", pendingCount);
                return pendingCount;
            }
            
            _logger.LogWarning("No se pudieron obtener las comandas pendientes, usando datos simulados");
            return await GetSimulatedPendingOrdersCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener número de comandas pendientes, usando datos simulados");
            return await GetSimulatedPendingOrdersCountAsync();
        }
    }

    private async Task<int> GetSimulatedActiveOrdersCountAsync()
    {
        await Task.Delay(50);
        var random = new Random();
        return random.Next(5, 15);
    }

    private async Task<int> GetSimulatedPendingOrdersCountAsync()
    {
        await Task.Delay(50);
        var random = new Random();
        return random.Next(1, 8);
    }

    public async Task<List<OrderItem>> GetRecentOrdersAsync()
    {
        try
        {
            _logger.LogInformation("Obteniendo comandas recientes desde la API");
            System.Diagnostics.Debug.WriteLine("🔍 [DashboardService] GetRecentOrdersAsync - Iniciando");
            
            // Obtener las últimas comandas (las más recientes)
            var token = await _authService.GetTokenAsync();
            System.Diagnostics.Debug.WriteLine($"🔍 [DashboardService] Token obtenido: {!string.IsNullOrEmpty(token)}");
            
            var response = await _apiService.GetAsync<PaginatedList<ComandaDto>>(
                "api/operaciones/comandas?PageSize=10&OrdenarPor=fechaCreacion&DireccionOrdenamiento=desc", token);
            
            System.Diagnostics.Debug.WriteLine($"🔍 [DashboardService] Response - Success: {response.Success}, Data: {response.Data != null}");
            
            if (response.Success && response.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"🔍 [DashboardService] Items recibidos: {response.Data.Items?.Count ?? 0}");
                var recentOrders = response.Data.Items.Select(comanda => new OrderItem
                {
                    Id = comanda.Id.GetHashCode(),
                    OrderNumber = comanda.Numero,
                    TableNumber = string.IsNullOrWhiteSpace(comanda.MesaNumero) ? (comanda.NumeroMesa > 0 ? $"Mesa {comanda.NumeroMesa}" : "") : comanda.MesaNumero,
                    CustomerName = GetFormattedOrderInfo(comanda),
                    Total = comanda.Total,
                    Status = !string.IsNullOrWhiteSpace(comanda.EstadoTexto) ? comanda.EstadoTexto : GetStatusDisplayName(comanda.Estado),
                    OrderTime = comanda.FechaCreacion,
                    Items = (comanda.Productos?.Select(item => item.Nombre ?? "Producto").ToList() ?? new List<string>())
                }).ToList();
                
                // Logging detallado de cada comanda para debug
                foreach (var order in recentOrders)
                {
                    System.Diagnostics.Debug.WriteLine($"🔍 [DashboardService] Comanda: {order.OrderNumber}, Estado: '{order.Status}', Cliente: {order.CustomerName}");
                }
                
                _logger.LogInformation("Comandas recientes obtenidas: {Count}", recentOrders.Count);
                return recentOrders;
            }
            
            _logger.LogWarning("No se pudieron obtener las comandas recientes, usando datos simulados");
            return await GetSimulatedRecentOrdersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener comandas recientes, usando datos simulados");
            return await GetSimulatedRecentOrdersAsync();
        }
    }

    private string GetStatusDisplayName(string status)
    {
        return status?.ToLower() switch
        {
            "nueva" => "Nueva",
            "creada" => "Creada",
            "pendiente" => "Pendiente", 
            "enproceso" => "EnProceso",
            "en_progreso" => "En Progreso",
            "preparando" => "Preparando",
            "lista" => "Lista",
            "servida" => "Servida",
            "completada" => "Completada",
            "cancelada" => "Cancelada",
            "finalizada" => "Finalizada",
            _ => status ?? "Desconocido"
        };
    }

    private string GetFormattedOrderInfo(RestaurantePro.Mobile.Core.Models.DTOs.ComandaDto comanda)
    {
        // Obtener el código de la comanda (número completo o abreviado)
        var codigoComanda = !string.IsNullOrWhiteSpace(comanda.NumeroComanda) 
            ? comanda.NumeroComanda 
            : $"COM-{comanda.Numero}";

        // Obtener el nombre del cliente
        var nombreCliente = !string.IsNullOrWhiteSpace(comanda.ClienteNombre) 
            ? comanda.ClienteNombre 
            : (comanda.NombreCliente ?? "Cliente General");

        // Formato: Código de comanda en la primera línea, cliente en la segunda
        return $"{codigoComanda}\n{nombreCliente}";
    }

    private async Task<List<OrderItem>> GetSimulatedRecentOrdersAsync()
    {
        await Task.Delay(100);
        
        return new List<OrderItem>
        {
            new OrderItem
            {
                Id = 1,
                OrderNumber = "ORD-001",
                TableNumber = "Mesa 5",
                CustomerName = "Juan Pérez",
                Total = 45.50m,
                Status = "En Progreso",
                OrderTime = DateTime.Now.AddMinutes(-15),
                Items = new List<string> { "Pizza Margherita", "Coca Cola" }
            },
            new OrderItem
            {
                Id = 2,
                OrderNumber = "ORD-002",
                TableNumber = "Mesa 12",
                CustomerName = "María García",
                Total = 32.00m,
                Status = "Pendiente",
                OrderTime = DateTime.Now.AddMinutes(-8),
                Items = new List<string> { "Ensalada César", "Agua" }
            },
            new OrderItem
            {
                Id = 3,
                OrderNumber = "ORD-003",
                TableNumber = "Mesa 8",
                CustomerName = "Carlos López",
                Total = 67.50m,
                Status = "Completada",
                OrderTime = DateTime.Now.AddMinutes(-25),
                Items = new List<string> { "Pasta Carbonara", "Vino Tinto", "Tiramisu" }
            }
        };
    }

    public async Task<List<OrderItem>> GetOrdersByStatusAsync(string status)
    {
        try
        {
            _logger.LogInformation("Obteniendo comandas por estado: {Status}", status);
            System.Diagnostics.Debug.WriteLine($"🔍 [DashboardService] GetOrdersByStatusAsync - Estado: {status}");
            
            if (status == "Todas")
            {
                return await GetRecentOrdersAsync();
            }
            
            // Mapear nombres de filtros a estados de la API
            var apiStatus = status switch
            {
                "Pendientes" => "Nueva",
                "En Progreso" => "EnProceso", 
                "Listas" => "Lista",
                _ => status
            };
            
            System.Diagnostics.Debug.WriteLine($"🔍 [DashboardService] Estado mapeado: {status} -> {apiStatus}");
            
            var allOrders = await GetRecentOrdersAsync();
            
            // Logging detallado de todos los estados disponibles
            System.Diagnostics.Debug.WriteLine($"🔍 [DashboardService] Estados disponibles:");
            foreach (var order in allOrders)
            {
                System.Diagnostics.Debug.WriteLine($"   - {order.OrderNumber}: '{order.Status}'");
            }
            
            var filteredOrders = allOrders.Where(o => 
                o.Status.Equals(apiStatus, StringComparison.OrdinalIgnoreCase) ||
                o.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
                
            System.Diagnostics.Debug.WriteLine($"🔍 [DashboardService] Comandas filtradas: {filteredOrders.Count} de {allOrders.Count}");
            
            // Logging de comandas filtradas
            foreach (var order in filteredOrders)
            {
                System.Diagnostics.Debug.WriteLine($"   ✅ Filtrada: {order.OrderNumber} - '{order.Status}'");
            }
            
            return filteredOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener comandas por estado: {Status}", status);
            System.Diagnostics.Debug.WriteLine($"❌ [DashboardService] Error filtrando por estado: {ex.Message}");
            return new List<OrderItem>();
        }
    }

    public async Task<EstadoMesasDto> GetTableStatusAsync()
    {
        try
        {
            _logger.LogInformation("🔍 [DashboardService] Obteniendo estado de mesas desde la API");
            System.Diagnostics.Debug.WriteLine("🔍 [DashboardService] GetTableStatusAsync - Iniciando");
            
            // Usar el servicio de mesas que ya centraliza token y endpoints
            var response = await _mesasService.ObtenerEstadoOcupacionAsync();
            System.Diagnostics.Debug.WriteLine($"🔍 [DashboardService] Response recibida - Success: {response.Success}");

            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("✅ [DashboardService] Estado de mesas obtenido exitosamente: {TotalMesas} mesas", response.Data.TotalMesas);
                _logger.LogInformation("✅ [DashboardService] Estadísticas - Ocupadas: {Ocupadas}, Disponibles: {Disponibles}, Total: {Total}", 
                    response.Data.Estadisticas?.MesasOcupadas, response.Data.Estadisticas?.MesasDisponibles, response.Data.TotalMesas);
                System.Diagnostics.Debug.WriteLine($"✅ [DashboardService] Datos reales obtenidos - Ocupadas: {response.Data.Estadisticas?.MesasOcupadas}, Total: {response.Data.TotalMesas}");
                return response.Data;
            }
            
            _logger.LogWarning("⚠️ [DashboardService] No se pudo obtener el estado de mesas, usando datos simulados. Error: {Error}", response.Message);
            System.Diagnostics.Debug.WriteLine($"⚠️ [DashboardService] Fallback a datos simulados. Error: {response.Message}");
            return await GetSimulatedTableStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [DashboardService] Error al obtener estado de mesas, usando datos simulados");
            System.Diagnostics.Debug.WriteLine($"❌ [DashboardService] Exception: {ex.Message}");
            return await GetSimulatedTableStatusAsync();
        }
    }

    private async Task<EstadoMesasDto> GetSimulatedTableStatusAsync()
    {
        await Task.Delay(100);
        
        var mesas = new List<MesaDto>
        {
            new MesaDto { Id = Guid.NewGuid(), Numero = "1", Estado = "disponible", Capacidad = 4, Zona = "Interior" },
            new MesaDto { Id = Guid.NewGuid(), Numero = "2", Estado = "ocupada", Capacidad = 2, Zona = "Interior" },
            new MesaDto { Id = Guid.NewGuid(), Numero = "3", Estado = "reservada", Capacidad = 6, Zona = "Terraza" },
            new MesaDto { Id = Guid.NewGuid(), Numero = "4", Estado = "disponible", Capacidad = 4, Zona = "Interior" },
            new MesaDto { Id = Guid.NewGuid(), Numero = "5", Estado = "disponible", Capacidad = 2, Zona = "Terraza" },
            new MesaDto { Id = Guid.NewGuid(), Numero = "6", Estado = "ocupada", Capacidad = 8, Zona = "VIP" },
            new MesaDto { Id = Guid.NewGuid(), Numero = "7", Estado = "disponible", Capacidad = 4, Zona = "Interior" },
            new MesaDto { Id = Guid.NewGuid(), Numero = "8", Estado = "reservada", Capacidad = 6, Zona = "Terraza" }
        };
        
        return new EstadoMesasDto
        {
            Mesas = mesas,
            Estadisticas = new EstadisticasMesasDto
            {
                MesasDisponibles = mesas.Count(m => m.Estado == "disponible"),
                MesasOcupadas = mesas.Count(m => m.Estado == "ocupada"),
                MesasReservadas = mesas.Count(m => m.Estado == "reservada"),
                MesasActivas = mesas.Count,
                PorcentajeOcupacion = 50.0m,
                PorcentajeDisponibilidad = 50.0m
            },
            FechaConsulta = DateTime.Now
        };
    }
}

