namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Commands;

/// <summary>
/// Tests unitarios para AcumularPuntosHandler
/// Valida la lógica completa de acumulación de puntos con reglas de negocio complejas y promociones automáticas
/// </summary>
public class AcumularPuntosHandlerTests
{
    private readonly Mock<IComercialServiceFacade> _comercialServiceFacadeMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<ILogger<AcumularPuntosHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobServiceMock;
    private readonly AcumularPuntosHandler _handler;

    public AcumularPuntosHandlerTests()
    {
        _comercialServiceFacadeMock = new Mock<IComercialServiceFacade>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _loggerMock = new Mock<ILogger<AcumularPuntosHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _backgroundJobServiceMock = new Mock<IBackgroundJobService>();

        _handler = new AcumularPuntosHandler(
            _comercialServiceFacadeMock.Object,
            _clienteRepositoryMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _backgroundJobServiceMock.Object);
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

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoAcumulacion));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(121, result.Value.PuntosAcumulados); // 120.50 = ~121 puntos
        Assert.Equal(2421, result.Value.PuntosTotalesCliente);
        Assert.Equal(NivelFidelizacion.Plata, result.Value.NivelActualCliente);
        Assert.False(result.Value.CambioDeNivel);
        Assert.Equal(1.0m, result.Value.MultiplicadorAplicado);
        Assert.False(result.Value.PromocionAplicada);
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

        var resultadoConUpgrade = CreateMockResultadoConCambioNivel(clienteId);

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoConUpgrade));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(451, result.Value.PuntosAcumulados);
        Assert.True(result.Value.CambioDeNivel);
        Assert.Equal(NivelFidelizacion.Plata, result.Value.NivelAnterior);
        Assert.Equal(NivelFidelizacion.Oro, result.Value.NivelActualCliente);
        Assert.Equal("¡Felicitaciones! Has alcanzado el nivel Oro", result.Value.MensajeCambioNivel);
        Assert.True(result.Value.BonusUpgradeAplicado);
        Assert.Equal(200, result.Value.PuntosBonusUpgrade);
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

        var resultadoConPromocion = CreateMockResultadoConPromocion(clienteId);

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoConPromocion));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(360, result.Value.PuntosAcumulados); // 180 × 2.0 multiplicador
        Assert.True(result.Value.PromocionAplicada);
        Assert.Equal("DOUBLE2025", result.Value.CodigoPromocionUsado);
        Assert.Equal(2.0m, result.Value.MultiplicadorAplicado);
        Assert.Equal("Puntos dobles - Promoción especial", result.Value.DescripcionPromocion);
    }

    [Fact]
    public async Task Handle_AcumulacionManualAdministrativa_DeberiaRegistrarManual()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            PuntosDirectos = 750,
            TipoAcumulacion = TipoAcumulacion.Manual,
            MotivoAcumulacionManual = "Compensación por mal servicio",
            UsuarioQueAcumula = "MANAGER_001",
            EsAcumulacionManual = true
        };

        var resultadoManual = CreateMockResultadoManual(clienteId);

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoManual));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(750, result.Value.PuntosAcumulados);
        Assert.True(result.Value.EsAcumulacionManual);
        Assert.Equal("MANAGER_001", result.Value.UsuarioQueAcumulo);
        Assert.Equal("Compensación por mal servicio", result.Value.MotivoManual);
        Assert.Equal(1.0m, result.Value.MultiplicadorAplicado); // Sin multiplicadores
        Assert.False(result.Value.PromocionAplicada);
    }

    [Fact]
    public async Task Handle_AcumulacionClienteVIP_DeberiaAplicarBeneficiosVIP()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            MontoCompra = 300.00m,
            TipoAcumulacion = TipoAcumulacion.PorCompra,
            Comentarios = "Compra restaurante"
        };

        var resultadoVIP = CreateMockResultadoClienteVIP(clienteId);

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoVIP));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(450, result.Value.PuntosAcumulados); // 300 × 1.5 (multiplicador VIP)
        Assert.Equal(NivelFidelizacion.Platino, result.Value.NivelActualCliente);
        Assert.Equal(1.5m, result.Value.MultiplicadorAplicado);
        Assert.True(result.Value.BeneficiosVIPAplicados);
        Assert.Equal("Multiplica puntos x1.5 + Descuento exclusivo", result.Value.DescripcionBeneficiosVIP);
    }

    [Fact]
    public async Task Handle_AcumulacionConEventoEspecial_DeberiaDispararEventos()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteId,
            MontoCompra = 500.00m,
            TipoAcumulacion = TipoAcumulacion.PorCompra,
            Comentarios = "Compra restaurante"
        };

        var resultadoConEventos = CreateMockResultadoConEventos(clienteId);

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoConEventos));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.EventosDisparados);
        Assert.Equal(3, result.Value.TiposEventosDisparados.Count);
        Assert.Contains("PuntosAcumulados", result.Value.TiposEventosDisparados);
        Assert.Contains("CambioNivel", result.Value.TiposEventosDisparados);
        Assert.Contains("NotificacionCliente", result.Value.TiposEventosDisparados);

        // Verificar que se programó el job de notificaciones
        _backgroundJobServiceMock.Verify(
            x => x.EnqueueBackgroundJob("ProcessFidelizacionEvents", It.IsAny<object>()),
            Times.Once);
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_ClienteInexistente_DeberiaRetornarError()
    {
        // Arrange
        var clienteInexistente = Guid.NewGuid();
        var command = new AcumularPuntosCommand
        {
            ClienteId = clienteInexistente,
            MontoCompra = 100.00m,
            TipoAcumulacion = TipoAcumulacion.PorCompra
        };

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AcumularPuntosResult>("Cliente no encontrado en el sistema"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Cliente no encontrado en el sistema", result.Error);
    }

    [Fact]
    public async Task Handle_MontoVentaInvalido_DeberiaRetornarError()
    {
        // Arrange
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            MontoCompra = -50.00m, // Monto negativo
            TipoAcumulacion = TipoAcumulacion.PorCompra
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El monto de compra debe ser mayor a cero", result.Error);
    }

    [Fact]
    public async Task Handle_AcumulacionManualSinMotivo_DeberiaRetornarError()
    {
        // Arrange
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            PuntosDirectos = 500,
            TipoAcumulacion = TipoAcumulacion.Manual,
            EsAcumulacionManual = true,
            MotivoAcumulacionManual = "", // Sin motivo
            UsuarioQueAcumula = "USER001"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La acumulación manual debe especificar un motivo", result.Error);
    }

    [Fact]
    public async Task Handle_PromocionVencida_DeberiaRetornarError()
    {
        // Arrange
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            MontoCompra = 200.00m,
            CodigoPromocion = "EXPIRED2024",
            TipoAcumulacion = TipoAcumulacion.PorPromocion
        };

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AcumularPuntosResult>("Código de promoción vencido o inválido"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Código de promoción vencido o inválido", result.Error);
    }

    [Fact]
    public async Task Handle_ClienteSinTarjetaFidelizacion_DeberiaRetornarError()
    {
        // Arrange
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            MontoCompra = 150.00m,
            TipoAcumulacion = TipoAcumulacion.PorCompra
        };

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AcumularPuntosResult>("Cliente no posee tarjeta de fidelización activa"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Cliente no posee tarjeta de fidelización activa", result.Error);
    }

    [Fact]
    public async Task Handle_PuntosDirectosExcesivos_DeberiaRetornarError()
    {
        // Arrange
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            PuntosDirectos = 50000, // Excesivo para acumulación manual
            TipoAcumulacion = TipoAcumulacion.Manual,
            MotivoAcumulacionManual = "Motivo válido",
            UsuarioQueAcumula = "ADMIN001",
            EsAcumulacionManual = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La acumulación manual no puede superar los 10,000 puntos", result.Error);
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ErrorServicioComercial_DeberiaRetornarErrorServicio()
    {
        // Arrange
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            MontoCompra = 100.00m,
            TipoAcumulacion = TipoAcumulacion.PorCompra
        };

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AcumularPuntosResult>("Error en cálculo de puntos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error en cálculo de puntos", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            MontoCompra = 100.00m,
            TipoAcumulacion = TipoAcumulacion.PorCompra
        };

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conectividad"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno del sistema", result.Error);
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_AcumulacionExitosa_DeberiaLoggearProceso()
    {
        // Arrange
        var command = new AcumularPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            MontoCompra = 100.00m,
            TipoAcumulacion = TipoAcumulacion.PorCompra
        };

        var resultado = CreateMockResultadoVentaBasica(command.ClienteId);

        _comercialServiceFacadeMock.Setup(x => x.AcumularPuntosAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultado));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        // Verificar logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Puntos acumulados exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Métodos Helper

    private static AcumularPuntosResult CreateMockResultadoVentaBasica(Guid clienteId)
    {
        return new AcumularPuntosResult
        {
            ClienteId = clienteId,
            PuntosAcumulados = 121,
            PuntosTotalesCliente = 2421,
            NivelActualCliente = NivelFidelizacion.Plata,
            CambioDeNivel = false,
            MultiplicadorAplicado = 1.0m,
            PromocionAplicada = false,
            FechaAcumulacion = DateTime.UtcNow,
            TransaccionId = Guid.NewGuid(),
            EsAcumulacionManual = false
        };
    }

    private static AcumularPuntosResult CreateMockResultadoConCambioNivel(Guid clienteId)
    {
        return new AcumularPuntosResult
        {
            ClienteId = clienteId,
            PuntosAcumulados = 451,
            PuntosTotalesCliente = 5451,
            NivelAnterior = NivelFidelizacion.Plata,
            NivelActualCliente = NivelFidelizacion.Oro,
            CambioDeNivel = true,
            MensajeCambioNivel = "¡Felicitaciones! Has alcanzado el nivel Oro",
            BonusUpgradeAplicado = true,
            PuntosBonusUpgrade = 200,
            MultiplicadorAplicado = 1.0m,
            FechaAcumulacion = DateTime.UtcNow,
            TransaccionId = Guid.NewGuid(),
            EventosDisparados = true,
            TiposEventosDisparados = new List<string> { "CambioNivel", "BonusUpgrade" }
        };
    }

    private static AcumularPuntosResult CreateMockResultadoConPromocion(Guid clienteId)
    {
        return new AcumularPuntosResult
        {
            ClienteId = clienteId,
            PuntosAcumulados = 360,
            PuntosTotalesCliente = 3360,
            NivelActualCliente = NivelFidelizacion.Plata,
            CambioDeNivel = false,
            MultiplicadorAplicado = 2.0m,
            PromocionAplicada = true,
            CodigoPromocionUsado = "DOUBLE2025",
            DescripcionPromocion = "Puntos dobles - Promoción especial",
            FechaAcumulacion = DateTime.UtcNow,
            TransaccionId = Guid.NewGuid()
        };
    }

    private static AcumularPuntosResult CreateMockResultadoManual(Guid clienteId)
    {
        return new AcumularPuntosResult
        {
            ClienteId = clienteId,
            PuntosAcumulados = 750,
            PuntosTotalesCliente = 3750,
            NivelActualCliente = NivelFidelizacion.Oro,
            CambioDeNivel = false,
            MultiplicadorAplicado = 1.0m,
            PromocionAplicada = false,
            EsAcumulacionManual = true,
            UsuarioQueAcumulo = "MANAGER_001",
            MotivoManual = "Compensación por mal servicio",
            FechaAcumulacion = DateTime.UtcNow,
            TransaccionId = Guid.NewGuid()
        };
    }

    private static AcumularPuntosResult CreateMockResultadoClienteVIP(Guid clienteId)
    {
        return new AcumularPuntosResult
        {
            ClienteId = clienteId,
            PuntosAcumulados = 450,
            PuntosTotalesCliente = 15450,
            NivelActualCliente = NivelFidelizacion.Platino,
            CambioDeNivel = false,
            MultiplicadorAplicado = 1.5m,
            PromocionAplicada = false,
            BeneficiosVIPAplicados = true,
            DescripcionBeneficiosVIP = "Multiplica puntos x1.5 + Descuento exclusivo",
            FechaAcumulacion = DateTime.UtcNow,
            TransaccionId = Guid.NewGuid()
        };
    }

    private static AcumularPuntosResult CreateMockResultadoConEventos(Guid clienteId)
    {
        return new AcumularPuntosResult
        {
            ClienteId = clienteId,
            PuntosAcumulados = 500,
            PuntosTotalesCliente = 8500,
            NivelAnterior = NivelFidelizacion.Oro,
            NivelActualCliente = NivelFidelizacion.Platino,
            CambioDeNivel = true,
            MensajeCambioNivel = "¡Excelente! Ahora eres cliente Platino",
            MultiplicadorAplicado = 1.0m,
            EventosDisparados = true,
            TiposEventosDisparados = new List<string> 
            { 
                "PuntosAcumulados", 
                "CambioNivel", 
                "NotificacionCliente" 
            },
            FechaAcumulacion = DateTime.UtcNow,
            TransaccionId = Guid.NewGuid()
        };
    }

    #endregion
} 