using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Infrastructure.Persistence;
using UnidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida;
using RotacionIngrediente = RestaurantePro.Domain.Inventario.Ingredientes.Enums.RotacionIngrediente;

namespace RestaurantePro.Web.Admin.IntegrationTests.Utils;

/// <summary>
/// Seeder para crear ingredientes de prueba en los tests de integración
/// </summary>
public static class IngredientesTestSeeder
{
    /// <summary>
    /// Crea ingredientes de prueba en la base de datos
    /// </summary>
    public static async Task<List<Guid>> SeedIngredientesAsync(RestauranteProDbContext context, int cantidad = 20)
    {
        Console.WriteLine($"=== IngredientesTestSeeder: Creando {cantidad} ingredientes ===");
        
        var ingredientes = new List<Ingrediente>();
        var ingredienteIds = new List<Guid>();

        // Lista de ingredientes comunes con costos realistas
        var ingredientesData = new[]
        {
            // Carnes
            ("Pollo", "POLLO001", "Pechuga de pollo fresca", UnidadMedida.Kilogramo, 5.50m, 10.0m, 2.0m),
            ("Res", "RES001", "Carne de res molida", UnidadMedida.Kilogramo, 8.75m, 15.0m, 3.0m),
            ("Cerdo", "CERDO001", "Lomo de cerdo", UnidadMedida.Kilogramo, 6.25m, 12.0m, 2.5m),
            ("Pescado", "PESCADO001", "Salmón fresco", UnidadMedida.Kilogramo, 12.00m, 8.0m, 1.5m),
            
            // Vegetales
            ("Cebolla", "CEBOLLA001", "Cebolla blanca", UnidadMedida.Kilogramo, 1.20m, 20.0m, 5.0m),
            ("Tomate", "TOMATE001", "Tomate rojo", UnidadMedida.Kilogramo, 2.50m, 15.0m, 3.0m),
            ("Lechuga", "LECHUGA001", "Lechuga fresca", UnidadMedida.Unidad, 0.50m, 30.0m, 10.0m),
            ("Zanahoria", "ZANAHORIA001", "Zanahoria fresca", UnidadMedida.Kilogramo, 1.80m, 25.0m, 4.0m),
            ("Papa", "PAPA001", "Papa blanca", UnidadMedida.Kilogramo, 1.00m, 30.0m, 5.0m),
            
            // Condimentos y especias
            ("Sal", "SAL001", "Sal marina", UnidadMedida.Kilogramo, 0.80m, 50.0m, 10.0m),
            ("Pimienta", "PIMIENTA001", "Pimienta negra molida", UnidadMedida.Gramo, 0.15m, 1000.0m, 200.0m),
            ("Ajo", "AJO001", "Ajo fresco", UnidadMedida.Kilogramo, 3.50m, 20.0m, 3.0m),
            ("Orégano", "OREGANO001", "Orégano seco", UnidadMedida.Gramo, 0.25m, 500.0m, 50.0m),
            
            // Lácteos
            ("Queso", "QUESO001", "Queso mozzarella", UnidadMedida.Kilogramo, 4.50m, 15.0m, 2.0m),
            ("Leche", "LECHE001", "Leche entera", UnidadMedida.Litro, 1.20m, 20.0m, 5.0m),
            ("Mantequilla", "MANTEQUILLA001", "Mantequilla sin sal", UnidadMedida.Kilogramo, 3.80m, 10.0m, 2.0m),
            
            // Granos y cereales
            ("Arroz", "ARROZ001", "Arroz blanco", UnidadMedida.Kilogramo, 2.20m, 25.0m, 5.0m),
            ("Pasta", "PASTA001", "Pasta espagueti", UnidadMedida.Kilogramo, 1.80m, 20.0m, 4.0m),
            ("Pan", "PAN001", "Pan blanco", UnidadMedida.Unidad, 0.75m, 20.0m, 5.0m),
            
            // Aceites y grasas
            ("Aceite", "ACEITE001", "Aceite de oliva", UnidadMedida.Litro, 4.50m, 15.0m, 3.0m)
        };

        for (int i = 0; i < Math.Min(cantidad, ingredientesData.Length); i++)
        {
            var (nombre, codigo, descripcion, unidad, costoPromedio, stock, stockMinimo) = ingredientesData[i];
            
            var ingrediente = Ingrediente.Crear(
                Guid.NewGuid(),
                nombre,
                codigo,
                descripcion,
                unidad,
                stockMinimo,
                stock,
                RotacionIngrediente.Media,
                TemporadaIngrediente.TodoElAño
            );

            // Establecer el costo promedio después de la creación
            ingrediente.ActualizarCostoPromedio(costoPromedio);

            ingredientes.Add(ingrediente);
            ingredienteIds.Add(ingrediente.Id);
        }

        // Si necesitamos más ingredientes de los predefinidos, generar algunos genéricos
        if (cantidad > ingredientesData.Length)
        {
            for (int i = ingredientesData.Length; i < cantidad; i++)
            {
                var ingrediente = Ingrediente.Crear(
                    Guid.NewGuid(),
                    $"Ingrediente {i + 1}",
                    $"ING{i + 1:D3}",
                    $"Descripción del ingrediente {i + 1}",
                    UnidadMedida.Kilogramo,
                    2.0m, // stock mínimo
                    10.0m, // stock actual
                    RotacionIngrediente.Media,
                    TemporadaIngrediente.TodoElAño
                );

                // Establecer un costo promedio aleatorio entre 1 y 10
                var costoAleatorio = (decimal)(new Random().NextDouble() * 9 + 1);
                ingrediente.ActualizarCostoPromedio(costoAleatorio);

                ingredientes.Add(ingrediente);
                ingredienteIds.Add(ingrediente.Id);
            }
        }

        context.Ingredientes.AddRange(ingredientes);
        await context.SaveChangesAsync();

        return ingredienteIds;
    }

