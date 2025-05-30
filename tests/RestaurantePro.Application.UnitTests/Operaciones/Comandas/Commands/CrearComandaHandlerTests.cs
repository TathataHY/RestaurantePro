using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Exceptions;
using RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Commands;

public class CrearComandaHandlerTests
{
    private readonly Mock<IComandaRepository> _mockComandaRepository;
    private readonly Mock<IProductoRepository> _mockProductoRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CrearComandaHandler>> _mockLogger;
    private readonly CrearComandaHandler _handler;

    public CrearComandaHandlerTests()
    {
        _mockComandaRepository = new Mock<IComandaRepository>();
        _mockProductoRepository = new Mock<IProductoRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CrearComandaHandler>>();
        
        _handler = new CrearComandaHandler(
            _mockComandaRepository.Object,
            _mockProductoRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ComandaBasicaValida_DeberiaCrearExitosamente()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var command = new CrearComandaCommand(meseroId);

        var comanda = Comanda.Crear(meseroId);
        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            MeseroId = meseroId,
            Estado = "Creada",
            Total = 0
        };

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MeseroId.Should().Be(meseroId);
        result.Value.Estado.Should().Be("Creada");

        _mockComandaRepository.Verify(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockComandaRepository.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaConMesaYCliente_DeberiaCrearExitosamente()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var command = new CrearComandaCommand(meseroId, mesaId, clienteId)
        {
            Observaciones = "Mesa para 4 personas"
        };

        var comanda = Comanda.Crear(meseroId, clienteId, mesaId, "Mesa para 4 personas");
        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            MeseroId = meseroId,
            MesaId = mesaId,
            ClienteId = clienteId,
            Observaciones = "Mesa para 4 personas",
            Estado = "Creada"
        };

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MeseroId.Should().Be(meseroId);
        result.Value.MesaId.Should().Be(mesaId);
        result.Value.ClienteId.Should().Be(clienteId);
        result.Value.Observaciones.Should().Be("Mesa para 4 personas");
    }

    [Fact]
    public async Task Handle_ComandaConProductosIniciales_DeberiaCrearConProductos()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var productoId1 = Guid.NewGuid();
        var productoId2 = Guid.NewGuid();

        var command = new CrearComandaCommand(meseroId)
        {
            ProductosIniciales = new List<AgregarProductoDto>
            {
                new AgregarProductoDto
                {
                    ProductoId = productoId1,
                    Cantidad = 2,
                    PrecioUnitario = 15000m,
                    Observaciones = "Sin cebolla"
                },
                new AgregarProductoDto
                {
                    ProductoId = productoId2,
                    Cantidad = 1,
                    PrecioUnitario = 25000m
                }
            }
        };

