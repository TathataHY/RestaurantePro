using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Demo
{
    /// <summary>
    /// Seeder que crea categorías de productos para demostración
    /// Categorías típicas de un restaurante con orden de presentación
    /// </summary>
    public class ProductoCategoriasSeeder : ISeedData
    {
        public string Name => "Categorías de Productos Demo";
        public int Order => 170;
        public bool IsDevOnly => false;
        public bool IsCritical => false;

        public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("🍽️ Iniciando seed de categorías de productos demo...");

            var categorias = new List<ProductoCategoria>
            {
                // Entradas y Aperitivos
                ProductoCategoria.Crear(
                    "Entradas",
                    "Aperitivos y entradas para abrir el apetito",
                    1
                ),

                // Sopas y Ensaladas
                ProductoCategoria.Crear(
                    "Sopas",
                    "Sopas y caldos caseros",
                    2
                ),

                ProductoCategoria.Crear(
                    "Ensaladas",
                    "Ensaladas frescas y nutritivas",
                    3
                ),

                // Platos Principales
                ProductoCategoria.Crear(
                    "Carnes",
                    "Cortes de carne premium y especialidades",
                    4
                ),

                ProductoCategoria.Crear(
                    "Pollo",
                    "Especialidades de pollo preparadas al momento",
                    5
                ),

                ProductoCategoria.Crear(
                    "Pescados y Mariscos",
                    "Pescados frescos y mariscos del día",
                    6
                ),

                ProductoCategoria.Crear(
                    "Pasta",
                    "Pastas frescas con salsas artesanales",
                    7
                ),

                ProductoCategoria.Crear(
                    "Pizza",
                    "Pizzas artesanales con masa casera",
                    8
                ),

                // Platos Vegetarianos
                ProductoCategoria.Crear(
                    "Vegetariano",
                    "Opciones saludables sin carne",
                    9
                ),

                // Postres
                ProductoCategoria.Crear(
                    "Postres",
                    "Dulces tentaciones para cerrar con broche de oro",
                    10
                ),

                // Bebidas
                ProductoCategoria.Crear(
                    "Bebidas Calientes",
                    "Café, té y bebidas reconfortantes",
                    11
                ),

                ProductoCategoria.Crear(
                    "Bebidas Frías",
                    "Refrescos, jugos y bebidas refrescantes",
                    12
                ),

                ProductoCategoria.Crear(
                    "Cocteles",
                    "Cocteles artesanales y bebidas premium",
                    13
                ),

                // Especialidades
                ProductoCategoria.Crear(
                    "Especialidades de la Casa",
                    "Nuestros platillos estrella y creaciones únicas",
                    14
                ),

                ProductoCategoria.Crear(
                    "Menu Infantil",
                    "Opciones especiales para los más pequeños",
                    15
                )
            };

            var categoriasAgregadas = 0;
            var categoriasOmitidas = 0;

            foreach (var categoria in categorias)
            {
                // Verificar si ya existe por nombre
                var existeCategoria = context.ProductoCategorias
                    .Any(c => c.Nombre == categoria.Nombre);

                if (!existeCategoria)
                {
                    // Asignar ID determinístico basado en nombre
                    var guidBytes = System.Text.Encoding.UTF8.GetBytes($"ProductoCategoria_{categoria.Nombre}");
                    var hash = System.Security.Cryptography.MD5.HashData(guidBytes);
                    categoria.GetType().GetProperty("Id")?.SetValue(categoria, new Guid(hash));

                    context.ProductoCategorias.Add(categoria);
                    categoriasAgregadas++;
                    logger.LogInformation("  ✅ Agregada categoría: {Nombre} (Orden: {Orden})", 
                        categoria.Nombre, categoria.Orden);
                }
                else
                {
                    categoriasOmitidas++;
                    logger.LogDebug("  ⏭️ Omitida categoría existente: {Nombre}", categoria.Nombre);
                }
            }

            if (categoriasAgregadas > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
                logger.LogInformation("💾 Guardadas {Count} categorías de productos en BD", categoriasAgregadas);
            }

            logger.LogInformation("🎯 Seed de categorías completado - ✅ {Agregadas} agregadas, ⏭️ {Omitidas} omitidas", 
                categoriasAgregadas, categoriasOmitidas);
        }

        public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken)
        {
            // Considerar que existe si tenemos al menos 10 categorías
            var categoriaCount = await context.ProductoCategorias.CountAsync(cancellationToken);
            return categoriaCount >= 10;
        }
    }
} 