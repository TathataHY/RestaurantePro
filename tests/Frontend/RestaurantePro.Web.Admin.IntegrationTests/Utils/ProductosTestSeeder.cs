using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Infrastructure.Persistence.Contexts;

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
            ProductoCategoria.Crear("Bebidas", "Bebidas frías y calientes", 1),
            ProductoCategoria.Crear("Entradas", "Aperitivos y entradas", 2),
            ProductoCategoria.Crear("Platos Principales", "Platos principales del menú", 3),
            ProductoCategoria.Crear("Postres", "Postres y dulces", 4),
            ProductoCategoria.Crear("Especialidades", "Platos especiales del chef", 5)
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
    public static async Task<List<Guid>> SeedProductosAsync(RestauranteProDbContext context, List<Guid> categoriaIds)
    {
        if (!categoriaIds.Any())
        {
            throw new InvalidOperationException("No hay categorías disponibles para crear productos");
        }

        var bebidasId = categoriaIds[0];
        var entradasId = categoriaIds[1];
        var platosId = categoriaIds[2];
        var postresId = categoriaIds[3];

        var productos = new List<Producto>
        {
            // Bebidas
            Producto.Crear(
                "Coca Cola",
                "Refresco de cola 350ml",
                new PrecioProducto(3.50m),
                bebidasId,
                "Bebidas"
            ),
            Producto.Crear(
                "Café Americano",
                "Café negro americano",
                new PrecioProducto(2.50m),
                bebidasId,
                "Bebidas"
            ),
            Producto.Crear(
                "Jugo de Naranja",
                "Jugo natural de naranja",
                new PrecioProducto(4.00m),
                bebidasId,
                "Bebidas"
            ),

            // Entradas
            Producto.Crear(
                "Ceviche de Pescado",
                "Ceviche fresco de pescado con cebolla y ají",
                new PrecioProducto(18.00m),
                entradasId,
                "Entradas"
            ),
            Producto.Crear(
                "Anticuchos",
                "Brochetas de corazón de res con ají panca",
                new PrecioProducto(15.00m),
                entradasId,
                "Entradas"
            ),

            // Platos Principales
            Producto.Crear(
                "Lomo Saltado",
                "Lomo de res salteado con papas fritas y arroz",
                new PrecioProducto(25.00m),
                platosId,
                "Platos Principales"
            ),
            Producto.Crear(
                "Arroz con Pollo",
                "Arroz con pollo y verduras",
                new PrecioProducto(20.00m),
                platosId,
                "Platos Principales"
            ),
            Producto.Crear(
                "Pollo a la Brasa",
                "Pollo asado con papas fritas y ensalada",
                new PrecioProducto(22.00m),
                platosId,
                "Platos Principales"
            ),

            // Postres
            Producto.Crear(
                "Suspiro a la Limeña",
                "Postre tradicional de manjar blanco y merengue",
                new PrecioProducto(8.00m),
                postresId,
                "Postres"
            ),
            Producto.Crear(
                "Mazamorra Morada",
                "Postre de maíz morado con frutas",
                new PrecioProducto(6.00m),
                postresId,
                "Postres"
            )
        };

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
}
