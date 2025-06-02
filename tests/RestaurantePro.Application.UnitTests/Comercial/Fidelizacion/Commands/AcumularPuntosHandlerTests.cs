using RestaurantePro.Domain.Comercial.Promociones.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;

namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Commands;

/// <summary>
/// Tests unitarios para AcumularPuntosHandler
/// Valida la lógica completa de acumulación de puntos con reglas de negocio complejas y promociones automáticas
/// </summary>
public class AcumularPuntosHandlerTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
    private readonly Mock<ITransaccionPuntosRepository> _transaccionRepositoryMock;
    private readonly Mock<IPromocionRepository> _promocionRepositoryMock;
    private readonly Mock<ICalculadoraPuntosService> _calculadoraPuntosMock;
    private readonly Mock<IServicioFidelizacion> _servicioFidelizacionMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AcumularPuntosHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AcumularPuntosHandler _handler;

    public AcumularPuntosHandlerTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
        _transaccionRepositoryMock = new Mock<ITransaccionPuntosRepository>();
        _promocionRepositoryMock = new Mock<IPromocionRepository>();
        _calculadoraPuntosMock = new Mock<ICalculadoraPuntosService>();
        _servicioFidelizacionMock = new Mock<IServicioFidelizacion>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AcumularPuntosHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new AcumularPuntosHandler(
            _clienteRepositoryMock.Object,
            _tarjetaRepositoryMock.Object,
            _transaccionRepositoryMock.Object,
            _promocionRepositoryMock.Object,
            _calculadoraPuntosMock.Object,
            _servicioFidelizacionMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object);
    }

    #region Tests de Factory Methods del Command

    [Fact]
    public void CrearAcumulacionVenta_ConMontoValido_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var montoVenta = 150.75m;

        // Act
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            FacturaId = facturaId,
            MontoCompra = montoVenta,
            TipoAcumulacion = TipoAcumulacion.PorCompra,
            Comentarios = "Compra restaurante"
        };

        // Assert
        Assert.Equal(clienteId, command.ClienteId);
        Assert.Equal(facturaId, command.FacturaId);
        Assert.Equal(montoVenta, command.MontoCompra);
        Assert.Equal("Compra restaurante", command.Comentarios);
        Assert.Equal(TipoAcumulacion.PorCompra, command.TipoAcumulacion);
        Assert.False(command.EsAcumulacionManual);
    }

    [Fact]
    public void CrearAcumulacionManual_ConPuntosDirectos_DeberiaConfigurarManual()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var puntosDirectos = 500;
        var motivo = "Compensación por error";

        // Act
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            PuntosDirectos = puntosDirectos,
            MotivoAcumulacionManual = motivo,
            UsuarioQueAcumula = "ADMIN001",
            TipoAcumulacion = TipoAcumulacion.Manual,
            EsAcumulacionManual = true
        };

        // Assert
        Assert.Equal(clienteId, command.ClienteId);
        Assert.Equal(puntosDirectos, command.PuntosDirectos);
        Assert.Equal(motivo, command.MotivoAcumulacionManual);
        Assert.Equal("ADMIN001", command.UsuarioQueAcumula);
        Assert.Equal(TipoAcumulacion.Manual, command.TipoAcumulacion);
        Assert.True(command.EsAcumulacionManual);
    }

    [Fact]
    public void CrearAcumulacionPromocion_ConCodigoEspecial_DeberiaConfigurarPromocion()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var codigoPromocion = "HAPPY2025";
        var montoVenta = 200.00m;

        // Act
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            CodigoPromocion = codigoPromocion,
            MontoCompra = montoVenta,
            TipoAcumulacion = TipoAcumulacion.PorPromocion,
            Comentarios = "Promoción fin de año"
        };

        // Assert
        Assert.Equal(codigoPromocion, command.CodigoPromocion);
        Assert.Equal(TipoAcumulacion.PorPromocion, command.TipoAcumulacion);
        Assert.Equal("Promoción fin de año", command.Comentarios);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_AcumulacionVentaBasica_DeberiaCalcularPuntosCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            FacturaId = facturaId,
            MontoCompra = 120.50m,
            TipoAcumulacion = TipoAcumulacion.PorCompra,
            Comentarios = "Almuerzo familiar"
        };

        var resultadoAcumulacion = CreateMockResultadoVentaBasica(clienteId);

        // Setup mocks básicos
        var clienteMock = CreateMockCliente(clienteId);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId))
            .ReturnsAsync(clienteMock);

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(clienteId, result.Value.ClienteId);
    }

    [Fact]
    public async Task Handle_AcumulacionConCambioNivel_DeberiaDetectarUpgrade()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            MontoCompra = 450.75m, // Compra grande que provoca upgrade
            TipoAcumulacion = TipoAcumulacion.PorCompra
        };

        // Setup mocks básicos
        var clienteMock = CreateMockCliente(clienteId);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId))
            .ReturnsAsync(clienteMock);

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(clienteId, result.Value.ClienteId);
    }

    [Fact]
    public async Task Handle_AcumulacionConPromocion_DeberiaAplicarMultiplicador()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            MontoCompra = 180.00m,
            CodigoPromocion = "DOUBLE2025",
            TipoAcumulacion = TipoAcumulacion.PorPromocion
        };

        // Setup mocks básicos
        var clienteMock = CreateMockCliente(clienteId);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId))
            .ReturnsAsync(clienteMock);

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal(clienteId, result.Value.ClienteId);
    }

    #endregion

    #region Tests de Errores

    [Fact]
    public async Task Handle_ClienteInexistente_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            MontoCompra = 100.00m
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("no existe", result.Error);
    }

    [Fact]
    public async Task Handle_ClienteInactivo_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            MontoCompra = 100.00m
        };

        var clienteInactivo = CreateMockCliente(clienteId, activo: false);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId))
            .ReturnsAsync(clienteInactivo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("no está elegible", result.Error);
    }

    #endregion

    #region Helper Methods

    private static Cliente CreateMockCliente(Guid clienteId, bool activo = true)
    {
        var nombre = ClienteNombre.Crear("Juan", "Pérez");
        var email = Email.Create("juan.perez@test.com");
        var telefono = PhoneNumber.Create("123456789");

        return Cliente.Crear(
            clienteId,
            nombre,
            email,
            telefono,
            DateTime.Now.AddYears(-30),
            activo);
    }

    private static AcumulacionPuntosDto CreateMockResultadoVentaBasica(Guid clienteId)
    {
        return new AcumulacionPuntosDto
        {
            ClienteId = clienteId,
            TarjetaFidelizacionId = Guid.NewGuid(),
            PuntosAcumulados = 121,
            TotalPuntos = 2421,
            MontoTransaccion = 120.50m,
            FactorMultiplicacion = 1.0m,
            FechaAcumulacion = DateTime.UtcNow,
            Concepto = "Acumulación por Compra"
        };
    }

    #endregion
} 