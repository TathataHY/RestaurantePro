using FluentAssertions;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Contexts
{
    public class OperacionesDbContextTests : IntegrationTestBase
    {
        [Fact]
        public async Task OperacionesDbContext_DebeGuardarReservacionCorrectamente()
        {
            // Arrange
            var reservacion = Reservacion.Crear(
                Guid.NewGuid(), // mesaId
                Guid.NewGuid(), // clienteId
                DateTime.Now.AddDays(1),
                TimeSpan.FromHours(2),
                4,
                "123456789",
                "test@test.com",
                "Mesa cerca de la ventana"
            );
            
            // Act
            DbContext.Reservaciones.Add(reservacion);
            await DbContext.SaveChangesAsync();
            
            // Assert
            var reservacionGuardada = await DbContext.Reservaciones.FindAsync(reservacion.Id);
            reservacionGuardada.Should().NotBeNull();
            reservacionGuardada.CantidadPersonas.Should().Be(4);
            reservacionGuardada.Email.Should().Be("test@test.com");
        }
    }
} 