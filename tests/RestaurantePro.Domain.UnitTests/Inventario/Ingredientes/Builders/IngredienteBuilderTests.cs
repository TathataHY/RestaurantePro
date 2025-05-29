using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Inventario.Ingredientes.Builders;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Domain.UnitTests.Inventario.Ingredientes.Builders;

public class IngredienteBuilderTests
{
    private readonly Mock<INotificationManager> _notificationManagerMock;
    private readonly Mock<ILogger<IngredienteBuilder>> _loggerMock;
    private readonly INotificationManager _notificationManager;
    private readonly IngredienteBuilder _builder;

    public IngredienteBuilderTests()
    {
        _notificationManagerMock = new Mock<INotificationManager>();
        _loggerMock = new Mock<ILogger<IngredienteBuilder>>();
        _notificationManager = new NotificationManager();
        
        // Setup mock behaviors
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(false);
        _notificationManagerMock.Setup(x => x.GetErrors()).Returns(new List<Error>().AsReadOnly());
        
        _builder = new IngredienteBuilder(_notificationManagerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_ConParametrosValidos_DebeCrearBuilder()
    {
        // Act & Assert
        Assert.NotNull(_builder);
    }

    [Fact]
    public void Constructor_ConNotificationManagerNulo_DebeLanzarExcepcion()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new IngredienteBuilder(null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_ConLoggerNulo_DebeLanzarExcepcion()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new IngredienteBuilder(_notificationManagerMock.Object, null!));
    }

    [Fact]
    public void ConNombre_ConNombreValido_DebeAsignarNombre()
    {
        // Arrange
        const string nombre = "Tomate";

        // Act
        var resultado = _builder.ConNombre(nombre);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConNombre_ConNombreVacio_DebeRegistrarError()
    {
        // Act
        _builder.ConNombre("");

        // Assert
        _notificationManagerMock.Verify(x => x.AddError("El nombre del ingrediente es obligatorio", "nombre", null), Times.AtLeastOnce);
    }

    [Fact]
    public void ConNombre_ConNombreMuyLargo_DebeRegistrarError()
    {
        // Arrange
        var nombreLargo = new string('a', 201);

        // Act
        _builder.ConNombre(nombreLargo);

        // Assert
        _notificationManagerMock.Verify(x => x.AddError("El nombre no puede exceder 200 caracteres", "nombre", null), Times.AtLeastOnce);
    }

    [Fact]
    public void ConCodigo_ConCodigoValido_DebeAsignarCodigo()
    {
        // Arrange
        const string codigo = "TOM001";

        // Act
        var resultado = _builder.ConCodigo(codigo);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConCodigo_ConCodigoVacio_DebeRegistrarError()
    {
        // Act
        _builder.ConCodigo("");

        // Assert
        _notificationManagerMock.Verify(x => x.AddError("El código del ingrediente es obligatorio", "codigo", null), Times.AtLeastOnce);
    }

    [Fact]
    public void ConCodigo_ConCodigoMuyLargo_DebeRegistrarError()
    {
        // Arrange
        var codigoLargo = new string('a', 51);

        // Act
        _builder.ConCodigo(codigoLargo);

        // Assert
        _notificationManagerMock.Verify(x => x.AddError("El código no puede exceder 50 caracteres", "codigo", null), Times.AtLeastOnce);
    }

    [Fact]
    public void ConDescripcion_ConDescripcionValida_DebeAsignarDescripcion()
    {
        // Arrange
        const string descripcion = "Tomate rojo fresco";

        // Act
        var resultado = _builder.ConDescripcion(descripcion);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConDescripcion_ConDescripcionVacia_DebeRegistrarError()
    {
        // Act
        _builder.ConDescripcion("");

        // Assert
        _notificationManagerMock.Verify(x => x.AddError("La descripción del ingrediente es obligatoria", "descripcion", null), Times.AtLeastOnce);
    }

    [Fact]
    public void ConDescripcion_ConDescripcionMuyLarga_DebeRegistrarError()
    {
        // Arrange
        var descripcionLarga = new string('a', 501);

        // Act
        _builder.ConDescripcion(descripcionLarga);

        // Assert
        _notificationManagerMock.Verify(x => x.AddError("La descripción no puede exceder 500 caracteres", "descripcion", null), Times.AtLeastOnce);
    }

    [Fact]
    public void ConUnidadMedida_ConUnidadValida_DebeAsignarUnidad()
    {
        // Act
        var resultado = _builder.ConUnidadMedida(UnidadMedida.Kilogramo);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConStockMinimo_ConStockValido_DebeAsignarStock()
    {
        // Arrange
        const decimal stock = 10.5m;

        // Act
        var resultado = _builder.ConStockMinimo(stock);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConStockMinimo_ConStockNegativo_DebeRegistrarError()
    {
        // Act
        _builder.ConStockMinimo(-1m);

        // Assert
        _notificationManagerMock.Verify(x => x.AddError("El stock mínimo no puede ser negativo", "stockMinimo", null), Times.AtLeastOnce);
    }

    [Fact]
    public void ConStockActual_ConStockValido_DebeAsignarStock()
    {
        // Arrange
        const decimal stock = 25.0m;

        // Act
        var resultado = _builder.ConStockActual(stock);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConStockActual_ConStockNegativo_DebeRegistrarError()
    {
        // Act
        _builder.ConStockActual(-5m);

        // Assert
        _notificationManagerMock.Verify(x => x.AddError("El stock actual no puede ser negativo", "stockActual", null), Times.AtLeastOnce);
    }

    [Fact]
    public void ConProveedorPrincipal_ConProveedorValido_DebeAsignarProveedor()
    {
        // Arrange
        var proveedorId = Guid.NewGuid();

        // Act
        var resultado = _builder.ConProveedorPrincipal(proveedorId);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConProveedorPrincipal_ConProveedorVacio_DebeRegistrarError()
    {
        // Act
        _builder.ConProveedorPrincipal(Guid.Empty);

        // Assert
        _notificationManagerMock.Verify(x => x.AddError("El ID del proveedor principal no puede estar vacío", "proveedorId", null), Times.AtLeastOnce);
    }

    [Fact]
    public void ConRotacion_ConRotacionValida_DebeAsignarRotacion()
    {
        // Act
        var resultado = _builder.ConRotacion(RotacionIngrediente.Alta);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConTemporada_ConTemporadaValida_DebeAsignarTemporada()
    {
        // Act
        var resultado = _builder.ConTemporada(TemporadaIngrediente.Verano);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConCostoPromedio_ConCostoValido_DebeAsignarCosto()
    {
        // Arrange
        const decimal costo = 15.50m;

        // Act
        var resultado = _builder.ConCostoPromedio(costo);

        // Assert
        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void ConCostoPromedio_ConCostoNegativo_DebeRegistrarError()
    {
        // Act
        _builder.ConCostoPromedio(-10m);

        // Assert
        _notificationManagerMock.Verify(x => x.AddError("El costo promedio no puede ser negativo", "costoPromedio", null), Times.AtLeastOnce);
    }

    [Fact]
    public void Construir_ConDatosCompletos_DebeCrearIngrediente()
    {
        // Arrange
        var builderReal = new IngredienteBuilder(_notificationManager, _loggerMock.Object);

        // Act
        var resultado = builderReal
            .ConNombre("Tomate")
            .ConCodigo("TOM001")
            .ConDescripcion("Tomate rojo fresco")
            .ConUnidadMedida(UnidadMedida.Kilogramo)
            .ConStockMinimo(10m)
            .ConStockActual(25m)
            .ConRotacion(RotacionIngrediente.Alta)
            .ConTemporada(TemporadaIngrediente.Verano)
            .Construir();

        // Assert
        Assert.True(resultado.Succeeded);
        Assert.NotNull(resultado.Value);
        Assert.Equal("Tomate", resultado.Value.Nombre);
        Assert.Equal("TOM001", resultado.Value.Codigo);
    }

    [Fact]
    public void Construir_SinNombre_DeberiaRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = _builder
            .ConCodigo("TOM001")
            .ConDescripcion("Tomate rojo")
            .ConUnidadMedida(UnidadMedida.Kilogramo)
            .ConStockMinimo(10m)
            .ConStockActual(25m)
            .Construir();

        // Assert
        Assert.False(resultado.Succeeded);
        Assert.Equal("Errores de validación en la construcción del ingrediente", resultado.Error);
    }

    [Fact]
    public void Construir_SinCodigo_DeberiaRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = _builder
            .ConNombre("Tomate")
            .ConDescripcion("Tomate rojo")
            .ConUnidadMedida(UnidadMedida.Kilogramo)
            .ConStockMinimo(10m)
            .ConStockActual(25m)
            .Construir();

        // Assert
        Assert.False(resultado.Succeeded);
    }

    [Fact]
    public void Construir_SinUnidadMedida_DeberiaRetornarFallo()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = _builder
            .ConNombre("Tomate")
            .ConCodigo("TOM001")
            .ConDescripcion("Tomate rojo")
            .ConStockMinimo(10m)
            .ConStockActual(25m)
            .Construir();

        // Assert
        Assert.False(resultado.Succeeded);
    }

    [Fact]
    public void InterfazFluida_DebePermitirEncadenamientoCompleto()
    {
        // Act & Assert - No debe lanzar excepciones
        var resultado = _builder
            .ConNombre("Tomate")
            .ConCodigo("TOM001")
            .ConDescripcion("Tomate rojo fresco")
            .ConUnidadMedida(UnidadMedida.Kilogramo)
            .ConStockMinimo(10m)
            .ConStockActual(25m)
            .ConProveedorPrincipal(Guid.NewGuid())
            .ConRotacion(RotacionIngrediente.Alta)
            .ConTemporada(TemporadaIngrediente.Verano)
            .ConCostoPromedio(15.50m);

        Assert.Same(_builder, resultado);
    }

    [Fact]
    public void Builder_ConMultiplesErrores_DeberiaAcumularTodos()
    {
        // Arrange
        _notificationManagerMock.Setup(x => x.HasErrors).Returns(true);

        // Act
        var resultado = _builder
            .ConNombre("")
            .ConCodigo("")
            .ConDescripcion("")
            .ConStockMinimo(-1m)
            .ConStockActual(-5m)
            .ConProveedorPrincipal(Guid.Empty)
            .ConCostoPromedio(-10m)
            .Construir();

        // Assert
        Assert.False(resultado.Succeeded);
    }

    [Fact]
    public void Reset_DebeLimpiarTodosLosCampos()
    {
        // Arrange
        _builder
            .ConNombre("Tomate")
            .ConCodigo("TOM001")
            .ConDescripcion("Tomate rojo");

        // Act
        var resultado = _builder.Reset();

        // Assert
        Assert.Same(_builder, resultado);
        _notificationManagerMock.Verify(x => x.ClearErrors(), Times.Once);
    }

    [Fact]
    public void Nuevo_DebeCrearBuilderConfigurado()
    {
        // Act
        var builder = IngredienteBuilder.Nuevo(_notificationManagerMock.Object, _loggerMock.Object);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<IngredienteBuilder>(builder);
    }

    [Fact]
    public void Construir_ConProveedorYCosto_DebeAsignarPropiedadesOpcionales()
    {
        // Arrange
        var builderReal = new IngredienteBuilder(_notificationManager, _loggerMock.Object);
        var proveedorId = Guid.NewGuid();

        // Act
        var resultado = builderReal
            .ConNombre("Tomate")
            .ConCodigo("TOM001")
            .ConDescripcion("Tomate rojo fresco")
            .ConUnidadMedida(UnidadMedida.Kilogramo)
            .ConStockMinimo(10m)
            .ConStockActual(25m)
            .ConProveedorPrincipal(proveedorId)
            .ConCostoPromedio(15.50m)
            .Construir();

        // Assert
        Assert.True(resultado.Succeeded);
        Assert.Equal(proveedorId, resultado.Value.ProveedorPrincipalId);
        Assert.Equal(15.50m, resultado.Value.CostoPromedio);
    }
} 