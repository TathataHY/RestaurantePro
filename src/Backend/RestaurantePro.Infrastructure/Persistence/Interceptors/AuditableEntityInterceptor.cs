using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.Base.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// Interceptor para auditoría automática de entidades
    /// </summary>
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<AuditableEntityInterceptor> _logger;

        public AuditableEntityInterceptor(
            ICurrentUserService currentUserService,
            IDateTimeService dateTimeService,
            ILogger<AuditableEntityInterceptor> logger)
        {
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
            _logger = logger;
        }

        /// <summary>
        /// Se ejecuta antes de guardar cambios
        /// </summary>
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            AplicarAuditoria(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        /// <summary>
        /// Se ejecuta antes de guardar cambios de forma asíncrona
        /// </summary>
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            AplicarAuditoria(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// Actualiza las propiedades de auditoría de las entidades
        /// </summary>
        private void AplicarAuditoria(DbContext? context)
        {
            if (context == null) return;

            // 🌍 Obtener hora de Chile directamente
            var fechaActual = ObtenerHoraChile();
            string? usuarioActual = _currentUserService.UserId;
            
            var cambios = 0;

            foreach (var entry in context.ChangeTracker.Entries<EntityBase>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // La fecha de creación normalmente se establece en el constructor de EntityBase
                        // pero podríamos confirmar que no se ha modificado
                        if (entry.Entity.FechaCreacion == default)
                        {
                            entry.Entity.SetFechaCreacionForTesting(fechaActual);
                            _logger.LogTrace("Estableciendo fecha de creación para entidad {EntityType} con ID {EntityId}", 
                                entry.Entity.GetType().Name, entry.Entity.Id);
                            cambios++;
                        }
                        break;
                    
                    case EntityState.Modified:
                        // La fecha de actualización se maneja generalmente en los métodos de dominio
                        // pero podríamos confirmar que se ha establecido
                        if (entry.Entity.FechaActualizacion == null || entry.Entity.FechaActualizacion < fechaActual)
                        {
                            entry.Property(nameof(EntityBase.FechaActualizacion)).CurrentValue = fechaActual;
                            _logger.LogTrace("Actualizando fecha de modificación para entidad {EntityType} con ID {EntityId}", 
                                entry.Entity.GetType().Name, entry.Entity.Id);
                            cambios++;
                        }
                        break;
                }
            }

            if (cambios > 0)
            {
                _logger.LogInformation("Se aplicó auditoría a {Count} entidades", cambios);
            }
        }

        /// <summary>
        /// Obtiene la hora actual de Chile
        /// </summary>
        private DateTime ObtenerHoraChile()
        {
            TimeZoneInfo chileTimeZone;
            try
            {
                // Intentar obtener la zona horaria de Chile
                chileTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");
            }
            catch
            {
                try
                {
                    // En Windows, el ID puede ser diferente
                    chileTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");
                }
                catch
                {
                    // Fallback: crear manualmente la zona horaria de Chile (UTC-3/-4 según DST)
                    chileTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                        "Chile Standard Time",
                        TimeSpan.FromHours(-3),
                        "Chile Standard Time",
                        "Chile Standard Time");
                }
            }

            var utcNow = DateTime.UtcNow;
            var chileNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, chileTimeZone);
            
            // Log temporal para debug
            _logger.LogInformation("🌍 [AuditableEntityInterceptor] UTC: {UtcTime:yyyy-MM-dd HH:mm:ss}", utcNow);
            _logger.LogInformation("🌍 [AuditableEntityInterceptor] Chile: {ChileTime:yyyy-MM-dd HH:mm:ss}", chileNow);
            _logger.LogInformation("🌍 [AuditableEntityInterceptor] TimeZone: {TimeZone}", chileTimeZone.DisplayName);
            
            return chileNow;
        }
    }

    /// <summary>
    /// Extensiones para el ChangeTracker
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Verifica si una entidad tiene cambios en entidades propias
        /// </summary>
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r => 
                r.TargetEntry != null && 
                r.TargetEntry.Metadata.IsOwned() && 
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
} 