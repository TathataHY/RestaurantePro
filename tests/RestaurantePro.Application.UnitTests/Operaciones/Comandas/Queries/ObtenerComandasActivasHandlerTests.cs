namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Queries;

/// <summary>
/// Tests unitarios para ObtenerComandasActivasHandler
/// Valida consultas paginadas con filtros avanzados para dashboard operativo
/// </summary>
public class ObtenerComandasActivasHandlerTests
{
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ObtenerComandasActivasHandler>> _loggerMock;
    private readonly ObtenerComandasActivasHandler _handler;

    public ObtenerComandasActivasHandlerTests()
    {
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ObtenerComandasActivasHandler>>();
        
        _handler = new ObtenerComandasActivasHandler(
            _comandaRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #region Tests de Consultas Básicas

    [Fact]
    public async Task Handle_ConsultaSinFiltros_DeberiaRetornarComandasActivasPaginadas()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var comandas = CreateMockComandas();
        var comandasDto = CreateMockComandaSummaryDtos();
        
        var expected = new PaginatedList<ComandaSummaryDto>(comandasDto, 5, 1, 10);

        SetupRepositoryQuery(comandas, 5);
        SetupMapperToSummaryDto(comandas, comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(5, result.Value.Items.Count);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
        
        VerifyRepositoryQueryCalled();
    }

    [Fact]
    public async Task Handle_ConsultaConPaginacion_DeberiaRetornarPaginaCorrecta()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 2,
            PageSize = 3
        };

        var comandas = CreateMockComandas().Take(3).ToList();
        var comandasDto = CreateMockComandaSummaryDtos().Take(3).ToList();
        
        SetupRepositoryQuery(comandas, 8); // 8 total, página 2 con 3 items
        SetupMapperToSummaryDto(comandas, comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(8, result.Value.TotalCount);
        Assert.Equal(3, result.Value.Items.Count);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.Equal(3, result.Value.PageSize);
    }

    #endregion

    #region Tests de Filtros Básicos

    [Fact]
    public async Task Handle_FiltrarPorEstado_DeberiaAplicarFiltroCorrectamente()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            EstadoFiltro = "EnProceso"
        };

        var comandas = CreateMockComandasPorEstado(EstadoComanda.EnProceso);
        var comandasDto = CreateMockComandaSummaryDtos().Take(2).ToList();
        
        SetupRepositoryQuery(comandas, 2);
        SetupMapperToSummaryDto(comandas, comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.Items.Count);
        
