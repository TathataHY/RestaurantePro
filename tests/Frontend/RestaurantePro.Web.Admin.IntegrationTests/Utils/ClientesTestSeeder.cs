using Microsoft.EntityFrameworkCore;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Domain.Comercial.Clientes.Enums;

namespace RestaurantePro.Web.Admin.IntegrationTests.Utils;

/// <summary>
/// Seeder para crear datos de prueba de clientes en los tests de integración
/// </summary>
public static class ClientesTestSeeder
{
    /// <summary>
    /// Crea clientes de prueba con diferentes segmentos y estados
    /// </summary>
    public static async Task<List<Guid>> SeedClientesAsync(RestauranteProDbContext context, int total, int activos)
    {
        var clienteIds = new List<Guid>();
        var random = new Random();

        // Crear clientes activos
        for (int i = 0; i < activos; i++)
        {
            var cliente = CrearClienteDePrueba(true, i);
            context.Clientes.Add(cliente);
            clienteIds.Add(cliente.Id);
        }

        // Crear clientes inactivos
        for (int i = activos; i < total; i++)
        {
            var cliente = CrearClienteDePrueba(false, i);
            context.Clientes.Add(cliente);
            clienteIds.Add(cliente.Id);
        }

        await context.SaveChangesAsync();
        return clienteIds;
    }

    /// <summary>
    /// Crea clientes con un segmento específico
    /// </summary>
    public static async Task<List<Guid>> SeedClientesConSegmentoAsync(RestauranteProDbContext context, int cantidad, string segmento)
    {
        var clienteIds = new List<Guid>();
        var segmentoEnum = Enum.Parse<SegmentoCliente>(segmento, true);

        for (int i = 0; i < cantidad; i++)
        {
            var cliente = CrearClienteConSegmento(segmentoEnum, i);
            context.Clientes.Add(cliente);
            clienteIds.Add(cliente.Id);
        }

        await context.SaveChangesAsync();
        return clienteIds;
    }

    /// <summary>
    /// Crea clientes con fechas de registro específicas
    /// </summary>
    public static async Task<List<Guid>> SeedClientesConFechasRegistroAsync(RestauranteProDbContext context, int cantidad, DateTime fechaDesde, DateTime fechaHasta)
    {
        var clienteIds = new List<Guid>();
        var random = new Random();

        for (int i = 0; i < cantidad; i++)
        {
            // Generar fecha aleatoria dentro del rango [fechaDesde, fechaHasta]
            var diasDiferencia = (fechaHasta - fechaDesde).Days;
            var diasAleatorios = random.Next(0, diasDiferencia + 1);
            var fechaRegistro = fechaDesde.AddDays(diasAleatorios);
            
            var cliente = CrearClienteConFechaRegistro(fechaRegistro, i);
            context.Clientes.Add(cliente);
            clienteIds.Add(cliente.Id);
        }

        await context.SaveChangesAsync();
        return clienteIds;
    }

    /// <summary>
    /// Crea clientes con filtros combinados
    /// </summary>
    public static async Task<List<Guid>> SeedClientesConFiltrosCombinadosAsync(RestauranteProDbContext context, int cantidad, string segmento, DateTime fechaDesde, DateTime fechaHasta, bool activo)
    {
        var clienteIds = new List<Guid>();
        var segmentoEnum = Enum.Parse<SegmentoCliente>(segmento, true);
        var random = new Random();

        for (int i = 0; i < cantidad; i++)
        {
            // Generar fecha aleatoria dentro del rango [fechaDesde, fechaHasta]
            var diasDiferencia = (fechaHasta - fechaDesde).Days;
            var diasAleatorios = random.Next(0, diasDiferencia + 1);
            var fechaRegistro = fechaDesde.AddDays(diasAleatorios);
            
            var cliente = CrearClienteConFiltrosCombinados(segmentoEnum, fechaRegistro, activo, i);
            context.Clientes.Add(cliente);
            clienteIds.Add(cliente.Id);
        }

        await context.SaveChangesAsync();
        return clienteIds;
    }

    /// <summary>
    /// Crea solo clientes activos
    /// </summary>
    public static async Task<List<Guid>> SeedClientesActivosAsync(RestauranteProDbContext context, int cantidad)
    {
        return await SeedClientesAsync(context, cantidad, cantidad);
    }

    private static Cliente CrearClienteDePrueba(bool activo, int indice)
    {
        var segmentos = Enum.GetValues<SegmentoCliente>();
        var random = new Random();
        var segmento = segmentos[random.Next(segmentos.Length)];

        return CrearClienteConSegmento(segmento, indice, activo);
    }

    private static Cliente CrearClienteConSegmento(SegmentoCliente segmento, int indice, bool activo = true)
    {
        var nombres = new[] { "Juan", "María", "Carlos", "Ana", "Luis", "Carmen", "Pedro", "Laura", "Diego", "Sofia" };
        var apellidos = new[] { "Pérez", "García", "López", "Martínez", "González", "Rodríguez", "Fernández", "Sánchez", "Ramírez", "Torres" };
        
        var random = new Random();
        var nombre = nombres[random.Next(nombres.Length)];
        var apellido = apellidos[random.Next(apellidos.Length)];

        // Generar email único usando patrones permitidos
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var guid = Guid.NewGuid().ToString("N")[..6];
        var email = $"test{indice}{guid}@test.com";

        var cliente = Cliente.Crear(
            ClienteNombre.Crear(nombre, apellido),
            Email.Create(email),
            PhoneNumber.Create($"+569{random.Next(10000000, 99999999)}"),
            DateTime.Now.AddYears(-random.Next(18, 65))
        );

        // Configurar segmento usando el método ActualizarSegmento
        cliente.ActualizarSegmento(segmento);

        // Desactivar cliente si es necesario
        if (!activo)
        {
            cliente.Desactivar();
        }

        return cliente;
    }

    private static Cliente CrearClienteConFechaRegistro(DateTime fechaRegistro, int indice)
    {
        var cliente = CrearClienteConSegmento(SegmentoCliente.Regular, indice);
        
        // Establecer la fecha de creación específica para el test
        cliente.SetFechaCreacionForTesting(fechaRegistro);
        
        return cliente;
    }

    private static Cliente CrearClienteConFiltrosCombinados(SegmentoCliente segmento, DateTime fechaRegistro, bool activo, int indice)
    {
        var cliente = CrearClienteConSegmento(segmento, indice, activo);
        
        // Establecer la fecha de creación específica para el test
        cliente.SetFechaCreacionForTesting(fechaRegistro);
        
        return cliente;
    }
}
