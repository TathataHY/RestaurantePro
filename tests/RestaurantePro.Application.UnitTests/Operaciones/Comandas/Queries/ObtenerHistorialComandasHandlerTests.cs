namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Queries;

/// <summary>
/// Tests unitarios para ObtenerHistorialComandasHandler
/// Valida la lógica de consulta histórica con filtros avanzados y estadísticas
/// </summary>
public class ObtenerHistorialComandasHandlerTests
{
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ObtenerHistorialComandasHandler>> _loggerMock;
    private readonly ObtenerHistorialComandasHandler _handler;

    public ObtenerHistorialComandasHandlerTests()
    {
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ObtenerHistorialComandasHandler>>();
        
        _handler = new ObtenerHistorialComandasHandler(
            _comandaRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #region Tests de Factory Methods

    [Fact]
    public void Basico_ConParametrosValidos_DeberiaCrearQueryCorrectamente()
    {
        // Act
        var query = ObtenerHistorialComandasQuery.Basico(2, 15);

        // Assert
        Assert.Equal(2, query.PageNumber);
        Assert.Equal(15, query.PageSize);
        Assert.True(query.SoloFinalizadas);
        Assert.False(query.IncluirCanceladas);
        Assert.Equal(OrdenHistorial.FechaMasReciente, query.OrdenarPor);
        Assert.True(query.FechaDesde.HasValue);
    }

    [Fact]
    public void PorMesa_ConMesaValida_DeberiaConfigurarCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var fechaDesde = DateTime.Today.AddDays(-14);

        // Act
        var query = ObtenerHistorialComandasQuery.PorMesa(mesaId, fechaDesde, 3);

        // Assert
        Assert.Equal(mesaId, query.MesaId);
        Assert.Equal(3, query.PageNumber);
        Assert.Equal(50, query.PageSize);
        Assert.Equal(fechaDesde, query.FechaDesde);
        Assert.True(query.SoloFinalizadas);
    }

    [Fact]
    public void PorMesero_ConMeseroValido_DeberiaConfigurarCorrectamente()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var fechaDesde = DateTime.Today.AddDays(-5);

        // Act
        var query = ObtenerHistorialComandasQuery.PorMesero(meseroId, fechaDesde, 2);

        // Assert
        Assert.Equal(meseroId, query.MeseroId);
        Assert.Equal(2, query.PageNumber);
        Assert.Equal(30, query.PageSize);
        Assert.Equal(fechaDesde, query.FechaDesde);
        Assert.Equal(OrdenHistorial.MontoMayor, query.OrdenarPor);
    }

    [Fact]
    public void PorCliente_ConClienteValido_DeberiaIncluirCanceladas()
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act
        var query = ObtenerHistorialComandasQuery.PorCliente(clienteId, 1, 25);

