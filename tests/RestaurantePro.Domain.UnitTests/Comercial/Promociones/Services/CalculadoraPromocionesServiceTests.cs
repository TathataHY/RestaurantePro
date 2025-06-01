namespace RestaurantePro.Domain.UnitTests.Comercial.Promociones.Services;

/// <summary>
/// Pruebas unitarias para CalculadoraPromocionesService (Servicio de Dominio)
/// </summary>
public class CalculadoraPromocionesServiceTests
{
    private readonly Mock<IPromocionRepository> _promocionRepositoryMock;
    private readonly Mock<IProductoRepository> _productoRepositoryMock;
    private readonly Mock<ILogger<CalculadoraPromocionesService>> _loggerMock;
    private readonly CalculadoraPromocionesService _service;

    public CalculadoraPromocionesServiceTests()
    {
        _promocionRepositoryMock = new Mock<IPromocionRepository>();
        _productoRepositoryMock = new Mock<IProductoRepository>();
        _loggerMock = new Mock<ILogger<CalculadoraPromocionesService>>();
        _service = new CalculadoraPromocionesService(
            _promocionRepositoryMock.Object,
            _productoRepositoryMock.Object,
            _loggerMock.Object);
    }

    #region EvaluarPromocionesAsync Tests

    [Fact]
    public async Task EvaluarPromocionesAsync_ConClienteYProductosValidos_DeberiaRetornarPromocionesAplicables()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var productosCompra = new List<ProductoCompraDto>
        {
            new() { ProductoId = Guid.NewGuid(), Cantidad = 2, PrecioUnitario = 15.50m },
            new() { ProductoId = Guid.NewGuid(), Cantidad = 1, PrecioUnitario = 25.00m }
        };

        var promocionesActivas = new List<Promocion>
        {
            CrearPromocionMock(Guid.NewGuid(), "2x1 Hamburguesas", TipoPromocion.CantidadPorUno, 50m),
            CrearPromocionMock(Guid.NewGuid(), "Descuento 10%", TipoPromocion.PorcentajeDescuento, 10m)
        };

        _promocionRepositoryMock
            .Setup(x => x.ObtenerPromocionesActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(promocionesActivas);

        // Act
        var resultado = await _service.EvaluarPromocionesAsync(clienteId, productosCompra);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.PromocionesAplicables.Should().NotBeEmpty();
    }

    [Fact]
    public async Task EvaluarPromocionesAsync_SinPromocionesActivas_DeberiaRetornarListaVacia()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var productosCompra = new List<ProductoCompraDto>
        {
            new() { ProductoId = Guid.NewGuid(), Cantidad = 1, PrecioUnitario = 20.00m }
        };

        _promocionRepositoryMock
            .Setup(x => x.ObtenerPromocionesActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Promocion>());

