using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Infrastructure.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Ingrediente> Ingredientes { get; set; }
        public DbSet<IngredienteProducto> IngredientesProductos { get; set; }
        public DbSet<ProductoIngrediente> ProductoIngredientes { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Comanda> Comandas { get; set; }
        public DbSet<ComandaDetalle> ComandaDetalles { get; set; }
        public DbSet<ComandaDetallePersonalizacion> ComandaDetallePersonalizaciones { get; set; }
        public DbSet<InventarioMovimiento> InventarioMovimientos { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Reservacion> Reservaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Usuario
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configuraciones para IngredienteProducto
            modelBuilder.Entity<IngredienteProducto>().ToTable("IngredientesProductos");
            modelBuilder.Entity<IngredienteProducto>().HasKey(ip => ip.Id);
            modelBuilder.Entity<IngredienteProducto>()
                .HasOne(ip => ip.Producto)
                .WithMany(p => p.Ingredientes)
                .HasForeignKey(ip => ip.ProductoId);
            modelBuilder.Entity<IngredienteProducto>()
                .HasOne(ip => ip.Ingrediente)
                .WithMany(i => i.Productos)
                .HasForeignKey(ip => ip.IngredienteId);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.FechaCreacion = DateTime.Now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UltimaModificacion = DateTime.Now;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
} 