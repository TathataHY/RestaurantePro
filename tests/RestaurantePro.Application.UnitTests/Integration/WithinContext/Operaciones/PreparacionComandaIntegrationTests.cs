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
using RestaurantePro.Domain.Core.Productos.Services;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using RestaurantePro.Domain.Inventario.Services;
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
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<IRecetaService> _recetaServiceMock;
        private readonly Mock<IInventarioServiceFacade> _inventarioServiceMock;
        private readonly Mock<IApplicationDbContext> _dbContextMock;
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
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _recetaServiceMock = new Mock<IRecetaService>();
            _inventarioServiceMock = new Mock<IInventarioServiceFacade>();
            _dbContextMock = new Mock<IApplicationDbContext>();
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
            var meseroId = Guid.NewGuid();

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
                meseroId,
                "Observaciones de prueba");
            comanda.SetIdForTesting(comandaId);

            // Configuración de mocks
            _preparacionRepositoryMock.Setup(r => r.AgregarAsync(It.IsAny<PreparacionDiaria>()))
                .Returns(Task.CompletedTask);
            
            _preparacionRepositoryMock.Setup(r => r.GuardarCambiosAsync())
                .ReturnsAsync(1);
            
            _preparacionRepositoryMock.Setup(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == preparacionId)))
                .ReturnsAsync(preparacionDiaria);
                
            // Setup secuencial para devolver primero null (no existe) y luego la preparación creada
            var setupSequence = _preparacionRepositoryMock
                .SetupSequence(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == preparacionId)));
            setupSequence.ReturnsAsync((PreparacionDiaria)null);
            setupSequence.ReturnsAsync(preparacionDiaria);
            
            _comandaRepositoryMock.Setup(r => r.AgregarAsync(It.IsAny<Comanda>()))
                .Returns(Task.CompletedTask);
                
            _comandaRepositoryMock.Setup(r => r.GuardarCambiosAsync())
                .ReturnsAsync(1);
                
            var setupComandaSequence = _comandaRepositoryMock
                .SetupSequence(r => r.ObtenerPorIdAsync(It.Is<Guid>(id => id == comandaId), It.IsAny<bool>()));
            setupComandaSequence.ReturnsAsync((Comanda)null);
            setupComandaSequence.ReturnsAsync(comanda);

            // Setup para servicioPreparaciones - usar valores concretos en lugar de argumentos opcionales
            _servicioPreparacionesMock.Setup(x => x.PrepararProductoAsync(
                It.Is<Guid>(id => id == productoId),
                It.Is<int>(c => c == cantidad),
                It.Is<Guid>(id => id == chefId),
                It.Is<DateTime>(d => d > DateTime.Now),
                It.Is<string>(s => s == "")))
                .ReturnsAsync(Result<PreparacionDiaria>.Success(preparacionDiaria));

            _servicioPreparacionesMock.Setup(x => x.MarcarComoDisponibleAsync(
                It.Is<Guid>(id => id == preparacionId)))
                .ReturnsAsync(Result.Success());

            _servicioPreparacionesMock.Setup(x => x.VerificarDisponibilidadAsync(
                It.Is<Guid>(id => id == productoId), 
                It.Is<int>(c => c == 1)))
                .ReturnsAsync(Result.Success(true));

            _servicioPreparacionesMock.Setup(x => x.ConsumirPreparacionAsync(
                It.Is<Guid>(id => id == productoId), 
                It.Is<int>(c => c == 1)))
                .ReturnsAsync(Result.Success());

            // Configuración de fecha y hora
            _dateTimeServiceMock.Setup(d => d.Now).Returns(DateTime.Now);
            
            // Configuración de mapper
            _mapperMock
                .Setup(x => x.Map<ComandaDto>(It.IsAny<Comanda>()))
                .Returns(new ComandaDto { Id = comandaId });
            
            // Crear handlers
            var crearPreparacionHandler = new CrearPreparacionCommandHandler(
                _dbContextMock.Object,
                _mapperMock.Object,
                _dateTimeServiceMock.Object);
                
            var marcarDisponibleHandler = new MarcarComoDisponibleCommandHandler(
                _servicioPreparacionesMock.Object,
                _mapperMock.Object,
                _loggerMarcarDisponibleMock.Object);
                
            var agregarItemHandler = new AgregarItemComandaHandler(
                _comandaRepositoryMock.Object,
                _mapperMock.Object,
                _loggerAgregarItemMock.Object,
                _servicioPreparacionesMock.Object);

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
            Assert.NotNull(crearResult);
            Assert.True(marcarResult.Succeeded);
            Assert.True(agregarItemResult.Succeeded);
            
            // Verificar que se realizaron las llamadas necesarias
            _preparacionRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<PreparacionDiaria>()), Times.Once);
            _preparacionRepositoryMock.Verify(r => r.GuardarCambiosAsync(), Times.AtLeastOnce);
            _preparacionRepositoryMock.Verify(r => r.ObtenerPorIdAsync(preparacionId), Times.AtLeastOnce);
            
            // Verificar que se marcó la preparación como disponible
            _servicioPreparacionesMock.Verify(
                s => s.MarcarComoDisponibleAsync(
                    It.Is<Guid>(id => id == preparacionId)), 
                Times.Once);
                
            // Verificar que se verificó la disponibilidad del producto
            _servicioPreparacionesMock.Verify(
                s => s.VerificarDisponibilidadAsync(
                    It.Is<Guid>(id => id == productoId), 
                    It.Is<int>(c => c == 1)), 
                Times.Once);
                
            // Verificar que se consumió la preparación
            _servicioPreparacionesMock.Verify(
                s => s.ConsumirPreparacionAsync(
                    It.Is<Guid>(id => id == productoId),
                    It.Is<int>(c => c == 1)),
                Times.Once);
        }
    }
} 