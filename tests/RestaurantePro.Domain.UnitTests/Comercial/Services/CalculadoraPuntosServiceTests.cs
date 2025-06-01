namespace RestaurantePro.Domain.UnitTests.Comercial.Services;

/// <summary>
/// Pruebas unitarias para CalculadoraPuntosService (Servicio de Dominio)
/// </summary>
public class CalculadoraPuntosServiceTests
{
    private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
    private readonly Mock<ILogger<CalculadoraPuntosService>> _loggerMock;
    private readonly CalculadoraPuntosService _service;

    public CalculadoraPuntosServiceTests()
    {
        _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
        _loggerMock = new Mock<ILogger<CalculadoraPuntosService>>();
        _service = new CalculadoraPuntosService(_tarjetaRepositoryMock.Object, _loggerMock.Object);
    }

    #region CalcularPuntosPorCompraAsync Tests

    [Fact]
    public async Task CalcularPuntosPorCompraAsync_ConTarjetaPlata_DeberiaCalcularPuntosCorrectamente()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var montoCompra = 200m;
        var tarjeta = TarjetaFidelizacionBuilder.Crear()
            .ConId(tarjetaId)
            .ConNivel(NivelFidelizacion.Plata)
            .ConEstadoActivo()
            .Build();

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.CalcularPuntosPorCompraAsync(tarjetaId, montoCompra);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Puntos.Should().Be(3); // 200 * 0.015 = 3
        resultado.Value.Bonificacion.Should().Be(0); // 5% de 3 = 0.15, Math.Floor = 0
        resultado.Value.TotalPuntos.Should().Be(3);
        resultado.Value.TasaConversion.Should().Be(0.015m);
    }

    [Fact]
    public async Task CalcularPuntosPorCompraAsync_ConTarjetaOro_DeberiaAplicarBonificacion()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var montoCompra = 1000m;
        var tarjeta = TarjetaFidelizacionBuilder.Crear()
            .ConId(tarjetaId)
            .ConNivel(NivelFidelizacion.Oro)
            .ConEstadoActivo()
            .Build();

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.CalcularPuntosPorCompraAsync(tarjetaId, montoCompra);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Puntos.Should().Be(20); // 1000 * 0.02 = 20
        resultado.Value.Bonificacion.Should().Be(2); // 10% de 20 = 2
        resultado.Value.TotalPuntos.Should().Be(22);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-100.50)]
    public async Task CalcularPuntosPorCompraAsync_ConMontoInvalido_DeberiaRetornarError(decimal montoInvalido)
    {
        // Act
        var resultado = await _service.CalcularPuntosPorCompraAsync(Guid.NewGuid(), montoInvalido);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ErrorMessage.Should().Be("El monto de compra debe ser mayor a cero");
    }

    [Fact]
    public async Task CalcularPuntosPorCompraAsync_ConTarjetaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TarjetaFidelizacion?)null);

        // Act
        var resultado = await _service.CalcularPuntosPorCompraAsync(tarjetaId, 100m);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ErrorMessage.Should().Be("Tarjeta de fidelización no encontrada");
    }

    [Fact]
    public async Task CalcularPuntosPorCompraAsync_ConTarjetaInactiva_DeberiaRetornarError()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var tarjeta = TarjetaFidelizacionBuilder.Crear()
            .ConId(tarjetaId)
            .ConNivel(NivelFidelizacion.Bronce)
            .ConEstadoInactivo()
            .Build();

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.CalcularPuntosPorCompraAsync(tarjetaId, 100m);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ErrorMessage.Should().Be("La tarjeta de fidelización está inactiva");
    }

    #endregion

    #region ValidarCanjePuntosAsync Tests

    [Fact]
    public async Task ValidarCanjePuntosAsync_ConPuntosSuficientes_DeberiaRetornarTrue()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var puntosACanjear = 50;
        var tarjeta = TarjetaFidelizacionBuilder.Crear()
            .ConId(tarjetaId)
            .ConNivel(NivelFidelizacion.Plata)
            .ConEstadoActivo()
            .ConPuntosDisponibles(100)
            .Build();

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.ValidarCanjePuntosAsync(tarjetaId, puntosACanjear);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ValidarCanjePuntosAsync_ConPuntosInsuficientes_DeberiaRetornarError()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var puntosACanjear = 150;
        var tarjeta = TarjetaFidelizacionBuilder.Crear()
            .ConId(tarjetaId)
            .ConNivel(NivelFidelizacion.Plata)
            .ConEstadoActivo()
            .ConPuntosDisponibles(100)
            .Build();

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.ValidarCanjePuntosAsync(tarjetaId, puntosACanjear);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ErrorMessage.Should().Contain("Puntos insuficientes");
        resultado.ErrorMessage.Should().Contain("Disponibles: 100");
        resultado.ErrorMessage.Should().Contain("Requeridos: 150");
    }

    #endregion

    #region ObtenerTasaConversionAsync Tests

    [Theory]
    [InlineData(NivelFidelizacion.Bronce, 0.01)]
    [InlineData(NivelFidelizacion.Plata, 0.015)]
    [InlineData(NivelFidelizacion.Oro, 0.02)]
    [InlineData(NivelFidelizacion.Platino, 0.025)]
    public async Task ObtenerTasaConversionAsync_ConDiferentesNiveles_DeberiaRetornarTasaCorrecta(
        NivelFidelizacion nivel, decimal tasaEsperada)
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var tarjeta = TarjetaFidelizacionBuilder.Crear()
            .ConId(tarjetaId)
            .ConNivel(nivel)
            .ConEstadoActivo()
            .Build();

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.ObtenerTasaConversionAsync(tarjetaId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(tasaEsperada);
    }

    #endregion

    #region CalcularValorPuntosAsync Tests

    [Theory]
    [InlineData(NivelFidelizacion.Bronce, 100, 50.0)] // 100 * $0.50
    [InlineData(NivelFidelizacion.Plata, 100, 60.0)]  // 100 * $0.60
    [InlineData(NivelFidelizacion.Oro, 100, 70.0)]    // 100 * $0.70
    [InlineData(NivelFidelizacion.Platino, 100, 80.0)] // 100 * $0.80
    public async Task CalcularValorPuntosAsync_ConDiferentesNiveles_DeberiaCalcularValorCorrectamente(
        NivelFidelizacion nivel, int puntos, decimal valorEsperado)
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var tarjeta = TarjetaFidelizacionBuilder.Crear()
            .ConId(tarjetaId)
            .ConNivel(nivel)
            .ConEstadoActivo()
            .Build();

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.CalcularValorPuntosAsync(tarjetaId, puntos);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(valorEsperado);
    }

    #endregion

    #region CalcularBonificacionPorNivelAsync Tests

    [Theory]
    [InlineData(NivelFidelizacion.Bronce, 100, 0)]   // Sin bonificación
    [InlineData(NivelFidelizacion.Plata, 100, 5)]   // 5% = 5 puntos
    [InlineData(NivelFidelizacion.Oro, 100, 10)]    // 10% = 10 puntos
    [InlineData(NivelFidelizacion.Platino, 100, 15)] // 15% = 15 puntos
    [InlineData(NivelFidelizacion.Oro, 53, 5)]      // 10% de 53 = 5.3, Math.Floor = 5
    public async Task CalcularBonificacionPorNivelAsync_ConDiferentesEscenarios_DeberiaCalcularCorrectamente(
        NivelFidelizacion nivel, int puntosBase, int bonificacionEsperada)
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var tarjeta = TarjetaFidelizacionBuilder.Crear()
            .ConId(tarjetaId)
            .ConNivel(nivel)
            .ConEstadoActivo()
            .Build();

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.CalcularBonificacionPorNivelAsync(tarjetaId, puntosBase);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(bonificacionEsperada);
    }

    #endregion

    #region Pruebas de Manejo de Excepciones

    [Fact]
    public async Task CalcularPuntosPorCompraAsync_CuandoRepositorioLanzaExcepcion_DeberiaRetornarError()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error de conexión"));

        // Act
        var resultado = await _service.CalcularPuntosPorCompraAsync(tarjetaId, 100m);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ErrorMessage.Should().Contain("Error calculando puntos");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error calculando puntos")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
} 