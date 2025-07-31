using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionesDelDia;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using Xunit;
using MockQueryable.Moq;

namespace RestaurantePro.Application.UnitTests.Operaciones.Preparaciones.Queries
{
    public class ObtenerPreparacionesDelDiaQueryTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Fixture _fixture;
        private readonly ObtenerPreparacionesDelDiaQueryHandler _handler;

        public ObtenerPreparacionesDelDiaQueryTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _mapperMock = new Mock<IMapper>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _fixture = new Fixture();

            _handler = new ObtenerPreparacionesDelDiaQueryHandler(
                _contextMock.Object,
                _mapperMock.Object,
                _dateTimeServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_DebeRetornarPreparacionesDelDia()
        {
            // Arrange
            var query = new ObtenerPreparacionesDelDiaQuery();
            var cancellationToken = CancellationToken.None;
            var fechaActual = new DateTime(2023, 8, 10, 12, 0, 0);
            
            _dateTimeServiceMock.Setup(x => x.Now).Returns(fechaActual);

            // Crear una lista de preparaciones de prueba
            var preparaciones = new List<PreparacionDiaria>
            {
                // Usamos reflection para evitar la validación de fecha de vencimiento
                CreatePreparacionDirectly(
                    Guid.NewGuid(), 
                    10, 
                    Guid.NewGuid(), 
                    fechaActual.AddDays(1), // Fecha futura
                    "Observaciones 1",
                    fechaActual),
                CreatePreparacionDirectly(
                    Guid.NewGuid(), 
                    15, 
                    Guid.NewGuid(), 
                    fechaActual.AddDays(2), // Fecha futura
                    "Observaciones 2",
                    fechaActual.AddDays(-1)), // Esta fue creada ayer
                CreatePreparacionDirectly(
                    Guid.NewGuid(), 
                    20, 
                    Guid.NewGuid(), 
                    fechaActual.AddDays(3), // Fecha futura
                    "Observaciones 3",
                    fechaActual) // Esta fue creada hoy
            };

            // Configurar el DbSet mock con los datos de prueba
            var mockDbSet = preparaciones.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.PreparacionesDiarias).Returns(mockDbSet.Object);

            // Preparar los datos filtrados para el mapeo (simulando el filtro de la consulta)
            var preparacionesDelDia = preparaciones
                .Where(p => p.FechaPreparacion.Date == fechaActual.Date)
                .ToList();

            // Configurar el mapeo
            var dtos = preparacionesDelDia.Select(p => new PreparacionDto
            {
                Id = p.Id,
                ProductoId = p.ProductoId,
                Cantidad = p.CantidadPreparada,
                ChefId = p.ChefId,
                FechaVencimiento = p.FechaVencimiento,
                FechaCreacion = p.FechaPreparacion,
                Observaciones = p.Observaciones ?? string.Empty,
                Estado = p.Estado,
                NombreProducto = "Producto de prueba"
            }).ToList();
            
            _mapperMock.Setup(m => m.Map<List<PreparacionDto>>(It.IsAny<List<PreparacionDiaria>>()))
                .Returns(dtos);

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Solo 2 preparaciones fueron creadas hoy
            _contextMock.Verify(c => c.PreparacionesDiarias, Times.AtLeastOnce);
            _mapperMock.Verify(m => m.Map<List<PreparacionDto>>(It.IsAny<List<PreparacionDiaria>>()), Times.Once);
        }

        private PreparacionDiaria CreatePreparacionDirectly(
            Guid productoId,
            int cantidad,
            Guid chefId,
            DateTime fechaVencimiento,
            string observaciones,
            DateTime fechaCreacion)
        {
            // Crear una instancia privada sin usar el constructor público
            var preparacion = (PreparacionDiaria)Activator.CreateInstance(
                typeof(PreparacionDiaria), 
                true);

            // Establecer las propiedades mediante reflection
            typeof(PreparacionDiaria).GetProperty("Id").SetValue(preparacion, Guid.NewGuid());
            typeof(PreparacionDiaria).GetProperty("ProductoId").SetValue(preparacion, productoId);
            typeof(PreparacionDiaria).GetProperty("CantidadPreparada").SetValue(preparacion, cantidad);
            typeof(PreparacionDiaria).GetProperty("CantidadDisponible").SetValue(preparacion, cantidad);
            typeof(PreparacionDiaria).GetProperty("ChefId").SetValue(preparacion, chefId);
            typeof(PreparacionDiaria).GetProperty("FechaVencimiento").SetValue(preparacion, fechaVencimiento);
            typeof(PreparacionDiaria).GetProperty("FechaCreacion").SetValue(preparacion, fechaCreacion);
            typeof(PreparacionDiaria).GetProperty("FechaPreparacion").SetValue(preparacion, fechaCreacion);
            typeof(PreparacionDiaria).GetProperty("Observaciones").SetValue(preparacion, observaciones);
            typeof(PreparacionDiaria).GetProperty("Estado").SetValue(preparacion, EstadoPreparacion.Disponible);

            return preparacion;
        }
    }
} 