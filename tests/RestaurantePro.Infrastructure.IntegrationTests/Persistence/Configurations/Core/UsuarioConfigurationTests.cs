using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System.Linq;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Core
{
    public class UsuarioConfigurationTests : IntegrationTestBase
    {
        public UsuarioConfigurationTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void Usuario_Configuration_ShouldBeAppliedCorrectly()
        {
            // Arrange
            var entityType = DbContext.Model.FindEntityType(typeof(Usuario));

            // Assert
            entityType.Should().NotBeNull();

            // Table and Schema
            entityType.GetTableName().Should().Be("Usuarios");
            entityType.GetSchema().Should().Be("Core");

            // Primary Key
            entityType.FindPrimaryKey().Properties.Should().ContainSingle(p => p.Name == "Id");
            entityType.FindProperty("Id").ValueGenerated.Should().Be(ValueGenerated.Never);

            // Properties
            entityType.FindProperty("NombreUsuario").IsNullable.Should().BeFalse();
            entityType.FindProperty("NombreUsuario").GetMaxLength().Should().Be(100);

            entityType.FindProperty("NombreCompleto").IsNullable.Should().BeFalse();
            entityType.FindProperty("NombreCompleto").GetMaxLength().Should().Be(100);

            entityType.FindProperty("Email").IsNullable.Should().BeFalse();
            entityType.FindProperty("Email").GetMaxLength().Should().Be(150);

            entityType.FindProperty("Rol").IsNullable.Should().BeFalse();
            entityType.FindProperty("Rol").GetMaxLength().Should().Be(50);

            var estadoProperty = entityType.FindProperty("Estado");
            estadoProperty.Should().NotBeNull();
            estadoProperty.GetValueConverter().ProviderClrType.Should().Be(typeof(string));

            var tipoUsuarioProperty = entityType.FindProperty("TipoUsuario");
            tipoUsuarioProperty.Should().NotBeNull();
            tipoUsuarioProperty.GetValueConverter().ProviderClrType.Should().Be(typeof(string));

            entityType.FindProperty("IdentityId").GetMaxLength().Should().Be(128);
            entityType.FindProperty("PasswordHash").GetMaxLength().Should().Be(256);
            entityType.FindProperty("Salt").GetMaxLength().Should().Be(128);
            entityType.FindProperty("NivelAcceso").IsNullable.Should().BeFalse();
            entityType.FindProperty("Departamento").GetMaxLength().Should().Be(100);
            entityType.FindProperty("Posicion").GetMaxLength().Should().Be(100);
            entityType.FindProperty("Identificacion").GetMaxLength().Should().Be(50);
            entityType.FindProperty("MotivoBloqueo").GetMaxLength().Should().Be(500);

            // Backing fields for collections
            entityType.FindProperty("_roles").Should().NotBeNull();
            entityType.FindProperty("_permisos").Should().NotBeNull();
            
            // Relationships
            var supervisorFk = entityType.GetForeignKeys().SingleOrDefault(fk => fk.Properties.Any(p => p.Name == "SupervisorId"));
            supervisorFk.Should().NotBeNull();
            supervisorFk.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);

            // Indexes
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "Email") && i.IsUnique);
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "NombreUsuario") && i.IsUnique);

            // Query Filter
            entityType.GetQueryFilter().Should().NotBeNull();
            var queryFilter = entityType.GetQueryFilter();
            var usuarioEliminado = Usuario.Crear("test", "test", "test@test.com", RolUsuario.Mesero);
            usuarioEliminado.MarkAsDeleted();
            var usuarioNoEliminado = Usuario.Crear("test2", "test2", "test2@test.com", RolUsuario.Mesero);

            ((System.Func<Usuario, bool>)queryFilter.Compile()).Invoke(usuarioEliminado).Should().BeFalse();
            ((System.Func<Usuario, bool>)queryFilter.Compile()).Invoke(usuarioNoEliminado).Should().BeTrue();
        }
    }
} 