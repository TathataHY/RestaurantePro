using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System.Net.Http;
using System.Text.Json;
using Xunit;
using RestaurantePro.Api.Models;
using RestaurantePro.Api.Models.Requests;
using RestaurantePro.Application.Common.Models.Dashboard;

namespace RestaurantePro.Api.IntegrationTests.Dashboard;

/// <summary>
/// Tests de integración para el dashboard web administrativo
/// Prueba todos los filtros, métricas y visualizaciones con datos simulados
/// </summary>
public class DashboardWebIntegrationTests : ApiIntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public DashboardWebIntegrationTests(TestWebApplicationFactory factory) : base(factory)
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

    #region Tests de Filtros de Período

    [Fact]
    public async Task DashboardWeb_ConFiltroHoy_DeberiaCalcularMetricasCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=todos");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar métricas principales
        // Hoy: Factura1 (mañana) + Factura2 (tarde) + Factura3 (noche) = 49.27 + 60.28 + 23.20 = 132.75
        Assert.Equal(132.75m, dashboard.Data.Metricas.VentasHoy);
        
        // Validar estado de mesas
        Assert.Equal(2, dashboard.Data.EstadoMesas.Ocupadas); // Mesa 1 y Mesa 5
        Assert.Equal(2, dashboard.Data.EstadoMesas.Disponibles); // Mesa 2 y Mesa 4
        Assert.Equal(1, dashboard.Data.EstadoMesas.Reservadas); // Mesa 3
        Assert.Equal(5, dashboard.Data.EstadoMesas.Total);

        // Validar comandas activas
        Assert.Equal(2, dashboard.Data.Metricas.ComandasActivas); // Comanda 1 y Comanda 2

        // Validar productos más vendidos
        Assert.True(dashboard.Data.ProductosMasVendidos.Count > 0);
        var productoMasVendido = dashboard.Data.ProductosMasVendidos.First();
        Assert.Equal("Pizza Margherita", productoMasVendido.Nombre);
    }

    [Fact]
    public async Task DashboardWeb_ConFiltroAyer_DeberiaCalcularMetricasCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=ayer&turno=todos");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar métricas de ayer
        // Ayer: Factura4 (mañana) + Factura5 (tarde) = 34.80 + 46.40 = 81.20
        Assert.Equal(81.20m, dashboard.Data.Metricas.VentasAyer);
    }

    [Fact]
    public async Task DashboardWeb_ConFiltroSemana_DeberiaCalcularMetricasCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=semana&turno=todos");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar métricas de la semana
        // Semana: Todas las facturas = 132.75 + 81.20 + 17.40 = 231.35
        Assert.Equal(231.35m, dashboard.Data.Metricas.VentasSemana);
    }

    [Fact]
    public async Task DashboardWeb_ConFiltroMes_DeberiaCalcularMetricasCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=mes&turno=todos");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar métricas del mes
        // Mes: Todas las facturas = 132.75 + 81.20 + 17.40 + 11.60 = 242.95
        Assert.Equal(242.95m, dashboard.Data.Metricas.VentasMes);
    }

    #endregion

    #region Tests de Filtros de Turno

    [Fact]
    public async Task DashboardWeb_ConFiltroTurnoManana_DeberiaCalcularMetricasCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=mañana");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar métricas de turno mañana
        // Solo Factura1 (10:00 AM) = 49.27
        Assert.Equal(49.27m, dashboard.Data.Metricas.VentasHoy);
    }

    [Fact]
    public async Task DashboardWeb_ConFiltroTurnoTarde_DeberiaCalcularMetricasCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=tarde");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar métricas de turno tarde
        // Solo Factura2 (2:00 PM) = 60.28
        Assert.Equal(60.28m, dashboard.Data.Metricas.VentasHoy);
    }

    [Fact]
    public async Task DashboardWeb_ConFiltroTurnoNoche_DeberiaCalcularMetricasCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=noche");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar métricas de turno noche
        // Solo Factura3 (8:00 PM) = 23.20
        Assert.Equal(23.20m, dashboard.Data.Metricas.VentasHoy);
    }

    [Fact]
    public async Task DashboardWeb_ConFiltroTurnoMadrugada_DeberiaCalcularMetricasCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=madrugada");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar métricas de turno madrugada (no hay facturas en este turno)
        Assert.Equal(0m, dashboard.Data.Metricas.VentasHoy);
    }

    #endregion

    #region Tests de Métricas Específicas

    [Fact]
    public async Task DashboardWeb_DeberiaCalcularOcupacionMesasCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=todos");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar ocupación de mesas
        var ocupacion = (dashboard.Data.EstadoMesas.Ocupadas * 100) / dashboard.Data.EstadoMesas.Total;
        Assert.Equal(40, ocupacion); // 2 ocupadas de 5 total = 40%
    }

    [Fact]
    public async Task DashboardWeb_DeberiaCalcularProductosMasVendidosCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=semana&turno=todos");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar productos más vendidos
        Assert.True(dashboard.Data.ProductosMasVendidos.Count > 0);
        
        // Pizza Margherita debería ser el más vendido (3 unidades)
        var pizza = dashboard.Data.ProductosMasVendidos.FirstOrDefault(p => p.Nombre == "Pizza Margherita");
        Assert.NotNull(pizza);
        Assert.Equal(3, pizza.CantidadVendida);
    }

    [Fact]
    public async Task DashboardWeb_DeberiaCalcularIngresosPorHoraCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=todos");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar ingresos por hora
        Assert.True(dashboard.Data.IngresosPorHora.Count > 0);
        
        // Debería haber ingresos en las horas 10, 14, y 20
        var ingresos10 = dashboard.Data.IngresosPorHora.FirstOrDefault(i => i.Hora == 10);
        var ingresos14 = dashboard.Data.IngresosPorHora.FirstOrDefault(i => i.Hora == 14);
        var ingresos20 = dashboard.Data.IngresosPorHora.FirstOrDefault(i => i.Hora == 20);
        
        Assert.NotNull(ingresos10);
        Assert.NotNull(ingresos14);
        Assert.NotNull(ingresos20);
    }

    #endregion

    #region Tests de Escenarios Edge

    [Fact]
    public async Task DashboardWeb_ConDatosVacios_DeberiaRetornarCeros()
    {
        // Arrange - No crear datos
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=todos");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar que todas las métricas sean 0
        Assert.Equal(0m, dashboard.Data.Metricas.VentasHoy);
        Assert.Equal(0, dashboard.Data.EstadoMesas.Ocupadas);
        Assert.Equal(0, dashboard.Data.Metricas.ComandasActivas);
        Assert.Empty(dashboard.Data.ProductosMasVendidos);
        
        // VentasPorPeriodo puede tener datos para los últimos 7 días con ventas en 0
        Assert.True(dashboard.Data.VentasPorPeriodo.All(v => v.Ventas == 0));
        // IngresosPorHora puede tener datos para las 24 horas con ingresos en 0
        Assert.True(dashboard.Data.IngresosPorHora.All(i => i.Ingresos == 0));
    }

    [Fact]
    public async Task DashboardWeb_ConFiltrosCombinados_DeberiaAplicarFiltrosCorrectamente()
    {
        // Arrange
        await CrearDatosSimuladosCompletosAsync();
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=semana&turno=mañana");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<ApiResponse<DashboardResumenDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar que se aplicaron ambos filtros
        // Solo facturas de la semana en turno mañana: Factura1 (hoy 10:00 AM) + Factura4 (ayer 11:00 AM) = 49.27 + 34.80 = 84.07
        Assert.Equal(84.07m, dashboard.Data.Metricas.VentasSemana);
    }

    #endregion

    #region Métodos de Ayuda

    private async Task CrearDatosSimuladosCompletosAsync()
    {
        var hoy = DateTime.Now.Date; // Usar medianoche de hoy para evitar problemas de tiempo
        var ayer = hoy.AddDays(-1);
        var semana = hoy.AddDays(-7);
        var mes = hoy.AddDays(-30);

        // Crear clientes
        var cliente1 = Cliente.Crear(
            ClienteNombre.Crear("Juan", "Pérez"),
            "juan@email.com",
            "1234567890",
            new DateTime(1990, 1, 1)
        );
        var cliente2 = Cliente.Crear(
            ClienteNombre.Crear("María", "González"),
            "maria@email.com",
            "0987654321",
            new DateTime(1985, 5, 15)
        );

        DbContext.Clientes.AddRange(cliente1, cliente2);
        await DbContext.SaveChangesAsync();

        var cliente1Id = cliente1.Id;
        var cliente2Id = cliente2.Id;

        // Crear categorías de productos
        var categoriaPizzas = ProductoCategoria.Crear("Pizzas", "Pizzas artesanales", 1, "#FF6B6B", "🍕");
        var categoriaHamburguesas = ProductoCategoria.Crear("Hamburguesas", "Hamburguesas gourmet", 2, "#4ECDC4", "🍔");
        var categoriaEnsaladas = ProductoCategoria.Crear("Ensaladas", "Ensaladas frescas", 3, "#45B7D1", "🥗");
        var categoriaPastas = ProductoCategoria.Crear("Pastas", "Pastas italianas", 4, "#96CEB4", "🍝");
        var categoriaBebidas = ProductoCategoria.Crear("Bebidas", "Bebidas refrescantes", 5, "#FECA57", "🥤");
        var categoriaPostres = ProductoCategoria.Crear("Postres", "Postres deliciosos", 6, "#FF9FF3", "🍰");

        DbContext.ProductoCategorias.AddRange(categoriaPizzas, categoriaHamburguesas, categoriaEnsaladas, categoriaPastas, categoriaBebidas, categoriaPostres);
        await DbContext.SaveChangesAsync();

        // Crear productos
        var producto1 = Producto.Crear(
            "Pizza Margherita",
            "Pizza con tomate, mozzarella y albahaca",
            new PrecioProducto(12.99m),
            categoriaPizzas.Id,
            "Pizzas"
        );
        var producto2 = Producto.Crear(
            "Hamburguesa Clásica",
            "Hamburguesa con carne, lechuga, tomate y cebolla",
            new PrecioProducto(15.00m),
            categoriaHamburguesas.Id,
            "Hamburguesas"
        );
        var producto3 = Producto.Crear(
            "Ensalada César",
            "Ensalada con lechuga, pollo, crutones y aderezo césar",
            new PrecioProducto(10.00m),
            categoriaEnsaladas.Id,
            "Ensaladas"
        );
        var producto4 = Producto.Crear(
            "Pasta Carbonara",
            "Pasta con crema, huevo, panceta y parmesano",
            new PrecioProducto(20.00m),
            categoriaPastas.Id,
            "Pastas"
        );
        var producto5 = Producto.Crear(
            "Coca Cola",
            "Bebida gaseosa de cola",
            new PrecioProducto(3.50m),
            categoriaBebidas.Id,
            "Bebidas"
        );
        var producto6 = Producto.Crear(
            "Tiramisú",
            "Postre italiano con café y mascarpone",
            new PrecioProducto(8.00m),
            categoriaPostres.Id,
            "Postres"
        );

        DbContext.Productos.AddRange(producto1, producto2, producto3, producto4, producto5, producto6);
        await DbContext.SaveChangesAsync();

        var producto1Id = producto1.Id;
        var producto2Id = producto2.Id;
        var producto3Id = producto3.Id;
        var producto4Id = producto4.Id;
        var producto5Id = producto5.Id;
        var producto6Id = producto6.Id;

        // Crear mesas
        var mesa1 = Mesa.Crear(1, 4, "Interior");
        var mesa2 = Mesa.Crear(2, 2, "Terraza");
        var mesa3 = Mesa.Crear(3, 6, "Interior");
        var mesa4 = Mesa.Crear(4, 4, "Interior");
        var mesa5 = Mesa.Crear(5, 2, "Terraza");

        DbContext.Mesas.AddRange(mesa1, mesa2, mesa3, mesa4, mesa5);
        await DbContext.SaveChangesAsync();

        var mesa1Id = mesa1.Id;
        var mesa2Id = mesa2.Id;
        var mesa3Id = mesa3.Id;
        var mesa4Id = mesa4.Id;
        var mesa5Id = mesa5.Id;

        // Configurar estados de mesas
        mesa1.MarcarComoOcupada();
        mesa2.MarcarComoDisponible();
        mesa3.MarcarComoReservada();
        mesa4.MarcarComoDisponible(); // Cambiar a disponible en lugar de en limpieza
        mesa5.MarcarComoOcupada();

        // Crear facturas con diferentes fechas y horas
        // Factura 1: Hoy Mañana (10:00 AM) - Pizza + Coca Cola = 49.27
        var factura1 = Factura.Crear(
            "FAC-001",
            TipoFactura.Electronica,
            cliente1.Nombre.ToString(),
            cliente1Id,
            null,
            null,
            null,
            null,
            hoy.AddHours(10) // 10:00 AM
        );
        factura1.AgregarDetalle(producto1Id, "Pizza Margherita", 1, 12.99m, 16.0m, 0m);
        factura1.AgregarDetalle(producto5Id, "Coca Cola", 1, 3.50m, 16.0m, 0m);
        factura1.Emitir();
        factura1.RegistrarPago(factura1.Total, Guid.NewGuid());

        // Factura 2: Hoy Tarde (2:00 PM) - 2 Pizzas = 60.28
        var factura2 = Factura.Crear(
            "FAC-002",
            TipoFactura.Electronica,
            cliente2.Nombre.ToString(),
            cliente2Id,
            null,
            null,
            null,
            null,
            hoy.AddHours(14) // 2:00 PM
        );
        factura2.AgregarDetalle(producto1Id, "Pizza Margherita", 2, 12.99m, 16.0m, 0m);
        factura2.Emitir();
        factura2.RegistrarPago(factura2.Total, Guid.NewGuid());

        // Factura 3: Hoy Noche (8:00 PM) - Ensalada = 23.20
        var factura3 = Factura.Crear(
            "FAC-003",
            TipoFactura.Electronica,
            cliente1.Nombre.ToString(),
            cliente1Id,
            null,
            null,
            null,
            null,
            hoy.AddHours(20) // 8:00 PM
        );
        factura3.AgregarDetalle(producto3Id, "Ensalada César", 2, 10.00m, 16.0m, 0m);
        factura3.Emitir();
        factura3.RegistrarPago(factura3.Total, Guid.NewGuid());

        // Factura 4: Ayer Mañana (11:00 AM) - Hamburguesa = 34.80
        var factura4 = Factura.Crear(
            "FAC-004",
            TipoFactura.Electronica,
            cliente2.Nombre.ToString(),
            cliente2Id,
            null,
            null,
            null,
            null,
            ayer.AddHours(11) // 11:00 AM
        );
        factura4.AgregarDetalle(producto2Id, "Hamburguesa Clásica", 2, 15.00m, 16.0m, 0m);
        factura4.Emitir();
        factura4.RegistrarPago(factura4.Total, Guid.NewGuid());

        // Factura 5: Ayer Tarde (3:00 PM) - Pasta = 46.40
        var factura5 = Factura.Crear(
            "FAC-005",
            TipoFactura.Electronica,
            cliente1.Nombre.ToString(),
            cliente1Id,
            null,
            null,
            null,
            null,
            ayer.AddHours(15) // 3:00 PM
        );
        factura5.AgregarDetalle(producto4Id, "Pasta Carbonara", 2, 20.00m, 16.0m, 0m);
        factura5.Emitir();
        factura5.RegistrarPago(factura5.Total, Guid.NewGuid());

        // Factura 6: Semana Pasada (Miércoles 7:00 PM) - Postre = 17.40
        var factura6 = Factura.Crear(
            "FAC-006",
            TipoFactura.Electronica,
            cliente2.Nombre.ToString(),
            cliente2Id,
            null,
            null,
            null,
            null,
            semana.AddHours(19) // 7:00 PM
        );
        factura6.AgregarDetalle(producto6Id, "Tiramisú", 2, 8.00m, 16.0m, 0m);
        factura6.Emitir();
        factura6.RegistrarPago(factura6.Total, Guid.NewGuid());

        // Factura 7: Mes Pasado (15 días atrás) - Bebida = 11.60
        var factura7 = Factura.Crear(
            "FAC-007",
            TipoFactura.Electronica,
            cliente1.Nombre.ToString(),
            cliente1Id,
            null,
            null,
            null,
            null,
            mes.AddDays(15).AddHours(12) // 12:00 PM
        );
        factura7.AgregarDetalle(producto5Id, "Coca Cola", 3, 3.50m, 16.0m, 0m);
        factura7.Emitir();
        factura7.RegistrarPago(factura7.Total, Guid.NewGuid());

        DbContext.Facturas.AddRange(factura1, factura2, factura3, factura4, factura5, factura6, factura7);

        // Crear comandas
        // Comanda 1: En Proceso (Mesa 1) - 8:00 AM de hoy
        var comanda1 = Comanda.Crear(
            null,
            hoy.AddHours(8),
            null,
            mesa1Id,
            "Cliente en mesa 1",
            "COM-001"
        );
        comanda1.AgregarProducto(producto1Id, 1, 12.99m);
        comanda1.ActualizarEstado(EstadoComanda.EnProceso);

        // Comanda 2: Lista (Mesa 5) - 10:00 AM de hoy
        var comanda2 = Comanda.Crear(
            null,
            hoy.AddHours(10),
            null,
            mesa5Id,
            "Cliente en mesa 5",
            "COM-002"
        );
        comanda2.AgregarProducto(producto2Id, 1, 15.00m);
        comanda2.ActualizarEstado(EstadoComanda.EnProceso);
        comanda2.ActualizarEstado(EstadoComanda.Lista);

        // Comanda 3: Entregada (Hoy) - 11:00 AM de hoy
        var comanda3 = Comanda.Crear(
            null,
            hoy.AddHours(11),
            null,
            mesa2Id,
            "Cliente en mesa 2",
            "COM-003"
        );
        comanda3.AgregarProducto(producto3Id, 1, 10.00m);
        comanda3.ActualizarEstado(EstadoComanda.EnProceso);
        comanda3.ActualizarEstado(EstadoComanda.Lista);
        comanda3.ActualizarEstado(EstadoComanda.Entregada);

        // Comanda 4: Cancelada (Ayer)
        var comanda4 = Comanda.Crear(
            null,
            ayer.AddHours(12),
            null,
            mesa3Id,
            "Cliente en mesa 3",
            "COM-004"
        );
        comanda4.AgregarProducto(producto4Id, 1, 20.00m);
        comanda4.Cancelar("Cliente no se presentó", ayer.AddHours(13));

        // Comanda 5: Pendiente (Mesa 3 - Reservada)
        var comanda5 = Comanda.Crear(
            null,
            hoy.AddHours(18),
            null,
            mesa3Id,
            "Cliente en mesa 3",
            "COM-005"
        );
        comanda5.AgregarProducto(producto6Id, 1, 8.00m);

        DbContext.Comandas.AddRange(comanda1, comanda2, comanda3, comanda4, comanda5);
        await DbContext.SaveChangesAsync();
    }

    #endregion
}

