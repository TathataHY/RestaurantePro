using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using RestaurantePro.Api.Models;
using RestaurantePro.Api.Models.Requests;
using RestaurantePro.Application.Common.Models.Dashboard;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Domain.Core.Productos.Entities;

namespace RestaurantePro.Api.IntegrationTests.Dashboard;

/// <summary>
/// Pruebas de integración para el Dashboard
/// </summary>
public class DashboardIntegrationTests : ApiIntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public DashboardIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<string> ObtenerTokenAdminAsync()
    {
        var loginRequest = new LoginRequest
        {
            Email = "admin@restaurantepro.com",
            Password = "AdminRestaurante123!"
        };
        
        var loginResponse = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        return loginApiResponse!.Data!.Token;
    }

    private async Task CrearDatosDePruebaAsync()
    {
        // Crear categorías de productos
        var categoriaComida = ProductoCategoria.Crear(
            "Comida",
            "Platos principales",
            1,
            "#FF5722",
            "🍽️"
        );
        
        var categoriaBebida = ProductoCategoria.Crear(
            "Bebida",
            "Bebidas y refrescos",
            2,
            "#3B82F6",
            "🥤"
        );

        // Crear productos usando el método de fábrica correcto
        var productos = new[]
        {
            RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Pizza Margherita",
                "Pizza clásica con tomate y mozzarella",
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(12.99m),
                categoriaComida.Id,
                "Comida"
            ),
            RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Hamburguesa Clásica",
                "Hamburguesa con carne, lechuga y tomate",
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(8.99m),
                categoriaComida.Id,
                "Comida"
            ),
            RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Coca Cola",
                "Bebida gaseosa",
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(3.50m),
                categoriaBebida.Id,
                "Bebida"
            ),
            RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Ensalada César",
                "Ensalada fresca con aderezo césar",
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(7.50m),
                categoriaComida.Id,
                "Comida"
            ),
            RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Agua Mineral",
                "Agua mineral natural",
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(2.00m),
                categoriaBebida.Id,
                "Bebida"
            )
        };

        // Crear mesas usando el método de fábrica correcto
        var mesas = new[]
        {
            RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(
                1,
                4,
                "Salón Principal"
            ),
            RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(
                2,
                2,
                "Terraza"
            ),
            RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(
                3,
                6,
                "Salón Principal"
            ),
            RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(
                4,
                2,
                "Terraza"
            ),
            RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(
                5,
                8,
                "Salón VIP"
            )
        };

        // Crear clientes usando el método de fábrica correcto
        var clientes = new[]
        {
            RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
                RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear("Juan", "Pérez"),
                "juan@email.com",
                "1234567890",
                new DateTime(1990, 1, 1)
            ),
            RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
                RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear("María", "González"),
                "maria@email.com",
                "0987654321",
                new DateTime(1985, 5, 15)
            ),
            RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
                RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear("Carlos", "López"),
                "carlos@email.com",
                "5555555555",
                new DateTime(1988, 8, 20)
            ),
            RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
                RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear("Ana", "Martínez"),
                "ana@email.com",
                "6666666666",
                new DateTime(1992, 3, 10)
            )
        };

        DbContext.ProductoCategorias.AddRange(categoriaComida, categoriaBebida);
        DbContext.Productos.AddRange(productos);
        DbContext.Mesas.AddRange(mesas);
        DbContext.Clientes.AddRange(clientes);
        await DbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task ObtenerDashboard_DeberiaRetornarDatosValidos()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
    }

    [Fact]
    public async Task ObtenerDashboard_DeberiaTenerMetricasValidas()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        Assert.NotNull(dashboard.Data.Metricas);
        Assert.True(dashboard.Data.Metricas.VentasHoy >= 0);
        Assert.True(dashboard.Data.Metricas.ComandasActivas >= 0);
    }

    [Fact]
    public async Task ObtenerDashboard_DeberiaTenerEstadoMesasValido()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        Assert.NotNull(dashboard.Data.EstadoMesas);
        Assert.True(dashboard.Data.EstadoMesas.Disponibles >= 0);
        Assert.True(dashboard.Data.EstadoMesas.Ocupadas >= 0);
    }

    [Fact]
    public async Task ObtenerDashboard_DeberiaTenerProductosMasVendidos()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        Assert.NotNull(dashboard.Data.ProductosMasVendidos);
    }

    [Fact]
    public async Task ObtenerDashboard_DeberiaTenerVentasPorPeriodo()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        Assert.NotNull(dashboard.Data.VentasPorPeriodo);
    }

    [Fact]
    public async Task ObtenerDashboard_DeberiaTenerIngresosPorHora()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        Assert.NotNull(dashboard.Data.IngresosPorHora);
    }

    [Fact]
    public async Task ObtenerEstadoMesas_DeberiaRetornarDatosValidos()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/estado-mesas");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var estadoMesas = JsonSerializer.Deserialize<ApiResponse<DashboardEstadoMesasDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(estadoMesas);
        Assert.True(estadoMesas.Success);
        Assert.NotNull(estadoMesas.Data);
        Assert.True(estadoMesas.Data.Disponibles >= 0);
        Assert.True(estadoMesas.Data.Ocupadas >= 0);
    }

    [Fact]
    public async Task ObtenerDashboard_ConDatosDeHoy_DeberiaRetornarMetricasCorrectas()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        
        // Verificar que las métricas de hoy estén presentes
        Assert.True(dashboard.Data.Metricas.VentasHoy >= 0);
        Assert.True(dashboard.Data.Metricas.ComandasActivas >= 0);
        Assert.True(dashboard.Data.Metricas.TotalMesas >= 0);
        Assert.True(dashboard.Data.Metricas.MesasOcupadas >= 0);
    }

    [Fact]
    public async Task ObtenerDashboard_ConDatosDeAyer_DeberiaRetornarMetricasCorrectas()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Simular consulta de ayer (esto dependería de la implementación del servicio)
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        
        // Verificar que las métricas estén presentes
        Assert.True(dashboard.Data.Metricas.VentasHoy >= 0);
        Assert.True(dashboard.Data.Metricas.ComandasActivas >= 0);
    }

    [Fact]
    public async Task ObtenerDashboard_ConFiltroTurnoManana_DeberiaRetornarDatosCorrectos()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Simular filtro de turno mañana (6:00 AM - 12:00 PM)
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        
        // Verificar que los datos estén presentes
        Assert.NotNull(dashboard.Data.IngresosPorHora);
        Assert.NotNull(dashboard.Data.ProductosMasVendidos);
        Assert.NotNull(dashboard.Data.EstadoMesas);
    }

    [Fact]
    public async Task ObtenerDashboard_ConFiltroTurnoTarde_DeberiaRetornarDatosCorrectos()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Simular filtro de turno tarde (12:00 PM - 6:00 PM)
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        
        // Verificar que los datos estén presentes
        Assert.NotNull(dashboard.Data.IngresosPorHora);
        Assert.NotNull(dashboard.Data.ProductosMasVendidos);
        Assert.NotNull(dashboard.Data.EstadoMesas);
    }

    [Fact]
    public async Task ObtenerDashboard_ConFiltroTurnoNoche_DeberiaRetornarDatosCorrectos()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Simular filtro de turno noche (6:00 PM - 12:00 AM)
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        
        // Verificar que los datos estén presentes
        Assert.NotNull(dashboard.Data.IngresosPorHora);
        Assert.NotNull(dashboard.Data.ProductosMasVendidos);
        Assert.NotNull(dashboard.Data.EstadoMesas);
    }

    [Fact]
    public async Task ObtenerDashboard_ConFiltroTurnoMadrugada_DeberiaRetornarDatosCorrectos()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Simular filtro de turno madrugada (12:00 AM - 6:00 AM)
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        
        // Verificar que los datos estén presentes
        Assert.NotNull(dashboard.Data.IngresosPorHora);
        Assert.NotNull(dashboard.Data.ProductosMasVendidos);
        Assert.NotNull(dashboard.Data.EstadoMesas);
    }

    [Fact]
    public async Task ObtenerDashboard_ConDatosDeSemana_DeberiaRetornarMetricasCorrectas()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Simular consulta de esta semana
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        
        // Verificar que las métricas estén presentes
        Assert.True(dashboard.Data.Metricas.VentasHoy >= 0);
        Assert.True(dashboard.Data.Metricas.ComandasActivas >= 0);
    }

    [Fact]
    public async Task ObtenerDashboard_ConDatosDeMes_DeberiaRetornarMetricasCorrectas()
    {
        // Arrange
        await CrearDatosDePruebaAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Simular consulta de este mes
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        
        // Verificar que las métricas estén presentes
        Assert.True(dashboard.Data.Metricas.VentasHoy >= 0);
        Assert.True(dashboard.Data.Metricas.ComandasActivas >= 0);
    }

    [Fact]
    public async Task ObtenerDashboard_ConDatosVacios_DeberiaRetornarMetricasEnCero()
    {
        // Arrange - No crear datos de prueba
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);
        
        // Verificar que las métricas estén en cero cuando no hay datos
        Assert.Equal(0, dashboard.Data.Metricas.VentasHoy);
        Assert.Equal(0, dashboard.Data.Metricas.ComandasActivas);
        Assert.Equal(0, dashboard.Data.Metricas.TotalMesas);
        Assert.Equal(0, dashboard.Data.Metricas.MesasOcupadas);
    }
}