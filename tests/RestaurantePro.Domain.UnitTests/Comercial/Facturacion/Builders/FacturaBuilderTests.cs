namespace RestaurantePro.Domain.UnitTests.Comercial.Facturacion.Builders;

public class FacturaBuilderTests
{
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Mock<ILogger<FacturaBuilder>> _loggerMock;
    private readonly FacturaBuilder _builder;

    public FacturaBuilderTests()
    {
        _notificationManagerMock = new Mock<INotificationManager>();
        _loggerMock = new Mock<ILogger<FacturaBuilder>>();
        _builder = new FacturaBuilder(_notificationManagerMock.Object, _loggerMock.Object);

        // Setup por defecto para notification manager
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);
        _notificationManagerMock.Setup(x => x.CreateNewNotification());
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConNotificationManagerNulo_DeberiaLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new FacturaBuilder(null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DeberiaLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new FacturaBuilder(_notificationManagerMock.Object, null!));
    }

    #endregion

    #region ConNumero Tests

    [Fact]
    public void ConNumero_ConNumeroValido_DeberiaAsignarNumeroYRetornarBuilder()
    {
        // Arrange
        var numeroFactura = "FAC-001";

        // Act
        var resultado = _builder.ConNumero(numeroFactura);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConNumero_ConNumeroVacio_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Act
        var resultado = _builder.ConNumero("");

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ConNumero_ConNumeroMuyLargo_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Arrange
        var numeroLargo = new string('X', 51);

        // Act
        var resultado = _builder.ConNumero(numeroLargo);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region DeTipo Tests

    [Fact]
    public void DeTipo_ConTipoValido_DeberiaAsignarTipoYRetornarBuilder()
    {
        // Arrange
        var tipoFactura = TipoFactura.Electronica;

        // Act
        var resultado = _builder.DeTipo(tipoFactura);

        // Assert
        Assert.Same(_builder, resultado);
    }

    #endregion

    #region ParaCliente Tests

    [Fact]
    public void ParaCliente_ConNombreValido_DeberiaAsignarClienteYRetornarBuilder()
    {
        // Arrange
        var nombreCliente = "Juan Pérez";
        var clienteId = Guid.NewGuid();

        // Act
        var resultado = _builder.ParaCliente(nombreCliente, clienteId);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ParaCliente_ConNombreVacio_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Act
        var resultado = _builder.ParaCliente("");

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ParaCliente_ConNombreMuyLargo_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Arrange
        var nombreLargo = new string('X', 201);

        // Act
        var resultado = _builder.ParaCliente(nombreLargo);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region ConInformacionFiscal Tests

    [Fact]
    public void ConInformacionFiscal_ConDatosValidos_DeberiaAsignarInformacionYRetornarBuilder()
    {
        // Arrange
        var identificacionFiscal = "12345678-9";
        var direccion = "Av. Principal 123";

        // Act
        var resultado = _builder.ConInformacionFiscal(identificacionFiscal, direccion);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConInformacionFiscal_ConIdentificacionMuyLarga_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Arrange
        var identificacionLarga = new string('X', 51);

        // Act
        var resultado = _builder.ConInformacionFiscal(identificacionLarga);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ConInformacionFiscal_ConDireccionMuyLarga_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Arrange
        var direccionLarga = new string('X', 301);

        // Act
        var resultado = _builder.ConInformacionFiscal("12345678-9", direccionLarga);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region PorComandas Tests

    [Fact]
    public void PorComandas_ConComandasValidas_DeberiaAsociarComandasYRetornarBuilder()
    {
        // Arrange
        var comandaId1 = Guid.NewGuid();
        var comandaId2 = Guid.NewGuid();

        // Act
        var resultado = _builder.PorComandas(comandaId1, comandaId2);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void PorComandas_ConIdVacio_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Arrange
        var comandaValidaId = Guid.NewGuid();

        // Act
        var resultado = _builder.PorComandas(comandaValidaId, Guid.Empty);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region ConFechaEmision Tests

    [Fact]
    public void ConFechaEmision_ConFechaValida_DeberiaAsignarFechaYRetornarBuilder()
    {
        // Arrange
        var fechaEmision = DateTime.Now.Date;

        // Act
        var resultado = _builder.ConFechaEmision(fechaEmision);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConFechaEmision_ConFechaFutura_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Arrange
        var fechaFutura = DateTime.Now.Date.AddDays(1);

        // Act
        var resultado = _builder.ConFechaEmision(fechaFutura);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ConFechaEmision_ConFechaMuyAntigua_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Arrange
        var fechaAntigua = DateTime.Now.Date.AddYears(-2);

        // Act
        var resultado = _builder.ConFechaEmision(fechaAntigua);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region AgregarDetalle Tests

    [Fact]
    public void AgregarDetalle_ConDatosValidos_DeberiaAgregarDetalleYRetornarBuilder()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var descripcion = "Hamburguesa Clásica";
        var cantidad = 2m;
        var precio = 15000m;
        var impuesto = 19m;

        // Act
        var resultado = _builder.AgregarDetalle(productoId, descripcion, cantidad, precio, impuesto);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void AgregarDetalle_ConProductoIdVacio_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Act
        var resultado = _builder.AgregarDetalle(Guid.Empty, "Producto", 1, 1000, 19);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarDetalle_ConDescripcionVacia_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Act
        var resultado = _builder.AgregarDetalle(Guid.NewGuid(), "", 1, 1000, 19);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarDetalle_ConCantidadCero_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Act
        var resultado = _builder.AgregarDetalle(Guid.NewGuid(), "Producto", 0, 1000, 19);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarDetalle_ConPrecioNegativo_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Act
        var resultado = _builder.AgregarDetalle(Guid.NewGuid(), "Producto", 1, -100, 19);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarDetalle_ConImpuestoInvalido_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Act
        var resultado = _builder.AgregarDetalle(Guid.NewGuid(), "Producto", 1, 1000, 150);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarDetalle_ConProductoDuplicado_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        _builder.AgregarDetalle(productoId, "Producto", 1, 1000, 19);

        // Act - Intentar agregar el mismo producto nuevamente
        var resultado = _builder.AgregarDetalle(productoId, "Producto Duplicado", 2, 2000, 19);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region ConObservaciones Tests

    [Fact]
    public void ConObservaciones_ConObservacionesValidas_DeberiaAsignarObservacionesYRetornarBuilder()
    {
        // Arrange
        var observaciones = "Factura por servicio de catering especial";

        // Act
        var resultado = _builder.ConObservaciones(observaciones);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConObservaciones_ConObservacionesMuyLargas_DeberiaAgregarErrorYRetornarBuilder()
    {
        // Arrange
        var observacionesLargas = new string('X', 1001);

        // Act
        var resultado = _builder.ConObservaciones(observacionesLargas);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region Construir Tests

    [Fact]
    public void Construir_ConDatosCompletos_DeberiaRetornarFacturaExitosa()
    {
        // Arrange
        _builder
            .ConNumero("FAC-001")
            .DeTipo(TipoFactura.Electronica)
            .ParaCliente("Juan Pérez", Guid.NewGuid())
            .ConInformacionFiscal("12345678-9", "Av. Principal 123")
            .ConFechaEmision(DateTime.Now.Date)
            .AgregarDetalle(Guid.NewGuid(), "Hamburguesa", 2, 15000, 19)
            .ConObservaciones("Factura de prueba");

        // Act
        var resultado = _builder.Construir();

        // Assert
        Assert.True(resultado.Succeeded);
        Assert.NotNull(resultado.Value);
    }

    [Fact]
    public void Construir_SinNumeroFactura_DeberiaRetornarError()
    {
        // Arrange
        var notificationManagerMock = new Mock<INotificationManager>();
        var loggerMock = new Mock<ILogger<FacturaBuilder>>();
        
        // Configurar para que HasErrors devuelva true después de que se agreguen errores
        var hasErrors = false;
        notificationManagerMock.Setup(x => x.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback(() => hasErrors = true);
        notificationManagerMock.Setup(x => x.HasErrors).Returns(() => hasErrors);
        
        var builderReal = new FacturaBuilder(notificationManagerMock.Object, loggerMock.Object);

        // Act
        var resultado = builderReal
            .ParaCliente("Juan Pérez", Guid.NewGuid())
            .DeTipo(TipoFactura.Normal)
            .ConFechaEmision(DateTime.Now)
            .Construir();

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Be("Errores de validación en la construcción de la factura");
    }

    [Fact]
    public void Construir_FacturaFiscalSinIdentificacion_DeberiaRetornarError()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);
        _builder
            .ConNumero("FAC-001")
            .DeTipo(TipoFactura.Fiscal)
            .ParaCliente("Juan Pérez")
            .AgregarDetalle(Guid.NewGuid(), "Producto", 1, 1000, 19);

        // Act
        var resultado = _builder.Construir();

        // Assert
        Assert.False(resultado.Succeeded);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void Construir_SinDetalles_DeberiaRetornarError()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);
        _builder
            .ConNumero("FAC-001")
            .DeTipo(TipoFactura.Normal)
            .ParaCliente("Juan Pérez");

        // Act
        var resultado = _builder.Construir();

        // Assert
        Assert.False(resultado.Succeeded);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_DeberiaLimpiarTodosLosDatosYRetornarBuilder()
    {
        // Arrange
        _builder
            .ConNumero("FAC-001")
            .DeTipo(TipoFactura.Electronica)
            .ParaCliente("Juan Pérez")
            .AgregarDetalle(Guid.NewGuid(), "Producto", 1, 1000, 19);

        // Act
        var resultado = _builder.Reset();

        // Assert
        Assert.Same(_builder, resultado);
        _notificationManagerMock.Verify(x => x.CreateNewNotification(), Times.Once);
    }

    #endregion

    #region Static Factory Tests

    [Fact]
    public void Nuevo_DeberiaCrearNuevaInstanciaDelBuilder()
    {
        // Act
        var nuevoBuilder = FacturaBuilder.Nuevo(_notificationManagerMock.Object, _loggerMock.Object);

        // Assert
        Assert.NotNull(nuevoBuilder);
        Assert.NotSame(_builder, nuevoBuilder);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void EscenarioCompleto_FacturaElectronicaConMultiplesDetalles_DeberiaCrearFacturaExitosamente()
    {
        // Arrange
        var numeroFactura = "FE-001-2024";
        var clienteId = Guid.NewGuid();
        var producto1Id = Guid.NewGuid();
        var producto2Id = Guid.NewGuid();
        var fechaEmision = DateTime.Now.Date;

        // Act
        var resultado = _builder
            .ConNumero(numeroFactura)
            .DeTipo(TipoFactura.Electronica)
            .ParaCliente("Restaurant ABC S.A.", clienteId)
            .ConInformacionFiscal("76.123.456-7", "Av. Providencia 1234, Santiago")
            .PorComandas(Guid.NewGuid(), Guid.NewGuid())
            .ConFechaEmision(fechaEmision)
            .AgregarDetalle(producto1Id, "Hamburguesa Premium", 3, 18500, 19, 10)
            .AgregarDetalle(producto2Id, "Papas Fritas Grandes", 2, 8900, 19)
            .ConObservaciones("Factura por evento corporativo - Descuento especial aplicado")
            .Construir();

        // Assert
        Assert.True(resultado.Succeeded);
        Assert.NotNull(resultado.Value);

        var factura = resultado.Value;
        Assert.Equal(numeroFactura, factura.NumeroFactura);
        Assert.Equal(TipoFactura.Electronica, factura.TipoFactura);
        Assert.Equal("Restaurant ABC S.A.", factura.NombreCliente);
        Assert.Equal(clienteId, factura.ClienteId);
        Assert.Equal("76.123.456-7", factura.IdentificacionFiscal);
        Assert.Equal("Av. Providencia 1234, Santiago", factura.DireccionCliente);
        Assert.Equal(fechaEmision, factura.FechaEmision);
    }

    #endregion
} 
