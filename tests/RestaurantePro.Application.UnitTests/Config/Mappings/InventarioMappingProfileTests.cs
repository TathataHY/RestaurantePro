namespace RestaurantePro.Application.UnitTests.Config.Mappings;

/// <summary>
/// Tests unitarios para InventarioMappingProfile
/// Cobertura completa de mapeos de Ingrediente, MovimientoInventario, OrdenCompra y DetalleOrdenCompra
/// </summary>
public class InventarioMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _configuration;

    public InventarioMappingProfileTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<InventarioMappingProfile>();
        });
        
        _mapper = _configuration.CreateMapper();
    }

    [Fact]
    public void Configuration_DeberiaSerValida()
    {
        // Act & Assert
        _configuration.AssertConfigurationIsValid();
    }

    #region Ingrediente Mappings Tests

    [Fact]
    public void Map_IngredienteToIngredienteDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var ingrediente = CrearIngredienteEjemplo();

        // Act
        var dto = _mapper.Map<IngredienteDto>(ingrediente);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(ingrediente.Id);
        dto.UnidadMedida.Should().Be(ingrediente.UnidadMedida.ToString());
        dto.FechaCreacion.Should().Be(ingrediente.FechaCreacion);
    }

    [Theory]
    [InlineData(UnidadMedida.Kilogramos, "Kilogramos")]
    [InlineData(UnidadMedida.Gramos, "Gramos")]
    [InlineData(UnidadMedida.Litros, "Litros")]
    [InlineData(UnidadMedida.Mililitros, "Mililitros")]
    [InlineData(UnidadMedida.Unidades, "Unidades")]
    [InlineData(UnidadMedida.Piezas, "Piezas")]
    public void Map_IngredienteToDto_ConDiferentesUnidadesMedida_DeberiaMapearTextoCorrectamente(UnidadMedida unidad, string expectedTexto)
    {
        // Arrange
        var ingrediente = CrearIngredienteEjemplo();
        typeof(Ingrediente).GetProperty("UnidadMedida")?.SetValue(ingrediente, unidad);

        // Act
        var dto = _mapper.Map<IngredienteDto>(ingrediente);

        // Assert
        dto.UnidadMedida.Should().Be(expectedTexto);
    }

    [Fact]
    public void Map_IngredienteToIngredienteSummaryDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var ingrediente = CrearIngredienteEjemplo();

        // Act
        var dto = _mapper.Map<IngredienteSummaryDto>(ingrediente);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(ingrediente.Id);
        dto.FechaRegistro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Map_IngredienteCreateDtoToCrearIngredienteCommand_DeberiaMapearCorrectamente()
    {
        // Arrange
        var createDto = new IngredienteCreateDto
        {
            Nombre = "Harina de trigo",
            Descripcion = "Harina de trigo para panadería",
            UnidadMedida = UnidadMedida.Kilogramos,
            StockMinimo = 10,
            StockMaximo = 100,
            StockInicial = 50,
            CostoInicial = 25.50m,
            CategoriaId = Guid.NewGuid(),
            ProveedorPrincipalId = Guid.NewGuid()
        };

        // Act
        var command = _mapper.Map<CrearIngredienteCommand>(createDto);

        // Assert
        command.Should().NotBeNull();
        command.Nombre.Should().Be(createDto.Nombre);
        command.Descripcion.Should().Be(createDto.Descripcion);
        command.UnidadMedida.Should().Be(createDto.UnidadMedida);
        command.StockMinimo.Should().Be(createDto.StockMinimo);
        command.StockMaximo.Should().Be(createDto.StockMaximo);
        command.StockInicial.Should().Be(createDto.StockInicial);
        command.CostoInicial.Should().Be(createDto.CostoInicial);
        command.CategoriaId.Should().Be(createDto.CategoriaId);
        command.ProveedorPrincipalId.Should().Be(createDto.ProveedorPrincipalId);
        command.MotivoStockInicial.Should().Be("Stock inicial al crear ingrediente");
    }

    [Fact]
    public void Map_IngredienteCreateDtoToCommand_UsuarioIdDeberiaIgnorarse()
    {
        // Arrange
        var createDto = new IngredienteCreateDto
        {
            Nombre = "Azúcar",
            Descripcion = "Azúcar refinada",
            UnidadMedida = UnidadMedida.Kilogramos,
            StockInicial = 25,
            CostoInicial = 15.00m
        };

        // Act
        var command = _mapper.Map<CrearIngredienteCommand>(createDto);

        // Assert
        command.UsuarioId.Should().Be(Guid.Empty); // Se ignora en el mapeo
    }

    #endregion

    #region MovimientoInventario Mappings Tests

    [Fact]
    public void Map_MovimientoInventarioToDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var movimiento = CrearMovimientoInventarioEjemplo();

        // Act
        var dto = _mapper.Map<MovimientoInventarioDto>(movimiento);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(movimiento.Id);
        dto.IngredienteId.Should().Be(movimiento.IngredienteId);
        dto.TipoMovimiento.Should().Be(movimiento.TipoMovimiento);
        dto.Cantidad.Should().Be(movimiento.Cantidad);
        dto.CostoUnitario.Should().Be(movimiento.CostoUnitario);
        dto.Motivo.Should().Be(movimiento.Motivo);
        dto.Fecha.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Theory]
    [InlineData(TipoMovimientoInventario.Entrada)]
    [InlineData(TipoMovimientoInventario.Salida)]
    [InlineData(TipoMovimientoInventario.Ajuste)]
    [InlineData(TipoMovimientoInventario.Transferencia)]
    [InlineData(TipoMovimientoInventario.Merma)]
    [InlineData(TipoMovimientoInventario.Devolucion)]
    public void Map_MovimientoInventarioToDto_ConDiferentesTipos_DeberiaMapearCorrectamente(TipoMovimientoInventario tipo)
    {
        // Arrange
        var movimiento = CrearMovimientoInventarioEjemplo();
        typeof(MovimientoInventario).GetProperty("TipoMovimiento")?.SetValue(movimiento, tipo);

        // Act
        var dto = _mapper.Map<MovimientoInventarioDto>(movimiento);

        // Assert
        dto.TipoMovimiento.Should().Be(tipo);
    }

    #endregion

    #region OrdenCompra Mappings Tests

    [Fact]
    public void Map_OrdenCompraToOrdenCompraDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var orden = CrearOrdenCompraEjemplo();

        // Act
        var dto = _mapper.Map<OrdenCompraDto>(orden);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(orden.Id);
        dto.NumeroOrden.Should().Be(orden.NumeroOrden.Value);
        dto.FechaOrden.Should().Be(orden.FechaOrden);
        dto.FechaEntregaEsperada.Should().Be(orden.FechaEntregaEsperada);
        dto.EstadoTexto.Should().Be(orden.Estado.ToString());
        dto.Total.Should().Be(orden.Total.Amount);
        dto.ProveedorId.Should().Be(orden.ProveedorId);
        dto.ProveedorNombre.Should().BeEmpty(); // TODO: Se mapea cuando tengamos navegación
        dto.Observaciones.Should().Be(orden.Observaciones);
        dto.FechaCreacion.Should().Be(orden.FechaCreacion);
        dto.CantidadItems.Should().Be(orden.Detalles.Count);
    }

    [Theory]
    [InlineData(EstadoOrdenCompra.Borrador, "Borrador")]
    [InlineData(EstadoOrdenCompra.Enviada, "Enviada")]
    [InlineData(EstadoOrdenCompra.Confirmada, "Confirmada")]
    [InlineData(EstadoOrdenCompra.EnTransito, "EnTransito")]
    [InlineData(EstadoOrdenCompra.Recibida, "Recibida")]
    [InlineData(EstadoOrdenCompra.Cancelada, "Cancelada")]
    public void Map_OrdenCompraToDto_ConDiferentesEstados_DeberiaMapearTextoCorrectamente(EstadoOrdenCompra estado, string expectedTexto)
    {
        // Arrange
        var orden = CrearOrdenCompraEjemplo();
        typeof(OrdenCompra).GetProperty("Estado")?.SetValue(orden, estado);

        // Act
        var dto = _mapper.Map<OrdenCompraDto>(orden);

        // Assert
        dto.EstadoTexto.Should().Be(expectedTexto);
    }

    [Fact]
    public void Map_OrdenCompraToDto_ConDetallesMultiples_DeberiaContarCorrectamente()
    {
        // Arrange
        var orden = CrearOrdenCompraEjemplo();
        var detalles = new List<DetalleOrdenCompra>
        {
            CrearDetalleOrdenCompraEjemplo(),
            CrearDetalleOrdenCompraEjemplo(),
            CrearDetalleOrdenCompraEjemplo()
        };
        typeof(OrdenCompra).GetProperty("Detalles")?.SetValue(orden, detalles);

        // Act
        var dto = _mapper.Map<OrdenCompraDto>(orden);

        // Assert
        dto.CantidadItems.Should().Be(3);
    }

    #endregion

    #region DetalleOrdenCompra Mappings Tests

    [Fact]
    public void Map_DetalleOrdenCompraToDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var detalle = CrearDetalleOrdenCompraEjemplo();

        // Act
        var dto = _mapper.Map<DetalleOrdenCompraDto>(detalle);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(detalle.Id);
        dto.IngredienteId.Should().Be(detalle.IngredienteId);
        dto.IngredienteNombre.Should().BeEmpty(); // TODO: Se mapea cuando tengamos navegación
        dto.CantidadSolicitada.Should().Be(detalle.CantidadSolicitada);
        dto.PrecioUnitario.Should().Be(detalle.PrecioUnitario.Amount);
        dto.Subtotal.Should().Be(detalle.Subtotal.Amount);
        dto.CantidadRecibida.Should().Be(detalle.CantidadRecibida);
    }

    [Theory]
    [InlineData(10, 5, true, false)]  // Pendiente
    [InlineData(10, 10, false, true)] // Completo
    [InlineData(10, 15, false, true)] // Sobre-recibido (completo)
    [InlineData(10, 0, true, false)]  // Sin recibir
    public void Map_DetalleOrdenCompraToDto_ConDiferentesCantidades_DeberiaCalcularEstadosCorrectamente(
        decimal cantidadSolicitada, decimal cantidadRecibida, bool expectedPendiente, bool expectedCompleto)
    {
        // Arrange
        var detalle = CrearDetalleOrdenCompraEjemplo();
        typeof(DetalleOrdenCompra).GetProperty("CantidadSolicitada")?.SetValue(detalle, cantidadSolicitada);
        typeof(DetalleOrdenCompra).GetProperty("CantidadRecibida")?.SetValue(detalle, cantidadRecibida);

        // Act
        var dto = _mapper.Map<DetalleOrdenCompraDto>(detalle);

        // Assert
        dto.EstaPendiente.Should().Be(expectedPendiente);
        dto.EstaCompleto.Should().Be(expectedCompleto);
    }

    #endregion

    #region Edge Cases y Null Handling

    [Fact]
    public void Map_IngredienteNull_DeberiaRetornarNull()
    {
        // Arrange
        Ingrediente? ingrediente = null;

        // Act
        var dto = _mapper.Map<IngredienteDto>(ingrediente);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_MovimientoInventarioNull_DeberiaRetornarNull()
    {
        // Arrange
        MovimientoInventario? movimiento = null;

        // Act
        var dto = _mapper.Map<MovimientoInventarioDto>(movimiento);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_OrdenCompraNull_DeberiaRetornarNull()
    {
        // Arrange
        OrdenCompra? orden = null;

        // Act
        var dto = _mapper.Map<OrdenCompraDto>(orden);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_DetalleOrdenCompraNull_DeberiaRetornarNull()
    {
        // Arrange
        DetalleOrdenCompra? detalle = null;

        // Act
        var dto = _mapper.Map<DetalleOrdenCompraDto>(detalle);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_ListaIngredientes_DeberiaMapearTodos()
    {
        // Arrange
        var ingredientes = new List<Ingrediente>
        {
            CrearIngredienteEjemplo(),
            CrearIngredienteEjemplo(),
            CrearIngredienteEjemplo()
        };

        // Act
        var dtos = _mapper.Map<List<IngredienteDto>>(ingredientes);

        // Assert
        dtos.Should().HaveCount(3);
        dtos.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void Map_ListaVacia_DeberiaRetornarListaVacia()
    {
        // Arrange
        var ingredientes = new List<Ingrediente>();

        // Act
        var dtos = _mapper.Map<List<IngredienteDto>>(ingredientes);

        // Assert
        dtos.Should().BeEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Map_IngredienteToDto_DeberiaSerRapido()
    {
        // Arrange
        var ingrediente = CrearIngredienteEjemplo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _mapper.Map<IngredienteDto>(ingrediente);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Menos de 100ms para 1000 mapeos
    }

    [Fact]
    public void Map_OrdenCompraToDto_DeberiaSerRapido()
    {
        // Arrange
        var orden = CrearOrdenCompraEjemplo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _mapper.Map<OrdenCompraDto>(orden);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150); // Menos de 150ms para 1000 mapeos (más complejo)
    }

    #endregion

    #region Helper Methods

    private Ingrediente CrearIngredienteEjemplo()
    {
        // Usar reflection para crear ingrediente con propiedades privadas
        var ingrediente = (Ingrediente)Activator.CreateInstance(typeof(Ingrediente), true)!;
        
        typeof(Ingrediente).GetProperty("Id")?.SetValue(ingrediente, Guid.NewGuid());
        typeof(Ingrediente).GetProperty("Nombre")?.SetValue(ingrediente, "Harina de trigo");
        typeof(Ingrediente).GetProperty("Descripcion")?.SetValue(ingrediente, "Harina de trigo para panadería");
        typeof(Ingrediente).GetProperty("UnidadMedida")?.SetValue(ingrediente, UnidadMedida.Kilogramos);
        typeof(Ingrediente).GetProperty("FechaCreacion")?.SetValue(ingrediente, DateTime.UtcNow.AddDays(-10));
        typeof(Ingrediente).GetProperty("Activo")?.SetValue(ingrediente, true);
        
        return ingrediente;
    }

    private MovimientoInventario CrearMovimientoInventarioEjemplo()
    {
        // Usar reflection para crear movimiento con propiedades privadas
        var movimiento = (MovimientoInventario)Activator.CreateInstance(typeof(MovimientoInventario), true)!;
        
        typeof(MovimientoInventario).GetProperty("Id")?.SetValue(movimiento, Guid.NewGuid());
        typeof(MovimientoInventario).GetProperty("IngredienteId")?.SetValue(movimiento, Guid.NewGuid());
        typeof(MovimientoInventario).GetProperty("TipoMovimiento")?.SetValue(movimiento, TipoMovimientoInventario.Entrada);
        typeof(MovimientoInventario).GetProperty("Cantidad")?.SetValue(movimiento, 25.5m);
        typeof(MovimientoInventario).GetProperty("CostoUnitario")?.SetValue(movimiento, 15.75m);
        typeof(MovimientoInventario).GetProperty("Motivo")?.SetValue(movimiento, "Compra a proveedor");
        typeof(MovimientoInventario).GetProperty("UsuarioId")?.SetValue(movimiento, Guid.NewGuid());
        typeof(MovimientoInventario).GetProperty("FechaCreacion")?.SetValue(movimiento, DateTime.UtcNow.AddHours(-2));
        
        return movimiento;
    }

    private OrdenCompra CrearOrdenCompraEjemplo()
    {
        // Usar reflection para crear orden con propiedades privadas
        var orden = (OrdenCompra)Activator.CreateInstance(typeof(OrdenCompra), true)!;
        
        typeof(OrdenCompra).GetProperty("Id")?.SetValue(orden, Guid.NewGuid());
        typeof(OrdenCompra).GetProperty("FechaOrden")?.SetValue(orden, DateTime.UtcNow);
        typeof(OrdenCompra).GetProperty("FechaEntregaEsperada")?.SetValue(orden, DateTime.UtcNow.AddDays(7));
        typeof(OrdenCompra).GetProperty("Estado")?.SetValue(orden, EstadoOrdenCompra.Enviada);
        typeof(OrdenCompra).GetProperty("ProveedorId")?.SetValue(orden, Guid.NewGuid());
        typeof(OrdenCompra).GetProperty("Observaciones")?.SetValue(orden, "Entrega urgente");
        typeof(OrdenCompra).GetProperty("FechaCreacion")?.SetValue(orden, DateTime.UtcNow.AddHours(-1));
        
        // Crear value objects mock
        var numeroOrden = CrearNumeroOrdenMock("OC-2024-001");
        var total = CrearMoneyMock(1250.75m);
        
        typeof(OrdenCompra).GetProperty("NumeroOrden")?.SetValue(orden, numeroOrden);
        typeof(OrdenCompra).GetProperty("Total")?.SetValue(orden, total);
        
        // Crear detalles mock
        var detalles = new List<DetalleOrdenCompra> { CrearDetalleOrdenCompraEjemplo() };
        typeof(OrdenCompra).GetProperty("Detalles")?.SetValue(orden, detalles);
        
        return orden;
    }

    private DetalleOrdenCompra CrearDetalleOrdenCompraEjemplo()
    {
        // Usar reflection para crear detalle con propiedades privadas
        var detalle = (DetalleOrdenCompra)Activator.CreateInstance(typeof(DetalleOrdenCompra), true)!;
        
        typeof(DetalleOrdenCompra).GetProperty("Id")?.SetValue(detalle, Guid.NewGuid());
        typeof(DetalleOrdenCompra).GetProperty("IngredienteId")?.SetValue(detalle, Guid.NewGuid());
        typeof(DetalleOrdenCompra).GetProperty("CantidadSolicitada")?.SetValue(detalle, 50.0m);
        typeof(DetalleOrdenCompra).GetProperty("CantidadRecibida")?.SetValue(detalle, 45.0m);
        
        // Crear value objects mock
        var precioUnitario = CrearMoneyMock(25.50m);
        var subtotal = CrearMoneyMock(1275.00m);
        
        typeof(DetalleOrdenCompra).GetProperty("PrecioUnitario")?.SetValue(detalle, precioUnitario);
        typeof(DetalleOrdenCompra).GetProperty("Subtotal")?.SetValue(detalle, subtotal);
        
        return detalle;
    }

    private object CrearNumeroOrdenMock(string numero)
    {
        // Crear mock del value object NumeroOrden
        var numeroType = typeof(OrdenCompra).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Contains("NumeroOrden"));
            
        if (numeroType != null)
        {
            try
            {
                var numeroObj = Activator.CreateInstance(numeroType, true);
                numeroType.GetProperty("Value")?.SetValue(numeroObj, numero);
                return numeroObj!;
            }
            catch { }
        }
        
        // Fallback: crear objeto dinámico
        return new { Value = numero };
    }

    private object CrearMoneyMock(decimal amount)
    {
        // Crear mock del value object Money
        var moneyType = typeof(OrdenCompra).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Contains("Money"));
            
        if (moneyType != null)
        {
            try
            {
                var moneyObj = Activator.CreateInstance(moneyType, true);
                moneyType.GetProperty("Amount")?.SetValue(moneyObj, amount);
                return moneyObj!;
            }
            catch { }
        }
        
        // Fallback: crear objeto dinámico
        return new { Amount = amount };
    }

    #endregion
} 