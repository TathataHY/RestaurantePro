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
                ("Nachos Supreme", "Totopos crujientes con queso fundido, jalapeños, guacamole y crema", 185.00m, "Entradas", 8),
                ("Alitas Buffalo", "12 alitas de pollo con salsa buffalo picante y aderezo ranch", 225.00m, "Entradas", 9),
                ("Quesadillas de Pollo", "Tortillas de harina con pollo desmenuzado y queso Oaxaca", 165.00m, "Entradas", 7),
                ("Dedos de Mozzarella", "Bastones de queso mozzarella empanizados con salsa marinara", 175.00m, "Entradas", 6),
                ("Camarones al Coco", "Camarones gigantes empanizados con coco y salsa agridulce", 285.00m, "Entradas", 8),

                // SOPAS
                ("Sopa de Tortilla", "Caldo de jitomate con tiras de tortilla, aguacate y queso fresco", 125.00m, "Sopas", 8),
                ("Crema de Poblano", "Cremosa sopa de chile poblano con granos de elote", 135.00m, "Sopas", 7),
                ("Sopa de Mariscos", "Caldo de pescado con camarones, pulpo y mejillones", 195.00m, "Sopas", 6),
                ("Consomé de Pollo", "Caldo casero de pollo con verduras y arroz", 115.00m, "Sopas", 5),

                // ENSALADAS
                ("Ensalada César", "Lechuga romana, crotones, queso parmesano y aderezo césar", 165.00m, "Ensaladas", 7),
                ("Ensalada de Arúgula", "Arúgula con pera, nuez, queso de cabra y vinagreta balsámica", 185.00m, "Ensaladas", 6),
                ("Ensalada Griega", "Tomate, pepino, cebolla, aceitunas, queso feta y aceite de oliva", 175.00m, "Ensaladas", 5),
                ("Ensalada de Pollo", "Mix de lechugas con pollo a la plancha, aguacate y vinagreta", 205.00m, "Ensaladas", 8),

                // CARNES
                ("Rib Eye Premium", "Corte de 350g al carbón con papas gajo y verduras asadas", 485.00m, "Carnes", 9),
                ("Filete New York", "Corte de 300g con mantequilla de hierbas y puré de papa", 425.00m, "Carnes", 8),
                ("Arrachera Marinada", "Arrachera de res con chimichurri y elote asado", 325.00m, "Carnes", 9),
                ("Costillas BBQ", "Costillas de cerdo con salsa barbacoa y ensalada de col", 285.00m, "Carnes", 8),
                ("Hamburguesa Gourmet", "Carne Angus con queso manchego, tocino y papas fritas", 245.00m, "Carnes", 10),

                // POLLO
                ("Pollo a la Plancha", "Pechuga de pollo con arroz salvaje y vegetales al vapor", 225.00m, "Pollo", 7),
                ("Pollo en Mole", "Muslo de pollo en mole poblano tradicional con arroz", 245.00m, "Pollo", 8),
                ("Pollo Teriyaki", "Pechuga glasada con salsa teriyaki y arroz frito", 235.00m, "Pollo", 6),
                ("Pollo Parmesano", "Pechuga empanizada con salsa pomodoro y queso mozzarella", 255.00m, "Pollo", 7),

                // PESCADOS Y MARISCOS
                ("Salmón a la Plancha", "Filete de salmón con quinoa y espárragos", 345.00m, "Pescados y Mariscos", 8),
                ("Tacos de Pescado", "Tacos de mahi-mahi con col morada y salsa de mango", 225.00m, "Pescados y Mariscos", 9),
                ("Camarones al Ajillo", "Camarones gigantes salteados en aceite de oliva y ajo", 285.00m, "Pescados y Mariscos", 8),
                ("Pescado Zarandeado", "Huachinango entero a las brasas con salsa de chile", 385.00m, "Pescados y Mariscos", 7),
                ("Pulpo a la Parrilla", "Tentáculos de pulpo con papas cambray y aceite de oliva", 325.00m, "Pescados y Mariscos", 6),

                // PASTA
                ("Spaghetti Carbonara", "Pasta con pancetta, huevo, queso parmesano y pimienta negra", 195.00m, "Pasta", 8),
                ("Penne Arrabbiata", "Pasta con salsa de tomate picante y albahaca fresca", 175.00m, "Pasta", 7),
                ("Lasagna de Carne", "Lasagna casera con carne bolognesa y queso ricotta", 225.00m, "Pasta", 9),
                ("Ravioles de Espinaca", "Pasta rellena de espinaca y ricotta con salsa de mantequilla", 205.00m, "Pasta", 6),
                ("Fettuccine Alfredo", "Pasta plana con salsa alfredo cremosa y pollo", 215.00m, "Pasta", 8),

                // PIZZA
                ("Pizza Margherita", "Salsa de tomate, mozzarella fresca y albahaca", 245.00m, "Pizza", 8),
                ("Pizza Pepperoni", "Salsa de tomate, mozzarella y pepperoni premium", 265.00m, "Pizza", 10),
                ("Pizza Hawaiana", "Salsa de tomate, mozzarella, jamón y piña", 255.00m, "Pizza", 7),
                ("Pizza Quattro Stagioni", "Cuatro estaciones con champiñones, jamón, alcachofas y aceitunas", 285.00m, "Pizza", 6),
                ("Pizza de Pollo BBQ", "Salsa barbacoa, pollo, cebolla morada y mozzarella", 275.00m, "Pizza", 8),

                // VEGETARIANO
                ("Bowl de Quinoa", "Quinoa con verduras asadas, aguacate y vinagreta de limón", 185.00m, "Vegetariano", 7),
                ("Burger Vegetariana", "Hamburguesa de lentejas con queso vegano y papas", 205.00m, "Vegetariano", 6),
                ("Curry de Verduras", "Curry aromático con coconut milk y arroz basmati", 195.00m, "Vegetariano", 5),
                ("Tacos Veganos", "Tacos de jackfruit con salsa verde y aguacate", 175.00m, "Vegetariano", 6),

                // POSTRES
                ("Cheesecake NY", "Pastel de queso estilo Nueva York con coulis de frutos rojos", 145.00m, "Postres", 9),
                ("Tiramisu", "Postre italiano con café, mascarpone y cacao", 135.00m, "Postres", 8),
                ("Brownie con Helado", "Brownie de chocolate caliente con helado de vainilla", 125.00m, "Postres", 10),
                ("Flan Napolitano", "Flan casero con caramelo y crema batida", 105.00m, "Postres", 7),
                ("Tres Leches", "Pastel esponjoso bañado en tres tipos de leche", 115.00m, "Postres", 8),

                // BEBIDAS CALIENTES
                ("Café Americano", "Café de grano selecto servido negro", 45.00m, "Bebidas Calientes", 8),
                ("Cappuccino", "Espresso con leche vaporizada y espuma", 55.00m, "Bebidas Calientes", 9),
                ("Chocolate Caliente", "Chocolate artesanal con malvaviscos", 65.00m, "Bebidas Calientes", 7),
                ("Té Verde", "Té verde premium con miel de abeja", 35.00m, "Bebidas Calientes", 5),

                // BEBIDAS FRÍAS
                ("Agua Fresca de Horchata", "Bebida tradicional de arroz con canela", 45.00m, "Bebidas Frías", 8),
                ("Limonada Mineral", "Limonada con agua mineral y hierbabuena", 55.00m, "Bebidas Frías", 9),
                ("Smoothie de Mango", "Batido de mango con yogurt griego", 75.00m, "Bebidas Frías", 7),
                ("Coca Cola", "Refresco de cola en botella de vidrio", 35.00m, "Bebidas Frías", 10),

                // COCTELES
                ("Margarita Clásica", "Tequila blanco, triple sec y jugo de limón", 145.00m, "Cocteles", 9),
                ("Mojito Tradicional", "Ron blanco, hierbabuena, limón y agua mineral", 135.00m, "Cocteles", 8),
                ("Piña Colada", "Ron, crema de coco y jugo de piña", 155.00m, "Cocteles", 7),
                ("Old Fashioned", "Whiskey bourbon con azúcar y bitter de angostura", 175.00m, "Cocteles", 6),

                // ESPECIALIDADES DE LA CASA
                ("Molcajete Mar y Tierra", "Carne asada, camarones, nopales y salsa en molcajete de piedra", 485.00m, "Especialidades de la Casa", 10),
                ("Cochinita Pibil", "Cerdo deshebrado en achiote con cebolla morada y tortillas", 285.00m, "Especialidades de la Casa", 9),
                ("Paella Valenciana", "Arroz con mariscos, pollo y azafrán para dos personas", 565.00m, "Especialidades de la Casa", 8),

                // MENU INFANTIL
                ("Nuggets de Pollo", "Dedos de pollo empanizados con papas fritas", 125.00m, "Menu Infantil", 10),
                ("Mini Hamburguesa", "Hamburguesa pequeña con queso y papas", 115.00m, "Menu Infantil", 9),
                ("Quesadilla Sencilla", "Quesadilla de queso con guacamole", 95.00m, "Menu Infantil", 8),
                ("Espagueti con Mantequilla", "Pasta sencilla con mantequilla y queso parmesano", 105.00m, "Menu Infantil", 7)
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