namespace RestaurantePro.Domain.UnitTests.Inventario.Ingredientes.Exceptions;

public class IngredienteInvalidoExceptionTests
{
    [Fact]
    public void Constructor_ConParametrosValidos_DeberiaCrearExcepcionCorrectamente()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 10.5m;
        var operacion = "usar en receta";
        var razon = "Stock insuficiente";

        // Act
        var excepcion = new IngredienteInvalidoException(ingredienteId, stockActual, operacion, razon);

        // Assert
        excepcion.IngredienteId.Should().Be(ingredienteId);
        excepcion.StockActual.Should().Be(stockActual);
        excepcion.ErrorCode.Should().Be("BUSINESS_RULE_VIOLATION");
        excepcion.DomainContext.Should().Be("Inventario");
        excepcion.Message.Should().Contain("usar en receta");
        excepcion.Message.Should().Contain("Stock: 10.5");
        excepcion.AdditionalData.Should().ContainKey("StockActual");
        excepcion.AdditionalData.Should().ContainKey("Operacion");
        excepcion.AdditionalData.Should().ContainKey("Razon");
    }

    [Fact]
    public void ParaStockInsuficiente_ConCantidades_DeberiaCrearExcepcionConDatos()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 5.0m;
        var cantidadSolicitada = 10.0m;
        var operacion = "preparar plato";

        // Act
        var excepcion = IngredienteInvalidoException.ParaStockInsuficiente(
            ingredienteId, stockActual, cantidadSolicitada, operacion);

        // Assert
        excepcion.IngredienteId.Should().Be(ingredienteId);
        excepcion.StockActual.Should().Be(stockActual);
        excepcion.Message.Should().Contain("preparar plato");
        excepcion.Message.Should().Contain("disponible 5");
        excepcion.Message.Should().Contain("solicitado 10");
        excepcion.AdditionalData.Should().ContainKey("CantidadSolicitada");
        excepcion.AdditionalData.Should().ContainKey("Deficit");
        excepcion.AdditionalData["CantidadSolicitada"].Should().Be(cantidadSolicitada);
        excepcion.AdditionalData["Deficit"].Should().Be(5.0m);
    }

    [Fact]
    public void ParaStockInsuficiente_SinOperacionEspecifica_DeberiaUsarOperacionPorDefecto()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 2.0m;
        var cantidadSolicitada = 5.0m;

        // Act
        var excepcion = IngredienteInvalidoException.ParaStockInsuficiente(
            ingredienteId, stockActual, cantidadSolicitada);

        // Assert
        excepcion.Message.Should().Contain("usar");
    }

    [Fact]
    public void ParaIngredienteVencido_ConFecha_DeberiaCrearExcepcionConDiasVencido()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 8.0m;
        var fechaVencimiento = DateTime.Now.AddDays(-5); // Vencido hace 5 días
        var operacion = "usar en cocina";

        // Act
        var excepcion = IngredienteInvalidoException.ParaIngredienteVencido(
            ingredienteId, stockActual, fechaVencimiento, operacion);

        // Assert
        excepcion.IngredienteId.Should().Be(ingredienteId);
        excepcion.StockActual.Should().Be(stockActual);
        excepcion.Message.Should().Contain("usar en cocina");
        excepcion.Message.Should().Contain("vencido desde hace");
        excepcion.Message.Should().Contain("días");
        excepcion.AdditionalData.Should().ContainKey("FechaVencimiento");
        excepcion.AdditionalData.Should().ContainKey("DiasVencido");
        excepcion.AdditionalData["FechaVencimiento"].Should().Be(fechaVencimiento);
        excepcion.AdditionalData["DiasVencido"].Should().Be(5);
    }

    [Fact]
    public void ParaIngredienteProximoAVencer_ConDiasLimite_DeberiaCrearExcepcionCorrecta()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 3.5m;
        var fechaVencimiento = DateTime.Now.AddDays(2); // Vence en 2 días
        var diasLimite = 5;
        var operacion = "usar en producción";

        // Act
        var excepcion = IngredienteInvalidoException.ParaIngredienteProximoAVencer(
            ingredienteId, stockActual, fechaVencimiento, diasLimite, operacion);

        // Assert
        excepcion.IngredienteId.Should().Be(ingredienteId);
        // Permitir tolerancia de ±1 día debido a timing de ejecución
        excepcion.Message.Should().ContainAny("próximo a vencer en 1 días", "próximo a vencer en 2 días", "próximo a vencer en 3 días");
        excepcion.AdditionalData.Should().ContainKey("FechaVencimiento");
        excepcion.AdditionalData.Should().ContainKey("DiasRestantes");
        excepcion.AdditionalData.Should().ContainKey("DiasLimite");
        
        // Permitir tolerancia en los días restantes
        var diasRestantes = (int)excepcion.AdditionalData["DiasRestantes"]!;
        diasRestantes.Should().BeInRange(1, 3, "because the calculation should be around 2 days with timing tolerance");
        excepcion.AdditionalData["DiasLimite"].Should().Be(diasLimite);
    }

    [Fact]
    public void ParaIngredienteProximoAVencer_SinParametrosOpcionales_DeberiaUsarValoresPorDefecto()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 1.0m;
        var fechaVencimiento = DateTime.Now.AddDays(1);

        // Act
        var excepcion = IngredienteInvalidoException.ParaIngredienteProximoAVencer(
            ingredienteId, stockActual, fechaVencimiento);

        // Assert
        excepcion.Message.Should().Contain("usar en producción");
        excepcion.AdditionalData["DiasLimite"].Should().Be(3);
    }

    [Fact]
    public void ParaProveedorSinStock_ConProveedor_DeberiaCrearExcepcionConProveedorId()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 0.0m;
        var proveedorId = Guid.NewGuid();
        var operacion = "solicitar reabastecimiento";

        // Act
        var excepcion = IngredienteInvalidoException.ParaProveedorSinStock(
            ingredienteId, stockActual, proveedorId, operacion);

        // Assert
        excepcion.IngredienteId.Should().Be(ingredienteId);
        excepcion.Message.Should().Contain("solicitar reabastecimiento");
        excepcion.Message.Should().Contain("proveedor no tiene stock");
        excepcion.AdditionalData.Should().ContainKey("ProveedorId");
        excepcion.AdditionalData.Should().ContainKey("TipoProblema");
        excepcion.AdditionalData["ProveedorId"].Should().Be(proveedorId);
        excepcion.AdditionalData["TipoProblema"].Should().Be("ProveedorSinStock");
    }

    [Fact]
    public void ParaUnidadIncompatible_ConUnidades_DeberiaCrearExcepcionConUnidades()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 15.0m;
        var unidadActual = "kilogramos";
        var unidadSolicitada = "litros";
        var operacion = "convertir unidad";

        // Act
        var excepcion = IngredienteInvalidoException.ParaUnidadIncompatible(
            ingredienteId, stockActual, unidadActual, unidadSolicitada, operacion);

        // Assert
        excepcion.IngredienteId.Should().Be(ingredienteId);
        excepcion.Message.Should().Contain("convertir unidad");
        excepcion.Message.Should().Contain("kilogramos");
        excepcion.Message.Should().Contain("litros");
        excepcion.Message.Should().Contain("unidades incompatibles");
        excepcion.AdditionalData.Should().ContainKey("UnidadActual");
        excepcion.AdditionalData.Should().ContainKey("UnidadSolicitada");
        excepcion.AdditionalData.Should().ContainKey("TipoProblema");
        excepcion.AdditionalData["UnidadActual"].Should().Be(unidadActual);
        excepcion.AdditionalData["UnidadSolicitada"].Should().Be(unidadSolicitada);
        excepcion.AdditionalData["TipoProblema"].Should().Be("UnidadIncompatible");
    }

    [Fact]
    public void ParaFueraDeTemporada_ConTemporadas_DeberiaCrearExcepcionConTemporadas()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 7.5m;
        var temporadaActual = "Invierno";
        var temporadaRequerida = "Verano";
        var operacion = "usar en menú";

        // Act
        var excepcion = IngredienteInvalidoException.ParaFueraDeTemporada(
            ingredienteId, stockActual, temporadaActual, temporadaRequerida, operacion);

        // Assert
        excepcion.IngredienteId.Should().Be(ingredienteId);
        excepcion.Message.Should().Contain("usar en menú");
        excepcion.Message.Should().Contain("fuera de temporada");
        excepcion.Message.Should().Contain("Verano");
        excepcion.Message.Should().Contain("Invierno");
        excepcion.AdditionalData.Should().ContainKey("TemporadaActual");
        excepcion.AdditionalData.Should().ContainKey("TemporadaRequerida");
        excepcion.AdditionalData.Should().ContainKey("TipoProblema");
        excepcion.AdditionalData["TemporadaActual"].Should().Be(temporadaActual);
        excepcion.AdditionalData["TemporadaRequerida"].Should().Be(temporadaRequerida);
        excepcion.AdditionalData["TipoProblema"].Should().Be("FueraDeTemporada");
    }

    [Fact]
    public void ParaStockCritico_ConNiveles_DeberiaCrearExcepcionConDeficitCritico()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 2.0m;
        var stockMinimoCritico = 5.0m;
        var operacion = "verificar disponibilidad";

        // Act
        var excepcion = IngredienteInvalidoException.ParaStockCritico(
            ingredienteId, stockActual, stockMinimoCritico, operacion);

        // Assert
        excepcion.IngredienteId.Should().Be(ingredienteId);
        excepcion.Message.Should().Contain("verificar disponibilidad");
        excepcion.Message.Should().Contain("nivel crítico");
        excepcion.Message.Should().Contain("2");
        excepcion.Message.Should().Contain("mínimo crítico: 5");
        excepcion.AdditionalData.Should().ContainKey("StockMinimoCritico");
        excepcion.AdditionalData.Should().ContainKey("DeficitCritico");
        excepcion.AdditionalData.Should().ContainKey("TipoProblema");
        excepcion.AdditionalData["StockMinimoCritico"].Should().Be(stockMinimoCritico);
        excepcion.AdditionalData["DeficitCritico"].Should().Be(3.0m);
        excepcion.AdditionalData["TipoProblema"].Should().Be("StockCritico");
    }

    [Fact]
    public void ParaCalidadInaceptable_ConCalidades_DeberiaCrearExcepcionConCalidades()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 12.0m;
        var calidadActual = "Regular";
        var calidadMinima = "Buena";
        var operacion = "usar en plato especial";

        // Act
        var excepcion = IngredienteInvalidoException.ParaCalidadInaceptable(
            ingredienteId, stockActual, calidadActual, calidadMinima, operacion);

        // Assert
        excepcion.IngredienteId.Should().Be(ingredienteId);
        excepcion.Message.Should().Contain("usar en plato especial");
        excepcion.Message.Should().Contain("Calidad inaceptable");
        excepcion.Message.Should().Contain("Regular");
        excepcion.Message.Should().Contain("Buena");
        excepcion.AdditionalData.Should().ContainKey("CalidadActual");
        excepcion.AdditionalData.Should().ContainKey("CalidadMinima");
        excepcion.AdditionalData.Should().ContainKey("TipoProblema");
        excepcion.AdditionalData["CalidadActual"].Should().Be(calidadActual);
        excepcion.AdditionalData["CalidadMinima"].Should().Be(calidadMinima);
        excepcion.AdditionalData["TipoProblema"].Should().Be("CalidadInaceptable");
    }

    [Fact]
    public void ToResult_DeberiaRetornarResultConError()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var excepcion = IngredienteInvalidoException.ParaStockInsuficiente(
            ingredienteId, 1.0m, 5.0m);

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
        var ingredienteId = Guid.NewGuid();
        var excepcion = IngredienteInvalidoException.ParaIngredienteVencido(
            ingredienteId, 3.0m, DateTime.Now.AddDays(-2), "usar");

        // Act
        var mensaje = excepcion.GetDetailedMessage();

        // Assert
        mensaje.Should().Contain("BUSINESS_RULE_VIOLATION");
        mensaje.Should().Contain("Inventario");
        mensaje.Should().Contain("usar");
        mensaje.Should().Contain("StockActual");
        mensaje.Should().Contain("FechaVencimiento");
        mensaje.Should().Contain("DiasVencido");
    }

    [Theory]
    [InlineData(0.0, "sin stock")]
    [InlineData(1.5, "poco stock")]
    [InlineData(100.0, "stock alto")]
    public void Constructor_ConDiferentesStocks_DeberiaCrearExcepcionCorrecta(
        decimal stock, string descripcion)
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var operacion = "test";
        var razon = $"Ingrediente con {descripcion}";

        // Act
        var excepcion = new IngredienteInvalidoException(ingredienteId, stock, operacion, razon);

        // Assert
        excepcion.StockActual.Should().Be(stock);
        excepcion.Message.Should().Contain($"Stock: {stock}");
    }

    [Fact]
    public void ParaProveedorSinStock_SinOperacionEspecifica_DeberiaUsarOperacionPorDefecto()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 0.0m;
        var proveedorId = Guid.NewGuid();

        // Act
        var excepcion = IngredienteInvalidoException.ParaProveedorSinStock(
            ingredienteId, stockActual, proveedorId);

        // Assert
        excepcion.Message.Should().Contain("reabastecer");
    }

    [Fact]
    public void ParaStockCritico_SinOperacionEspecifica_DeberiaUsarOperacionPorDefecto()
    {
        // Arrange
        var ingredienteId = Guid.NewGuid();
        var stockActual = 1.0m;
        var stockMinimoCritico = 10.0m;

        // Act
        var excepcion = IngredienteInvalidoException.ParaStockCritico(
            ingredienteId, stockActual, stockMinimoCritico);

        // Assert
        excepcion.Message.Should().Contain("usar");
    }
} 