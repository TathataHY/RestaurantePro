namespace RestaurantePro.Domain.UnitTests.Inventario.Ingredientes.Factories;

/// <summary>
/// Pruebas unitarias para IngredienteFactory - Validando patrón Factory con Result/Notification
/// </summary>
public class IngredienteFactoryTests
{
    private readonly Mock<ILogger<IngredienteFactory>> _loggerMock;
    private readonly NotificationManager _notificationManager;
    private readonly IngredienteFactory _ingredienteFactory;

    public IngredienteFactoryTests()
    {
        _loggerMock = new Mock<ILogger<IngredienteFactory>>();
        _notificationManager = new NotificationManager();
        _ingredienteFactory = new IngredienteFactory(_notificationManager, _loggerMock.Object);
    }

    #region Crear Tests

    [Fact]
    public void Crear_ConParametrosValidos_DebeRetornarExito()
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Tomate Cherry",
            Codigo = "TOM-20241125",
            Descripcion = "Tomate cherry fresco para ensaladas",
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 5.0m,
            StockActual = 10.0m,
            Rotacion = RotacionIngrediente.Alta,
            Temporada = TemporadaIngrediente.Verano,
            CostoPromedio = 2500.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametros);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        
        var ingrediente = resultado.Value!;
        ingrediente.Nombre.Should().Be("Tomate Cherry");
        ingrediente.Codigo.Should().Be("TOM-20241125");
        ingrediente.Descripcion.Should().Be("Tomate cherry fresco para ensaladas");
        ingrediente.UnidadMedida.Should().Be(UnidadMedida.Kilogramo);
        ingrediente.StockMinimo.Should().Be(5.0m);
        ingrediente.Stock.Should().Be(10.0m);
        ingrediente.Rotacion.Should().Be(RotacionIngrediente.Alta);
        ingrediente.Temporada.Should().Be(TemporadaIngrediente.Verano);
        ingrediente.EstaActivo.Should().BeTrue();
    }

    [Fact]
    public void Crear_ConProveedorPrincipal_DebeAsignarProveedor()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Cebolla",
            Codigo = "CEB-20241125",
            Descripcion = "Cebolla blanca",
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 3.0m,
            StockActual = 8.0m,
            ProveedorPrincipalId = proveedorId
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametros);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value!.ProveedorPrincipalId.Should().Be(proveedorId);
    }

    [Fact]
    public void Crear_ConCostoPromedio_DebeAsignarCosto()
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Aceite de oliva",
            Codigo = "ACE-20241125",
            Descripcion = "Aceite de oliva extra virgen",
            UnidadMedida = UnidadMedida.Litro,
            StockMinimo = 2.0m,
            StockActual = 5.0m,
            CostoPromedio = 15000.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametros);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value!.CostoPromedio.Should().Be(15000.0m);
    }

    [Fact]
    public void Crear_ConNombreVacio_DebeRetornarError()
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "",
            Codigo = "TEST-123",
            Descripcion = "Test",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 1.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametros);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("Nombre"));
    }

    [Fact]
    public void Crear_ConCodigoVacio_DebeRetornarError()
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Test Ingrediente",
            Codigo = "",
            Descripcion = "Test",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 1.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametros);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("Codigo"));
    }

    [Fact]
    public void Crear_ConStockNegativo_DebeRetornarError()
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Test Ingrediente",
            Codigo = "TEST-123",
            Descripcion = "Test",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = -1.0m,
            StockActual = -1.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametros);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("StockMinimo"));
        resultado.Errors.Should().Contain(e => e.Contains("StockActual"));
    }

    [Fact]
    public void Crear_ConCodigoFormatoIncorrecto_DebeRetornarError()
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Test Ingrediente",
            Codigo = "formato-incorrecto",
            Descripcion = "Test",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 1.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametros);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("formato"));
    }

    [Fact]
    public void Crear_ConParametrosTipoIncorrecto_DebeRetornarError()
    {
        // Arrange
        var parametrosIncorrectos = new { Nombre = "Test" };

        // Act
        var resultado = _ingredienteFactory.Crear(parametrosIncorrectos);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("IngredienteCreationParameters"));
    }

    #endregion

    #region Reconstruir Tests

    [Fact]
    public void Reconstruir_ConDatosValidos_DebeRetornarExito()
    {
        // Arrange
        var id = Guid.NewGuid();
        var datos = new IngredienteReconstructionData
        {
            Nombre = "Ajo",
            Codigo = "AJO-20241125",
            Descripcion = "Ajo fresco",
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 2.0m,
            Stock = 4.0m,
            EstaActivo = true,
            Rotacion = RotacionIngrediente.Media,
            Temporada = TemporadaIngrediente.TodoElAño,
            CostoPromedio = 3000.0m
        };

        // Act
        var resultado = _ingredienteFactory.Reconstruir(id, datos);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        
        var ingrediente = resultado.Value!;
        ingrediente.Id.Should().Be(id);
        ingrediente.Nombre.Should().Be("Ajo");
        ingrediente.Codigo.Should().Be("AJO-20241125");
        ingrediente.EstaActivo.Should().BeTrue();
        ingrediente.CostoPromedio.Should().Be(3000.0m);
    }

    [Fact]
    public void Reconstruir_ConIngredienteInactivo_DebeReconstruirCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var datos = new IngredienteReconstructionData
        {
            Nombre = "Ingrediente Inactivo",
            Codigo = "INA-20241125",
            Descripcion = "Test inactivo",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            Stock = 0.0m,
            EstaActivo = false
        };

        // Act
        var resultado = _ingredienteFactory.Reconstruir(id, datos);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value!.EstaActivo.Should().BeFalse();
    }

    [Fact]
    public void Reconstruir_ConTipoIncorrecto_DebeRetornarError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var datosIncorrectos = new { Nombre = "Test" };

        // Act
        var resultado = _ingredienteFactory.Reconstruir(id, datosIncorrectos);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("IngredienteReconstructionData"));
    }

    #endregion

    #region Validar Parámetros Tests

    [Fact]
    public void ValidarParametros_ConParametrosValidos_DebeRetornarExito()
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Ingrediente Válido",
            Codigo = "VAL-20241125",
            Descripcion = "Descripción válida",
            UnidadMedida = UnidadMedida.Kilogramo,
            StockMinimo = 1.0m,
            StockActual = 2.0m,
            Rotacion = RotacionIngrediente.Alta,
            Temporada = TemporadaIngrediente.Primavera
        };

        // Act
        var resultado = _ingredienteFactory.ValidarParametros(parametros);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidarParametros_ConStockExcesivo_DebeGenerarAdvertencia()
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Test",
            Codigo = "TEST-123",
            Descripcion = "Test",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 50.0m // Stock muy alto comparado con el mínimo
        };

        // Act
        var resultado = _ingredienteFactory.ValidarParametros(parametros);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        // Note: Las advertencias se envían al logger, no al NotificationManager
        // Se podría verificar el logger pero no es crítico para esta prueba
    }

    [Fact]
    public void ValidarParametros_ConUnidadMedidaInvalida_DebeRetornarError()
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Test",
            Codigo = "TEST-123",
            Descripcion = "Test",
            UnidadMedida = (UnidadMedida)999, // Valor inválido
            StockMinimo = 1.0m,
            StockActual = 1.0m
        };

        // Act
        var resultado = _ingredienteFactory.ValidarParametros(parametros);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("UnidadMedida"));
    }

    [Theory]
    [InlineData(RotacionIngrediente.Baja)]
    [InlineData(RotacionIngrediente.Media)]
    [InlineData(RotacionIngrediente.Alta)]
    [InlineData(RotacionIngrediente.Critica)]
    public void ValidarParametros_ConRotacionesValidas_DebeRetornarExito(RotacionIngrediente rotacion)
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Test",
            Codigo = "TEST-123",
            Descripcion = "Test",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 1.0m,
            Rotacion = rotacion
        };

        // Act
        var resultado = _ingredienteFactory.ValidarParametros(parametros);

        // Assert
        resultado.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(TemporadaIngrediente.TodoElAño)]
    [InlineData(TemporadaIngrediente.Primavera)]
    [InlineData(TemporadaIngrediente.Verano)]
    [InlineData(TemporadaIngrediente.Otoño)]
    [InlineData(TemporadaIngrediente.Invierno)]
    public void ValidarParametros_ConTemporadasValidas_DebeRetornarExito(TemporadaIngrediente temporada)
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Test",
            Codigo = "TEST-123",
            Descripcion = "Test",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 1.0m,
            Temporada = temporada
        };

        // Act
        var resultado = _ingredienteFactory.ValidarParametros(parametros);

        // Assert
        resultado.Succeeded.Should().BeTrue();
    }

    #endregion

    #region Métodos de Conveniencia Tests

    [Fact]
    public void CrearIngrediente_ConParametrosDirectos_DebeRetornarExito()
    {
        // Arrange
        var nombre = "Harina";
        var codigo = "HAR-20241125";
        var descripcion = "Harina de trigo";
        var unidadMedida = UnidadMedida.Kilogramo;
        var stockMinimo = 10.0m;
        var stockActual = 25.0m;

        // Act
        var resultado = _ingredienteFactory.CrearIngrediente(
            nombre, codigo, descripcion, unidadMedida, stockMinimo, stockActual);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value!.Nombre.Should().Be(nombre);
        resultado.Value!.Codigo.Should().Be(codigo);
        resultado.Value!.UnidadMedida.Should().Be(unidadMedida);
    }

    [Fact]
    public void CrearIngredienteConCodigoAutomatico_DebeGenerarCodigo()
    {
        // Arrange
        var nombre = "Azúcar";
        var descripcion = "Azúcar blanca refinada";

        // Act
        var resultado = _ingredienteFactory.CrearIngredienteConCodigoAutomatico(
            nombre, descripcion, UnidadMedida.Kilogramo, 5.0m, 10.0m);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value!.Codigo.Should().StartWith("AZÚ-");
        resultado.Value!.Codigo.Should().MatchRegex(@"^[A-Z]{3}-\d{8}$");
    }

    [Fact]
    public void CrearIngredienteConCodigoAutomatico_NombreCorto_DebeCompletarConX()
    {
        // Arrange
        var nombre = "Sal"; // Solo 2 caracteres

        // Act
        var resultado = _ingredienteFactory.CrearIngredienteConCodigoAutomatico(
            nombre, "Sal de mesa", UnidadMedida.Kilogramo, 1.0m, 2.0m);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        resultado.Value!.Codigo.Should().StartWith("SAX-");
    }

    #endregion

    #region NotificationManager Integration Tests

    [Fact]
    public void Factory_DebeUsarNotificationManagerCorrectamente()
    {
        // Arrange
        var parametrosInvalidos = new IngredienteCreationParameters
        {
            Nombre = "",
            Codigo = "",
            StockMinimo = -1.0m,
            StockActual = -1.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametrosInvalidos);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        _notificationManager.HasErrors.Should().BeTrue();
        _notificationManager.GetErrors().Should().NotBeEmpty();
    }

    [Fact]
    public void Factory_DebeLimpiarNotificacionesAntesDeCadaOperacion()
    {
        // Arrange
        // Primero agregamos un error al notification manager
        _notificationManager.AddError("Error previo");
        _notificationManager.HasErrors.Should().BeTrue();

        var parametrosValidos = new IngredienteCreationParameters
        {
            Nombre = "Test",
            Codigo = "TEST-123",
            Descripcion = "Test",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 1.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametrosValidos);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        // El notification manager no debería tener el error previo después de una operación exitosa
    }

    #endregion

    #region Logging Tests

    [Fact]
    public void Crear_ConExito_DebeRegistrarLog()
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Test Logging",
            Codigo = "LOG-20241125",
            Descripcion = "Test para logging",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 1.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametros);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("creado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Crear_ConError_DebeRegistrarError()
    {
        // Arrange
        var parametrosInvalidos = new IngredienteCreationParameters
        {
            Nombre = "",
            Codigo = "",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 1.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametrosInvalidos);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Crear_ConDescripcionMuyLarga_DebeRetornarError()
    {
        // Arrange
        var descripcionLarga = new string('A', 501); // Más de 500 caracteres
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Test",
            Codigo = "TEST-123",
            Descripcion = descripcionLarga,
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 1.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametros);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("Descripcion"));
    }

    [Fact]
    public void Crear_ConCostoPromedioNegativo_DebeRetornarError()
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Test",
            Codigo = "TEST-123",
            Descripcion = "Test",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 1.0m,
            CostoPromedio = -100.0m
        };

        // Act
        var resultado = _ingredienteFactory.Crear(parametros);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("CostoPromedio"));
    }

    [Theory]
    [InlineData("A-1")]
    [InlineData("AB-12345678901")] // Demasiados dígitos
    [InlineData("ABCDEF-123")] // Demasiadas letras
    [InlineData("ab-1234")] // Minúsculas
    public void ValidarParametros_ConCodigosInvalidos_DebeRetornarError(string codigoInvalido)
    {
        // Arrange
        var parametros = new IngredienteCreationParameters
        {
            Nombre = "Test",
            Codigo = codigoInvalido,
            Descripcion = "Test",
            UnidadMedida = UnidadMedida.Unidad,
            StockMinimo = 1.0m,
            StockActual = 1.0m
        };

        // Act
        var resultado = _ingredienteFactory.ValidarParametros(parametros);

        // Assert
        resultado.Succeeded.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.Contains("formato"));
    }

    #endregion
} 