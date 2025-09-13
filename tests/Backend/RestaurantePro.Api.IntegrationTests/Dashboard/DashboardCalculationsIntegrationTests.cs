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
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;

namespace RestaurantePro.Api.IntegrationTests.Dashboard;

/// <summary>
/// Pruebas de integración para validar cálculos matemáticos del Dashboard
/// </summary>
public class DashboardCalculationsIntegrationTests : ApiIntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public DashboardCalculationsIntegrationTests(TestWebApplicationFactory factory) : base(factory)
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

    /// <summary>
    /// Crea datos específicos para validar cálculos exactos
    /// </summary>
    private async Task<(Guid producto1Id, Guid producto2Id, Guid mesa1Id, Guid mesa2Id, Guid cliente1Id, Guid cliente2Id)> CrearDatosEspecificosAsync()
    {
        // Crear categorías
        var categoriaComida = ProductoCategoria.Crear("Comida", "Platos principales", 1, "#FF5722", "��️");
        var categoriaBebida = ProductoCategoria.Crear("Bebida", "Bebidas y refrescos", 2, "#3B82F6", "🥤");

        // Crear productos con precios específicos
        var producto1 = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
            "Pizza Margherita",
            "Pizza clásica con tomate y mozzarella",
            new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(12.99m),
            categoriaComida.Id,
            "Comida"
        );

        var producto2 = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
            "Coca Cola",
            "Bebida gaseosa",
            new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(3.50m),
            categoriaBebida.Id,
            "Bebida"
        );

        // Crear mesas
        var mesa1 = RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(1, 4, "Salón Principal");
        var mesa2 = RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa.Crear(2, 2, "Terraza");

        // Crear clientes
        var cliente1 = RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
            RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear("Juan", "Pérez"),
            "juan@email.com",
            "1234567890",
            new DateTime(1990, 1, 1)
        );

        var cliente2 = RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente.Crear(
            RestaurantePro.Domain.Comercial.Clientes.ValueObjects.ClienteNombre.Crear("María", "González"),
            "maria@email.com",
            "0987654321",
            new DateTime(1985, 5, 15)
        );

        // Guardar en base de datos
        DbContext.ProductoCategorias.AddRange(categoriaComida, categoriaBebida);
        DbContext.Productos.AddRange(producto1, producto2);
        DbContext.Mesas.AddRange(mesa1, mesa2);
        DbContext.Clientes.AddRange(cliente1, cliente2);
        await DbContext.SaveChangesAsync();

        return (producto1.Id, producto2.Id, mesa1.Id, mesa2.Id, cliente1.Id, cliente2.Id);
    }

    /// <summary>
    /// Crea facturas específicas para validar cálculos de ventas
    /// </summary>
    private async Task CrearFacturasEspecificasAsync(Guid producto1Id, Guid producto2Id, Guid cliente1Id, Guid cliente2Id)
    {
        var hoy = DateTime.Today;
        var ayer = hoy.AddDays(-1);
        var semana = hoy.AddDays(-7);

        // Obtener clientes para nombres
        var cliente1 = await DbContext.Clientes.FindAsync(cliente1Id);
        var cliente2 = await DbContext.Clientes.FindAsync(cliente2Id);

        // Factura 1: Hoy - Pizza + Coca Cola = 12.99 + 3.50 = 16.49
        var factura1 = Factura.Crear(
            "FAC-001",
            TipoFactura.Electronica,
            cliente1!.Nombre.ToString(),
            cliente1Id,
            null, // identificacionFiscal
            null, // direccionCliente
            null, // comandasIds
            null, // observaciones
            hoy.AddHours(10) // 10:00 AM
        );

        // Agregar detalles a factura1
        factura1.AgregarDetalle(producto1Id, "Pizza Margherita", 1, 12.99m, 16.0m, 0m);
        factura1.AgregarDetalle(producto2Id, "Coca Cola", 1, 3.50m, 16.0m, 0m);
        factura1.Emitir(); // Emitir la factura
        factura1.RegistrarPago(factura1.Total, Guid.NewGuid()); // Marcar como pagada

        // Factura 2: Hoy - 2 Pizzas = 12.99 * 2 = 25.98
        var factura2 = Factura.Crear(
            "FAC-002",
            TipoFactura.Electronica,
            cliente2!.Nombre.ToString(),
            cliente2Id,
            null, // identificacionFiscal
            null, // direccionCliente
            null, // comandasIds
            null, // observaciones
            hoy.AddHours(14) // 2:00 PM
        );

        factura2.AgregarDetalle(producto1Id, "Pizza Margherita", 2, 12.99m, 16.0m, 0m);
        factura2.Emitir(); // Emitir la factura
        factura2.RegistrarPago(factura2.Total, Guid.NewGuid()); // Marcar como pagada

        // Factura 3: Ayer - Coca Cola = 3.50
        var factura3 = Factura.Crear(
            "FAC-003",
            TipoFactura.Electronica,
            cliente1.Nombre.ToString(),
            cliente1Id,
            null, // identificacionFiscal
            null, // direccionCliente
            null, // comandasIds
            null, // observaciones
            ayer.AddHours(20) // 8:00 PM ayer
        );

        factura3.AgregarDetalle(producto2Id, "Coca Cola", 1, 3.50m, 16.0m, 0m);
        factura3.Emitir(); // Emitir la factura
        factura3.RegistrarPago(factura3.Total, Guid.NewGuid()); // Marcar como pagada

        // Factura 4: Semana pasada - Pizza = 12.99
        var factura4 = Factura.Crear(
            "FAC-004",
            TipoFactura.Electronica,
            cliente2.Nombre.ToString(),
            cliente2Id,
            null, // identificacionFiscal
            null, // direccionCliente
            null, // comandasIds
            null, // observaciones
            semana.AddHours(12) // 12:00 PM semana pasada
        );

        factura4.AgregarDetalle(producto1Id, "Pizza Margherita", 1, 12.99m, 16.0m, 0m);
        factura4.Emitir(); // Emitir la factura
        factura4.RegistrarPago(factura4.Total, Guid.NewGuid()); // Marcar como pagada

        DbContext.Facturas.AddRange(factura1, factura2, factura3, factura4);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Crea comandas específicas para validar cálculos de comandas
    /// </summary>
    private async Task CrearComandasEspecificasAsync(Guid producto1Id, Guid producto2Id, Guid mesa1Id, Guid mesa2Id)
    {
        var hoy = DateTime.Today;

        // Comanda 1: Hoy - En Proceso
        var comanda1 = Comanda.Crear(
            null, // meseroId
            hoy.AddHours(10), // fechaCreacion
            null, // clienteId
            mesa1Id,
            "Cliente en mesa 1",
            "COM-001"
        );

        comanda1.AgregarProducto(producto1Id, 1, 12.99m);
        comanda1.ActualizarEstado(EstadoComanda.EnProceso);

        // Comanda 2: Hoy - Lista
        var comanda2 = Comanda.Crear(
            null, // meseroId
            hoy.AddHours(11), // fechaCreacion
            null, // clienteId
            mesa2Id,
            "Cliente en mesa 2",
            "COM-002"
        );

        comanda2.AgregarProducto(producto2Id, 2, 3.50m);
        comanda2.ActualizarEstado(EstadoComanda.EnProceso);
        comanda2.ActualizarEstado(EstadoComanda.Lista);

        // Comanda 3: Hoy - Entregada
        var comanda3 = Comanda.Crear(
            null, // meseroId
            hoy.AddHours(12), // fechaCreacion
            null, // clienteId
            mesa1Id,
            "Cliente en mesa 1",
            "COM-003"
        );

        comanda3.AgregarProducto(producto1Id, 1, 12.99m);
        comanda3.ActualizarEstado(EstadoComanda.EnProceso);
        comanda3.ActualizarEstado(EstadoComanda.Lista);
        comanda3.ActualizarEstado(EstadoComanda.Entregada);

        // Comanda 4: Ayer - Cancelada
        var comanda4 = Comanda.Crear(
            null, // meseroId
            hoy.AddDays(-1).AddHours(15), // fechaCreacion
            null, // clienteId
            mesa2Id,
            "Cliente en mesa 2",
            "COM-004"
        );

        comanda4.AgregarProducto(producto2Id, 1, 3.50m);
        comanda4.Cancelar("Cancelación de prueba");

        DbContext.Comandas.AddRange(comanda1, comanda2, comanda3, comanda4);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Crea mesas con estados específicos para validar cálculos
    /// </summary>
    private async Task ConfigurarEstadosMesasAsync(Guid mesa1Id, Guid mesa2Id)
    {
        var mesa1 = await DbContext.Mesas.FindAsync(mesa1Id);
        var mesa2 = await DbContext.Mesas.FindAsync(mesa2Id);

        // Mesa 1: Ocupada
        mesa1!.MarcarComoOcupada();

        // Mesa 2: Disponible (ya está disponible por defecto)
        // mesa2!.MarcarComoDisponible(); // No es necesario, ya está disponible

        await DbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Dashboard_ConDatosEspecificos_DeberiaCalcularVentasCorrectamente()
    {
        // Arrange
        var (producto1Id, producto2Id, mesa1Id, mesa2Id, cliente1Id, cliente2Id) = await CrearDatosEspecificosAsync();
        await CrearFacturasEspecificasAsync(producto1Id, producto2Id, cliente1Id, cliente2Id);
        await CrearComandasEspecificasAsync(producto1Id, producto2Id, mesa1Id, mesa2Id);
        await ConfigurarEstadosMesasAsync(mesa1Id, mesa2Id);

        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=todos");
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

        // Debug: Verificar qué facturas se están consultando
        var facturasEnDb = await DbContext.Facturas.ToListAsync();
        Console.WriteLine($"Facturas en DB: {facturasEnDb.Count}");
        foreach (var f in facturasEnDb)
        {
            Console.WriteLine($"Factura {f.NumeroFactura}: Estado={f.Estado}, FechaEmision={f.FechaEmision}, Total={f.Total}");
        }
        
        // Debug: Verificar respuesta del dashboard
        Console.WriteLine($"Dashboard Success: {dashboard.Success}");
        Console.WriteLine($"Dashboard Data: {dashboard.Data != null}");
        if (dashboard.Data != null)
        {
            Console.WriteLine($"Ventas Hoy: {dashboard.Data.Metricas.VentasHoy}");
            Console.WriteLine($"Productos Mas Vendidos Count: {dashboard.Data.ProductosMasVendidos?.Count ?? 0}");
        }

        Assert.NotNull(dashboard);
        Assert.True(dashboard.Success);
        Assert.NotNull(dashboard.Data);

        // Validar cálculos exactos de ventas de hoy
        // Factura1: 19.13 + Factura2: 30.14 = 49.27 (con impuestos del 16%)
        Assert.Equal(49.27m, dashboard.Data.Metricas.VentasHoy);
        
        // Validar estado de mesas
        Assert.Equal(1, dashboard.Data.EstadoMesas.Ocupadas); // Mesa 1
        Assert.Equal(1, dashboard.Data.EstadoMesas.Disponibles); // Mesa 2
        Assert.Equal(2, dashboard.Data.EstadoMesas.Total); // Total mesas

        // Validar comandas por estado
        Assert.Equal(1, dashboard.Data.ComandasPorEstado.EnProceso); // Comanda 1
        Assert.Equal(1, dashboard.Data.ComandasPorEstado.Listas); // Comanda 2
        Assert.Equal(1, dashboard.Data.ComandasPorEstado.Entregadas); // Comanda 3
        Assert.Equal(0, dashboard.Data.ComandasPorEstado.Canceladas); // Comanda 4 está de ayer
        Assert.Equal(3, dashboard.Data.ComandasPorEstado.Total); // Total comandas de hoy

        // Validar productos más vendidos
        Assert.NotNull(dashboard.Data.ProductosMasVendidos);
        Assert.True(dashboard.Data.ProductosMasVendidos.Count > 0);
        
        // Pizza Margherita debería ser el más vendido (3 unidades: 1 en factura1, 2 en factura2)
        var pizzaMasVendida = dashboard.Data.ProductosMasVendidos.FirstOrDefault(p => p.Nombre == "Pizza Margherita");
        Assert.NotNull(pizzaMasVendida);
        Assert.Equal(3, pizzaMasVendida.CantidadVendida);
        Assert.Equal(38.97m, pizzaMasVendida.Ingresos); // 12.99 * 3 = 38.97

        // Coca Cola debería ser el segundo (1 unidad de hoy)
        var cocaCola = dashboard.Data.ProductosMasVendidos.FirstOrDefault(p => p.Nombre == "Coca Cola");
        Assert.NotNull(cocaCola);
        Assert.Equal(1, cocaCola.CantidadVendida); // Solo de hoy
        Assert.Equal(3.50m, cocaCola.Ingresos);
    }

    [Fact]
    public async Task Dashboard_ConFiltroAyer_DeberiaCalcularVentasCorrectamente()
    {
        // Arrange
        var (producto1Id, producto2Id, mesa1Id, mesa2Id, cliente1Id, cliente2Id) = await CrearDatosEspecificosAsync();
        await CrearFacturasEspecificasAsync(producto1Id, producto2Id, cliente1Id, cliente2Id);

        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=ayer&turno=todos");
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

        // Validar cálculos exactos de ventas de ayer
        // Solo factura3: 4.06 (con impuestos del 16%)
        Assert.Equal(4.06m, dashboard.Data.Metricas.VentasAyer);
    }

    [Fact]
    public async Task Dashboard_ConFiltroTurnoManana_DeberiaCalcularVentasCorrectamente()
    {
        // Arrange
        var (producto1Id, producto2Id, mesa1Id, mesa2Id, cliente1Id, cliente2Id) = await CrearDatosEspecificosAsync();
        await CrearFacturasEspecificasAsync(producto1Id, producto2Id, cliente1Id, cliente2Id);

        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=mañana");
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

        // Validar cálculos exactos de ventas de turno mañana (6:00 AM - 12:00 PM)
        // Solo factura1: 19.13 (10:00 AM, con impuestos del 16%)
        Assert.Equal(19.13m, dashboard.Data.Metricas.VentasHoy);
    }

    [Fact]
    public async Task Dashboard_ConFiltroTurnoTarde_DeberiaCalcularVentasCorrectamente()
    {
        // Arrange
        var (producto1Id, producto2Id, mesa1Id, mesa2Id, cliente1Id, cliente2Id) = await CrearDatosEspecificosAsync();
        await CrearFacturasEspecificasAsync(producto1Id, producto2Id, cliente1Id, cliente2Id);

        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=tarde");
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

        // Validar cálculos exactos de ventas de turno tarde (12:00 PM - 6:00 PM)
        // Solo factura2: 30.14 (2:00 PM, con impuestos del 16%)
        Assert.Equal(30.14m, dashboard.Data.Metricas.VentasHoy);
    }

    [Fact]
    public async Task Dashboard_ConFiltroSemana_DeberiaCalcularVentasCorrectamente()
    {
        // Arrange
        var (producto1Id, producto2Id, mesa1Id, mesa2Id, cliente1Id, cliente2Id) = await CrearDatosEspecificosAsync();
        await CrearFacturasEspecificasAsync(producto1Id, producto2Id, cliente1Id, cliente2Id);

        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=semana&turno=todos");
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

        // Validar cálculos exactos de ventas de la semana
        // Factura1: 19.13 + Factura2: 30.14 + Factura3: 4.06 + Factura4: 15.07 = 68.40 (con impuestos del 16%)
        Assert.Equal(68.40m, dashboard.Data.Metricas.VentasSemana);
    }

    [Fact]
    public async Task Dashboard_ConDatosVacios_DeberiaRetornarCeros()
    {
        // Arrange - No crear datos
        var token = await ObtenerTokenAdminAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/dashboard/resumen?periodo=hoy&turno=todos");
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

        // Validar que todos los valores sean cero
        Assert.Equal(0, dashboard.Data.Metricas.VentasHoy);
        Assert.Equal(0, dashboard.Data.Metricas.ComandasActivas);
        Assert.Equal(0, dashboard.Data.Metricas.TotalMesas);
        Assert.Equal(0, dashboard.Data.Metricas.MesasOcupadas);
        Assert.Equal(0, dashboard.Data.EstadoMesas.Total);
        Assert.Equal(0, dashboard.Data.ComandasPorEstado.Total);
        Assert.Empty(dashboard.Data.ProductosMasVendidos);
        // VentasPorPeriodo puede tener datos para los últimos 7 días con ventas en 0
        Assert.True(dashboard.Data.VentasPorPeriodo.All(v => v.Ventas == 0));
        // IngresosPorHora puede tener datos para las 24 horas con ingresos en 0
        Assert.True(dashboard.Data.IngresosPorHora.All(i => i.Ingresos == 0));
    }
}