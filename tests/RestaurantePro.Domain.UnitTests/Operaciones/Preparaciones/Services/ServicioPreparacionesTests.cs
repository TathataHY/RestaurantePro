using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Common.Interfaces;
using RestaurantePro.Domain.Common.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;

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
        // Arrange & Act
        var servicio = new ServicioPreparaciones(
            _loggerMock.Object,
            _notificationManager,
            _dateTimeServiceMock.Object,
            _preparacionRepositoryMock.Object);

        // Assert
        servicio.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => 
            new ServicioPreparaciones(null!, _notificationManager, _dateTimeServiceMock.Object, _preparacionRepositoryMock.Object));
        
        ex.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_ConNotificationManagerNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => 
            new ServicioPreparaciones(_loggerMock.Object, null!, _dateTimeServiceMock.Object, _preparacionRepositoryMock.Object));
        
        ex.ParamName.Should().Be("notificationManager");
    }

    [Fact]
    public void Constructor_ConDateTimeServiceNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => 
            new ServicioPreparaciones(_loggerMock.Object, _notificationManager, null!, _preparacionRepositoryMock.Object));
        
        ex.ParamName.Should().Be("dateTimeService");
    }

    [Fact]
    public void Constructor_ConPreparacionRepositoryNulo_DebeLanzarArgumentNullException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => 
            new ServicioPreparaciones(_loggerMock.Object, _notificationManager, _dateTimeServiceMock.Object, null!));
        
        ex.ParamName.Should().Be("preparacionRepository");
    }

    #endregion

    #region PrepararProductoAsync Tests

    [Fact]
    public async Task PrepararProductoAsync_ConParametrosValidos_DebeCrearPreparacion()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        var cantidad = 10;
        var fechaVencimiento = _fechaActual.AddHours(8);
        var observaciones = "Preparación especial";

        // Configurar mock del repositorio
        _preparacionRepositoryMock
            .Setup(x => x.AgregarAsync(It.IsAny<PreparacionDiaria>(), default))
            .Returns(Task.CompletedTask);

        _preparacionRepositoryMock
            .Setup(x => x.GuardarCambiosAsync(default))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _servicio.PrepararProductoAsync(
            productoId, cantidad, chefId, fechaVencimiento, observaciones);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.ProductoId.Should().Be(productoId);
        resultado.Value.ChefId.Should().Be(chefId);
        resultado.Value.CantidadPreparada.Should().Be(cantidad);
        resultado.Value.FechaVencimiento.Should().Be(fechaVencimiento);
        resultado.Value.Observaciones.Should().Be(observaciones);

        // Verificar que se llamó al repositorio
        _preparacionRepositoryMock.Verify(x => x.AgregarAsync(It.IsAny<PreparacionDiaria>(), default), Times.Once);
        _preparacionRepositoryMock.Verify(x => x.GuardarCambiosAsync(default), Times.Once);
    }

    [Fact]
    public async Task PrepararProductoAsync_ConProductoIdVacio_DebeRetornarFallo()
    {
        // Arrange
        var chefId = Guid.NewGuid();

        // Act
        var resultado = await _servicio.PrepararProductoAsync(Guid.Empty, 10, chefId);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("producto") || e.Contains("ID"));

        // Verificar que no se llamó al repositorio
        _preparacionRepositoryMock.Verify(x => x.AgregarAsync(It.IsAny<PreparacionDiaria>(), default), Times.Never);
    }

    [Fact]
    public async Task PrepararProductoAsync_ConCantidadCero_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, 0, chefId);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("cantidad"));

        // Verificar que no se llamó al repositorio
        _preparacionRepositoryMock.Verify(x => x.AgregarAsync(It.IsAny<PreparacionDiaria>(), default), Times.Never);
    }

    [Fact]
    public async Task PrepararProductoAsync_ConCantidadNegativa_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, -5, chefId);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("cantidad"));

        // Verificar que no se llamó al repositorio
        _preparacionRepositoryMock.Verify(x => x.AgregarAsync(It.IsAny<PreparacionDiaria>(), default), Times.Never);
    }

    [Fact]
    public async Task PrepararProductoAsync_ConChefIdVacio_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, 10, Guid.Empty);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("chef"));

        // Verificar que no se llamó al repositorio
        _preparacionRepositoryMock.Verify(x => x.AgregarAsync(It.IsAny<PreparacionDiaria>(), default), Times.Never);
    }

    [Fact]
    public async Task PrepararProductoAsync_ConFechaVencimientoPasada_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();
        var fechaVencimientoPasada = _fechaActual.AddHours(-2);

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, 10, chefId, fechaVencimientoPasada);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("fecha") || e.Contains("vencimiento"));

        // Verificar que no se llamó al repositorio
        _preparacionRepositoryMock.Verify(x => x.AgregarAsync(It.IsAny<PreparacionDiaria>(), default), Times.Never);
    }

    [Fact]
    public async Task PrepararProductoAsync_ConErrorEnRepositorio_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var chefId = Guid.NewGuid();

        // Configurar repositorio para lanzar excepción
        _preparacionRepositoryMock
            .Setup(x => x.AgregarAsync(It.IsAny<PreparacionDiaria>(), default))
            .ThrowsAsync(new Exception("Error de conexión"));

        // Act
        var resultado = await _servicio.PrepararProductoAsync(productoId, 10, chefId);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("Error al preparar producto"));
    }

    #endregion

    #region VerificarDisponibilidadAsync Tests

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConPreparacionesDisponibles_DebeRetornarTrue()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidadRequerida = 5;

        // Crear preparaciones disponibles
        var preparaciones = new List<PreparacionDiaria>
        {
            CrearPreparacionMock(productoId, 3, EstadoPreparacion.Disponible),
            CrearPreparacionMock(productoId, 4, EstadoPreparacion.Disponible),
            CrearPreparacionMock(productoId, 2, EstadoPreparacion.PorVencer),
            CrearPreparacionMock(productoId, 1, EstadoPreparacion.Vencida) // Esta no debería contar
        };

        // Configurar mock del repositorio
        _preparacionRepositoryMock
            .Setup(x => x.ObtenerPreparacionesDisponiblesPorProductoAsync(productoId, default))
            .ReturnsAsync(preparaciones);

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(productoId, cantidadRequerida);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().BeTrue(); // Hay 3 + 4 + 2 = 9 disponibles, y se requieren 5
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_SinPreparacionesSuficientes_DebeRetornarFalse()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidadRequerida = 10;

        // Crear preparaciones disponibles (insuficientes)
        var preparaciones = new List<PreparacionDiaria>
        {
            CrearPreparacionMock(productoId, 3, EstadoPreparacion.Disponible),
            CrearPreparacionMock(productoId, 4, EstadoPreparacion.Disponible),
            CrearPreparacionMock(productoId, 1, EstadoPreparacion.Agotada) // Esta no debería contar
        };

        // Configurar mock del repositorio
        _preparacionRepositoryMock
            .Setup(x => x.ObtenerPreparacionesDisponiblesPorProductoAsync(productoId, default))
            .ReturnsAsync(preparaciones);

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(productoId, cantidadRequerida);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().BeFalse(); // Hay 3 + 4 = 7 disponibles, y se requieren 10
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConProductoIdVacio_DebeRetornarFallo()
    {
        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(Guid.Empty, 5);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("producto") || e.Contains("ID"));

        // Verificar que no se llamó al repositorio
        _preparacionRepositoryMock.Verify(
            x => x.ObtenerPreparacionesDisponiblesPorProductoAsync(It.IsAny<Guid>(), default), 
            Times.Never);
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

        // Verificar que no se llamó al repositorio
        _preparacionRepositoryMock.Verify(
            x => x.ObtenerPreparacionesDisponiblesPorProductoAsync(It.IsAny<Guid>(), default), 
            Times.Never);
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

        // Verificar que no se llamó al repositorio
        _preparacionRepositoryMock.Verify(
            x => x.ObtenerPreparacionesDisponiblesPorProductoAsync(It.IsAny<Guid>(), default), 
            Times.Never);
    }

    [Fact]
    public async Task VerificarDisponibilidadAsync_ConErrorEnRepositorio_DebeRetornarFallo()
    {
        // Arrange
        var productoId = Guid.NewGuid();

        // Configurar repositorio para lanzar excepción
        _preparacionRepositoryMock
            .Setup(x => x.ObtenerPreparacionesDisponiblesPorProductoAsync(productoId, default))
            .ThrowsAsync(new Exception("Error de conexión"));

        // Act
        var resultado = await _servicio.VerificarDisponibilidadAsync(productoId, 5);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("Error al verificar disponibilidad"));
    }

    #endregion

    #region ConsumirPreparacionAsync Tests

    [Fact]
    public async Task ConsumirPreparacionAsync_ConParametrosValidos_DebeRetornarExito()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidad = 3;

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
        // Act
        var resultado = await _servicio.ObtenerEstadisticasDelDiaAsync();

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        // Las estadísticas deberían estar vacías en la implementación temporal
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Crea una preparación mock para pruebas
    /// </summary>
    private PreparacionDiaria CrearPreparacionMock(Guid productoId, int cantidadDisponible, EstadoPreparacion estado)
    {
        var preparacion = PreparacionDiaria.Crear(
            productoId,
            cantidadDisponible, // Misma cantidad inicial y disponible
            Guid.NewGuid(),
            _fechaActual.AddHours(8),
            "Test",
            _fechaActual);

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