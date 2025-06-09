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
        private readonly Mock<ILogger<CrearPreparacionCommandHandler>> _loggerCrearPreparacionMock;
        private readonly Mock<ILogger<MarcarComoDisponibleCommandHandler>> _loggerMarcarDisponibleMock;
        private readonly Mock<ILogger<AgregarItemComandaHandler>> _loggerAgregarItemMock;
        private readonly Fixture _fixture;

        public PreparacionComandaIntegrationTests()
        {
            _fixture = new Fixture();
            _preparacionRepositoryMock = new Mock<IPreparacionRepository>();
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _servicioPreparacionesMock = new Mock<IServicioPreparaciones>();
            _mapperMock = new Mock<IMapper>();
            _loggerCrearPreparacionMock = new Mock<ILogger<CrearPreparacionCommandHandler>>();
            _loggerMarcarDisponibleMock = new Mock<ILogger<MarcarComoDisponibleCommandHandler>>();
            _loggerAgregarItemMock = new Mock<ILogger<AgregarItemComandaHandler>>();
        }

        [Fact]
        public async Task IntegrationTest_CrearPreparacion_MarcarDisponible_AgregarItemComanda_Success()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 10;
            var preparacionId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var chefId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();

            // Preparación diaria para el test
            var preparacionDiaria = PreparacionDiaria.Crear(
                productoId,
                cantidad,
                chefId,
                DateTime.Now.AddDays(1));
            preparacionDiaria.SetIdForTesting(preparacionId);

            // Comanda para el test
            var comanda = Comanda.Crear(
                mesaId,
                clienteId,
                2,
                "Observaciones de prueba");
            comanda.SetIdForTesting(comandaId);

            // Configuración de mocks
            _preparacionRepositoryMock.Setup(r => r.AgregarAsync(It.IsAny<PreparacionDiaria>()))
                .Returns(Task.CompletedTask);
            
            _preparacionRepositoryMock.Setup(r => r.GuardarCambiosAsync())
                .Returns(Task.CompletedTask);
            
            _preparacionRepositoryMock.Setup(r => r.ObtenerPorIdAsync(preparacionId))
                .ReturnsAsync(preparacionDiaria);
                
            // Setup secuencial para devolver primero null (no existe) y luego la preparación creada
            var setupSequence = _preparacionRepositoryMock
                .SetupSequence(r => r.ObtenerPorIdAsync(preparacionId));
            setupSequence.ReturnsAsync(null);
            setupSequence.ReturnsAsync(preparacionDiaria);
            
            _comandaRepositoryMock.Setup(r => r.AgregarAsync(It.IsAny<Comanda>()))
                .Returns(Task.CompletedTask);
                
            _comandaRepositoryMock.Setup(r => r.GuardarCambiosAsync())
                .Returns(Task.CompletedTask);
                
            var setupComandaSequence = _comandaRepositoryMock
                .SetupSequence(r => r.ObtenerPorIdAsync(comandaId));
            setupComandaSequence.ReturnsAsync(null);
            setupComandaSequence.ReturnsAsync(comanda);

            // Creación de handlers
            var dbContextMock = new Mock<IApplicationDbContext>();
            var mapperMock = new Mock<IMapper>();
            var dateTimeServiceMock = new Mock<IDateTimeService>();
            dateTimeServiceMock.Setup(d => d.Now).Returns(DateTime.Now);
            
            var loggerCrearPreparacionMock = new Mock<ILogger<CrearPreparacionCommandHandler>>();
            var loggerMarcarDisponibleMock = new Mock<ILogger<MarcarComoDisponibleCommandHandler>>();
            var loggerAgregarItemMock = new Mock<ILogger<AgregarItemComandaHandler>>();
            
            // Crear servicios de dominio
            var notificationManagerMock = new Mock<INotificationManager>();
            notificationManagerMock.Setup(n => n.HasErrors).Returns(false);
            
            var servicioPreparaciones = new ServicioPreparaciones(
                new Mock<ILogger<ServicioPreparaciones>>().Object,
                notificationManagerMock.Object,
                dateTimeServiceMock.Object,
                _preparacionRepositoryMock.Object);
                
            var servicioComandas = new ServicioComandas(
                new Mock<ILogger<ServicioComandas>>().Object,
                notificationManagerMock.Object,
                _comandaRepositoryMock.Object);

            // Crear handlers
            var crearPreparacionHandler = new CrearPreparacionCommandHandler(
                dbContextMock.Object,
                mapperMock.Object,
                dateTimeServiceMock.Object,
                loggerCrearPreparacionMock.Object);
                
            var marcarDisponibleHandler = new MarcarComoDisponibleCommandHandler(
                servicioPreparaciones,
                mapperMock.Object,
                loggerMarcarDisponibleMock.Object);
                
            var agregarItemHandler = new AgregarItemComandaHandler(
                servicioComandas,
                servicioPreparaciones,
                mapperMock.Object,
                loggerAgregarItemMock.Object);

            // Act
            // 1. Crear la preparación
            var crearCommand = new CrearPreparacionCommand
            {
                ProductoId = productoId,
                Cantidad = cantidad,
                ChefId = chefId,
                FechaVencimiento = DateTime.Now.AddDays(1)
            };
            var crearResult = await crearPreparacionHandler.Handle(crearCommand, CancellationToken.None);

            // 2. Marcar como disponible
            var marcarCommand = new MarcarComoDisponibleCommand
            {
                PreparacionId = preparacionId
            };
            var marcarResult = await marcarDisponibleHandler.Handle(marcarCommand, CancellationToken.None);

            // 3. Agregar item a la comanda
            var agregarItemCommand = new AgregarItemComandaCommand
            {
                ComandaId = comandaId,
                ProductoId = productoId,
                Cantidad = 1,
                NombreProducto = "Producto Test",
                PrecioUnitario = 10.5m,
                Observaciones = "Prueba de integración",
                Personalizaciones = new List<PersonalizacionCreateDto>()
            };
            var agregarItemResult = await agregarItemHandler.Handle(agregarItemCommand, CancellationToken.None);

            // Assert
            Assert.Equal(preparacionId, crearResult);
            Assert.True(marcarResult.Succeeded);
            Assert.True(agregarItemResult.Succeeded);

            // Verificar que los métodos fueron llamados con los parámetros correctos
            _preparacionRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<PreparacionDiaria>()), Times.Once);
            _preparacionRepositoryMock.Verify(r => r.GuardarCambiosAsync(), Times.Once);
            _preparacionRepositoryMock.Verify(r => r.ObtenerPorIdAsync(preparacionId), Times.AtMostOnce);
            _comandaRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Comanda>()), Times.Once);
            _comandaRepositoryMock.Verify(r => r.GuardarCambiosAsync(), Times.Once);
            _comandaRepositoryMock.Verify(r => r.ObtenerPorIdAsync(comandaId), Times.AtMostOnce);
        }
    }
} 