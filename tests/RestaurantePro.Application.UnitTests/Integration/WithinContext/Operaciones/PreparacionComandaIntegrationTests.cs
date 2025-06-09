using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarItemComanda;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacion;
using RestaurantePro.Application.Operaciones.Preparaciones.Commands.MarcarComoDisponible;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Integration.WithinContext.Operaciones
{
    /// <summary>
    /// Pruebas de integración para el flujo completo de preparaciones y comandas
    /// </summary>
    public class PreparacionComandaIntegrationTests
    {
        private readonly Mock<IPreparacionRepository> _preparacionRepositoryMock;
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<IServicioPreparaciones> _servicioPreparacionesMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<AgregarItemComandaHandler>> _loggerMock;
        private readonly Fixture _fixture;

        public PreparacionComandaIntegrationTests()
        {
            _preparacionRepositoryMock = new Mock<IPreparacionRepository>();
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _servicioPreparacionesMock = new Mock<IServicioPreparaciones>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<AgregarItemComandaHandler>>();
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Flujo_Completo_Preparacion_Comanda_Deberia_Funcionar()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var chefId = Guid.NewGuid();
            var preparacionId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var usuarioId = Guid.NewGuid(); // Usuario mesero

            // Crear una preparación
            var preparacion = PreparacionDiaria.Crear(
                productoId,
                cantidad,
                chefId,
                DateTime.Now.AddDays(1),
                "Observaciones de prueba",
                DateTime.Now);
            preparacion.SetIdForTesting(preparacionId);

            // Crear una comanda
            var comanda = Comanda.Crear(
                clienteId,
                mesaId,
                usuarioId, // Usuario mesero
                "Observaciones de prueba");
            comanda.SetIdForTesting(comandaId);

            // Configurar el mock del servicio de preparaciones
            _servicioPreparacionesMock
                .Setup(s => s.VerificarDisponibilidadAsync(productoId, It.IsAny<int>()))
                .ReturnsAsync(Result<bool>.Success(true));
            
            _servicioPreparacionesMock
                .Setup(s => s.ConsumirPreparacionAsync(productoId, It.IsAny<int>()))
                .ReturnsAsync(Result.Success());
                
            _servicioPreparacionesMock
                .Setup(s => s.MarcarComoDisponibleAsync(preparacionId))
                .ReturnsAsync(Result.Success());

            // Configurar el mock del repositorio de preparaciones
            _preparacionRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(preparacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(preparacion);

            // Configurar el mock del repositorio de comandas
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);

            // Configurar el mapper para devolver un DTO cuando se mapea la comanda
            var comandaDto = new ComandaDto
            {
                Id = comandaId,
                ClienteId = clienteId,
                MesaId = mesaId,
                UsuarioId = usuarioId,
                Total = 100
            };
            
            _mapperMock
                .Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
                .Returns(comandaDto);

            // Act
            // 1. Marcar la preparación como disponible
            var marcarComoDisponibleCommand = new MarcarComoDisponibleCommand
            {
                PreparacionId = preparacionId,
                Observaciones = "Preparación lista para consumo"
            };
            
            var marcarComoDisponibleHandler = new MarcarComoDisponibleCommandHandler(
                _servicioPreparacionesMock.Object,
                _mapperMock.Object,
                Mock.Of<ILogger<MarcarComoDisponibleCommandHandler>>());
                
            var marcarComoDisponibleResult = await marcarComoDisponibleHandler.Handle(
                marcarComoDisponibleCommand, 
                CancellationToken.None);

            // 2. Agregar el item a la comanda
            var agregarItemCommand = new AgregarItemComandaCommand
            {
                ComandaId = comandaId,
                ProductoId = productoId,
                Cantidad = 2,
                Observaciones = "Test"
            };
            
            var agregarItemHandler = new AgregarItemComandaHandler(
                _comandaRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _servicioPreparacionesMock.Object);
                
            var agregarItemResult = await agregarItemHandler.Handle(
                agregarItemCommand, 
                CancellationToken.None);

            // Assert
            Assert.True(marcarComoDisponibleResult.Succeeded);
            Assert.True(agregarItemResult.Succeeded);
            
            // Verificar que se llamó a los métodos esperados
            _servicioPreparacionesMock.Verify(
                s => s.MarcarComoDisponibleAsync(preparacionId), 
                Times.Once);
                
            _servicioPreparacionesMock.Verify(
                s => s.VerificarDisponibilidadAsync(productoId, It.IsAny<int>()), 
                Times.Once);
                
            _servicioPreparacionesMock.Verify(
                s => s.ConsumirPreparacionAsync(productoId, It.IsAny<int>()), 
                Times.Once);
                
            _comandaRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Comanda>()), 
                Times.Once);
                
            _comandaRepositoryMock.Verify(
                r => r.GuardarCambiosAsync(), 
                Times.Once);
        }
    }
} 