// DTOs para el dashboard web (simplificados para los tests)
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

public class DashboardResumenDto
{
    public DashboardMetricasDto Metricas { get; set; } = new();
    public List<ProductoMasVendidoDto> ProductosMasVendidos { get; set; } = new();
    public List<VentaPorPeriodoDto> VentasPorPeriodo { get; set; } = new();
    public EstadoMesasDto EstadoMesas { get; set; } = new();
    public ComandasPorEstadoDto ComandasPorEstado { get; set; } = new();
    public List<IngresosPorHoraDto> IngresosPorHora { get; set; } = new();
}

public class DashboardMetricasDto
{
    public decimal VentasHoy { get; set; }
    public decimal VentasAyer { get; set; }
    public decimal VentasSemana { get; set; }
    public decimal VentasMes { get; set; }
    public int MesasOcupadas { get; set; }
    public int MesasDisponibles { get; set; }
    public int TotalMesas { get; set; }
    public int ComandasActivas { get; set; }
    public int ComandasCompletadas { get; set; }
    public int ProductosVendidosHoy { get; set; }
    public int ClientesAtendidosHoy { get; set; }
    public decimal PromedioTicket { get; set; }
    public decimal CrecimientoVentas { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

public class ProductoMasVendidoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal Ingresos { get; set; }
    public decimal PorcentajeTotal { get; set; }
}

public class VentaPorPeriodoDto
{
    public DateTime Fecha { get; set; }
    public decimal Ventas { get; set; }
    public int CantidadFacturas { get; set; }
}

public class EstadoMesasDto
{
    public int Disponibles { get; set; }
    public int Ocupadas { get; set; }
    public int Reservadas { get; set; }
    public int EnLimpieza { get; set; }
    public int Total { get; set; }
}

public class ComandasPorEstadoDto
{
    public int Pendientes { get; set; }
    public int EnProceso { get; set; }
    public int Listas { get; set; }
    public int Entregadas { get; set; }
    public int Canceladas { get; set; }
    public int Total { get; set; }
}

public class IngresosPorHoraDto
{
    public int Hora { get; set; }
    public decimal Ingresos { get; set; }
    public int CantidadFacturas { get; set; }
}
