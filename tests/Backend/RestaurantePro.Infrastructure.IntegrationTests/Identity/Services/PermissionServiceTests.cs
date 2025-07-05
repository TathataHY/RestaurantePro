using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using FluentAssertions;
using System;
using RestaurantePro.Infrastructure.Identity.Services;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Infrastructure.IntegrationTests.Identity.Services
{
    public class PermissionServiceTests : IntegrationTestBase
    {
        private IUserPermissionService _permissionService;
        private PermissionService _permissionServiceConcrete;

        public PermissionServiceTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _permissionService = ServiceProvider.GetRequiredService<IUserPermissionService>();
            _permissionServiceConcrete = (_permissionService as PermissionService)!;
        }

        [Fact]
        public async Task CreateRoleAsync_DebeCrearYObtenerRolConPermisosCorrectamente()
        {
            // Arrange
            var roleName = "TestRole_" + Guid.NewGuid();
            var permissions = new List<string> { "Permiso.Test.Crear", "Permiso.Test.Leer" };

            // Act
            var createResult = await _permissionServiceConcrete.CreateRoleAsync(roleName, permissions);
            
            // Assert - Creación
            createResult.Succeeded.Should().BeTrue();
            createResult.Value.Should().NotBeNullOrEmpty();
            var roleId = createResult.Value;

            // Act - Obtención
            var getResult = await _permissionServiceConcrete.GetRoleByIdAsync(roleId);

            // Assert - Obtención
            getResult.Succeeded.Should().BeTrue();
            var roleDto = getResult.Value;
            roleDto.Should().NotBeNull();
            roleDto.Name.Should().Be(roleName);
            roleDto.Permissions.Should().BeEquivalentTo(permissions);
        }

        [Fact]
        public async Task UpdateRoleAsync_DeleteRoleAsync_DebeActualizarYEliminarRolCorrectamente()
        {
            // Arrange - Crear un rol inicial
            var initialRoleName = "Rol A Actualizar " + Guid.NewGuid();
            var initialPermissions = new List<string> { "Permiso.Ver.Todo" };
            var createResult = await _permissionServiceConcrete.CreateRoleAsync(initialRoleName, initialPermissions);
            var roleId = createResult.Value;

            // Act - Actualizar el rol
            var updatedRoleName = "Rol Actualizado " + Guid.NewGuid();
            var updatedPermissions = new List<string> { "Permiso.Editar.Todo", "Permiso.Ver.Usuarios" };
            var updateResult = await _permissionServiceConcrete.UpdateRoleAsync(roleId, updatedRoleName, updatedPermissions);

            // Assert - Verificación de la actualización
            updateResult.Succeeded.Should().BeTrue();
            var getUpdatedResult = await _permissionServiceConcrete.GetRoleByIdAsync(roleId);
            getUpdatedResult.Succeeded.Should().BeTrue();
            var updatedDto = getUpdatedResult.Value;
            updatedDto.Name.Should().Be(updatedRoleName);
            updatedDto.Permissions.Should().BeEquivalentTo(updatedPermissions);

            // Act - Eliminar el rol
            var deleteResult = await _permissionServiceConcrete.DeleteRoleAsync(roleId);

            // Assert - Verificación de la eliminación
            deleteResult.Succeeded.Should().BeTrue();
            var getDeletedResult = await _permissionServiceConcrete.GetRoleByIdAsync(roleId);
            getDeletedResult.Succeeded.Should().BeFalse();
        }
    }
} 