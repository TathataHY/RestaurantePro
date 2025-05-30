using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientePorId;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.UnitTests.Inventario.Ingredientes.Queries;

public class ObtenerIngredientePorIdHandlerTests
{
    private readonly Mock<IIngredienteRepository> _mockIngredienteRepository;
    private readonly Mock<IMovimientoInventarioRepository> _mockMovimientoRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerIngredientePorIdHandler>> _mockLogger;
    private readonly ObtenerIngredientePorIdHandler _handler;

    public ObtenerIngredientePorIdHandlerTests()
    {
        _mockIngredienteRepository = new Mock<IIngredienteRepository>();
        _mockMovimientoRepository = new Mock<IMovimientoInventarioRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerIngredientePorIdHandler>>();
        
        _handler = new ObtenerIngredientePorIdHandler(
            _mockIngredienteRepository.Object,
            _mockMovimientoRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ConIngredienteExistente_DeberiaRetornarIngredienteCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = new ObtenerIngredientePorIdQuery(ingredienteId, false);

        var ingrediente = CrearIngredienteMock("Tomate", 10m, 5m);
        var ingredienteDto = CrearIngredienteDtoMock("Tomate");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(ingrediente))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Nombre.Should().Be("Tomate");
        result.Value.StockActual.Should().Be(10m);

        _mockIngredienteRepository.Verify(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(m => m.Map<IngredienteDto>(ingrediente), Times.Once);
    }

    [Fact]
    public async Task Handle_ConIngredienteNoExistente_DeberiaRetornarError()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = new ObtenerIngredientePorIdQuery(ingredienteId);

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingrediente?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain($"No se encontró el ingrediente con ID {ingredienteId}");

        _mockIngredienteRepository.Verify(r => r.ObtenerPorIdAsync(ingredienteId, true, It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConIncluirMovimientosTrue_DeberiaCargarMovimientosRecientes()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = new ObtenerIngredientePorIdQuery(ingredienteId, true);

        var ingrediente = CrearIngredienteMock("Lechuga", 8m, 3m);
        var ingredienteDto = CrearIngredienteDtoMock("Lechuga");
        var movimientos = CrearMovimientosMock(ingredienteId, 5);
        var movimientosDto = CrearMovimientosDtoMock(5);

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockMovimientoRepository.Setup(r => r.ObtenerPorIngredienteAsync(ingredienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movimientos);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(ingrediente))
            .Returns(ingredienteDto);

        _mockMapper.Setup(m => m.Map<List<MovimientoInventarioDto>>(It.IsAny<IEnumerable<MovimientoInventario>>()))
            .Returns(movimientosDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MovimientosRecientes.Should().NotBeNull();
        result.Value.MovimientosRecientes.Should().HaveCount(5);

        _mockMovimientoRepository.Verify(r => r.ObtenerPorIngredienteAsync(ingredienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(m => m.Map<List<MovimientoInventarioDto>>(It.IsAny<IEnumerable<MovimientoInventario>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConIncluirMovimientosFalse_NoDeberiaCargarMovimientos()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = new ObtenerIngredientePorIdQuery(ingredienteId, false);

        var ingrediente = CrearIngredienteMock("Cebolla", 12m, 4m);
        var ingredienteDto = CrearIngredienteDtoMock("Cebolla");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(ingrediente))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MovimientosRecientes.Should().BeNullOrEmpty();

        _mockMovimientoRepository.Verify(r => r.ObtenerPorIngredienteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConMovimientosVarios_DeberiaMostrarSolo10Recientes()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = new ObtenerIngredientePorIdQuery(ingredienteId, true);

        var ingrediente = CrearIngredienteMock("Arroz", 25m, 10m);
        var ingredienteDto = CrearIngredienteDtoMock("Arroz");
        var movimientos = CrearMovimientosMock(ingredienteId, 15); // 15 movimientos, pero solo debería tomar 10
        var movimientosDto = CrearMovimientosDtoMock(10); // Solo los 10 más recientes

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockMovimientoRepository.Setup(r => r.ObtenerPorIngredienteAsync(ingredienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movimientos);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(ingrediente))
            .Returns(ingredienteDto);

        _mockMapper.Setup(m => m.Map<List<MovimientoInventarioDto>>(It.IsAny<IEnumerable<MovimientoInventario>>()))
            .Returns(movimientosDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.MovimientosRecientes.Should().HaveCount(10);

        // Verificar que se llamó para obtener solo los 10 más recientes
        _mockMapper.Verify(m => m.Map<List<MovimientoInventarioDto>>(
            It.Is<IEnumerable<MovimientoInventario>>(movs => movs.Count() <= 10)), Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcionEnRepositorio_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = new ObtenerIngredientePorIdQuery(ingredienteId);

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno al obtener el ingrediente");

        _mockMapper.Verify(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConExcepcionEnMovimientos_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = new ObtenerIngredientePorIdQuery(ingredienteId, true);

        var ingrediente = CrearIngredienteMock("Sal", 15m, 5m);
        var ingredienteDto = CrearIngredienteDtoMock("Sal");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(ingrediente))
            .Returns(ingredienteDto);

        _mockMovimientoRepository.Setup(r => r.ObtenerPorIngredienteAsync(ingredienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error al obtener movimientos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno al obtener el ingrediente");
    }

    [Fact]
    public async Task Handle_ConFactoryMethodCrear_DeberiaConfigurarParametrosCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = ObtenerIngredientePorIdQuery.Crear(ingredienteId);

        var ingrediente = CrearIngredienteMock("Pimienta", 6m, 2m);
        var ingredienteDto = CrearIngredienteDtoMock("Pimienta");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(ingrediente))
            .Returns(ingredienteDto);

        _mockMovimientoRepository.Setup(r => r.ObtenerPorIngredienteAsync(ingredienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MovimientoInventario>());

        _mockMapper.Setup(m => m.Map<List<MovimientoInventarioDto>>(It.IsAny<IEnumerable<MovimientoInventario>>()))
            .Returns(new List<MovimientoInventarioDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que por defecto incluye movimientos
        query.IncluirMovimientos.Should().BeTrue();
        _mockMovimientoRepository.Verify(r => r.ObtenerPorIngredienteAsync(ingredienteId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConFactoryMethodCrearSinMovimientos_NoDeberiaCargarMovimientos()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = ObtenerIngredientePorIdQuery.CrearSinMovimientos(ingredienteId);

        var ingrediente = CrearIngredienteMock("Azúcar", 20m, 8m);
        var ingredienteDto = CrearIngredienteDtoMock("Azúcar");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(ingrediente))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que NO incluye movimientos
        query.IncluirMovimientos.Should().BeFalse();
        _mockMovimientoRepository.Verify(r => r.ObtenerPorIngredienteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConIngredienteInactivo_DeberiaRetornarCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = new ObtenerIngredientePorIdQuery(ingredienteId, false);

        var ingrediente = CrearIngredienteMock("Pan", 5m, 2m);
        ingrediente.Desactivar(); // Ingrediente inactivo

        var ingredienteDto = CrearIngredienteDtoMock("Pan");
        ingredienteDto.Activo = false;

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(ingrediente))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ConIngredienteStockBajo_DeberiaRetornarCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = new ObtenerIngredientePorIdQuery(ingredienteId, false);

        var ingrediente = CrearIngredienteMock("Mantequilla", 2m, 5m); // Stock actual menor al mínimo
        var ingredienteDto = CrearIngredienteDtoMock("Mantequilla");
        ingredienteDto.StockActual = 2m;
        ingredienteDto.StockMinimo = 5m;

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(ingrediente))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.StockActual.Should().BeLessThan(result.Value.StockMinimo);
    }

    [Fact]
    public async Task Handle_ConConstructorSinParametros_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var query = new ObtenerIngredientePorIdQuery
        {
            Id = ingredienteId,
            IncluirMovimientos = false
        };

        var ingrediente = CrearIngredienteMock("Oregano", 3m, 1m);
        var ingredienteDto = CrearIngredienteDtoMock("Oregano");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorIdAsync(ingredienteId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingrediente);

        _mockMapper.Setup(m => m.Map<IngredienteDto>(ingrediente))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Nombre.Should().Be("Oregano");
    }

    // Métodos de ayuda para crear mocks
    private static Ingrediente CrearIngredienteMock(string nombre, decimal stock, decimal stockMinimo)
    {
        return Ingrediente.Crear(
            Guid.NewGuid(),
            nombre,
            $"COD-{nombre.ToUpper()}",
            $"Descripción de {nombre}",
            UnidadMedida.Kilogramo,
            stockMinimo,
            stock,
            RotacionIngrediente.Media,
            TemporadaIngrediente.TodoElAño);
    }

    private static IngredienteDto CrearIngredienteDtoMock(string nombre)
    {
        return new IngredienteDto
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Descripcion = $"Descripción de {nombre}",
            StockActual = 10m,
            StockMinimo = 5m,
            CostoUnitario = 1500m,
            Activo = true
        };
    }

    private static List<MovimientoInventario> CrearMovimientosMock(Guid ingredienteId, int cantidad)
    {
        var movimientos = new List<MovimientoInventario>();
        for (int i = 0; i < cantidad; i++)
        {
            var movimiento = MovimientoInventario.CrearIngreso(
                ingredienteId,
                5m,
                "Motivo test",
                DateTime.Now.AddDays(-i)); // Fechas diferentes para testing de orden
            movimientos.Add(movimiento);
        }
        return movimientos;
    }

    private static List<MovimientoInventarioDto> CrearMovimientosDtoMock(int cantidad)
    {
        var movimientos = new List<MovimientoInventarioDto>();
        for (int i = 0; i < cantidad; i++)
        {
            movimientos.Add(new MovimientoInventarioDto
            {
                Id = Guid.NewGuid(),
                TipoMovimiento = "Ingreso",
                Cantidad = 5m,
                Fecha = DateTime.Now.AddDays(-i),
                Motivo = $"Movimiento test {i + 1}"
            });
        }
        return movimientos;
    }
} 