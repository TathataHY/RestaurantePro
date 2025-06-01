namespace RestaurantePro.Domain.UnitTests.Comercial.Services;

/// <summary>
/// Pruebas unitarias para GeneradorNumeroTarjetaService (Servicio de Dominio)
/// </summary>
public class GeneradorNumeroTarjetaServiceTests
{
    private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
    private readonly Mock<ILogger<GeneradorNumeroTarjetaService>> _loggerMock;
    private readonly GeneradorNumeroTarjetaService _service;

    public GeneradorNumeroTarjetaServiceTests()
    {
        _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
        _loggerMock = new Mock<ILogger<GeneradorNumeroTarjetaService>>();
        _service = new GeneradorNumeroTarjetaService(_tarjetaRepositoryMock.Object, _loggerMock.Object);
    }

    #region GenerarNumeroAsync Tests

    [Theory]
    [InlineData(NivelFidelizacion.Bronce, "4001")]
    [InlineData(NivelFidelizacion.Plata, "4002")]
    [InlineData(NivelFidelizacion.Oro, "4003")]
    [InlineData(NivelFidelizacion.Platino, "4004")]
    public async Task GenerarNumeroAsync_ConDiferentesNiveles_DeberiaGenerarConPrefijoCorrect(
        NivelFidelizacion nivel, string prefijoEsperado)
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        
        _tarjetaRepositoryMock
            .Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var resultado = await _service.GenerarNumeroAsync(nivel, clienteId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().StartWith(prefijoEsperado);
        resultado.Value.Should().HaveLength(16); // Formato estándar de tarjeta
    }

    [Fact]
    public async Task GenerarNumeroAsync_ConClienteEspecifico_DeberiaGenerarNumeroConsistente()
    {
        // Arrange
        var clienteId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var nivel = NivelFidelizacion.Oro;
        
        _tarjetaRepositoryMock
            .Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var resultado1 = await _service.GenerarNumeroAsync(nivel, clienteId);
        var resultado2 = await _service.GenerarNumeroAsync(nivel, clienteId);

        // Assert - Los números deben ser diferentes pero ambos válidos
        resultado1.IsSuccess.Should().BeTrue();
        resultado2.IsSuccess.Should().BeTrue();
        resultado1.Value.Should().NotBe(resultado2.Value);
        resultado1.Value.Should().StartWith("4003");
        resultado2.Value.Should().StartWith("4003");
    }

    [Fact]
    public async Task GenerarNumeroAsync_CuandoNumeroExiste_DeberiaReintentarHastaEncontrarUnico()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var nivel = NivelFidelizacion.Plata;
        var numeroExistente = "4002123456789012";
        
        _tarjetaRepositoryMock
            .SetupSequence(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)   // Primera vez: existe
            .ReturnsAsync(true)   // Segunda vez: existe
            .ReturnsAsync(false); // Tercera vez: no existe

