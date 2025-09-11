using FluentAssertions;
using RestaurantePro.Web.Admin.Data;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Data;

public class WeatherForecastServiceTests
{
    private readonly WeatherForecastService _service;

    public WeatherForecastServiceTests()
    {
        _service = new WeatherForecastService();
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public async Task GetForecastAsync_ConFechaValida_DeberiaRetornarCincoPronosticos()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetForecastAsync_ConFechaValida_DeberiaRetornarPronosticosConsecutivos()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        
        for (int i = 0; i < result.Length; i++)
        {
            result[i].Date.Should().Be(startDate.AddDays(i + 1));
        }
    }

    [Fact]
    public async Task GetForecastAsync_ConFechaValida_DeberiaRetornarPronosticosConTemperaturasValidas()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        
        foreach (var forecast in result)
        {
            forecast.TemperatureC.Should().BeInRange(-20, 54); // Rango del código: -20 a 54
            forecast.Summary.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetForecastAsync_ConFechaValida_DeberiaRetornarPronosticosConResumenValido()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);
        var validSummaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", 
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        
        foreach (var forecast in result)
        {
            forecast.Summary.Should().BeOneOf(validSummaries);
        }
    }

    // ===== PRUEBAS ROBUSTAS - CASOS EDGE =====

    [Fact]
    public async Task GetForecastAsync_ConFechaMinima_DeberiaManejarCorrectamente()
    {
        // Arrange
        var startDate = DateOnly.MinValue;

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        
        for (int i = 0; i < result.Length; i++)
        {
            result[i].Date.Should().Be(startDate.AddDays(i + 1));
        }
    }

    [Fact]
    public async Task GetForecastAsync_ConFechaMaxima_DeberiaLanzarExcepcion()
    {
        // Arrange
        var startDate = DateOnly.MaxValue;

        // Act & Assert
        var action = async () => await _service.GetForecastAsync(startDate);
        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetForecastAsync_ConFechaActual_DeberiaManejarCorrectamente()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        
        for (int i = 0; i < result.Length; i++)
        {
            result[i].Date.Should().Be(startDate.AddDays(i + 1));
        }
    }

    [Fact]
    public async Task GetForecastAsync_ConFechaFutura_DeberiaManejarCorrectamente()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddYears(10));

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        
        for (int i = 0; i < result.Length; i++)
        {
            result[i].Date.Should().Be(startDate.AddDays(i + 1));
        }
    }

    [Fact]
    public async Task GetForecastAsync_ConFechaPasada_DeberiaManejarCorrectamente()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-10));

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        
        for (int i = 0; i < result.Length; i++)
        {
            result[i].Date.Should().Be(startDate.AddDays(i + 1));
        }
    }

    // ===== PRUEBAS ROBUSTAS - CONCURRENCIA =====

    [Fact]
    public async Task GetForecastAsync_ConConcurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);
        var tasks = Enumerable.Range(1, 10).Select(_ => _service.GetForecastAsync(startDate)).ToArray();

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        foreach (var result in results)
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
        }
    }

    [Fact]
    public async Task GetForecastAsync_ConConcurrenciaYFechasDiferentes_DeberiaManejarCorrectamente()
    {
        // Arrange
        var startDates = Enumerable.Range(1, 5).Select(i => new DateOnly(2024, 1, i)).ToArray();
        var tasks = startDates.Select(date => _service.GetForecastAsync(date)).ToArray();

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        for (int i = 0; i < results.Length; i++)
        {
            results[i].Should().NotBeNull();
            results[i].Should().HaveCount(5);
            results[i][0].Date.Should().Be(startDates[i].AddDays(1));
        }
    }

    // ===== PRUEBAS ROBUSTAS - RENDIMIENTO Y LÍMITES =====

    [Fact]
    public async Task GetForecastAsync_ConMuchasLlamadas_DeberiaManejarCorrectamente()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);
        var tasks = Enumerable.Range(1, 100).Select(_ => _service.GetForecastAsync(startDate)).ToArray();

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(100);
        foreach (var result in results)
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
        }
    }

    [Fact]
    public async Task GetForecastAsync_ConFechasExtremas_DeberiaManejarCorrectamente()
    {
        // Arrange
        var extremeDates = new[]
        {
            DateOnly.MinValue,
            new DateOnly(1, 1, 1),
            new DateOnly(2024, 12, 31) // Fecha segura para evitar desbordamiento
        };

        // Act & Assert
        foreach (var date in extremeDates)
        {
            var result = await _service.GetForecastAsync(date);
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
        }
    }

    // ===== PRUEBAS ROBUSTAS - VALIDACIÓN DE DATOS =====

    [Fact]
    public async Task GetForecastAsync_DeberiaRetornarTemperaturasDiferentes()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);
        var results = new List<WeatherForecast[]>();

        // Act - Ejecutar múltiples veces para verificar variabilidad
        for (int i = 0; i < 10; i++)
        {
            var result = await _service.GetForecastAsync(startDate);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(10);
        
        // Verificar que al menos algunas temperaturas son diferentes
        var allTemperatures = results.SelectMany(r => r.Select(f => f.TemperatureC)).ToList();
        allTemperatures.Should().Contain(t => t != allTemperatures[0]);
    }

    [Fact]
    public async Task GetForecastAsync_DeberiaRetornarResumenesDiferentes()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);
        var results = new List<WeatherForecast[]>();

        // Act - Ejecutar múltiples veces para verificar variabilidad
        for (int i = 0; i < 10; i++)
        {
            var result = await _service.GetForecastAsync(startDate);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(10);
        
        // Verificar que al menos algunos resúmenes son diferentes
        var allSummaries = results.SelectMany(r => r.Select(f => f.Summary)).ToList();
        allSummaries.Should().Contain(s => s != allSummaries[0]);
    }

    [Fact]
    public async Task GetForecastAsync_DeberiaRetornarPronosticosConPropiedadesValidas()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 1);

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        
        foreach (var forecast in result)
        {
            forecast.Date.Should().NotBe(default(DateOnly));
            forecast.TemperatureC.Should().BeInRange(-20, 54);
            forecast.Summary.Should().NotBeNullOrEmpty();
            forecast.Summary.Should().NotBeNullOrWhiteSpace();
        }
    }

    // ===== PRUEBAS ROBUSTAS - CASOS ESPECIALES =====

    [Fact]
    public async Task GetForecastAsync_ConFechaBisiesta_DeberiaManejarCorrectamente()
    {
        // Arrange
        var startDate = new DateOnly(2024, 2, 28); // Año bisiesto

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        
        for (int i = 0; i < result.Length; i++)
        {
            result[i].Date.Should().Be(startDate.AddDays(i + 1));
        }
    }

    [Fact]
    public async Task GetForecastAsync_ConFechaFinDeMes_DeberiaManejarCorrectamente()
    {
        // Arrange
        var startDate = new DateOnly(2024, 1, 31); // Fin de enero

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        
        for (int i = 0; i < result.Length; i++)
        {
            result[i].Date.Should().Be(startDate.AddDays(i + 1));
        }
    }

    [Fact]
    public async Task GetForecastAsync_ConFechaFinDeAño_DeberiaManejarCorrectamente()
    {
        // Arrange
        var startDate = new DateOnly(2024, 12, 31); // Fin de año

        // Act
        var result = await _service.GetForecastAsync(startDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        
        for (int i = 0; i < result.Length; i++)
        {
            result[i].Date.Should().Be(startDate.AddDays(i + 1));
        }
    }
}