        // Verify que el filtro fue aplicado en la consulta
        VerifyRepositoryQueryCalledWithCriteria("Estado", "EnProceso");
    }

    [Fact]
    public async Task Handle_FiltrarPorMesa_DeberiaAplicarFiltroCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            MesaId = mesaId
        };

        var comandas = CreateMockComandasPorMesa(mesaId);
        var comandasDto = CreateMockComandaSummaryDtos().Take(1).ToList();
        
        SetupRepositoryQuery(comandas, 1);
        SetupMapperToSummaryDto(comandas, comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value.Items.Count);
        
        VerifyRepositoryQueryCalledWithCriteria("MesaId", mesaId);
    }

    [Fact]
    public async Task Handle_FiltrarPorMesero_DeberiaAplicarFiltroCorrectamente()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            MeseroId = meseroId
        };

        var comandas = CreateMockComandasPorMesero(meseroId);
        var comandasDto = CreateMockComandaSummaryDtos().Take(2).ToList();
        
        SetupRepositoryQuery(comandas, 2);
        SetupMapperToSummaryDto(comandas, comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.Items.Count);
        
        VerifyRepositoryQueryCalledWithCriteria("MeseroId", meseroId);
    }

    [Fact]
    public async Task Handle_FiltrarPorCliente_DeberiaAplicarFiltroCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            ClienteId = clienteId
        };

        var comandas = CreateMockComandasPorCliente(clienteId);
        var comandasDto = CreateMockComandaSummaryDtos().Take(1).ToList();
        
        SetupRepositoryQuery(comandas, 1);
        SetupMapperToSummaryDto(comandas, comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value.Items.Count);
        
        VerifyRepositoryQueryCalledWithCriteria("ClienteId", clienteId);
    }

    #endregion

    #region Tests de Filtros de Fecha

    [Fact]
    public async Task Handle_SoloHoy_DeberiaFiltrarPorFechaActual()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloHoy = true
        };

        var comandas = CreateMockComandasHoy();
        var comandasDto = CreateMockComandaSummaryDtos().Take(3).ToList();
        
        SetupRepositoryQuery(comandas, 3);
        SetupMapperToSummaryDto(comandas, comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Value.Items.Count);
        
        // Verify que se aplicaron los filtros de fecha de hoy
        VerifyRepositoryQueryCalledWithDateRange(DateTime.Today, DateTime.Today.AddDays(1).AddTicks(-1));
    }

    [Fact]
    public async Task Handle_FechaEspecifica_DeberiaFiltrarPorFechaSeleccionada()
    {
        // Arrange
        var fechaEspecifica = new DateTime(2025, 1, 15);
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            FechaEspecifica = fechaEspecifica
        };

        var comandas = CreateMockComandasFecha(fechaEspecifica);
        var comandasDto = CreateMockComandaSummaryDtos().Take(2).ToList();
        
        SetupRepositoryQuery(comandas, 2);
        SetupMapperToSummaryDto(comandas, comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.Items.Count);
        
        var fechaInicio = fechaEspecifica.Date;
        var fechaFin = fechaInicio.AddDays(1).AddTicks(-1);
        VerifyRepositoryQueryCalledWithDateRange(fechaInicio, fechaFin);
    }

    #endregion

    #region Tests de Filtros Avanzados

    [Fact]
    public async Task Handle_SoloAtrasadas_DeberiaFiltrarComandasAtrasadas()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloAtrasadas = true
        };

        var comandas = CreateMockComandasAtrasadas();
        var comandasDto = CreateMockComandaSummaryDtos().Take(2).ToList();
        
        SetupRepositoryQuery(comandas, 5); // 5 totales en repo
        SetupMapperToSummaryDto(comandas.Take(2).ToList(), comandasDto); // Solo 2 atrasadas después del filtro

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.Items.Count); // Solo las atrasadas
    }

    [Fact]
    public async Task Handle_SoloConDescuentos_DeberiaFiltrarComandasConDescuento()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloConDescuentos = true
        };

        var comandas = CreateMockComandasConDescuentos();
        var comandasDto = CreateMockComandaSummaryDtos().Take(1).ToList();
        
        SetupRepositoryQuery(comandas, 3); // 3 totales en repo
        SetupMapperToSummaryDto(comandas.Take(1).ToList(), comandasDto); // Solo 1 con descuento después del filtro

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value.Items.Count); // Solo la con descuento
    }

    [Fact]
    public async Task Handle_FiltrosCombinados_DeberiaAplicarTodosLosFiltros()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            EstadoFiltro = "EnProceso",
            MesaId = mesaId,
            SoloHoy = true,
            SoloAtrasadas = true
        };

        var comandas = CreateMockComandasConFiltrosCombinados();
        var comandasDto = CreateMockComandaSummaryDtos().Take(1).ToList();
        
        SetupRepositoryQuery(comandas, 1);
        SetupMapperToSummaryDto(comandas, comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value.Items.Count);
        
        // Verify múltiples criterios aplicados
        VerifyRepositoryQueryCalledWithCriteria("Estado", "EnProceso");
        VerifyRepositoryQueryCalledWithCriteria("MesaId", mesaId);
    }

    #endregion

    #region Tests de Estados por Defecto

    [Fact]
    public async Task Handle_SinFiltroEstado_DeberiaExcluirFinalizadasYCanceladas()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10
            // Sin EstadoFiltro - debe excluir Finalizada y Cancelada por defecto
        };

        var comandas = CreateMockComandasActivasSolamente();
        var comandasDto = CreateMockComandaSummaryDtos();
        
        SetupRepositoryQuery(comandas, 5);
        SetupMapperToSummaryDto(comandas, comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify que se excluyeron estados no activos
        VerifyRepositoryQueryCalledWithCriteria("EstadosExcluidos", new[] { "Finalizada", "Cancelada" });
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ErrorEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        _comandaRepositoryMock.Setup(x => x.ObtenerComandasActivasAsync(It.IsAny<Dictionary<string, object>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error de base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Ocurrió un error interno al consultar las comandas", result.Error);
    }

    [Fact]
    public async Task Handle_ParametrosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = -1, // Página inválida
            PageSize = 10
        };

        SetupRepositoryToThrowArgumentException();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("parámetros", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_ConsultaVacia_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = new ObtenerComandasActivasQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        SetupRepositoryQuery(new List<Comanda>(), 0);
        SetupMapperToSummaryDto(new List<Comanda>(), new List<ComandaSummaryDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Empty(result.Value.Items);
    }

    #endregion

    #region Helper Methods - Setup

    private void SetupRepositoryQuery(List<Comanda> comandas, int totalCount)
    {
        _comandaRepositoryMock.Setup(x => x.ObtenerComandasActivasAsync(
                It.IsAny<Dictionary<string, object>>(), 
                It.IsAny<int>(), 
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((comandas, totalCount));
    }

    private void SetupMapperToSummaryDto(List<Comanda> comandas, List<ComandaSummaryDto> dto)
    {
        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(comandas))
            .Returns(dto);
    }

    private void SetupRepositoryToThrowArgumentException()
    {
        _comandaRepositoryMock.Setup(x => x.ObtenerComandasActivasAsync(
                It.IsAny<Dictionary<string, object>>(), 
                It.IsAny<int>(), 
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Parámetros de paginación inválidos"));
    }

    #endregion

    #region Helper Methods - Verification

    private void VerifyRepositoryQueryCalled()
    {
        _comandaRepositoryMock.Verify(x => x.ObtenerComandasActivasAsync(
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private void VerifyRepositoryQueryCalledWithCriteria(string key, object value)
    {
        _comandaRepositoryMock.Verify(x => x.ObtenerComandasActivasAsync(
            It.Is<Dictionary<string, object>>(d => d.ContainsKey(key) && d[key].Equals(value)),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private void VerifyRepositoryQueryCalledWithDateRange(DateTime fechaInicio, DateTime fechaFin)
    {
        _comandaRepositoryMock.Verify(x => x.ObtenerComandasActivasAsync(
            It.Is<Dictionary<string, object>>(d => 
                d.ContainsKey("FechaInicio") && 
                d.ContainsKey("FechaFin") &&
                ((DateTime)d["FechaInicio"]).Date == fechaInicio.Date &&
                ((DateTime)d["FechaFin"]).Date >= fechaFin.Date),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Methods - Data Creation

    private static List<Comanda> CreateMockComandas()
    {
        var comandas = new List<Comanda>();
        for (int i = 1; i <= 5; i++)
        {
            var meseroId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var observaciones = $"Observaciones {i}";
            
            var comanda = Comanda.Crear(meseroId, clienteId, mesaId, observaciones);
            comandas.Add(comanda);
        }
        return comandas;
    }

    private static List<ComandaSummaryDto> CreateMockComandaSummaryDtos()
    {
        var dtos = new List<ComandaSummaryDto>();
        for (int i = 1; i <= 5; i++)
        {
            dtos.Add(new ComandaSummaryDto
            {
                Id = Guid.NewGuid(),
                NumeroComanda = $"COM-{i:000}",
                Estado = ((EstadoComanda)(i % 3 + 1)).ToString(),
                FechaCreacion = DateTime.Now.AddMinutes(-i * 10),
                Total = 50.00m + (i * 10),
                TotalItems = i + 2,
                NombreMesero = $"Mesero {i}",
                NumeroMesa = i.ToString()
            });
        }
        return dtos;
    }

    private static List<Comanda> CreateMockComandasPorEstado(EstadoComanda estado)
    {
        var comandas = new List<Comanda>();
        for (int i = 1; i <= 2; i++)
        {
            var meseroId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var comanda = Comanda.Crear(meseroId, null, mesaId);
            // Usamos reflexión para cambiar el estado ya que no hay método público
            var estadoProperty = typeof(Comanda).GetProperty("Estado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            estadoProperty?.SetValue(comanda, estado);
            comandas.Add(comanda);
        }
        return comandas;
    }

    private static List<Comanda> CreateMockComandasPorMesa(Guid mesaId)
    {
        var meseroId = Guid.NewGuid();
        var comanda = Comanda.Crear(meseroId, null, mesaId);
        return new List<Comanda> { comanda };
    }

    private static List<Comanda> CreateMockComandasPorMesero(Guid meseroId)
    {
        var comandas = new List<Comanda>();
        for (int i = 1; i <= 2; i++)
        {
            var mesaId = Guid.NewGuid();
            var comanda = Comanda.Crear(meseroId, null, mesaId);
            comandas.Add(comanda);
        }
        return comandas;
    }

    private static List<Comanda> CreateMockComandasPorCliente(Guid clienteId)
    {
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var comanda = Comanda.Crear(meseroId, clienteId, mesaId);
        return new List<Comanda> { comanda };
    }

    private static List<Comanda> CreateMockComandasHoy()
    {
        var comandas = new List<Comanda>();
        for (int i = 1; i <= 3; i++)
        {
            var meseroId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var comanda = Comanda.Crear(meseroId, null, mesaId);
            
            // Usamos reflexión para cambiar la fecha de creación
            var fechaProperty = typeof(Comanda).GetProperty("FechaCreacion", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            fechaProperty?.SetValue(comanda, DateTime.Today.AddHours(i * 2));
            
            comandas.Add(comanda);
        }
        return comandas;
    }

    private static List<Comanda> CreateMockComandasFecha(DateTime fecha)
    {
        var comandas = new List<Comanda>();
        for (int i = 1; i <= 2; i++)
        {
            var meseroId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var comanda = Comanda.Crear(meseroId, null, mesaId);
            
            // Usamos reflexión para cambiar la fecha de creación
            var fechaProperty = typeof(Comanda).GetProperty("FechaCreacion", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            fechaProperty?.SetValue(comanda, fecha.AddHours(i));
            
            comandas.Add(comanda);
        }
        return comandas;
    }

    private static List<Comanda> CreateMockComandasAtrasadas()
    {
        var comandas = new List<Comanda>();
        
        // Comanda creada hace 10 minutos (atrasada)
        var meseroId1 = Guid.NewGuid();
        var mesaId1 = Guid.NewGuid();
        var comandaAtrasada1 = Comanda.Crear(meseroId1, null, mesaId1);
        var fechaProperty = typeof(Comanda).GetProperty("FechaCreacion", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        fechaProperty?.SetValue(comandaAtrasada1, DateTime.Now.AddMinutes(-10));
        comandas.Add(comandaAtrasada1);
        
        // Comanda en proceso hace 40 minutos (atrasada)
        var meseroId2 = Guid.NewGuid();
        var mesaId2 = Guid.NewGuid();
        var comandaAtrasada2 = Comanda.Crear(meseroId2, null, mesaId2);
        fechaProperty?.SetValue(comandaAtrasada2, DateTime.Now.AddMinutes(-40));
        var estadoProperty = typeof(Comanda).GetProperty("Estado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        estadoProperty?.SetValue(comandaAtrasada2, EstadoComanda.EnProceso);
        comandas.Add(comandaAtrasada2);
        
        return comandas;
    }

    private static List<Comanda> CreateMockComandasConDescuentos()
    {
        var comandas = new List<Comanda>();
        
        // Comanda con descuento
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var comandaConDescuento = Comanda.Crear(meseroId, clienteId, mesaId);
        
        // Usamos reflexión para establecer el descuento de fidelización
        var descuentoProperty = typeof(Comanda).GetProperty("DescuentoFidelizacion", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        descuentoProperty?.SetValue(comandaConDescuento, 10.50m);
        
        comandas.Add(comandaConDescuento);
        
        return comandas;
    }

    private static List<Comanda> CreateMockComandasConFiltrosCombinados()
    {
        var meseroId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var comanda = Comanda.Crear(meseroId, null, mesaId);
        
        // Usamos reflexión para establecer las propiedades
        var estadoProperty = typeof(Comanda).GetProperty("Estado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        estadoProperty?.SetValue(comanda, EstadoComanda.EnProceso);
        
        var fechaProperty = typeof(Comanda).GetProperty("FechaCreacion", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        fechaProperty?.SetValue(comanda, DateTime.Now.AddMinutes(-40)); // Atrasada
        
        return new List<Comanda> { comanda };
    }

    private static List<Comanda> CreateMockComandasActivasSolamente()
    {
        var comandas = new List<Comanda>();
        var estadosActivos = new[] { EstadoComanda.Creada, EstadoComanda.EnProceso, EstadoComanda.Lista };
        
        for (int i = 0; i < 5; i++)
        {
            var meseroId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var comanda = Comanda.Crear(meseroId, null, mesaId);
            
            // Usamos reflexión para cambiar el estado
            var estadoProperty = typeof(Comanda).GetProperty("Estado", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            estadoProperty?.SetValue(comanda, estadosActivos[i % estadosActivos.Length]);
            
            comandas.Add(comanda);
        }
        return comandas;
    }

    #endregion
} 