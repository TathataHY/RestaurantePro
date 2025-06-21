using Microsoft.EntityFrameworkCore;

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
        });
    }
} 