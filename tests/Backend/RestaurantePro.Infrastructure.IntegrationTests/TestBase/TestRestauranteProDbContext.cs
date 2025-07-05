using Microsoft.EntityFrameworkCore;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase;

/// <summary>
/// Contexto de base de datos personalizado para tests que incluye entidades de prueba
/// </summary>
public class TestRestauranteProDbContext : RestauranteProDbContext
{
    public DbSet<TestEntity> TestEntities { get; set; }
    public DbSet<NonAuditableTestEntity> NonAuditableTestEntities { get; set; }

    public TestRestauranteProDbContext(DbContextOptions<RestauranteProDbContext> options)
        : base(options, null!, null!) // Usar null! para los parámetros requeridos en tests
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Registrar entidades de prueba
        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.DomainEvents); // Ignorar la propiedad DomainEvents
        });

        modelBuilder.Entity<NonAuditableTestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valor).IsRequired().HasMaxLength(100); // Usar Valor en lugar de Nombre
        });
    }
} 