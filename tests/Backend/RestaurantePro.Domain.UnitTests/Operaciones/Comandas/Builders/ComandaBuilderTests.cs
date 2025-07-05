namespace RestaurantePro.Domain.UnitTests.Operaciones.Comandas.Builders;

/// <summary>
/// Tests unitarios para ComandaBuilder - Validando construcción fluida de comandas
/// </summary>
public class ComandaBuilderTests
{
    private readonly Mock<ILogger<ComandaBuilder>> _loggerMock;
    private readonly NotificationManager _notificationManager;
    private readonly ComandaBuilder _sut;

    public ComandaBuilderTests()
    {
        _loggerMock = new Mock<ILogger<ComandaBuilder>>();
        _notificationManager = new NotificationManager();
        _sut = new ComandaBuilder(_notificationManager, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConParametrosValidos_DebeCrearBuilder()
    {
        // Arrange & Act
        var builder = new ComandaBuilder(_notificationManager, _loggerMock.Object);

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConParametrosNulos_DebeLanzarExcepcion()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ComandaBuilder(null!, _loggerMock.Object));
        
        Assert.Throws<ArgumentNullException>(() => 
            new ComandaBuilder(_notificationManager, null!));
    }

    #endregion

    #region Builder Methods Tests

    [Fact]
    public void ConMesero_ConIdValido_DebeConfigurarMesero()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();

        // Act
        var resultado = _sut.ConMesero(meseroId).EnMesa(mesaId);

        // Assert
        resultado.Should().Be(_sut); // Fluent interface
        // Verificar que se puede construir con el mesero y mesa
        var comanda = _sut.Construir();
        comanda.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void ConMesero_ConIdVacio_DebeLanzarExcepcion()
    {
        // Arrange
        var meseroIdVacio = Guid.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.ConMesero(meseroIdVacio));
    }

    [Fact]
    public void ConCliente_ConIdValido_DebeConfigurarCliente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();

        // Act
        var resultado = _sut.ConMesero(meseroId).EnMesa(mesaId).ConCliente(clienteId);

