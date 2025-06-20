using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Demo
{
    /// <summary>
    /// Seeder que crea productos de demostración con precios realistas
    /// Usa las categorías previamente creadas y datos típicos de restaurante
    /// </summary>
    public class ProductosSeeder : ISeedData
    {
        public string Name => "Productos Demo";
        public int Order => 180;
        public bool IsDevOnly => false;
        public bool IsCritical => false;

        public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("🍕 Iniciando seed de productos demo...");

            // Obtener categorías existentes
            var categorias = await context.ProductoCategorias
                .Where(c => c.EstaActivo)
                .ToDictionaryAsync(c => c.Nombre, c => new { c.Id, c.Nombre }, cancellationToken);

            if (categorias.Count == 0)
            {
                logger.LogWarning("⚠️ No se encontraron categorías. Ejecutar ProductoCategoriasSeeder primero.");
                return;
            }

            var productos = new List<(string nombre, string descripcion, decimal precio, string categoria, int popularidad)>
            {
                // ENTRADAS
                ("Palta Pura Chilena", "Palta molida con tostadas crujientes, tomate cherry y merkén", 6500m, "Entradas", 8),
                ("Alitas al Merkén", "12 alitas de pollo con salsa de merkén picante y pebre", 8500m, "Entradas", 9),
                ("Empanadas de Pino", "Empanadas de horno rellenas de pino tradicional con aceitunas", 6200m, "Entradas", 7),
                ("Palitos de Queso", "Bastones de queso chanco empanizados con salsa americana", 6800m, "Entradas", 6),
                ("Machas a la Parmesana", "Machas gratinadas con queso parmesano al horno", 12500m, "Entradas", 8),

                // SOPAS
                ("Cazuela de Cordero", "Caldo de cordero con zapallo, choclo y papas", 4800m, "Sopas", 8),
                ("Crema de Zapallo", "Cremosa sopa de zapallo con jengibre y semillas de zapallo", 4200m, "Sopas", 7),
                ("Parihuela Chilena", "Caldo de mariscos con congrio, cholgas y almejas", 7200m, "Sopas", 6),
                ("Consomé de Ave", "Caldo casero de pollo con verduras y fideos", 3800m, "Sopas", 5),

                // ENSALADAS
                ("Ensalada Chilena", "Tomate, cebolla y cilantro con aceite de oliva y limón", 4800m, "Ensaladas", 7),
                ("Ensalada de Palta y Rúcula", "Rúcula con palta, pera, nuez y vinagreta de limón", 6500m, "Ensaladas", 6),
                ("Ensalada Mediterránea", "Tomate, pepino, cebolla, aceitunas negras y queso de cabra", 6200m, "Ensaladas", 5),
                ("Ensalada de Pollo Palta", "Mix de lechugas con pollo grillado, palta y vinagreta", 7800m, "Ensaladas", 8),

                // CARNES
                ("Bife de Chorizo Premium", "Corte de 350g a la parrilla con papas doradas y ensalada", 18500m, "Carnes", 9),
                ("Lomo Vetado", "Corte de 300g con mantequilla de hierbas y puré de papas", 16200m, "Carnes", 8),
                ("Asado de Tira", "Asado de tira marinado con pebre y choclo asado", 12800m, "Carnes", 9),
                ("Costillar de Cerdo", "Costillas de cerdo con salsa barbacoa y ensalada de repollo", 11200m, "Carnes", 8),
                ("Chacarero Completo", "Hamburguesa con palta, tomate, porotos verdes y mayo", 9800m, "Carnes", 10),

                // POLLO
                ("Pollo Grillado", "Pechuga de pollo con arroz pilaf y verduras al vapor", 8500m, "Pollo", 7),
                ("Pollo al Ají", "Muslo de pollo en salsa de ají amarillo con arroz", 9200m, "Pollo", 8),
                ("Pollo Teriyaki", "Pechuga glasada con salsa teriyaki y arroz frito", 8800m, "Pollo", 6),
                ("Pollo a la Parmesana", "Pechuga empanizada con salsa de tomate y queso derretido", 9800m, "Pollo", 7),

                // PESCADOS Y MARISCOS
                ("Salmón a la Plancha", "Filete de salmón del sur con quinoa y espárragos", 13500m, "Pescados y Mariscos", 8),
                ("Reineta a la Plancha", "Reineta fresca con papas mayo y ensalada chilena", 8500m, "Pescados y Mariscos", 9),
                ("Camarones al Pil Pil", "Camarones ecuatorianos salteados en aceite de oliva y ajo", 11200m, "Pescados y Mariscos", 8),
                ("Congrio Frito", "Congrio en trozos frito con papas fritas y pebre", 14800m, "Pescados y Mariscos", 7),
                ("Pulpo al Olivo", "Tentáculos de pulpo con papas cocidas y salsa de olivo", 12800m, "Pescados y Mariscos", 6),

                // PASTA
                ("Spaghetti Carbonara", "Pasta con tocino, huevo, queso parmesano y pimienta negra", 7500m, "Pasta", 8),
                ("Penne Arrabbiata", "Pasta con salsa de tomate picante y albahaca fresca", 6800m, "Pasta", 7),
                ("Lasagna de Carne", "Lasagna casera con carne molida y salsa blanca", 8500m, "Pasta", 9),
                ("Ravioles de Ricotta", "Pasta rellena de ricotta y espinaca con salsa de mantequilla", 7800m, "Pasta", 6),
                ("Fettuccine Alfredo", "Pasta plana con salsa alfredo cremosa y pollo", 8200m, "Pasta", 8),

                // PIZZA
                ("Pizza Margherita", "Salsa de tomate, mozzarella fresca y albahaca", 9500m, "Pizza", 8),
                ("Pizza Pepperoni", "Salsa de tomate, mozzarella y pepperoni italiano", 10200m, "Pizza", 10),
                ("Pizza Italiana", "Salsa de tomate, mozzarella, jamón y piña", 9800m, "Pizza", 7),
                ("Pizza Quattro Stagioni", "Cuatro estaciones con champiñones, jamón, alcachofas y aceitunas", 11200m, "Pizza", 6),
                ("Pizza BBQ", "Salsa barbacoa, pollo, cebolla morada y mozzarella", 10500m, "Pizza", 8),

                // VEGETARIANO
                ("Bowl de Quinoa", "Quinoa con verduras asadas, palta y vinagreta de limón", 7200m, "Vegetariano", 7),
                ("Burger Vegana", "Hamburguesa de lentejas con queso vegano y papas", 7800m, "Vegetariano", 6),
                ("Curry de Verduras", "Curry aromático con leche de coco y arroz basmati", 7500m, "Vegetariano", 5),
                ("Tacos Veganos", "Tacos de hongos con salsa verde y palta", 6800m, "Vegetariano", 6),

                // POSTRES
                ("Cheesecake de Berries", "Pastel de queso con coulis de berries del sur", 5500m, "Postres", 9),
                ("Tiramisu", "Postre italiano con café, mascarpone y cacao", 5200m, "Postres", 8),
                ("Brownie con Helado", "Brownie de chocolate caliente con helado de manjar", 4800m, "Postres", 10),
                ("Flan con Manjar", "Flan casero con manjar y crema batida", 4200m, "Postres", 7),
                ("Tres Leches", "Pastel esponjoso bañado en tres tipos de leche", 4500m, "Postres", 8),

                // BEBIDAS CALIENTES
                ("Café Americano", "Café de grano selecto servido negro", 1800m, "Bebidas Calientes", 8),
                ("Cappuccino", "Espresso con leche vaporizada y espuma", 2200m, "Bebidas Calientes", 9),
                ("Chocolate Caliente", "Chocolate artesanal con marshmallows", 2500m, "Bebidas Calientes", 7),
                ("Té de Boldo", "Té de boldo premium con miel de ulmo", 1500m, "Bebidas Calientes", 5),

                // BEBIDAS FRÍAS
                ("Mote con Huesillo", "Bebida tradicional de duraznos secos con mote", 1800m, "Bebidas Frías", 8),
                ("Limonada Mineral", "Limonada con agua mineral y menta", 2200m, "Bebidas Frías", 9),
                ("Smoothie de Chirimoya", "Batido de chirimoya con yogurt griego", 2800m, "Bebidas Frías", 7),
                ("Coca Cola", "Refresco de cola en botella de vidrio", 1500m, "Bebidas Frías", 10),

                // COCTELES
                ("Pisco Sour", "Pisco chileno, jugo de limón, clara de huevo y azúcar", 5500m, "Cocteles", 9),
                ("Mojito Tradicional", "Ron blanco, hierbabuena, limón y agua mineral", 5200m, "Cocteles", 8),
                ("Piña Colada", "Ron, crema de coco y jugo de piña", 5800m, "Cocteles", 7),
                ("Old Fashioned", "Whiskey bourbon con azúcar y bitter de angostura", 6500m, "Cocteles", 6),

                // ESPECIALIDADES DE LA CASA
                ("Parrillada Completa", "Asado de tira, longaniza, morcilla y pollo con ensaladas", 18500m, "Especialidades de la Casa", 10),
                ("Curanto en Olla", "Curanto tradicional con mariscos, carnes y papas", 11200m, "Especialidades de la Casa", 9),
                ("Paella Chilena", "Arroz con mariscos del Pacífico y pollo para dos personas", 21500m, "Especialidades de la Casa", 8),

                // MENU INFANTIL
                ("Nuggets de Pollo", "Dedos de pollo empanizados con papas fritas", 4800m, "Menu Infantil", 10),
                ("Mini Hamburguesa", "Hamburguesa pequeña con queso chanco y papas", 4500m, "Menu Infantil", 9),
                ("Empanada de Queso", "Empanada pequeña de queso con ketchup", 3500m, "Menu Infantil", 8),
                ("Tallarines con Mantequilla", "Pasta sencilla con mantequilla y queso rallado", 4200m, "Menu Infantil", 7)
            };

            var productosAgregados = 0;
            var productosOmitidos = 0;

            foreach (var (nombre, descripcion, precio, categoriaNombre, popularidad) in productos)
            {
                // Verificar si ya existe
                var existeProducto = await context.Productos
                    .AnyAsync(p => p.Nombre == nombre, cancellationToken);

                if (!existeProducto && categorias.ContainsKey(categoriaNombre))
                {
                    var categoria = categorias[categoriaNombre];
                    
                    // Crear el producto usando el método estático
                    var producto = Producto.Crear(
                        nombre,
                        descripcion,
                        new PrecioProducto(precio),
                        categoria.Id,
                        categoria.Nombre
                    );

                    // Asignar ID determinístico
                    var guidBytes = System.Text.Encoding.UTF8.GetBytes($"Producto_{nombre}");
                    var hash = System.Security.Cryptography.MD5.HashData(guidBytes);
                    producto.GetType().GetProperty("Id")?.SetValue(producto, new Guid(hash));

                    // Asignar popularidad usando reflexión (no hay método público)
                    producto.GetType().GetProperty("Popularidad")?.SetValue(producto, popularidad);

                    context.Productos.Add(producto);
                    productosAgregados++;
                    
                    logger.LogInformation("  ✅ Agregado: {Nombre} - ${Precio} ({Categoria}) ⭐{Popularidad}", 
                        nombre, precio, categoriaNombre, popularidad);
                }
                else
                {
                    productosOmitidos++;
                    if (!categorias.ContainsKey(categoriaNombre))
                    {
                        logger.LogWarning("  ⚠️ Omitido {Nombre} - Categoría '{Categoria}' no encontrada", 
                            nombre, categoriaNombre);
                    }
                    else
                    {
                        logger.LogDebug("  ⏭️ Omitido producto existente: {Nombre}", nombre);
                    }
                }
            }

            if (productosAgregados > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
                logger.LogInformation("💾 Guardados {Count} productos en BD", productosAgregados);
            }

            logger.LogInformation("🎯 Seed de productos completado - ✅ {Agregados} agregados, ⏭️ {Omitidos} omitidos", 
                productosAgregados, productosOmitidos);
        }

        public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken)
        {
            // Considerar que existe si tenemos al menos 50 productos
            var productCount = await context.Productos.CountAsync(cancellationToken);
            return productCount >= 50;
        }
    }
} 