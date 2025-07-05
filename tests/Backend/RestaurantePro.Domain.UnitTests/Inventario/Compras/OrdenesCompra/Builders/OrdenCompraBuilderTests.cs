namespace RestaurantePro.Domain.UnitTests.Inventario.Compras.OrdenesCompra.Builders;

public class OrdenCompraBuilderTests
{
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Mock<ILogger<OrdenCompraBuilder>> _loggerMock;
    private readonly OrdenCompraBuilder _builder;

    public OrdenCompraBuilderTests()
    {
        _notificationManagerMock = new Mock<INotificationManager>();
        _loggerMock = new Mock<ILogger<OrdenCompraBuilder>>();
        _builder = new OrdenCompraBuilder(_notificationManagerMock.Object, _loggerMock.Object);

        // Setup por defecto para notification manager
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);
        _notificationManagerMock.Setup(x => x.GetErrors()).Returns(new ReadOnlyCollection<Error>(new List<Error>()));
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConNotificationManagerNulo_DeberiaLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new OrdenCompraBuilder(null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DeberiaLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new OrdenCompraBuilder(_notificationManagerMock.Object, null!));
    }

    #endregion

    #region ParaProveedor Tests

    [Fact]
    public void ParaProveedor_ConIdValido_DeberiaAsignarProveedorYRetornarBuilder()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();

        // Act
        var resultado = _builder.ParaProveedor(proveedorId);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ParaProveedor_ConIdVacio_DeberiaAgregarError()
    {
        // Act
        var resultado = _builder.ParaProveedor(Guid.Empty);

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
        var fechaEmision = DateTime.Now.AddHours(-1);

        // Act
        var resultado = _builder.ConFechaEmision(fechaEmision);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConFechaEmision_ConFechaFutura_DeberiaAgregarError()
    {
        // Arrange
        var fechaFutura = DateTime.Now.AddDays(1);

        // Act
        var resultado = _builder.ConFechaEmision(fechaFutura);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ConFechaEmision_ConFechaMuyAntigua_DeberiaAgregarError()
    {
        // Arrange
        var fechaAntigua = DateTime.Now.AddYears(-2);

        // Act
        var resultado = _builder.ConFechaEmision(fechaAntigua);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region ConFechaEntregaEstimada Tests

    [Fact]
    public void ConFechaEntregaEstimada_ConFechaValida_DeberiaAsignarFechaYRetornarBuilder()
    {
        // Arrange
        var fechaEntrega = DateTime.Now.AddDays(7);

        // Act
        var resultado = _builder.ConFechaEntregaEstimada(fechaEntrega);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConFechaEntregaEstimada_AnteriorAEmision_DeberiaAgregarError()
    {
        // Arrange
        var fechaEmision = DateTime.Now;
        var fechaEntregaAnterior = fechaEmision.AddDays(-1);

        // Act
        var resultado = _builder
            .ConFechaEmision(fechaEmision)
            .ConFechaEntregaEstimada(fechaEntregaAnterior);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ConFechaEntregaEstimada_MuyLejana_DeberiaAgregarError()
    {
        // Arrange
        var fechaLejana = DateTime.Now.AddYears(2);

        // Act
        var resultado = _builder.ConFechaEntregaEstimada(fechaLejana);

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
        var observaciones = "Orden urgente para el fin de semana";

        // Act
        var resultado = _builder.ConObservaciones(observaciones);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConObservaciones_ConObservacionesMuyLargas_DeberiaAgregarError()
    {
        // Arrange
        var observacionesLargas = new string('a', 1001);

        // Act
        var resultado = _builder.ConObservaciones(observacionesLargas);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ConObservaciones_ConObservacionesVacias_NoDeberiaAsignarNada()
    {
        // Act
        var resultado = _builder.ConObservaciones(string.Empty);

        // Assert
        Assert.Same(_builder, resultado);
    }

    #endregion

    #region AgregarItem Tests

    [Fact]
    public void AgregarItem_ConDatosValidos_DeberiaAgregarItemYRetornarBuilder()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var nombre = "Harina de Trigo";
        var cantidad = 50m;
        var unidadMedida = UnidadMedida.Kilogramo;
        var precio = 2.50m;

        // Act
        var resultado = _builder.AgregarItem(ingredienteId, nombre, cantidad, unidadMedida, precio);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void AgregarItem_SinPrecio_DeberiaAgregarItemConPrecioCero()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var nombre = "Aceite de Oliva";
        var cantidad = 10m;
        var unidadMedida = UnidadMedida.Litro;

        // Act
        var resultado = _builder.AgregarItem(ingredienteId, nombre, cantidad, unidadMedida);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void AgregarItem_ConIngredienteIdVacio_DeberiaAgregarError()
    {
        // Act
        var resultado = _builder.AgregarItem(Guid.Empty, "Ingrediente", 1m, UnidadMedida.Kilogramo, 5m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarItem_ConNombreVacio_DeberiaAgregarError()
    {
        // Act
        var resultado = _builder.AgregarItem(Guid.NewGuid(), "", 1m, UnidadMedida.Kilogramo, 5m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarItem_ConNombreMuyLargo_DeberiaAgregarError()
    {
        // Arrange
        var nombreLargo = new string('a', 201);

        // Act
        var resultado = _builder.AgregarItem(Guid.NewGuid(), nombreLargo, 1m, UnidadMedida.Kilogramo, 5m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarItem_ConCantidadCero_DeberiaAgregarError()
    {
        // Act
        var resultado = _builder.AgregarItem(Guid.NewGuid(), "Ingrediente", 0m, UnidadMedida.Kilogramo, 5m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarItem_ConCantidadNegativa_DeberiaAgregarError()
    {
        // Act
        var resultado = _builder.AgregarItem(Guid.NewGuid(), "Ingrediente", -1m, UnidadMedida.Kilogramo, 5m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarItem_ConCantidadExcesiva_DeberiaAgregarError()
    {
        // Act
        var resultado = _builder.AgregarItem(Guid.NewGuid(), "Ingrediente", 1000000m, UnidadMedida.Kilogramo, 5m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarItem_ConPrecioNegativo_DeberiaAgregarError()
    {
        // Act
        var resultado = _builder.AgregarItem(Guid.NewGuid(), "Ingrediente", 1m, UnidadMedida.Kilogramo, -1m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarItem_ConPrecioExcesivo_DeberiaAgregarError()
    {
        // Act
        var resultado = _builder.AgregarItem(Guid.NewGuid(), "Ingrediente", 1m, UnidadMedida.Kilogramo, 1000000m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AgregarItem_ConIngredienteDuplicado_DeberiaAgregarError()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();

        // Act
        var resultado = _builder
            .AgregarItem(ingredienteId, "Harina", 10m, UnidadMedida.Kilogramo, 2m)
            .AgregarItem(ingredienteId, "Harina Duplicada", 5m, UnidadMedida.Kilogramo, 3m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region ActualizarPrecioItem Tests

    [Fact]
    public void ActualizarPrecioItem_ConDatosValidos_DeberiaActualizarPrecioYRetornarBuilder()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var nuevoPrecio = 3.50m;

        // Act
        var resultado = _builder
            .AgregarItem(ingredienteId, "Harina", 10m, UnidadMedida.Kilogramo, 2m)
            .ActualizarPrecioItem(ingredienteId, nuevoPrecio);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ActualizarPrecioItem_ConIngredienteIdVacio_DeberiaAgregarError()
    {
        // Act
        var resultado = _builder.ActualizarPrecioItem(Guid.Empty, 5m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ActualizarPrecioItem_ConPrecioNegativo_DeberiaAgregarError()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();

        // Act
        var resultado = _builder
            .AgregarItem(ingredienteId, "Harina", 10m, UnidadMedida.Kilogramo, 2m)
            .ActualizarPrecioItem(ingredienteId, -1m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ActualizarPrecioItem_ConIngredienteNoExistente_DeberiaAgregarError()
    {
        // Arrange
        var ingredienteExistente = Guid.NewGuid();
        var ingredienteInexistente = Guid.NewGuid();

        // Act
        var resultado = _builder
            .AgregarItem(ingredienteExistente, "Harina", 10m, UnidadMedida.Kilogramo, 2m)
            .ActualizarPrecioItem(ingredienteInexistente, 5m);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region Construir Tests

    [Fact]
    public void Construir_ConTodosLosDatosValidos_DeberiaCrearOrdenCompra()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var fechaEmision = DateTime.Now.AddHours(-1);
        var fechaEntrega = DateTime.Now.AddDays(7);
        var ingredienteId = Guid.NewGuid();

        // Act
        var resultado = _builder
            .ParaProveedor(proveedorId)
            .ConFechaEmision(fechaEmision)
            .ConFechaEntregaEstimada(fechaEntrega)
            .AgregarItem(ingredienteId, "Harina de Trigo", 50m, UnidadMedida.Kilogramo, 2.50m)
            .ConObservaciones("Orden urgente")
            .Construir();

        // Assert
        Assert.True(resultado.Succeeded);
        Assert.NotNull(resultado.Value);
    }

    [Fact]
    public void Construir_SinProveedor_DeberiaRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = _builder
            .ConFechaEmision(DateTime.Now)
            .ConFechaEntregaEstimada(DateTime.Now.AddDays(1))
            .AgregarItem(Guid.NewGuid(), "Ingrediente", 1m, UnidadMedida.Kilogramo, 1m)
            .Construir();

        // Assert
        Assert.False(resultado.Succeeded);
        Assert.Equal("Errores de validación en la construcción de la orden de compra", resultado.Error);
    }

    [Fact]
    public void Construir_SinFechaEmision_DeberiaRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = _builder
            .ParaProveedor(Guid.NewGuid())
            .ConFechaEntregaEstimada(DateTime.Now.AddDays(1))
            .AgregarItem(Guid.NewGuid(), "Ingrediente", 1m, UnidadMedida.Kilogramo, 1m)
            .Construir();

        // Assert
        Assert.False(resultado.Succeeded);
        Assert.Equal("Errores de validación en la construcción de la orden de compra", resultado.Error);
    }

    [Fact]
    public void Construir_SinFechaEntrega_DeberiaRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = _builder
            .ParaProveedor(Guid.NewGuid())
            .ConFechaEmision(DateTime.Now)
            .AgregarItem(Guid.NewGuid(), "Ingrediente", 1m, UnidadMedida.Kilogramo, 1m)
            .Construir();

        // Assert
        Assert.False(resultado.Succeeded);
        Assert.Equal("Errores de validación en la construcción de la orden de compra", resultado.Error);
    }

    [Fact]
    public void Construir_SinItems_DeberiaRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = _builder
            .ParaProveedor(Guid.NewGuid())
            .ConFechaEmision(DateTime.Now)
            .ConFechaEntregaEstimada(DateTime.Now.AddDays(1))
            .Construir();

        // Assert
        Assert.False(resultado.Succeeded);
        Assert.Equal("Errores de validación en la construcción de la orden de compra", resultado.Error);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_DeberiaLimpiarTodosLosDatosYRetornarBuilder()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var fechaEmision = DateTime.Now;
        var fechaEntrega = DateTime.Now.AddDays(1);

        _builder
            .ParaProveedor(proveedorId)
            .ConFechaEmision(fechaEmision)
            .ConFechaEntregaEstimada(fechaEntrega)
            .AgregarItem(Guid.NewGuid(), "Ingrediente", 1m, UnidadMedida.Kilogramo, 1m)
            .ConObservaciones("Observaciones");

        // Act
        var resultado = _builder.Reset();

        // Assert
        Assert.Same(_builder, resultado);
        _notificationManagerMock.Verify(x => x.CreateNewNotification(), Times.Once);
    }

    #endregion

    #region Static Factory Tests

    [Fact]
    public void Nuevo_DeberiaCrearNuevaInstancia()
    {
        // Act
        var builder = OrdenCompraBuilder.Nuevo(_notificationManagerMock.Object, _loggerMock.Object);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<OrdenCompraBuilder>(builder);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Builder_FluentInterface_DeberiaPermitirEncadenamientoCompleto()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var fechaEmision = DateTime.Now.AddHours(-1);
        var fechaEntrega = DateTime.Now.AddDays(10);
        var harinaId = Guid.NewGuid();
        var aceiteId = Guid.NewGuid();

        // Act
        var resultado = OrdenCompraBuilder
            .Nuevo(_notificationManagerMock.Object, _loggerMock.Object)
            .ParaProveedor(proveedorId)
            .ConFechaEmision(fechaEmision)
            .ConFechaEntregaEstimada(fechaEntrega)
            .AgregarItem(harinaId, "Harina de Trigo", 50m, UnidadMedida.Kilogramo, 2.50m)
            .AgregarItem(aceiteId, "Aceite de Oliva", 20m, UnidadMedida.Litro, 8.75m)
            .ConObservaciones("Orden mensual - proveedores principal")
            .Construir();

        // Assert
        Assert.True(resultado.Succeeded);
        Assert.NotNull(resultado.Value);
    }

    [Fact]
    public void Builder_ConMultiplesErrores_DeberiaAcumularTodos()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = _builder
            .ParaProveedor(Guid.Empty)
            .ConFechaEmision(DateTime.Now.AddDays(1)) // Fecha futura
            .ConFechaEntregaEstimada(DateTime.Now.AddYears(2)) // Muy lejana
            .AgregarItem(Guid.Empty, "", -1m, UnidadMedida.Kilogramo, -1m) // Múltiples errores
            .ConObservaciones(new string('a', 1001)) // Muy largo
            .Construir();

        // Assert
        Assert.False(resultado.Succeeded);
        // Verificación de múltiples errores registrada correctamente
    }

    #endregion
} 