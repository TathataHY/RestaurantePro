using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System;
using System.Linq;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Comercial
{
    public class FacturaConfigurationTests
    {
        [Fact]
        public void FacturaConfiguration_DebeMapearEntidadCorrectamente()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RestauranteProDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var loggerMock = new Mock<ILogger<RestauranteProDbContext>>().Object;

            using var context = new RestauranteProDbContext(options, loggerMock);

            // Act
            var model = context.Model;
            var entityType = model.FindEntityType(typeof(Factura));

            // Assert
            entityType.Should().NotBeNull();

            // Verificar nombre de tabla y esquema
            entityType.GetTableName().Should().Be("Facturas");
            entityType.GetSchema().Should().Be("Comercial");

            // Verificar clave primaria
            entityType.FindPrimaryKey().Properties.Should().Contain(p => p.Name == "Id");

            // Verificar la relación con DetalleFactura
            var relacionDetalles = entityType.GetNavigations()
                .FirstOrDefault(n => n.Name == "Detalles");

            relacionDetalles.Should().NotBeNull();
            relacionDetalles.IsCollection.Should().BeTrue();
            relacionDetalles.ForeignKey.PrincipalKey.DeclaringEntityType.ClrType.Should().Be(typeof(Factura));
            relacionDetalles.ForeignKey.Properties.Should().Contain(p => p.Name == "FacturaId");
            relacionDetalles.ForeignKey.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);

            // Verificar índice único en NumeroFactura
            var numeroFacturaIndex = entityType.GetIndexes()
                .FirstOrDefault(i => i.Properties.Any(p => p.Name == "NumeroFactura"));
            
            numeroFacturaIndex.Should().NotBeNull();
            numeroFacturaIndex.IsUnique.Should().BeTrue();
            numeroFacturaIndex.GetDatabaseName().Should().Be("IX_Facturas_NumeroFactura");

            // Verificar precisión de las propiedades decimales
            entityType.FindProperty("Total").GetPrecision().Should().Be(18);
            entityType.FindProperty("Total").GetScale().Should().Be(2);
        }
    }
} 