using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public class TestDbContext : RestauranteProDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options, ILogger<RestauranteProDbContext> logger)
            : base(options, logger)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<NonAuditableTestEntity> NonAuditableTestEntities { get; set; }
        public DbSet<ProductoCategoria> ProductoCategorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuracion para TestEntity que hereda de EntityBase
            modelBuilder.Entity<TestEntity>(e =>
            {
                e.ToTable("TestEntities", "test");
                e.HasKey(x => x.Id);
            });

            // Configuracion para NonAuditableTestEntity que no hereda de EntityBase
            modelBuilder.Entity<NonAuditableTestEntity>(e =>
            {
                e.ToTable("NonAuditableTestEntities", "test");
                e.HasKey(x => x.Id);
            });
        }
    }
} 