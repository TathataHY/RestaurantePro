using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Core.Identity;
using RestaurantePro.Core.Entities;
using System.Reflection;
using RestaurantePro.Core.Interfaces.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace RestaurantePro.Infrastructure.Data
{
    public class RestauranteContext : IdentityDbContext<ApplicationUser>, IRestauranteContext
    {
        private IDbContextTransaction _currentTransaction;

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

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                await _currentTransaction?.CommitAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _currentTransaction?.RollbackAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }
    }
}