using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Proveedores.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Testing
{
    /// <summary>
    /// Seeder que crea grandes volúmenes de datos para pruebas de rendimiento
    /// Incluye miles de registros para testing de carga y performance
    /// </summary>
    public class DatosRendimiento : ISeedData
    {
        public string Name => "Datos Rendimiento";
        public int Order => 310;
        public bool IsDevOnly => true;
        public bool IsCritical => false;

        // Configuración de volúmenes
        private const int USUARIOS_COUNT = 1000;
        private const int CLIENTES_COUNT = 5000;
        private const int PRODUCTOS_COUNT = 500;
        private const int INGREDIENTES_COUNT = 1000;
        private const int PROVEEDORES_COUNT = 200;
        private const int MESAS_COUNT = 100;

        public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("⚡ Iniciando seed de Datos Rendimiento...");
                logger.LogWarning("🚨 ATENCIÓN: Este seeder creará {TotalRegistros} registros aproximadamente", 
                    USUARIOS_COUNT + CLIENTES_COUNT + PRODUCTOS_COUNT + INGREDIENTES_COUNT + PROVEEDORES_COUNT + MESAS_COUNT);

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // 1. USUARIOS MASIVOS
                await SeedUsuariosMasivos(context, logger, cancellationToken);

                // 2. CLIENTES MASIVOS
                await SeedClientesMasivos(context, logger, cancellationToken);

                // 3. PRODUCTOS MASIVOS
                await SeedProductosMasivos(context, logger, cancellationToken);

                // 4. INGREDIENTES MASIVOS
                await SeedIngredientesMasivos(context, logger, cancellationToken);

                // 5. PROVEEDORES MASIVOS
                await SeedProveedoresMasivos(context, logger, cancellationToken);

                // 6. MESAS MASIVAS
                await SeedMesasMasivas(context, logger, cancellationToken);

                // Guardar en lotes para optimizar rendimiento
                logger.LogInformation("💾 Guardando datos masivos en base de datos...");
                await context.SaveChangesAsync(cancellationToken);

                stopwatch.Stop();
                logger.LogInformation("✅ Seed de Datos Rendimiento completado en {TiempoSegundos:F2} segundos", 
                    stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error durante el seed de Datos Rendimiento");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
        {
            return await context.Set<Usuario>()
                .AnyAsync(u => u.NombreUsuario.StartsWith("perf-user-"), cancellationToken);
        }

        private async Task SeedUsuariosMasivos(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("👥 Creando {Count} usuarios para testing de rendimiento...", USUARIOS_COUNT);

            var usuarios = new List<Usuario>(USUARIOS_COUNT);
            var roles = Enum.GetValues<RolUsuario>();
            var random = new Random(42); // Seed fijo para reproducibilidad

            for (int i = 1; i <= USUARIOS_COUNT; i++)
            {
                var rol = roles[random.Next(roles.Length)];
                var usuario = Usuario.Crear(
                    $"perf-user-{i:D6}",
                    $"Usuario Rendimiento {i:D6}",
                    $"perf.user.{i:D6}@performance.test",
                    rol
                );

                // 80% de usuarios confirmados
                if (random.Next(100) < 80)
                {
                    usuario.ConfirmarCuenta();
                }

                // 5% de usuarios inactivos
                if (random.Next(100) < 5)
                {
                    usuario.Desactivar();
                }

                usuarios.Add(usuario);

                // Procesar en lotes para evitar problemas de memoria
                if (i % 100 == 0)
                {
                    context.Set<Usuario>().AddRange(usuarios);
                    usuarios.Clear();
                    
                    if (cancellationToken.IsCancellationRequested)
                        return;
                }
            }

            // Agregar usuarios restantes
            if (usuarios.Count > 0)
            {
                context.Set<Usuario>().AddRange(usuarios);
            }

            logger.LogInformation("✅ {Count} usuarios de rendimiento creados", USUARIOS_COUNT);
        }

        private async Task SeedClientesMasivos(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("👤 Creando {Count} clientes para testing de rendimiento...", CLIENTES_COUNT);

            var clientes = new List<Cliente>(1000); // Lotes de 1000
            var segmentos = Enum.GetValues<SegmentoCliente>();
            var random = new Random(42);

            var nombres = new[] { "Juan", "María", "Carlos", "Ana", "Luis", "Carmen", "José", "Laura", "Miguel", "Elena" };
            var apellidos = new[] { "García", "Rodríguez", "González", "Fernández", "López", "Martínez", "Sánchez", "Pérez", "Gómez", "Martín" };
            var dominios = new[] { "gmail.com", "hotmail.com", "yahoo.com", "outlook.com", "test.com" };

            for (int i = 1; i <= CLIENTES_COUNT; i++)
            {
                var nombre = nombres[random.Next(nombres.Length)];
                var apellido = apellidos[random.Next(apellidos.Length)];
                var dominio = dominios[random.Next(dominios.Length)];
                var segmento = segmentos[random.Next(segmentos.Length)];

                var cliente = Cliente.Crear(
                    Guid.NewGuid(),
                    ClienteNombre.Crear(nombre, $"{apellido} {i:D6}"),
                    Email.Create($"{nombre.ToLower()}.{apellido.ToLower()}.{i:D6}@{dominio}"),
                    PhoneNumber.Create($"+52-{random.Next(100, 999)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}"),
                    new DateTime(random.Next(1950, 2005), random.Next(1, 13), random.Next(1, 29)),
                    random.Next(100) < 95 // 95% activos
                );

                cliente.ActualizarSegmento(segmento);
                clientes.Add(cliente);

                // Procesar en lotes
                if (i % 1000 == 0)
                {
                    context.Set<Cliente>().AddRange(clientes);
                    clientes.Clear();
                    logger.LogInformation("📊 Procesados {Count}/{Total} clientes...", i, CLIENTES_COUNT);
                    
                    if (cancellationToken.IsCancellationRequested)
                        return;
                }
            }

            // Agregar clientes restantes
            if (clientes.Count > 0)
            {
                context.Set<Cliente>().AddRange(clientes);
            }

            logger.LogInformation("✅ {Count} clientes de rendimiento creados", CLIENTES_COUNT);
        }

        private async Task SeedProductosMasivos(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("🍕 Creando {Count} productos para testing de rendimiento...", PRODUCTOS_COUNT);

            // Crear categorías primero
            var categorias = new List<ProductoCategoria>
            {
                ProductoCategoria.Crear("Rendimiento Entradas", "Categoría para testing de rendimiento", 800),
                ProductoCategoria.Crear("Rendimiento Principales", "Categoría para testing de rendimiento", 801),
                ProductoCategoria.Crear("Rendimiento Postres", "Categoría para testing de rendimiento", 802),
                ProductoCategoria.Crear("Rendimiento Bebidas", "Categoría para testing de rendimiento", 803)
            };

            context.Set<ProductoCategoria>().AddRange(categorias);
            await context.SaveChangesAsync(cancellationToken);

            var productos = new List<Producto>(100);
            var random = new Random(42);
            var tiposProducto = new[] { "Plato", "Bebida", "Postre", "Entrada", "Especial" };
            var adjetivos = new[] { "Delicioso", "Especial", "Premium", "Clásico", "Gourmet", "Tradicional", "Moderno" };

            for (int i = 1; i <= PRODUCTOS_COUNT; i++)
            {
                var tipo = tiposProducto[random.Next(tiposProducto.Length)];
                var adjetivo = adjetivos[random.Next(adjetivos.Length)];
                var categoria = categorias[random.Next(categorias.Count)];
                var precio = (decimal)(random.NextDouble() * 500 + 10); // Entre $10 y $510

                var producto = Producto.Crear(
                    $"{adjetivo} {tipo} Rendimiento {i:D4}",
                    $"Descripción detallada del {tipo.ToLower()} número {i} para testing de rendimiento del sistema",
                    new PrecioProducto(Math.Round(precio, 2)),
                    categoria.Id,
                    categoria.Nombre
                );

                productos.Add(producto);

                // Procesar en lotes
                if (i % 100 == 0)
                {
                    context.Set<Producto>().AddRange(productos);
                    productos.Clear();
                    
                    if (cancellationToken.IsCancellationRequested)
                        return;
                }
            }

            if (productos.Count > 0)
            {
                context.Set<Producto>().AddRange(productos);
            }

            logger.LogInformation("✅ {Count} productos de rendimiento creados", PRODUCTOS_COUNT);
        }

        private async Task SeedIngredientesMasivos(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("🥕 Creando {Count} ingredientes para testing de rendimiento...", INGREDIENTES_COUNT);

            var ingredientes = new List<Ingrediente>(100);
            var random = new Random(42);
            var unidades = Enum.GetValues<UnidadMedida>();
            var rotaciones = Enum.GetValues<RotacionIngrediente>();
            var temporadas = Enum.GetValues<TemporadaIngrediente>();

            var tiposIngrediente = new[] { "Carne", "Verdura", "Fruta", "Especia", "Lácteo", "Grano", "Aceite", "Condimento" };
            var origenes = new[] { "Nacional", "Importado", "Local", "Orgánico", "Premium", "Estándar" };

            for (int i = 1; i <= INGREDIENTES_COUNT; i++)
            {
                var tipo = tiposIngrediente[random.Next(tiposIngrediente.Length)];
                var origen = origenes[random.Next(origenes.Length)];
                var unidad = unidades[random.Next(unidades.Length)];
                var rotacion = rotaciones[random.Next(rotaciones.Length)];
                var temporada = temporadas[random.Next(temporadas.Length)];

                var stockMinimo = (decimal)(random.NextDouble() * 100 + 1);
                var stockActual = stockMinimo * (decimal)(random.NextDouble() * 5 + 1);

                var ingrediente = Ingrediente.Crear(
                    Guid.NewGuid(),
                    $"{origen} {tipo} Rendimiento {i:D4}",
                    $"PERF-{i:D4}",
                    $"Ingrediente {tipo.ToLower()} {origen.ToLower()} para testing de rendimiento",
                    unidad,
                    Math.Round(stockMinimo, 2),
                    Math.Round(stockActual, 2),
                    rotacion,
                    temporada
                );

                ingredientes.Add(ingrediente);

                // Procesar en lotes
                if (i % 100 == 0)
                {
                    context.Set<Ingrediente>().AddRange(ingredientes);
                    ingredientes.Clear();
                    logger.LogInformation("📊 Procesados {Count}/{Total} ingredientes...", i, INGREDIENTES_COUNT);
                    
                    if (cancellationToken.IsCancellationRequested)
                        return;
                }
            }

            if (ingredientes.Count > 0)
            {
                context.Set<Ingrediente>().AddRange(ingredientes);
            }

            logger.LogInformation("✅ {Count} ingredientes de rendimiento creados", INGREDIENTES_COUNT);
        }

        private async Task SeedProveedoresMasivos(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("🏭 Creando {Count} proveedores para testing de rendimiento...", PROVEEDORES_COUNT);

            var proveedores = new List<Proveedor>(50);
            var random = new Random(42);
            var categorias = Enum.GetValues<CategoriaProveedor>();

            var tiposEmpresa = new[] { "S.A. de C.V.", "S.R.L.", "S.A.", "Ltda.", "Corp.", "Inc." };
            var sectores = new[] { "Alimentos", "Bebidas", "Equipos", "Limpieza", "Servicios", "Tecnología" };
            var ciudades = new[] { "Santiago", "Valparaíso", "Concepción", "La Serena", "Antofagasta", "Temuco", "Rancagua" };

            for (int i = 1; i <= PROVEEDORES_COUNT; i++)
            {
                var sector = sectores[random.Next(sectores.Length)];
                var tipoEmpresa = tiposEmpresa[random.Next(tiposEmpresa.Length)];
                var ciudad = ciudades[random.Next(ciudades.Length)];
                var categoria = categorias[random.Next(categorias.Length)];

                var proveedor = Proveedor.Crear(
                    $"{sector} Rendimiento {i:D3} {tipoEmpresa}",
                    $"Contacto Rendimiento {i:D3}",
                    $"contacto.{i:D3}@rendimiento-{sector.ToLower()}.cl",
                    $"+52-{random.Next(100, 999)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}",
                    $"Dirección Rendimiento {i} No. {random.Next(1, 9999)}",
                    ciudad,
                    $"{random.Next(10000, 99999)}",
                    "Chile",
                    $"REN{i:D3}{random.Next(100000, 999999)}",
                    $"Banco Rendimiento - Cuenta: {random.Next(1000000000, int.MaxValue)}",
                    random.Next(1, 90)
                );

                proveedor.AgregarCategoria(categoria);

                // 10% de proveedores inactivos
                if (random.Next(100) < 10)
                {
                    proveedor.Desactivar("Proveedor de rendimiento inactivo");
                }

                proveedores.Add(proveedor);

                // Procesar en lotes
                if (i % 50 == 0)
                {
                    context.Set<Proveedor>().AddRange(proveedores);
                    proveedores.Clear();
                    
                    if (cancellationToken.IsCancellationRequested)
                        return;
                }
            }

            if (proveedores.Count > 0)
            {
                context.Set<Proveedor>().AddRange(proveedores);
            }

            logger.LogInformation("✅ {Count} proveedores de rendimiento creados", PROVEEDORES_COUNT);
        }

        private async Task SeedMesasMasivas(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            logger.LogInformation("🏠 Creando {Count} mesas para testing de rendimiento...", MESAS_COUNT);

            var mesas = new List<Mesa>(MESAS_COUNT);
            var random = new Random(42);
            var ubicaciones = new[] { "Terraza", "Interior", "Barra", "Privado", "VIP", "Patio", "Salón Principal", "Salón Secundario" };

            for (int i = 1; i <= MESAS_COUNT; i++)
            {
                var ubicacion = ubicaciones[random.Next(ubicaciones.Length)];
                var capacidad = random.Next(1, 12); // Mesas de 1 a 12 personas
                var numeroMesa = 2000 + i; // Numeración especial para rendimiento

                var mesa = Mesa.Crear(numeroMesa, capacidad, $"{ubicacion} Rendimiento");

                // Simular diferentes estados
                var estadoRandom = random.Next(100);
                if (estadoRandom < 60) // 60% disponibles
                {
                    // Mesa disponible (estado por defecto)
                }
                else if (estadoRandom < 80) // 20% ocupadas
                {
                    mesa.MarcarComoOcupada();
                }
                else if (estadoRandom < 95) // 15% reservadas
                {
                    mesa.MarcarComoReservada();
                }
                else // 5% fuera de servicio
                {
                    mesa.MarcarComoFueraDeServicio("Mantenimiento de rendimiento");
                }

                mesas.Add(mesa);
            }

            context.Set<Mesa>().AddRange(mesas);
            logger.LogInformation("✅ {Count} mesas de rendimiento creadas", MESAS_COUNT);
        }
    }
} 