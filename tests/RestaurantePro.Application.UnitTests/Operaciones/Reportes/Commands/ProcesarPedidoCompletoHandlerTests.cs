using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using RestaurantePro.Application.Comercial.Facturacion.Interfaces;
using RestaurantePro.Application.Comercial.Fidelizacion.Interfaces;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using RestaurantePro.Application.Operaciones.Mesas.Interfaces;
using RestaurantePro.Application.Operaciones.Reportes.DTOs;
using RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto;

namespace RestaurantePro.Application.UnitTests.Operaciones.Reportes.Commands
{
    /// <summary>
    /// Tests unitarios para ProcesarPedidoCompletoHandler
    /// Valida la orquestación completa de workflows: Comanda → Pago → Factura → Fidelización → Mesa
    /// </summary>
    public class ProcesarPedidoCompletoHandlerTests
    {
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<IFacturaRepository> _facturaRepositoryMock;
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IMesaRepository> _mesaRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<ProcesarPedidoCompletoHandler>> _loggerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IServicioFacturacion> _servicioFacturacionMock;
        private readonly Mock<IComercialServiceFacade> _comercialServiceFacadeMock;
        private readonly Mock<IFacturacionService> _facturacionServiceMock;
        private readonly Mock<IFidelizacionService> _fidelizacionServiceMock;
        private readonly Mock<IMesaService> _mesaServiceMock;
        private readonly ProcesarPedidoCompletoHandler _handler;

        public ProcesarPedidoCompletoHandlerTests()
        {
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _facturaRepositoryMock = new Mock<IFacturaRepository>();
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _mesaRepositoryMock = new Mock<IMesaRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<ProcesarPedidoCompletoHandler>>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _mediatorMock = new Mock<IMediator>();
            _servicioFacturacionMock = new Mock<IServicioFacturacion>();
            _comercialServiceFacadeMock = new Mock<IComercialServiceFacade>();
            _facturacionServiceMock = new Mock<IFacturacionService>();
            _fidelizacionServiceMock = new Mock<IFidelizacionService>();
            _mesaServiceMock = new Mock<IMesaService>();

            _handler = new ProcesarPedidoCompletoHandler(
                _comandaRepositoryMock.Object,
                _facturaRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _mesaRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _currentUserServiceMock.Object,
                _unitOfWorkMock.Object,
                _dateTimeServiceMock.Object,
                _mediatorMock.Object,
                _servicioFacturacionMock.Object,
                _comercialServiceFacadeMock.Object,
                _facturacionServiceMock.Object,
                _fidelizacionServiceMock.Object,
                _mesaServiceMock.Object);
        }

        #region Tests de Escenarios Exitosos

        [Fact]
        public async Task Handle_WorkflowCompletoConPago_DeberiaOrquestarTodoElProceso()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var command = new ProcesarPedidoCompletoCommand
            {
                ComandaId = comandaId,
                TipoPago = "Tarjeta",
                RequierePago = true,
                InfoPago = new InfoPagoDto
                {
                    NumeroTarjeta = "4111111111111111",
                    NombreTitular = "Juan Pérez",
                    MontoTotal = 150.00m,
                    Moneda = "USD"
                },
                UsuarioId = Guid.NewGuid()
            };

            var comanda = CreateMockComanda(comandaId, clienteId, mesaId, 150.00m);
            var resultadoPago = CreateMockResultadoPago("APPROVED", "12345");
            var factura = CreateMockFactura(Guid.NewGuid(), comandaId, 150.00m);
            var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, factura.Id);

            SetupMocksWorkflowCompleto(comanda, resultadoPago, factura, resultadoDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(comandaId, result.Value.ComandaId);
            Assert.Equal(factura.Id, result.Value.FacturaId);
            Assert.Equal(150.00m, result.Value.Total);
            Assert.True(result.Value.PuntosAcumulados > 0);
            Assert.NotNull(result.Value.MesaId);

            // Verificar que se ejecutó todo el workflow
            VerifyWorkflowCompleto(comandaId, clienteId, mesaId);
        }

