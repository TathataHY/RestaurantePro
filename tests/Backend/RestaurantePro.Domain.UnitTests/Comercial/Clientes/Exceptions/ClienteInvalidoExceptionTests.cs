namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.Exceptions;

public class ClienteInvalidoExceptionTests
{
    [Fact]
    public void Constructor_ConParametrosValidos_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var estaActivo = true;
        var operacion = "procesar venta";
        var razon = "Límite de crédito excedido";

        // Act
        var excepcion = new ClienteInvalidoException(clienteId, estaActivo, operacion, razon);

        // Assert
        excepcion.ClienteId.Should().Be(clienteId);
        excepcion.EstaActivo.Should().Be(estaActivo);
        excepcion.ErrorCode.Should().Be("BUSINESS_RULE_VIOLATION");
        excepcion.EntityId.Should().Be(clienteId);
        excepcion.Message.Should().Contain(operacion);
        excepcion.Message.Should().Contain(razon);
        excepcion.Data["EstaActivo"].Should().Be(estaActivo);
        excepcion.Data["Operacion"].Should().Be(operacion);
        excepcion.Data["Razon"].Should().Be(razon);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_ConDiferentesEstados_DeberiaCrearExcepcionCorrecta(bool estaActivo)
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var operacion = "test";
        var razon = "Test reason";

        // Act
        var excepcion = new ClienteInvalidoException(clienteId, estaActivo, operacion, razon);

