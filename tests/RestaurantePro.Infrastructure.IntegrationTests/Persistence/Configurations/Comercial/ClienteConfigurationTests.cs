using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System;
using System.Linq;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Comercial
{
    public class ClienteConfigurationTests
    {
        [Fact]
        public void ClienteConfiguration_DebeMapearEntidadCorrectamente()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RestauranteProDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
                
            var loggerMock = new Mock<ILogger<RestauranteProDbContext>>().Object;

            using var context = new RestauranteProDbContext(options, loggerMock);
            
            // Act
            var model = context.Model;
            var entityType = model.FindEntityType(typeof(Cliente));

            // Assert
            entityType.Should().NotBeNull();

            // Verificar nombre de tabla y esquema
            entityType.GetTableName().Should().Be("Clientes");
            entityType.GetSchema().Should().Be("Comercial");

            // Verificar clave primaria
            entityType.FindPrimaryKey().Properties.Should().Contain(p => p.Name == "Id");

            // Verificar owned types (Value Objects)
            var nombre = entityType.FindNavigation("Nombre");
            nombre.Should().NotBeNull();
            nombre.TargetEntityType.IsOwned().Should().BeTrue();
            nombre.TargetEntityType.FindProperty("Nombre").GetMaxLength().Should().Be(50);
            nombre.TargetEntityType.FindProperty("Apellido").GetMaxLength().Should().Be(50);

            var email = entityType.FindNavigation("Email");
            email.Should().NotBeNull();
            email.TargetEntityType.IsOwned().Should().BeTrue();
            email.TargetEntityType.FindProperty("Value").GetMaxLength().Should().Be(150);
            email.TargetEntityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "Value") && i.IsUnique);

            // Verificar índice de nombre completo
            var nombreCompletoIndex = nombre.TargetEntityType.GetIndexes()
                                       .FirstOrDefault(i => i.Properties.Any(p => p.Name == "Nombre") && 
                                                              i.Properties.Any(p => p.Name == "Apellido"));
            nombreCompletoIndex.Should().NotBeNull();
            nombreCompletoIndex.GetDatabaseName().Should().Be("IX_Clientes_NombreCompleto");
        }
    }
} 