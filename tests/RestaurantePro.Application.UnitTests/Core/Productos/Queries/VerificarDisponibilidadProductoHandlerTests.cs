namespace RestaurantePro.Application.UnitTests.Core.Productos.Queries;

/// <summary>
/// Tests unitarios para VerificarDisponibilidadProductoHandler
/// Valida la lógica completa de verificación de disponibilidad con análisis de inventario
/// </summary>
public class VerificarDisponibilidadProductoHandlerTests
{
    private readonly Mock<IProductoRepository> _productoRepositoryMock;
    private readonly Mock<IInventarioIngredientesRepository> _inventarioRepositoryMock;
    private readonly Mock<IProductoService> _productoServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<VerificarDisponibilidadProductoHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly VerificarDisponibilidadProductoHandler _handler;

    public VerificarDisponibilidadProductoHandlerTests()
    {
        _productoRepositoryMock = new Mock<IProductoRepository>();
        _inventarioRepositoryMock = new Mock<IInventarioIngredientesRepository>();
        _productoServiceMock = new Mock<IProductoService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<VerificarDisponibilidadProductoHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new VerificarDisponibilidadProductoHandler(
            _productoRepositoryMock.Object,
            _inventarioRepositoryMock.Object,
            _productoServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object);
    }

    #region Tests de Factory Methods del Query

    [Fact]
    public void VerificacionSimple_ConParametrosValidos_DeberiaCrearQueryCorrectamente()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidad = 5;

        // Act
        var query = VerificarDisponibilidadProductoQuery.VerificacionSimple(productoId, cantidad);

