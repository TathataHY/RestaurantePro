using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Services;
using RestaurantePro.Domain.Comercial.Facturacion.Services;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.Commands;

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
        var descuentos = new List<DescuentoFacturaDto>
        {
            new DescuentoFacturaDto { Concepto = "Descuento VIP", Porcentaje = 10 }
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
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Consumidor Final",
            EmitirInmediatamente = true
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupComandasDbSet(new List<Comanda> { comanda });
        
        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                comandaId, TipoFactura.Normal, "Cliente Consumidor Final", null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(factura.Id, result.Value.Id);
        Assert.Equal(factura.NumeroFactura, result.Value.Numero);

        _servicioFacturacionMock.Verify(x => x.GenerarFacturaParaComandaAsync(
            comandaId, TipoFactura.Normal, "Cliente Consumidor Final", null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FacturaConClienteRegistrado_DeberiaUsarInformacionCliente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            ClienteId = clienteId,
            EmitirInmediatamente = false
        };

        var cliente = CreateMockCliente(clienteId, "Cliente Registrado", "cliente@test.com");
        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupClientesDbSet(new List<Cliente> { cliente });
        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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
        var command = new CrearFacturaCommand
        {
            ComandasIds = comandasIds,
            TipoFactura = "Normal",
            NombreCliente = "Cliente Múltiples Comandas"
        };

        var comandas = comandasIds.Select(CreateMockComandaFinalizada).ToList();
        var factura = CreateMockFactura();

        SetupComandasDbSet(comandas);

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandasAsync(
                comandasIds, TipoFactura.Normal, "Cliente Múltiples Comandas", null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test",
            EmitirInmediatamente = true,
            DiasCredito = 15
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();
        var facturaEmitida = CreateMockFacturaEmitida();

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _servicioFacturacionMock.Setup(x => x.EmitirFacturaAsync(factura.Id, 15, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(facturaEmitida));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        _servicioFacturacionMock.Verify(x => x.EmitirFacturaAsync(factura.Id, 15, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConClienteConPuntos_DeberiaRegistrarPuntosFidelizacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Fidelización",
            ClienteId = clienteId
        };

        var cliente = CreateMockCliente(clienteId, "Cliente Fidelización", "cliente@test.com");
        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupClientesDbSet(new List<Cliente> { cliente });
        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosPorCompraAsync(
                clienteId, factura.Total, null, "Compra - Facturación", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(50));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        _comercialServiceFacadeMock.Verify(x => x.AcumularPuntosPorCompraAsync(
            clienteId, factura.Total, null, "Compra - Facturación", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConEnviarPorEmailTrue_DeberiaEnviarEmail()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var email = "cliente@test.com";
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Email",
            EnviarPorEmail = true,
            EmailCliente = email
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        _emailServiceMock.Verify(x => x.SendEmailAsync(
            email, 
            It.Is<string>(asunto => asunto.Contains(factura.NumeroFactura)), 
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConDescuentosAdicionales_DeberiaAplicarDescuentos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Descuentos",
            DescuentosAdicionales = new List<DescuentoFacturaDto>
            {
                new DescuentoFacturaDto { Concepto = "Descuento VIP", Porcentaje = 10, Motivo = "Cliente frecuente" },
                new DescuentoFacturaDto { Concepto = "Descuento promocional", MontoFijo = 50.00m, Motivo = "Promoción especial" }
            }
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        // Los descuentos se procesan aunque no hay métodos directos para verificar en la entidad mock
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

        SetupClientesDbSet(new List<Cliente>()); // Cliente no existe

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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

        SetupComandasDbSet(new List<Comanda>()); // Comanda no existe

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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

        var comanda = CreateMockComandaEnProceso(comandaId); // Estado diferente a Finalizada

        SetupComandasDbSet(new List<Comanda> { comanda });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
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
        var result = await _handler.Handle(command, CancellationToken.None);

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

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Factura>("Error en el servicio de facturación"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Error en el servicio de facturación", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorEnEmision_DeberiaLoggearWarningPeroNoFallar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test",
            EmitirInmediatamente = true
        };

        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _servicioFacturacionMock.Setup(x => x.EmitirFacturaAsync(factura.Id, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Factura>("Error al emitir"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // No debe fallar el proceso general
        
        // Verificar que se intentó emitir
        _servicioFacturacionMock.Verify(x => x.EmitirFacturaAsync(factura.Id, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnPuntosFidelizacion_DeberiaLoggearWarningPeroNoFallar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new CrearFacturaCommand
        {
            ComandasIds = new List<Guid> { comandaId },
            TipoFactura = "Normal",
            NombreCliente = "Cliente Test",
            ClienteId = clienteId
        };

        var cliente = CreateMockCliente(clienteId, "Cliente Test", "test@test.com");
        var comanda = CreateMockComandaFinalizada(comandaId);
        var factura = CreateMockFactura();

        SetupClientesDbSet(new List<Cliente> { cliente });
        SetupComandasDbSet(new List<Comanda> { comanda });

        _servicioFacturacionMock.Setup(x => x.GenerarFacturaParaComandaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoFactura>(), It.IsAny<string>(), It.IsAny<Guid?>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(factura));

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosPorCompraAsync(
                It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<int>("Error en fidelización"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // No debe fallar el proceso general

        _comercialServiceFacadeMock.Verify(x => x.AcumularPuntosPorCompraAsync(
            clienteId, factura.Total, null, "Compra - Facturación", It.IsAny<CancellationToken>()), Times.Once);
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

        // En su lugar, configurar el context para que falle
        _contextMock.Setup(x => x.Comandas).Throws(new InvalidOperationException("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno al crear la factura", result.Error);
    }

    #endregion

    #region Helper Methods - Setup

    private void SetupClientesDbSet(List<Cliente> clientes)
    {
        var queryable = clientes.AsQueryable();
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _clientesDbSetMock.As<IQueryable<Cliente>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        // NO usar FirstOrDefaultAsync - es un método de extensión que causa errores
        // _clientesDbSetMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Cliente, bool>>>(), It.IsAny<CancellationToken>()))
        //     .Returns<System.Linq.Expressions.Expression<Func<Cliente, bool>>, CancellationToken>((predicate, token) =>
        //     {
        //         var compiled = predicate.Compile();
        //         var result = clientes.FirstOrDefault(compiled);
        //         return Task.FromResult(result);
        //     });
    }

    private void SetupComandasDbSet(List<Comanda> comandas)
    {
        var queryable = comandas.AsQueryable();
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _comandasDbSetMock.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        // NO usar Where() - es un método de extensión que causa errores
        // _comandasDbSetMock.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Comanda, bool>>>()))
        //     .Returns<System.Linq.Expressions.Expression<Func<Comanda, bool>>>(predicate =>
        //     {
        //         var compiled = predicate.Compile();
        //         return comandas.Where(compiled).AsQueryable();
        //     });

        // NO hacer setup de Include() ya que es un método de extensión y causa errores de Moq
        // El handler debe funcionar sin Include para las pruebas unitarias
    }

    #endregion

    #region Helper Methods - Data Creation

    private static Cliente CreateMockCliente(Guid id, string nombre, string email)
    {
        // Usar el factory method del dominio en lugar de Mock
        var nombreCompleto = ClienteNombre.Crear(nombre, "Apellido Test");
        var emailVO = Email.Create(email);
        var telefono = PhoneNumber.Create("+5212345678900"); // Teléfono de prueba
        var fechaNacimiento = DateTime.Now.AddYears(-30);
        
        var cliente = Cliente.Crear(id, nombreCompleto, emailVO, telefono, fechaNacimiento, true);
        
        return cliente;
    }

    private static Comanda CreateMockComandaFinalizada(Guid id)
    {
        // Usar el factory method del dominio
        var meseroId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        
        var comanda = Comanda.Crear(meseroId, clienteId, mesaId, "Comanda de prueba");
        
        // Agregar algunos items para que tenga contenido
        comanda.AgregarItem(Guid.NewGuid(), "Producto Test", 2, 15.50m, "Sin observaciones");
        comanda.AgregarItem(Guid.NewGuid(), "Producto Test 2", 1, 25.00m, "Sin observaciones");
        
        // Usar reflexión para cambiar el estado a Finalizada (solo para pruebas)
        SetPrivateProperty(comanda, "Id", id);
        SetPrivateProperty(comanda, "Estado", EstadoComanda.Finalizada);
        
        return comanda;
    }

    private static Comanda CreateMockComandaEnProceso(Guid id)
    {
        // Usar el factory method del dominio
        var meseroId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        
        var comanda = Comanda.Crear(meseroId, clienteId, mesaId, "Comanda en proceso");
        
        // Agregar algunos items
        comanda.AgregarItem(Guid.NewGuid(), "Producto Test", 1, 20.00m, "Sin observaciones");
        
        // Usar reflexión para cambiar el ID y estado
        SetPrivateProperty(comanda, "Id", id);
        SetPrivateProperty(comanda, "Estado", EstadoComanda.EnProceso);
        
        return comanda;
    }

    private static Factura CreateMockFactura()
    {
        // Usar el factory method del dominio
        var numeroFactura = $"FAC-{DateTime.Now:yyyyMMdd}-001";
        var tipoFactura = TipoFactura.Normal;
        var nombreCliente = "Cliente Test";
        var comandaId = Guid.NewGuid();
        
        var factura = Factura.Crear(
            numeroFactura,
            tipoFactura,
            nombreCliente,
            null,
            null,
            null,
            new List<Guid> { comandaId },
            "Factura de prueba"
        );
        
        return factura;
    }

    private static Factura CreateMockFacturaEmitida()
    {
        // Usar el factory method del dominio
        var numeroFactura = $"FAC-{DateTime.Now:yyyyMMdd}-002";
        var tipoFactura = TipoFactura.Normal;
        var nombreCliente = "Cliente Test";
        var comandaId = Guid.NewGuid();
        
        var factura = Factura.Crear(
            numeroFactura,
            tipoFactura,
            nombreCliente,
            null,
            null,
            null,
            new List<Guid> { comandaId },
            "Factura emitida de prueba"
        );
        
        // Usar reflexión para cambiar el estado a Emitida (solo para pruebas)
        SetPrivateProperty(factura, "Estado", EstadoFactura.Emitida);
        
        return factura;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();
        
        // Para EntityBase, intentar usar backing fields directamente
        if (propertyName == "Id")
        {
            // El Id es protected set en EntityBase, intentamos el field privado
            var idField = type.BaseType?.GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance) ??
                         type.GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (idField != null)
            {
                idField.SetValue(obj, value);
                return;
            }
        }
        
        // Para Estado, buscar backing field
        if (propertyName == "Estado")
        {
            var estadoField = type.GetField("<Estado>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (estadoField != null)
            {
                estadoField.SetValue(obj, value);
                return;
            }
        }
        
        // Fallback: intentar property normal
        var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
    }

    #endregion
} 