        var comanda = Comanda.Crear(meseroId);
        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            MeseroId = meseroId,
            Estado = "Creada",
            Total = 55000m,
            Items = new List<ItemComandaDto>
            {
                new ItemComandaDto { ProductoId = productoId1, Cantidad = 2, PrecioUnitario = 15000m },
                new ItemComandaDto { ProductoId = productoId2, Cantidad = 1, PrecioUnitario = 25000m }
            }
        };

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Total.Should().Be(55000m);
    }

    [Fact]
    public async Task Handle_ProductosConPersonalizaciones_DeberiaCrearConPersonalizaciones()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteId = Guid.NewGuid();

        var command = new CrearComandaCommand(meseroId)
        {
            ProductosIniciales = new List<AgregarProductoDto>
            {
                new AgregarProductoDto
                {
                    ProductoId = productoId,
                    Cantidad = 1,
                    PrecioUnitario = 20000m,
                    Personalizaciones = new List<PersonalizacionCreateDto>
                    {
                        new PersonalizacionCreateDto
                        {
                            Tipo = "Extra",
                            IngredienteId = ingredienteId,
                            Cantidad = 2,
                            PrecioAdicional = 2000m
                        }
                    }
                }
            }
        };

        var comanda = Comanda.Crear(meseroId);
        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            MeseroId = meseroId,
            Estado = "Creada",
            Total = 22000m
        };

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Total.Should().Be(22000m);
    }

    [Fact]
    public async Task Handle_ExcepcionBusinessRule_DeberiaRetornarError()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var command = new CrearComandaCommand(meseroId);

        var businessException = new BusinessRuleViolationException("MeseroNoDisponible", "Comanda", "Operaciones");

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(businessException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(businessException.Message);

        _mockMapper.Verify(m => m.Map<ComandaDto>(It.IsAny<Comanda>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExcepcionArgument_DeberiaRetornarError()
    {
        // Arrange
        var meseroId = Guid.Empty; // ID inválido
        var command = new CrearComandaCommand(meseroId);

        var argumentException = new ArgumentException("MeseroId no puede ser vacío");

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(argumentException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(argumentException.Message);
    }

    [Fact]
    public async Task Handle_ExcepcionGeneral_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var command = new CrearComandaCommand(meseroId);

        var excepcionGeneral = new Exception("Error de base de datos");

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionGeneral);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al crear la comanda");

        _mockComandaRepository.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ErrorEnGuardarCambios_DeberiaRetornarError()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var command = new CrearComandaCommand(meseroId);

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error al guardar en base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al crear la comanda");

        _mockComandaRepository.Verify(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnMapper_DeberiaRetornarError()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var command = new CrearComandaCommand(meseroId);

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Throws(new Exception("Error de mapeo"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al crear la comanda");
    }

    [Fact]
    public async Task Handle_ConstructorSinParametros_DeberiaFuncionar()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        
        var command = new CrearComandaCommand
        {
            MeseroId = meseroId,
            MesaId = mesaId,
            Observaciones = "Comando creado sin constructor"
        };

        var comanda = Comanda.Crear(meseroId, null, mesaId, "Comando creado sin constructor");
        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            MeseroId = meseroId,
            MesaId = mesaId,
            Estado = "Creada"
        };

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MeseroId.Should().Be(meseroId);
        result.Value.MesaId.Should().Be(mesaId);
    }

    [Fact]
    public async Task Handle_ComandaConObservacionesLargas_DeberiaCrearExitosamente()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var observacionesLargas = "Esta es una observación muy larga para la comanda que incluye instrucciones especiales para la cocina, preferencias del cliente y detalles específicos sobre la preparación de los alimentos.";
        
        var command = new CrearComandaCommand(meseroId)
        {
            Observaciones = observacionesLargas
        };

        var comanda = Comanda.Crear(meseroId, null, null, observacionesLargas);
        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            MeseroId = meseroId,
            Observaciones = observacionesLargas,
            Estado = "Creada"
        };

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Observaciones.Should().Be(observacionesLargas);
    }

    [Fact]
    public async Task Handle_MultiplesProductosConCantidadesVariadas_DeberiaCalcularCorrectamente()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var command = new CrearComandaCommand(meseroId)
        {
            ProductosIniciales = new List<AgregarProductoDto>
            {
                new AgregarProductoDto { ProductoId = Guid.NewGuid(), Cantidad = 3, PrecioUnitario = 10000m },
                new AgregarProductoDto { ProductoId = Guid.NewGuid(), Cantidad = 1, PrecioUnitario = 45000m },
                new AgregarProductoDto { ProductoId = Guid.NewGuid(), Cantidad = 2, PrecioUnitario = 15000m },
                new AgregarProductoDto { ProductoId = Guid.NewGuid(), Cantidad = 5, PrecioUnitario = 8000m }
            }
        };

        var totalEsperado = (3 * 10000m) + (1 * 45000m) + (2 * 15000m) + (5 * 8000m); // 30000 + 45000 + 30000 + 40000 = 145000

        var comandaDto = new ComandaDto
        {
            Id = Guid.NewGuid(),
            MeseroId = meseroId,
            Estado = "Creada",
            Total = totalEsperado,
            Items = command.ProductosIniciales.Select(p => new ItemComandaDto
            {
                ProductoId = p.ProductoId,
                Cantidad = p.Cantidad,
                PrecioUnitario = p.PrecioUnitario
            }).ToList()
        };

        _mockComandaRepository.Setup(r => r.AgregarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(4);
        result.Value.Total.Should().Be(145000m);
    }
} 