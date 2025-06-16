using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Notificaciones.Entities;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;
using System;
using System.Threading.Tasks;
using Xunit;

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
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<Receta> Recetas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<TarjetaFidelizacion> TarjetasFidelizacion { get; set; }
        public DbSet<Factura> Facturas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<RestaurantePro.Domain.Core.Base.Events.DomainEvent>();
            modelBuilder.Ignore<PersonalizacionItem>();
            modelBuilder.Ignore<TotalComanda>();

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

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreUsuario).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Ignore(e => e.DomainEvents);
                entity.Ignore(e => e.Roles);
            });

            modelBuilder.Entity<Notificacion>(entity =>
            {
                entity.ToTable("Notificaciones", "Test");
                entity.HasKey(e => e.Id);
                entity.Ignore(e => e.DomainEvents);
            });

            modelBuilder.Entity<Receta>(entity =>
            {
                entity.ToTable("Recetas", "Core");
                entity.HasKey(e => e.Id);
                entity.OwnsMany(e => e.Ingredientes, ownedNavigationBuilder =>
                {
                    ownedNavigationBuilder.ToJson();
                });
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes", "Comercial");
                entity.HasKey(e => e.Id);
                entity.OwnsOne(e => e.Nombre);
                entity.OwnsOne(e => e.Email);
                entity.OwnsOne(e => e.Telefono);
            });

            modelBuilder.Entity<TarjetaFidelizacion>(entity =>
            {
                entity.ToTable("TarjetasFidelizacion", "Comercial");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Factura>(entity =>
            {
                entity.ToTable("Facturas", "Comercial");
                entity.HasKey(e => e.Id);
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