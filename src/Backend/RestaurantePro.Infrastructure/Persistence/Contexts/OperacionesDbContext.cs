using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using System.Reflection;

namespace RestaurantePro.Infrastructure.Persistence.Contexts
{
    public class OperacionesDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ILogger<OperacionesDbContext> _logger;
        private readonly AuditableEntityInterceptor _auditableEntityInterceptor;
        private readonly DomainEventInterceptor _domainEventInterceptor;
        private readonly SoftDeleteInterceptor _softDeleteInterceptor;

        // DbSets para entidades del contexto Operaciones
        public DbSet<Comanda> Comandas { get; set; }
        public DbSet<ItemComanda> ItemsComandas { get; set; }
        public DbSet<Reservacion> Reservaciones { get; set; }
        public DbSet<PreparacionDiaria> PreparacionesDiarias { get; set; }

        public OperacionesDbContext(
            DbContextOptions<OperacionesDbContext> options,
            ICurrentUserService currentUserService,
            IDateTimeService dateTimeService,
            IDomainEventDispatcher domainEventDispatcher,
            ILogger<OperacionesDbContext> logger,
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

            // Aplicar configuraciones del contexto Operaciones
            modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly(),
                type => type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>) &&
                    type.Namespace?.Contains("Operaciones") == true));

            // Configurar esquema para todas las tablas del contexto Operaciones
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName());
                entity.SetSchema("Operaciones");
            }

            _logger.LogInformation("Modelo de datos del contexto Operaciones configurado correctamente");
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
            _logger.LogInformation("Guardando cambios en el contexto Operaciones");
            
            try
            {
                var result = await base.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Se guardaron {Count} cambios en el contexto Operaciones", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios en el contexto Operaciones");
                throw;
            }
        }
    }
} 