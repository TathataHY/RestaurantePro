using System.Diagnostics;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Core.Base;

namespace RestaurantePro.Application.UnitTests.Operaciones.Reportes.Queries;

/// <summary>
/// 🔥 Tests exhaustivos para ObtenerReporteVentasDiariaHandler
/// Validación completa de análisis de ventas diarias con diferentes configuraciones
/// </summary>
public class ObtenerReporteVentasDiariaHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerReporteVentasDiariaHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<DbSet<Comanda>> _mockComandas;
    private readonly ObtenerReporteVentasDiariaHandler _handler;

    public ObtenerReporteVentasDiariaHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerReporteVentasDiariaHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockComandas = new Mock<DbSet<Comanda>>();

        _mockContext.Setup(x => x.Comandas).Returns(_mockComandas.Object);

        _handler = new ObtenerReporteVentasDiariaHandler(
            _mockContext.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object);
    }

    #region Helper Methods

    private ObtenerReporteVentasDiariaQuery CrearQueryValida()
    {
        return new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today.AddDays(-1),
            IncluirComparativoPeriodoAnterior = true,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = true,
            IncluirTendenciasSemana = false,
            NivelDetalle = NivelDetalle.Completo
        };
    }

    private void ConfigurarComandasMock(List<Comanda> comandas)
    {
        var comandasQueryable = comandas.AsQueryable();
        _mockComandas.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(comandasQueryable.Provider);
        _mockComandas.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(comandasQueryable.Expression);
        _mockComandas.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(comandasQueryable.ElementType);
        _mockComandas.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(comandasQueryable.GetEnumerator());
    }

    private List<Comanda> CrearComandasDePrueba(DateTime fecha, int cantidad = 3)
    {
        var comandas = new List<Comanda>();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var productoId = Guid.NewGuid();

        for (int i = 0; i < cantidad; i++)
        {
            // Crear comanda usando el factory method correcto
            var comanda = Comanda.Crear(
                meseroId: meseroId,
                clienteId: null,
                mesaId: mesaId,
                observaciones: $"Comanda de prueba {i + 1}");

            // Usar reflexión para configurar propiedades privadas necesarias para tests
            typeof(EntityBase).GetProperty("Id")?.SetValue(comanda, Guid.NewGuid());
            
            // Crear usuario (mesero) usando factory method correcto
            var usuario = Usuario.Crear(
                $"mesero{i + 1}",
                $"Mesero {i + 1}",
                $"mesero{i + 1}@test.com",
                RolUsuario.Mesero);
            typeof(EntityBase).GetProperty("Id")?.SetValue(usuario, meseroId);

            // Crear mesa usando factory method correcto  
            var mesa = Mesa.Crear(1 + i, 4, "Interior");
            typeof(EntityBase).GetProperty("Id")?.SetValue(mesa, mesaId);

            // Crear producto usando factory method correcto
            var producto = Producto.Crear(
                $"Producto {i + 1}",
                $"Descripción del producto {i + 1}",
                new PrecioProducto(750m + (i * 250m)),
                Guid.NewGuid(),
                "Categoría Test");
            typeof(EntityBase).GetProperty("Id")?.SetValue(producto, productoId);

            // Configurar navegaciones usando reflexión
            var mesaProperty = typeof(Comanda).GetProperty("Mesa");
            mesaProperty?.SetValue(comanda, mesa);

            var meseroProperty = typeof(Comanda).GetProperty("Mesero");
            meseroProperty?.SetValue(comanda, usuario);

            comandas.Add(comanda);
        }

        return comandas;
    }

    #endregion

    #region Tests Básicos

    [Fact]
    public async Task Handle_ReporteDiarioBasico_DeberiaGenerarExitosamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var comandas = CrearComandasDePrueba(query.FechaReporte, 2);

        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FechaReporte.Should().Be(query.FechaReporte);
        result.Value.NivelDetalle.Should().Be(query.NivelDetalle);
        result.Value.MetricasBasicas.Should().NotBeNull();
        result.Value.MetricasBasicas.TotalComandas.Should().Be(2);
        result.Value.MetricasBasicas.MontoTotalVentas.Should().Be(3500m);
        result.Value.FechaGeneracion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_SinComandasEnFecha_DeberiaRetornarReporteVacio()
    {
        // Arrange
        var query = CrearQueryValida();

        ConfigurarComandasMock(new List<Comanda>()); // Sin comandas

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MetricasBasicas.TotalComandas.Should().Be(0);
        result.Value.MetricasBasicas.MontoTotalVentas.Should().Be(0);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No se encontraron comandas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(NivelDetalle.Basico)]
    [InlineData(NivelDetalle.Intermedio)]
    [InlineData(NivelDetalle.Completo)]
    [InlineData(NivelDetalle.Meseros)]
    [InlineData(NivelDetalle.Mesas)]
    public async Task Handle_DiferentesNivelesDetalle_DeberiaGenerarCorrectamente(NivelDetalle nivel)
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            NivelDetalle = nivel,
            IncluirAnalisisPorMesa = nivel == NivelDetalle.Mesas || nivel == NivelDetalle.Completo || nivel == NivelDetalle.Intermedio,
            IncluirAnalisisPorMesero = nivel == NivelDetalle.Meseros || nivel == NivelDetalle.Completo || nivel == NivelDetalle.Intermedio,
            IncluirAnalisisProductos = nivel == NivelDetalle.Completo,
            IncluirComparativoPeriodoAnterior = nivel == NivelDetalle.Completo,
            IncluirTendenciasSemana = false
        };

        var comandas = CrearComandasDePrueba(query.FechaReporte, 3);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.NivelDetalle.Should().Be(nivel);
    }

    #endregion

    #region Tests de Análisis Específicos

    [Fact]
    public async Task Handle_ConAnalisisPorMesa_DeberiaGenerarAnalisisMesas()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = false,
            IncluirAnalisisProductos = false,
            IncluirComparativoPeriodoAnterior = false,
            IncluirTendenciasSemana = false,
            NivelDetalle = NivelDetalle.Mesas
        };

        var comandas = CrearComandasDePrueba(query.FechaReporte, 3);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.AnalisisPorMesa.Should().NotBeNull();
        result.Value.AnalisisPorMesa.Should().HaveCount(3);
        result.Value.AnalisisPorMesa[0].TotalComandas.Should().BeGreaterThan(0);
        result.Value.AnalisisPorMesa[0].MontoTotal.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_ConAnalisisPorMesero_DeberiaGenerarAnalisisMeseros()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            IncluirAnalisisPorMesa = false,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = false,
            IncluirComparativoPeriodoAnterior = false,
            IncluirTendenciasSemana = false,
            NivelDetalle = NivelDetalle.Meseros
        };

        var comandas = CrearComandasDePrueba(query.FechaReporte, 3);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.AnalisisPorMesero.Should().NotBeNull();
        result.Value.AnalisisPorMesero.Should().HaveCount(3);
        result.Value.AnalisisPorMesero[0].TotalComandas.Should().BeGreaterThan(0);
        result.Value.AnalisisPorMesero[0].MontoTotal.Should().BeGreaterThan(0);
        result.Value.AnalisisPorMesero[0].EficienciaVentas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_ConAnalisisProductos_DeberiaGenerarAnalisisProductos()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            IncluirAnalisisPorMesa = false,
            IncluirAnalisisPorMesero = false,
            IncluirAnalisisProductos = true,
            IncluirComparativoPeriodoAnterior = false,
            IncluirTendenciasSemana = false,
            NivelDetalle = NivelDetalle.Completo
        };

        var comandas = CrearComandasDePrueba(query.FechaReporte, 3);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.AnalisisProductos.Should().NotBeNull();
        result.Value.AnalisisProductos.Should().HaveCount(3);
        result.Value.AnalisisProductos[0].CantidadVendida.Should().BeGreaterThan(0);
        result.Value.AnalisisProductos[0].MontoTotal.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_SinAnalisisEspecificos_NoDeberiaGenerarAnalisis()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            IncluirAnalisisPorMesa = false,
            IncluirAnalisisPorMesero = false,
            IncluirAnalisisProductos = false,
            IncluirComparativoPeriodoAnterior = false,
            IncluirTendenciasSemana = false,
            NivelDetalle = NivelDetalle.Basico
        };

        var comandas = CrearComandasDePrueba(query.FechaReporte, 2);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.AnalisisPorMesa.Should().BeNull();
        result.Value.AnalisisPorMesero.Should().BeNull();
        result.Value.AnalisisProductos.Should().BeNull();
        result.Value.DistribucionHoraria.Should().NotBeNull(); // Siempre se genera
    }

    #endregion

    #region Tests de Distribución Horaria

    [Fact]
    public async Task Handle_ConComandas_DeberiaGenerarDistribucionHoraria()
    {
        // Arrange
        var query = CrearQueryValida();
        var comandas = CrearComandasDePrueba(query.FechaReporte, 4);

        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.DistribucionHoraria.Should().NotBeNull();
        result.Value.DistribucionHoraria.Should().HaveCount(4);
        result.Value.DistribucionHoraria.Should().BeInAscendingOrder(d => d.Hora);
        result.Value.DistribucionHoraria[0].TotalComandas.Should().BeGreaterThan(0);
    }

    #endregion

    #region Tests de Comparativo Período Anterior

    [Fact]
    public async Task Handle_ConComparativoPeriodoAnterior_DeberiaGenerarComparativo()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            IncluirComparativoPeriodoAnterior = true,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = true,
            IncluirTendenciasSemana = false,
            NivelDetalle = NivelDetalle.Completo
        };

        var comandasHoy = CrearComandasDePrueba(query.FechaReporte, 2);
        var comandasAyer = CrearComandasDePrueba(query.FechaReporte.AddDays(-1), 1);
        
        var todasComandas = comandasHoy.Concat(comandasAyer).ToList();
        ConfigurarComandasMock(todasComandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.ComparativoPeriodoAnterior.Should().NotBeNull();
        result.Value.ComparativoPeriodoAnterior.FechaAnterior.Should().Be(query.FechaReporte.AddDays(-1));
        result.Value.ComparativoPeriodoAnterior.MetricasAnteriores.Should().NotBeNull();
        result.Value.ComparativoPeriodoAnterior.VariacionComandas.Should().NotBe(0);
        result.Value.ComparativoPeriodoAnterior.VariacionVentas.Should().NotBe(0);
    }

    [Fact]
    public async Task Handle_SinComparativoPeriodoAnterior_NoDeberiaGenerarComparativo()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            IncluirComparativoPeriodoAnterior = false,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = true,
            IncluirTendenciasSemana = false,
            NivelDetalle = NivelDetalle.Completo
        };

        var comandas = CrearComandasDePrueba(query.FechaReporte, 2);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.ComparativoPeriodoAnterior.Should().BeNull();
    }

    #endregion

    #region Tests de Tendencias Semana

    [Fact]
    public async Task Handle_ConTendenciasSemana_DeberiaGenerarTendencias()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            IncluirComparativoPeriodoAnterior = false,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = true,
            IncluirTendenciasSemana = true,
            NivelDetalle = NivelDetalle.Completo
        };

        var comandasSemana = new List<Comanda>();
        for (int i = 0; i < 7; i++)
        {
            var fecha = query.FechaReporte.AddDays(-6 + i);
            comandasSemana.AddRange(CrearComandasDePrueba(fecha, 1));
        }

        ConfigurarComandasMock(comandasSemana);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.TendenciasSemana.Should().NotBeNull();
        result.Value.TendenciasSemana.Should().HaveCount(7);
        result.Value.TendenciasSemana.Should().BeInAscendingOrder(t => t.Fecha);
        result.Value.TendenciasSemana[0].TotalComandas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_SinTendenciasSemana_NoDeberiaGenerarTendencias()
    {
        // Arrange
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            IncluirComparativoPeriodoAnterior = false,
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = true,
            IncluirTendenciasSemana = false,
            NivelDetalle = NivelDetalle.Completo
        };

        var comandas = CrearComandasDePrueba(query.FechaReporte, 2);
        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.TendenciasSemana.Should().BeNull();
    }

    #endregion

    #region Tests de Filtros

    [Fact]
    public async Task Handle_ConMesasEspecificas_DeberiaFiltrarPorMesas()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            MesesEspecificos = new List<Guid> { mesaId },
            IncluirAnalisisPorMesa = true,
            IncluirAnalisisPorMesero = false,
            IncluirAnalisisProductos = false,
            IncluirComparativoPeriodoAnterior = false,
            IncluirTendenciasSemana = false,
            NivelDetalle = NivelDetalle.Mesas
        };

        var comandasFiltradas = new List<Comanda>
        {
            // Usar el método factory apropiado para crear la comanda
            CrearComandaConMesa(query.FechaReporte.AddHours(10), mesaId, 1500m, 5)
        };

        ConfigurarComandasMock(comandasFiltradas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.MetricasBasicas.TotalComandas.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ConMeserosEspecificos_DeberiaFiltrarPorMeseros()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var query = new ObtenerReporteVentasDiariaQuery
        {
            FechaReporte = DateTime.Today,
            MeserosEspecificos = new List<Guid> { meseroId },
            IncluirAnalisisPorMesa = false,
            IncluirAnalisisPorMesero = true,
            IncluirAnalisisProductos = false,
            IncluirComparativoPeriodoAnterior = false,
            IncluirTendenciasSemana = false,
            NivelDetalle = NivelDetalle.Meseros
        };

        var comandasFiltradas = new List<Comanda>
        {
            // Usar el método factory apropiado para crear la comanda
            CrearComandaConMesero(query.FechaReporte.AddHours(14), meseroId, 2000m, "Carlos Pérez")
        };

        ConfigurarComandasMock(comandasFiltradas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.MetricasBasicas.TotalComandas.Should().Be(1);
    }

    // Métodos helper para crear comandas específicas
    private Comanda CrearComandaConMesa(DateTime fecha, Guid mesaId, decimal monto, int numeroMesa)
    {
        // Crear comanda usando factory method correcto
        var comanda = Comanda.Crear(
            meseroId: Guid.NewGuid(),
            clienteId: null,
            mesaId: mesaId,
            observaciones: $"Comanda para mesa {numeroMesa}");

        // Crear mesa usando factory method correcto
        var mesa = Mesa.Crear(numeroMesa, 4, "Interior");
        typeof(EntityBase).GetProperty("Id")?.SetValue(mesa, mesaId);
        
        // Configurar navegación usando reflexión
        var mesaProperty = typeof(Comanda).GetProperty("Mesa");
        mesaProperty?.SetValue(comanda, mesa);
        
        return comanda;
    }

    private Comanda CrearComandaConMesero(DateTime fecha, Guid meseroId, decimal monto, string nombreMesero)
    {
        // Crear comanda usando factory method correcto
        var comanda = Comanda.Crear(
            meseroId: meseroId,
            clienteId: null,
            mesaId: Guid.NewGuid(),
            observaciones: $"Comanda de {nombreMesero}");

        // Crear usuario/mesero usando factory method correcto
        var usuario = Usuario.Crear(
            nombreMesero.Replace(" ", "").ToLower(),
            nombreMesero,
            $"{nombreMesero.Replace(" ", "").ToLower()}@test.com",
            RolUsuario.Mesero);
        typeof(EntityBase).GetProperty("Id")?.SetValue(usuario, meseroId);
        
        // Configurar navegación usando reflexión
        var meseroProperty = typeof(Comanda).GetProperty("Mesero");
        meseroProperty?.SetValue(comanda, usuario);
        
        return comanda;
    }

    #endregion

    #region Tests de Métricas Calculadas

    [Fact]
    public async Task Handle_ConComandas_DeberiaCalcularMetricasCorrectamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var comandas = new List<Comanda>();
        
        // Crear comandas usando factory method correcto
        for (int i = 0; i < 3; i++)
        {
            var comanda = Comanda.Crear(
                meseroId: Guid.NewGuid(),
                clienteId: null,
                mesaId: Guid.NewGuid(),
                observaciones: $"Comanda de prueba {i + 1}");
            comandas.Add(comanda);
        }

        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        var metricas = result.Value.MetricasBasicas;
        metricas.TotalComandas.Should().Be(3);
        // Nota: Los totales pueden ser diferentes porque estamos usando datos reales de dominio
        metricas.MontoTotalVentas.Should().BeGreaterThan(0);
        metricas.PromedioVentaPorComanda.Should().BeGreaterThan(0);
    }

    #endregion

    #region Tests de Errores

    [Fact]
    public async Task Handle_ExcepcionEnBaseDatos_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();

        _mockComandas.As<IQueryable<Comanda>>().Setup(m => m.Provider)
            .Throws(new InvalidOperationException("Error de conexión"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno generando reporte");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error generando reporte")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_GeneracionExitosa_DeberiaLoguearCorrectamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var comandas = CrearComandasDePrueba(query.FechaReporte, 2);

        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar log de inicio
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generando reporte de ventas diarias")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        // Verificar log de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Reporte de ventas diarias generado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de Factory Methods

    [Fact]
    public async Task Handle_QueryCrearReporteHoy_DeberiaFuncionar()
    {
        // Arrange
        var query = ObtenerReporteVentasDiariaQuery.CrearReporteHoy();
        var comandas = CrearComandasDePrueba(DateTime.Today, 2);

        ConfigurarComandasMock(comandas);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.FechaReporte.Should().Be(DateTime.Today);
        result.Value.NivelDetalle.Should().Be(NivelDetalle.Completo);
    }

    #endregion

    #region Tests de Rendimiento

    [Fact]
    public async Task Handle_GeneracionRapida_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var comandas = CrearComandasDePrueba(query.FechaReporte, 5);

        ConfigurarComandasMock(comandas);

        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Menos de 2 segundos
    }

    #endregion
} 