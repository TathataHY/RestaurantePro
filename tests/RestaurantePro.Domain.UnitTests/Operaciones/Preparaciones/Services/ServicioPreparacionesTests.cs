using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using Xunit;

namespace RestaurantePro.Domain.UnitTests.Operaciones.Preparaciones.Services;

/// <summary>
/// Pruebas unitarias para ServicioPreparaciones
/// </summary>
public class ServicioPreparacionesTests
{
    private readonly Mock<ILogger<ServicioPreparaciones>> _loggerMock;
    private readonly INotificationManager _notificationManager;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<IPreparacionRepository> _preparacionRepositoryMock;
    private readonly ServicioPreparaciones _servicio;
    private readonly DateTime _fechaActual = new DateTime(2025, 1, 15, 10, 30, 0);

    public ServicioPreparacionesTests()
    {
        _loggerMock = new Mock<ILogger<ServicioPreparaciones>>();
        _notificationManager = new NotificationManager();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _preparacionRepositoryMock = new Mock<IPreparacionRepository>();
        
        _dateTimeServiceMock.Setup(x => x.Now).Returns(_fechaActual);

        _servicio = new ServicioPreparaciones(
            _loggerMock.Object,
            _notificationManager,
            _dateTimeServiceMock.Object,
            _preparacionRepositoryMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConParametrosValidos_DebeCrearInstancia()
    {
        // Arrange
        var logger = new Mock<ILogger<ServicioPreparaciones>>().Object;
        var notificationManager = new NotificationManager();
        var dateTimeService = new Mock<IDateTimeService>().Object;
        var repository = new Mock<IPreparacionRepository>().Object;

        // Act
        var servicio = new ServicioPreparaciones(logger, notificationManager, dateTimeService, repository);

        // Assert
        servicio.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new ServicioPreparaciones(
            logger: null!, 
            notificationManager: _notificationManager,
            dateTimeService: _dateTimeServiceMock.Object,
            preparacionRepository: _preparacionRepositoryMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_ConNotificationManagerNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new ServicioPreparaciones(
            logger: _loggerMock.Object,
            notificationManager: null!,
            dateTimeService: _dateTimeServiceMock.Object,
            preparacionRepository: _preparacionRepositoryMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("notificationManager");
    }

    [Fact]
    public void Constructor_ConDateTimeServiceNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new ServicioPreparaciones(
            logger: _loggerMock.Object,
            notificationManager: _notificationManager,
            dateTimeService: null!,
            preparacionRepository: _preparacionRepositoryMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("dateTimeService");
    }

    [Fact]
    public void Constructor_ConPreparacionRepositoryNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new ServicioPreparaciones(
            logger: _loggerMock.Object,
            notificationManager: _notificationManager,
            dateTimeService: _dateTimeServiceMock.Object,
            preparacionRepository: null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("preparacionRepository");
    }

    #endregion

    #region PrepararProductoAsync Tests

    [Fact]
    public async Task PrepararProductoAsync_ConParametrosValidos_DebeCrearPreparacion()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidad = 10;
        var chefId = Guid.NewGuid();
        var fechaVencimiento = _fechaActual.AddDays(1);
        var observaciones = "Prueba de preparación";
        
        var preparacion = new PreparacionDiaria(); // Preparación vacía para pruebas
        
        _preparacionRepositoryMock
            .Setup(x => x.AgregarAsync(It.IsAny<PreparacionDiaria>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _preparacionRepositoryMock
            .Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, cantidad, chefId, fechaVencimiento, observaciones);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        
        _preparacionRepositoryMock.Verify(
            x => x.AgregarAsync(It.IsAny<PreparacionDiaria>(), It.IsAny<CancellationToken>()), 
            Times.Once);
        
        _preparacionRepositoryMock.Verify(
            x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task PrepararProductoAsync_ConProductoIdVacio_DebeRetornarFallo()
    {
        // Arrange
        var chefId = Guid.NewGuid();
        var fechaVencimiento = _fechaActual.AddDays(1);
        var observaciones = "Prueba de preparación";

        // Act
        var resultado = await _servicio.PrepararProductoAsync(Guid.Empty, 10, chefId, fechaVencimiento, observaciones);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("producto"));
    }

    [Fact]
    public async Task PrepararProductoAsync_ConCantidadCero_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        var fechaVencimiento = _fechaActual.AddDays(1);
        var observaciones = "Prueba de preparación";

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, 0, chefId, fechaVencimiento, observaciones);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("cantidad"));
    }

    [Fact]
    public async Task PrepararProductoAsync_ConCantidadNegativa_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        var fechaVencimiento = _fechaActual.AddDays(1);
        var observaciones = "Prueba de preparación";

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, -5, chefId, fechaVencimiento, observaciones);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("cantidad"));
    }

    [Fact]
    public async Task PrepararProductoAsync_ConChefIdVacio_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var fechaVencimiento = _fechaActual.AddDays(1);
        var observaciones = "Prueba de preparación";

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, 10, Guid.Empty, fechaVencimiento, observaciones);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("chef"));
    }

    [Fact]
    public async Task PrepararProductoAsync_ConFechaVencimientoPasada_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        var fechaVencimientoPasada = _fechaActual.AddDays(-1);
        var observaciones = "Prueba de preparación";

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, 10, chefId, fechaVencimientoPasada, observaciones);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("vencimiento"));
    }

    [Fact]
    public async Task PrepararProductoAsync_ConErrorEnRepositorio_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidad = 10;
        var chefId = Guid.NewGuid();
        var fechaVencimiento = _fechaActual.AddDays(1);
        var observaciones = "Prueba de preparación";
        
        _preparacionRepositoryMock
            .Setup(x => x.AgregarAsync(It.IsAny<PreparacionDiaria>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, cantidad, chefId, fechaVencimiento, observaciones);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("Error") || e.Contains("error"));
    }

    #endregion

    #region VerificarDisponibilidadAsync Tests

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConPreparacionesDisponibles_DebeRetornarTrue()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidadRequerida = 3;
        
        // Crear preparaciones con cantidad suficiente
        var preparaciones = new List<PreparacionDiaria>
        {
            CrearPreparacionMock(productoId, 5, EstadoPreparacion.Disponible)
        };
        
        _preparacionRepositoryMock
            .Setup(x => x.ObtenerPreparacionesDisponiblesPorProductoAsync(
                It.Is<Guid>(id => id == productoId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(preparaciones);

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(productoId, cantidadRequerida);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_SinPreparacionesSuficientes_DebeRetornarFalse()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidadRequerida = 10;
        
        // Crear preparaciones con cantidad insuficiente
        var preparaciones = new List<PreparacionDiaria>
        {
            CrearPreparacionMock(productoId, 2, EstadoPreparacion.Disponible),
            CrearPreparacionMock(productoId, 3, EstadoPreparacion.Disponible)
        };
        
        _preparacionRepositoryMock
            .Setup(x => x.ObtenerPreparacionesDisponiblesPorProductoAsync(
                It.Is<Guid>(id => id == productoId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(preparaciones);

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(productoId, cantidadRequerida);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().BeFalse();
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConProductoIdVacio_DebeRetornarFallo()
    {
        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(Guid.Empty, 5);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("producto"));
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConCantidadCero_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(productoId, 0);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("cantidad"));
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConCantidadNegativa_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(productoId, -5);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("cantidad"));
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConErrorEnRepositorio_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        
        _preparacionRepositoryMock
            .Setup(x => x.ObtenerPreparacionesDisponiblesPorProductoAsync(
                It.Is<Guid>(id => id == productoId), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(productoId, 5);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("Error") || e.Contains("error"));
    }

    #endregion

    #region ConsumirPreparacionAsync Tests

    [Fact]
    public async Task ConsumirPreparacionAsync_ConParametrosValidos_DebeRetornarExito()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidad = 3;
    
        // Configurar mock para retornar preparaciones con cantidad suficiente
        var preparacion = CrearPreparacionMock(productoId, 5, EstadoPreparacion.Disponible);
    
        _preparacionRepositoryMock
            .Setup(x => x.ObtenerPreparacionesDisponiblesPorProductoAsync(
                It.Is<Guid>(id => id == productoId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PreparacionDiaria> { preparacion });
            
        _preparacionRepositoryMock
            .Setup(x => x.ActualizarAsync(It.IsAny<PreparacionDiaria>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _preparacionRepositoryMock
            .Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _servicio.ConsumirPreparacionAsync(productoId, cantidad);

        // Assert
        resultado.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ConsumirPreparacionAsync_ConProductoIdVacio_DebeRetornarFallo()
    {
        // Act
        var resultado = await _servicio.ConsumirPreparacionAsync(Guid.Empty, 3);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task ConsumirPreparacionAsync_ConCantidadCero_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();

        // Act
        var resultado = await _servicio.ConsumirPreparacionAsync(productoId, 0);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    #endregion

    #region ObtenerPreparacionesDelDiaAsync Tests

    [Fact]
    public async Task ObtenerPreparacionesDelDiaAsync_DebeRetornarListaVacia()
    {
        // Act
        var resultado = await _servicio.ObtenerPreparacionesDelDiaAsync();

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Should().BeEmpty(); // Implementación temporal retorna lista vacía
    }

    #endregion

    #region ObtenerPreparacionesPorProductoAsync Tests

    [Fact]
    public async Task ObtenerPreparacionesPorProductoAsync_ConProductoValido_DebeRetornarListaVacia()
    {
        // Arrange
        var productoId = Guid.NewGuid();

        // Act
        var resultado = await _servicio.ObtenerPreparacionesPorProductoAsync(productoId);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPreparacionesPorProductoAsync_ConProductoIdVacio_DebeRetornarFallo()
    {
        // Act
        var resultado = await _servicio.ObtenerPreparacionesPorProductoAsync(Guid.Empty);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("producto"));
    }

    #endregion

    #region MarcarVencidasAsync Tests

    [Fact]
    public async Task MarcarVencidasAsync_DebeRetornarCero()
    {
        // Act
        var resultado = await _servicio.MarcarVencidasAsync();

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().Be(0); // Implementación temporal
    }

    #endregion

    #region ObtenerPreparacionesPorVencerAsync Tests

    [Fact]
    public async Task ObtenerPreparacionesPorVencerAsync_ConHorasValidas_DebeRetornarListaVacia()
    {
        // Arrange
        var horasAnticipacion = 2;

        // Act
        var resultado = await _servicio.ObtenerPreparacionesPorVencerAsync(horasAnticipacion);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerPreparacionesPorVencerAsync_ConHorasNegativas_DebeRetornarFallo()
    {
        // Act
        var resultado = await _servicio.ObtenerPreparacionesPorVencerAsync(-1);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("horas"));
    }

    #endregion

    #region MarcarComoDisponibleAsync Tests

    [Fact]
    public async Task MarcarComoDisponibleAsync_ConIdValido_DebeRetornarExito()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        var fechaVencimiento = _fechaActual.AddHours(8);
        var observaciones = "Test";
        var fechaCreacion = _fechaActual;
        
        var preparacion = PreparacionDiaria.Crear(
            Guid.NewGuid(), 
            5, 
            chefId,
            fechaVencimiento,
            observaciones,
            fechaCreacion);
            
        // Configurar el mock para devolver una preparación válida
        _preparacionRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(
                It.Is<Guid>(id => id == preparacionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(preparacion);
            
        _preparacionRepositoryMock
            .Setup(x => x.ActualizarAsync(
                It.IsAny<PreparacionDiaria>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _preparacionRepositoryMock
            .Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _servicio.MarcarComoDisponibleAsync(preparacionId);

        // Assert
        resultado.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task MarcarComoDisponibleAsync_ConIdVacio_DebeRetornarFallo()
    {
        // Act
        var resultado = await _servicio.MarcarComoDisponibleAsync(Guid.Empty);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    #endregion

    #region AgregarCantidadAsync Tests

    [Fact]
    public async Task AgregarCantidadAsync_ConParametrosValidos_DebeRetornarExito()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();
        var cantidadAdicional = 5;
        var chefId = Guid.NewGuid();
        var fechaVencimiento = _fechaActual.AddHours(8);
        var observaciones = "Test";
        var fechaCreacion = _fechaActual;
        
        var preparacion = PreparacionDiaria.Crear(
            Guid.NewGuid(), 
            10, 
            chefId,
            fechaVencimiento,
            observaciones,
            fechaCreacion);
            
        // Configurar el mock para devolver una preparación válida
        _preparacionRepositoryMock
            .Setup(x => x.ObtenerPorIdAsync(
                It.Is<Guid>(id => id == preparacionId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(preparacion);
            
        _preparacionRepositoryMock
            .Setup(x => x.ActualizarAsync(
                It.IsAny<PreparacionDiaria>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _preparacionRepositoryMock
            .Setup(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _servicio.AgregarCantidadAsync(preparacionId, cantidadAdicional);

        // Assert
        resultado.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task AgregarCantidadAsync_ConIdVacio_DebeRetornarFallo()
    {
        // Act
        var resultado = await _servicio.AgregarCantidadAsync(Guid.Empty, 5);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AgregarCantidadAsync_ConCantidadCero_DebeRetornarFallo()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();

        // Act
        var resultado = await _servicio.AgregarCantidadAsync(preparacionId, 0);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AgregarCantidadAsync_ConCantidadNegativa_DebeRetornarFallo()
    {
        // Arrange
        var preparacionId = Guid.NewGuid();

        // Act
        var resultado = await _servicio.AgregarCantidadAsync(preparacionId, -2);

        // Assert
        resultado.Succeeded.Should().BeFalse();
    }

    #endregion

    #region ObtenerEstadisticasDelDiaAsync Tests

    [Fact]
    public async Task ObtenerEstadisticasDelDiaAsync_DebeRetornarEstadisticasVacias()
    {
        // Arrange
        var estadisticas = new EstadisticasPreparaciones();
        
        // Configurar el mock para devolver estadísticas vacías
        _preparacionRepositoryMock
            .Setup(x => x.ObtenerEstadisticasDelDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(estadisticas);

        // Act
        var resultado = await _servicio.ObtenerEstadisticasDelDiaAsync();

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.TotalPreparaciones.Should().Be(0);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Crea una preparación mock para pruebas
    /// </summary>
    private PreparacionDiaria CrearPreparacionMock(Guid productoId, int cantidadDisponible, EstadoPreparacion estado)
    {
        var chefId = Guid.NewGuid();
        var fechaVencimiento = _fechaActual.AddHours(8);
        var observaciones = "Test";
        var fechaCreacion = _fechaActual;
        
        // Crear la preparación con todos los parámetros explícitos (sin usar valores opcionales)
        var preparacion = PreparacionDiaria.Crear(
            productoId,
            cantidadDisponible,
            chefId,
            fechaVencimiento,
            observaciones,
            fechaCreacion);

        // Si el estado es diferente a Preparando, llamar a MarcarComoDisponible
        if (estado != EstadoPreparacion.Preparando)
        {
            preparacion.MarcarComoDisponible();
        }

        // Establecer el estado correcto según corresponda
        switch (estado)
        {
            case EstadoPreparacion.PorVencer:
                preparacion.MarcarComoPorVencer();
                break;
            case EstadoPreparacion.Vencida:
                preparacion.MarcarComoVencida();
                break;
            case EstadoPreparacion.Agotada:
                preparacion.ConsumirCantidad(cantidadDisponible); // Consumir todo
                break;
        }

        return preparacion;
    }

    #endregion
} 