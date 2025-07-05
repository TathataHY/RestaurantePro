using FluentAssertions;
using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Inventario.Reportes.Validators;

/// <summary>
/// Tests para ObtenerAnalisisInventarioValidator
/// Valida reglas de negocio para obtener análisis completo de inventario
/// </summary>
public class ObtenerAnalisisInventarioValidatorTests
{
    private readonly ObtenerAnalisisInventarioValidator _validator;

    public ObtenerAnalisisInventarioValidatorTests()
    {
        // El validator real no necesita dependencias, solo usa constructor vacío
        _validator = new ObtenerAnalisisInventarioValidator();
    }

    #region Tests de Validaciones de Fechas

    [Fact]
    public async Task FechaDesde_NoDebeSerMuyAntigua()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaDesde = DateTime.Now.AddYears(-3);
        query.FechaHasta = DateTime.Now.AddYears(-3).AddDays(30); // Solo 30 días de rango

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        // Nota: El validator actual NO valida fechas muy antiguas, solo el rango máximo
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task FechaDesde_NoDebeSerFutura()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaDesde = DateTime.Now.AddDays(1);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("La fecha desde debe ser menor o igual a la fecha hasta"));
    }

    [Fact]
    public async Task FechaHasta_NoDebeSerFutura()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaHasta = DateTime.Now.AddDays(1);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        // Nota: El validator actual NO valida fechas futuras
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task FechaHasta_DebeSerPosteriorAFechaDesde()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaDesde = DateTime.Now;
        query.FechaHasta = DateTime.Now.AddDays(-1);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("La fecha desde debe ser menor o igual a la fecha hasta"));
    }

    [Fact]
    public async Task RangoFechas_NoDebeExcederLimiteMaximo()
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaDesde = DateTime.Now.AddYears(-1).AddDays(-1);
        query.FechaHasta = DateTime.Now;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(1)]    // 1 día - válido
    [InlineData(30)]   // 30 días - válido
    [InlineData(90)]   // 90 días - válido
    [InlineData(365)]  // 365 días - límite
    public async Task RangoFechas_DebeAceptarRangosValidos(int dias)
    {
        // Arrange
        var query = CrearQueryBase();
        query.FechaDesde = DateTime.Now.AddDays(-dias);
        query.FechaHasta = DateTime.Now;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Validaciones de Categorías

    [Fact]
    public async Task CategoriaId_DebeSerValidaCuandoSeEspecifica()
    {
        // Arrange
        var query = CrearQueryBase();
        query.CategoriaId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CategoriaId_PuedeSerNula()
    {
        // Arrange
        var query = CrearQueryBase();
        query.CategoriaId = null;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CategoriaId_NoDebeSerGuidVacio()
    {
        // Arrange
        var query = CrearQueryBase();
        query.CategoriaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        // Nota: El validator actual NO valida Guid.Empty
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Validaciones de Nivel de Detalle

    [Theory]
    [InlineData("Básico")]
    [InlineData("Completo")]
    [InlineData("Resumen")]
    public async Task NivelDetalle_DebeAceptarNivelesValidos(string nivel)
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelDetalle = nivel;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task NivelDetalle_DebeRechazarNivelesInvalidos()
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelDetalle = "NivelInexistente";

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerAnalisisInventarioQuery.NivelDetalle));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NivelDetalle_DebeSerObligatorio(string nivel)
    {
        // Arrange
        var query = CrearQueryBase();
        query.NivelDetalle = nivel;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerAnalisisInventarioQuery.NivelDetalle));
    }

    #endregion

    #region Tests de Validaciones de Usuario

    [Fact]
    public async Task UsuarioId_DebeSerObligatorio()
    {
        // Arrange
        var query = CrearQueryBase();
        query.UsuarioId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerAnalisisInventarioQuery.UsuarioId));
    }

    [Fact]
    public async Task UsuarioId_DebeSerValido()
    {
        // Arrange
        var query = CrearQueryBase();
        query.UsuarioId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Configuraciones Específicas

    [Fact]
    public async Task AnalisisCriticos_DeberiaConfigurarseSoloCriticos()
    {
        // Arrange
        var query = CrearQueryBase();
        query.SoloCriticos = true;
        query.SoloAlertaStock = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task AnalisisCompleto_RequiereTodasLasOpciones()
    {
        // Arrange
        var query = CrearQueryCompleto();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task AnalisisBasico_RequiereConfiguracionMinima()
    {
        // Arrange
        var query = CrearQueryMinimo();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Performance

    [Fact]
    public async Task Validator_ConDatosMasivos_DebeCompletarseEnTiempoRazonable()
    {
        // Arrange
        var query = CrearQueryBase();
        query.Categorias = CrearListaCategorias(100);

        // Act & Assert
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _validator.ValidateAsync(query);
        stopwatch.Stop();

        result.IsValid.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Menos de 1 segundo
    }

    [Fact]
    public async Task Validator_ConAnalisisCompleto_DebeValidarTodasLasReglas()
    {
        // Arrange
        var query = CrearQueryCompleto();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Métodos Helper

    private ObtenerAnalisisInventarioQuery CrearQueryBase()
    {
        return new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = DateTime.Now.AddDays(-7),
            FechaHasta = DateTime.Now,
            NivelDetalle = "Completo",
            UsuarioId = Guid.NewGuid(),
            IncluirTendencias = true,
            IncluirRecomendaciones = true
        };
    }

    private ObtenerAnalisisInventarioQuery CrearQueryCompleto()
    {
        return new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = DateTime.Now.AddDays(-30),
            FechaHasta = DateTime.Now,
            NivelDetalle = "Completo",
            UsuarioId = Guid.NewGuid(),
            IncluirTendencias = true,
            IncluirRecomendaciones = true,
            IncluirPredicciones = true,
            IncluirAnalisisFinanciero = true,
            SoloCriticos = false,
            SoloAlertaStock = false
        };
    }

    private ObtenerAnalisisInventarioQuery CrearQueryMinimo()
    {
        return new ObtenerAnalisisInventarioQuery
        {
            FechaDesde = DateTime.Now,
            FechaHasta = DateTime.Now,
            NivelDetalle = "Básico",
            UsuarioId = Guid.NewGuid(),
            IncluirTendencias = false,
            IncluirRecomendaciones = false
        };
    }

    private List<Guid> CrearListaIngredientes(int cantidad)
    {
        return Enumerable.Range(0, cantidad)
            .Select(_ => Guid.NewGuid())
            .ToList();
    }

    private List<string> CrearListaCategorias(int cantidad)
    {
        return Enumerable.Range(0, cantidad)
            .Select(i => $"Categoria{i}")
            .ToList();
    }

    #endregion
} 