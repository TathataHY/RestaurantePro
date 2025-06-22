using Microsoft.EntityFrameworkCore;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase;

public class TestInterceptorDbContext : DbContext
{
    public DbSet<AuditableTestEntity> AuditableTestEntities { get; set; }

    public TestInterceptorDbContext(DbContextOptions<TestInterceptorDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditableTestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Ignore(e => e.DomainEvents);
        });

        // Registrar entidades de prueba
        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.Ignore(e => e.DomainEvents);
        });
        modelBuilder.Entity<NonAuditableTestEntity>();
    }
} 