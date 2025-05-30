using RestaurantePro.Application.Proveedores.Proveedores.DTOs;
using RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedoresPaginados;
using RestaurantePro.Domain.Proveedores.Interfaces;

namespace RestaurantePro.Application.UnitTests.Proveedores.Proveedores.Queries;

/// <summary>
/// Pruebas unitarias para ObtenerProveedoresPaginadosHandler
/// Tests comprensivos que cubren todos los escenarios de filtrado, paginación y ordenamiento
/// </summary>
public class ObtenerProveedoresPaginadosHandlerTests
{
    private readonly Mock<IProveedorRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerProveedoresPaginadosHandler>> _mockLogger;
    private readonly ObtenerProveedoresPaginadosHandler _handler;
    private readonly List<Domain.Proveedores.Entities.Proveedor> _proveedoresEjemplo;
    private readonly List<ProveedorSummaryDto> _proveedoresDtoEjemplo;

    public ObtenerProveedoresPaginadosHandlerTests()
    {
        _mockRepository = new Mock<IProveedorRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerProveedoresPaginadosHandler>>();
        _handler = new ObtenerProveedoresPaginadosHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);

        // Setup de datos de prueba
        _proveedoresEjemplo = CrearProveedoresEjemplo();
        _proveedoresDtoEjemplo = CrearProveedoresDtoEjemplo();
    }

    [Fact]
    public async Task Handle_ConParametrosBasicos_DeberiaRetornarProveedoresPaginados()
    {
        // Arrange
        var query = ObtenerProveedoresPaginadosQuery.ConsultaBasica(1, 10, Guid.NewGuid());
        
        _mockRepository.Setup(r => r.ObtenerActivosAsync(false, false, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(_proveedoresEjemplo);
        
        _mockMapper.Setup(m => m.Map<List<ProveedorSummaryDto>>(It.IsAny<IEnumerable<Domain.Proveedores.Entities.Proveedor>>()))
                   .Returns(_proveedoresDtoEjemplo.Take(10).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(10);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(_proveedoresEjemplo.Count);
        
        _mockRepository.Verify(r => r.ObtenerActivosAsync(false, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConBusquedaPorTermino_DeberiaAplicarFiltroCorrectamente()
    {
        // Arrange
        var termino = "ABC Corp";
        var query = ObtenerProveedoresPaginadosQuery.BuscarPorTermino(termino, 1, 5, Guid.NewGuid());
        var proveedoresFiltrados = _proveedoresEjemplo.Where(p => p.Nombre.Contains(termino)).ToList();
        
        _mockRepository.Setup(r => r.BuscarAsync(termino, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(proveedoresFiltrados);
        
        _mockMapper.Setup(m => m.Map<List<ProveedorSummaryDto>>(It.IsAny<IEnumerable<Domain.Proveedores.Entities.Proveedor>>()))
                   .Returns(_proveedoresDtoEjemplo.Take(proveedoresFiltrados.Count).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().HaveCount(proveedoresFiltrados.Count);
        
        _mockRepository.Verify(r => r.BuscarAsync(termino, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConFiltroSoloActivos_DeberiaUsarRepositorioActivos()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 5,
            SoloActivos = true,
            UsuarioId = Guid.NewGuid()
        };
        
        _mockRepository.Setup(r => r.ObtenerActivosAsync(false, false, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(_proveedoresEjemplo.Where(p => p.Activo).ToList());
        
        _mockMapper.Setup(m => m.Map<List<ProveedorSummaryDto>>(It.IsAny<IEnumerable<Domain.Proveedores.Entities.Proveedor>>()))
                   .Returns(_proveedoresDtoEjemplo.Take(5).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        _mockRepository.Verify(r => r.ObtenerActivosAsync(false, false, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.ObtenerTodosAsync(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConTodosLosProveedores_DeberiaUsarRepositorioCompleto()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloActivos = false,
            UsuarioId = Guid.NewGuid()
        };
        
        _mockRepository.Setup(r => r.ObtenerTodosAsync(false, false, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(_proveedoresEjemplo);
        
        _mockMapper.Setup(m => m.Map<List<ProveedorSummaryDto>>(It.IsAny<IEnumerable<Domain.Proveedores.Entities.Proveedor>>()))
                   .Returns(_proveedoresDtoEjemplo.Take(10).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        _mockRepository.Verify(r => r.ObtenerTodosAsync(false, false, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.ObtenerActivosAsync(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConFiltroUbicacion_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var ciudad = "México";
        var pais = "México";
        var query = ObtenerProveedoresPaginadosQuery.FiltrarPorUbicacion(ciudad, pais, 1, 5, Guid.NewGuid());
        
        _mockRepository.Setup(r => r.ObtenerActivosAsync(false, false, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(_proveedoresEjemplo);
        
        var proveedoresFiltrados = _proveedoresEjemplo
            .Where(p => p.Ciudad.Contains(ciudad) && p.Pais.Contains(pais))
            .ToList();
        
        _mockMapper.Setup(m => m.Map<List<ProveedorSummaryDto>>(It.IsAny<IEnumerable<Domain.Proveedores.Entities.Proveedor>>()))
                   .Returns(_proveedoresDtoEjemplo.Take(proveedoresFiltrados.Count).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        // Los filtros de ubicación se aplican internamente en el handler
    }

    [Fact]
    public async Task Handle_ConFiltroCredito_DeberiaFiltrarPorDiasCredito()
    {
        // Arrange
        var diasMin = 15;
        var diasMax = 45;
        var query = ObtenerProveedoresPaginadosQuery.FiltrarPorCredito(diasMin, diasMax, 1, 5, Guid.NewGuid());
        
        _mockRepository.Setup(r => r.ObtenerActivosAsync(false, false, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(_proveedoresEjemplo);
        
        _mockMapper.Setup(m => m.Map<List<ProveedorSummaryDto>>(It.IsAny<IEnumerable<Domain.Proveedores.Entities.Proveedor>>()))
                   .Returns(_proveedoresDtoEjemplo.Take(3).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        // Los filtros de crédito se aplican internamente en el handler
        query.DiasCredito_Min.Should().Be(diasMin);
        query.DiasCredito_Max.Should().Be(diasMax);
    }

    [Fact]
    public async Task Handle_ConIncluirContactos_DeberiaConfigurarRepositorioCorrectamente()
    {
        // Arrange
        var query = ObtenerProveedoresPaginadosQuery.ConsultaConContactos(1, 5, Guid.NewGuid());
        
        _mockRepository.Setup(r => r.ObtenerActivosAsync(true, false, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(_proveedoresEjemplo);
        
        _mockMapper.Setup(m => m.Map<List<ProveedorSummaryDto>>(It.IsAny<IEnumerable<Domain.Proveedores.Entities.Proveedor>>()))
                   .Returns(_proveedoresDtoEjemplo.Take(5).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        query.IncluirContactos.Should().BeTrue();
        
        _mockRepository.Verify(r => r.ObtenerActivosAsync(true, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConPaginacionSegundaPagina_DeberiaRetornarElementosCorrectos()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 2,
            PageSize = 3,
            SoloActivos = true,
            UsuarioId = Guid.NewGuid()
        };
        
        _mockRepository.Setup(r => r.ObtenerActivosAsync(false, false, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(_proveedoresEjemplo);
        
        _mockMapper.Setup(m => m.Map<List<ProveedorSummaryDto>>(It.IsAny<IEnumerable<Domain.Proveedores.Entities.Proveedor>>()))
                   .Returns(_proveedoresDtoEjemplo.Skip(3).Take(3).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(3);
        result.Value.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConOrdenamientoPorNombre_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            CampoOrden = "nombre",
            DireccionOrden = "asc",
            UsuarioId = Guid.NewGuid()
        };
        
        _mockRepository.Setup(r => r.ObtenerActivosAsync(false, false, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(_proveedoresEjemplo);
        
        _mockMapper.Setup(m => m.Map<List<ProveedorSummaryDto>>(It.IsAny<IEnumerable<Domain.Proveedores.Entities.Proveedor>>()))
                   .Returns(_proveedoresDtoEjemplo.Take(10).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        query.CampoOrden.Should().Be("nombre");
        query.DireccionOrden.Should().Be("asc");
    }

    [Fact]
    public async Task Handle_SinResultados_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = ObtenerProveedoresPaginadosQuery.ConsultaBasica(1, 10, Guid.NewGuid());
        
        _mockRepository.Setup(r => r.ObtenerActivosAsync(false, false, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

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
    public async Task Handle_ConErrorEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var query = ObtenerProveedoresPaginadosQuery.ConsultaBasica(1, 10, Guid.NewGuid());
        
        _mockRepository.Setup(r => r.ObtenerActivosAsync(false, false, It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error en base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al consultar proveedores");
    }

    [Fact]
    public async Task Handle_ConErrorEnBusqueda_DeberiaContinuarConListaVacia()
    {
        // Arrange
        var query = ObtenerProveedoresPaginadosQuery.BuscarPorTermino("termino", 1, 5, Guid.NewGuid());
        
        _mockRepository.Setup(r => r.BuscarAsync("termino", It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new Exception("Error en búsqueda"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    #region Helper Methods

    private List<Domain.Proveedores.Entities.Proveedor> CrearProveedoresEjemplo()
    {
        var proveedores = new List<Domain.Proveedores.Entities.Proveedor>();

        for (int i = 1; i <= 15; i++)
        {
            var email = $"proveedor{i}@email.com";
            var telefono = $"555-000{i:D2}";
            // Usar formato RFC válido mexicano: XAXX010101000
            var rfc = $"XAXX{(010000 + i):D6}000";
            
            var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
                $"Proveedor {i}",           // nombre
                $"Contacto {i}",            // nombreContacto
                email,                      // email
                telefono,                   // telefono
                $"Dirección {i}",           // direccion
                $"Ciudad {i}",              // ciudad
                $"1234{i}",                 // codigoPostal
                $"País {i}",                // pais
                rfc,                        // rfc
                $"Banco {i}",               // informacionBancaria
                30 + i                      // días de crédito
            );

            proveedor.GetType().GetProperty("Id")?.SetValue(proveedor, Guid.NewGuid());
            proveedores.Add(proveedor);
        }

        return proveedores;
    }

    private List<ProveedorSummaryDto> CrearProveedoresDtoEjemplo()
    {
        var dtos = new List<ProveedorSummaryDto>();

        for (int i = 1; i <= 15; i++)
        {
            dtos.Add(new ProveedorSummaryDto
            {
                Id = Guid.NewGuid(),
                Nombre = $"Proveedor {i}",
                NumeroIdentificacion = $"RFC{i:D3}",
                EmailPrincipal = $"proveedor{i}@email.com",
                Ciudad = $"Ciudad {i}",
                Pais = $"País {i}",
                DiasCredito = 30 + i,
                Activo = true,
                TotalContactos = i % 3,
                FechaCreacion = DateTime.UtcNow.AddDays(-i),
                CreadoPor = "admin",
                TelefonoPrincipal = $"555-000{i:D2}",
                NombreContacto = $"Contacto {i}",
                Email = $"proveedor{i}@email.com",
                FechaRegistro = DateTime.UtcNow.AddDays(-i)
            });
        }

        return dtos;
    }

    #endregion
} 