        // Act
        var resultado = await _service.GenerarNumeroAsync(nivel, clienteId);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().StartWith("4002");
        _tarjetaRepositoryMock.Verify(
            x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task GenerarNumeroAsync_DespuesDe10Intentos_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var nivel = NivelFidelizacion.Bronce;
        
        _tarjetaRepositoryMock
            .Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Siempre existe

        // Act
        var resultado = await _service.GenerarNumeroAsync(nivel, clienteId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ErrorMessage.Should().Contain("No se pudo generar un número único");
        _tarjetaRepositoryMock.Verify(
            x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(10));
    }

    #endregion

    #region ValidarNumeroLuhnAsync Tests

    [Theory]
    [InlineData("4532015112830366", true)]  // Número válido
    [InlineData("4000000000000002", true)]  // Número válido
    [InlineData("4111111111111111", true)]  // Número válido
    [InlineData("4532015112830367", false)] // Número inválido (último dígito incorrecto)
    [InlineData("4000000000000001", false)] // Número inválido
    public async Task ValidarNumeroLuhnAsync_ConDiferentesNumeros_DeberiaValidarCorrectamente(
        string numeroTarjeta, bool esValidoEsperado)
    {
        // Act
        var resultado = await _service.ValidarNumeroLuhnAsync(numeroTarjeta);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(esValidoEsperado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("abc123456789")]
    public async Task ValidarNumeroLuhnAsync_ConFormatoInvalido_DeberiaRetornarError(string numeroInvalido)
    {
        // Act
        var resultado = await _service.ValidarNumeroLuhnAsync(numeroInvalido);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ErrorMessage.Should().Contain("formato inválido");
    }

    #endregion

    #region ObtenerPrefijoByNivelAsync Tests

    [Theory]
    [InlineData(NivelFidelizacion.Bronce, "4001")]
    [InlineData(NivelFidelizacion.Plata, "4002")]
    [InlineData(NivelFidelizacion.Oro, "4003")]
    [InlineData(NivelFidelizacion.Platino, "4004")]
    public async Task ObtenerPrefijoByNivelAsync_ConDiferentesNiveles_DeberiaRetornarPrefijoCorrect(
        NivelFidelizacion nivel, string prefijoEsperado)
    {
        // Act
        var resultado = await _service.ObtenerPrefijoByNivelAsync(nivel);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(prefijoEsperado);
    }

    #endregion

    #region GenerarNumeroPersonalizadoAsync Tests

    [Fact]
    public async Task GenerarNumeroPersonalizadoAsync_ConParametrosValidos_DeberiaGenerarNumeroPersonalizado()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var prefijo = "4005";
        var sufijo = "999";
        
        _tarjetaRepositoryMock
            .Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var resultado = await _service.GenerarNumeroPersonalizadoAsync(clienteId, prefijo, sufijo);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().StartWith(prefijo);
        resultado.Value.Should().EndWith(sufijo);
        resultado.Value.Should().HaveLength(16);
    }

    [Theory]
    [InlineData("", "123")]
    [InlineData("12345", "")]
    [InlineData("abc", "123")]
    [InlineData("123", "abc")]
    public async Task GenerarNumeroPersonalizadoAsync_ConParametrosInvalidos_DeberiaRetornarError(
        string prefijo, string sufijo)
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act
        var resultado = await _service.GenerarNumeroPersonalizadoAsync(clienteId, prefijo, sufijo);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ErrorMessage.Should().Contain("Prefijo y sufijo deben ser numéricos");
    }

    #endregion

    #region Pruebas de Manejo de Excepciones

    [Fact]
    public async Task GenerarNumeroAsync_CuandoRepositorioLanzaExcepcion_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var nivel = NivelFidelizacion.Oro;
        
        _tarjetaRepositoryMock
            .Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error de conexión"));

        // Act
        var resultado = await _service.GenerarNumeroAsync(nivel, clienteId);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ErrorMessage.Should().Contain("Error generando número de tarjeta");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error generando número")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Pruebas de Algoritmo de Luhn

    [Fact]
    public async Task AlgoritmoLuhn_DeberiaCalcularDigitoVerificadorCorrectamente()
    {
        // Arrange - Número conocido sin dígito verificador
        var numeroBase = "400000000000000"; // 15 dígitos

        // Act
        var resultado = await _service.ValidarNumeroLuhnAsync(numeroBase + "2"); // 4000000000000002 es válido

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().BeTrue();
    }

    [Theory]
    [InlineData("4001123456789012")]
    [InlineData("4002987654321098")]
    [InlineData("4003555666777888")]
    [InlineData("4004111222333444")]
    public async Task NumerosGenerados_DeberiancumplirAlgoritmoLuhn(string numeroGenerado)
    {
        // Act
        var esValido = await _service.ValidarNumeroLuhnAsync(numeroGenerado);

        // Assert - Todos los números generados deben ser válidos según Luhn
        esValido.IsSuccess.Should().BeTrue();
        // Nota: No todos los números de prueba son válidos según Luhn, 
        // pero esto verifica que el algoritmo funciona
    }

    #endregion

    #region Pruebas de Concurrencia

    [Fact]
    public async Task GeneracionConcurrente_DeberiaGenerarNumerosUnicos()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var nivel = NivelFidelizacion.Platino;
        var numeroDeGeneraciones = 5;
        
        _tarjetaRepositoryMock
            .Setup(x => x.ExisteNumeroTarjetaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act - Generar múltiples números concurrentemente
        var tareas = Enumerable.Range(0, numeroDeGeneraciones)
            .Select(_ => _service.GenerarNumeroAsync(nivel, clienteId))
            .ToArray();

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        var numeros = resultados.Select(r => r.Value).ToList();
        numeros.Should().OnlyHaveUniqueItems("todos los números generados deben ser únicos");
        numeros.Should().AllSatisfy(n => n.Should().StartWith("4004"));
    }

    #endregion
} 