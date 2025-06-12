using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using MockQueryable.Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerEstadisticasPreparaciones;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Operaciones.Preparaciones.Queries
{
    public class ObtenerEstadisticasPreparacionesQueryTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Fixture _fixture;
        private readonly ObtenerEstadisticasPreparacionesQueryHandler _handler;

        public ObtenerEstadisticasPreparacionesQueryTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _fixture = new Fixture();

            _handler = new ObtenerEstadisticasPreparacionesQueryHandler(
                _contextMock.Object,
                _dateTimeServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_DebeCalcularEstadisticasCorrectamente()
        {
            // Arrange
            var query = new ObtenerEstadisticasPreparacionesQuery();
            var cancellationToken = CancellationToken.None;
            var fechaActual = new DateTime(2023, 8, 10, 12, 0, 0);
            
            _dateTimeServiceMock.Setup(x => x.Now).Returns(fechaActual);

            // Preparaciones en diferentes estados
            var preparaciones = new List<PreparacionDiaria>
            {
                // Disponibles (5)
                CrearPreparacion(Guid.NewGuid(), 10, 10, fechaActual.AddDays(1), fechaActual, EstadoPreparacion.Disponible),
                CrearPreparacion(Guid.NewGuid(), 5, 5, fechaActual.AddDays(1), fechaActual, EstadoPreparacion.Disponible),
                CrearPreparacion(Guid.NewGuid(), 8, 8, fechaActual.AddDays(1), fechaActual, EstadoPreparacion.Disponible),
                CrearPreparacion(Guid.NewGuid(), 12, 12, fechaActual.AddDays(1), fechaActual, EstadoPreparacion.Disponible),
                CrearPreparacion(Guid.NewGuid(), 7, 7, fechaActual.AddDays(1), fechaActual, EstadoPreparacion.Disponible),
                
                // Por vencer (2) - fechas cercanas al vencimiento
                CrearPreparacion(Guid.NewGuid(), 6, 6, fechaActual.AddHours(3), fechaActual, EstadoPreparacion.Disponible),
                CrearPreparacion(Guid.NewGuid(), 9, 9, fechaActual.AddHours(4), fechaActual, EstadoPreparacion.Disponible),
                
                // Agotadas (3)
                CrearPreparacion(Guid.NewGuid(), 15, 0, fechaActual.AddDays(1), fechaActual, EstadoPreparacion.Agotada),
                CrearPreparacion(Guid.NewGuid(), 20, 0, fechaActual.AddDays(1), fechaActual, EstadoPreparacion.Agotada),
                CrearPreparacion(Guid.NewGuid(), 10, 0, fechaActual.AddDays(1), fechaActual, EstadoPreparacion.Agotada)
            };

            // Configurar el DbSet mock
            var mockDbSet = preparaciones.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Preparaciones).Returns(mockDbSet.Object);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.TotalPreparaciones);
            Assert.Equal(5, result.PreparacionesDisponibles);
            Assert.Equal(2, result.PreparacionesPorVencer);
            Assert.Equal(3, result.PreparacionesAgotadas);
        }

        private PreparacionDiaria CrearPreparacion(
            Guid productoId, 
            int cantidadPreparada, 
            int cantidadDisponible, 
            DateTime fechaVencimiento,
            DateTime fechaCreacion,
            EstadoPreparacion estado)
        {
            // Crear una instancia privada sin usar el constructor público
            var preparacion = (PreparacionDiaria)Activator.CreateInstance(
                typeof(PreparacionDiaria), 
                true);

            // Establecer las propiedades mediante reflection
            typeof(PreparacionDiaria).GetProperty("Id").SetValue(preparacion, Guid.NewGuid());
            typeof(PreparacionDiaria).GetProperty("ProductoId").SetValue(preparacion, productoId);
            typeof(PreparacionDiaria).GetProperty("CantidadPreparada").SetValue(preparacion, cantidadPreparada);
            typeof(PreparacionDiaria).GetProperty("CantidadDisponible").SetValue(preparacion, cantidadDisponible);
            typeof(PreparacionDiaria).GetProperty("ChefId").SetValue(preparacion, Guid.NewGuid());
            typeof(PreparacionDiaria).GetProperty("FechaVencimiento").SetValue(preparacion, fechaVencimiento);
            typeof(PreparacionDiaria).GetProperty("FechaCreacion").SetValue(preparacion, fechaCreacion);
            typeof(PreparacionDiaria).GetProperty("FechaPreparacion").SetValue(preparacion, fechaCreacion);
            typeof(PreparacionDiaria).GetProperty("Observaciones").SetValue(preparacion, "Observaciones");
            typeof(PreparacionDiaria).GetProperty("Estado").SetValue(preparacion, estado);

            return preparacion;
        }
    }
} 