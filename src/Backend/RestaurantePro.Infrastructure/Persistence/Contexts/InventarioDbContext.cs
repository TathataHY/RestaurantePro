using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using System.Reflection;

namespace RestaurantePro.Infrastructure.Persistence.Contexts
{
    public class InventarioDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ILogger<InventarioDbContext> _logger;
        private readonly AuditableEntityInterceptor _auditableEntityInterceptor;
        private readonly DomainEventInterceptor _domainEventInterceptor;
        private readonly SoftDeleteInterceptor _softDeleteInterceptor;

        // DbSets para entidades del contexto Inventario
        public DbSet<Ingrediente> Ingredientes { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<OrdenCompra> OrdenesCompra { get; set; }

        public InventarioDbContext(
            DbContextOptions<InventarioDbContext> options,
            ICurrentUserService currentUserService,
            IDateTimeService dateTimeService,
            IDomainEventDispatcher domainEventDispatcher,
            ILogger<InventarioDbContext> logger,
            AuditableEntityInterceptor auditableEntityInterceptor,
            DomainEventInterceptor domainEventInterceptor,
            SoftDeleteInterceptor softDeleteInterceptor) : base(options)
        {
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
            _domainEventDispatcher = domainEventDispatcher;
            _logger = logger;
            _auditableEntityInterceptor = auditableEntityInterceptor;
            _domainEventInterceptor = domainEventInterceptor;
            _softDeleteInterceptor = softDeleteInterceptor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar configuraciones del contexto Inventario
            modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly(),
                type => type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>) &&
                    type.Namespace?.Contains("Inventario") == true));

            // Configurar esquema para todas las tablas del contexto Inventario
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName());
                entity.SetSchema("Inventario");
            }

            _logger.LogInformation("Modelo de datos del contexto Inventario configurado correctamente");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Registrar interceptores
            optionsBuilder
                .AddInterceptors(_auditableEntityInterceptor)
                .AddInterceptors(_domainEventInterceptor)
                .AddInterceptors(_softDeleteInterceptor);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Guardando cambios en el contexto Inventario");
            
            try
            {
                var result = await base.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Se guardaron {Count} cambios en el contexto Inventario", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios en el contexto Inventario");
                throw;
            }
        }
    }
} 