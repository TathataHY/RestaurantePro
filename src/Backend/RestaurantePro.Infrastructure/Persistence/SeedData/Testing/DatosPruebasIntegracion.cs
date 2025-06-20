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
using RestaurantePro.Domain.Proveedores.ValueObjects;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Testing;

public class DatosPruebasIntegracion : ISeedData
{
    public string Name => "Datos Pruebas Integración";
    public int Order => 310;
    public bool IsDevOnly => true;
    public bool IsCritical => false;

    public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
    {
        // Verificar si ya existen datos de integración
        var existenDatos = await context.Usuarios.AnyAsync(u => u.NombreUsuario == "admin-integration", cancellationToken);
        return existenDatos;
    }

    public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("🧪 Iniciando seed de datos para pruebas de integración...");

        try
        {
            // Verificar si ya existen datos
            if (await ExistsAsync(context, cancellationToken))
            {
                logger.LogInformation("⏭️ Datos de Integración ya existen, omitiendo seeding");
                return;
            }

            // ===========================================
            // 1. USUARIOS PARA FLUJOS DE INTEGRACIÓN
            // ===========================================
            var usuarios = new[]
            {
                // Administrador para flujos completos
                Usuario.Crear(
                    "admin-integration",
                    "Administrador Integración",
                    "admin.integration@testing.com",
                    RolUsuario.Administrador
                ),

                // Mesero para flujos de comandas
                Usuario.Crear(
                    "mesero-integration",
                    "Mesero Integración",
                    "mesero.integration@testing.com",
                    RolUsuario.Mesero
                ),

                // Cocinero para flujos de preparación
                Usuario.Crear(
                    "cocinero-integration",
                    "Cocinero Integración",
                    "cocinero.integration@testing.com",
                    RolUsuario.Cocinero
                ),

                // Cajero para flujos de facturación
                Usuario.Crear(
                    "cajero-integration",
                    "Cajero Integración",
                    "cajero.integration@testing.com",
                    RolUsuario.Cajero
                )
            };

            // ===========================================
            // 2. CATEGORÍAS DE PRODUCTOS PARA INTEGRACIÓN
            // ===========================================
            var categorias = new[]
            {
                ProductoCategoria.Crear(
                    "Platos Integración",
                    "Categoría para testing de integración de platos",
                    1
                ),
                ProductoCategoria.Crear(
                    "Bebidas Integración",
                    "Categoría para testing de integración de bebidas",
                    2
                )
            };

            // ===========================================
            // 3. PRODUCTOS INTERCONECTADOS
            // ===========================================
            var productos = new[]
            {
                // Producto complejo con receta
                Producto.Crear(
                    "Hamburguesa Integración Premium",
                    "Hamburguesa premium para testing de flujos completos",
                    new PrecioProducto(250.00m),
                    categorias[0].Id,
                    categorias[0].Nombre
                ),

                // Producto simple para combos
                Producto.Crear(
                    "Refresco Integración",
                    "Refresco para testing de combos",
                    new PrecioProducto(45.00m),
                    categorias[1].Id,
                    categorias[1].Nombre
                ),

                // Producto para testing de inventario
                Producto.Crear(
                    "Ensalada Integración",
                    "Ensalada para testing de control de inventario",
                    new PrecioProducto(120.00m),
                    categorias[0].Id,
                    categorias[0].Nombre
                )
            };

            // ===========================================
            // 4. INGREDIENTES CON RELACIONES
            // ===========================================
            var ingredientes = new[]
            {
                // Ingrediente principal para hamburguesa
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Carne Res Integración",
                    "INTEG-CARNE-001",
                    "Carne de res premium para testing de integración",
                    UnidadMedida.Kilogramo,
                    2.0m,
                    50.0m,
                    RotacionIngrediente.Alta,
                    TemporadaIngrediente.TodoElAño
                ),

                // Ingrediente secundario
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Pan Hamburguesa Integración",
                    "INTEG-PAN-001",
                    "Pan especial para hamburguesas de integración",
                    UnidadMedida.Piezas,
                    10.0m,
                    200.0m,
                    RotacionIngrediente.Media,
                    TemporadaIngrediente.TodoElAño
                ),

