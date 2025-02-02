using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Core.Identity;
using RestaurantePro.Core.Entities;
using System.Reflection;

namespace RestaurantePro.Infrastructure.Data
{
    public class RestauranteContext : IdentityDbContext<ApplicationUser>
    {
        public RestauranteContext(DbContextOptions<RestauranteContext> options)
            : base(options)
        {
        }

        public DbSet<Comanda> Comandas { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Plato> Platos { get; set; }
        public DbSet<ComandaDetalle> ComandaDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configuraciones específicas de las entidades
            builder.Entity<ApplicationUser>()
                .Property(u => u.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Entity<ApplicationUser>()
                .Property(u => u.Apellido)
                .IsRequired()
                .HasMaxLength(100);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}