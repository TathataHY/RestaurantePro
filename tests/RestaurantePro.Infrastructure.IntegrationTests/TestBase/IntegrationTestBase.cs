using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System;
using Xunit;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductoCategoria> Categorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones mínimas para las pruebas
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Productos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired();
                entity.Property(e => e.CategoriaId).IsRequired();
                entity.HasOne<ProductoCategoria>().WithMany().HasForeignKey(e => e.CategoriaId);
                entity.Ignore(e => e.DomainEvents);

                // Configurar PrecioProducto como tipo de propiedad poseída (owned)
                entity.OwnsOne(
                    e => e.Precio,
                    builder =>
                    {
                        builder.Property(p => p.Valor)
                            .HasColumnName("Precio")
                            .IsRequired();
                    });
            });

            modelBuilder.Entity<ProductoCategoria>(entity =>
            {
                entity.ToTable("Categorias");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired();
                entity.Ignore(e => e.DomainEvents);
            });
        }

        public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync()
        {
            return await Database.BeginTransactionAsync();
        }
    }

    // Para manejar la interacción entre TestDbContext y ProductoRepository
    public class TestRepositories
    {
        public static TestDbContext DbContext { get; set; }
    }

    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly TestDbContext DbContext;
        protected readonly ILogger<IntegrationTestBase> Logger;

        protected IntegrationTestBase()
        {
            var services = new ServiceCollection();

            // Registrar servicios para pruebas
            services.AddLogging(builder => builder.AddConsole());
            
            // Usamos un contexto personalizado para pruebas
            services.AddDbContext<TestDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString())
                       .ConfigureWarnings(warnings => warnings.Ignore(
                           Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)));

            // Registrar otros servicios necesarios para las pruebas
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            Logger = ServiceProvider.GetRequiredService<ILogger<IntegrationTestBase>>();
            DbContext = ServiceProvider.GetRequiredService<TestDbContext>();
            
            // Configurar un DbContext estático para poder ser usado por los repositorios
            TestRepositories.DbContext = DbContext;

            // Inicializar la base de datos con datos de prueba
            SeedDatabase();
        }

        // Método para registrar servicios adicionales (a ser implementado por clases derivadas)
        protected virtual void RegisterServices(IServiceCollection services)
        {
        }

        // Método para sembrar datos de prueba (a ser implementado por clases derivadas)
        protected virtual void SeedDatabase()
        {
        }

        // Método para que las clases derivadas configuren servicios adicionales
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            RegisterServices(services);
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
            (ServiceProvider as IDisposable)?.Dispose();
        }
    }
} 