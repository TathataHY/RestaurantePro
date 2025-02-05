using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using RestaurantePro.Core.Services;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using Moq;
using RestaurantePro.Core.Interfaces;

namespace RestaurantePro.Tests.Services
{
    public class ComandaStateManagerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly ComandaStateManager _sut;

        public ComandaStateManagerTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _sut = new ComandaStateManager(_unitOfWork.Object);
        }

        [Theory]
        [InlineData(EstadoComanda.Pendiente, EstadoComanda.EnPreparacion, true)]
        [InlineData(EstadoComanda.EnPreparacion, EstadoComanda.Lista, true)]
        [InlineData(EstadoComanda.Lista, EstadoComanda.Entregada, true)]
        [InlineData(EstadoComanda.Entregada, EstadoComanda.Cancelada, true)]
        [InlineData(EstadoComanda.Pendiente, EstadoComanda.Cancelada, false)]
        public void IsValidTransition_DebeValidarTransicionesCorrectamente(
            EstadoComanda estadoActual,
            EstadoComanda nuevoEstado,
            bool resultadoEsperado)
        {
            // Act
            var resultado = _sut.IsValidTransition(estadoActual, nuevoEstado);

            // Assert
            resultado.Should().Be(resultadoEsperado);
        }

        [Fact]
        public void GetTransitionError_DebeRetornarMensajeDeErrorCorrecto()
        {
            // Arrange
            var estadoActual = EstadoComanda.Pendiente;
            var nuevoEstado = EstadoComanda.Cancelada;

            // Act
            var mensaje = _sut.GetTransitionError(estadoActual, nuevoEstado);

            // Assert
            mensaje.Should().Contain(estadoActual.ToString());
            mensaje.Should().Contain(nuevoEstado.ToString());
        }
    }
} 