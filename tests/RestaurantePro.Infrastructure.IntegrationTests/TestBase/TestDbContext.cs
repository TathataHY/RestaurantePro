using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public class TestDbContext : RestauranteProDbContext
    {
        public TestDbContext(
            DbContextOptions<RestauranteProDbContext> options, 
            ILogger<RestauranteProDbContext> logger,
            IDomainEventDispatcher dispatcher)
            : base(options, logger, dispatcher)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<NonAuditableTestEntity> NonAuditableTestEntities { get; set; }
        public DbSet<AuditableTestEntity> AuditableTestEntities { get; set; }

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

            // Configuracion para AuditableTestEntity
            modelBuilder.Entity<AuditableTestEntity>(e =>
            {
                e.ToTable("AuditableTestEntities", "test");
                e.HasKey(x => x.Id);
            });
        }
    }
} 