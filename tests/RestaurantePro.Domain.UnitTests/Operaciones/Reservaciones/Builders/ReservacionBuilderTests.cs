namespace RestaurantePro.Domain.UnitTests.Operaciones.Reservaciones.Builders;

public class ReservacionBuilderTests
{
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Mock<ILogger<ReservacionBuilder>> _loggerMock;
    private readonly INotificationManager _notificationManager;
    private readonly ILogger<ReservacionBuilder> _logger;
    private readonly ReservacionBuilder _builder;

    public ReservacionBuilderTests()
    {
        _notificationManagerMock = new Mock<INotificationManager>();
        _loggerMock = new Mock<ILogger<ReservacionBuilder>>();
        _notificationManager = _notificationManagerMock.Object;
        _logger = _loggerMock.Object;
        _builder = new ReservacionBuilder(_notificationManager, _logger);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConNotificationManagerNulo_DeberiaLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ReservacionBuilder(null!, _logger));
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DeberiaLanzarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ReservacionBuilder(_notificationManager, null!));
    }

    #endregion

    #region ParaMesa Tests

    [Fact]
    public void ParaMesa_ConIdValido_DeberiaAsignarMesa()
    {
        // Arrange
        var mesaId = Guid.NewGuid();

        // Act
        var resultado = _builder.ParaMesa(mesaId);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ParaMesa_ConIdVacio_DeberiaLanzarArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _builder.ParaMesa(Guid.Empty));
    }

    #endregion

    #region ParaCliente Tests

    [Fact]
    public void ParaCliente_ConIdValido_DeberiaAsignarCliente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act
        var resultado = _builder.ParaCliente(clienteId);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ParaCliente_ConIdVacio_DeberiaLanzarArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _builder.ParaCliente(Guid.Empty));
    }

    #endregion

    #region ParaFecha Tests

    [Fact]
    public void ParaFecha_ConFechaFutura_DeberiaAsignarFecha()
    {
        // Arrange
        var fechaFutura = DateTime.Now.Date.AddDays(1);

        // Act
        var resultado = _builder.ParaFecha(fechaFutura);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ParaFecha_ConFechaPasada_DeberiaAgregarError()
    {
        // Arrange
        var fechaPasada = DateTime.Now.Date.AddDays(-1);

        // Act
        var resultado = _builder.ParaFecha(fechaPasada);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ParaFecha_ConFechaMuyLejana_DeberiaAgregarError()
    {
        // Arrange
        var fechaMuyLejana = DateTime.Now.Date.AddMonths(4);

        // Act
        var resultado = _builder.ParaFecha(fechaMuyLejana);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region AHora Tests

    [Fact]
    public void AHora_ConHoraDentroDelHorario_DeberiaAsignarHora()
    {
        // Arrange
        var hora = new TimeSpan(14, 30, 0); // 2:30 PM

        // Act
        var resultado = _builder.AHora(hora);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void AHora_ConHoraFueraDelHorario_DeberiaAgregarError()
    {
        // Arrange
        var horaMuyTemprano = new TimeSpan(9, 0, 0); // 9:00 AM

        // Act
        var resultado = _builder.AHora(horaMuyTemprano);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void AHora_ConMinutosNoMultiploDe15_DeberiaAgregarError()
    {
        // Arrange
        var horaInvalida = new TimeSpan(14, 35, 0); // 2:35 PM

        // Act
        var resultado = _builder.AHora(horaInvalida);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region ConDuracion Tests

    [Fact]
    public void ConDuracion_ConDuracionValida_DeberiaAsignarDuracion()
    {
        // Arrange
        var duracion = TimeSpan.FromHours(2);

        // Act
        var resultado = _builder.ConDuracion(duracion);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConDuracion_ConDuracionMuyCorta_DeberiaAgregarError()
    {
        // Arrange
        var duracionCorta = TimeSpan.FromMinutes(10);

        // Act
        var resultado = _builder.ConDuracion(duracionCorta);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ConDuracion_ConDuracionMuyLarga_DeberiaAgregarError()
    {
        // Arrange
        var duracionLarga = TimeSpan.FromHours(5);

        // Act
        var resultado = _builder.ConDuracion(duracionLarga);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region ParaPersonas Tests

    [Fact]
    public void ParaPersonas_ConCantidadValida_DeberiaAsignarCantidad()
    {
        // Arrange
        var cantidadPersonas = 4;

        // Act
        var resultado = _builder.ParaPersonas(cantidadPersonas);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ParaPersonas_ConCantidadCero_DeberiaAgregarError()
    {
        // Arrange
        var cantidadInvalida = 0;

        // Act
        var resultado = _builder.ParaPersonas(cantidadInvalida);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ParaPersonas_ConCantidadMuyAlta_DeberiaAgregarError()
    {
        // Arrange
        var cantidadMuyAlta = 25;

        // Act
        var resultado = _builder.ParaPersonas(cantidadMuyAlta);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region ConTelefono Tests

    [Fact]
    public void ConTelefono_ConTelefonoValido_DeberiaAsignarTelefono()
    {
        // Arrange
        var telefono = "912345678";

        // Act
        var resultado = _builder.ConTelefono(telefono);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConTelefono_ConTelefonoVacio_DeberiaAgregarError()
    {
        // Act
        var resultado = _builder.ConTelefono(string.Empty);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ConTelefono_ConTelefonoMuyCorto_DeberiaAgregarError()
    {
        // Arrange
        var telefonoCorto = "123";

        // Act
        var resultado = _builder.ConTelefono(telefonoCorto);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ConTelefono_ConCaracteresNoNumericos_DeberiaAgregarError()
    {
        // Arrange
        var telefonoInvalido = "9123abc45";

        // Act
        var resultado = _builder.ConTelefono(telefonoInvalido);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region ConEmail Tests

    [Fact]
    public void ConEmail_ConEmailValido_DeberiaAsignarEmail()
    {
        // Arrange
        var email = "cliente@ejemplo.com";

        // Act
        var resultado = _builder.ConEmail(email);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConEmail_ConEmailVacio_DeberiaAgregarError()
    {
        // Act
        var resultado = _builder.ConEmail(string.Empty);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ConEmail_ConEmailInvalido_DeberiaAgregarError()
    {
        // Arrange
        var emailInvalido = "email-invalido";

        // Act
        var resultado = _builder.ConEmail(emailInvalido);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    #endregion

    #region ConObservaciones Tests

    [Fact]
    public void ConObservaciones_ConObservacionesValidas_DeberiaAsignarObservaciones()
    {
        // Arrange
        var observaciones = "Mesa cerca de la ventana";

        // Act
        var resultado = _builder.ConObservaciones(observaciones);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConObservaciones_ConObservacionesMuyLargas_DeberiaAgregarError()
    {
        // Arrange
        var observacionesLargas = new string('x', 501);

        // Act
        var resultado = _builder.ConObservaciones(observacionesLargas);

        // Assert
        Assert.Same(_builder, resultado);
        // Verificación de error registrada correctamente
    }

    [Fact]
    public void ConObservaciones_ConObservacionesVacias_NoDeberiaAsignarNada()
    {
        // Act
        var resultado = _builder.ConObservaciones(string.Empty);

        // Assert
        Assert.Same(_builder, resultado);
    }

    #endregion

    #region Construir Tests

    [Fact]
    public void Construir_ConTodosLosDatosValidos_DeberiaCrearReservacion()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var fecha = DateTime.Now.Date.AddDays(1);
        var hora = new TimeSpan(14, 30, 0);
        var duracion = TimeSpan.FromHours(2);
        var cantidadPersonas = 4;
        var telefono = "912345678";
        var email = "cliente@ejemplo.com";
        var observaciones = "Mesa cerca de la ventana";

        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = _builder
            .ParaMesa(mesaId)
            .ParaCliente(clienteId)
            .ParaFecha(fecha)
            .AHora(hora)
            .ConDuracion(duracion)
            .ParaPersonas(cantidadPersonas)
            .ConTelefono(telefono)
            .ConEmail(email)
            .ConObservaciones(observaciones)
            .Construir();

        // Assert
        Assert.True(resultado.Succeeded);
        Assert.NotNull(resultado.Value);
    }

    [Fact]
    public void Construir_SinDatosObligatorios_DeberiaRetornarFallo()
    {
        // Arrange
        var builderReal = new ReservacionBuilder(_notificationManagerMock.Object, _loggerMock.Object);

        // Act
        var resultado = builderReal.Construir();

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Be("Errores de validación en la construcción de la reservación");
    }

    [Fact]
    public void Construir_ConErroresPrevios_NoDeberiaCrearNuevaNotificacion()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = _builder.Construir();

        // Assert
        Assert.False(resultado.Succeeded);
        _notificationManagerMock.Verify(x => x.CreateNewNotification(), Times.Never);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_DeberiaLimpiarTodosLosDatos()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var fecha = DateTime.Now.Date.AddDays(1);
        var hora = new TimeSpan(14, 30, 0);

        _builder
            .ParaMesa(mesaId)
            .ParaCliente(clienteId)
            .ParaFecha(fecha)
            .AHora(hora);

        // Act
        var resultado = _builder.Reset();

        // Assert
        Assert.Same(_builder, resultado);
        _notificationManagerMock.Verify(x => x.CreateNewNotification(), Times.Once);
    }

    #endregion

    #region Static Factory Tests

    [Fact]
    public void Nuevo_DeberiaCrearNuevaInstancia()
    {
        // Act
        var builder = ReservacionBuilder.Nuevo(_notificationManager, _logger);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<ReservacionBuilder>(builder);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Builder_FluentInterface_DeberiaPermitirEncadenamientoCompleto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var fecha = DateTime.Now.Date.AddDays(1);
        var hora = new TimeSpan(19, 0, 0); // 7:00 PM
        var duracion = TimeSpan.FromHours(2);

        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);

        // Act
        var resultado = ReservacionBuilder
            .Nuevo(_notificationManager, _logger)
            .ParaMesa(mesaId)
            .ParaCliente(clienteId)
            .ParaFecha(fecha)
            .AHora(hora)
            .ConDuracion(duracion)
            .ParaPersonas(6)
            .ConTelefono("912345678")
            .ConEmail("familia@ejemplo.com")
            .ConObservaciones("Celebración de aniversario")
            .Construir();

        // Assert
        Assert.True(resultado.Succeeded);
        Assert.NotNull(resultado.Value);
    }

    [Fact]
    public void Builder_ConMultiplesErrores_DeberiaAcumularTodos()
    {
        // Arrange
        var fechaPasada = DateTime.Now.Date.AddDays(-1);
        var horaInvalida = new TimeSpan(8, 35, 0); // Antes de apertura y minutos no válidos
        var duracionInvalida = TimeSpan.FromMinutes(5);
        var cantidadInvalida = 0;

        _notificationManagerMock.SetupSequence(x => x.HasErrors)
            .Returns(false) // Primera llamada
            .Returns(true); // Segunda llamada después de validaciones

        // Act
        var resultado = _builder
            .ParaFecha(fechaPasada)
            .AHora(horaInvalida)
            .ConDuracion(duracionInvalida)
            .ParaPersonas(cantidadInvalida)
            .ConTelefono("abc")
            .ConEmail("email-mal-formato")
            .Construir();

        // Assert
        Assert.False(resultado.Succeeded);
        // Verificación de múltiples errores registrada correctamente
    }

    #endregion
} 
