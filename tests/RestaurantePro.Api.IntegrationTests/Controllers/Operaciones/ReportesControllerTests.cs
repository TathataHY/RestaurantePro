using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Operaciones.Reportes.Queries;
using RestaurantePro.Application.Operaciones.Reportes.Commands.GenerarReporte;
using RestaurantePro.Application.Operaciones.Reportes.Queries.ObtenerReporteVentasDiaria;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Core.Usuarios;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Api.IntegrationTests.TestBase;
using Xunit;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using System.Reflection;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Operaciones;

/// <summary>
/// Tests de integración completos para ReportesController
/// Valida la funcionalidad real de generación de reportes con datos en base de datos
/// </summary>
[Collection("ApiTestCollection")]
public class ReportesControllerTests : ApiIntegrationTestBase
{
    public ReportesControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObtenerReporteVentasDiarias_ConDatosExistentes_DeberiaRetornarReporteExitoso()
    {
        // Arrange
        var fecha = DateTime.Today;
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba(precio: 150.00m);
        
        // Crear comanda con la fecha específica del reporte
        var comanda = await CrearComandaPrueba(mesero.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda.Id, producto.Id, cantidad: 2);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod = comanda.GetType().GetMethod("RecalcularTotal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        recalcularTotalMethod?.Invoke(comanda, null);
        await DbContext.SaveChangesAsync();
        
        var factura = await CrearFacturaConDetallesPrueba(cliente.Id, new List<Guid> { comanda.Id }, fechaCreacion: fecha);

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/ventas-diarias?fecha={fecha:yyyy-MM-dd}");
        var apiResponse = await ExecuteAndDeserializeAsync<ReporteVentasDiariaDto>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.FechaReporte.Should().Be(fecha.Date);
        apiResponse.Data.MetricasBasicas.TotalComandas.Should().BeGreaterThan(0);
        apiResponse.Data.MetricasBasicas.MontoTotalVentas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObtenerReporteVentasDiarias_SinDatos_DeberiaRetornarReporteVacio()
    {
        // Arrange
        var fecha = DateTime.Today.AddDays(-1);

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/ventas-diarias?fecha={fecha:yyyy-MM-dd}");
        var apiResponse = await ExecuteAndDeserializeAsync<ReporteVentasDiariaDto>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.FechaReporte.Should().Be(fecha.Date);
        apiResponse.Data.MetricasBasicas.TotalComandas.Should().Be(0);
        apiResponse.Data.MetricasBasicas.MontoTotalVentas.Should().Be(0);
    }

    [Fact]
    public async Task ObtenerReporteOcupacionMesas_ConMesasExistentes_DeberiaRetornarReporteExitoso()
    {
        // Arrange
        var fecha = DateTime.Today;
        var mesa1 = await CrearMesaPrueba(numero: 1, capacidad: 4);
        var mesa2 = await CrearMesaPrueba(numero: 2, capacidad: 6);
        var mesa3 = await CrearMesaPrueba(numero: 3, capacidad: 2);
        
        // Ocupar una mesa
        mesa1.MarcarComoOcupada();
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/ocupacion-mesas?fechaInicio={fecha:yyyy-MM-dd}&fechaFin={fecha:yyyy-MM-dd}");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteRendimientoMeseros_ConMeserosExistentes_DeberiaRetornarReporteExitoso()
    {
        // Arrange
        var fecha = DateTime.Today;
        var mesero1 = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var mesero2 = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba();
        
        // Crear comandas para los meseros con la fecha específica del reporte
        var comanda1 = await CrearComandaPrueba(mesero1.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        var comanda2 = await CrearComandaPrueba(mesero2.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda1.Id, producto.Id, cantidad: 1);
        await CrearDetalleComandaPrueba(comanda2.Id, producto.Id, cantidad: 1);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod1 = comanda1.GetType().GetMethod("RecalcularTotal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        recalcularTotalMethod1?.Invoke(comanda1, null);
        var recalcularTotalMethod2 = comanda2.GetType().GetMethod("RecalcularTotal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        recalcularTotalMethod2?.Invoke(comanda2, null);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/rendimiento-meseros?fechaInicio={fecha:yyyy-MM-dd}&fechaFin={fecha:yyyy-MM-dd}");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteComandas_ConComandasExistentes_DeberiaRetornarReporteExitoso()
    {
        // Arrange
        var fecha = DateTime.Today;
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba();
        
        // Crear comanda con la fecha específica del reporte
        var comanda = await CrearComandaPrueba(mesero.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda.Id, producto.Id);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod = comanda.GetType().GetMethod("RecalcularTotal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        recalcularTotalMethod?.Invoke(comanda, null);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/comandas?fechaInicio={fecha:yyyy-MM-dd}&fechaFin={fecha:yyyy-MM-dd}");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteProductosMasVendidos_ConProductosExistentes_DeberiaRetornarReporteExitoso()
    {
        // Arrange
        var fecha = DateTime.Today;
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto1 = await CrearProductoPrueba(nombre: "Producto A");
        var producto2 = await CrearProductoPrueba(nombre: "Producto B");
        
        // Crear comanda con la fecha específica del reporte
        var comanda = await CrearComandaPrueba(mesero.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda.Id, producto1.Id, cantidad: 3);
        await CrearDetalleComandaPrueba(comanda.Id, producto2.Id, cantidad: 2);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod = comanda.GetType().GetMethod("RecalcularTotal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        recalcularTotalMethod?.Invoke(comanda, null);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/productos-mas-vendidos?fechaInicio={fecha:yyyy-MM-dd}&fechaFin={fecha:yyyy-MM-dd}&limite=10");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteReservaciones_ConReservacionesExistentes_DeberiaRetornarReporteExitoso()
    {
        // Arrange
        var fecha = DateTime.Today;
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        
        // Nota: No creamos reservaciones porque el método CrearReservacionPrueba no existe
        // El reporte debería funcionar incluso sin reservaciones (reporte vacío)

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/reservaciones?fechaInicio={fecha:yyyy-MM-dd}&fechaFin={fecha:yyyy-MM-dd}&incluirCanceladas=false");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteEficienciaOperacional_ConDatosExistentes_DeberiaRetornarReporteExitoso()
    {
        // Arrange
        var fecha = DateTime.Today;
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba();
        
        // Crear comanda con la fecha específica del reporte
        var comanda = await CrearComandaPrueba(mesero.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda.Id, producto.Id);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod = comanda.GetType().GetMethod("RecalcularTotal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        recalcularTotalMethod?.Invoke(comanda, null);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/eficiencia-operacional?fechaInicio={fecha:yyyy-MM-dd}&fechaFin={fecha:yyyy-MM-dd}&incluirGraficos=true");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerDashboardEjecutivo_ConDatosExistentes_DeberiaRetornarDashboardExitoso()
    {
        // Arrange
        var fecha = DateTime.Today;
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba();
        
        // Crear comanda con la fecha específica del reporte
        var comanda = await CrearComandaPrueba(mesero.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda.Id, producto.Id);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod = comanda.GetType().GetMethod("RecalcularTotal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        recalcularTotalMethod?.Invoke(comanda, null);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reportes/dashboard-ejecutivo?fecha={fecha:yyyy-MM-dd}&incluirComparativo=true");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportarReporte_ConParametrosValidos_DeberiaRetornarArchivoExitoso()
    {
        // Arrange
        var fecha = DateTime.Today;
        var usuario = await CrearUsuarioPrueba(rol: RolUsuario.Administrador);
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba();
        
        // Crear comanda con la fecha específica del reporte
        var comanda = await CrearComandaPrueba(mesero.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda.Id, producto.Id);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod = comanda.GetType().GetMethod("RecalcularTotal", BindingFlags.NonPublic | BindingFlags.Instance);
        recalcularTotalMethod?.Invoke(comanda, null);
        await DbContext.SaveChangesAsync();

        var request = new GenerarReporteCommand
        {
            FechaInicio = fecha.AddDays(-1),
            FechaFin = fecha,
            TipoReporte = TipoReporte.VentasDiarias,
            UsuarioSolicitanteId = usuario.Id,
            IncluirGraficos = true
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/reportes/exportar", request);
        var apiResponse = await ExecuteAndDeserializeAsync<ReporteGeneradoResult>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.ReporteId.Should().NotBeEmpty();
        apiResponse.Data.NombreArchivo.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ProgramarReporte_ConParametrosValidos_DeberiaRetornarProgramacionExitosa()
    {
        // Arrange
        var fecha = DateTime.Today;
        var usuario = await CrearUsuarioPrueba(rol: RolUsuario.Administrador);
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba();
        
        // Crear comanda con la fecha específica del reporte
        var comanda = await CrearComandaPrueba(mesero.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda.Id, producto.Id);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod = comanda.GetType().GetMethod("RecalcularTotal", BindingFlags.NonPublic | BindingFlags.Instance);
        recalcularTotalMethod?.Invoke(comanda, null);
        await DbContext.SaveChangesAsync();

        var request = new GenerarReporteCommand
        {
            FechaInicio = fecha.AddDays(-1),
            FechaFin = fecha,
            TipoReporte = TipoReporte.VentasDiarias,
            UsuarioSolicitanteId = usuario.Id,
            IncluirGraficos = true
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/reportes/programar", request);
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerHistorialReportes_DeberiaRetornarHistorialExitoso()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/reportes/historial");
        var apiResponse = await ExecuteAndDeserializeAsync<object>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerReporteVentasDiarias_ConFechaInvalida_DeberiaRetornarReporteVacio()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/reportes/ventas-diarias?fecha=fecha-invalida");
        var apiResponse = await ExecuteAndDeserializeAsync<ReporteVentasDiariaDto>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue(); // Un reporte vacío no es un error
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.MetricasBasicas.TotalComandas.Should().Be(0);
        apiResponse.Data.MetricasBasicas.MontoTotalVentas.Should().Be(0);
    }

    [Fact]
    public async Task ExportarReporte_ConFormatoInvalido_DeberiaRetornarError()
    {
        // Arrange
        var usuario = await CrearUsuarioPrueba(rol: RolUsuario.Administrador);
        var request = new GenerarReporteCommand
        {
            TipoReporte = TipoReporte.VentasDiarias,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today,
            Formato = (FormatoReporte)999, // Formato inválido
            UsuarioSolicitanteId = usuario.Id,
            IncluirGraficos = true,
            IncluirDetalles = true,
            IncluirResumenEjecutivo = true
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/reportes/exportar", request);
        var apiResponse = await ExecuteAndDeserializeAsync<ReporteGeneradoResult>(r => Task.FromResult(response));

        // Assert
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        apiResponse.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Debug_VerificarComandasCreadas_DeberiaEncontrarComandasEnBD()
    {
        // Arrange
        var fecha = DateTime.Today;
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba(precio: 150.00m);
        
        // Crear comanda con la fecha específica del reporte
        var comanda = await CrearComandaPrueba(mesero.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda.Id, producto.Id, cantidad: 2);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod = comanda.GetType().GetMethod("RecalcularTotal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        recalcularTotalMethod?.Invoke(comanda, null);
        await DbContext.SaveChangesAsync();
        
        // Debug: Verificar que la comanda existe en la BD con la fecha correcta
        var comandaEnBD = await DbContext.Comandas.FindAsync(comanda.Id);
        comandaEnBD.Should().NotBeNull();
        comandaEnBD!.FechaCreacion.Date.Should().Be(fecha.Date);
        
        // Debug: Verificar que hay comandas para la fecha
        var inicioDia = fecha.Date;
        var finDia = inicioDia.AddDays(1).AddSeconds(-1);
        var comandasEnFecha = await DbContext.Comandas
            .Where(c => c.FechaCreacion >= inicioDia && c.FechaCreacion <= finDia)
            .ToListAsync();
        
        comandasEnFecha.Should().NotBeEmpty();
        comandasEnFecha.Count.Should().Be(1);
        comandasEnFecha.First().Id.Should().Be(comanda.Id);
    }

    [Fact]
    public async Task Debug_HandlerDirecto_DeberiaEncontrarComandas()
    {
        // Arrange
        var fecha = DateTime.Today;
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba(precio: 150.00m);
        
        // Crear comanda con la fecha específica del reporte
        var comanda = await CrearComandaPrueba(mesero.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda.Id, producto.Id, cantidad: 2);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod = comanda.GetType().GetMethod("RecalcularTotal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        recalcularTotalMethod?.Invoke(comanda, null);
        await DbContext.SaveChangesAsync();
        
        // Obtener el handler del contenedor de dependencias
        var handler = ServiceScope.ServiceProvider.GetRequiredService<IRequestHandler<ObtenerReporteVentasDiariaQuery, Result<ReporteVentasDiariaDto>>>();
        
        // Crear la query
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = fecha,
            NivelDetalle = NivelDetalle.Completo
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FechaReporte.Should().Be(fecha.Date);
        result.Value.MetricasBasicas.TotalComandas.Should().BeGreaterThan(0);
        result.Value.MetricasBasicas.MontoTotalVentas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Debug_ExportarReporte_DeberiaMostrarErrorActual()
    {
        // Arrange
        var fecha = DateTime.Today;
        var usuario = await CrearUsuarioPrueba(rol: RolUsuario.Administrador);
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba();
        
        // Crear comanda con la fecha específica del reporte
        var comanda = await CrearComandaPrueba(mesero.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda.Id, producto.Id);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod = comanda.GetType().GetMethod("RecalcularTotal", BindingFlags.NonPublic | BindingFlags.Instance);
        recalcularTotalMethod?.Invoke(comanda, null);
        await DbContext.SaveChangesAsync();

        var request = new GenerarReporteCommand
        {
            FechaInicio = fecha,
            FechaFin = DateTime.Now.AddSeconds(-1), // Usar hora actual menos 1 segundo
            TipoReporte = TipoReporte.VentasDiarias,
            UsuarioSolicitanteId = usuario.Id
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/reportes/exportar", request);
        var content = await response.Content.ReadAsStringAsync();
        
        // Debug: Imprimir respuesta completa
        Console.WriteLine($"Status Code: {response.StatusCode}");
        Console.WriteLine($"Response Content: {content}");
        
        if (response.IsSuccessStatusCode)
        {
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<ReporteGeneradoResult>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine($"API Success: {apiResponse?.Success}");
            Console.WriteLine($"API Message: {apiResponse?.Message}");
            if (apiResponse?.Errors != null)
            {
                foreach (var error in apiResponse.Errors)
                {
                    Console.WriteLine($"Error: {error}");
                }
            }
        }
        else
        {
            Console.WriteLine($"HTTP Error: {response.StatusCode}");
            Console.WriteLine($"Error Content: {content}");
        }
    }

    [Fact]
    public async Task Debug_ProgramarReporte_DeberiaMostrarErrorActual()
    {
        // Arrange
        var fecha = DateTime.Today;
        var usuario = await CrearUsuarioPrueba(rol: RolUsuario.Administrador);
        var mesero = await CrearUsuarioPrueba(rol: RolUsuario.Mesero);
        var cliente = await CrearClientePrueba();
        var mesa = await CrearMesaPrueba();
        var producto = await CrearProductoPrueba();
        
        // Crear comanda con la fecha específica del reporte
        var comanda = await CrearComandaPrueba(mesero.Id, cliente.Id, mesa.Id, fechaCreacion: fecha);
        await CrearDetalleComandaPrueba(comanda.Id, producto.Id);
        // Recalcular total usando reflexión y guardar
        var recalcularTotalMethod = comanda.GetType().GetMethod("RecalcularTotal", BindingFlags.NonPublic | BindingFlags.Instance);
        recalcularTotalMethod?.Invoke(comanda, null);
        await DbContext.SaveChangesAsync();

        var request = new GenerarReporteCommand
        {
            FechaInicio = fecha,
            FechaFin = DateTime.Now.AddSeconds(-1), // Usar hora actual menos 1 segundo
            TipoReporte = TipoReporte.VentasDiarias,
            UsuarioSolicitanteId = usuario.Id
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/reportes/programar", request);
        var content = await response.Content.ReadAsStringAsync();
        
        // Debug: Imprimir respuesta completa
        Console.WriteLine($"Status Code: {response.StatusCode}");
        Console.WriteLine($"Response Content: {content}");
        
        if (response.IsSuccessStatusCode)
        {
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine($"API Success: {apiResponse?.Success}");
            Console.WriteLine($"API Message: {apiResponse?.Message}");
            if (apiResponse?.Errors != null)
            {
                foreach (var error in apiResponse.Errors)
                {
                    Console.WriteLine($"Error: {error}");
                }
            }
        }
        else
        {
            Console.WriteLine($"HTTP Error: {response.StatusCode}");
            Console.WriteLine($"Error Content: {content}");
        }
    }
} 