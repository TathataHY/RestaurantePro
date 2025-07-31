using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacion;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Operaciones.Preparaciones.Commands
{
    public class CrearPreparacionCommandTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Fixture _fixture;
        private readonly CrearPreparacionCommandHandler _handler;

        public CrearPreparacionCommandTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _mapperMock = new Mock<IMapper>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _fixture = new Fixture();

            // Configurar el DbSet mock
            var dbSetMock = new Mock<DbSet<PreparacionDiaria>>();
            _contextMock.Setup(c => c.PreparacionesDiarias).Returns(dbSetMock.Object);

            // Configurar el servicio de fecha/hora
            _dateTimeServiceMock.Setup(s => s.Now).Returns(DateTime.UtcNow);

            _handler = new CrearPreparacionCommandHandler(
                _contextMock.Object,
                _mapperMock.Object,
                _dateTimeServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_ConDatosValidos_DebeCrearPreparacion()
        {
            // Arrange
            var command = new CrearPreparacionCommand
            {
                ProductoId = Guid.NewGuid(),
                Cantidad = 10,
                ChefId = Guid.NewGuid(),
                FechaVencimiento = DateTime.UtcNow.AddDays(1), // Usar fecha futura
                Observaciones = "Observaciones de prueba"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            _contextMock.Verify(c => c.PreparacionesDiarias.AddAsync(It.IsAny<PreparacionDiaria>(), It.IsAny<CancellationToken>()), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 