        [Fact]
        public async Task Handle_WorkflowSinPago_DeberiaOmitirProcesoPago()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var command = new ProcesarPedidoCompletoCommand
            {
                ComandaId = comandaId,
                TipoPago = "Efectivo",
                RequierePago = false,
                UsuarioId = Guid.NewGuid()
            };

            var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 85.00m);
            var factura = CreateMockFactura(Guid.NewGuid(), comandaId, 85.00m);
            var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, factura.Id);

            SetupMocksWorkflowSinPago(comanda, factura, resultadoDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(factura.Id, result.Value.FacturaId);
            Assert.NotNull(result.Value.MesaId);

            // Verificar que NO se llamó al servicio de pagos
            _servicioFacturacionMock.Verify(x => x.ProcesarPagoAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ComandaConCliente_DeberiaAcumularPuntosFidelizacion()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var command = new ProcesarPedidoCompletoCommand
            {
                ComandaId = comandaId,
                TipoPago = "Efectivo",
                RequierePago = false,
                UsuarioId = Guid.NewGuid()
            };

            var comanda = CreateMockComanda(comandaId, clienteId, Guid.NewGuid(), 200.00m);
            var cliente = CreateMockCliente(clienteId);
            var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, Guid.NewGuid());

            SetupMocksConFidelizacion(comanda, cliente, resultadoDto);

            _fidelizacionServiceMock.Setup(x => x.AcumularPuntosAsync(
                It.IsAny<AcumularPuntosRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(20)); // 20 puntos acumulados

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(20, result.Value.PuntosAcumulados);

            _fidelizacionServiceMock.Verify(x => x.AcumularPuntosAsync(
                It.IsAny<AcumularPuntosRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Tests de Validaciones y Errores

        [Fact]
        public async Task Handle_ComandaInexistente_DeberiaRetornarError()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var command = new ProcesarPedidoCompletoCommand
            {
                ComandaId = comandaId,
                TipoPago = "Efectivo",
                UsuarioId = Guid.NewGuid()
            };

            _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Comanda?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("no existe", result.Error);
        }

        [Fact]
        public async Task Handle_ComandaYaFinalizada_DeberiaRetornarError()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var command = new ProcesarPedidoCompletoCommand
            {
                ComandaId = comandaId,
                TipoPago = "Efectivo",
                UsuarioId = Guid.NewGuid()
            };

            var comandaFinalizada = CreateMockComanda(comandaId, null, Guid.NewGuid(), 100.00m);
            comandaFinalizada.ActualizarEstado(EstadoComanda.Finalizada);

            _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comandaFinalizada);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("ya está finalizada", result.Error);
        }

        [Fact]
        public async Task Handle_ErrorProcesoPago_DeberiaRetornarErrorYRollback()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var command = new ProcesarPedidoCompletoCommand
            {
                ComandaId = comandaId,
                TipoPago = "Tarjeta",
                RequierePago = true,
                InfoPago = new InfoPagoDto
                {
                    NumeroTarjeta = "4000000000000002", // Tarjeta que falla
                    MontoTotal = 100.00m
                },
                UsuarioId = Guid.NewGuid()
            };

            var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 100.00m);

            _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);

            _servicioFacturacionMock.Setup(x => x.ProcesarPagoAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<Factura>("Error en el procesamiento del pago"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Error en el procesamiento del pago", result.Error);

            // No debe continuar con facturación si falla el pago
            // _servicioFacturacionMock.Verify(x => x.GenerarFacturaAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ErrorFacturacion_DeberiaRollbackPago()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var command = new ProcesarPedidoCompletoCommand
            {
                ComandaId = comandaId,
                TipoPago = "Tarjeta",
                RequierePago = true,
                InfoPago = new InfoPagoDto
                {
                    NumeroTarjeta = "4111111111111111",
                    MontoTotal = 100.00m
                },
                TipoFactura = "Consumidor Final",
                NombreCliente = "Cliente Test",
                UsuarioId = Guid.NewGuid()
            };

            var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 100.00m);
            var resultadoPago = CreateMockResultadoPago("APPROVED", "12345");

            _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);

            _servicioFacturacionMock.Setup(x => x.ProcesarPagoAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(CreateMockFactura()));

            // Comentado: El handler usa CrearFacturaCommand a través del mediator, no el servicio directamente
            // _servicioFacturacionMock.Setup(x => x.GenerarFacturaAsync(comandaId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            //     .ReturnsAsync(Result.Failure<Factura>("Error al generar factura"));

            _servicioFacturacionMock.Setup(x => x.RevertirPagoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<Factura>(CreateMockFactura()));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Error al generar factura", result.Error);

            // Debe revertir el pago
            _servicioFacturacionMock.Verify(x => x.RevertirPagoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_MesaInexistente_NoDeberiaAfectarProceso()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var command = new ProcesarPedidoCompletoCommand
            {
                ComandaId = comandaId,
                TipoPago = "Efectivo",
                RequierePago = false,
                UsuarioId = Guid.NewGuid()
            };

            var comanda = CreateMockComanda(comandaId, null, mesaId, 50.00m);
            var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, Guid.NewGuid());

            _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);

            _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Mesa?)null); // Mesa no existe

            _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
                .Returns(resultadoDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded); // El proceso debe continuar
            Assert.Null(result.Value.MesaId); // Mesa no se libera cuando no existe

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Mesa no encontrada")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region Tests de Logging y Auditoria

        [Fact]
        public async Task Handle_WorkflowCompleto_DeberiaLoggearCadaPaso()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var command = new ProcesarPedidoCompletoCommand
            {
                ComandaId = comandaId,
                TipoPago = "Efectivo",
                RequierePago = false,
                UsuarioId = Guid.NewGuid()
            };

            var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 100.00m);
            var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, Guid.NewGuid());

            SetupMocksBasico(comanda, resultadoDto);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando procesamiento completo")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("procesado exitosamente")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_TiempoEjecucion_DeberiaLoggearMetricas()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var command = new ProcesarPedidoCompletoCommand
            {
                ComandaId = comandaId,
                TipoPago = "Efectivo",
                RequierePago = false,
                UsuarioId = Guid.NewGuid()
            };

            var comanda = CreateMockComanda(comandaId, null, Guid.NewGuid(), 100.00m);
            var resultadoDto = CreateMockProcesarPedidoCompletoDto(comandaId, Guid.NewGuid());

            SetupMocksBasico(comanda, resultadoDto);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tiempo total")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region Helper Methods

        private void SetupMocksWorkflowCompleto(Comanda comanda, object resultadoPago, object factura, ProcesarPedidoCompletoDto resultadoDto)
        {
            // Configuración básica
            _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);

            // Configuración para pago
            _servicioFacturacionMock.Setup(x => x.ProcesarPagoAsync(
                comanda.Id, It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(resultadoPago));

            // Configuración para facturación
            _facturacionServiceMock.Setup(x => x.GenerarFacturaAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success((Factura)factura));

            // Configuración para fidelización
            _fidelizacionServiceMock.Setup(x => x.AcumularPuntosAsync(
                It.IsAny<AcumularPuntosRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(15)); // 15 puntos

            // Configuración para liberar mesa
            _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.MesaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMockMesa(comanda.MesaId));

            _mesaServiceMock.Setup(x => x.LiberarMesaAsync(
                comanda.MesaId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new LiberarMesaResult { MesaId = comanda.MesaId }));

            // Configuración del mapper
            _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
                .Returns(resultadoDto);
        }

        private void SetupMocksWorkflowSinPago(Comanda comanda, object factura, ProcesarPedidoCompletoDto resultadoDto)
        {
            // Configuración básica
            _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);

            // Configuración para facturación
            _facturacionServiceMock.Setup(x => x.GenerarFacturaAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success((Factura)factura));

            // Configuración para fidelización (sin cliente)
            _fidelizacionServiceMock.Setup(x => x.AcumularPuntosAsync(
                It.IsAny<AcumularPuntosRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(0)); // 0 puntos (sin cliente)

            // Configuración para liberar mesa
            _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.MesaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMockMesa(comanda.MesaId));

            _mesaServiceMock.Setup(x => x.LiberarMesaAsync(
                comanda.MesaId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new LiberarMesaResult { MesaId = comanda.MesaId }));

            // Configuración del mapper
            _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
                .Returns(resultadoDto);
        }

        private void SetupMocksConFidelizacion(Comanda comanda, Cliente cliente, ProcesarPedidoCompletoDto resultadoDto)
        {
            // Configuración básica
            _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);

            // Configuración del cliente
            _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);

            // Configuración para facturación
            var factura = CreateMockFactura(Guid.NewGuid(), comanda.Id, comanda.Total);
            _facturacionServiceMock.Setup(x => x.GenerarFacturaAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(factura));

            // Configuración para liberar mesa
            _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.MesaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMockMesa(comanda.MesaId));

            _mesaServiceMock.Setup(x => x.LiberarMesaAsync(
                comanda.MesaId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new LiberarMesaResult { MesaId = comanda.MesaId }));

            // Configuración del mapper
            _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
                .Returns(resultadoDto);
        }

        private void SetupMocksConPagoTarjeta(Comanda comanda, object resultadoPago, ProcesarPedidoCompletoDto resultadoDto)
        {
            _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);

            _servicioFacturacionMock.Setup(x => x.ProcesarPagoAsync(
                It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(resultadoPago as Factura ?? CreateMockFactura()));

            // Setup mediator para FinalizarComandaCommand
            _mediatorMock.Setup(x => x.Send(It.IsAny<FinalizarComandaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new ComandaDto { Id = comanda.Id, Estado = EstadoComanda.Finalizada }));

            // Setup mediator para CrearFacturaCommand
            _mediatorMock.Setup(x => x.Send(It.IsAny<CrearFacturaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new FacturaDto { Id = Guid.NewGuid() }));

            // Setup para obtener factura creada
            _facturaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMockFactura());

            _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Setup para servicios de contexto
            _currentUserServiceMock.Setup(x => x.UserId)
                .Returns(Guid.NewGuid().ToString());

            _dateTimeServiceMock.Setup(x => x.Now)
                .Returns(DateTime.UtcNow);

            _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
                .Returns(resultadoDto);
        }

        private void SetupMocksFacturacion(Comanda comanda, object factura, ProcesarPedidoCompletoDto resultadoDto)
        {
            _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);

            // Setup mediator para FinalizarComandaCommand
            _mediatorMock.Setup(x => x.Send(It.IsAny<FinalizarComandaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new ComandaDto { Id = comanda.Id, Estado = EstadoComanda.Finalizada }));

            // Setup mediator para CrearFacturaCommand
            _mediatorMock.Setup(x => x.Send(It.IsAny<CrearFacturaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new FacturaDto { Id = Guid.NewGuid() }));

            // Setup para obtener factura creada
            _facturaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(factura as Factura ?? CreateMockFactura());

            _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Setup para servicios de contexto
            _currentUserServiceMock.Setup(x => x.UserId)
                .Returns(Guid.NewGuid().ToString());

            _dateTimeServiceMock.Setup(x => x.Now)
                .Returns(DateTime.UtcNow);

            _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
                .Returns(resultadoDto);
        }

        private void SetupMocksBasico(Comanda comanda, ProcesarPedidoCompletoDto resultadoDto)
        {
            _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comanda.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);

            // Setup mediator para FinalizarComandaCommand
            _mediatorMock.Setup(x => x.Send(It.IsAny<FinalizarComandaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new ComandaDto { Id = comanda.Id, Estado = EstadoComanda.Finalizada }));

            // Setup mediator para CrearFacturaCommand
            _mediatorMock.Setup(x => x.Send(It.IsAny<CrearFacturaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new FacturaDto { Id = Guid.NewGuid() }));

            // Setup para obtener factura creada
            _facturaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateMockFactura());

            _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Setup para servicios de contexto
            _currentUserServiceMock.Setup(x => x.UserId)
                .Returns(Guid.NewGuid().ToString());

            _dateTimeServiceMock.Setup(x => x.Now)
                .Returns(DateTime.UtcNow);

            _mapperMock.Setup(x => x.Map<ProcesarPedidoCompletoDto>(It.IsAny<object>()))
                .Returns(resultadoDto);
        }

        private void VerifyWorkflowCompleto(Guid comandaId, Guid clienteId, Guid mesaId)
        {
            _comandaRepositoryMock.Verify(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()), Times.Once);
            _servicioFacturacionMock.Verify(x => x.ProcesarPagoAsync(
                It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _servicioFacturacionMock.Verify(x => x.AcumularPuntosPorCompraAsync(
                clienteId, It.IsAny<decimal>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            _mesaRepositoryMock.Verify(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()), Times.Once);
            _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private static Comanda CreateMockComanda(Guid id, Guid? clienteId, Guid mesaId, decimal total)
        {
            return new Comanda
            {
                Id = id,
                ClienteId = clienteId,
                MesaId = mesaId,
                Total = total,
                Estado = EstadoComanda.EnProceso,
                Items = new List<ComandaItem>()
            };
        }

        private static Cliente CreateMockCliente(Guid id)
        {
            return new Cliente
            {
                Id = id,
                Nombre = "Cliente Test",
                Email = "cliente@test.com",
                Telefono = "1234567890",
                FechaRegistro = DateTime.Now.AddYears(-1),
                Activo = true
            };
        }

        private static Mesa CreateMockMesa(Guid id)
        {
            return new Mesa
            {
                Id = id,
                Numero = "10",
                Capacidad = 4,
                Estado = EstadoMesa.Ocupada
            };
        }

        private static object CreateMockResultadoPago(string estado, string transaccionId)
        {
            return new
            {
                Estado = estado,
                TransaccionId = transaccionId,
                Fecha = DateTime.Now,
                Monto = 150.00m
            };
        }

        private static Factura CreateMockFactura(Guid facturaId, Guid comandaId, decimal total)
        {
            return new Factura
            {
                Id = facturaId,
                ComandaId = comandaId,
                Numero = "F-001",
                Total = total,
                Subtotal = total * 0.85m,
                Impuestos = total * 0.15m,
                FechaEmision = DateTime.Now,
                Estado = EstadoFactura.Emitida,
                ClienteId = Guid.NewGuid()
            };
        }

        private static Factura CreateMockFactura()
        {
            return Factura.Crear(
                numeroFactura: $"FAC-{Guid.NewGuid().ToString("N")[^8..].ToUpper()}",
                tipoFactura: TipoFactura.Normal,
                nombreCliente: "Cliente Test",
                clienteId: null,
                identificacionFiscal: null,
                direccionCliente: null,
                comandasIds: new[] { Guid.NewGuid() },
                observaciones: "Factura de prueba",
                fechaEmision: DateTime.UtcNow);
        }

        private static ProcesarPedidoCompletoDto CreateMockProcesarPedidoCompletoDto(Guid comandaId, Guid facturaId)
        {
            return new ProcesarPedidoCompletoDto
            {
                ComandaId = comandaId,
                FacturaId = facturaId,
                Total = 150.00m,
                PuntosAcumulados = 15,
                MesaLiberada = true,
                MesaId = Guid.NewGuid(),
                FechaHora = DateTime.Now,
                EstadoComanda = "Finalizada"
            };
        }

        #endregion
    }
} 