        // Act
        var resultado = await _service.EvaluarPromocionesAsync(clienteId, productosCompra);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.PromocionesAplicables.Should().BeEmpty();
        resultado.Value.AhorroTotal.Should().Be(0m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new object[0])]
    public async Task EvaluarPromocionesAsync_ConListaProductosInvalida_DeberiaRetornarError(IEnumerable<ProductoCompraDto>? productos)
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act
        var resultado = await _service.EvaluarPromocionesAsync(clienteId, productos ?? new List<ProductoCompraDto>());

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ErrorMessage.Should().Contain("La lista de productos no puede estar vacía");
    }

    #endregion

    #region CalcularMejorComboAsync Tests

    [Fact]
    public async Task CalcularMejorComboAsync_ConProductosCompatibles_DeberiaRetornarMejorCombinacion()
    {
        // Arrange
        var productosDisponibles = new List<Producto>
        {
            CrearProductoMock(Guid.NewGuid(), "Hamburguesa", 15.00m, "Plato Principal"),
            CrearProductoMock(Guid.NewGuid(), "Papas", 8.00m, "Acompañamiento"),
            CrearProductoMock(Guid.NewGuid(), "Refresco", 5.00m, "Bebida")
        };

        var reglasCombo = new List<ReglaComboDto>
        {
            new() 
            { 
                CategoriasRequeridas = new[] { "Plato Principal", "Acompañamiento", "Bebida" },
                DescuentoPorcentaje = 15m,
                PrecioFijo = null
            }
        };

        _productoRepositoryMock
            .Setup(x => x.ObtenerProductosActivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(productosDisponibles);

        // Act
        var resultado = await _service.CalcularMejorComboAsync(reglasCombo);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.ProductosCombo.Should().HaveCount(3);
        resultado.Value.AhorroCalculado.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CalcularMejorComboAsync_SinProductosSuficientes_DeberiaRetornarComboVacio()
    {
        // Arrange
        var productosDisponibles = new List<Producto>
        {
            CrearProductoMock(Guid.NewGuid(), "Hamburguesa", 15.00m, "Plato Principal")
            // Solo 1 producto, pero el combo necesita 3 categorías
        };

        var reglasCombo = new List<ReglaComboDto>
        {
            new() 
            { 
                CategoriasRequeridas = new[] { "Plato Principal", "Acompañamiento", "Bebida" },
                DescuentoPorcentaje = 15m
            }
        };

        _productoRepositoryMock
            .Setup(x => x.ObtenerProductosActivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(productosDisponibles);

        // Act
        var resultado = await _service.CalcularMejorComboAsync(reglasCombo);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.ProductosCombo.Should().BeEmpty();
        resultado.Value.AhorroCalculado.Should().Be(0m);
    }

    #endregion

    #region ValidarCompatibilidadPromocionesAsync Tests

    [Fact]
    public async Task ValidarCompatibilidadPromocionesAsync_ConPromocionesCompatibles_DeberiaRetornarTrue()
    {
        // Arrange
        var promocion1 = CrearPromocionMock(Guid.NewGuid(), "Descuento 10%", TipoPromocion.PorcentajeDescuento, 10m);
        var promocion2 = CrearPromocionMock(Guid.NewGuid(), "Producto Gratis", TipoPromocion.ProductoGratis, 0m);
        
        promocion1.Setup(x => x.PermiteAcumulacion).Returns(true);
        promocion2.Setup(x => x.PermiteAcumulacion).Returns(true);

        var promocionesSeleccionadas = new List<Promocion> { promocion1.Object, promocion2.Object };

        // Act
        var resultado = await _service.ValidarCompatibilidadPromocionesAsync(promocionesSeleccionadas);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ValidarCompatibilidadPromocionesAsync_ConPromocionesIncompatibles_DeberiaRetornarFalse()
    {
        // Arrange
        var promocion1 = CrearPromocionMock(Guid.NewGuid(), "Descuento 50%", TipoPromocion.PorcentajeDescuento, 50m);
        var promocion2 = CrearPromocionMock(Guid.NewGuid(), "Descuento 30%", TipoPromocion.PorcentajeDescuento, 30m);
        
        promocion1.Setup(x => x.PermiteAcumulacion).Returns(false);
        promocion2.Setup(x => x.PermiteAcumulacion).Returns(false);

        var promocionesSeleccionadas = new List<Promocion> { promocion1.Object, promocion2.Object };

        // Act
        var resultado = await _service.ValidarCompatibilidadPromocionesAsync(promocionesSeleccionadas);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().BeFalse();
    }

    [Fact]
    public async Task ValidarCompatibilidadPromocionesAsync_ConListaVacia_DeberiaRetornarTrue()
    {
        // Act
        var resultado = await _service.ValidarCompatibilidadPromocionesAsync(new List<Promocion>());

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().BeTrue();
    }

    #endregion

    #region CalcularAhorroEstimadoAsync Tests

    [Theory]
    [InlineData(TipoPromocion.PorcentajeDescuento, 10, 100.00, 10.00)]
    [InlineData(TipoPromocion.PorcentajeDescuento, 25, 200.00, 50.00)]
    [InlineData(TipoPromocion.MontoDescuento, 15, 100.00, 15.00)]
    [InlineData(TipoPromocion.MontoDescuento, 50, 30.00, 30.00)] // No puede ser mayor al total
    public async Task CalcularAhorroEstimadoAsync_ConDiferentesTipos_DeberiaCalcularCorrectamente(
        TipoPromocion tipo, decimal valor, decimal montoCompra, decimal ahorroEsperado)
    {
        // Arrange
        var promocion = CrearPromocionMock(Guid.NewGuid(), "Test", tipo, valor);
        var productosCompra = new List<ProductoCompraDto>
        {
            new() { ProductoId = Guid.NewGuid(), Cantidad = 1, PrecioUnitario = montoCompra }
        };

        // Act
        var resultado = await _service.CalcularAhorroEstimadoAsync(promocion.Object, productosCompra);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(ahorroEsperado);
    }

    [Fact]
    public async Task CalcularAhorroEstimadoAsync_ConPromocion2x1_DeberiaCalcularAhorroCorrectamente()
    {
        // Arrange
        var promocion = CrearPromocionMock(Guid.NewGuid(), "2x1", TipoPromocion.CantidadPorUno, 50m);
        var productosCompra = new List<ProductoCompraDto>
        {
            new() { ProductoId = Guid.NewGuid(), Cantidad = 4, PrecioUnitario = 20.00m } // 4 productos, paga 2
        };

        // Act
        var resultado = await _service.CalcularAhorroEstimadoAsync(promocion.Object, productosCompra);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(40.00m); // Ahorra 2 productos de $20 cada uno
    }

    [Fact]
    public async Task CalcularAhorroEstimadoAsync_ConProductoGratis_DeberiaRetornarPrecioProducto()
    {
        // Arrange
        var productogratiId = Guid.NewGuid();
        var promocion = CrearPromocionMock(Guid.NewGuid(), "Producto Gratis", TipoPromocion.ProductoGratis, 0m);
        var productosCompra = new List<ProductoCompraDto>
        {
            new() { ProductoId = productogratiId, Cantidad = 1, PrecioUnitario = 25.00m }
        };

        // Mock del producto gratis
        promocion.Setup(x => x.ProductosGratis).Returns(new List<Guid> { productogratiId });

        // Act
        var resultado = await _service.CalcularAhorroEstimadoAsync(promocion.Object, productosCompra);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().Be(25.00m);
    }

    #endregion

    #region ObtenerPromocionesPersonalizadasAsync Tests

    [Fact]
    public async Task ObtenerPromocionesPersonalizadasAsync_ConClienteFrecuente_DeberiaRetornarPromocionesEspeciales()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var historialCompras = new List<HistorialCompraDto>
        {
            new() { ProductoId = Guid.NewGuid(), Cantidad = 5, MontoTotal = 100.00m },
            new() { ProductoId = Guid.NewGuid(), Cantidad = 3, MontoTotal = 75.00m }
        };

        var promocionesPersonalizadas = new List<Promocion>
        {
            CrearPromocionMock(Guid.NewGuid(), "Cliente VIP 20%", TipoPromocion.PorcentajeDescuento, 20m)
        };

        _promocionRepositoryMock
            .Setup(x => x.ObtenerPromocionesPersonalizadasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(promocionesPersonalizadas);

        // Act
        var resultado = await _service.ObtenerPromocionesPersonalizadasAsync(clienteId, historialCompras);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().NotBeEmpty();
        resultado.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObtenerPromocionesPersonalizadasAsync_ConClienteNuevo_DeberiaRetornarPromocionesGenericas()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var historialCompras = new List<HistorialCompraDto>(); // Cliente nuevo

        _promocionRepositoryMock
            .Setup(x => x.ObtenerPromocionesPersonalizadasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Promocion>());

        _promocionRepositoryMock
            .Setup(x => x.ObtenerPromocionesGeneralAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Promocion>
            {
                CrearPromocionMock(Guid.NewGuid(), "Bienvenida 5%", TipoPromocion.PorcentajeDescuento, 5m).Object
            });

        // Act
        var resultado = await _service.ObtenerPromocionesPersonalizadasAsync(clienteId, historialCompras);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().NotBeEmpty();
    }

    #endregion

    #region Pruebas de Manejo de Excepciones

    [Fact]
    public async Task EvaluarPromocionesAsync_CuandoRepositorioLanzaExcepcion_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var productosCompra = new List<ProductoCompraDto>
        {
            new() { ProductoId = Guid.NewGuid(), Cantidad = 1, PrecioUnitario = 20.00m }
        };

        _promocionRepositoryMock
            .Setup(x => x.ObtenerPromocionesActivasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error de conexión"));

        // Act
        var resultado = await _service.EvaluarPromocionesAsync(clienteId, productosCompra);

        // Assert
        resultado.IsSuccess.Should().BeFalse();
        resultado.ErrorMessage.Should().Contain("Error evaluando promociones");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error evaluando promociones")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Pruebas de Lógica de Algoritmos

    [Fact]
    public async Task AlgoritmoOptimizacion_DeberiaSeleccionarCombinacionConMayorAhorro()
    {
        // Arrange
        var promociones = new List<Promocion>
        {
            CrearPromocionMock(Guid.NewGuid(), "Descuento 10%", TipoPromocion.PorcentajeDescuento, 10m).Object,
            CrearPromocionMock(Guid.NewGuid(), "Descuento $20", TipoPromocion.MontoDescuento, 20m).Object,
            CrearPromocionMock(Guid.NewGuid(), "Descuento 5%", TipoPromocion.PorcentajeDescuento, 5m).Object
        };

        var productosCompra = new List<ProductoCompraDto>
        {
            new() { ProductoId = Guid.NewGuid(), Cantidad = 1, PrecioUnitario = 100.00m }
        };

        promociones.ForEach(p => 
        {
            var mock = Mock.Get(p);
            mock.Setup(x => x.PermiteAcumulacion).Returns(false);
        });

        _promocionRepositoryMock
            .Setup(x => x.ObtenerPromocionesActivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(promociones);

        // Act
        var resultado = await _service.EvaluarPromocionesAsync(Guid.NewGuid(), productosCompra);

        // Assert
        resultado.IsSuccess.Should().BeTrue();
        var mejorPromocion = resultado.Value.PromocionesAplicables.First();
        mejorPromocion.AhorroCalculado.Should().Be(20.00m); // La promoción de $20 es mejor que 10%
    }

    #endregion

    #region Métodos Helper

    private Mock<Promocion> CrearPromocionMock(Guid id, string nombre, TipoPromocion tipo, decimal valor)
    {
        var promocion = new Mock<Promocion>();
        promocion.Setup(x => x.Id).Returns(id);
        promocion.Setup(x => x.Nombre).Returns(nombre);
        promocion.Setup(x => x.TipoPromocion).Returns(tipo);
        promocion.Setup(x => x.ValorDescuento).Returns(valor);
        promocion.Setup(x => x.Activa).Returns(true);
        promocion.Setup(x => x.FechaInicio).Returns(DateTime.Today.AddDays(-1));
        promocion.Setup(x => x.FechaFin).Returns(DateTime.Today.AddDays(30));
        promocion.Setup(x => x.PermiteAcumulacion).Returns(true);
        return promocion;
    }

    private Mock<Producto> CrearProductoMock(Guid id, string nombre, decimal precio, string categoria)
    {
        var producto = new Mock<Producto>();
        producto.Setup(x => x.Id).Returns(id);
        producto.Setup(x => x.Nombre).Returns(nombre);
        producto.Setup(x => x.Precio).Returns(precio);
        producto.Setup(x => x.Categoria).Returns(categoria);
        producto.Setup(x => x.Activo).Returns(true);
        return producto;
    }

    #endregion
} 