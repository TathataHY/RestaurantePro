using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

/// <summary>
/// Servicio para obtener métricas y datos del dashboard administrativo
/// </summary>
public class DashboardApiService : IDashboardApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public DashboardApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
    {
        _httpFactory = httpFactory;
        _tokenStore = tokenStore;
    }

    private HttpClient CreateClient()
    {
        var http = _httpFactory.CreateClient("Api");
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token);
            if (!http.DefaultRequestHeaders.Contains("X-Bearer-Token"))
            {
                http.DefaultRequestHeaders.Add("X-Bearer-Token", _tokenStore.Token);
            }
        }
        return http;
    }

    /// <summary>
    /// Obtiene el resumen completo del dashboard
    /// </summary>
    public async Task<DashboardResumenDto?> ObtenerResumenAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<DashboardResumenDto>>("api/admin/dashboard/resumen");
            return resp?.Data;
        }
        catch (Exception)
        {
            // En caso de error, retornar datos de ejemplo para desarrollo
            return CrearDatosEjemplo();
        }
    }

    /// <summary>
    /// Obtiene métricas básicas del dashboard
    /// </summary>
    public async Task<DashboardMetricasDto?> ObtenerMetricasAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<DashboardMetricasDto>>("api/admin/dashboard/metricas");
            return resp?.Data;
        }
        catch (Exception)
        {
            // Datos de ejemplo para desarrollo
            return new DashboardMetricasDto
            {
                VentasHoy = 1250.50m,
                VentasAyer = 1180.75m,
                VentasSemana = 8750.25m,
                VentasMes = 32500.00m,
                MesasOcupadas = 8,
                MesasDisponibles = 12,
                TotalMesas = 20,
                ComandasActivas = 15,
                ComandasCompletadas = 45,
                ProductosVendidosHoy = 89,
                ClientesAtendidosHoy = 32,
                PromedioTicket = 39.08m,
                CrecimientoVentas = 5.9m,
                UltimaActualizacion = DateTime.Now
            };
        }
    }

    /// <summary>
    /// Obtiene productos más vendidos
    /// </summary>
    public async Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync(int cantidad = 5)
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<List<ProductoMasVendidoDto>>>($"api/admin/dashboard/productos-mas-vendidos?cantidad={cantidad}");
            return resp?.Data ?? new List<ProductoMasVendidoDto>();
        }
        catch (Exception)
        {
            // Datos de ejemplo para desarrollo
            return new List<ProductoMasVendidoDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Pizza Margherita", CategoriaNombre = "Pizzas", CantidadVendida = 25, Ingresos = 625.00m, PorcentajeTotal = 15.2m },
                new() { Id = Guid.NewGuid(), Nombre = "Hamburguesa Clásica", CategoriaNombre = "Hamburguesas", CantidadVendida = 18, Ingresos = 450.00m, PorcentajeTotal = 10.9m },
                new() { Id = Guid.NewGuid(), Nombre = "Ensalada César", CategoriaNombre = "Ensaladas", CantidadVendida = 15, Ingresos = 225.00m, PorcentajeTotal = 9.1m },
                new() { Id = Guid.NewGuid(), Nombre = "Pasta Carbonara", CategoriaNombre = "Pastas", CantidadVendida = 12, Ingresos = 360.00m, PorcentajeTotal = 7.3m },
                new() { Id = Guid.NewGuid(), Nombre = "Café Americano", CategoriaNombre = "Bebidas", CantidadVendida = 30, Ingresos = 150.00m, PorcentajeTotal = 6.8m }
            };
        }
    }

    /// <summary>
    /// Obtiene ventas de los últimos 7 días
    /// </summary>
    public async Task<List<VentaPorPeriodoDto>> ObtenerVentasUltimos7DiasAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<List<VentaPorPeriodoDto>>>("api/admin/dashboard/ventas-ultimos-7-dias");
            return resp?.Data ?? new List<VentaPorPeriodoDto>();
        }
        catch (Exception)
        {
            // Datos de ejemplo para desarrollo
            var ventas = new List<VentaPorPeriodoDto>();
            var random = new Random();
            for (int i = 6; i >= 0; i--)
            {
                ventas.Add(new VentaPorPeriodoDto
                {
                    Fecha = DateTime.Today.AddDays(-i),
                    Monto = 800 + random.Next(200, 800),
                    CantidadComandas = 15 + random.Next(5, 25),
                    CantidadProductos = 45 + random.Next(15, 60)
                });
            }
            return ventas;
        }
    }

    /// <summary>
    /// Obtiene estado actual de las mesas
    /// </summary>
    public async Task<EstadoMesasDto?> ObtenerEstadoMesasAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<EstadoMesasDto>>("api/admin/dashboard/estado-mesas");
            return resp?.Data;
        }
        catch (Exception)
        {
            // Datos de ejemplo para desarrollo
            return new EstadoMesasDto
            {
                Disponibles = 12,
                Ocupadas = 6,
                Reservadas = 2,
                EnLimpieza = 0,
                Total = 20
            };
        }
    }

    /// <summary>
    /// Obtiene comandas por estado
    /// </summary>
    public async Task<ComandasPorEstadoDto?> ObtenerComandasPorEstadoAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<ComandasPorEstadoDto>>("api/admin/dashboard/comandas-por-estado");
            return resp?.Data;
        }
        catch (Exception)
        {
            // Datos de ejemplo para desarrollo
            return new ComandasPorEstadoDto
            {
                Pendientes = 5,
                EnPreparacion = 8,
                Listas = 2,
                Completadas = 45,
                Canceladas = 1,
                Total = 61
            };
        }
    }

    /// <summary>
    /// Obtiene ingresos por hora del día actual
    /// </summary>
    public async Task<List<IngresosPorHoraDto>> ObtenerIngresosPorHoraAsync()
    {
        try
        {
            var http = CreateClient();
            var resp = await http.GetFromJsonAsync<ApiResponse<List<IngresosPorHoraDto>>>("api/admin/dashboard/ingresos-por-hora");
            return resp?.Data ?? new List<IngresosPorHoraDto>();
        }
        catch (Exception)
        {
            // Datos de ejemplo para desarrollo
            var ingresos = new List<IngresosPorHoraDto>();
            var random = new Random();
            for (int hora = 12; hora <= 22; hora++)
            {
                ingresos.Add(new IngresosPorHoraDto
                {
                    Hora = hora,
                    Monto = random.Next(50, 300),
                    CantidadComandas = random.Next(2, 12)
                });
            }
            return ingresos;
        }
    }

    /// <summary>
    /// Obtiene el dashboard completo
    /// </summary>
    public async Task<DashboardResumenDto?> ObtenerDashboardAsync()
    {
        return await ObtenerResumenAsync();
    }

    /// <summary>
    /// Obtiene productos más vendidos (sin parámetros)
    /// </summary>
    public async Task<List<ProductoMasVendidoDto>> ObtenerProductosMasVendidosAsync()
    {
        return await ObtenerProductosMasVendidosAsync(5);
    }

    /// <summary>
    /// Crea datos de ejemplo para desarrollo
    /// </summary>
    private DashboardResumenDto CrearDatosEjemplo()
    {
        return new DashboardResumenDto
        {
            Metricas = new DashboardMetricasDto
            {
                VentasHoy = 1250.50m,
                VentasAyer = 1180.75m,
                VentasSemana = 8750.25m,
                VentasMes = 32500.00m,
                MesasOcupadas = 8,
                MesasDisponibles = 12,
                TotalMesas = 20,
                ComandasActivas = 15,
                ComandasCompletadas = 45,
                ProductosVendidosHoy = 89,
                ClientesAtendidosHoy = 32,
                PromedioTicket = 39.08m,
                CrecimientoVentas = 5.9m,
                UltimaActualizacion = DateTime.Now
            },
            ProductosMasVendidos = new List<ProductoMasVendidoDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Pizza Margherita", CategoriaNombre = "Pizzas", CantidadVendida = 25, Ingresos = 625.00m, PorcentajeTotal = 15.2m },
                new() { Id = Guid.NewGuid(), Nombre = "Hamburguesa Clásica", CategoriaNombre = "Hamburguesas", CantidadVendida = 18, Ingresos = 450.00m, PorcentajeTotal = 10.9m },
                new() { Id = Guid.NewGuid(), Nombre = "Ensalada César", CategoriaNombre = "Ensaladas", CantidadVendida = 15, Ingresos = 225.00m, PorcentajeTotal = 9.1m }
            },
            EstadoMesas = new EstadoMesasDto
            {
                Disponibles = 12,
                Ocupadas = 6,
                Reservadas = 2,
                EnLimpieza = 0,
                Total = 20
            },
            ComandasPorEstado = new ComandasPorEstadoDto
            {
                Pendientes = 5,
                EnPreparacion = 8,
                Listas = 2,
                Completadas = 45,
                Canceladas = 1,
                Total = 61
            }
        };
    }
}
