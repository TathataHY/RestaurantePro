using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Critical;

/// <summary>
/// Seeder para validar las unidades de medida críticas del sistema.
/// Las unidades de medida son fundamentales para el manejo correcto del inventario.
/// 
/// Orden: 120 (después de RolesSeeder=100, antes de EstadosSeeder=150)
/// </summary>
public class UnidadesMedidaSeeder : ISeedData
{
    public string Name => "Unidades de Medida del Sistema";
    public int Order => 120;
    public bool IsDevOnly => false;
    public bool IsCritical => true;

    public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("🔧 Validando unidades de medida del sistema...");

        try
        {
            await ValidarUnidadesMedida(logger);
            await DocumentarUnidadesDisponibles(logger);
            
            logger.LogInformation("✅ Unidades de medida validadas exitosamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error al validar unidades de medida");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
    {
        // Las unidades están definidas como enum, siempre "existen" pero ejecutamos para validar
        return false; // Siempre ejecutar validación
    }

    private async Task ValidarUnidadesMedida(ILogger logger)
    {
        logger.LogInformation("📏 Validando enum UnidadMedida...");

        // Validar que el enum está correctamente definido
        var unidadesDefinidas = Enum.GetValues<UnidadMedida>().ToList();
        
        if (!unidadesDefinidas.Any())
        {
            throw new InvalidOperationException("El enum UnidadMedida no tiene valores definidos");
        }

        logger.LogInformation("✅ Enum UnidadMedida validado: {Count} unidades disponibles", unidadesDefinidas.Count);

        // Validar unidades críticas específicas
        await ValidarUnidadesCriticas(unidadesDefinidas, logger);
        
        await Task.CompletedTask;
    }

    private async Task ValidarUnidadesCriticas(List<UnidadMedida> unidades, ILogger logger)
    {
        logger.LogDebug("🔍 Validando unidades críticas...");

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
            if (!unidades.Contains(unidadCritica))
            {
                throw new InvalidOperationException($"Unidad crítica faltante: {unidadCritica}");
            }
            
            logger.LogDebug("  ✓ {Unidad} - OK", unidadCritica);
        }

        await Task.CompletedTask;
    }

    private async Task DocumentarUnidadesDisponibles(ILogger logger)
    {
        logger.LogInformation("📋 Documentando unidades de medida disponibles...");

        var categorias = new Dictionary<string, List<(UnidadMedida unidad, string descripcion, string simbolo)>>
        {
            ["PESO"] = new()
            {
                (UnidadMedida.Kilogramo, "Kilogramo - Unidad base de peso", "kg"),
                (UnidadMedida.Gramo, "Gramo - Peso pequeño", "g")
            },
            ["VOLUMEN"] = new()
            {
                (UnidadMedida.Litro, "Litro - Unidad base de volumen", "L"),
                (UnidadMedida.Mililitro, "Mililitro - Volumen pequeño", "ml")
            },
            ["CANTIDAD"] = new()
            {
                (UnidadMedida.Unidad, "Unidad individual", "un"),
                (UnidadMedida.Piezas, "Piezas - Unidades individuales", "pcs"),
                (UnidadMedida.Paquete, "Paquete - Cantidad predefinida", "paq")
            },
            ["COCINA"] = new()
            {
                (UnidadMedida.Cucharada, "Cucharada - Medida de cocina", "cda"),
                (UnidadMedida.Cucharadita, "Cucharadita - Medida pequeña", "cdta"),
                (UnidadMedida.Taza, "Taza - Medida de cocina", "tz")
            }
        };

        foreach (var categoria in categorias)
        {
            logger.LogInformation("📂 Categoría: {Categoria}", categoria.Key);
            
            foreach (var (unidad, descripcion, simbolo) in categoria.Value)
            {
                logger.LogDebug("  📏 {Unidad} ({Simbolo}) - {Descripcion}", 
                    unidad, simbolo, descripcion);
            }
        }

        await LogearUnidadesConAlias(logger);
        await Task.CompletedTask;
    }

    private async Task LogearUnidadesConAlias(ILogger logger)
    {
        logger.LogInformation("🔗 Documentando alias de unidades...");

        var alias = new[]
        {
            (UnidadMedida.Kilogramos, UnidadMedida.Kilogramo, "Kilogramos → Kilogramo"),
            (UnidadMedida.Gramos, UnidadMedida.Gramo, "Gramos → Gramo"),
            (UnidadMedida.Litros, UnidadMedida.Litro, "Litros → Litro"),
            (UnidadMedida.Mililitros, UnidadMedida.Mililitro, "Mililitros → Mililitro"),
            (UnidadMedida.Unidades, UnidadMedida.Unidad, "Unidades → Unidad")
        };

        foreach (var (aliasUnidad, unidadBase, descripcion) in alias)
        {
            if ((int)aliasUnidad == (int)unidadBase)
            {
                logger.LogDebug("  🔗 {Descripcion}", descripcion);
            }
            else
            {
                logger.LogWarning("⚠️ Alias inconsistente: {Descripcion}", descripcion);
            }
        }

        await Task.CompletedTask;
    }
} 