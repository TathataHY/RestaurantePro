using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Web.Admin.IntegrationTests.Core;
using RestaurantePro.Web.Admin.IntegrationTests.Utils;
using Xunit;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.ReportesComercial;

public class TestDebugFechas : IClassFixture<WebApplicationFactory>
{
    private readonly WebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TestDebugFechas(WebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task DebugFechas_DeberiaMostrarFechasGeneradas()
    {
        // Arrange
        var fechaDesde = DateTime.Now.AddDays(-30);
        var fechaHasta = DateTime.Now.AddDays(-15);
        
        // Crear clientes usando el seeder
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        var clienteIds = await ClientesTestSeeder.SeedClientesConFechasRegistroAsync(context, 8, fechaDesde, fechaHasta);
        
        // Verificar las fechas de los clientes creados
        var clientes = await context.Clientes.Where(c => clienteIds.Contains(c.Id)).ToListAsync();
        
        Console.WriteLine($"Fecha desde: {fechaDesde:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Fecha hasta: {fechaHasta:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Clientes creados: {clientes.Count}");
        
        foreach (var cliente in clientes)
        {
            Console.WriteLine($"Cliente {cliente.Id}: FechaCreacion = {cliente.FechaCreacion:yyyy-MM-dd HH:mm:ss}");
        }
        
        // Verificar cuántos clientes están en el rango
        var clientesEnRango = clientes.Where(c => c.FechaCreacion >= fechaDesde && c.FechaCreacion <= fechaHasta).ToList();
        Console.WriteLine($"Clientes en rango: {clientesEnRango.Count}");
        
        // Assert
        clientes.Count.Should().Be(8);
        clientesEnRango.Count.Should().Be(8);
    }
}