        // Assert
        Assert.Equal(productoId, query.ProductoId);
        Assert.Equal(cantidad, query.CantidadSolicitada);
        Assert.Equal("Simple", query.TipoVerificacion);
        Assert.False(query.IncluirAnalisisIngredientes);
        Assert.False(query.IncluirRecomendacionesAlternativas);
        Assert.False(query.VerificarTodasLasVariantes);
    }

    [Fact]
    public void VerificacionCompleta_ConParametrosValidos_DeberiaIncluirTodosLosAnalisis()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidad = 10;
        var fechaDeseada = DateTime.UtcNow.AddHours(2);

        // Act
        var query = VerificarDisponibilidadProductoQuery.VerificacionCompleta(productoId, cantidad, fechaDeseada);

        // Assert
        Assert.Equal("Completa", query.TipoVerificacion);
        Assert.True(query.IncluirAnalisisIngredientes);
        Assert.True(query.IncluirRecomendacionesAlternativas);
        Assert.True(query.VerificarTodasLasVariantes);
        Assert.True(query.CalcularTiempoPreparacion);
        Assert.Equal(fechaDeseada, query.FechaHoraDeseada);
    }

    [Fact]
    public void VerificacionParaComanda_ConComandaId_DeberiaAsociarComanda()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var cantidad = 3;

        // Act
        var query = VerificarDisponibilidadProductoQuery.VerificacionParaComanda(productoId, comandaId, cantidad);

        // Assert
        Assert.Equal("ParaComanda", query.TipoVerificacion);
        Assert.Equal(comandaId, query.ComandaId);
        Assert.True(query.PriorizarVelocidadPreparacion);
        Assert.True(query.IncluirAnalisisIngredientes);
        Assert.Equal(3, query.PrioridadVerificacion);
    }

    [Fact]
    public void VerificacionMasiva_ConListaProductos_DeberiaConfigurarCorrectamente()
    {
        // Arrange
        var productos = new List<(Guid ProductoId, int Cantidad)>
        {
            (Guid.NewGuid(), 2),
            (Guid.NewGuid(), 5),
            (Guid.NewGuid(), 1)
        };

        // Act
        var query = VerificarDisponibilidadProductoQuery.VerificacionMasiva(productos);

        // Assert
        Assert.Equal("Masiva", query.TipoVerificacion);
        Assert.Equal(3, query.ProductosAVerificar.Count);
        Assert.True(query.OptimizarConsultasBaseDatos);
        Assert.True(query.AgruparPorCategoria);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_ProductoDisponibleSimple_DeberiaRetornarDisponible()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 3,
            TipoVerificacion = "Simple",
            IncluirAnalisisIngredientes = false
        };

        var producto = CreateMockProductoActivo(productoId, "Hamburguesa Clásica", true);
        var resultadoDto = CreateMockDisponibilidadDto(productoId, true, 3);

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _productoServiceMock.Setup(x => x.VerificarDisponibilidadAsync(producto, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));
        _mapperMock.Setup(x => x.Map<DisponibilidadProductoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.EstaDisponible);
        Assert.Equal(3, result.Value.CantidadVerificada);
        Assert.Equal(productoId, result.Value.ProductoId);
    }

    [Fact]
    public async Task Handle_ProductoConAnalisisIngredientes_DeberiaIncluirDetallesIngredientes()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 2,
            TipoVerificacion = "Completa",
            IncluirAnalisisIngredientes = true,
            CalcularTiempoPreparacion = true
        };

        var producto = CreateMockProductoConIngredientes(productoId, "Pizza Margherita");
        var resultadoDto = CreateMockDisponibilidadDtoCompleto(productoId, true, 2);

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _productoServiceMock.Setup(x => x.VerificarDisponibilidadConIngredientesAsync(
            producto, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new { Disponible = true, IngredientesVerificados = 4 }));
        _mapperMock.Setup(x => x.Map<DisponibilidadProductoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.EstaDisponible);
        Assert.NotEmpty(result.Value.AnalisisIngredientes);
        Assert.True(result.Value.TiempoPreparacionMinutos > 0);
        Assert.Contains("Mozzarella", result.Value.AnalisisIngredientes.Select(a => a.NombreIngrediente));
    }

    [Fact]
    public async Task Handle_ProductoConRecomendacionesAlternativas_DeberiaIncluirAlternativas()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 5,
            TipoVerificacion = "Completa",
            IncluirRecomendacionesAlternativas = true
        };

        var producto = CreateMockProductoActivo(productoId, "Pasta Carbonara", false); // No disponible
        var resultadoDto = CreateMockDisponibilidadDtoConAlternativas(productoId, false, 5);

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _productoServiceMock.Setup(x => x.VerificarDisponibilidadAsync(producto, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(false));
        _productoServiceMock.Setup(x => x.ObtenerAlternativasDisponiblesAsync(producto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<Producto> { CreateMockProductoActivo(Guid.NewGuid(), "Pasta Bolognesa", true) }));
        _mapperMock.Setup(x => x.Map<DisponibilidadProductoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Value.EstaDisponible);
        Assert.NotEmpty(result.Value.AlternativasDisponibles);
        Assert.Contains("Pasta Bolognesa", result.Value.AlternativasDisponibles.Select(a => a.Nombre));
    }

    [Fact]
    public async Task Handle_ProductoParaComandaUrgente_DeberiaPriorizarVelocidad()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var comandaId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            ComandaId = comandaId,
            CantidadSolicitada = 1,
            TipoVerificacion = "ParaComanda",
            PriorizarVelocidadPreparacion = true,
            PrioridadVerificacion = 5
        };

        var producto = CreateMockProductoRapido(productoId, "Ensalada César");
        var resultadoDto = CreateMockDisponibilidadDtoRapido(productoId, true, 1);

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _productoServiceMock.Setup(x => x.VerificarDisponibilidadRapidaAsync(producto, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));
        _mapperMock.Setup(x => x.Map<DisponibilidadProductoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.EstaDisponible);
        Assert.True(result.Value.TiempoPreparacionMinutos <= 10); // Productos rápidos
        Assert.Equal(comandaId, result.Value.ComandaAsociadaId);
        Assert.Equal(5, result.Value.PrioridadPreparacion);
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_ProductoNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 1,
            TipoVerificacion = "Simple"
        };

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Producto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("no encontrado", result.Error);
    }

    [Fact]
    public async Task Handle_ProductoInactivo_DeberiaRetornarNoDisponible()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 2,
            TipoVerificacion = "Simple"
        };

        var producto = CreateMockProductoInactivo(productoId, "Producto Descontinuado");
        var resultadoDto = CreateMockDisponibilidadDto(productoId, false, 0);

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _mapperMock.Setup(x => x.Map<DisponibilidadProductoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Value.EstaDisponible);
        Assert.Contains("inactivo", result.Value.MotivoNoDisponibilidad);
    }

    [Fact]
    public async Task Handle_CantidadInvalida_DeberiaRetornarError()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 0, // Cantidad inválida
            TipoVerificacion = "Simple"
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("cantidad debe ser mayor a 0", result.Error);
    }

    [Fact]
    public async Task Handle_ProductoSinStock_DeberiaRetornarNoDisponible()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 10,
            TipoVerificacion = "Simple"
        };

        var producto = CreateMockProductoActivo(productoId, "Producto Agotado", true);
        var resultadoDto = CreateMockDisponibilidadDto(productoId, false, 0);

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _productoServiceMock.Setup(x => x.VerificarDisponibilidadAsync(producto, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(false));
        _mapperMock.Setup(x => x.Map<DisponibilidadProductoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Value.EstaDisponible);
        Assert.Contains("stock insuficiente", result.Value.MotivoNoDisponibilidad);
    }

    [Fact]
    public async Task Handle_IngredientesFaltantes_DeberiaRetornarDetallesIngredientes()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 3,
            TipoVerificacion = "Completa",
            IncluirAnalisisIngredientes = true
        };

        var producto = CreateMockProductoConIngredientes(productoId, "Pizza Especial");
        var resultadoDto = CreateMockDisponibilidadDtoConIngredientesFaltantes(productoId, false, 3);

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _productoServiceMock.Setup(x => x.VerificarDisponibilidadConIngredientesAsync(
            producto, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new { Disponible = false, IngredientesFaltantes = new[] { "Salami", "Champiñones" } }));
        _mapperMock.Setup(x => x.Map<DisponibilidadProductoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Value.EstaDisponible);
        Assert.Contains("Salami", result.Value.AnalisisIngredientes.Where(a => !a.EstaDisponible).Select(a => a.NombreIngrediente));
        Assert.Contains("Champiñones", result.Value.AnalisisIngredientes.Where(a => !a.EstaDisponible).Select(a => a.NombreIngrediente));
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ExcepcionRepositorio_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 1,
            TipoVerificacion = "Simple"
        };

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorServicioProducto_DeberiaRetornarErrorServicio()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 2,
            TipoVerificacion = "Simple"
        };

        var producto = CreateMockProductoActivo(productoId, "Producto Test", true);

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _productoServiceMock.Setup(x => x.VerificarDisponibilidadAsync(producto, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<bool>("Error en verificación de inventario"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error en verificación de inventario", result.Error);
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_VerificacionExitosa_DeberiaLoggearInformacion()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 4,
            TipoVerificacion = "Completa"
        };

        var producto = CreateMockProductoActivo(productoId, "Producto Test", true);
        var resultadoDto = CreateMockDisponibilidadDto(productoId, true, 4);

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _productoServiceMock.Setup(x => x.VerificarDisponibilidadAsync(producto, 4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));
        _mapperMock.Setup(x => x.Map<DisponibilidadProductoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        // Verificar logging de inicio
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verificando disponibilidad")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de resultado
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verificación completada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ProductoNoDisponible_DeberiaLoggearWarning()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var query = new VerificarDisponibilidadProductoQuery
        {
            ProductoId = productoId,
            CantidadSolicitada = 5,
            TipoVerificacion = "Simple"
        };

        var producto = CreateMockProductoActivo(productoId, "Producto Agotado", true);
        var resultadoDto = CreateMockDisponibilidadDto(productoId, false, 0);

        _productoRepositoryMock.Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _productoServiceMock.Setup(x => x.VerificarDisponibilidadAsync(producto, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(false));
        _mapperMock.Setup(x => x.Map<DisponibilidadProductoDto>(It.IsAny<object>()))
            .Returns(resultadoDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        // Verificar que se loggeó el warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no está disponible")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Métodos Helper

    private static Producto CreateMockProductoActivo(Guid id, string nombre, bool activo)
    {
        var producto = new Producto();
        typeof(Producto).GetProperty("Id")?.SetValue(producto, id);
        typeof(Producto).GetProperty("Nombre")?.SetValue(producto, nombre);
        typeof(Producto).GetProperty("Activo")?.SetValue(producto, activo);
        typeof(Producto).GetProperty("Disponible")?.SetValue(producto, activo);
        return producto;
    }

    private static Producto CreateMockProductoInactivo(Guid id, string nombre)
    {
        var producto = new Producto();
        typeof(Producto).GetProperty("Id")?.SetValue(producto, id);
        typeof(Producto).GetProperty("Nombre")?.SetValue(producto, nombre);
        typeof(Producto).GetProperty("Activo")?.SetValue(producto, false);
        typeof(Producto).GetProperty("Disponible")?.SetValue(producto, false);
        return producto;
    }

    private static Producto CreateMockProductoConIngredientes(Guid id, string nombre)
    {
        var producto = CreateMockProductoActivo(id, nombre, true);
        // TODO: Agregar ingredientes mock cuando la entidad lo soporte
        return producto;
    }

    private static Producto CreateMockProductoRapido(Guid id, string nombre)
    {
        var producto = CreateMockProductoActivo(id, nombre, true);
        typeof(Producto).GetProperty("TiempoPreparacionMinutos")?.SetValue(producto, 5);
        return producto;
    }

    private static DisponibilidadProductoDto CreateMockDisponibilidadDto(Guid productoId, bool disponible, int cantidad)
    {
        return new DisponibilidadProductoDto
        {
            ProductoId = productoId,
            EstaDisponible = disponible,
            CantidadVerificada = cantidad,
            CantidadDisponible = disponible ? cantidad : 0,
            MotivoNoDisponibilidad = disponible ? null : "Stock insuficiente",
            FechaVerificacion = DateTime.UtcNow,
            AnalisisIngredientes = new List<AnalisisIngredienteDto>(),
            AlternativasDisponibles = new List<ProductoSummaryDto>()
        };
    }

    private static DisponibilidadProductoDto CreateMockDisponibilidadDtoCompleto(Guid productoId, bool disponible, int cantidad)
    {
        return new DisponibilidadProductoDto
        {
            ProductoId = productoId,
            EstaDisponible = disponible,
            CantidadVerificada = cantidad,
            TiempoPreparacionMinutos = 15,
            AnalisisIngredientes = new List<AnalisisIngredienteDto>
            {
                new() { NombreIngrediente = "Mozzarella", EstaDisponible = true, CantidadNecesaria = 200, UnidadMedida = "gr" },
                new() { NombreIngrediente = "Salsa Tomate", EstaDisponible = true, CantidadNecesaria = 100, UnidadMedida = "ml" }
            },
            AlternativasDisponibles = new List<ProductoSummaryDto>()
        };
    }

    private static DisponibilidadProductoDto CreateMockDisponibilidadDtoConAlternativas(Guid productoId, bool disponible, int cantidad)
    {
        return new DisponibilidadProductoDto
        {
            ProductoId = productoId,
            EstaDisponible = disponible,
            CantidadVerificada = cantidad,
            AlternativasDisponibles = new List<ProductoSummaryDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Pasta Bolognesa", Precio = 18.50m },
                new() { Id = Guid.NewGuid(), Nombre = "Pasta Alfredo", Precio = 20.00m }
            },
            AnalisisIngredientes = new List<AnalisisIngredienteDto>()
        };
    }

    private static DisponibilidadProductoDto CreateMockDisponibilidadDtoRapido(Guid productoId, bool disponible, int cantidad)
    {
        return new DisponibilidadProductoDto
        {
            ProductoId = productoId,
            ComandaAsociadaId = Guid.NewGuid(),
            EstaDisponible = disponible,
            CantidadVerificada = cantidad,
            TiempoPreparacionMinutos = 8,
            PrioridadPreparacion = 5,
            AnalisisIngredientes = new List<AnalisisIngredienteDto>(),
            AlternativasDisponibles = new List<ProductoSummaryDto>()
        };
    }

    private static DisponibilidadProductoDto CreateMockDisponibilidadDtoConIngredientesFaltantes(Guid productoId, bool disponible, int cantidad)
    {
        return new DisponibilidadProductoDto
        {
            ProductoId = productoId,
            EstaDisponible = disponible,
            CantidadVerificada = cantidad,
            MotivoNoDisponibilidad = "Ingredientes insuficientes",
            AnalisisIngredientes = new List<AnalisisIngredienteDto>
            {
                new() { NombreIngrediente = "Salami", EstaDisponible = false, CantidadNecesaria = 150, UnidadMedida = "gr" },
                new() { NombreIngrediente = "Champiñones", EstaDisponible = false, CantidadNecesaria = 100, UnidadMedida = "gr" },
                new() { NombreIngrediente = "Mozzarella", EstaDisponible = true, CantidadNecesaria = 200, UnidadMedida = "gr" }
            },
            AlternativasDisponibles = new List<ProductoSummaryDto>()
        };
    }

    #endregion
} 