        // Assert
        excepcion.EstaActivo.Should().Be(estaActivo);
    }

    [Fact]
    public void ParaClienteDesactivado_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var operacion = "realizar compra";

        // Act
        var excepcion = ClienteInvalidoException.ParaClienteDesactivado(clienteId, operacion);

        // Assert
        excepcion.ClienteId.Should().Be(clienteId);
        excepcion.EstaActivo.Should().BeFalse();
        excepcion.Message.Should().Contain("realizar compra");
        excepcion.Message.Should().Contain("desactivado");
        excepcion.Data["Operacion"].Should().Be(operacion);
    }

    [Fact]
    public void ParaClienteDesactivado_SinOperacion_DeberiaUsarValorPorDefecto()
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act
        var excepcion = ClienteInvalidoException.ParaClienteDesactivado(clienteId);

        // Assert
        excepcion.Data["Operacion"].Should().Be("procesar");
    }

    [Fact]
    public void ParaTarjetaVencida_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var fechaVencimiento = DateTime.Now.AddDays(-30); // Vencida hace 30 días
        var operacion = "canjear puntos";

        // Act
        var excepcion = ClienteInvalidoException.ParaTarjetaVencida(clienteId, fechaVencimiento, operacion);

        // Assert
        excepcion.ClienteId.Should().Be(clienteId);
        excepcion.EstaActivo.Should().BeTrue();
        excepcion.Message.Should().Contain("tarjeta de fidelización venció");
        excepcion.Message.Should().Contain("30 días");
        excepcion.Data["FechaVencimiento"].Should().Be(fechaVencimiento);
        excepcion.Data["DiasVencida"].Should().Be(30);
        excepcion.Data["TipoProblema"].Should().Be("TarjetaVencida");
    }

    [Fact]
    public void ParaPuntosInsuficientes_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var puntosActuales = 100;
        var puntosRequeridos = 250;
        var operacion = "canjear producto";

        // Act
        var excepcion = ClienteInvalidoException.ParaPuntosInsuficientes(clienteId, puntosActuales, puntosRequeridos, operacion);

        // Assert
        excepcion.ClienteId.Should().Be(clienteId);
        excepcion.EstaActivo.Should().BeTrue();
        excepcion.Message.Should().Contain("Puntos insuficientes");
        excepcion.Message.Should().Contain("100");
        excepcion.Message.Should().Contain("250");
        excepcion.Data["PuntosActuales"].Should().Be(puntosActuales);
        excepcion.Data["PuntosRequeridos"].Should().Be(puntosRequeridos);
        excepcion.Data["Deficit"].Should().Be(150);
        excepcion.Data["TipoProblema"].Should().Be("PuntosInsuficientes");
    }

    [Fact]
    public void ParaClienteSinTarjeta_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var operacion = "acumular puntos";

        // Act
        var excepcion = ClienteInvalidoException.ParaClienteSinTarjeta(clienteId, operacion);

        // Assert
        excepcion.ClienteId.Should().Be(clienteId);
        excepcion.EstaActivo.Should().BeTrue();
        excepcion.Message.Should().Contain("no tiene una tarjeta de fidelización");
        excepcion.Data["TipoProblema"].Should().Be("SinTarjeta");
    }

    [Fact]
    public void ParaLimiteCreditoExcedido_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var limiteCredito = 1000m;
        var saldoPendiente = 700m;
        var montoSolicitud = 400m;
        var operacion = "venta a crédito";

        // Act
        var excepcion = ClienteInvalidoException.ParaLimiteCreditoExcedido(
            clienteId, limiteCredito, saldoPendiente, montoSolicitud, operacion);

        // Assert
        excepcion.ClienteId.Should().Be(clienteId);
        excepcion.EstaActivo.Should().BeTrue();
        excepcion.Message.Should().Contain("Límite de crédito excedido");
        excepcion.Message.Should().Contain("300.00"); // Disponible
        excepcion.Message.Should().Contain("400.00"); // Solicitado
        excepcion.Data["LimiteCredito"].Should().Be(limiteCredito);
        excepcion.Data["SaldoPendiente"].Should().Be(saldoPendiente);
        excepcion.Data["MontoSolicitud"].Should().Be(montoSolicitud);
        excepcion.Data["Disponible"].Should().Be(300m);
        excepcion.Data["Exceso"].Should().Be(100m);
        excepcion.Data["TipoProblema"].Should().Be("LimiteCreditoExcedido");
    }

    [Fact]
    public void ParaFacturacionVencida_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var facturasPendientes = 3;
        var montoVencido = 1500.50m;
        var diasVencimiento = 45;
        var operacion = "nueva compra";

        // Act
        var excepcion = ClienteInvalidoException.ParaFacturacionVencida(
            clienteId, facturasPendientes, montoVencido, diasVencimiento, operacion);

        // Assert
        excepcion.ClienteId.Should().Be(clienteId);
        excepcion.EstaActivo.Should().BeTrue();
        excepcion.Message.Should().Contain("3 facturas vencidas");
        excepcion.Message.Should().Contain("1500.50");
        excepcion.Message.Should().Contain("45 días");
        excepcion.Data["FacturasPendientes"].Should().Be(facturasPendientes);
        excepcion.Data["MontoVencido"].Should().Be(montoVencido);
        excepcion.Data["DiasVencimiento"].Should().Be(diasVencimiento);
        excepcion.Data["TipoProblema"].Should().Be("FacturacionVencida");
    }

    [Fact]
    public void ParaInformacionIncompleta_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var camposFaltantes = new[] { "Email", "Teléfono", "Dirección" };
        var operacion = "envío a domicilio";

        // Act
        var excepcion = ClienteInvalidoException.ParaInformacionIncompleta(clienteId, camposFaltantes, operacion);

        // Assert
        excepcion.ClienteId.Should().Be(clienteId);
        excepcion.EstaActivo.Should().BeTrue();
        excepcion.Message.Should().Contain("Información de contacto incompleta");
        excepcion.Message.Should().Contain("Email, Teléfono, Dirección");
        excepcion.Data["CamposFaltantes"].Should().BeEquivalentTo(camposFaltantes);
        excepcion.Data["TipoProblema"].Should().Be("InformacionIncompleta");
    }

    [Fact]
    public void ParaClienteMenorEdad_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var edad = 16;
        var edadMinima = 18;
        var operacion = "comprar bebidas alcohólicas";

        // Act
        var excepcion = ClienteInvalidoException.ParaClienteMenorEdad(clienteId, edad, edadMinima, operacion);

        // Assert
        excepcion.ClienteId.Should().Be(clienteId);
        excepcion.EstaActivo.Should().BeTrue();
        excepcion.Message.Should().Contain("Cliente menor de edad");
        excepcion.Message.Should().Contain("16 años");
        excepcion.Message.Should().Contain("18 años");
        excepcion.Data["EdadCliente"].Should().Be(edad);
        excepcion.Data["EdadMinima"].Should().Be(edadMinima);
        excepcion.Data["TipoProblema"].Should().Be("MenorEdad");
    }

    [Fact]
    public void ParaClienteBloqueado_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var razonBloqueo = "Actividad sospechosa detectada";
        var fechaBloqueo = DateTime.Now.AddDays(-15);
        var operacion = "realizar pago";

        // Act
        var excepcion = ClienteInvalidoException.ParaClienteBloqueado(clienteId, razonBloqueo, fechaBloqueo, operacion);

        // Assert
        excepcion.ClienteId.Should().Be(clienteId);
        excepcion.EstaActivo.Should().BeFalse(); // Cliente bloqueado
        excepcion.Message.Should().Contain("Cliente bloqueado por fraude");
        excepcion.Message.Should().Contain("Actividad sospechosa detectada");
        excepcion.Message.Should().Contain("15 días");
        excepcion.Data["RazonBloqueo"].Should().Be(razonBloqueo);
        excepcion.Data["FechaBloqueo"].Should().Be(fechaBloqueo);
        excepcion.Data["DiasBloqueado"].Should().Be(15);
        excepcion.Data["TipoProblema"].Should().Be("ClienteBloqueado");
    }

    [Fact]
    public void ParaPromocionNoElegible_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var promocionId = Guid.NewGuid();
        var criteriosNoMet = new[] { "Compra mínima no alcanzada", "Cliente no VIP", "Producto no aplicable" };
        var operacion = "aplicar descuento";

        // Act
        var excepcion = ClienteInvalidoException.ParaPromocionNoElegible(clienteId, promocionId, criteriosNoMet, operacion);

        // Assert
        excepcion.ClienteId.Should().Be(clienteId);
        excepcion.EstaActivo.Should().BeTrue();
        excepcion.Message.Should().Contain("No elegible para la promoción");
        excepcion.Message.Should().Contain("Compra mínima no alcanzada, Cliente no VIP, Producto no aplicable");
        excepcion.Data["PromocionId"].Should().Be(promocionId);
        excepcion.Data["CriteriosNoMet"].Should().BeEquivalentTo(criteriosNoMet);
        excepcion.Data["TipoProblema"].Should().Be("PromocionNoElegible");
    }

    [Fact]
    public void MethodosFactory_DeberianRetornarInstanciaCorrecta()
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act & Assert
        var desactivado = ClienteInvalidoException.ParaClienteDesactivado(clienteId);
        desactivado.Should().BeOfType<ClienteInvalidoException>();
        
        var tarjetaVencida = ClienteInvalidoException.ParaTarjetaVencida(clienteId, DateTime.Now.AddDays(-1));
        tarjetaVencida.Should().BeOfType<ClienteInvalidoException>();
        
        var puntosInsuficientes = ClienteInvalidoException.ParaPuntosInsuficientes(clienteId, 50, 100);
        puntosInsuficientes.Should().BeOfType<ClienteInvalidoException>();
        
        var sinTarjeta = ClienteInvalidoException.ParaClienteSinTarjeta(clienteId);
        sinTarjeta.Should().BeOfType<ClienteInvalidoException>();
    }

    [Fact]
    public void ToString_DeberiaIncluirInformacionRelevante()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var excepcion = ClienteInvalidoException.ParaPuntosInsuficientes(clienteId, 50, 150);

        // Act
        var resultado = excepcion.ToString();

        // Assert
        resultado.Should().Contain("Error: [BUSINESS_RULE_VIOLATION]");
        resultado.Should().Contain("Puntos insuficientes");
        resultado.Should().Contain(clienteId.ToString());
        resultado.Should().Contain("Contexto: Comercial");
    }

    [Fact]
    public void Data_DeberiaContenerInformacionCompleta()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var excepcion = ClienteInvalidoException.ParaLimiteCreditoExcedido(clienteId, 1000m, 800m, 300m);

        // Act
        var data = excepcion.Data;

        // Assert
        data.Keys.Cast<string>().Should().Contain("EstaActivo");
        data.Keys.Cast<string>().Should().Contain("Operacion");
        data.Keys.Cast<string>().Should().Contain("Razon");
        data.Keys.Cast<string>().Should().Contain("LimiteCredito");
        data.Keys.Cast<string>().Should().Contain("SaldoPendiente");
        data.Keys.Cast<string>().Should().Contain("MontoSolicitud");
        data.Keys.Cast<string>().Should().Contain("Disponible");
        data.Keys.Cast<string>().Should().Contain("Exceso");
        data.Keys.Cast<string>().Should().Contain("TipoProblema");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ConOperacionVacia_DeberiaCrearExcepcion(string operacion)
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act
        var excepcion = new ClienteInvalidoException(clienteId, true, operacion, "Test");

        // Assert
        excepcion.Data["Operacion"].Should().Be(operacion);
    }

    [Fact]
    public void ParaTarjetaVencida_ConFechaFutura_DeberiaCalcularDiasNegativos()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var fechaFutura = DateTime.Now.AddDays(10);

        // Act
        var excepcion = ClienteInvalidoException.ParaTarjetaVencida(clienteId, fechaFutura);

        // Assert
        // Permitir una tolerancia de ±1 día debido a timing de ejecución
        var diasVencida = (int)excepcion.Data["DiasVencida"]!;
        diasVencida.Should().BeInRange(-11, -9, "because the calculation should be around -10 days with timing tolerance");
    }

    [Fact]
    public void ParaClienteBloqueado_ConFechaFutura_DeberiaCalcularDiasNegativos()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var fechaFutura = DateTime.Now.AddDays(5);

        // Act
        var excepcion = ClienteInvalidoException.ParaClienteBloqueado(clienteId, "Test", fechaFutura);

        // Assert
        // Permitir una tolerancia de ±1 día debido a timing de ejecución
        var diasBloqueado = (int)excepcion.Data["DiasBloqueado"]!;
        diasBloqueado.Should().BeInRange(-6, -4, "because the calculation should be around -5 days with timing tolerance");
    }

    [Fact]
    public void ParaInformacionIncompleta_ConArrayVacio_DeberiaCrearExcepcion()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var camposVacios = Array.Empty<string>();

        // Act
        var excepcion = ClienteInvalidoException.ParaInformacionIncompleta(clienteId, camposVacios);

        // Assert
        excepcion.Data["CamposFaltantes"].Should().BeEquivalentTo(camposVacios);
        excepcion.Message.Should().Contain("Campos requeridos: ");
    }

    [Fact]
    public void ParaPromocionNoElegible_ConArrayVacio_DeberiaCrearExcepcion()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var promocionId = Guid.NewGuid();
        var criteriosVacios = Array.Empty<string>();

        // Act
        var excepcion = ClienteInvalidoException.ParaPromocionNoElegible(clienteId, promocionId, criteriosVacios);

        // Assert
        excepcion.Data["CriteriosNoMet"].Should().BeEquivalentTo(criteriosVacios);
        excepcion.Message.Should().Contain("Criterios no cumplidos: ");
    }
} 