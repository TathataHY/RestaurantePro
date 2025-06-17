using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Interceptors
{
    // Entidad de prueba para los tests de interceptores
    public class AuditableTestEntity : EntityBase
    {
        public string Nombre { get; private set; }

        // Exponer la propiedad Activo para poder verificarla en los tests
        public bool IsActive => !EstaEliminado;

        private AuditableTestEntity(string nombre)
        {
            Nombre = nombre;
        }

        public static AuditableTestEntity Crear(string nombre)
        {
            return new AuditableTestEntity(nombre);
        }

        public void ActualizarNombre(string nuevoNombre)
        {
            Nombre = nuevoNombre;
            MarkAsModified();
        }

        public void RegistrarEventoPrueba()
        {
            AddDomainEvent(new TestDomainEvent());
        }
    }

    // DbContext de prueba extendido
    public class TestDbContext : RestauranteProDbContext
    {
        public TestDbContext(DbContextOptions<RestauranteProDbContext> options, ILogger<RestauranteProDbContext> logger)
            : base(options, logger)
        {
        }

        public DbSet<AuditableTestEntity> TestEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AuditableTestEntity>().ToTable("TestEntities");
        }
    }

    // Extensión para acceder a propiedades protegidas en tests
    public static class EntityBaseExtensions
    {
        public static void SetFechaCreacionForTesting(this EntityBase entity, DateTime date)
        {
            var propertyInfo = typeof(EntityBase).GetProperty(nameof(EntityBase.FechaCreacion));
            if (propertyInfo != null)
            {
                var setter = propertyInfo.GetSetMethod(nonPublic: true);
                if (setter != null)
                {
                    setter.Invoke(entity, new object[] { date });
                }
            }
        }
    }
} 