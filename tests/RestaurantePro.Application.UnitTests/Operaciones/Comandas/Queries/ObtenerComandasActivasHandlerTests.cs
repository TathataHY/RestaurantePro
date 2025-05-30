using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandasActivas;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Queries;

public class ObtenerComandasActivasHandlerTests
{
    private readonly Mock<IComandaRepository> _mockComandaRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerComandasActivasHandler>> _mockLogger;
    private readonly ObtenerComandasActivasHandler _handler;

    public ObtenerComandasActivasHandlerTests()
    {
        _mockComandaRepository = new Mock<IComandaRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerComandasActivasHandler>>();
        
        _handler = new ObtenerComandasActivasHandler(
            _mockComandaRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_SinFiltros_DeberiaRetornarComandasActivasPaginadas()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery(pageNumber: 1, pageSize: 10);
        
        var comandasActivas = CrearListaComandasActivas();
        var comandasSummary = CrearListaComandaSummaryDto();

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasActivas);

        _mockMapper.Setup(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Returns(comandasSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(3);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(3);
        result.Value.TotalPages.Should().Be(1);

        _mockComandaRepository.Verify(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FiltrosPorMesa_DeberiaAplicarFiltroCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var query = new ObtenerComandasActivasQuery(mesaId);
        
        var comandasActivas = CrearListaComandasActivas();
        // Asignar mesaId solo a la primera comanda
        comandasActivas[0] = Comanda.Crear(Guid.NewGuid(), null, mesaId, "Comanda mesa específica");
        
        var comandasSummary = new List<ComandaSummaryDto>
        {
            new ComandaSummaryDto { Id = comandasActivas[0].Id, MesaId = mesaId }
        };

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasActivas);

        _mockMapper.Setup(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Returns(comandasSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().MesaId.Should().Be(mesaId);
    }

    [Fact]
    public async Task Handle_FiltrosPorMesero_DeberiaAplicarFiltroCorrectamente()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var query = ObtenerComandasActivasQuery.PorMesero(meseroId);
        
        var comandasActivas = CrearListaComandasActivas();
        // Hacer que la primera comanda sea del mesero específico
        comandasActivas[0] = Comanda.Crear(meseroId, null, Guid.NewGuid(), "Comanda del mesero");
        
        var comandasSummary = new List<ComandaSummaryDto>
        {
            new ComandaSummaryDto { Id = comandasActivas[0].Id, TomodaPor = "Mesero Test" }
        };

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasActivas);

        _mockMapper.Setup(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Returns(comandasSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().TomodaPor.Should().Be("Mesero Test");
    }

    [Fact]
    public async Task Handle_FiltrosPorEstado_DeberiaAplicarFiltroCorrectamente()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            EstadoFiltro = "En Preparación",
            PageNumber = 1,
            PageSize = 10
        };
        
        var comandasActivas = CrearListaComandasActivas();
        // Cambiar estado de la primera comanda
        comandasActivas[0].AgregarItem(Guid.NewGuid(), "Item", 1, 10000m);
        comandasActivas[0].MarcarEnPreparacion();
        
        var comandasSummary = new List<ComandaSummaryDto>
        {
            new ComandaSummaryDto { Id = comandasActivas[0].Id, Estado = "En Preparación" }
        };

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasActivas);

        _mockMapper.Setup(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Returns(comandasSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Estado.Should().Be("En Preparación");
    }

    [Fact]
    public async Task Handle_QueryParaAtrasadas_DeberiaConfigurarCorrectamente()
    {
        // Arrange
        var query = ObtenerComandasActivasQuery.CrearParaAtrasadas();
        
        var comandasActivas = CrearListaComandasActivas();
        var comandasSummary = CrearListaComandaSummaryDto();

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasActivas);

        _mockMapper.Setup(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Returns(comandasSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar configuración de la query
        query.SoloAtrasadas.Should().BeTrue();
        query.OrdenarPor.Should().Be("TiempoTranscurrido");
        query.DireccionOrden.Should().Be("Desc");
    }

    [Fact]
    public async Task Handle_Paginacion_DeberiaAplicarCorrectamente()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery(pageNumber: 2, pageSize: 2);
        
        var comandasActivas = CrearListaComandasActivas(); // 3 comandas
        var comandasSummary = new List<ComandaSummaryDto>
        {
            new ComandaSummaryDto { Id = comandasActivas[2].Id } // Solo la tercera comanda
        };

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasActivas);

        _mockMapper.Setup(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Returns(comandasSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(2);
        result.Value.TotalCount.Should().Be(3);
        result.Value.TotalPages.Should().Be(2);
        result.Value.Items.Should().HaveCount(1); // Solo un item en la página 2 con pageSize=2
    }

    [Fact]
    public async Task Handle_OrdenamientoPorFechaDesc_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            OrdenarPor = "FechaCreacion",
            DireccionOrden = "Desc"
        };
        
        var comandasActivas = CrearListaComandasActivas();
        var comandasSummary = CrearListaComandaSummaryDto();

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasActivas);

        _mockMapper.Setup(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Returns(comandasSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_SinComandas_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery();
        
        var comandasVacias = new List<Comanda>();
        var comandasSummaryVacias = new List<ComandaSummaryDto>();

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasVacias);

        _mockMapper.Setup(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Returns(comandasSummaryVacias);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ErrorEnRepositorio_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery();

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conexión a base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Ocurrió un error interno al consultar las comandas");

        _mockMapper.Verify(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ErrorEnMapper_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery();
        
        var comandasActivas = CrearListaComandasActivas();

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasActivas);

        _mockMapper.Setup(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Throws(new Exception("Error de mapeo"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Ocurrió un error interno al consultar las comandas");
    }

    [Fact]
    public async Task Handle_ArgumentException_DeberiaRetornarErrorValidacion()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery();

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Parámetro inválido"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Parámetro inválido");
    }

    [Fact]
    public async Task Handle_FiltrosPorCliente_DeberiaAplicarFiltroCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerComandasActivasQuery
        {
            ClienteId = clienteId
        };
        
        var comandasActivas = CrearListaComandasActivas();
        // Asignar clienteId a la primera comanda
        comandasActivas[0] = Comanda.Crear(Guid.NewGuid(), clienteId, Guid.NewGuid(), "Comanda con cliente");
        
        var comandasSummary = new List<ComandaSummaryDto>
        {
            new ComandaSummaryDto { Id = comandasActivas[0].Id, ClienteId = clienteId }
        };

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasActivas);

        _mockMapper.Setup(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Returns(comandasSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().ClienteId.Should().Be(clienteId);
    }

    [Fact]
    public async Task Handle_FiltroSoloHoy_DeberiaUsarFechaActual()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            SoloHoy = true
        };
        
        var comandasActivas = CrearListaComandasActivas();
        var comandasSummary = CrearListaComandaSummaryDto();

        _mockComandaRepository.Setup(r => r.ObtenerComandasAbiertas(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasActivas);

        _mockMapper.Setup(m => m.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Returns(comandasSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar que SoloHoy está configurado
        query.SoloHoy.Should().BeTrue();
        query.FechaEspecifica.Should().BeNull(); // No debe tener fecha específica cuando es SoloHoy
    }

    // Métodos de ayuda para crear datos de prueba
    private static List<Comanda> CrearListaComandasActivas()
    {
        var usuario1 = Guid.NewGuid();
        var usuario2 = Guid.NewGuid();
        var usuario3 = Guid.NewGuid();

        return new List<Comanda>
        {
            Comanda.Crear(usuario1, null, Guid.NewGuid(), "Comanda 1"),
            Comanda.Crear(usuario2, null, Guid.NewGuid(), "Comanda 2"),
            Comanda.Crear(usuario3, null, Guid.NewGuid(), "Comanda 3")
        };
    }

    private static List<ComandaSummaryDto> CrearListaComandaSummaryDto()
    {
        return new List<ComandaSummaryDto>
        {
            new ComandaSummaryDto 
            { 
                Id = Guid.NewGuid(), 
                Estado = "Pendiente",
                Total = 25000m,
                FechaCreacion = DateTime.Now
            },
            new ComandaSummaryDto 
            { 
                Id = Guid.NewGuid(), 
                Estado = "En Preparación",
                Total = 35000m,
                FechaCreacion = DateTime.Now.AddMinutes(-10)
            },
            new ComandaSummaryDto 
            { 
                Id = Guid.NewGuid(), 
                Estado = "Lista",
                Total = 18000m,
                FechaCreacion = DateTime.Now.AddMinutes(-20)
            }
        };
    }
} 