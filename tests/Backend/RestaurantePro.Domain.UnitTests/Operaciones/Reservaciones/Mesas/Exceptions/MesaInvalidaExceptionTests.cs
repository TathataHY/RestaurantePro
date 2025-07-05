namespace RestaurantePro.Domain.UnitTests.Operaciones.Reservaciones.Mesas.Exceptions;

public class MesaInvalidaExceptionTests
{
    [Fact]
    public void Constructor_ConParametrosValidos_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var estadoActual = EstadoMesa.Ocupada;
        var operacion = "reservar";
        var razon = "La mesa ya está ocupada";

        // Act
        var excepcion = new MesaInvalidaException(mesaId, estadoActual, operacion, razon);

        // Assert
        excepcion.MesaId.Should().Be(mesaId);
        excepcion.EstadoActual.Should().Be(estadoActual);
        excepcion.ErrorCode.Should().Be("BUSINESS_RULE_VIOLATION");
        excepcion.DomainContext.Should().Be("Operaciones");
        excepcion.Message.Should().Contain("reservar");
        excepcion.Message.Should().Contain("Ocupada");
        excepcion.AdditionalData.Should().ContainKey("EstadoActual");
        excepcion.AdditionalData.Should().ContainKey("Operacion");
        excepcion.AdditionalData.Should().ContainKey("Razon");
    }

    [Fact]
    public void ParaMesaOcupada_ConOperacionEspecifica_DeberiaCrearExcepcionCorrecta()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var operacion = "limpiar";

        // Act
        var excepcion = MesaInvalidaException.ParaMesaOcupada(mesaId, operacion);

        // Assert
        excepcion.MesaId.Should().Be(mesaId);
        excepcion.EstadoActual.Should().Be(EstadoMesa.Ocupada);
        excepcion.Message.Should().Contain("limpiar");
        excepcion.Message.Should().Contain("ya está ocupada");
    }

    [Fact]
    public void ParaMesaOcupada_SinOperacion_DeberiaUsarOperacionPorDefecto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();

        // Act
        var excepcion = MesaInvalidaException.ParaMesaOcupada(mesaId);

        // Assert
        excepcion.Message.Should().Contain("asignar");
    }

    [Fact]
    public void ParaMesaReservada_DeberiaCrearExcepcionCorrecta()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var operacion = "ocupar";

        // Act
        var excepcion = MesaInvalidaException.ParaMesaReservada(mesaId, operacion);

        // Assert
        excepcion.MesaId.Should().Be(mesaId);
        excepcion.EstadoActual.Should().Be(EstadoMesa.Reservada);
        excepcion.Message.Should().Contain("ocupar");
        excepcion.Message.Should().Contain("está reservada");
    }

    [Fact]
    public void ParaMesaFueraDeServicio_ConRazonMantenimiento_DeberiaCrearExcepcionCorrecta()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var operacion = "asignar cliente";
        var razonMantenimiento = "reparación de sillas";

        // Act
        var excepcion = MesaInvalidaException.ParaMesaFueraDeServicio(
            mesaId, operacion, razonMantenimiento);

        // Assert
        excepcion.MesaId.Should().Be(mesaId);
        excepcion.EstadoActual.Should().Be(EstadoMesa.FueraDeServicio);
        excepcion.Message.Should().Contain("asignar cliente");
        excepcion.Message.Should().Contain("reparación de sillas");
        excepcion.AdditionalData.Should().ContainKey("RazonMantenimiento");
        excepcion.AdditionalData["RazonMantenimiento"].Should().Be(razonMantenimiento);
    }

    [Fact]
    public void ParaMesaFueraDeServicio_SinRazon_DeberiaUsarRazonPorDefecto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();

        // Act
        var excepcion = MesaInvalidaException.ParaMesaFueraDeServicio(mesaId);

        // Assert
        excepcion.Message.Should().Contain("mantenimiento general");
    }

    [Fact]
    public void ParaCapacidadInsuficiente_DeberiaCrearExcepcionConDatos()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var personasSolicitadas = 8;
        var capacidadMesa = 4;

        // Act
        var excepcion = MesaInvalidaException.ParaCapacidadInsuficiente(
            mesaId, personasSolicitadas, capacidadMesa);

        // Assert
        excepcion.MesaId.Should().Be(mesaId);
        excepcion.EstadoActual.Should().Be(EstadoMesa.Disponible);
        excepcion.Message.Should().Contain("capacidad para 4 personas");
        excepcion.Message.Should().Contain("se solicitaron 8");
        excepcion.AdditionalData.Should().ContainKey("PersonasSolicitadas");
        excepcion.AdditionalData.Should().ContainKey("CapacidadMesa");
        excepcion.AdditionalData["PersonasSolicitadas"].Should().Be(personasSolicitadas);
        excepcion.AdditionalData["CapacidadMesa"].Should().Be(capacidadMesa);
    }

    [Fact]
    public void ParaNumeroMesaDuplicado_DeberiaCrearExcepcionConNumero()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var numeroMesa = 15;

        // Act
        var excepcion = MesaInvalidaException.ParaNumeroMesaDuplicado(mesaId, numeroMesa);

        // Assert
        excepcion.MesaId.Should().Be(mesaId);
        excepcion.Message.Should().Contain("número 15");
        excepcion.AdditionalData.Should().ContainKey("NumeroMesaDuplicado");
        excepcion.AdditionalData["NumeroMesaDuplicado"].Should().Be(numeroMesa);
    }

    [Fact]
    public void ParaUbicacionInvalida_DeberiaCrearExcepcionConUbicacionYRazon()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var ubicacion = "Terraza Norte";
        var razon = "área en construcción";

        // Act
        var excepcion = MesaInvalidaException.ParaUbicacionInvalida(mesaId, ubicacion, razon);

        // Assert
        excepcion.MesaId.Should().Be(mesaId);
        excepcion.Message.Should().Contain("Terraza Norte");
        excepcion.Message.Should().Contain("área en construcción");
        excepcion.AdditionalData.Should().ContainKey("UbicacionInvalida");
        excepcion.AdditionalData.Should().ContainKey("RazonRechazo");
        excepcion.AdditionalData["UbicacionInvalida"].Should().Be(ubicacion);
        excepcion.AdditionalData["RazonRechazo"].Should().Be(razon);
    }

    [Fact]
    public void ParaLimpiezaRequerida_DeberiaCrearExcepcionCorrecta()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var operacion = "sentar clientes";

        // Act
        var excepcion = MesaInvalidaException.ParaLimpiezaRequerida(mesaId, operacion);

        // Assert
        excepcion.MesaId.Should().Be(mesaId);
        excepcion.EstadoActual.Should().Be(EstadoMesa.Disponible);
        excepcion.Message.Should().Contain("sentar clientes");
        excepcion.Message.Should().Contain("requiere limpieza");
        excepcion.AdditionalData.Should().ContainKey("RequiereLimpieza");
        excepcion.AdditionalData["RequiereLimpieza"].Should().Be(true);
    }

    [Fact]
    public void ToResult_DeberiaRetornarResultConError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var excepcion = MesaInvalidaException.ParaMesaOcupada(mesaId);

        // Act
        var resultado = excepcion.ToResult();

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().NotBeEmpty();
        resultado.Errors.First().Should().Contain("BUSINESS_RULE_VIOLATION");
    }

    [Fact]
    public void GetDetailedMessage_DeberiaIncluirTodaLaInformacion()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var excepcion = MesaInvalidaException.ParaMesaFueraDeServicio(
            mesaId, "limpiar", "mantenimiento de muebles");

        // Act
        var mensaje = excepcion.GetDetailedMessage();

        // Assert
        mensaje.Should().Contain("BUSINESS_RULE_VIOLATION");
        mensaje.Should().Contain("Operaciones");
        mensaje.Should().Contain("limpiar");
        mensaje.Should().Contain("EstadoActual");
        mensaje.Should().Contain("RazonMantenimiento");
    }

    [Theory]
    [InlineData(EstadoMesa.Disponible, "disponible")]
    [InlineData(EstadoMesa.Ocupada, "ocupada")]
    [InlineData(EstadoMesa.Reservada, "reservada")]
    [InlineData(EstadoMesa.FueraDeServicio, "fuera de servicio")]
    public void Constructor_ConDiferentesEstados_DeberiaCrearExcepcionCorrecta(
        EstadoMesa estado, string descripcionEstado)
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var operacion = "test";
        var razon = $"Mesa {descripcionEstado}";

        // Act
        var excepcion = new MesaInvalidaException(mesaId, estado, operacion, razon);

        // Assert
        excepcion.EstadoActual.Should().Be(estado);
        excepcion.Message.Should().Contain(descripcionEstado);
    }
} 