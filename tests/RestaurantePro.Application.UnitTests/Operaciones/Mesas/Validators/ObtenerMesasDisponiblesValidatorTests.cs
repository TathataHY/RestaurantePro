using FluentAssertions;
using FluentValidation.TestHelper;
using RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesasDisponibles;

namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Validators;

/// <summary>
/// Tests para ObtenerMesasDisponiblesValidator
/// </summary>
public class ObtenerMesasDisponiblesValidatorTests
{
    private readonly ObtenerMesasDisponiblesValidator _validator;

    public ObtenerMesasDisponiblesValidatorTests()
    {
        _validator = new ObtenerMesasDisponiblesValidator();
    }

    #region Helper Methods

    private ObtenerMesasDisponiblesQuery CrearQueryValida()
    {
        return new ObtenerMesasDisponiblesQuery
        {
            CapacidadMinima = 4,
            Zona = "Interior",
            SoloActivas = true,
            Pagina = 1,
            TamanoPagina = 20,
            OrdenarPorNumero = true
        };
    }

    #endregion

    #region Validación CapacidadMinima

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task Validate_ConCapacidadMinimaMenorIgualCero_DeberiaRetornarError(int capacidadInvalida)
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMinima = capacidadInvalida;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.CapacidadMinima) &&
            e.ErrorMessage.Contains("La capacidad mínima debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConCapacidadMinimaExcesiva_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMinima = 51; // Más de 50 personas

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.CapacidadMinima) &&
            e.ErrorMessage.Contains("La capacidad mínima no puede exceder 50 personas"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task Validate_ConCapacidadMinimaValida_NoDeberiaRetornarErrorDeCapacidad(int capacidadValida)
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMinima = capacidadValida;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.CapacidadMinima));
    }

    [Fact]
    public async Task Validate_ConCapacidadMinimaNula_NoDeberiaValidar()
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMinima = null;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.CapacidadMinima));
    }

    #endregion

    #region Validación Zona

    [Fact]
    public async Task Validate_ConZonaMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Zona = new string('A', 101); // Más de 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.Zona) &&
            e.ErrorMessage.Contains("La zona no puede exceder 100 caracteres"));
    }

    [Theory]
    [InlineData("Interior")]
    [InlineData("Terraza")]
    [InlineData("VIP")]
    [InlineData("Bar")]
    [InlineData("Salón Principal")]
    [InlineData("Área Familiar")]
    public async Task Validate_ConZonaValida_NoDeberiaRetornarErrorDeZona(string zonaValida)
    {
        // Arrange
        var query = CrearQueryValida();
        query.Zona = zonaValida;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.Zona));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConZonaVacia_NoDeberiaValidarZona(string zonaVacia)
    {
        // Arrange
        var query = CrearQueryValida();
        query.Zona = zonaVacia;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.Zona));
    }

    #endregion

    #region Validación Paginación

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task Validate_ConPaginaMenorIgualCero_DeberiaRetornarError(int paginaInvalida)
    {
        // Arrange
        var query = CrearQueryValida();
        query.Pagina = paginaInvalida;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.Pagina) &&
            e.ErrorMessage.Contains("La página debe ser mayor a 0"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task Validate_ConTamanoPaginaMenorIgualCero_DeberiaRetornarError(int tamanoInvalido)
    {
        // Arrange
        var query = CrearQueryValida();
        query.TamanoPagina = tamanoInvalido;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.TamanoPagina) &&
            e.ErrorMessage.Contains("El tamaño de página debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConTamanoPaginaExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.TamanoPagina = 101; // Más de 100

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.TamanoPagina) &&
            e.ErrorMessage.Contains("El tamaño de página no puede exceder 100"));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 10)]
    [InlineData(2, 20)]
    [InlineData(5, 50)]
    [InlineData(10, 100)]
    public async Task Validate_ConPaginacionValida_NoDeberiaRetornarErrorDePaginacion(int pagina, int tamanoPagina)
    {
        // Arrange
        var query = CrearQueryValida();
        query.Pagina = pagina;
        query.TamanoPagina = tamanoPagina;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.Pagina) ||
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.TamanoPagina));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConQueryCompleta_DeberiaSerValida()
    {
        // Arrange
        var query = new ObtenerMesasDisponiblesQuery
        {
            CapacidadMinima = 6,
            Zona = "VIP",
            SoloActivas = true,
            Pagina = 2,
            TamanoPagina = 15,
            OrdenarPorNumero = true
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConQueryMinima_DeberiaSerValida()
    {
        // Arrange
        var query = new ObtenerMesasDisponiblesQuery
        {
            Pagina = 1,
            TamanoPagina = 20
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConMultiplesErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var query = new ObtenerMesasDisponiblesQuery
        {
            CapacidadMinima = -1, // Error
            Zona = new string('A', 101), // Error - muy largo
            Pagina = 0, // Error
            TamanoPagina = 101 // Error - excesivo
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Interior", 2)]
    [InlineData("Terraza", 4)]
    [InlineData("VIP", 8)]
    [InlineData("Bar", 1)]
    public async Task Validate_ConDiferentesEscenarios_DeberiaSerValido(string zona, int capacidadMinima)
    {
        // Arrange
        var query = new ObtenerMesasDisponiblesQuery
        {
            Zona = zona,
            CapacidadMinima = capacidadMinima,
            SoloActivas = true,
            Pagina = 1,
            TamanoPagina = 20,
            OrdenarPorNumero = true
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConBusquedaParaEventoEspecial_DeberiaSerValido()
    {
        // Arrange
        var query = new ObtenerMesasDisponiblesQuery
        {
            CapacidadMinima = 20,
            Zona = "Salón de Eventos",
            SoloActivas = true,
            Pagina = 1,
            TamanoPagina = 50,
            OrdenarPorNumero = false
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConBusquedaRapida_DeberiaSerValida()
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.Basica();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConBusquedaPorCapacidad_DeberiaSerValida()
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.ConCapacidad(6);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConBusquedaPorZona_DeberiaSerValida()
    {
        // Arrange
        var query = ObtenerMesasDisponiblesQuery.PorZona("Terraza");

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Fact]
    public async Task Validate_ConCapacidadEnLimiteMaximo_DeberiaSerValida()
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMinima = 50; // Límite máximo

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConZonaEnLimiteMaximo_DeberiaSerValida()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Zona = new string('A', 100); // Exactamente 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConTamanoPaginaEnLimiteMaximo_DeberiaSerValida()
    {
        // Arrange
        var query = CrearQueryValida();
        query.TamanoPagina = 100; // Límite máximo

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConSoloActivasFalse_DeberiaSerValida()
    {
        // Arrange
        var query = CrearQueryValida();
        query.SoloActivas = false;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConOrdenarPorNumeroFalse_DeberiaSerValida()
    {
        // Arrange
        var query = CrearQueryValida();
        query.OrdenarPorNumero = false;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
} 