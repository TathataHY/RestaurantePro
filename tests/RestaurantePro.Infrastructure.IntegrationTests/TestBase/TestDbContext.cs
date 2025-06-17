using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public class TestEntity : EntityBase
    {
        public string Nombre { get; set; } = string.Empty;
    }

    public class NonAuditableTestEntity
    {
        public int Id { get; set; }
        public string Valor { get; set; } = string.Empty;
    }

    public class TestDbContext : RestauranteProDbContext
    {
        public TestDbContext(
            DbContextOptions<RestauranteProDbContext> options,
            ILogger<RestauranteProDbContext> logger)
            : base(options, logger)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<NonAuditableTestEntity> NonAuditableTestEntities { get; set; }
        public DbSet<ProductoCategoria> ProductoCategorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<TestEntity>();
            modelBuilder.Entity<NonAuditableTestEntity>();
        }
    }
} 