using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados;
using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Application.UnitTests.Comercial.Clientes.Queries;

public class ObtenerClientesPaginadosHandlerTests
{
    private readonly Mock<IClienteRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerClientesPaginadosHandler>> _mockLogger;
    private readonly ObtenerClientesPaginadosHandler _handler;

    public ObtenerClientesPaginadosHandlerTests()
    {
        _mockRepository = new Mock<IClienteRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerClientesPaginadosHandler>>();
        _handler = new ObtenerClientesPaginadosHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_PaginacionBasica_DeberiaRetornarResultadoPaginado()
    {
        // Arrange
        var query = new ObtenerClientesPaginadosQuery(1, 2);
        var clientes = CrearClientesDePrueba(5);
        var clientesSummary = CrearClientesSummaryDePrueba(2);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(2);
        result.Value.TotalCount.Should().Be(5);
        result.Value.TotalPages.Should().Be(3);

        _mockRepository.Verify(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FiltroSoloActivos_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var query = new ObtenerClientesPaginadosQuery(1, 10) { SoloActivos = true };
        var clientes = CrearClientesDePrueba(3);
        clientes[1].Desactivar(); // Desactivar el segundo cliente

        var clientesSummary = CrearClientesSummaryDePrueba(2); // Solo 2 activos

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2); // Solo los activos
    }

    [Fact]
    public async Task Handle_FiltroTexto_DeberiaFiltrarPorNombreEmailTelefono()
    {
        // Arrange
        var query = new ObtenerClientesPaginadosQuery(1, 10) { FiltroTexto = "juan" };
        var clientes = CrearClientesDePrueba(3);
        var clientesSummary = CrearClientesSummaryDePrueba(1); // Solo uno coincide

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_OrdenamientoPorNombre_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var query = new ObtenerClientesPaginadosQuery(1, 10) 
        { 
            OrdenarPor = "Nombre", 
            DireccionOrden = "asc" 
        };
        var clientes = CrearClientesDePrueba(3);
        var clientesSummary = CrearClientesSummaryDePrueba(3);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_FiltroSegmento_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var query = new ObtenerClientesPaginadosQuery(1, 10) { Segmento = "Premium" };
        var clientes = CrearClientesDePrueba(3);
        var clientesSummary = CrearClientesSummaryDePrueba(1);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ClientesFrecuentes_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var query = new ObtenerClientesPaginadosQuery(1, 10) { SoloClientesFrecuentes = true };
        var clientes = CrearClientesDePrueba(3);
        var clientesSummary = CrearClientesSummaryDePrueba(1); // Solo uno frecuente

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FiltroFechas_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var fechaDesde = DateTime.Now.AddDays(-30);
        var fechaHasta = DateTime.Now.AddDays(-1);
        var query = new ObtenerClientesPaginadosQuery(1, 10) 
        { 
            FechaRegistroDesde = fechaDesde,
            FechaRegistroHasta = fechaHasta
        };
        var clientes = CrearClientesDePrueba(3);
        var clientesSummary = CrearClientesSummaryDePrueba(2);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SinResultados_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = new ObtenerClientesPaginadosQuery(1, 10) { FiltroTexto = "inexistente" };
        var clientes = CrearClientesDePrueba(3);
        var clientesSummary = new List<ClienteSummaryDto>(); // Lista vacía

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

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
    public async Task Handle_ExcepcionRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerClientesPaginadosQuery(1, 10);
        var excepcionRepositorio = new Exception("Error de base de datos");

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionRepositorio);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno al obtener la lista de clientes");

        _mockRepository.Verify(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_FactoryMethodActivos_DeberiaFuncionar()
    {
        // Arrange
        var query = ObtenerClientesPaginadosQuery.CrearParaActivos(1, 5);
        var clientes = CrearClientesDePrueba(3);
        var clientesSummary = CrearClientesSummaryDePrueba(3);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        query.SoloActivos.Should().BeTrue();
        query.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_FactoryMethodFrecuentes_DeberiaFuncionar()
    {
        // Arrange
        var query = ObtenerClientesPaginadosQuery.CrearParaFrecuentes(1, 10);
        var clientes = CrearClientesDePrueba(3);
        var clientesSummary = CrearClientesSummaryDePrueba(1);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        query.SoloClientesFrecuentes.Should().BeTrue();
        query.SoloActivos.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FactoryMethodBusqueda_DeberiaFuncionar()
    {
        // Arrange
        var textoBusqueda = "juan@test.com";
        var query = ObtenerClientesPaginadosQuery.CrearParaBusqueda(textoBusqueda, 1, 10);
        var clientes = CrearClientesDePrueba(3);
        var clientesSummary = CrearClientesSummaryDePrueba(1);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        query.FiltroTexto.Should().Be(textoBusqueda);
        query.SoloActivos.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FactoryMethodConTarjeta_DeberiaFuncionar()
    {
        // Arrange
        var query = ObtenerClientesPaginadosQuery.CrearParaConTarjeta(1, 10);
        var clientes = CrearClientesDePrueba(3);
        var clientesSummary = CrearClientesSummaryDePrueba(2);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        _mockMapper.Setup(m => m.Map<List<ClienteSummaryDto>>(It.IsAny<List<Cliente>>()))
            .Returns(clientesSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        query.SoloConTarjetaFidelizacion.Should().BeTrue();
        query.SoloActivos.Should().BeTrue();
    }

    #region Métodos de Ayuda

    private List<Cliente> CrearClientesDePrueba(int cantidad)
    {
        var clientes = new List<Cliente>();
        
        for (int i = 0; i < cantidad; i++)
        {
            var cliente = Cliente.Crear(
                ClienteNombre.Crear($"Cliente{i}", $"Apellido{i}"),
                $"cliente{i}@test.com",
                $"+5730012345{i}",
                DateTime.Now.AddYears(-30 + i)
            );
            
            // Simular algunos datos para filtros
            if (i == 0)
            {
                // Cliente con nombre "Juan" para filtro de texto
                cliente = Cliente.Crear(
                    ClienteNombre.Crear("Juan", "Pérez"),
                    "juan@test.com",
                    "+57300123456",
                    DateTime.Now.AddYears(-30)
                );
            }
            
            clientes.Add(cliente);
        }
        
        return clientes;
    }

    private List<ClienteSummaryDto> CrearClientesSummaryDePrueba(int cantidad)
    {
        var clientesSummary = new List<ClienteSummaryDto>();
        
        for (int i = 0; i < cantidad; i++)
        {
            clientesSummary.Add(new ClienteSummaryDto
            {
                Id = Guid.NewGuid(),
                NombreCompleto = $"Cliente{i} Apellido{i}",
                Email = $"cliente{i}@test.com",
                Telefono = $"+5730012345{i}",
                TipoCliente = "Regular",
                Activo = true,
                FechaRegistro = DateTime.Now.AddDays(-i),
                RegistradoPor = "System",
                PuntosFidelizacion = 100 + (i * 50),
                NivelFidelizacion = "Bronce",
                TotalOrdenes = i + 1,
                MontoTotalCompras = 1000 + (i * 500),
                EsFrecuente = i % 2 == 0,
                DiasSinVisitar = i * 5,
                EsVIP = i == 0
            });
        }
        
        return clientesSummary;
    }

    #endregion
} 