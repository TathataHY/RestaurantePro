using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Web.Admin.IntegrationTests.Utils;

namespace RestaurantePro.Web.Admin.IntegrationTests.Utils;

/// <summary>
/// Seeder para crear datos de prueba de productos y categorías
/// </summary>
public static class ProductosTestSeeder
{
    /// <summary>
    /// Crea categorías de productos de prueba en la base de datos
    /// </summary>
    public static async Task<List<Guid>> SeedCategoriasAsync(RestauranteProDbContext context)
    {
        var categorias = new List<ProductoCategoria>
        {
            ProductoCategoria.Crear("Bebidas", "Bebidas frías y calientes", 1, "#2196F3", "🥤"),
            ProductoCategoria.Crear("Entradas", "Aperitivos y entradas", 2, "#FF9800", "🍤"),
            ProductoCategoria.Crear("Platos Principales", "Platos principales del menú", 3, "#4CAF50", "🍽️"),
            ProductoCategoria.Crear("Postres", "Postres y dulces", 4, "#E91E63", "🍰"),
            ProductoCategoria.Crear("Especialidades", "Platos especiales del chef", 5, "#9C27B0", "⭐")
        };

        // Establecer fecha de creación para las categorías
        var fechaCreacion = DateTime.UtcNow;
        foreach (var categoria in categorias)
        {
            categoria.SetFechaCreacionForTesting(fechaCreacion);
        }

        context.ProductoCategorias.AddRange(categorias);
        await context.SaveChangesAsync();

        return categorias.Select(c => c.Id).ToList();
    }

    /// <summary>
    /// Crea productos de prueba en la base de datos
    /// </summary>
    public static async Task<List<Guid>> SeedProductosAsync(RestauranteProDbContext context, List<Guid> categoriaIds, int cantidad = 10)
    {
        if (!categoriaIds.Any())
        {
            throw new InvalidOperationException("No hay categorías disponibles para crear productos");
        }

        // Usar la primera categoría disponible para todos los productos si no hay suficientes
        var categoriaId = categoriaIds[0];
        var bebidasId = categoriaIds.Count > 0 ? categoriaIds[0] : categoriaId;
        var entradasId = categoriaIds.Count > 1 ? categoriaIds[1] : categoriaId;
        var platosId = categoriaIds.Count > 2 ? categoriaIds[2] : categoriaId;
        var postresId = categoriaIds.Count > 3 ? categoriaIds[3] : categoriaId;

        var productos = new List<Producto>();
        var nombresProductos = new[]
        {
            "Coca Cola", "Café Americano", "Jugo de Naranja", "Agua Mineral", "Cerveza",
            "Ceviche de Pescado", "Anticuchos", "Causa Limeña", "Papa a la Huancaína", "Tequeños",
            "Lomo Saltado", "Arroz con Pollo", "Aji de Gallina", "Tallarines Verdes", "Pollo a la Brasa",
            "Tres Leches", "Flan de Caramelo", "Helado de Vainilla", "Mazamorra Morada", "Suspiro Limeño"
        };
        
        var descripciones = new[]
        {
            "Delicioso plato tradicional", "Especialidad de la casa", "Preparado con ingredientes frescos",
            "Sabor auténtico", "Receta familiar", "Ingredientes de calidad", "Preparación artesanal"
        };
        
        var precios = new[] { 3.50m, 5.00m, 8.00m, 12.00m, 15.00m, 18.00m, 22.00m, 25.00m, 28.00m, 30.00m };
        var categorias = new[] { bebidasId, entradasId, platosId, postresId };
        var nombresCategorias = new[] { "Bebidas", "Entradas", "Platos Principales", "Postres" };

        for (int i = 0; i < cantidad; i++)
        {
            var nombre = nombresProductos[i % nombresProductos.Length] + (i > nombresProductos.Length - 1 ? $" {i + 1}" : "");
            var descripcion = descripciones[i % descripciones.Length];
            var precio = precios[i % precios.Length];
            var categoriaIdActual = categorias[i % categorias.Length];
            var nombreCategoria = nombresCategorias[i % nombresCategorias.Length];

            productos.Add(Producto.Crear(nombre, descripcion, new PrecioProducto(precio), categoriaIdActual, nombreCategoria));
        }

        context.Productos.AddRange(productos);
        await context.SaveChangesAsync();

        return productos.Select(p => p.Id).ToList();
    }

