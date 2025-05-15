using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Core.Base.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Mesas.Entities;

namespace RestaurantePro.Infrastructure.Persistence.Contexts
{
    public class RestauranteProDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public RestauranteProDbContext(
            DbContextOptions<RestauranteProDbContext> options) : base(options)
        {
        }

        public RestauranteProDbContext(
            DbContextOptions<RestauranteProDbContext> options,
            ICurrentUserService currentUserService,
            IDateTime dateTime) : base(options)
        {
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        // Comercial
        public DbSet<Cliente> Clientes { get; set; }

        // Operaciones
        public DbSet<Comanda> Comandas { get; set; }
        public DbSet<DetalleComanda> DetallesComanda { get; set; }
        public DbSet<Mesa> Mesas { get; set; }

        // Aquí agregar los demás DbSets

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar todas las configuraciones desde el assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Si tenemos el servicio de usuario actual, aplicamos auditoría
            if (_currentUserService != null)
            {
                foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Entity.CreadoPor = _currentUserService.UserId;
                            entry.Entity.FechaCreacion = _dateTime?.Now ?? DateTime.Now;
                            break;
                        case EntityState.Modified:
                            entry.Entity.ModificadoPor = _currentUserService.UserId;
                            entry.Entity.FechaModificacion = _dateTime?.Now ?? DateTime.Now;
                            break;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
} 