        // Assert
        resultado.Should().Be(_sut);
        var comanda = _sut.Construir();
        comanda.Succeeded.Should().BeTrue();
        comanda.Value!.ClienteId.Should().Be(clienteId);
    }

    [Fact]
    public void EnMesa_ConIdValido_DebeConfigurarMesa()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();

        // Act
        var resultado = _sut.ConMesero(meseroId).EnMesa(mesaId);

        // Assert
        resultado.Should().Be(_sut);
        var comanda = _sut.Construir();
        comanda.Succeeded.Should().BeTrue();
        comanda.Value!.MesaId.Should().Be(mesaId);
    }

    [Fact]
    public void ConObservaciones_ConTextoValido_DebeConfigurarObservaciones()
    {
        // Arrange
        var observaciones = "Plato sin cebolla, por favor";
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();

        // Act
        var resultado = _sut.ConMesero(meseroId).EnMesa(mesaId).ConObservaciones(observaciones);

        // Assert
        resultado.Should().Be(_sut);
        var comanda = _sut.Construir();
        comanda.Succeeded.Should().BeTrue();
        comanda.Value!.Observaciones.Should().Be(observaciones);
    }

    [Fact]
    public void ConObservaciones_ConTextoMuyLargo_DebeLanzarExcepcion()
    {
        // Arrange
        var observacionesLargas = new string('X', 501); // Más de 500 caracteres
        var meseroId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _sut.ConMesero(meseroId).ConObservaciones(observacionesLargas));
    }

    #endregion

    #region Producto Tests

    [Fact]
    public void AgregarProducto_ConDatosValidos_DebeAgregarProducto()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var cantidad = 2;
        var precio = 15.50m;
        var observaciones = "Término medio";

        // Act
        var resultado = _sut
            .ConMesero(meseroId)
            .EnMesa(mesaId)
            .AgregarProducto(productoId, cantidad, precio, observaciones);

        // Assert
        resultado.Should().Be(_sut);
        var comanda = _sut.Construir();
        comanda.Succeeded.Should().BeTrue();
        comanda.Value!.Items.Should().HaveCount(1);
        
        var item = comanda.Value.Items.First();
        item.ProductoId.Should().Be(productoId);
        item.Cantidad.Should().Be(cantidad);
        item.PrecioUnitario.Should().Be(precio);
        item.Observaciones.Should().Be(observaciones);
    }

    [Fact]
    public void AgregarProducto_ConCantidadExcesiva_DebeAgregarError()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();

        // Act
        _sut.ConMesero(meseroId).EnMesa(mesaId).AgregarProducto(productoId, 51, 10m); // Más de 50
        var resultado = _sut.Construir();

        // Assert
        resultado.Succeeded.Should().BeFalse();
        _notificationManager.HasErrors.Should().BeTrue();
        _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("cantidad"));
    }

    [Fact]
    public void AgregarProducto_ConPrecioExcesivo_DebeAgregarError()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();

        // Act
        _sut.ConMesero(meseroId).EnMesa(mesaId).AgregarProducto(productoId, 1, 1000001m); // Más de 1,000,000
        var resultado = _sut.Construir();

        // Assert
        resultado.Succeeded.Should().BeFalse();
        _notificationManager.HasErrors.Should().BeTrue();
        _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("precio"));
    }

    [Fact]
    public void AgregarProducto_ProductoDuplicado_DebeAgregarError()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();

        // Act
        _sut.ConMesero(meseroId)
            .EnMesa(mesaId)
            .AgregarProducto(productoId, 1, 10m)
            .AgregarProducto(productoId, 2, 15m); // Mismo producto
        
        var resultado = _sut.Construir();

        // Assert
        resultado.Succeeded.Should().BeFalse();
        _notificationManager.HasErrors.Should().BeTrue();
        _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("duplicado") || e.Message.Contains("existe"));
    }

    [Fact]
    public void AgregarProductos_ConMultiplesProductos_DebeAgregarTodos()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var producto1 = Guid.NewGuid();
        var producto2 = Guid.NewGuid();
        var producto3 = Guid.NewGuid();

        // Act
        var resultado = _sut
            .ConMesero(meseroId)
            .EnMesa(mesaId)
            .AgregarProductos(
                (producto1, 1, 10m, "Sin sal"),
                (producto2, 2, 15m, null),
                (producto3, 1, 8m, "Extra queso")
            );

        // Assert
        resultado.Should().Be(_sut);
        var comanda = _sut.Construir();
        comanda.Succeeded.Should().BeTrue();
        comanda.Value!.Items.Should().HaveCount(3);
    }

    #endregion

    #region Descuento Tests

    [Fact]
    public void ConDescuentoFidelizacion_ConClienteYPorcentajeValido_DebeConfigurarDescuento()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var descuento = 0.15m; // 15%

        // Act
        var resultado = _sut
            .ConMesero(meseroId)
            .EnMesa(mesaId)
            .ConCliente(clienteId)
            .AgregarProducto(productoId, 1, 100m)
            .ConDescuentoFidelizacion(descuento);

        // Assert
        resultado.Should().Be(_sut);
        var comanda = _sut.Construir();
        comanda.Succeeded.Should().BeTrue();
        comanda.Value!.TieneDescuentoFidelizacion().Should().BeTrue();
    }

    [Fact]
    public void ConDescuentoFidelizacion_SinCliente_DebeAgregarError()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();

        // Act
        _sut.ConMesero(meseroId).EnMesa(mesaId).ConDescuentoFidelizacion(0.1m);
        var resultado = _sut.Construir();

        // Assert
        resultado.Succeeded.Should().BeFalse();
        _notificationManager.HasErrors.Should().BeTrue();
        _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("cliente"));
    }

    [Fact]
    public void ConDescuentoFidelizacion_ConPorcentajeInvalido_DebeAgregarError()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        // Act
        _sut.ConMesero(meseroId).EnMesa(mesaId).ConCliente(clienteId).ConDescuentoFidelizacion(0.6m); // 60% - excede límite
        var resultado = _sut.Construir();

        // Assert
        resultado.Succeeded.Should().BeFalse();
        _notificationManager.HasErrors.Should().BeTrue();
        _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("50%"));
    }

    #endregion

    #region Construir Tests

    [Fact]
    public void Construir_SinMesero_DebeRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        // No agregar mesero

        // Act
        var resultado = _sut.EnMesa(mesaId).Construir();

        // Assert
        resultado.Succeeded.Should().BeFalse();
        _notificationManager.HasErrors.Should().BeTrue();
        _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("mesero"));
    }

    [Fact]
    public void Construir_SinMesa_DebeRetornarError()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        // No agregar mesa

        // Act
        var resultado = _sut.ConMesero(meseroId).Construir();

        // Assert
        resultado.Succeeded.Should().BeFalse();
        _notificationManager.HasErrors.Should().BeTrue();
        _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("mesa"));
    }

    [Fact]
    public void Construir_ConComandaCompleta_DebeRetornarExito()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();

        // Act
        var resultado = _sut
            .ConMesero(meseroId)
            .ConCliente(clienteId)
            .EnMesa(mesaId)
            .ConObservaciones("Mesa para celebración")
            .AgregarProducto(productoId, 2, 25.50m, "Bien cocido")
            .ConDescuentoFidelizacion(0.1m) // 10%
            .Construir();

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        
        var comanda = resultado.Value!;
        comanda.MeseroId.Should().Be(meseroId);
        comanda.ClienteId.Should().Be(clienteId);
        comanda.MesaId.Should().Be(mesaId);
        comanda.Items.Should().HaveCount(1);
        comanda.TieneDescuentoFidelizacion().Should().BeTrue();
    }

    [Fact]
    public void Construir_SinProductos_DebeGenerarInformacion()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();

        // Act
        var resultado = _sut.ConMesero(meseroId).EnMesa(mesaId).Construir();

        // Assert
        resultado.Succeeded.Should().BeTrue(); // Sigue siendo válido
        // Nota: AddInformation no afecta el resultado, es solo informativo
    }

    #endregion

    #region Utility Methods Tests

    [Fact]
    public void Reset_DebeReiniciarBuilder()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var productoId = Guid.NewGuid();

        _sut.ConMesero(meseroId)
            .EnMesa(mesaId)
            .ConCliente(clienteId)
            .AgregarProducto(productoId, 1, 10m);

        // Act
        var resultado = _sut.Reset();

        // Assert
        resultado.Should().Be(_sut);
        
        // Verificar que está reiniciado - debería fallar sin mesero y mesa
        var comanda = _sut.Construir();
        comanda.Succeeded.Should().BeFalse();
        _notificationManager.GetErrors().Should().Contain(e => e.Message.Contains("mesero") || e.Message.Contains("mesa"));
    }

    [Fact]
    public void Nuevo_DebeCrearBuilderConfigurado()
    {
        // Arrange
        var logger = new Mock<ILogger<ComandaBuilder>>().Object;
        var notificationManager = new NotificationManager();

        // Act
        var builder = ComandaBuilder.Nuevo(notificationManager, logger);

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeOfType<ComandaBuilder>();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void BuilderCompleto_DebeCrearComandaConTodasLasPropiedades()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var producto1Id = Guid.NewGuid();
        var producto2Id = Guid.NewGuid();

        // Act
        var resultado = _sut
            .ConMesero(meseroId)
            .ConCliente(clienteId)
            .EnMesa(mesaId)
            .ConObservaciones("Mesa VIP - servicio especial")
            .AgregarProducto(producto1Id, 2, 45.00m, "Término medio")
            .AgregarProducto(producto2Id, 1, 12.50m, "Sin hielo")
            .ConDescuentoFidelizacion(0.15m) // 15%
            .Construir();

        // Assert
        resultado.Succeeded.Should().BeTrue();
        
        var comanda = resultado.Value!;
        comanda.MeseroId.Should().Be(meseroId);
        comanda.ClienteId.Should().Be(clienteId);
        comanda.MesaId.Should().Be(mesaId);
        comanda.Observaciones.Should().Be("Mesa VIP - servicio especial");
        comanda.Items.Should().HaveCount(2);
        comanda.TieneDescuentoFidelizacion().Should().BeTrue();
        comanda.Estado.Should().Be(EstadoComanda.Creada);
    }

    [Fact]
    public void BuilderReutilizable_DebeFuncionarParaMultiplesComandas()
    {
        // Arrange
        var mesero1Id = Guid.NewGuid();
        var mesero2Id = Guid.NewGuid();
        var mesa1Id = Guid.NewGuid();
        var mesa2Id = Guid.NewGuid();
        var producto1Id = Guid.NewGuid();
        var producto2Id = Guid.NewGuid();

        // Act - Primera comanda
        var comanda1 = _sut
            .ConMesero(mesero1Id)
            .EnMesa(mesa1Id)
            .AgregarProducto(producto1Id, 1, 20m)
            .Construir();

        // Reset y segunda comanda
        var comanda2 = _sut
            .Reset()
            .ConMesero(mesero2Id)
            .EnMesa(mesa2Id)
            .AgregarProducto(producto2Id, 3, 15m)
            .Construir();

        // Assert
        comanda1.Succeeded.Should().BeTrue();
        comanda2.Succeeded.Should().BeTrue();
        
        comanda1.Value!.MeseroId.Should().Be(mesero1Id);
        comanda2.Value!.MeseroId.Should().Be(mesero2Id);
        
        comanda1.Value.Items.Should().HaveCount(1);
        comanda2.Value.Items.Should().HaveCount(1);
    }

    #endregion
} 