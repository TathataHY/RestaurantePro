using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Proveedores.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.UnitTests.Common;

/// <summary>
/// Contexto de base de datos en memoria para tests
/// </summary>
public class TestDbContext : DbContext, IApplicationDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    // Implementación de IApplicationDbContext
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Receta> Recetas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<TarjetaFidelizacion> TarjetasFidelizacion { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<Promocion> Promociones { get; set; }
    public DbSet<Comanda> Comandas { get; set; }
    public DbSet<ItemComanda> ItemsComanda { get; set; }
    public DbSet<Reservacion> Reservaciones { get; set; }
    public DbSet<Mesa> Mesas { get; set; }
    public DbSet<Ingrediente> Ingredientes { get; set; }
    public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
    public DbSet<OrdenCompra> OrdenesCompra { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<ContactoProveedor> ContactosProveedor { get; set; }
    public DbSet<PreparacionDiaria> Preparaciones { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración básica para que las entidades funcionen en memoria
        // Sin aplicar configuraciones complejas que puedan causar problemas en tests
        
        // Configurar claves primarias para entidades que las necesiten
        modelBuilder.Entity<Cliente>().HasKey(c => c.Id);
        modelBuilder.Entity<Producto>().HasKey(p => p.Id);
        modelBuilder.Entity<Usuario>().HasKey(u => u.Id);
        modelBuilder.Entity<Notificacion>().HasKey(n => n.Id);
        modelBuilder.Entity<TarjetaFidelizacion>().HasKey(t => t.Id);
        modelBuilder.Entity<Factura>().HasKey(f => f.Id);
        modelBuilder.Entity<Promocion>().HasKey(p => p.Id);
        modelBuilder.Entity<Comanda>().HasKey(c => c.Id);
        modelBuilder.Entity<ItemComanda>().HasKey(i => i.Id);
        modelBuilder.Entity<Reservacion>().HasKey(r => r.Id);
        modelBuilder.Entity<Mesa>().HasKey(m => m.Id);
        modelBuilder.Entity<Ingrediente>().HasKey(i => i.Id);
        modelBuilder.Entity<MovimientoInventario>().HasKey(m => m.Id);
        modelBuilder.Entity<OrdenCompra>().HasKey(o => o.Id);
        modelBuilder.Entity<Proveedor>().HasKey(p => p.Id);
        modelBuilder.Entity<ContactoProveedor>().HasKey(c => c.Id);
        modelBuilder.Entity<PreparacionDiaria>().HasKey(p => p.Id);

        // Configurar value objects - ClienteNombre
        modelBuilder.Entity<Cliente>().OwnsOne(c => c.Nombre);
    }
}

/// <summary>
/// Factory para crear contextos de base de datos en memoria para tests
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Crea un nuevo contexto de base de datos en memoria
    /// </summary>
    /// <param name="databaseName">Nombre único para la base de datos (opcional)</param>
    /// <returns>Contexto de base de datos configurado</returns>
    public static TestDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new TestDbContext(options);
        
        // Asegurar que la base de datos esté creada
        context.Database.EnsureCreated();
        
        return context;
    }

    /// <summary>
    /// Crea un contexto y lo puebla con datos iniciales para tests
    /// </summary>
    /// <param name="seedAction">Acción para poblar datos iniciales</param>
    /// <param name="databaseName">Nombre único para la base de datos (opcional)</param>
    /// <returns>Contexto poblado con datos</returns>
    public static TestDbContext CreateWithData(Action<TestDbContext> seedAction, string? databaseName = null)
    {
        var context = Create(databaseName);
        
        seedAction(context);
        context.SaveChanges();
        
        return context;
    }
} 