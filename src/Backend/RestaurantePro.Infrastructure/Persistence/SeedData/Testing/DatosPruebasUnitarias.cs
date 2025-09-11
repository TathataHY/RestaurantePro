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
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Proveedores.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Testing
{
    /// <summary>
    /// Seeder que crea datos específicos para pruebas unitarias
    /// Incluye casos edge, valores límite y escenarios de error controlados
    /// </summary>
    public class DatosPruebasUnitarias : ISeedData
    {
        public string Name => "Datos Pruebas Unitarias";
        public int Order => 300;
        public bool IsDevOnly => true;
        public bool IsCritical => false;

        public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("🧪 Iniciando seed de Datos Pruebas Unitarias...");

                // 1. USUARIOS PARA TESTING
                await SeedUsuariosTesting(context, logger);

                // 2. PRODUCTOS PARA TESTING
                await SeedProductosTesting(context, logger);

                // 3. INGREDIENTES PARA TESTING
                await SeedIngredientesTesting(context, logger);

                // 4. CLIENTES PARA TESTING
                await SeedClientesTesting(context, logger);

                // 5. PROVEEDORES PARA TESTING
                await SeedProveedoresTesting(context, logger);

                // 6. MESAS PARA TESTING
                await SeedMesasTesting(context, logger);

                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("✅ Seed de Datos Pruebas Unitarias completado exitosamente");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error durante el seed de Datos Pruebas Unitarias");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
        {
            return await context.Set<Usuario>()
                .AnyAsync(u => u.NombreUsuario == "test-user-edge-case", cancellationToken);
        }

        private async Task SeedUsuariosTesting(RestauranteProDbContext context, ILogger logger)
        {
            logger.LogInformation("👥 Creando usuarios para testing...");

            var usuarios = new List<Usuario>
            {
                // CASO EDGE: Usuario con nombre muy largo
                Usuario.Crear(
                    "test-user-edge-case",
                    "Usuario Con Nombre Extremadamente Largo Para Probar Límites Del Sistema Y Validaciones",
                    "edge.case@testing.com",
                    RolUsuario.Mesero
                ),

                // CASO LÍMITE: Usuario con datos mínimos
                Usuario.Crear(
                    "min",
                    "A",
                    "a@b.co",
                    RolUsuario.Mesero
                ),

                // CASO NORMAL: Usuario estándar para comparación
                Usuario.Crear(
                    "test-standard",
                    "Usuario Estándar Testing",
                    "standard@testing.com",
                    RolUsuario.Mesero
                ),

                // CASO ESPECIAL: Usuario inactivo
                Usuario.Crear(
                    "test-inactive",
                    "Usuario Inactivo Testing",
                    "inactive@testing.com",
                    RolUsuario.Mesero
                ),

                // CASO ESPECIAL: Usuario administrador
                Usuario.Crear(
                    "test-admin",
                    "Administrador Testing",
                    "admin@testing.com",
                    RolUsuario.Administrador
                )
            };

            // Configurar estados especiales
            usuarios[0].ConfirmarCuenta(); // Edge case confirmado
            usuarios[1].ConfirmarCuenta(); // Mínimo confirmado
            usuarios[2].ConfirmarCuenta(); // Estándar confirmado
            // usuarios[3] permanece sin confirmar (Inactivo)
            usuarios[4].ConfirmarCuenta(); // Admin confirmado

            // Desactivar el usuario inactivo
            usuarios[3].Desactivar();

            context.Set<Usuario>().AddRange(usuarios);
            logger.LogInformation("✅ {Count} usuarios de testing creados", usuarios.Count);
        }

        private async Task SeedProductosTesting(RestauranteProDbContext context, ILogger logger)
        {
            logger.LogInformation("🍕 Creando productos para testing...");

            // Crear categorías primero
            var categorias = new List<ProductoCategoria>
            {
                ProductoCategoria.Crear("Test Categoría", "Categoría para testing", 999, "#FF5722", "🧪"),
                ProductoCategoria.Crear("Test Edge", "Categoría casos edge", 998, "#FF9800", "🔬")
            };

            context.Set<ProductoCategoria>().AddRange(categorias);
            await context.SaveChangesAsync();

            var productos = new List<Producto>
            {
                // CASO EDGE: Producto con precio muy alto
                Producto.Crear(
                    "Producto Testing Precio Alto",
                    "Producto con precio extremadamente alto para testing",
                    new PrecioProducto(99999.99m),
                    categorias[1].Id,
                    categorias[1].Nombre
                ),

                // CASO LÍMITE: Producto con precio mínimo
                Producto.Crear(
                    "Producto Testing Precio Mínimo",
                    "Producto con precio mínimo para testing",
                    new PrecioProducto(0.01m),
                    categorias[0].Id,
                    categorias[0].Nombre
                ),

                // CASO NORMAL: Producto estándar
                Producto.Crear(
                    "Producto Testing Estándar",
                    "Producto estándar para comparación en testing",
                    new PrecioProducto(25.50m),
                    categorias[0].Id,
                    categorias[0].Nombre
                ),

                // CASO ESPECIAL: Producto inactivo
                Producto.Crear(
                    "Producto Testing Inactivo",
                    "Producto inactivo para testing",
                    new PrecioProducto(15.00m),
                    categorias[0].Id,
                    categorias[0].Nombre
                ),

                // CASO EDGE: Producto con nombre muy largo
                Producto.Crear(
                    "Producto Testing Con Nombre Extremadamente Largo Para Probar Límites Del Sistema Y Validaciones De Longitud",
                    "Descripción también muy larga para probar los límites del sistema de validación y asegurar que funciona correctamente",
                    new PrecioProducto(50.00m),
                    categorias[1].Id,
                    categorias[1].Nombre
                )
            };

            context.Set<Producto>().AddRange(productos);
            logger.LogInformation("✅ {Count} productos de testing creados", productos.Count);
        }

        private async Task SeedIngredientesTesting(RestauranteProDbContext context, ILogger logger)
        {
            logger.LogInformation("🥕 Creando ingredientes para testing...");

            var ingredientes = new List<Ingrediente>
            {
                // CASO EDGE: Stock muy alto
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Ingrediente Testing Stock Alto",
                    "TEST-HIGH-001",
                    "Ingrediente con stock extremadamente alto",
                    UnidadMedida.Kilogramo,
                    1.0m,
                    99999.99m
                ),

                // CASO LÍMITE: Stock mínimo
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Ingrediente Testing Stock Mínimo",
                    "TEST-MIN-001",
                    "Ingrediente con stock mínimo",
                    UnidadMedida.Gramo,
                    0.01m,
                    0.01m
                ),

                // CASO NORMAL: Ingrediente estándar
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Ingrediente Testing Estándar",
                    "TEST-STD-001",
                    "Ingrediente estándar para comparación",
                    UnidadMedida.Kilogramo,
                    5.0m,
                    25.0m
                ),

                // CASO ESPECIAL: Ingrediente sin stock
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Ingrediente Testing Sin Stock",
                    "TEST-ZERO-001",
                    "Ingrediente sin stock para testing",
                    UnidadMedida.Litro,
                    2.0m,
                    0.0m
                ),

                // CASO EDGE: Ingrediente con rotación especial
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Ingrediente Testing Rotación",
                    "TEST-ROT-001",
                    "Ingrediente para testing de rotación",
                    UnidadMedida.Piezas,
                    10.0m,
                    50.0m,
                    RotacionIngrediente.Alta,
                    TemporadaIngrediente.Verano
                )
            };

            // Los ingredientes ya están configurados con sus propiedades en el constructor

            context.Set<Ingrediente>().AddRange(ingredientes);
            logger.LogInformation("✅ {Count} ingredientes de testing creados", ingredientes.Count);
        }

        private async Task SeedClientesTesting(RestauranteProDbContext context, ILogger logger)
        {
            logger.LogInformation("👤 Creando clientes para testing...");

            var clientes = new List<Cliente>
            {
                // CASO EDGE: Cliente con nombre muy largo
                Cliente.Crear(
                    Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                    ClienteNombre.Crear(
                        "Nombre Extremadamente Largo Para Probar Límites",
                        "Apellido También Muy Largo Para Testing De Validaciones"
                    ),
                    Email.Create("edge.case.cliente@testing-very-long-domain.com"),
                    PhoneNumber.Create("+52-999-999-9999"),
                    new DateTime(1990, 1, 1),
                    true
                ),

                // CASO LÍMITE: Cliente con datos mínimos
                Cliente.Crear(
                    Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
                    ClienteNombre.Crear("A", "B"),
                    Email.Create("a@b.co"),
                    PhoneNumber.Create("+1-1-1"),
                    new DateTime(2000, 12, 31),
                    true
                ),

                // CASO NORMAL: Cliente estándar
                Cliente.Crear(
                    Guid.Parse("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC"),
                    ClienteNombre.Crear("Cliente", "Testing Estándar"),
                    Email.Create("cliente.testing@example.com"),
                    PhoneNumber.Create("+52-555-123-4567"),
                    new DateTime(1985, 6, 15),
                    true
                ),

                // CASO ESPECIAL: Cliente inactivo
                Cliente.Crear(
                    Guid.Parse("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD"),
                    ClienteNombre.Crear("Cliente", "Inactivo Testing"),
                    Email.Create("inactivo@testing.com"),
                    PhoneNumber.Create("+52-555-999-0000"),
                    new DateTime(1975, 3, 20),
                    false
                ),

                // CASO EDGE: Cliente muy joven
                Cliente.Crear(
                    Guid.Parse("EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE"),
                    ClienteNombre.Crear("Cliente", "Muy Joven"),
                    Email.Create("joven@testing.com"),
                    PhoneNumber.Create("+52-555-111-2222"),
                    DateTime.Now.AddYears(-18).AddDays(-1), // Exactamente 18 años
                    true
                )
            };

            // Configurar segmentos usando el método real
            clientes[0].ActualizarSegmento(SegmentoCliente.Premium);
            clientes[1].ActualizarSegmento(SegmentoCliente.Regular);
            clientes[2].ActualizarSegmento(SegmentoCliente.FrecuenciaAlta);
            clientes[3].ActualizarSegmento(SegmentoCliente.Regular);
            clientes[4].ActualizarSegmento(SegmentoCliente.Regular);

            context.Set<Cliente>().AddRange(clientes);
            logger.LogInformation("✅ {Count} clientes de testing creados", clientes.Count);
        }

        private async Task SeedProveedoresTesting(RestauranteProDbContext context, ILogger logger)
        {
            logger.LogInformation("🏭 Creando proveedores para testing...");

            var proveedores = new List<Proveedor>
            {
                // CASO EDGE: Proveedor con datos muy largos
                Proveedor.Crear(
                    "Proveedor Testing Con Nombre Extremadamente Largo Para Probar Límites Del Sistema S.A. de C.V.",
                    "Contacto Con Nombre Muy Largo Para Testing",
                    "contacto.muy.largo@proveedor-testing-edge-case.com",
                    "+52-999-999-9999-ext-12345",
                    "Dirección Extremadamente Larga Para Testing De Validaciones Y Límites Del Sistema",
                    "Ciudad Con Nombre Muy Largo",
                    "99999",
                    "Chile",
                    "TEST999999XXX",
                    "Información Bancaria Muy Larga Para Testing",
                    365
                ),

                // CASO LÍMITE: Proveedor con datos mínimos
                Proveedor.Crear(
                    "P",
                    "C",
                    "a@b.co",
                    "1",
                    "D",
                    "C",
                    "1",
                    "P",
                    "MIN999999",
                    "B",
                    1
                ),

                // CASO NORMAL: Proveedor estándar
                Proveedor.Crear(
                    "Proveedor Testing Estándar S.A.",
                    "Contacto Estándar",
                    "contacto@testing-standard.com",
                    "+52-555-123-4567",
                    "Dirección Testing 123",
                    "Ciudad Testing",
                    "12345",
                    "Chile",
                    "STD123456789",
                    "Banco Testing - Cuenta: 123456789",
                    30
                ),

                // CASO ESPECIAL: Proveedor inactivo
                Proveedor.Crear(
                    "Proveedor Testing Inactivo",
                    "Contacto Inactivo",
                    "inactivo@testing.com",
                    "+52-555-000-0000",
                    "Dirección Inactiva",
                    "Ciudad Inactiva",
                    "00000",
                    "Chile",
                    "INA000000000",
                    "Banco Inactivo",
                    0
                ),

                // CASO EDGE: Proveedor con muchas categorías
                Proveedor.Crear(
                    "Proveedor Testing Multi-Categoría",
                    "Contacto Multi",
                    "multi@testing.com",
                    "+52-555-777-8888",
                    "Dirección Multi-Categoría",
                    "Ciudad Multi",
                    "77777",
                    "Chile",
                    "MUL777777777",
                    "Banco Multi",
                    45
                )
            };

            // Configurar categorías y estados especiales
            proveedores[0].AgregarCategoria(CategoriaProveedor.AlimentosBasicos);
            proveedores[2].AgregarCategoria(CategoriaProveedor.BebidasNoAlcoholicas);
            proveedores[3].Desactivar("Proveedor de testing inactivo");
            
            // Proveedor multi-categoría
            proveedores[4].AgregarCategoria(CategoriaProveedor.AlimentosBasicos);
            proveedores[4].AgregarCategoria(CategoriaProveedor.BebidasNoAlcoholicas);
            proveedores[4].AgregarCategoria(CategoriaProveedor.Limpieza);

            context.Set<Proveedor>().AddRange(proveedores);
            logger.LogInformation("✅ {Count} proveedores de testing creados", proveedores.Count);
        }

        private async Task SeedMesasTesting(RestauranteProDbContext context, ILogger logger)
        {
            logger.LogInformation("🏠 Creando mesas para testing...");

            var mesas = new List<Mesa>
            {
                // CASO EDGE: Mesa con capacidad muy alta
                Mesa.Crear(999, 50, "Salón Testing Edge"),

                // CASO LÍMITE: Mesa con capacidad mínima
                Mesa.Crear(1000, 1, "Barra Testing"),

                // CASO NORMAL: Mesa estándar
                Mesa.Crear(1001, 4, "Salón Testing Estándar"),

                // CASO ESPECIAL: Mesa fuera de servicio
                Mesa.Crear(1002, 6, "Terraza Testing"),

                // CASO ESPECIAL: Mesa ocupada
                Mesa.Crear(1003, 2, "Privado Testing"),

                // CASO ESPECIAL: Mesa reservada
                Mesa.Crear(1004, 8, "Evento Testing")
            };

            // Configurar estados especiales
            mesas[3].MarcarComoFueraDeServicio("Mesa de testing fuera de servicio");
            mesas[4].MarcarComoOcupada();
            mesas[5].MarcarComoReservada();

            context.Set<Mesa>().AddRange(mesas);
            logger.LogInformation("✅ {Count} mesas de testing creadas", mesas.Count);
        }
    }
} 