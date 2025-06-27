using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using System.Reflection;

namespace RestaurantePro.Infrastructure.Persistence.Contexts
{
    public class ComercialDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ILogger<ComercialDbContext> _logger;
        private readonly AuditableEntityInterceptor _auditableEntityInterceptor;
        private readonly DomainEventInterceptor _domainEventInterceptor;
        private readonly SoftDeleteInterceptor _softDeleteInterceptor;

        // DbSets para entidades del contexto Comercial
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFacturas { get; set; }

        public ComercialDbContext(
            DbContextOptions<ComercialDbContext> options,
            ICurrentUserService currentUserService,
            IDateTimeService dateTimeService,
            IDomainEventDispatcher domainEventDispatcher,
            ILogger<ComercialDbContext> logger,
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

            // Aplicar configuraciones del contexto Comercial
            modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly(),
                type => type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>) &&
                    type.Namespace?.Contains("Comercial") == true));

            // Configurar esquema para todas las tablas del contexto Comercial
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName());
                entity.SetSchema("Comercial");
            }

            modelBuilder.Ignore<RestaurantePro.Domain.Core.Base.Events.DomainEvent>();
            modelBuilder.Ignore<RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.PersonalizacionItem>();
            modelBuilder.Ignore<RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.TotalComanda>();

            _logger.LogInformation("Modelo de datos del contexto Comercial configurado correctamente");
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
            _logger.LogInformation("Guardando cambios en el contexto Comercial");
            
            try
            {
                var result = await base.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Se guardaron {Count} cambios en el contexto Comercial", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios en el contexto Comercial");
                throw;
            }
        }
    }
} 