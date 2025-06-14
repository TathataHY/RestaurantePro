using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Notificaciones.Entities;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using System.Reflection;

namespace RestaurantePro.Infrastructure.Persistence.Contexts
{
    public class CoreDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ILogger<CoreDbContext> _logger;
        private readonly AuditableEntityInterceptor _auditableEntityInterceptor;
        private readonly DomainEventInterceptor _domainEventInterceptor;
        private readonly SoftDeleteInterceptor _softDeleteInterceptor;

        // DbSets para entidades del contexto Core
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<Receta> Recetas { get; set; }

        public CoreDbContext(
            DbContextOptions<CoreDbContext> options,
            ICurrentUserService currentUserService,
            IDateTimeService dateTimeService,
            IDomainEventDispatcher domainEventDispatcher,
            ILogger<CoreDbContext> logger,
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

            // Aplicar configuraciones del contexto Core
            modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly(),
                type => type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>) &&
                    type.Namespace?.Contains("Core") == true));

            // Configurar esquema para todas las tablas del contexto Core
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName());
                entity.SetSchema("Core");
            }

            _logger.LogInformation("Modelo de datos del contexto Core configurado correctamente");
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
            _logger.LogInformation("Guardando cambios en el contexto Core");
            
            try
            {
                var result = await base.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Se guardaron {Count} cambios en el contexto Core", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios en el contexto Core");
                throw;
            }
        }
    }
} 