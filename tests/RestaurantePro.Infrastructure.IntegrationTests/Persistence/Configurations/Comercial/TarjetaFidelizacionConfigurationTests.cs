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
    public class TarjetaFidelizacionConfigurationTests
    {
        [Fact]
        public void TarjetaFidelizacionConfiguration_DebeMapearEntidadCorrectamente()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RestauranteProDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var loggerMock = new Mock<ILogger<RestauranteProDbContext>>().Object;

            using var context = new RestauranteProDbContext(options, loggerMock);

            // Act
            var model = context.Model;
            var entityType = model.FindEntityType(typeof(TarjetaFidelizacion));

            // Assert
            entityType.Should().NotBeNull();

            // Verificar nombre de tabla y esquema
            entityType.GetTableName().Should().Be("TarjetasFidelizacion");
            entityType.GetSchema().Should().Be("Comercial");

            // Verificar clave primaria
            entityType.FindPrimaryKey().Properties.Should().Contain(p => p.Name == "Id");
            
            // Verificar relación con HistorialPuntos
            var relacionHistorial = entityType.GetNavigations()
                .FirstOrDefault(n => n.Name == "HistorialPuntos");
            
            relacionHistorial.Should().NotBeNull();
            relacionHistorial.IsCollection.Should().BeTrue();
            relacionHistorial.ForeignKey.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);

            // Verificar índice único en Codigo
            var codigoIndex = entityType.GetIndexes()
                .FirstOrDefault(i => i.Properties.Any(p => p.Name == "Codigo"));
            
            codigoIndex.Should().NotBeNull();
            codigoIndex.IsUnique.Should().BeTrue();
            codigoIndex.GetDatabaseName().Should().Be("IX_TarjetasFidelizacion_Codigo");
        }
    }
} 