    /// <summary>
    /// Limpia todos los datos de productos y categorías de la base de datos
    /// </summary>
    public static async Task CleanupAsync(RestauranteProDbContext context)
    {
        // Eliminar productos primero (por las foreign keys)
        var productos = context.Productos.ToList();
        context.Productos.RemoveRange(productos);

        // Eliminar categorías
        var categorias = context.ProductoCategorias.ToList();
        context.ProductoCategorias.RemoveRange(categorias);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Obtiene una categoría aleatoria de la lista proporcionada
    /// </summary>
    public static Guid GetRandomCategoriaId(List<Guid> categoriaIds)
    {
        if (!categoriaIds.Any())
        {
            throw new InvalidOperationException("No hay categorías disponibles");
        }

        var random = new Random();
        return categoriaIds[random.Next(categoriaIds.Count)];
    }

    /// <summary>
    /// Obtiene la primera categoría de la lista (útil para pruebas consistentes)
    /// </summary>
    public static Guid GetFirstCategoriaId(List<Guid> categoriaIds)
    {
        if (!categoriaIds.Any())
        {
            throw new InvalidOperationException("No hay categorías disponibles");
        }

        return categoriaIds.First();
    }

    #region Recetas Helper Methods

    /// <summary>
    /// Crea una receta de prueba para un producto
    /// </summary>
    public static async Task<Guid> SeedRecetaAsync(RestauranteProDbContext context, Guid productoId)
    {
        // Crear ingredientes básicos si no existen
        var ingredientesExistentes = await context.Ingredientes.CountAsync();
        if (ingredientesExistentes == 0)
        {
            await IngredientesTestSeeder.SeedIngredientesAsync(context, 5);
        }

        // Obtener ingredientes existentes
        var ingredientesDisponibles = await context.Ingredientes.Take(2).ToListAsync();
        if (!ingredientesDisponibles.Any())
        {
            throw new InvalidOperationException("No se pudieron crear ingredientes para la receta");
        }

        // Crear receta
        var receta = Receta.Crear(
            productoId,
            "Receta de prueba - Mezclar ingredientes y cocinar por 20 minutos",
            20 // Tiempo en minutos
        );

        // Agregar ingredientes a la receta
        foreach (var ingrediente in ingredientesDisponibles)
        {
            receta.AgregarIngrediente(ingrediente.Id, ingrediente.Nombre, 1.0m, ingrediente.UnidadMedida, false);
        }

        context.Recetas.Add(receta);
        await context.SaveChangesAsync();

        return receta.Id;
    }

    /// <summary>
    /// Crea una receta compleja de prueba para un producto
    /// </summary>
    public static async Task<Guid> SeedRecetaComplejaAsync(RestauranteProDbContext context, Guid productoId)
    {
        // Crear ingredientes básicos si no existen
        var ingredientesExistentes = await context.Ingredientes.CountAsync();
        if (ingredientesExistentes == 0)
        {
            await IngredientesTestSeeder.SeedIngredientesAsync(context, 10);
        }

        // Obtener ingredientes existentes
        var ingredientesDisponibles = await context.Ingredientes.Take(5).ToListAsync();
        if (!ingredientesDisponibles.Any())
        {
            throw new InvalidOperationException("No se pudieron crear ingredientes para la receta compleja");
        }

        // Crear receta
        var receta = Receta.Crear(
            productoId,
            "Receta compleja - Preparación detallada con múltiples pasos y técnicas avanzadas de cocina",
            45 // Tiempo en minutos
        );

        // Agregar ingredientes a la receta
        foreach (var ingrediente in ingredientesDisponibles)
        {
            receta.AgregarIngrediente(ingrediente.Id, ingrediente.Nombre, 2.0m, ingrediente.UnidadMedida, false);
        }

        context.Recetas.Add(receta);
        await context.SaveChangesAsync();

        return receta.Id;
    }

    /// <summary>
    /// Crea una receta con muchos ingredientes de prueba para un producto
    /// </summary>
    public static async Task<Guid> SeedRecetaConMuchosIngredientesAsync(RestauranteProDbContext context, Guid productoId)
    {
        // Crear ingredientes básicos si no existen
        var ingredientesExistentes = await context.Ingredientes.CountAsync();
        if (ingredientesExistentes == 0)
        {
            await IngredientesTestSeeder.SeedIngredientesAsync(context, 20);
        }

        // Obtener ingredientes existentes
        var ingredientesDisponibles = await context.Ingredientes.Take(10).ToListAsync();
        if (!ingredientesDisponibles.Any())
        {
            throw new InvalidOperationException("No se pudieron crear ingredientes para la receta con muchos ingredientes");
        }

        // Crear receta
        var receta = Receta.Crear(
            productoId,
            "Receta con muchos ingredientes - Preparación que requiere múltiples ingredientes y técnicas",
            60 // Tiempo en minutos
        );

        // Agregar ingredientes a la receta
        foreach (var ingrediente in ingredientesDisponibles)
        {
            receta.AgregarIngrediente(ingrediente.Id, ingrediente.Nombre, 1.5m, ingrediente.UnidadMedida, false);
        }

        context.Recetas.Add(receta);
        await context.SaveChangesAsync();

        return receta.Id;
    }

    #endregion
}
