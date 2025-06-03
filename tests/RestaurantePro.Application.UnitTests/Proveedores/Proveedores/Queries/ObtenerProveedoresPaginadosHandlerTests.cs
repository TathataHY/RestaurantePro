namespace RestaurantePro.Application.UnitTests.Proveedores.Proveedores.Queries;

/// <summary>
/// Tests unitarios para ObtenerProveedoresPaginadosHandler
/// Valida la lógica de paginación, filtros avanzados y ordenamiento de proveedores
/// </summary>
public class ObtenerProveedoresPaginadosHandlerTests
{
    private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ObtenerProveedoresPaginadosHandler>> _loggerMock;
    private readonly ObtenerProveedoresPaginadosHandler _handler;

    public ObtenerProveedoresPaginadosHandlerTests()
    {
        _proveedorRepositoryMock = new Mock<IProveedorRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ObtenerProveedoresPaginadosHandler>>();

        _handler = new ObtenerProveedoresPaginadosHandler(
            _proveedorRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #region Tests de Factory Methods del Query

    [Fact]
    public void CrearConsultaPaginada_ConParametrosBasicos_DeberiaCrearQueryCorrectamente()
    {
        // Arrange
        var pagina = 1;
        var tamanoPagina = 10;

        // Act
        var query = ObtenerProveedoresPaginadosQuery.ConsultaBasica(pagina, tamanoPagina);

        // Assert
        Assert.Equal(pagina, query.PageNumber);
        Assert.Equal(tamanoPagina, query.PageSize);
        Assert.True(query.SoloActivos);
        Assert.Null(query.TerminoBusqueda);
        Assert.Equal("Nombre", query.CampoOrden);
        Assert.Equal("asc", query.DireccionOrden);
    }

    [Fact]
    public void CrearConsultaConFiltros_ConFiltrosAvanzados_DeberiaConfigurarFiltros()
    {
        // Arrange
        var filtroNombre = "Distribuidora";
        var incluirInactivos = false;

        // Act
        var query = ObtenerProveedoresPaginadosQuery.BuscarPorTermino(
            filtroNombre, 1, 20);

        // Assert
        Assert.Equal(filtroNombre, query.TerminoBusqueda);
        Assert.True(query.SoloActivos);
    }

    [Fact]
    public void CrearConsultaOrdenada_ConOrdenamiento_DeberiaConfigurarOrden()
    {
        // Arrange
        var ordenarPor = "FechaCreacion";
        var ascendente = false;

        // Act
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 15,
            CampoOrden = ordenarPor,
            DireccionOrden = ascendente ? "asc" : "desc"
        };

        // Assert
        Assert.Equal(ordenarPor, query.CampoOrden);
        Assert.Equal("desc", query.DireccionOrden);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_ConsultaPaginadaBasica_DeberiaRetornarProveedoresPaginados()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloActivos = true,
            CampoOrden = "Nombre",
            DireccionOrden = "asc"
        };

        var proveedoresEntidades = CreateMockProveedoresEntidades();
        var proveedoresDto = CreateMockProveedoresDto();
        var resultadoPaginado = CreateMockPaginatedList(proveedoresDto, 1, 10, 25);

        _proveedorRepositoryMock.Setup(x => x.ObtenerProveedoresPaginadosAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(),
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedoresEntidades);

        _proveedorRepositoryMock.Setup(x => x.ContarProveedoresAsync(
            It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(), It.IsAny<bool>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(25);

        _mapperMock.Setup(x => x.Map<IEnumerable<ProveedorDto>>(It.IsAny<IEnumerable<Proveedor>>()))
            .Returns(proveedoresDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(10, result.Value.Items.Count());
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(25, result.Value.TotalCount);
        Assert.Equal(3, result.Value.TotalPages);
        Assert.True(result.Value.HasPreviousPage == false);
        Assert.True(result.Value.HasNextPage);
    }

    [Fact]
    public async Task Handle_ConsultaConFiltroNombre_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            TerminoBusqueda = "Distribuidora",
            SoloActivos = true
        };

        var proveedoresFiltrados = CreateMockProveedoresFiltrados();
        var proveedoresDto = CreateMockProveedoresFiltradosDto();

        _proveedorRepositoryMock.Setup(x => x.ObtenerProveedoresPaginadosAsync(
            It.Is<int>(p => p == 1),
            It.Is<int>(t => t == 10),
            It.Is<string>(f => f == "Distribuidora"),
            It.IsAny<CategoriaProveedor?>(),
            It.Is<bool>(a => a == true),
            It.Is<bool>(i => i == false),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedoresFiltrados);

        _proveedorRepositoryMock.Setup(x => x.ContarProveedoresAsync(
            It.Is<string>(f => f == "Distribuidora"),
            It.IsAny<CategoriaProveedor?>(),
            It.Is<bool>(a => a == true),
            It.Is<bool>(i => i == false),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _mapperMock.Setup(x => x.Map<IEnumerable<ProveedorDto>>(It.IsAny<IEnumerable<Proveedor>>()))
            .Returns(proveedoresDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(5, result.Value.Items.Count());
        Assert.All(result.Value.Items, p => Assert.Contains("Distribuidora", p.Nombre));
    }

    [Fact]
    public async Task Handle_ConsultaConFiltroCategoria_DeberiaFiltrarPorCategoria()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloActivos = true,
            TerminoBusqueda = CategoriaProveedor.BebidasNoAlcoholicas.ToString(),
            CampoOrden = "Categoria",
            DireccionOrden = "asc"
        };

        var proveedoresBebidas = CreateMockProveedoresBebidas();
        var proveedoresDto = CreateMockProveedoresBebidasDto();

        _proveedorRepositoryMock.Setup(x => x.ObtenerProveedoresPaginadosAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
            It.Is<CategoriaProveedor?>(c => c == CategoriaProveedor.BebidasNoAlcoholicas),
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedoresBebidas);

        _proveedorRepositoryMock.Setup(x => x.ContarProveedoresAsync(
            It.IsAny<string>(),
            It.Is<CategoriaProveedor?>(c => c == CategoriaProveedor.BebidasNoAlcoholicas),
            It.IsAny<bool>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(8);

        _mapperMock.Setup(x => x.Map<IEnumerable<ProveedorDto>>(It.IsAny<IEnumerable<Proveedor>>()))
            .Returns(proveedoresDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.All(result.Value.Items, p => Assert.Equal(CategoriaProveedor.BebidasNoAlcoholicas, p.Categoria));
    }

    [Fact]
    public async Task Handle_ConsultaIncluyendoInactivos_DeberiaIncluirProveedoresInactivos()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloActivos = true,
            CampoOrden = "Nombre",
            DireccionOrden = "asc"
        };

        var proveedoresMixtos = CreateMockProveedoresMixtos();
        var proveedoresDto = CreateMockProveedoresMixtosDto();

        _proveedorRepositoryMock.Setup(x => x.ObtenerProveedoresPaginadosAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(),
            It.Is<bool>(a => a == true),
            It.Is<bool>(i => i == true),
            It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedoresMixtos);

        _proveedorRepositoryMock.Setup(x => x.ContarProveedoresAsync(
            It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(),
            It.Is<bool>(a => a == true),
            It.Is<bool>(i => i == true),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);

        _mapperMock.Setup(x => x.Map<IEnumerable<ProveedorDto>>(It.IsAny<IEnumerable<Proveedor>>()))
            .Returns(proveedoresDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Contains(result.Value.Items, p => p.Activo);
        Assert.Contains(result.Value.Items, p => !p.Activo);
    }

    [Fact]
    public async Task Handle_ConsultaOrdenadaPorFecha_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloActivos = true,
            CampoOrden = "FechaCreacion",
            DireccionOrden = "desc"
        };

        var proveedoresOrdenados = CreateMockProveedoresOrdenadosPorFecha();
        var proveedoresDto = CreateMockProveedoresOrdenadosDto();

        _proveedorRepositoryMock.Setup(x => x.ObtenerProveedoresPaginadosAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(),
            It.IsAny<bool>(), It.IsAny<bool>(),
            It.Is<string>(o => o == "FechaCreacion"),
            It.Is<bool>(a => a == false),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedoresOrdenados);

        _proveedorRepositoryMock.Setup(x => x.ContarProveedoresAsync(
            It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(), It.IsAny<bool>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        _mapperMock.Setup(x => x.Map<IEnumerable<ProveedorDto>>(It.IsAny<IEnumerable<Proveedor>>()))
            .Returns(proveedoresDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        var fechas = result.Value.Items.Select(p => p.FechaCreacion).ToList();
        Assert.True(fechas.SequenceEqual(fechas.OrderByDescending(f => f)));
    }

    [Fact]
    public async Task Handle_ConsultaPaginaVacia_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 10, // Página que no existe
            PageSize = 10,
            SoloActivos = true
        };

        _proveedorRepositoryMock.Setup(x => x.ObtenerProveedoresPaginadosAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(),
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Proveedor>());

        _proveedorRepositoryMock.Setup(x => x.ContarProveedoresAsync(
            It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(), It.IsAny<bool>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(25);

        _mapperMock.Setup(x => x.Map<IEnumerable<ProveedorDto>>(It.IsAny<IEnumerable<Proveedor>>()))
            .Returns(new List<ProveedorDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Empty(result.Value.Items);
        Assert.Equal(10, result.Value.PageNumber);
        Assert.Equal(25, result.Value.TotalCount);
    }

    #endregion

    #region Tests de Validaciones

    [Fact]
    public async Task Handle_PaginaInvalida_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 0, // Página inválida
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La página debe ser mayor a 0", result.Error);
    }

    [Fact]
    public async Task Handle_TamanoPaginaInvalido_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 0 // Tamaño inválido
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El tamaño de página debe ser mayor a 0", result.Error);
    }

    [Fact]
    public async Task Handle_TamanoPaginaExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 1000 // Tamaño excesivo
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El tamaño de página no puede superar los 100 elementos", result.Error);
    }

    [Fact]
    public async Task Handle_CampoOrdenamientoInvalido_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            CampoOrden = "CampoInexistente"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Campo de ordenamiento no válido", result.Error);
    }

    [Fact]
    public async Task Handle_SinProveedoresActivosNiInactivos_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloActivos = false,
            CampoOrden = "Nombre",
            DireccionOrden = "asc"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Debe incluir al menos proveedores activos o inactivos", result.Error);
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ErrorRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloActivos = true
        };

        _proveedorRepositoryMock.Setup(x => x.ObtenerProveedoresPaginadosAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(),
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error al obtener proveedores", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorMapeo_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloActivos = true
        };

        var proveedores = CreateMockProveedoresEntidades();

        _proveedorRepositoryMock.Setup(x => x.ObtenerProveedoresPaginadosAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(),
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedores);

        _proveedorRepositoryMock.Setup(x => x.ContarProveedoresAsync(
            It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(), It.IsAny<bool>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        _mapperMock.Setup(x => x.Map<IEnumerable<ProveedorDto>>(It.IsAny<IEnumerable<Proveedor>>()))
            .Throws(new Exception("Error de mapeo"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error al mapear proveedores", result.Error);
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_ConsultaExitosa_DeberiaLoggearProceso()
    {
        // Arrange
        var query = new ObtenerProveedoresPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloActivos = true
        };

        var proveedores = CreateMockProveedoresEntidades();
        var proveedoresDto = CreateMockProveedoresDto();

        _proveedorRepositoryMock.Setup(x => x.ObtenerProveedoresPaginadosAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(),
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(proveedores);

        _proveedorRepositoryMock.Setup(x => x.ContarProveedoresAsync(
            It.IsAny<string>(), It.IsAny<CategoriaProveedor?>(), It.IsAny<bool>(), It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        _mapperMock.Setup(x => x.Map<IEnumerable<ProveedorDto>>(It.IsAny<IEnumerable<Proveedor>>()))
            .Returns(proveedoresDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        // Verificar logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Obteniendo proveedores paginados")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Métodos Helper

    private static List<Proveedor> CreateMockProveedoresEntidades()
    {
        return new List<Proveedor>
        {
            CreateMockProveedor(Guid.NewGuid(), "Proveedor A", CategoriaProveedor.Carnes, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor B", CategoriaProveedor.FrutasVerduras, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor C", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor D", CategoriaProveedor.Lacteos, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor E", CategoriaProveedor.AlimentosBasicos, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor F", CategoriaProveedor.Especias, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor G", CategoriaProveedor.Carnes, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor H", CategoriaProveedor.FrutasVerduras, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor I", CategoriaProveedor.BebidasAlcoholicas, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor J", CategoriaProveedor.Lacteos, true)
        };
    }

    private static List<ProveedorDto> CreateMockProveedoresDto()
    {
        return new List<ProveedorDto>
        {
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor A", CategoriaProveedor.Carnes, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor B", CategoriaProveedor.FrutasVerduras, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor C", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor D", CategoriaProveedor.Lacteos, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor E", CategoriaProveedor.AlimentosBasicos, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor F", CategoriaProveedor.Especias, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor G", CategoriaProveedor.Carnes, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor H", CategoriaProveedor.FrutasVerduras, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor I", CategoriaProveedor.BebidasAlcoholicas, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor J", CategoriaProveedor.Lacteos, true)
        };
    }

    private static List<Proveedor> CreateMockProveedoresFiltrados()
    {
        return new List<Proveedor>
        {
            CreateMockProveedor(Guid.NewGuid(), "Distribuidora Norte", CategoriaProveedor.Carnes, true),
            CreateMockProveedor(Guid.NewGuid(), "Distribuidora Sur", CategoriaProveedor.FrutasVerduras, true),
            CreateMockProveedor(Guid.NewGuid(), "Distribuidora Central", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedor(Guid.NewGuid(), "Distribuidora Este", CategoriaProveedor.Lacteos, true),
            CreateMockProveedor(Guid.NewGuid(), "Distribuidora Oeste", CategoriaProveedor.AlimentosBasicos, true)
        };
    }

    private static List<ProveedorDto> CreateMockProveedoresFiltradosDto()
    {
        return new List<ProveedorDto>
        {
            CreateMockProveedorDto(Guid.NewGuid(), "Distribuidora Norte", CategoriaProveedor.Carnes, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Distribuidora Sur", CategoriaProveedor.FrutasVerduras, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Distribuidora Central", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Distribuidora Este", CategoriaProveedor.Lacteos, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Distribuidora Oeste", CategoriaProveedor.AlimentosBasicos, true)
        };
    }

    private static List<Proveedor> CreateMockProveedoresBebidas()
    {
        return new List<Proveedor>
        {
            CreateMockProveedor(Guid.NewGuid(), "Bebidas Premium", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedor(Guid.NewGuid(), "Refrescos SA", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedor(Guid.NewGuid(), "Aguas Minerales", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedor(Guid.NewGuid(), "Jugos Naturales", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedor(Guid.NewGuid(), "Cervezas Artesanales", CategoriaProveedor.BebidasAlcoholicas, true),
            CreateMockProveedor(Guid.NewGuid(), "Vinos Selectos", CategoriaProveedor.BebidasAlcoholicas, true),
            CreateMockProveedor(Guid.NewGuid(), "Licores Finos", CategoriaProveedor.BebidasAlcoholicas, true),
            CreateMockProveedor(Guid.NewGuid(), "Café Gourmet", CategoriaProveedor.BebidasNoAlcoholicas, true)
        };
    }

    private static List<ProveedorDto> CreateMockProveedoresBebidasDto()
    {
        return new List<ProveedorDto>
        {
            CreateMockProveedorDto(Guid.NewGuid(), "Bebidas Premium", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Refrescos SA", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Aguas Minerales", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Jugos Naturales", CategoriaProveedor.BebidasNoAlcoholicas, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Cervezas Artesanales", CategoriaProveedor.BebidasAlcoholicas, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Vinos Selectos", CategoriaProveedor.BebidasAlcoholicas, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Licores Finos", CategoriaProveedor.BebidasAlcoholicas, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Café Gourmet", CategoriaProveedor.BebidasNoAlcoholicas, true)
        };
    }

    private static List<Proveedor> CreateMockProveedoresMixtos()
    {
        return new List<Proveedor>
        {
            CreateMockProveedor(Guid.NewGuid(), "Proveedor Activo 1", CategoriaProveedor.Carnes, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor Activo 2", CategoriaProveedor.FrutasVerduras, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor Inactivo 1", CategoriaProveedor.BebidasNoAlcoholicas, false),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor Activo 3", CategoriaProveedor.Lacteos, true),
            CreateMockProveedor(Guid.NewGuid(), "Proveedor Inactivo 2", CategoriaProveedor.AlimentosBasicos, false)
        };
    }

    private static List<ProveedorDto> CreateMockProveedoresMixtosDto()
    {
        return new List<ProveedorDto>
        {
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor Activo 1", CategoriaProveedor.Carnes, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor Activo 2", CategoriaProveedor.FrutasVerduras, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor Inactivo 1", CategoriaProveedor.BebidasNoAlcoholicas, false),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor Activo 3", CategoriaProveedor.Lacteos, true),
            CreateMockProveedorDto(Guid.NewGuid(), "Proveedor Inactivo 2", CategoriaProveedor.AlimentosBasicos, false)
        };
    }

    private static List<Proveedor> CreateMockProveedoresOrdenadosPorFecha()
    {
        var fechaBase = DateTime.UtcNow;
        return new List<Proveedor>
        {
            CreateMockProveedorConFecha(Guid.NewGuid(), "Proveedor Reciente", fechaBase),
            CreateMockProveedorConFecha(Guid.NewGuid(), "Proveedor Medio", fechaBase.AddDays(-5)),
            CreateMockProveedorConFecha(Guid.NewGuid(), "Proveedor Antiguo", fechaBase.AddDays(-10))
        };
    }

    private static List<ProveedorDto> CreateMockProveedoresOrdenadosDto()
    {
        var fechaBase = DateTime.UtcNow;
        return new List<ProveedorDto>
        {
            CreateMockProveedorDtoConFecha(Guid.NewGuid(), "Proveedor Reciente", fechaBase),
            CreateMockProveedorDtoConFecha(Guid.NewGuid(), "Proveedor Medio", fechaBase.AddDays(-5)),
            CreateMockProveedorDtoConFecha(Guid.NewGuid(), "Proveedor Antiguo", fechaBase.AddDays(-10))
        };
    }

    private static Proveedor CreateMockProveedor(Guid id, string nombre, CategoriaProveedor categoria, bool activo)
    {
        // Usar el método factory para crear un proveedor válido
        var proveedor = Proveedor.Crear(
            nombre: nombre,
            nombreContacto: "Contacto Test",
            email: "test@test.com",
            telefono: "+1234567890",
            direccion: "Dirección Test",
            ciudad: "Ciudad Test",
            codigoPostal: "12345",
            pais: "País Test",
            rfc: "ABCD123456",
            informacionBancaria: "Banco Test",
            diasCredito: 30
        );

        // Usar reflection para establecer el ID y estado activo si es necesario
        var idProperty = typeof(Proveedor).BaseType.GetProperty("Id");
        idProperty?.SetValue(proveedor, id);

        if (!activo)
        {
            proveedor.Desactivar("Proveedor de prueba desactivado");
        }

        proveedor.AgregarCategoria(categoria);

        return proveedor;
    }

    private static ProveedorDto CreateMockProveedorDto(Guid id, string nombre, CategoriaProveedor categoria, bool activo)
    {
        return new ProveedorDto
        {
            Id = id,
            Nombre = nombre,
            Categoria = categoria,
            Activo = activo,
            FechaCreacion = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
        };
    }

    private static Proveedor CreateMockProveedorConFecha(Guid id, string nombre, DateTime fecha)
    {
        // Usar el método factory para crear un proveedor válido
        var proveedor = Proveedor.Crear(
            nombre: nombre,
            nombreContacto: "Contacto Test",
            email: "test@test.com",
            telefono: "+1234567890",
            direccion: "Dirección Test",
            ciudad: "Ciudad Test",
            codigoPostal: "12345",
            pais: "País Test",
            rfc: "ABCD123456",
            informacionBancaria: "Banco Test",
            diasCredito: 30
        );

        // Usar reflection para establecer el ID y fecha de creación
        var idProperty = typeof(Proveedor).BaseType.GetProperty("Id");
        idProperty?.SetValue(proveedor, id);

        var fechaProperty = typeof(Proveedor).BaseType.GetProperty("FechaCreacion");
        fechaProperty?.SetValue(proveedor, fecha);

        return proveedor;
    }

    private static ProveedorDto CreateMockProveedorDtoConFecha(Guid id, string nombre, DateTime fecha)
    {
        return new ProveedorDto
        {
            Id = id,
            Nombre = nombre,
            Categoria = CategoriaProveedor.Carnes,
            Activo = true,
            FechaCreacion = fecha
        };
    }

    private static PaginatedList<ProveedorDto> CreateMockPaginatedList(
        IEnumerable<ProveedorDto> items, int pagina, int tamanoPagina, int totalElementos)
    {
        return new PaginatedList<ProveedorDto>(
            items.ToList(), totalElementos, pagina, tamanoPagina);
    }

    #endregion
} 