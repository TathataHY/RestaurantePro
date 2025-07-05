namespace RestaurantePro.Domain.UnitTests.Comercial.Services;

/// <summary>
/// Pruebas unitarias para CalculadoraPuntosService (Servicio de Dominio)
/// </summary>
public class CalculadoraPuntosServiceTests
{
    private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
    private readonly Mock<ILogger<CalculadoraPuntosService>> _loggerMock;
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Mock<ILogger<TarjetaFidelizacionBuilder>> _builderLoggerMock;
    private readonly CalculadoraPuntosService _service;

    public CalculadoraPuntosServiceTests()
    {
        _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
        _loggerMock = new Mock<ILogger<CalculadoraPuntosService>>();
        _notificationManagerMock = new Mock<INotificationManager>();
        _builderLoggerMock = new Mock<ILogger<TarjetaFidelizacionBuilder>>();
        _service = new CalculadoraPuntosService(_tarjetaRepositoryMock.Object, _loggerMock.Object);
    }

    #region CalcularPuntosPorCompraAsync Tests

    [Fact]
    public async Task CalcularPuntosPorCompraAsync_ConTarjetaPlata_DeberiaCalcularPuntosCorrectamente()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoCompra = 200m;
        
        var tarjeta = await CrearTarjetaPrueba(tarjetaId, clienteId, NivelFidelizacion.Plata, activa: true);

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, default, false))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.CalcularPuntosPorCompraAsync(tarjetaId, montoCompra);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
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
        var clienteId = Guid.NewGuid();
        var montoCompra = 1000m;
        
        var tarjeta = await CrearTarjetaPrueba(tarjetaId, clienteId, NivelFidelizacion.Oro, activa: true);

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, default, false))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.CalcularPuntosPorCompraAsync(tarjetaId, montoCompra);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
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
        resultado.IsSuccess().Should().BeFalse();
        resultado.ErrorMessage().Should().Be("El monto de compra debe ser mayor que cero");
    }

    [Fact]
    public async Task CalcularPuntosPorCompraAsync_ConTarjetaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, default, false))
            .ReturnsAsync((TarjetaFidelizacion?)null);

        // Act
        var resultado = await _service.CalcularPuntosPorCompraAsync(tarjetaId, 100m);

        // Assert
        resultado.IsSuccess().Should().BeFalse();
        resultado.ErrorMessage().Should().Be("Tarjeta de fidelización no encontrada");
    }

    [Fact]
    public async Task CalcularPuntosPorCompraAsync_ConTarjetaInactiva_DeberiaRetornarError()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        
        var tarjeta = await CrearTarjetaPrueba(tarjetaId, clienteId, NivelFidelizacion.Basico, activa: false);

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, default, false))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.CalcularPuntosPorCompraAsync(tarjetaId, 100m);

        // Assert
        resultado.IsSuccess().Should().BeFalse();
        resultado.ErrorMessage().Should().Be("La tarjeta de fidelización está inactiva");
    }

    #endregion

    #region ValidarCanjePuntosAsync Tests

    [Fact]
    public async Task ValidarCanjePuntosAsync_ConPuntosSuficientes_DeberiaRetornarTrue()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var puntosACanjear = 50;
        
        var tarjeta = await CrearTarjetaPrueba(tarjetaId, clienteId, NivelFidelizacion.Plata, activa: true, puntosDisponibles: 100);

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, default, false))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.ValidarCanjePuntosAsync(tarjetaId, puntosACanjear);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ValidarCanjePuntosAsync_ConPuntosInsuficientes_DeberiaRetornarError()
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var puntosACanjear = 150;
        
        var tarjeta = await CrearTarjetaPrueba(tarjetaId, clienteId, NivelFidelizacion.Plata, activa: true, puntosDisponibles: 100);

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, default, false))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.ValidarCanjePuntosAsync(tarjetaId, puntosACanjear);

        // Assert
        resultado.IsSuccess().Should().BeFalse();
        resultado.ErrorMessage().Should().Contain("Puntos insuficientes");
        resultado.ErrorMessage().Should().Contain("Disponibles: 100");
        resultado.ErrorMessage().Should().Contain("Requeridos: 150");
    }

    #endregion

    #region ObtenerTasaConversionAsync Tests

    [Theory]
    [InlineData(NivelFidelizacion.Basico, 0.01)]
    [InlineData(NivelFidelizacion.Plata, 0.015)]
    [InlineData(NivelFidelizacion.Oro, 0.02)]
    [InlineData(NivelFidelizacion.Platino, 0.025)]
    public async Task ObtenerTasaConversionAsync_ConDiferentesNiveles_DeberiaRetornarTasaCorrecta(
        NivelFidelizacion nivel, decimal tasaEsperada)
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        
        var tarjeta = await CrearTarjetaPrueba(tarjetaId, clienteId, nivel, activa: true);

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, default, false))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.ObtenerTasaConversionAsync(tarjetaId);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().Be(tasaEsperada);
    }

    #endregion

    #region CalcularValorPuntosAsync Tests

    [Theory]
    [InlineData(NivelFidelizacion.Basico, 100, 50.0)] // 100 * $0.50
    [InlineData(NivelFidelizacion.Plata, 100, 60.0)]  // 100 * $0.60
    [InlineData(NivelFidelizacion.Oro, 100, 70.0)]    // 100 * $0.70
    [InlineData(NivelFidelizacion.Platino, 100, 80.0)] // 100 * $0.80
    public async Task CalcularValorPuntosAsync_ConDiferentesNiveles_DeberiaCalcularValorCorrectamente(
        NivelFidelizacion nivel, int puntos, decimal valorEsperado)
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        
        var tarjeta = await CrearTarjetaPrueba(tarjetaId, clienteId, nivel, activa: true);

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, default, false))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.CalcularValorPuntosAsync(tarjetaId, puntos);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().Be(valorEsperado);
    }

    #endregion

    #region CalcularBonificacionPorNivelAsync Tests

    [Theory]
    [InlineData(NivelFidelizacion.Basico, 100, 0)]   // Sin bonificación
    [InlineData(NivelFidelizacion.Plata, 100, 5)]   // 5% = 5 puntos
    [InlineData(NivelFidelizacion.Oro, 100, 10)]    // 10% = 10 puntos
    [InlineData(NivelFidelizacion.Platino, 100, 15)] // 15% = 15 puntos
    [InlineData(NivelFidelizacion.Oro, 53, 5)]      // 10% de 53 = 5.3, Math.Floor = 5
    public async Task CalcularBonificacionPorNivelAsync_ConDiferentesEscenarios_DeberiaCalcularCorrectamente(
        NivelFidelizacion nivel, int puntosBase, int bonificacionEsperada)
    {
        // Arrange
        var tarjetaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        
        var tarjeta = await CrearTarjetaPrueba(tarjetaId, clienteId, nivel, activa: true);

        _tarjetaRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, default, false))
            .ReturnsAsync(tarjeta);

        // Act
        var resultado = await _service.CalcularBonificacionPorNivelAsync(tarjetaId, puntosBase);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
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
            .Setup(x => x.ObtenerPorIdAsync(tarjetaId, default, false))
            .ThrowsAsync(new InvalidOperationException("Error de conexión"));

        // Act
        var resultado = await _service.CalcularPuntosPorCompraAsync(tarjetaId, 100m);

        // Assert
        resultado.IsSuccess().Should().BeFalse();
        resultado.ErrorMessage().Should().Contain("Error calculando puntos");
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

    #region Métodos Helper

    /// <summary>
    /// Crea una tarjeta de fidelización para pruebas usando el builder real del dominio
    /// </summary>
    private async Task<TarjetaFidelizacion> CrearTarjetaPrueba(
        Guid tarjetaId, 
        Guid clienteId, 
        NivelFidelizacion nivel, 
        bool activa = true, 
        int puntosDisponibles = 0)
    {
        // Configurar el mock del notification manager para que no tenga errores
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);
        _notificationManagerMock.Setup(x => x.GetErrors()).Returns(new ReadOnlyCollection<Error>(new List<Error>()));

        // Crear el builder con las dependencias requeridas
        var builder = TarjetaFidelizacionBuilder.Nuevo(_notificationManagerMock.Object, _builderLoggerMock.Object);

        // Generar un número de tarjeta válido de 16 dígitos
        var numeroTarjeta = GenerarNumeroTarjeta(nivel);

        // Configurar el builder - SIEMPRE crear como activa inicialmente
        var tarjetaResult = await builder
            .ParaCliente(clienteId)
            .ConNumero(numeroTarjeta)
            .ConNivel(nivel)
            .ConEstado(true) // Siempre crear como activa primero
            .ConstruirAsync(); // No agregar puntos iniciales aquí

        // Verificar que la construcción fue exitosa
        tarjetaResult.IsSuccess().Should().BeTrue($"Error construyendo tarjeta: {tarjetaResult.ErrorMessage()}");
        var tarjeta = tarjetaResult.Value;

        // Usar reflexión para establecer el ID deseado
        var idProperty = typeof(EntityBase).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(tarjeta, tarjetaId);
        }

        // Activar la tarjeta si no está activa
        if (tarjeta.Estado != EstadoTarjeta.Activa)
        {
            tarjeta.Activar();
        }

        // Agregar puntos si es necesario usando el método público
        if (puntosDisponibles > 0)
        {
            // Intentar usar el método público para agregar puntos
            try
            {
                // Agregar puntos usando un método público si existe
                var agregarPuntosMethod = typeof(TarjetaFidelizacion).GetMethod("AgregarPuntos", BindingFlags.Public | BindingFlags.Instance);
                if (agregarPuntosMethod != null)
                {
                    agregarPuntosMethod.Invoke(tarjeta, new object[] { puntosDisponibles, "Puntos de prueba" });
                }
                else
                {
                    // Fallback a reflexión para establecer los puntos directamente
                    var puntosDisponiblesField = typeof(TarjetaFidelizacion).GetField("_puntosDisponibles", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (puntosDisponiblesField != null)
                    {
                        puntosDisponiblesField.SetValue(tarjeta, puntosDisponibles);
                    }
                    
                    var puntosAcumuladosField = typeof(TarjetaFidelizacion).GetField("_puntosAcumulados", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (puntosAcumuladosField != null)
                    {
                        puntosAcumuladosField.SetValue(tarjeta, puntosDisponibles);
                    }
                }
            }
            catch (Exception)
            {
                // Si falla, usar reflexión directa
                var puntosDisponiblesField = typeof(TarjetaFidelizacion).GetField("_puntosDisponibles", BindingFlags.NonPublic | BindingFlags.Instance);
                if (puntosDisponiblesField != null)
                {
                    puntosDisponiblesField.SetValue(tarjeta, puntosDisponibles);
                }
                
                var puntosAcumuladosField = typeof(TarjetaFidelizacion).GetField("_puntosAcumulados", BindingFlags.NonPublic | BindingFlags.Instance);
                if (puntosAcumuladosField != null)
                {
                    puntosAcumuladosField.SetValue(tarjeta, puntosDisponibles);
                }
            }
        }

        // Cambiar estado si debe ser inactiva (usando métodos públicos si existen)
        if (!activa)
        {
            try
            {
                // Primero activar la tarjeta para poder suspenderla
                if (tarjeta.Estado != EstadoTarjeta.Activa)
                {
                    tarjeta.Activar();
                }
                
                // Luego suspender la tarjeta usando el método público
                tarjeta.Suspender("Tarjeta suspendida para pruebas");
            }
            catch (Exception)
            {
                // Fallback a reflexión directa si el método no existe o falla
                var estadoField = typeof(TarjetaFidelizacion).GetField("_estado", BindingFlags.NonPublic | BindingFlags.Instance);
                if (estadoField != null)
                {
                    estadoField.SetValue(tarjeta, EstadoTarjeta.Suspendida);
                }
            }
        }

        return tarjeta;
    }

    /// <summary>
    /// Genera un número de tarjeta válido según el nivel
    /// </summary>
    private static string GenerarNumeroTarjeta(NivelFidelizacion nivel)
    {
        var prefijo = nivel switch
        {
            NivelFidelizacion.Basico => "4001",
            NivelFidelizacion.Plata => "4002",
            NivelFidelizacion.Oro => "4003",
            NivelFidelizacion.Platino => "4004",
            _ => "4000"
        };

        // Generar 12 dígitos aleatorios para completar 16 total
        var random = new Random();
        var digitosAleatorios = string.Join("", Enumerable.Range(0, 12)
            .Select(_ => random.Next(0, 10).ToString()));

        return prefijo + digitosAleatorios;
    }

    #endregion
} 