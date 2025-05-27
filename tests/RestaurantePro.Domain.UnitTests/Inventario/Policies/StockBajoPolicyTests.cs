using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RestaurantePro.Domain.Core.SharedKernel.Services.Notification;
using RestaurantePro.Domain.Core.Services;
using RestaurantePro.Domain.Core.SharedKernel;
using RestaurantePro.Domain.Inventario.Ingredientes;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Policies;
using RestaurantePro.Domain.Inventario.Services;
using RestaurantePro.Domain.Inventario.Results;
using Xunit;

namespace RestaurantePro.Domain.UnitTests.Inventario.Policies
{
    public class StockBajoPolicyTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IServicioNotificacionesInventario> _servicioNotificacionesMock;
        private readonly Mock<IVerificadorStock> _verificadorStockMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly NotificationManager _notificationManager;
        private readonly StockBajoPolicy _sut;

        public StockBajoPolicyTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _servicioNotificacionesMock = new Mock<IServicioNotificacionesInventario>();
            _verificadorStockMock = new Mock<IVerificadorStock>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationManager = new NotificationManager();

            // Configuración básica
            _dateTimeServiceMock
                .Setup(s => s.Now)
                .Returns(DateTime.Now);

            _sut = new StockBajoPolicy(
                _ingredienteRepositoryMock.Object,
                _servicioNotificacionesMock.Object,
                _verificadorStockMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManager);
        }

        [Fact]
        public async Task EjecutarPolicy_SinIngredientesConStockBajo_DebeRetornarResultadoVacio()
        {
            // Arrange
            var ingredientesVacios = new List<Ingrediente>();
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientesVacios);

            // Act
            var result = await _sut.EjecutarPolicy();

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.IngredientesPriorizados.Should().BeEmpty();
            result.Value.Notificaciones.Should().BeEmpty();
            result.Value.OrdenesCompraGeneradas.Should().BeEmpty();
        }
    }
}



