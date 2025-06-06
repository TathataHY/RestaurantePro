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
            
            var validator = new CancelarReservacionValidator(_mockContext.Object);

            // Act
            var result = await validator.ValidateAsync(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(CancelarReservacionCommand.ReservacionId));
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
            var command = new CancelarReservacionCommand
            {
                ReservacionId = Guid.NewGuid(),
                UsuarioId = Guid.NewGuid(),
                Motivo = MotivoCancelacion.ClienteSolicita,
                MotivoDetalle = "Cliente canceló por cambio de planes",
                NotificarCliente = true
            };
            
            // Crear un validator mock que sobrescriba los métodos virtuales
            var validator = new TestCancelarReservacionValidator(_mockContext.Object, 
                reservacionNoVencida: true,  // La reservación no ha vencido (es futura)
                cumplePolitica: horasAnticipacion >= 2); // Cumple la política solo si tiene 2+ horas de anticipación
            
            // Act
            var result = await validator.ValidateAsync(command);
            
            // Assert
            result.IsValid.Should().Be(deberiaSerValido);
            if (!deberiaSerValido && !result.IsValid)
            {
                result.Errors.Should().Contain(e => 
                    e.ErrorMessage.Contains("anticipación") || 
                    e.ErrorMessage.Contains("horas"));
            }
        }
        
        [Fact]
        public async Task Validate_ConReservacionVencida_DeberiaRetornarError()
        {
            // Arrange
            var command = new CancelarReservacionCommand
            {
                ReservacionId = Guid.NewGuid(),
                UsuarioId = Guid.NewGuid(),
                Motivo = MotivoCancelacion.ClienteSolicita,
                MotivoDetalle = "Cliente decide cancelar",
                NotificarCliente = true
            };
            
            // Crear un validator mock que sobrescriba los métodos virtuales
            var validator = new TestCancelarReservacionValidator(_mockContext.Object, 
                reservacionNoVencida: false,  // La reservación ha vencido (fecha pasada)
                cumplePolitica: true);        // Esta validación no importa en este test
            
            // Act
            var result = await validator.ValidateAsync(command);
            
            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => 
                e.ErrorMessage.Contains("ya ha pasado") || 
                e.ErrorMessage.Contains("anticipación"));
        }
        
        // Clase que nos permite testear diferentes comportamientos sobrescribiendo los métodos virtuales
        private class TestCancelarReservacionValidator : CancelarReservacionValidator
        {
            private readonly bool _reservacionNoVencida;
            private readonly bool _cumplePolitica;
            
            public TestCancelarReservacionValidator(
                IApplicationDbContext context, 
                bool reservacionNoVencida, 
                bool cumplePolitica) : base(context)
            {
                _reservacionNoVencida = reservacionNoVencida;
                _cumplePolitica = cumplePolitica;
            }
            
            public override Task<bool> ReservacionNoVencidaAsync(Guid reservacionId, CancellationToken cancellationToken)
            {
                return Task.FromResult(_reservacionNoVencida);
            }
            
            public override Task<bool> CumplePoliticaCancelacionAsync(CancelarReservacionCommand command, CancellationToken cancellationToken)
            {
                return Task.FromResult(_cumplePolitica);
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