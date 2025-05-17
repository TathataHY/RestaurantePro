using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Inventario.Policies;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Services;
using RestaurantePro.Domain.Core.SharedKernel.Services;

namespace RestaurantePro.Domain.UnitTests.Inventario.Policies
{
    public class StockBajoPolicyTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IServicioNotificaciones> _servicioNotificacionesMock;
        private readonly Mock<IVerificadorStock> _verificadorStockMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly StockBajoPolicy _policy;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
        
        public StockBajoPolicyTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _servicioNotificacionesMock = new Mock<IServicioNotificaciones>();
            _verificadorStockMock = new Mock<IVerificadorStock>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));
            
            _policy = new StockBajoPolicy(
                _ingredienteRepositoryMock.Object,
                _servicioNotificacionesMock.Object,
                _verificadorStockMock.Object,
                _dateTimeServiceMock.Object);
        }
        
        [Fact]
        public async Task EjecutarPolicy_ConIngredientesBajoStock_DebeNotificarYGenerarOrdenes()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngrediente("Tomate", 5, 2),
                CrearIngrediente("Cebolla", 8, 3)
            };
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(ingredientes);
                
            var resultadoVerificacion = new ResultadoVerificacionStock();
            var proveedor = Guid.NewGuid();
            var orden = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                proveedor, 
                "Orden de prueba", 
                DateTime.Now);
            resultadoVerificacion.OrdenesGeneradas.Add(orden);
            
            _verificadorStockMock
                .Setup(v => v.VerificarYGenerarOrdenesCompraAsync())
                .ReturnsAsync(resultadoVerificacion);
                
            // Configuración simple sin árboles de expresión complejos
            var notificacionId = Guid.NewGuid();
            _servicioNotificacionesMock
                .Setup(s => s.NotificarStockBajo(
                    It.IsAny<Guid>(), 
                    It.IsAny<string>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<decimal>()))
                .ReturnsAsync(notificacionId);
            
            // Act
            var resultado = await _policy.EjecutarPolicy();
            
            // Assert
            resultado.Notificaciones.Should().HaveCount(2);
            resultado.OrdenesCompraGeneradas.Should().HaveCount(1);
            
            // Verificación simple usando It.IsAny<T>
            _servicioNotificacionesMock.Verify(
                s => s.NotificarStockBajo(
                    It.IsAny<Guid>(), 
                    It.IsAny<string>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<decimal>()), 
                Times.Exactly(2));
                
            _verificadorStockMock.Verify(
                v => v.VerificarYGenerarOrdenesCompraAsync(),
                Times.Once());
        }
        
        [Fact]
        public async Task EjecutarPolicy_SinIngredientesBajoStock_NoDebeNotificarNiGenerarOrdenes()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>();
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(ingredientes);
            
            // Act
            var resultado = await _policy.EjecutarPolicy();
            
            // Assert
            resultado.Notificaciones.Should().BeEmpty();
            resultado.OrdenesCompraGeneradas.Should().BeEmpty();
            
            // Verificación simple usando It.IsAny<T>
            _servicioNotificacionesMock.Verify(
                s => s.NotificarStockBajo(
                    It.IsAny<Guid>(), 
                    It.IsAny<string>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<decimal>()), 
                Times.Never());
                
            _verificadorStockMock.Verify(
                v => v.VerificarYGenerarOrdenesCompraAsync(),
                Times.Never());
        }
        
        [Fact]
        public async Task EjecutarPolicy_ConIngredientesBajoStockPeroSinGenerarOrdenes_DebeNotificarPeroNoGenerarOrdenes()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngrediente("Tomate", 5, 2),
                CrearIngrediente("Cebolla", 8, 3)
            };
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync())
                .ReturnsAsync(ingredientes);
                
            var resultadoVerificacion = new ResultadoVerificacionStock();
            
            _verificadorStockMock
                .Setup(v => v.VerificarYGenerarOrdenesCompraAsync())
                .ReturnsAsync(resultadoVerificacion);
                
            // Configuración simple sin árboles de expresión complejos
            var notificacionId = Guid.NewGuid();
            _servicioNotificacionesMock
                .Setup(s => s.NotificarStockBajo(
                    It.IsAny<Guid>(), 
                    It.IsAny<string>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<decimal>()))
                .ReturnsAsync(notificacionId);
            
            // Act
            var resultado = await _policy.EjecutarPolicy();
            
            // Assert
            resultado.Notificaciones.Should().HaveCount(2);
            resultado.OrdenesCompraGeneradas.Should().BeEmpty();
            
            // Verificación simple usando It.IsAny<T>
            _servicioNotificacionesMock.Verify(
                s => s.NotificarStockBajo(
                    It.IsAny<Guid>(), 
                    It.IsAny<string>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<decimal>()), 
                Times.Exactly(2));
                
            _verificadorStockMock.Verify(
                v => v.VerificarYGenerarOrdenesCompraAsync(),
                Times.Once());
        }
        
        private Ingrediente CrearIngrediente(string nombre, decimal stockMinimo, decimal stockActual)
        {
            var ingrediente = Ingrediente.Crear(
                nombre,
                "ING-" + Guid.NewGuid().ToString().Substring(0, 5),
                $"Descripción de {nombre}",
                UnidadMedida.Kilogramo,
                stockMinimo,
                stockActual);
                
            // Asignar un proveedor ficticio
            ingrediente.AsociarProveedorPrincipal(Guid.NewGuid());
            
            return ingrediente;
        }
    }
}



