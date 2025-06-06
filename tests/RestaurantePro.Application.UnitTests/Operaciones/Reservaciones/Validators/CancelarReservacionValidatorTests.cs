using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.CancelarReservacion;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Validators
{
    public class CancelarReservacionValidatorTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;

        public CancelarReservacionValidatorTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
        }

        // Helper para crear una reservación válida para testing
        private Reservacion CrearReservacionParaTest(Guid id, EstadoReservacion estado, DateTime fechaReservacion)
        {
            // Utilizamos el método de fábrica estático Crear en lugar de crear directamente
            var reservacion = Reservacion.Crear(
                mesaId: Guid.NewGuid(),
                clienteId: Guid.NewGuid(),
                fecha: fechaReservacion,
                hora: new TimeSpan(20, 0, 0),
                cantidadPersonas: 4,
                telefono: "612345678",
                email: "cliente@example.com",
                observaciones: "Observación de prueba"
            );
            
            // Establecemos el ID específico para la prueba
            reservacion.SetIdForTesting(id);
            
            // Establecemos el estado correcto según la prueba
            switch (estado)
            {
                case EstadoReservacion.Confirmada:
                    reservacion.Confirmar();
                    break;
                case EstadoReservacion.Cancelada:
                    reservacion.Confirmar(); // Primero confirmar
                    reservacion.Cancelar("Motivo de prueba");
                    break;
                case EstadoReservacion.Completada:
                    reservacion.Confirmar(); // Primero confirmar
                    reservacion.Completar();
                    break;
                // Para Pendiente no hacemos nada, ya viene así por defecto
            }
            
            return reservacion;
        }

        [Fact]
        public async Task Validate_ConReservacionIdVacia_DeberiaRetornarError()
        {
            // Arrange
            var command = new CancelarReservacionCommand
            {
                ReservacionId = Guid.Empty,
                UsuarioId = Guid.NewGuid(),
                Motivo = MotivoCancelacion.ClienteSolicita,
                MotivoDetalle = "Motivo válido",
                NotificarCliente = true
            };
            
            var mockSet = MockDbSet(new List<Reservacion>());
            _mockContext.Setup(c => c.Reservaciones).Returns(mockSet.Object);
            
            var validator = new CancelarReservacionValidator(_mockContext.Object);
            
            // Act
            var result = await validator.ValidateAsync(command);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "El ID de la reservación es obligatorio");
        }

        [Theory]
        [InlineData(0.5, false)]   // 30 minutos (menos del mínimo requerido)
        [InlineData(1, false)]     // 1 hora (menos del mínimo requerido)
        [InlineData(1.9, false)]   // 1 hora 54 minutos (menos del mínimo requerido)
        [InlineData(2, true)]      // 2 horas exactas (mínimo requerido)
        [InlineData(3, true)]      // 3 horas (más del mínimo requerido)
        [InlineData(24, true)]     // 1 día (más del mínimo requerido)
        public async Task Validate_ConDiferentesHorasDeAnticipacion_DeberiaValidarCorrectamente(double horasAnticipacion, bool deberiaSerValido)
        {
            // Arrange
            var reservacionId = Guid.NewGuid();
            var fechaReservacion = DateTime.Now.AddHours(horasAnticipacion);
            
            var reservacion = CrearReservacionParaTest(reservacionId, EstadoReservacion.Confirmada, fechaReservacion);
            
            var reservaciones = new List<Reservacion> { reservacion };
            var mockSet = MockDbSet(reservaciones);
            _mockContext.Setup(c => c.Reservaciones).Returns(mockSet.Object);
            
            var command = new CancelarReservacionCommand
            {
                ReservacionId = reservacionId,
                UsuarioId = Guid.NewGuid(),
                Motivo = MotivoCancelacion.ClienteSolicita,
                MotivoDetalle = "Motivo válido",
                NotificarCliente = true
            };
            
            var validator = new CancelarReservacionValidator(_mockContext.Object);
            
            // Act
            var result = await validator.ValidateAsync(command);
            
            // Assert
            result.IsValid.Should().Be(deberiaSerValido);
            if (!deberiaSerValido)
            {
                result.Errors.Should().Contain(e => e.ErrorMessage.Contains("anticipación mínima"));
            }
        }

        [Theory]
        [InlineData(EstadoReservacion.Pendiente, true)]
        [InlineData(EstadoReservacion.Confirmada, true)]
        [InlineData(EstadoReservacion.Cancelada, false)]
        [InlineData(EstadoReservacion.Completada, false)]
        public async Task Validate_ConDiferentesEstados_DeberiaValidarCorrectamente(EstadoReservacion estado, bool deberiaSerValido)
        {
            // Arrange
            var reservacionId = Guid.NewGuid();
            var fechaReservacion = DateTime.Now.AddHours(24); // Suficiente anticipación
            
            var reservacion = CrearReservacionParaTest(reservacionId, estado, fechaReservacion);
            
            var reservaciones = new List<Reservacion> { reservacion };
            var mockSet = MockDbSet(reservaciones);
            _mockContext.Setup(c => c.Reservaciones).Returns(mockSet.Object);
            
            var command = new CancelarReservacionCommand
            {
                ReservacionId = reservacionId,
                UsuarioId = Guid.NewGuid(),
                Motivo = MotivoCancelacion.ClienteSolicita,
                MotivoDetalle = "Motivo válido",
                NotificarCliente = true
            };
            
            var validator = new CancelarReservacionValidator(_mockContext.Object);
            
            // Act
            var result = await validator.ValidateAsync(command);
            
            // Assert
            result.IsValid.Should().Be(deberiaSerValido);
            if (!deberiaSerValido)
            {
                result.Errors.Should().Contain(e => e.ErrorMessage.Contains("no puede ser cancelada"));
            }
        }
        
        private static Mock<DbSet<T>> MockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();
            
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((object[] ids, CancellationToken token) => data.FirstOrDefault(d => (Guid)typeof(T).GetProperty("Id").GetValue(d) == (Guid)ids[0]));

            // En lugar de intentar hacer mock de LocalView, vamos a configurar el validator
            // para que trabaje con FirstOrDefaultAsync directamente y no use Local
            return mockSet;
        }
    }
} 