        // Assert
        Assert.Equal(clienteId, query.ClienteId);
        Assert.Equal(25, query.PageSize);
        Assert.True(query.IncluirCanceladas); // Para clientes incluye canceladas
        Assert.True(query.SoloFinalizadas);
    }

    [Fact]
    public void PorRangoFechas_ConFechasValidas_DeberiaConfigurarRango()
    {
        // Arrange
        var fechaDesde = new DateTime(2025, 1, 1);
        var fechaHasta = new DateTime(2025, 1, 31);

        // Act
        var query = ObtenerHistorialComandasQuery.PorRangoFechas(fechaDesde, fechaHasta, 2);

        // Assert
        Assert.Equal(fechaDesde.Date, query.FechaDesde);
        Assert.True(query.FechaHasta.HasValue);
        Assert.Equal(2, query.PageNumber);
        Assert.Equal(50, query.PageSize);
    }

    [Fact]
    public void PorRangoMonto_ConMontosValidos_DeberiaConfigurarFiltros()
    {
        // Arrange
        var montoMin = 50.00m;
        var montoMax = 200.00m;

        // Act
        var query = ObtenerHistorialComandasQuery.PorRangoMonto(montoMin, montoMax, 1);

        // Assert
        Assert.Equal(montoMin, query.MontoMinimo);
        Assert.Equal(montoMax, query.MontoMaximo);
        Assert.Equal(OrdenHistorial.MontoMayor, query.OrdenarPor);
    }

    [Fact]
    public void ParaAnalisis_ConParametrosPorDefecto_DeberiaIncluirTodo()
    {
        // Act
        var query = ObtenerHistorialComandasQuery.ParaAnalisis();

        // Assert
        Assert.Equal(100, query.PageSize);
        Assert.False(query.SoloFinalizadas); // Incluye todos los estados
        Assert.True(query.IncluirCanceladas);
        Assert.True(query.FechaDesde.HasValue);
    }

    #endregion

    #region Tests de Consultas con Rangos de Fechas

    [Fact]
    public async Task Handle_ConRangoFechas_DeberiaUsarObtenerPorRangoFechasAsync()
    {
        // Arrange
        var fechaDesde = DateTime.Today.AddDays(-7);
        var fechaHasta = DateTime.Today;
        
        var query = new ObtenerHistorialComandasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        };

        var comandas = CreateMockComandas(5);
        var comandasDto = CreateMockComandaSummaryDtos(5);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorRangoFechasAsync(
                fechaDesde, fechaHasta, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandas);

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Returns(comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(5, result.Value.Items.Count);

        _comandaRepositoryMock.Verify(x => x.ObtenerPorRangoFechasAsync(
            fechaDesde, fechaHasta, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConRangoFechasYPaginacion_DeberiaAplicarPaginacionCorrectamente()
    {
        // Arrange
        var query = new ObtenerHistorialComandasQuery
        {
            PageNumber = 2,
            PageSize = 3,
            FechaDesde = DateTime.Today.AddDays(-5),
            FechaHasta = DateTime.Today
        };

        var comandas = CreateMockComandas(8); // 8 comandas total
        var comandasDto = CreateMockComandaSummaryDtos(3); // Solo 3 en la página 2

        _comandaRepositoryMock.Setup(x => x.ObtenerPorRangoFechasAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandas);

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(
                It.Is<List<Comanda>>(list => list.Count == 3))) // Verifica paginación
            .Returns(comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(8, result.Value.TotalCount); // Total original
        Assert.Equal(3, result.Value.Items.Count); // Items en la página
        Assert.Equal(2, result.Value.PageNumber);
    }

    #endregion

    #region Tests de Consultas Paginadas

    [Fact]
    public async Task Handle_SinRangoFechas_DeberiaUsarObtenerPaginadoAsync()
    {
        // Arrange
        var query = new ObtenerHistorialComandasQuery
        {
            PageNumber = 1,
            PageSize = 20
        };

        var comandas = CreateMockComandas(15);
        var comandasDto = CreateMockComandaSummaryDtos(15);
        var resultadoPaginado = (Comandas: comandas, Total: 45);

        _comandaRepositoryMock.Setup(x => x.ObtenerPaginadoAsync(
                0, 20, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultadoPaginado);

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(comandas))
            .Returns(comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(45, result.Value.TotalCount);
        Assert.Equal(15, result.Value.Items.Count);

        _comandaRepositoryMock.Verify(x => x.ObtenerPaginadoAsync(
            0, 20, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Tests de Filtros en Memoria

    [Fact]
    public async Task Handle_ConFiltroMesa_DeberiaFiltrarCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var query = new ObtenerHistorialComandasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            MesaId = mesaId,
            FechaDesde = DateTime.Today.AddDays(-7),
            FechaHasta = DateTime.Today
        };

        var comandas = CreateMockComandasConMesas(mesaId);
        var comandasDto = CreateMockComandaSummaryDtos(2);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorRangoFechasAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandas);

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(
                It.Is<List<Comanda>>(list => list.All(c => c.MesaId == mesaId))))
            .Returns(comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task Handle_ConFiltroMonto_DeberiaFiltrarPorRango()
    {
        // Arrange
        var query = new ObtenerHistorialComandasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            MontoMinimo = 50.00m,
            MontoMaximo = 100.00m,
            FechaDesde = DateTime.Today.AddDays(-7),
            FechaHasta = DateTime.Today
        };

        var comandas = CreateMockComandasConMontos();
        var comandasDto = CreateMockComandaSummaryDtos(2);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorRangoFechasAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandas);

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(
                It.Is<List<Comanda>>(list => 
                    list.All(c => c.Total != null && c.Total.Total >= 50.00m && c.Total.Total <= 100.00m))))
            .Returns(comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task Handle_ConFiltroEstado_DeberiaFiltrarPorEstado()
    {
        // Arrange
        var query = new ObtenerHistorialComandasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            Estado = EstadoComanda.Finalizada,
            FechaDesde = DateTime.Today.AddDays(-7),
            FechaHasta = DateTime.Today
        };

        var comandas = CreateMockComandasConEstados();
        var comandasDto = CreateMockComandaSummaryDtos(3);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorRangoFechasAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandas);

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(
                It.Is<List<Comanda>>(list => list.All(c => c.Estado == EstadoComanda.Finalizada))))
            .Returns(comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Value.Items.Count);
    }

    [Fact]
    public async Task Handle_ConTerminoBusqueda_DeberiaFiltrarPorObservaciones()
    {
        // Arrange
        var query = new ObtenerHistorialComandasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            TerminoBusqueda = "especial",
            FechaDesde = DateTime.Today.AddDays(-7),
            FechaHasta = DateTime.Today
        };

        var comandas = CreateMockComandasConObservaciones();
        var comandasDto = CreateMockComandaSummaryDtos(1);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorRangoFechasAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandas);

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(
                It.Is<List<Comanda>>(list => 
                    list.All(c => c.Observaciones.Contains("especial", StringComparison.OrdinalIgnoreCase)))))
            .Returns(comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value.Items.Count);
    }

    [Fact]
    public async Task Handle_SinIncluirCanceladas_DeberiaExcluirCanceladas()
    {
        // Arrange
        var query = new ObtenerHistorialComandasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            IncluirCanceladas = false,
            FechaDesde = DateTime.Today.AddDays(-7),
            FechaHasta = DateTime.Today
        };

        var comandas = CreateMockComandasConCanceladas();
        var comandasDto = CreateMockComandaSummaryDtos(2);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorRangoFechasAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandas);

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(
                It.Is<List<Comanda>>(list => list.All(c => c.Estado != EstadoComanda.Cancelada))))
            .Returns(comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task Handle_SoloFinalizadas_DeberiaFiltrarSoloFinalizadas()
    {
        // Arrange
        var query = new ObtenerHistorialComandasQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloFinalizadas = true,
            FechaDesde = DateTime.Today.AddDays(-7),
            FechaHasta = DateTime.Today
        };

        var comandas = CreateMockComandasVariosEstados();
        var comandasDto = CreateMockComandaSummaryDtos(2);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorRangoFechasAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandas);

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(
                It.Is<List<Comanda>>(list => list.All(c => c.Estado == EstadoComanda.Finalizada))))
            .Returns(comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value.Items.Count);
    }

    #endregion

    #region Tests de Enriquecimiento de Datos

    [Fact]
    public async Task Handle_ConComandasFinalizadas_DeberiaCalcularDuracionServicio()
    {
        // Arrange
        var query = ObtenerHistorialComandasQuery.Basico();
        var comandas = CreateMockComandasFinalizadas();
        var comandasDto = CreateMockComandaSummaryDtosConFechas();

        SetupRepositoryAndMapper(comandas, comandasDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify que se enriquecieron los datos (esto se hace internamente en el handler)
        foreach (var comanda in result.Value.Items.Where(c => c.Estado == "Finalizada"))
        {
            Assert.True(comanda.DuracionServicio.HasValue || !comanda.FechaFinalizacion.HasValue);
        }
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ErrorEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var query = ObtenerHistorialComandasQuery.Basico();

        _comandaRepositoryMock.Setup(x => x.ObtenerPaginadoAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error de base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno al obtener historial", result.Error);
    }

    [Fact]
    public async Task Handle_SinComandas_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = ObtenerHistorialComandasQuery.Basico();
        var comandasVacias = new List<Comanda>();
        var comandasDtoVacias = new List<ComandaSummaryDto>();

        _comandaRepositoryMock.Setup(x => x.ObtenerPaginadoAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((comandasVacias, 0));

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(comandasVacias))
            .Returns(comandasDtoVacias);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task Handle_ExcepcionEnMapeo_DeberiaRetornarError()
    {
        // Arrange
        var query = ObtenerHistorialComandasQuery.Basico();
        var comandas = CreateMockComandas(3);

        _comandaRepositoryMock.Setup(x => x.ObtenerPaginadoAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((comandas, 3));

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(It.IsAny<List<Comanda>>()))
            .Throws(new AutoMapperMappingException("Error en mapeo"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno al obtener historial", result.Error);
    }

    #endregion

    #region Helper Methods - Setup

    private void SetupRepositoryAndMapper(List<Comanda> comandas, List<ComandaSummaryDto> comandasDto)
    {
        _comandaRepositoryMock.Setup(x => x.ObtenerPaginadoAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((comandas, comandas.Count));

        _mapperMock.Setup(x => x.Map<List<ComandaSummaryDto>>(comandas))
            .Returns(comandasDto);
    }

    #endregion

    #region Helper Methods - Data Creation

    private static List<Comanda> CreateMockComandas(int count)
    {
        var comandas = new List<Comanda>();
        for (int i = 1; i <= count; i++)
        {
            // Usar instancia real en lugar de mock
            var comanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), $"Observación {i}");
            
            // Usar reflexión para establecer propiedades que necesitamos configurar
            SetPrivateProperty(comanda, "Estado", EstadoComanda.Finalizada);
            SetPrivateProperty(comanda, "FechaCreacion", DateTime.Today.AddDays(-i));
            
            // Crear el total usando reflexión o constructor
            var totalValue = 50.00m + i * 10;
            // Nota: TotalComanda podría necesitar configuración especial dependiendo de su implementación
            
            comandas.Add(comanda);
        }
        return comandas;
    }

    // Helper method para usar reflexión
    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName, 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
    }

    private static List<ComandaSummaryDto> CreateMockComandaSummaryDtos(int count)
    {
        var dtos = new List<ComandaSummaryDto>();
        for (int i = 1; i <= count; i++)
        {
            dtos.Add(new ComandaSummaryDto
            {
                Id = Guid.NewGuid(),
                Estado = "Finalizada",
                Total = 50.00m + i * 10,
                FechaCreacion = DateTime.Today.AddDays(-i),
                TotalItems = i + 2,
                NumeroComanda = $"COM-{i:000}"
            });
        }
        return dtos;
    }

    private static List<Comanda> CreateMockComandasConMesas(Guid mesaEspecifica)
    {
        var comandas = new List<Comanda>();
        
        // 2 comandas de la mesa específica
        for (int i = 1; i <= 2; i++)
        {
            var comanda = Comanda.Crear(Guid.NewGuid(), null, mesaEspecifica, "");
            SetPrivateProperty(comanda, "Estado", EstadoComanda.Finalizada);
            // El total se establecerá según la implementación de TotalComanda
            comandas.Add(comanda);
        }
        
        // 1 comanda de otra mesa
        var otraComanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), "");
        SetPrivateProperty(otraComanda, "Estado", EstadoComanda.Finalizada);
        comandas.Add(otraComanda);
        
        return comandas;
    }

    private static List<Comanda> CreateMockComandasConMontos()
    {
        var comandas = new List<Comanda>();
        var montos = new[] { 30.00m, 75.00m, 90.00m, 150.00m }; // 2 en rango 50-100
        
        for (int i = 0; i < montos.Length; i++)
        {
            var comanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), "");
            SetPrivateProperty(comanda, "Estado", EstadoComanda.Finalizada);
            // Nota: Los montos específicos se deberán configurar según la implementación de TotalComanda
            comandas.Add(comanda);
        }
        
        return comandas;
    }

    private static List<Comanda> CreateMockComandasConEstados()
    {
        var comandas = new List<Comanda>();
        var estados = new[] { EstadoComanda.Finalizada, EstadoComanda.Finalizada, EstadoComanda.Finalizada, EstadoComanda.Cancelada };
        
        for (int i = 0; i < estados.Length; i++)
        {
            var comanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), "");
            SetPrivateProperty(comanda, "Estado", estados[i]);
            comandas.Add(comanda);
        }
        
        return comandas;
    }

    private static List<Comanda> CreateMockComandasConObservaciones()
    {
        var comandas = new List<Comanda>();
        var observaciones = new[] { "Pedido especial para cliente VIP", "Comanda normal", "Otra observación" };
        
        for (int i = 0; i < observaciones.Length; i++)
        {
            var comanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), observaciones[i]);
            SetPrivateProperty(comanda, "Estado", EstadoComanda.Finalizada);
            comandas.Add(comanda);
        }
        
        return comandas;
    }

    private static List<Comanda> CreateMockComandasConCanceladas()
    {
        var comandas = new List<Comanda>();
        var estados = new[] { EstadoComanda.Finalizada, EstadoComanda.Finalizada, EstadoComanda.Cancelada };
        
        for (int i = 0; i < estados.Length; i++)
        {
            var comanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), "");
            SetPrivateProperty(comanda, "Estado", estados[i]);
            comandas.Add(comanda);
        }
        
        return comandas;
    }

    private static List<Comanda> CreateMockComandasVariosEstados()
    {
        var comandas = new List<Comanda>();
        var estados = new[] { EstadoComanda.Finalizada, EstadoComanda.Finalizada, EstadoComanda.EnProceso, EstadoComanda.Lista };
        
        for (int i = 0; i < estados.Length; i++)
        {
            var comanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), "");
            SetPrivateProperty(comanda, "Estado", estados[i]);
            comandas.Add(comanda);
        }
        
        return comandas;
    }

    private static List<Comanda> CreateMockComandasFinalizadas()
    {
        var comandas = new List<Comanda>();
        
        for (int i = 1; i <= 3; i++)
        {
            var comanda = Comanda.Crear(Guid.NewGuid(), null, Guid.NewGuid(), "");
            SetPrivateProperty(comanda, "Estado", EstadoComanda.Finalizada);
            SetPrivateProperty(comanda, "FechaCreacion", DateTime.Today.AddHours(-2 - i));
            comandas.Add(comanda);
        }
        
        return comandas;
    }

    private static List<ComandaSummaryDto> CreateMockComandaSummaryDtosConFechas()
    {
        var dtos = new List<ComandaSummaryDto>();
        
        for (int i = 1; i <= 3; i++)
        {
            dtos.Add(new ComandaSummaryDto
            {
                Id = Guid.NewGuid(),
                Estado = "Finalizada",
                Total = 55.00m + i * 5,
                FechaCreacion = DateTime.Today.AddHours(-2 - i),
                FechaFinalizacion = DateTime.Today.AddHours(-1 - i),
                TotalItems = i + 1,
                NumeroComanda = $"COM-{i:000}"
            });
        }
        
        return dtos;
    }

    #endregion
} 