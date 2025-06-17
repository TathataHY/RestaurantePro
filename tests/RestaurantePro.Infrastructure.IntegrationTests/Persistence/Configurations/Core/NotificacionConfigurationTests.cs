using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RestaurantePro.Domain.Core.Notificaciones.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System.Linq;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Core
{
    public class NotificacionConfigurationTests : IntegrationTestBase
    {
        public NotificacionConfigurationTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void Notificacion_Configuration_ShouldBeAppliedCorrectly()
        {
            // Arrange
            var entityType = DbContext.Model.FindEntityType(typeof(Notificacion));

            // Assert
            entityType.Should().NotBeNull();
            
            // Table and Schema
            entityType.GetTableName().Should().Be("Notificaciones");
            entityType.GetSchema().Should().Be("Core");

            // Primary Key
            entityType.FindPrimaryKey().Properties.Should().ContainSingle(p => p.Name == "Id");
            entityType.FindProperty("Id").ValueGenerated.Should().Be(ValueGenerated.Never);

            // Properties
            entityType.FindProperty("Titulo").IsNullable.Should().BeFalse();
            entityType.FindProperty("Titulo").GetMaxLength().Should().Be(100);

            entityType.FindProperty("Mensaje").IsNullable.Should().BeFalse();
            entityType.FindProperty("Mensaje").GetMaxLength().Should().Be(500);

            var tipoProperty = entityType.FindProperty("Tipo");
            tipoProperty.Should().NotBeNull();
            tipoProperty.IsNullable.Should().BeFalse();
            tipoProperty.GetMaxLength().Should().Be(50);
            tipoProperty.GetValueConverter().ProviderClrType.Should().Be(typeof(string));

            entityType.FindProperty("DestinatarioId").IsNullable.Should().BeFalse();
            entityType.FindProperty("FechaLectura").IsNullable.Should().BeTrue();
            entityType.FindProperty("EntidadRelacionadaId").IsNullable.Should().BeTrue();

            // Ignored properties
            entityType.FindProperty("EstaLeida").Should().BeNull();
            
            // Relationships
            var destinatarioFk = entityType.GetForeignKeys().SingleOrDefault(fk => fk.Properties.Any(p => p.Name == "DestinatarioId"));
            destinatarioFk.Should().NotBeNull();
            destinatarioFk.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);

            // Indexes
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "DestinatarioId"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "Tipo"));
            
            // Query Filter
            entityType.GetQueryFilter().Should().NotBeNull();
            var queryFilter = entityType.GetQueryFilter();
            var notificacionEliminada = Notificacion.Crear("t", "m", Domain.Core.Notificaciones.Enums.TipoNotificacion.Informativa, System.Guid.NewGuid());
            notificacionEliminada.MarkAsDeleted();
            var notificacionNoEliminada = Notificacion.Crear("t", "m", Domain.Core.Notificaciones.Enums.TipoNotificacion.Informativa, System.Guid.NewGuid());
            
            ((System.Func<Notificacion, bool>)queryFilter.Compile()).Invoke(notificacionEliminada).Should().BeFalse();
            ((System.Func<Notificacion, bool>)queryFilter.Compile()).Invoke(notificacionNoEliminada).Should().BeTrue();
        }
    }
} 