                // Ingrediente para ensalada
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Lechuga Integración",
                    "INTEG-LECHUGA-001",
                    "Lechuga fresca para testing de integración",
                    UnidadMedida.Kilogramo,
                    1.0m,
                    25.0m,
                    RotacionIngrediente.Alta,
                    TemporadaIngrediente.TodoElAño
                )
            };

            // ===========================================
            // 5. CLIENTES PARA FLUJOS COMPLETOS
            // ===========================================
            var clientes = new[]
            {
                // Cliente Premium para flujos VIP
                Cliente.Crear(
                    ClienteNombre.Crear("Carlos", "Rodríguez"),
                    "carlos.rodriguez.integration@testing.com",
                    "555-1001",
                    DateTime.Now.AddYears(-25)
                ),

                // Cliente Frecuente para flujos de fidelización
                Cliente.Crear(
                    ClienteNombre.Crear("María", "García"),
                    "maria.garcia.integration@testing.com",
                    "555-1002",
                    DateTime.Now.AddYears(-30)
                ),

                // Cliente Regular para flujos estándar
                Cliente.Crear(
                    ClienteNombre.Crear("José", "Martínez"),
                    "jose.martinez.integration@testing.com",
                    "555-1003",
                    DateTime.Now.AddYears(-28)
                )
            };

            // Configurar segmentos de clientes
            clientes[0].ActualizarSegmento(SegmentoCliente.Premium);
            clientes[1].ActualizarSegmento(SegmentoCliente.FrecuenciaAlta);
            clientes[2].ActualizarSegmento(SegmentoCliente.Regular);

            // ===========================================
            // 6. PROVEEDORES PARA CADENA DE SUMINISTRO
            // ===========================================
            var proveedores = new[]
            {
                // Proveedor principal de carnes
                Proveedor.Crear(
                    "Carnes Premium Integración",
                    "Juan Pérez",
                    "ventas@carnespremium.com",
                    "555-2001",
                    "Av. Ganaderos 123",
                    "Ciudad de México",
                    "12345",
                    "México",
                    "CPI240101ABC",
                    "Cuenta: 1234567890",
                    30
                ),

                // Proveedor de vegetales
                Proveedor.Crear(
                    "Vegetales Frescos Integración",
                    "María González",
                    "ventas@vegetalesfrescos.com",
                    "555-2002",
                    "Carretera Rural Km 45",
                    "Estado de México",
                    "54321",
                    "México",
                    "VFI240101DEF",
                    "Cuenta: 0987654321",
                    15
                )
            };

            // Configurar categorías de proveedores
            proveedores[0].AgregarCategoria(CategoriaProveedor.Carnes);
            proveedores[1].AgregarCategoria(CategoriaProveedor.FrutasVerduras);

            // Agregar contactos a proveedores
            proveedores[0].AgregarContacto(
                "Roberto Carnes",
                "Gerente de Ventas",
                "555-2001",
                "roberto.carnes@carnespremium.com"
            );

            proveedores[1].AgregarContacto(
                "Ana Vegetales",
                "Coordinadora de Distribución",
                "555-2002",
                "ana.vegetales@vegetalesfrescos.com"
            );

            // ===========================================
            // 7. MESAS PARA FLUJOS DE RESERVACIÓN
            // ===========================================
            var mesas = new[]
            {
                // Mesa VIP para clientes premium (números 500+ para evitar conflictos)
                Mesa.Crear(501, 4, "Testing VIP"),
                
                // Mesa estándar para flujos normales
                Mesa.Crear(502, 2, "Integration Area"),
                
                // Mesa familiar para grupos grandes
                Mesa.Crear(503, 6, "Testing Familiar"),
                
                // Mesa de prueba para diferentes estados
                Mesa.Crear(504, 4, "Integration Testing")
            };

            // Configurar estados específicos para testing
            mesas[1].MarcarComoOcupada();
            mesas[3].MarcarComoFueraDeServicio("Mantenimiento para testing de integración");

            // ===========================================
            // PERSISTIR TODAS LAS ENTIDADES
            // ===========================================
            
            await context.Usuarios.AddRangeAsync(usuarios, cancellationToken);
            await context.ProductoCategorias.AddRangeAsync(categorias, cancellationToken);
            await context.Productos.AddRangeAsync(productos, cancellationToken);
            await context.Ingredientes.AddRangeAsync(ingredientes, cancellationToken);
            await context.Clientes.AddRangeAsync(clientes, cancellationToken);
            await context.Proveedores.AddRangeAsync(proveedores, cancellationToken);
            await context.Mesas.AddRangeAsync(mesas, cancellationToken);
            
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("✅ Seed de datos de integración completado exitosamente");
            logger.LogInformation("📊 Datos creados: {Usuarios} usuarios, {Productos} productos, {Ingredientes} ingredientes, {Clientes} clientes, {Proveedores} proveedores, {Mesas} mesas",
                usuarios.Length, productos.Length, ingredientes.Length, clientes.Length, proveedores.Length, mesas.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error durante el seed de datos de integración");
            throw;
        }
    }
} 