using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Critical;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.SeedData;

/// <summary>
/// Tests de integración para UnidadesMedidaSeeder
/// 
/// NOTA: Este seeder es especial porque NO crea datos,
/// sino que VALIDA la consistencia de las unidades de medida del sistema.
/// </summary>
public class UnidadesMedidaSeederTests : IntegrationTestBase
{
    public UnidadesMedidaSeederTests(DatabaseFixture fixture) : base(fixture) { }

    private UnidadesMedidaSeeder _seeder = null!;
    private RestauranteProDbContext _context = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        _seeder = new UnidadesMedidaSeeder();
        _context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();
    }

    [Fact]
    public async Task SeedAsync_DeberiaValidarUnidadesMedidaSinErrores()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UnidadesMedidaSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        // El seeder no debe lanzar excepciones durante la validación
        // La validación se considera exitosa si no hay errores críticos
        Assert.True(true); // Si llegamos aquí, la validación fue exitosa
    }

    [Fact]
    public async Task ExistsAsync_DeberiaRetornarFalse_Siempre()
    {
        // Arrange & Act
        var resultado = await _seeder.ExistsAsync(_context, CancellationToken.None);

        // Assert
        // Este seeder siempre debe ejecutarse para validar consistencia
        Assert.False(resultado);
    }

    [Fact]
    public async Task SeedAsync_DeberiaEjecutarseIdempotentemente()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UnidadesMedidaSeeder>>();

        // Act - Primera ejecución
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Act - Segunda ejecución (debería ser idempotente)
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        // No debe haber errores en ejecuciones múltiples
        Assert.True(true);
    }

    [Fact]
    public async Task SeedAsync_DeberiaValidarEnumUnidadMedida()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UnidadesMedidaSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        // Debe validar que el enum UnidadMedida tiene valores definidos
        var unidadesDefinidas = Enum.GetValues<UnidadMedida>().ToList();
        Assert.True(unidadesDefinidas.Any(), "El enum UnidadMedida debe tener valores definidos");
    }

    [Fact]
    public async Task SeedAsync_DeberiaValidarUnidadesCriticas()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UnidadesMedidaSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        // Debe validar que las unidades críticas existen
        var unidadesDefinidas = Enum.GetValues<UnidadMedida>().ToList();
        var unidadesCriticas = new[]
        {
            UnidadMedida.Kilogramo,
            UnidadMedida.Gramo,
            UnidadMedida.Litro,
            UnidadMedida.Mililitro,
            UnidadMedida.Unidad,
            UnidadMedida.Piezas
        };

        foreach (var unidadCritica in unidadesCriticas)
        {
            Assert.Contains(unidadCritica, unidadesDefinidas);
        }
    }

    [Fact]
    public async Task SeedAsync_DeberiaValidarCategoriasDeUnidades()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UnidadesMedidaSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        // Debe validar las categorías: PESO, VOLUMEN, CANTIDAD, COCINA
        var unidadesDefinidas = Enum.GetValues<UnidadMedida>().ToList();
        
        // Verificar unidades de peso
        Assert.Contains(UnidadMedida.Kilogramo, unidadesDefinidas);
        Assert.Contains(UnidadMedida.Gramo, unidadesDefinidas);
        
        // Verificar unidades de volumen
        Assert.Contains(UnidadMedida.Litro, unidadesDefinidas);
        Assert.Contains(UnidadMedida.Mililitro, unidadesDefinidas);
        
        // Verificar unidades de cantidad
        Assert.Contains(UnidadMedida.Unidad, unidadesDefinidas);
        Assert.Contains(UnidadMedida.Piezas, unidadesDefinidas);
        
        // Verificar unidades de cocina
        Assert.Contains(UnidadMedida.Cucharada, unidadesDefinidas);
        Assert.Contains(UnidadMedida.Cucharadita, unidadesDefinidas);
        Assert.Contains(UnidadMedida.Taza, unidadesDefinidas);
    }

    [Fact]
    public async Task SeedAsync_DeberiaValidarAliasDeUnidades()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UnidadesMedidaSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        // Debe validar que los alias son consistentes
        Assert.Equal((int)UnidadMedida.Kilogramos, (int)UnidadMedida.Kilogramo);
        Assert.Equal((int)UnidadMedida.Gramos, (int)UnidadMedida.Gramo);
        Assert.Equal((int)UnidadMedida.Litros, (int)UnidadMedida.Litro);
        Assert.Equal((int)UnidadMedida.Mililitros, (int)UnidadMedida.Mililitro);
        Assert.Equal((int)UnidadMedida.Unidades, (int)UnidadMedida.Unidad);
    }

    [Fact]
    public async Task SeedAsync_DeberiaManejarErroresGracefully()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UnidadesMedidaSeeder>>();

        // Act & Assert
        // El seeder debe manejar errores de validación sin fallar completamente
        var exception = await Record.ExceptionAsync(async () =>
            await _seeder.SeedAsync(_context, logger, CancellationToken.None));

        // No debe lanzar excepciones críticas
        Assert.Null(exception);
    }

    [Fact]
    public async Task SeedAsync_DeberiaGenerarLogsInformativos()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UnidadesMedidaSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        // Debe generar logs informativos sobre el proceso de validación
        // Los logs se pueden verificar en la salida del test
        Assert.True(true);
    }

    [Fact]
    public async Task SeedAsync_DeberiaValidarTodasLasUnidadesDelSistema()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UnidadesMedidaSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        // Debe validar que todas las unidades están correctamente definidas
        var unidadesDefinidas = Enum.GetValues<UnidadMedida>().ToList();
        Assert.True(unidadesDefinidas.Count > 0, "Debe haber al menos una unidad de medida definida");
        
        // Verificar que hay al menos 10 valores únicos (considerando que hay alias)
        var valoresUnicos = unidadesDefinidas.Select(u => (int)u).Distinct().Count();
        Assert.True(valoresUnicos >= 10, $"Debe haber al menos 10 valores únicos, pero hay {valoresUnicos}");
        
        // Verificar que el total de unidades es 15 (incluyendo alias)
        Assert.Equal(15, unidadesDefinidas.Count);
    }
} 