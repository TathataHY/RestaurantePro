namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Commands;

using Moq;
using Xunit;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Facturacion.Services;
using RestaurantePro.Domain.Comercial.Services;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Application.Common.Interfaces;
using System.Reflection;
using System.Linq.Expressions;

/// <summary>
/// Tests unitarios para CrearFacturaHandler
/// Valida la lógica completa de creación de facturas con integración de servicios
/// </summary>
public class CrearFacturaHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CrearFacturaHandler>> _loggerMock;
    private readonly Mock<IServicioFacturacion> _servicioFacturacionMock;
    private readonly Mock<IComercialServiceFacade> _comercialServiceFacadeMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<DbSet<Cliente>> _clientesDbSetMock;
    private readonly Mock<DbSet<Comanda>> _comandasDbSetMock;
    private readonly CrearFacturaHandler _handler;

    public CrearFacturaHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CrearFacturaHandler>>();
        _servicioFacturacionMock = new Mock<IServicioFacturacion>();
        _comercialServiceFacadeMock = new Mock<IComercialServiceFacade>();
        _emailServiceMock = new Mock<IEmailService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _clientesDbSetMock = new Mock<DbSet<Cliente>>();
        _comandasDbSetMock = new Mock<DbSet<Comanda>>();

        // Setup DbContext
        _contextMock.Setup(x => x.Clientes).Returns(_clientesDbSetMock.Object);
        _contextMock.Setup(x => x.Comandas).Returns(_comandasDbSetMock.Object);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

        _handler = new CrearFacturaHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _servicioFacturacionMock.Object,
            _comercialServiceFacadeMock.Object,
            _emailServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    #region Tests de Factory Methods del Command

    [Fact]
    public void CrearConsumidorFinal_ConParametrosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var nombreCliente = "Juan Pérez";

        // Act
        var command = CrearFacturaCommand.CrearConsumidorFinal(comandaId, nombreCliente);

        // Assert
        Assert.Single(command.ComandasIds);
        Assert.Equal(comandaId, command.ComandasIds.First());
        Assert.Equal("Normal", command.TipoFactura);
        Assert.Equal(nombreCliente, command.NombreCliente);
        Assert.True(command.EmitirInmediatamente);
        Assert.Equal(0, command.DiasCredito);
    }

    [Fact]
    public void CrearFiscal_ConParametrosCompletos_DeberiaConfigurarFacturaFiscal()
    {
        // Arrange
        var comandasIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var nombreCliente = "Empresa ABC S.A. de C.V.";
        var rfc = "EAB123456789";
        var direccion = "Av. Principal 123, México";
        var email = "facturacion@empresaabc.com";

        // Act
        var command = CrearFacturaCommand.CrearFiscal(comandasIds, nombreCliente, rfc, direccion, email);

        // Assert
        Assert.Equal(comandasIds.Count, command.ComandasIds.Count);
        Assert.Equal("Fiscal", command.TipoFactura);
        Assert.Equal(nombreCliente, command.NombreCliente);
        Assert.Equal(rfc, command.IdentificacionFiscal);
        Assert.Equal(direccion, command.DireccionCliente);
        Assert.Equal(email, command.EmailCliente);
        Assert.True(command.EmitirInmediatamente);
        Assert.True(command.EnviarPorEmail);
        Assert.Equal(30, command.DiasCredito);
    }

    [Fact]
    public void CrearParaCliente_ConClienteRegistrado_DeberiaUsarClienteId()
    {
        // Arrange
        var comandasIds = new List<Guid> { Guid.NewGuid() };
        var clienteId = Guid.NewGuid();
        var tipoFactura = "Normal";

        // Act
        var command = CrearFacturaCommand.CrearParaCliente(comandasIds, clienteId, tipoFactura);

        // Assert
        Assert.Equal(clienteId, command.ClienteId);
        Assert.Equal(tipoFactura, command.TipoFactura);
        Assert.True(command.EmitirInmediatamente);
        Assert.True(command.EnviarPorEmail);
    }

    [Fact]
    public void CrearConDescuentos_ConDescuentosEspeciales_DeberiaConfigurarDescuentos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var nombreCliente = "Cliente VIP";
        var descuentos = new List<DescuentoAdicionalDto>
        {
            new DescuentoAdicionalDto { 
                Concepto = "Descuento VIP", 
                TipoDescuento = "Promocional",
                Monto = 100,
                Motivo = "Cliente frecuente",
                UsuarioAutorizaId = Guid.NewGuid()
            }
        };

        // Act
        var command = CrearFacturaCommand.CrearConDescuentos(comandaId, "Normal", nombreCliente, descuentos);

        // Assert
        Assert.Single(command.DescuentosAdicionales);
        Assert.Equal("Descuento VIP", command.DescuentosAdicionales.First().Concepto);
        Assert.False(command.EmitirInmediatamente); // Para revisar antes de emitir
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_FacturaConsumidorFinalSinCliente_DeberiaCrearFacturaExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Consumidor Final"
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();
        SetPrivateProperty(factura, "Id", facturaId);

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                comandaId, TipoFactura.Normal, command.NombreCliente, null, 
                null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        var expectedDto = new FacturaDto
        {
            Id = facturaId,
            Numero = factura.NumeroFactura,
            Estado = factura.Estado,
            Total = factura.Total
        };

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
                   .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Console.WriteLine($"Result: {result.Succeeded}, Error: {result.Error}");
        Assert.True(result.Succeeded);
        Assert.Equal(facturaId, result.Value.Id);
    }

    [Fact]
    public async Task Handle_FacturaConClienteRegistrado_DeberiaUsarInformacionCliente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            ClienteId = clienteId,
            NombreCliente = "Cliente Registrado",
            EmitirInmediatamente = false
        };

        var cliente = CreateMockCliente(clienteId, "Cliente Registrado", "cliente@test.com");
        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();
        SetPrivateProperty(factura, "Id", facturaId);

        SetupClientesDbSet(new List<Cliente> { cliente });
        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        var expectedDto = new FacturaDto
        {
            Id = facturaId,
            Numero = factura.NumeroFactura,
            Estado = factura.Estado,
            Total = factura.Total
        };

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
                   .Returns(expectedDto);

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.True(result.Succeeded);
        
        _servicioFacturacionMock.Verify(x => x.GenerarFacturaParaComandaAsync(
            comandaId, TipoFactura.Normal, "Cliente Registrado", clienteId, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FacturaMultiplesComandas_DeberiaUsarServicioMultiplesComandas()
    {
        // Arrange
        var comandasIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var facturaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = comandasIds,
            TipoFactura = "Normal",
            NombreCliente = "Cliente Múltiples Comandas"
        };

        var comandas = comandasIds.Select(CreateMockComandaFinalizada).ToList();
        var factura = CreateMockFactura();
        SetPrivateProperty(factura, "Id", facturaId);

        SetupComandasDbSet(comandas);

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandasAsync(
                comandasIds, TipoFactura.Normal, "Cliente Múltiples Comandas", null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        var expectedDto = new FacturaDto
        {
            Id = facturaId,
            Numero = factura.NumeroFactura,
            Estado = factura.Estado,
            Total = factura.Total
        };

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
                   .Returns(expectedDto);

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.True(result.Succeeded);

        _servicioFacturacionMock.Verify(x => x.GenerarFacturaParaComandasAsync(
            comandasIds, TipoFactura.Normal, "Cliente Múltiples Comandas", null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConEmisionInmediataTrue_DeberiaEmitirFactura()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test",
            EmitirInmediatamente = true
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();
        SetPrivateProperty(factura, "Id", facturaId);

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _servicioFacturacionMock.Setup(x => x.EmitirFacturaAsync(
                It.Is<Guid>(id => id == facturaId), 
                It.IsAny<int>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(CreateMockFacturaEmitida()));

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.True(result.Succeeded);
        _servicioFacturacionMock.Verify(x => x.EmitirFacturaAsync(
             It.Is<Guid>(id => id == facturaId), 
             It.Is<int>(dias => dias == 0), 
             It.IsAny<CancellationToken>()), 
             Times.Once);
    }

    [Fact]
    public async Task Handle_ConClienteConPuntos_DeberiaRegistrarPuntosFidelizacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            ClienteId = clienteId,
            NombreCliente = "Cliente Con Puntos"
        };

        var cliente = CreateMockCliente(clienteId, "Cliente Con Puntos", "cliente@test.com");
        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();
        SetPrivateProperty(factura, "Id", facturaId);

        SetupClientesDbSet(new List<Cliente> { cliente });
        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosPorCompraAsync(
                clienteId, factura.Total, facturaId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(100));

        var expectedDto = new FacturaDto
        {
            Id = facturaId,
            Numero = factura.NumeroFactura,
            Estado = factura.Estado,
            Total = factura.Total
        };

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
                   .Returns(expectedDto);

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.True(result.Succeeded);

        _comercialServiceFacadeMock.Verify(x => x.AcumularPuntosPorCompraAsync(
            clienteId, factura.Total, facturaId, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConEnviarPorEmailTrue_DeberiaEnviarEmail()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var emailCliente = "cliente@test.com";
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test",
            EnviarPorEmail = true,
            EmailCliente = emailCliente
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();
        SetPrivateProperty(factura, "Id", facturaId);

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _emailServiceMock.Setup(x => x.SendHtmlEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.True(result.Succeeded);
        _emailServiceMock.Verify(x => x.SendHtmlEmailAsync(
            emailCliente, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConDescuentosAdicionales_DeberiaAplicarDescuentos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var usuarioAutorizaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test",
            DescuentosAdicionales = new List<DescuentoAdicionalDto>
            {
                new DescuentoAdicionalDto { 
                    TipoDescuento = "Porcentaje", 
                    Monto = 10, 
                    Concepto = "Descuento promocional",
                    Motivo = "Promoción especial",
                    UsuarioAutorizaId = usuarioAutorizaId
                }
            }
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();
        SetPrivateProperty(factura, "Id", facturaId);

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _servicioFacturacionMock.Setup(x => x.AplicarDescuentoAsync(
                facturaId, It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Success(factura));

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.True(result.Succeeded);
        _servicioFacturacionMock.Verify(
            x => x.AplicarDescuentoAsync(
                It.IsAny<Guid>(), 
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()),
            Times.Once);

        // Verificar adicionalmente que se llamó exactamente una vez
        var callCount = _servicioFacturacionMock.Invocations
            .Count(i => i.Method.Name == "AplicarDescuentoAsync");
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Handle_ErrorEnEmision_DeberiaLoggearWarningPeroNoFallar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test",
            EmitirInmediatamente = true
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();
        SetPrivateProperty(factura, "Id", facturaId);

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _servicioFacturacionMock.Setup(x => x.EmitirFacturaAsync(
                It.Is<Guid>(id => id == facturaId), 
                It.IsAny<int>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Factura>("Error al emitir factura"));

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.True(result.Succeeded, "La factura debería crearse exitosamente a pesar del error en emisión");
        
        // Verificar que EmitirFacturaAsync fue llamado con los parámetros correctos
        _servicioFacturacionMock.Verify(
            x => x.EmitirFacturaAsync(
                It.IsAny<Guid>(), 
                It.IsAny<int>(), 
                It.IsAny<CancellationToken>()), 
            Times.Once);
        
        // Verificar que se registró un log de advertencia
        // Verificamos que se llamó al logger
        Assert.True(_loggerMock.Invocations.Any(i => i.Method.Name == "Log"), "El logger debería haber sido llamado");
        
        // Verificamos que fue un log de nivel Warning usando una aproximación diferente
        var hasWarningLog = false;
        foreach (var invocation in _loggerMock.Invocations.Where(i => i.Method.Name == "Log"))
        {
            if (invocation.Arguments.Count > 0 && invocation.Arguments[0] is LogLevel level && level == LogLevel.Warning)
            {
                hasWarningLog = true;
                break;
            }
        }
        Assert.True(hasWarningLog, "Debería haberse registrado un log de nivel Warning");
        
        // Verificar adicionalmente que se llamó exactamente una vez
        var callCountLog = _loggerMock.Invocations
            .Count(i => i.Method.Name == "Log");
        Assert.Equal(1, callCountLog);
    }

    [Fact]
    public async Task Handle_ErrorEnPuntosFidelizacion_DeberiaLoggearWarningPeroNoFallar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            ClienteId = clienteId,
            NombreCliente = "Cliente Test"
        };

        var cliente = CreateMockCliente(clienteId, "Cliente Test", "cliente@test.com");
        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();
        SetPrivateProperty(factura, "Id", facturaId);

        SetupClientesDbSet(new List<Cliente> { cliente });
        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosPorCompraAsync(
                clienteId, factura.Total, facturaId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<int>("Error al acumular puntos"));

        var expectedDto = new FacturaDto
        {
            Id = facturaId,
            Numero = factura.NumeroFactura,
            Estado = factura.Estado,
            Total = factura.Total
        };

        _mapperMock.Setup(x => x.Map<FacturaDto>(It.IsAny<Factura>()))
                   .Returns(expectedDto);

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.True(result.Succeeded); // La factura se crea de todas formas
        
        // Verificar que se registró un log de advertencia
        // Verificamos que se llamó al logger
        Assert.True(_loggerMock.Invocations.Any(i => i.Method.Name == "Log"), "El logger debería haber sido llamado");
        
        // Verificamos que fue un log de nivel Warning usando una aproximación diferente
        var hasWarningLog = false;
        foreach (var invocation in _loggerMock.Invocations.Where(i => i.Method.Name == "Log"))
        {
            if (invocation.Arguments.Count > 0 && invocation.Arguments[0] is LogLevel level && level == LogLevel.Warning)
            {
                hasWarningLog = true;
                break;
            }
        }
        Assert.True(hasWarningLog, "Debería haberse registrado un log de nivel Warning");
        
        // Verificar adicionalmente que se llamó exactamente una vez
        var callCountPuntos = _loggerMock.Invocations
            .Count(i => i.Method.Name == "Log");
        Assert.Equal(1, callCountPuntos);
    }

    #endregion

    #region Tests de Validaciones y Errores

    [Fact]
    public async Task Handle_ClienteNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            ClienteId = clienteId
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        SetupComandasDbSet(new List<Comanda> { comanda });

        // No configuramos clienteDbSet, lo que hará que Cliente sea null

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El cliente especificado no existe", result.Error);
    }

    [Fact]
    public async Task Handle_ComandaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test"
        };

        // No configuramos comandasDbSet, lo que hará que Comanda sea null o una lista vacía

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se encontraron las comandas", result.Error);
    }

    [Fact]
    public async Task Handle_ComandaNoFinalizada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test"
        };

        var comanda = CreateMockComandaEnProceso(comandaId);
        SetupComandasDbSet(new List<Comanda> { comanda });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Console.WriteLine($"Result: {result.Succeeded}, Error: {result.Error}");
        Assert.False(result.Succeeded);
        Assert.Contains("no están finalizadas", result.Error);
    }

    [Fact]
    public async Task Handle_TipoFacturaInvalido_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "TipoInvalido",
            NombreCliente = "Cliente Test"
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        SetupComandasDbSet(new List<Comanda> { comanda });

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Tipo de factura no válido", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorEnServicioFacturacion_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test"
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        SetupComandasDbSet(new List<Comanda> { comanda });

        var errorEsperado = "Error en el servicio de facturación";
        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
            It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotSupportedException(errorEsperado));

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(errorEsperado, result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test"
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        SetupComandasDbSet(new List<Comanda> { comanda });

        // Simular excepción al generar factura
        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error inesperado simulado"));

        // Act
        var result = await Handle_WithSetup(command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error inesperado simulado", result.Error);
    }

    #endregion

    #region Helper Methods - Setup

    private void SetupClientesDbSet(List<Cliente> clientes)
    {
        var queryable = clientes.AsQueryable();
        
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        
        _clientesDbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken _) => clientes.FirstOrDefault(c => c.Id == (Guid)ids[0]));
    }

    private void SetupComandasDbSet(List<Comanda> comandas)
    {
        // Configurar IQueryable
        var queryable = comandas.AsQueryable();
        
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        
        // Configurar FindAsync
        _comandasDbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken _) => comandas.FirstOrDefault(c => c.Id == (Guid)ids[0]));
            
        // Configurar Find
        _comandasDbSetMock.Setup(x => x.Find(It.IsAny<object[]>()))
            .Returns((object[] ids) => comandas.FirstOrDefault(c => c.Id == (Guid)ids[0]));
            
        // Configurar Include para que devuelva el mismo DbSet
        _comandasDbSetMock.Setup(m => m.Include(It.IsAny<string>()))
            .Returns(_comandasDbSetMock.Object);
            
        // Configurar ToListAsync para que devuelva la lista filtrada por Where
        _comandasDbSetMock.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandas);
            
        // Configurar Where para que aplique el filtro y devuelva el resultado filtrado
        _comandasDbSetMock.Setup(m => m.Where(It.IsAny<Expression<Func<Comanda, bool>>>()))
            .Returns<Expression<Func<Comanda, bool>>>(expr => {
                var mock = new Mock<DbSet<Comanda>>();
                var filteredData = comandas.AsQueryable().Where(expr).ToList();
                
                var filteredQueryable = filteredData.AsQueryable();
                mock.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(filteredQueryable.Provider);
                mock.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(filteredQueryable.Expression);
                mock.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(filteredQueryable.ElementType);
                mock.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(() => filteredQueryable.GetEnumerator());
                
                // Configurar ToListAsync para el resultado filtrado
                mock.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(filteredData);
                    
                return mock.Object;
            });
            
        // Configurar el DbContext para devolver este DbSet
        _contextMock.Setup(c => c.Comandas).Returns(_comandasDbSetMock.Object);
    }

    #endregion

    #region Helper Methods - Data Creation

    private static Cliente CreateMockCliente(Guid id, string nombre, string email)
    {
        var clienteNombre = ClienteNombre.Crear(nombre, "Apellido");
        var clienteEmail = Email.Create(email);
        var telefonoCliente = PhoneNumber.Create("5551234567");
        
        // Usar una fecha de nacimiento que garantice más de 18 años
        var fechaNacimiento = DateTime.Now.AddYears(-25);
        
        var cliente = Cliente.Crear(id, clienteNombre, clienteEmail, telefonoCliente, fechaNacimiento);
        return cliente;
    }

    private static Comanda CreateMockComandaFinalizada(Guid id)
    {
        var comanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), "Mesa Test");
        SetPrivateProperty(comanda, "Id", id);
        SetPrivateProperty(comanda, "Estado", EstadoComanda.Finalizada);
        SetPrivateProperty(comanda, "Total", TotalComanda.Crear(250.00m, 0.00m));
        return comanda;
    }

    private static Comanda CreateMockComandaEnProceso(Guid id)
    {
        var comanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), "Comanda en proceso");
        SetPrivateProperty(comanda, "Id", id);
        SetPrivateProperty(comanda, "Estado", EstadoComanda.EnProceso);
        return comanda;
    }

    private static Factura CreateMockFactura()
    {
        var facturaId = Guid.NewGuid();
        var comandasIds = new List<Guid> { Guid.NewGuid() };
        
        var factura = Factura.Crear(
            numeroFactura: "F-12345",
            nombreCliente: "Cliente Test",
            tipoFactura: TipoFactura.Normal,
            clienteId: null,
            identificacionFiscal: null,
            direccionCliente: null,
            comandasIds: comandasIds,
            observaciones: null);
        
        SetPrivateProperty(factura, "Subtotal", 500.00m);
        SetPrivateProperty(factura, "TotalImpuestos", 80.00m);
        SetPrivateProperty(factura, "Total", 580.00m);
        
        return factura;
    }

    private static Factura CreateMockFacturaEmitida()
    {
        var facturaId = Guid.NewGuid();
        var comandasIds = new List<Guid> { Guid.NewGuid() };
        
        var factura = Factura.Crear(
            numeroFactura: "F-12346",
            nombreCliente: "Cliente Test",
            tipoFactura: TipoFactura.Normal,
            clienteId: null,
            identificacionFiscal: null,
            direccionCliente: null,
            comandasIds: comandasIds,
            observaciones: null);
        
        SetPrivateProperty(factura, "Subtotal", 500.00m);
        SetPrivateProperty(factura, "TotalImpuestos", 80.00m);
        SetPrivateProperty(factura, "Total", 580.00m);
        SetPrivateProperty(factura, "Estado", EstadoFactura.Emitida);
        
        return factura;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        var type = obj.GetType();
        var property = type.GetProperty(propertyName, 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (property == null)
        {
            var field = type.GetField(propertyName, 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (field == null)
                throw new ArgumentException($"Property or field '{propertyName}' not found in type '{type.FullName}'");
            
            field.SetValue(obj, value);
        }
        else
        {
            property.SetValue(obj, value);
        }
    }

    #endregion

    private async Task<Result<FacturaDto>> Handle_WithSetup(CrearFacturaCommand command, Action? additionalSetup = null)
    {
        additionalSetup?.Invoke();
        
        return await _handler.Handle(command, CancellationToken.None);
    }
} 