using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Entities;

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
            DbContextOptions<TestDbContext> options,
            ILogger<RestauranteProDbContext> logger)
            : base(options, logger)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<NonAuditableTestEntity> NonAuditableTestEntities { get; set; }
        public DbSet<ProductoCategoria> ProductoCategorias { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }
        public DbSet<TarjetaFidelizacion> TarjetasFidelizacion { get; set; }
        public DbSet<HistorialPuntos> PuntosHistorial { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<TestEntity>();
            modelBuilder.Entity<NonAuditableTestEntity>();
        }
    }
} 