    /// <summary>
    /// Crea ingredientes específicos para recetas de prueba
    /// </summary>
    public static async Task<List<Guid>> SeedIngredientesParaRecetasAsync(RestauranteProDbContext context)
    {
        var ingredientes = new List<Ingrediente>
        {
            // Ingredientes para pizza
            Ingrediente.Crear(
                Guid.NewGuid(),
                "Masa de Pizza",
                "MASA001",
                "Masa base para pizza",
                UnidadMedida.Kilogramo,
                2.0m,
                20.0m,
                RotacionIngrediente.Alta,
                TemporadaIngrediente.TodoElAño
            ),
            
            // Ingredientes para hamburguesa
            Ingrediente.Crear(
                Guid.NewGuid(),
                "Pan de Hamburguesa",
                "PAN_HAMB001",
                "Pan especial para hamburguesas",
                UnidadMedida.Unidad,
                5.0m,
                50.0m,
                RotacionIngrediente.Alta,
                TemporadaIngrediente.TodoElAño
            ),
            
            // Ingredientes para ensalada
            Ingrediente.Crear(
                Guid.NewGuid(),
                "Vinagre Balsámico",
                "VINAGRE001",
                "Vinagre balsámico de calidad",
                UnidadMedida.Litro,
                1.0m,
                10.0m,
                RotacionIngrediente.Baja,
                TemporadaIngrediente.TodoElAño
            )
        };

        // Establecer costos específicos
        ingredientes[0].ActualizarCostoPromedio(1.50m); // Masa de pizza
        ingredientes[1].ActualizarCostoPromedio(0.25m); // Pan de hamburguesa
        ingredientes[2].ActualizarCostoPromedio(8.00m); // Vinagre balsámico

        context.Ingredientes.AddRange(ingredientes);
        await context.SaveChangesAsync();

        Console.WriteLine($"=== IngredientesTestSeeder: {ingredientes.Count} ingredientes creados exitosamente ===");
        foreach (var ing in ingredientes)
        {
            Console.WriteLine($"  - {ing.Id}: {ing.Nombre} (Activo: {ing.EstaActivo})");
        }

        return ingredientes.Select(i => i.Id).ToList();
    }
}
