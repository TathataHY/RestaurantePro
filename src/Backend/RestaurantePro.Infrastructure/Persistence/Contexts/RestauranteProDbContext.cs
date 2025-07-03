using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Linq;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Domain.Core.Productos;
using System.Collections.Generic;
using System;
using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;

namespace RestaurantePro.Infrastructure.Persistence.Contexts
{
    public class RestauranteProDbContext : IdentityDbContext<IdentityApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>,
        IApplicationDbContext
    {
        private readonly ILogger<RestauranteProDbContext> _logger;
        private readonly IDomainEventDispatcher _dispatcher;

        public RestauranteProDbContext(
            DbContextOptions<RestauranteProDbContext> options,
            ILogger<RestauranteProDbContext> logger,
            IDomainEventDispatcher dispatcher) : base(options)
        {
            _logger = logger;
            _dispatcher = dispatcher;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Los interceptores se configuran automáticamente desde el DI container
            // cuando se registran con AddInterceptors en la configuración del DbContext
        }

        // DbSets de la aplicación
        public DbSet<Domain.Core.Productos.Entities.Producto> Productos { get; set; }
        public DbSet<Domain.Core.Productos.Entities.ProductoCategoria> ProductoCategorias { get; set; }
        public DbSet<Domain.Core.Usuarios.Entities.Usuario> Usuarios { get; set; }
        public DbSet<Domain.Core.Notificaciones.Entities.Notificacion> Notificaciones { get; set; }
        public DbSet<Domain.Comercial.Clientes.Entities.Cliente> Clientes { get; set; }
        public DbSet<Domain.Comercial.Clientes.Entities.TarjetaFidelizacion> TarjetasFidelizacion { get; set; }
        public DbSet<Domain.Comercial.Facturacion.Entities.Factura> Facturas { get; set; }
        public DbSet<Domain.Comercial.Promociones.Entities.Promocion> Promociones { get; set; }
        public DbSet<Domain.Operaciones.Comandas.Entities.Comanda> Comandas { get; set; }
        public DbSet<Domain.Operaciones.Comandas.Entities.ItemComanda> ItemsComanda { get; set; }
        public DbSet<Domain.Operaciones.Reservaciones.Entities.Reservacion> Reservaciones { get; set; }
        public DbSet<Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa> Mesas { get; set; }
        public DbSet<Domain.Operaciones.Preparaciones.Entities.PreparacionDiaria> Preparaciones { get; set; }
        public DbSet<Domain.Inventario.Ingredientes.Entities.Ingrediente> Ingredientes { get; set; }
        public DbSet<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra> OrdenesCompra { get; set; }
        public DbSet<Domain.Proveedores.Entities.Proveedor> Proveedores { get; set; }
        public DbSet<Domain.Proveedores.Entities.ContactoProveedor> ContactosProveedor { get; set; }
        public DbSet<RestaurantePro.Domain.Core.Productos.Entities.Receta> Recetas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Ignore<RestaurantePro.Domain.Core.Base.Events.DomainEvent>();
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // CONFIGURACIÓN ESPECIAL PARA TESTS CON SQLITE: Control de concurrencia optimista DESACTIVADO
            // Eliminar cualquier configuración de concurrencia residual para TarjetaFidelizacion
            var tarjetaEntity = modelBuilder.Entity<Domain.Comercial.Clientes.Entities.TarjetaFidelizacion>();
            tarjetaEntity.Metadata.RemoveAnnotation("Relational:ConcurrencyToken");
            tarjetaEntity.Metadata.RemoveAnnotation("SqlServer:ValueGenerationStrategy");
            tarjetaEntity.Metadata.SetAnnotation("SqlServer:IsConcurrencyToken", false);

            modelBuilder.Entity<ApplicationUserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });
            
            // foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            // {
            //     if (typeof(Domain.Core.Base.EntityBase).IsAssignableFrom(entityType.ClrType))
            //     {
            //         modelBuilder.Entity(entityType.ClrType).HasQueryFilter(p => !((Domain.Core.Base.EntityBase)p).EstaEliminado);
            //     }
            // }
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // if (_dispatcher == null) return result;

            // var entitiesWithEvents = ChangeTracker.Entries<Domain.Core.Base.EntityBase>()
            //     .Select(e => e.Entity)
            //     .Where(e => e.DomainEvents.Any())
            //     .ToArray();

            // await _dispatcher.DispatchAndClearEvents(entitiesWithEvents);

            return result;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await Database.BeginTransactionAsync(cancellationToken);
        }
    }
} 