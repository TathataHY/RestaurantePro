namespace RestaurantePro.Domain.UnitTests.Operaciones.Comandas.Exceptions;

public class ComandaInvalidaExceptionTests
{
    [Fact]
    public void Constructor_ConParametrosValidos_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var estadoActual = EstadoComanda.Finalizada;
        var operacion = "modificar items";
        var razon = "La comanda ya está finalizada";

        // Act
        var excepcion = new ComandaInvalidaException(comandaId, estadoActual, operacion, razon);

        // Assert
        excepcion.ComandaId.Should().Be(comandaId);
        excepcion.EstadoActual.Should().Be(estadoActual);
        excepcion.ErrorCode.Should().Be("BUSINESS_RULE_VIOLATION");
        excepcion.DomainContext.Should().Be("Operaciones");
        excepcion.Message.Should().Contain("modificar items");
        excepcion.Message.Should().Contain("Finalizada");
        excepcion.AdditionalData.Should().ContainKey("EstadoActual");
        excepcion.AdditionalData.Should().ContainKey("Operacion");
        excepcion.AdditionalData.Should().ContainKey("Razon");
    }

    [Fact]
    public void ParaComandaFinalizada_ConOperacionEspecifica_DeberiaCrearExcepcionCorrecta()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var operacion = "agregar producto";

        // Act
        var excepcion = ComandaInvalidaException.ParaComandaFinalizada(comandaId, operacion);

        // Assert
        excepcion.ComandaId.Should().Be(comandaId);
        excepcion.EstadoActual.Should().Be(EstadoComanda.Finalizada);
        excepcion.Message.Should().Contain("agregar producto");
        excepcion.Message.Should().Contain("ya ha sido finalizada");
    }

    [Fact]
    public void ParaComandaFinalizada_SinOperacion_DeberiaUsarOperacionPorDefecto()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var excepcion = ComandaInvalidaException.ParaComandaFinalizada(comandaId);

        // Assert
        excepcion.Message.Should().Contain("modificar");
    }

    [Fact]
    public void ParaComandaCancelada_DeberiaCrearExcepcionCorrecta()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var operacion = "procesar pago";

        // Act
        var excepcion = ComandaInvalidaException.ParaComandaCancelada(comandaId, operacion);

        // Assert
        excepcion.ComandaId.Should().Be(comandaId);
        excepcion.EstadoActual.Should().Be(EstadoComanda.Cancelada);
        excepcion.Message.Should().Contain("procesar pago");
        excepcion.Message.Should().Contain("ha sido cancelada");
    }

    [Fact]
    public void ParaComandaSinItems_DeberiaCrearExcepcionCorrecta()
    {
        // Arrange
        var comandaId = Guid.NewGuid();

        // Act
        var excepcion = ComandaInvalidaException.ParaComandaSinItems(comandaId);

        // Assert
        excepcion.ComandaId.Should().Be(comandaId);
        excepcion.Message.Should().Contain("no tiene items");
        excepcion.AdditionalData.Should().ContainKey("ItemsCount");
        excepcion.AdditionalData["ItemsCount"].Should().Be(0);
    }

    [Fact]
    public void ParaDescuentoInvalido_DeberiaCrearExcepcionConDatos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var descuentoSolicitado = 0.25m; // 25%
        var descuentoMaximo = 0.15m; // 15%

        // Act
        var excepcion = ComandaInvalidaException.ParaDescuentoInvalido(
            comandaId, descuentoSolicitado, descuentoMaximo);

        // Assert
        excepcion.ComandaId.Should().Be(comandaId);
        excepcion.Message.Should().Contain("25.00%");
        excepcion.Message.Should().Contain("15.00%");
        excepcion.AdditionalData.Should().ContainKey("DescuentoSolicitado");
        excepcion.AdditionalData.Should().ContainKey("DescuentoMaximo");
        excepcion.AdditionalData["DescuentoSolicitado"].Should().Be(descuentoSolicitado);
        excepcion.AdditionalData["DescuentoMaximo"].Should().Be(descuentoMaximo);
    }

    [Fact]
    public void ParaMesaOcupada_DeberiaCrearExcepcionConMesaId()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();

        // Act
        var excepcion = ComandaInvalidaException.ParaMesaOcupada(comandaId, mesaId);

        // Assert
        excepcion.ComandaId.Should().Be(comandaId);
        excepcion.Message.Should().Contain("ya está ocupada");
        excepcion.AdditionalData.Should().ContainKey("MesaId");
        excepcion.AdditionalData["MesaId"].Should().Be(mesaId);
    }

    [Fact]
    public void ToResult_DeberiaRetornarResultConError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var excepcion = ComandaInvalidaException.ParaComandaFinalizada(comandaId);

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
        var comandaId = Guid.NewGuid();
        var excepcion = ComandaInvalidaException.ParaComandaFinalizada(comandaId, "eliminar items");

        // Act
        var mensaje = excepcion.GetDetailedMessage();

        // Assert
        mensaje.Should().Contain("BUSINESS_RULE_VIOLATION");
        mensaje.Should().Contain("Operaciones");
        mensaje.Should().Contain("eliminar items");
        mensaje.Should().Contain("EstadoActual");
        mensaje.Should().Contain("Operacion");
    }
} 