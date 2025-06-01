namespace RestaurantePro.Domain.UnitTests.Operaciones.Services;

/// <summary>
/// Pruebas unitarias para GeneradorNumeroComandaService (Servicio de Dominio)
/// </summary>
public class GeneradorNumeroComandaServiceTests
{
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<ILogger<GeneradorNumeroComandaService>> _loggerMock;
    private readonly GeneradorNumeroComandaService _service;

    public GeneradorNumeroComandaServiceTests()
    {
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _loggerMock = new Mock<ILogger<GeneradorNumeroComandaService>>();
        _service = new GeneradorNumeroComandaService(_comandaRepositoryMock.Object, _loggerMock.Object);
    }

    #region GenerarNumeroAsync Tests

    [Fact]
    public async Task GenerarNumeroAsync_ConParametrosBasicos_DeberiaGenerarNumeroCorrectamente()
    {
        // Arrange
        var sucursalId = Guid.NewGuid();
        var fecha = new DateTime(2024, 1, 15);
        var tipoComanda = TipoComanda.Mesa;
        var canalOrden = CanalOrden.Web;
        var numeroMesa = 5;

        _comandaRepositoryMock
            .Setup(x => x.ObtenerUltimoSecuencialAsync(sucursalId, fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var resultado = await _service.GenerarNumeroAsync(sucursalId, fecha, tipoComanda, canalOrden, numeroMesa);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().NotBeNullOrEmpty();
        // Formato esperado: SUCURSAL_ID + 20240115 + MSA + 005 + W + 0001
        resultado.Value.Should().Contain("20240115"); // Fecha
        resultado.Value.Should().Contain("MSA"); // Tipo Mesa
        resultado.Value.Should().Contain("005"); // Mesa
        resultado.Value.Should().Contain("W"); // Canal Web
        resultado.Value.Should().EndWith("0001"); // Secuencial
    }

    [Theory]
    [InlineData(TipoComanda.Delivery, "DLV")]
    [InlineData(TipoComanda.TakeAway, "TKW")]
    [InlineData(TipoComanda.Mesa, "MSA")]
    public async Task GenerarNumeroAsync_ConDiferentesTipos_DeberiaUsarAbreviaturaCorrecta(
        TipoComanda tipo, string abreviaturaEsperada)
    {
        // Arrange
        var sucursalId = Guid.NewGuid();
        var fecha = DateTime.Today;
        var canalOrden = CanalOrden.Movil;

        _comandaRepositoryMock
            .Setup(x => x.ObtenerUltimoSecuencialAsync(sucursalId, fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var resultado = await _service.GenerarNumeroAsync(sucursalId, fecha, tipo, canalOrden);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().Contain(abreviaturaEsperada);
    }

    [Theory]
    [InlineData(CanalOrden.Web, "W")]
    [InlineData(CanalOrden.Movil, "M")]
    [InlineData(CanalOrden.Telefono, "T")]
    [InlineData(CanalOrden.Presencial, "P")]
    public async Task GenerarNumeroAsync_ConDiferentesCanales_DeberiaUsarAbreviaturaCorrecta(
        CanalOrden canal, string abreviaturaEsperada)
    {
        // Arrange
        var sucursalId = Guid.NewGuid();
        var fecha = DateTime.Today;
        var tipoComanda = TipoComanda.TakeAway;

        _comandaRepositoryMock
            .Setup(x => x.ObtenerUltimoSecuencialAsync(sucursalId, fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var resultado = await _service.GenerarNumeroAsync(sucursalId, fecha, tipoComanda, canal);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().Contain(abreviaturaEsperada);
    }

    [Fact]
    public async Task GenerarNumeroAsync_ConSecuencialExistente_DeberiaIncrementarCorrectamente()
    {
        // Arrange
        var sucursalId = Guid.NewGuid();
        var fecha = DateTime.Today;
        var tipoComanda = TipoComanda.Mesa;
        var canalOrden = CanalOrden.Presencial;

        _comandaRepositoryMock
            .Setup(x => x.ObtenerUltimoSecuencialAsync(sucursalId, fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(42); // Ya existen 42 comandas

        // Act
        var resultado = await _service.GenerarNumeroAsync(sucursalId, fecha, tipoComanda, canalOrden);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().EndWith("0043"); // Secuencial 43
    }

    [Fact]
    public async Task GenerarNumeroAsync_ConNumeroMesaGrande_DeberiaFormatearCorrectamente()
    {
        // Arrange
        var sucursalId = Guid.NewGuid();
        var fecha = DateTime.Today;
        var tipoComanda = TipoComanda.Mesa;
        var canalOrden = CanalOrden.Presencial;
        var numeroMesa = 125;

        _comandaRepositoryMock
            .Setup(x => x.ObtenerUltimoSecuencialAsync(sucursalId, fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var resultado = await _service.GenerarNumeroAsync(sucursalId, fecha, tipoComanda, canalOrden, numeroMesa);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().Contain("125"); // Mesa sin padding extra si >100
    }

    #endregion

    #region GenerarNumeroConPrefijoAsync Tests

    [Fact]
    public async Task GenerarNumeroConPrefijoAsync_ConPrefijoPersonalizado_DeberiaIncluirPrefijo()
    {
        // Arrange
        var prefijo = "VIP";
        var sucursalId = Guid.NewGuid();
        var fecha = new DateTime(2024, 12, 25);
        var tipoComanda = TipoComanda.Mesa;
        var canalOrden = CanalOrden.Web;

        _comandaRepositoryMock
            .Setup(x => x.ObtenerUltimoSecuencialAsync(sucursalId, fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        // Act
        var resultado = await _service.GenerarNumeroConPrefijoAsync(prefijo, sucursalId, fecha, tipoComanda, canalOrden);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().StartWith("VIP-");
        resultado.Value.Should().Contain("20241225");
        resultado.Value.Should().EndWith("0008");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task GenerarNumeroConPrefijoAsync_ConPrefijoInvalido_DeberiaRetornarError(string prefijoInvalido)
    {
        // Arrange
        var sucursalId = Guid.NewGuid();
        var fecha = DateTime.Today;
        var tipoComanda = TipoComanda.Mesa;
        var canalOrden = CanalOrden.Web;

        // Act
        var resultado = await _service.GenerarNumeroConPrefijoAsync(prefijoInvalido, sucursalId, fecha, tipoComanda, canalOrden);

        // Assert
        resultado.IsSuccess().Should().BeFalse();
        resultado.ErrorMessage().Should().Contain("Prefijo no puede estar vacío");
    }

    #endregion

    #region ObtenerFormatoNumeroAsync Tests

    [Fact]
    public async Task ObtenerFormatoNumeroAsync_ConParametrosCompletos_DeberiaRetornarFormatoPrevisto()
    {
        // Arrange
        var sucursalId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var fecha = new DateTime(2024, 6, 10);
        var tipoComanda = TipoComanda.Delivery;
        var canalOrden = CanalOrden.Telefono;
        var numeroMesa = 15;
        var secuencial = 99;

        // Act
        var resultado = await _service.ObtenerFormatoNumeroAsync(
            sucursalId, fecha, tipoComanda, canalOrden, numeroMesa, secuencial);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        var formato = resultado.Value;
        formato.Should().Contain(sucursalId.ToString("N")[..8]); // Primeros 8 caracteres del GUID
        formato.Should().Contain("20240610"); // Fecha
        formato.Should().Contain("DLV"); // Delivery
        formato.Should().Contain("015"); // Mesa formateada
        formato.Should().Contain("T"); // Teléfono
        formato.Should().Contain("0099"); // Secuencial
    }

    [Fact]
    public async Task ObtenerFormatoNumeroAsync_SinMesa_DeberiaOmitirNumeroMesa()
    {
        // Arrange
        var sucursalId = Guid.NewGuid();
        var fecha = DateTime.Today;
        var tipoComanda = TipoComanda.TakeAway;
        var canalOrden = CanalOrden.Movil;
        var secuencial = 1;

        // Act
        var resultado = await _service.ObtenerFormatoNumeroAsync(
            sucursalId, fecha, tipoComanda, canalOrden, null, secuencial);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().NotContain("000"); // No debería tener número de mesa
        resultado.Value.Should().Contain("TKW");
        resultado.Value.Should().Contain("M");
    }

    #endregion

    #region ValidarNumeroComandaAsync Tests

    [Fact]
    public async Task ValidarNumeroComandaAsync_ConNumeroValido_DeberiaRetornarTrue()
    {
        // Arrange
        var numeroComanda = "12345678-20240115-MSA-005-W-0001";

        // Act
        var resultado = await _service.ValidarNumeroComandaAsync(numeroComanda);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("FORMATO-INCORRECTO")]
    [InlineData("12345678-20240115-INVALID-005-W-0001")]
    [InlineData("12345678-20240115-MSA-005-Z-0001")] // Canal inválido
    public async Task ValidarNumeroComandaAsync_ConNumeroInvalido_DeberiaRetornarFalse(string numeroInvalido)
    {
        // Act
        var resultado = await _service.ValidarNumeroComandaAsync(numeroInvalido);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().BeFalse();
    }

    #endregion

    #region ExtraerInformacionNumeroAsync Tests

    [Fact]
    public async Task ExtraerInformacionNumeroAsync_ConNumeroValido_DeberiaExtraerInformacionCorrectamente()
    {
        // Arrange
        var numeroComanda = "ABCD1234-20240315-DLV-010-M-0042";

        // Act
        var resultado = await _service.ExtraerInformacionNumeroAsync(numeroComanda);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        var info = resultado.Value;
        info.Should().NotBeNull();
        info.SucursalId.Should().Be("ABCD1234");
        info.Fecha.Should().Be(new DateTime(2024, 3, 15));
        info.TipoComanda.Should().Be("DLV");
        info.NumeroMesa.Should().Be(10);
        info.CanalOrden.Should().Be("M");
        info.Secuencial.Should().Be(42);
    }

    [Fact]
    public async Task ExtraerInformacionNumeroAsync_ConNumeroSinMesa_DeberiaExtraerCorrectamente()
    {
        // Arrange
        var numeroComanda = "12345678-20241201-TKW-P-0001";

        // Act
        var resultado = await _service.ExtraerInformacionNumeroAsync(numeroComanda);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        var info = resultado.Value;
        info.NumeroMesa.Should().BeNull();
        info.TipoComanda.Should().Be("TKW");
        info.CanalOrden.Should().Be("P");
    }

    #endregion

    #region Pruebas de Concurrencia

    [Fact]
    public async Task GeneracionConcurrente_DeBeriGenerirnumeroSecuencialesUnicos()
    {
        // Arrange
        var sucursalId = Guid.NewGuid();
        var fecha = DateTime.Today;
        var tipoComanda = TipoComanda.Mesa;
        var canalOrden = CanalOrden.Web;
        var numeroGeneraciones = 10;

        // Simular secuenciales incrementales
        var secuencial = 0;
        _comandaRepositoryMock
            .Setup(x => x.ObtenerUltimoSecuencialAsync(sucursalId, fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Interlocked.Increment(ref secuencial) - 1);

        // Act - Generar múltiples números concurrentemente
        var tareas = Enumerable.Range(0, numeroGeneraciones)
            .Select(_ => _service.GenerarNumeroAsync(sucursalId, fecha, tipoComanda, canalOrden))
            .ToArray();

        var resultados = await Task.WhenAll(tareas);

        // Assert
        resultados.Should().AllSatisfy(r => r.IsSuccess().Should().BeTrue());
        var numeros = resultados.Select(r => r.Value).ToList();
        numeros.Should().OnlyHaveUniqueItems("todos los números generados deben ser únicos");
    }

    #endregion

    #region Pruebas de Manejo de Excepciones

    [Fact]
    public async Task GenerarNumeroAsync_CuandoRepositorioLanzaExcepcion_DeberiaRetornarError()
    {
        // Arrange
        var sucursalId = Guid.NewGuid();
        var fecha = DateTime.Today;
        var tipoComanda = TipoComanda.Mesa;
        var canalOrden = CanalOrden.Web;

        _comandaRepositoryMock
            .Setup(x => x.ObtenerUltimoSecuencialAsync(sucursalId, fecha, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error de conexión"));

        // Act
        var resultado = await _service.GenerarNumeroAsync(sucursalId, fecha, tipoComanda, canalOrden);

        // Assert
        resultado.IsSuccess().Should().BeFalse();
        resultado.ErrorMessage().Should().Contain("Error generando número de comanda");
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

    #region Pruebas de Formato y Validación

    [Theory]
    [InlineData(1, "0001")]
    [InlineData(42, "0042")]
    [InlineData(999, "0999")]
    [InlineData(9999, "9999")]
    [InlineData(12345, "12345")] // Más de 4 dígitos
    public async Task FormateoSecuencial_DeberiaFormatearCorrectamente(int secuencial, string esperado)
    {
        // Arrange
        var sucursalId = Guid.NewGuid();
        var fecha = DateTime.Today;
        var tipoComanda = TipoComanda.Mesa;
        var canalOrden = CanalOrden.Web;

        _comandaRepositoryMock
            .Setup(x => x.ObtenerUltimoSecuencialAsync(sucursalId, fecha, It.IsAny<CancellationToken>()))
            .ReturnsAsync(secuencial - 1);

        // Act
        var resultado = await _service.GenerarNumeroAsync(sucursalId, fecha, tipoComanda, canalOrden);

        // Assert
        resultado.IsSuccess().Should().BeTrue();
        resultado.Value.Should().EndWith(esperado);
    }

    [Fact]
    public async Task GenerarNumero_ConDiferentesFechas_DeberiaReiniciarSecuencial()
    {
        // Arrange
        var sucursalId = Guid.NewGuid();
        var fecha1 = new DateTime(2024, 1, 1);
        var fecha2 = new DateTime(2024, 1, 2);
        var tipoComanda = TipoComanda.Mesa;
        var canalOrden = CanalOrden.Web;

        _comandaRepositoryMock
            .Setup(x => x.ObtenerUltimoSecuencialAsync(sucursalId, fecha1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        _comandaRepositoryMock
            .Setup(x => x.ObtenerUltimoSecuencialAsync(sucursalId, fecha2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0); // Nuevo día, nuevo secuencial

        // Act
        var resultado1 = await _service.GenerarNumeroAsync(sucursalId, fecha1, tipoComanda, canalOrden);
        var resultado2 = await _service.GenerarNumeroAsync(sucursalId, fecha2, tipoComanda, canalOrden);

        // Assert
        resultado1.Value.Should().EndWith("0101"); // Secuencial 101
        resultado2.Value.Should().EndWith("0001"); // Secuencial reiniciado
    }

    #endregion
} 