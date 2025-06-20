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

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Demo;

public class EscenariosDemoSeeder : ISeedData
{
    public string Name => "Escenarios Completos Demo";
    public int Order => 250;
    public bool IsDevOnly => false;
    public bool IsCritical => false;

    public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
    {
        // Verificar si ya existen datos del escenario
        var existenDatos = await context.Usuarios.AnyAsync(u => u.NombreUsuario == "gerente.general", cancellationToken);
        return existenDatos;
    }

    public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("🎭 Iniciando seed de escenarios completos de demo...");

        try
        {
            // Verificar si ya existen datos
            if (await ExistsAsync(context, cancellationToken))
            {
                logger.LogInformation("⏭️ Escenarios Demo ya existen, omitiendo seeding");
                return;
            }

            // ===========================================
            // ESCENARIO 1: RESTAURANTE "EL BUEN SABOR" - OPERACIÓN COMPLETA
            // ===========================================

            // 👨‍💼 EQUIPO DE TRABAJO COMPLETO
            var equipoTrabajo = new[]
            {
                // Gerente General
                Usuario.Crear(
                    "gerente.general",
                    "Roberto Mendoza García",
                    "gerente@elbuensabor.com",
                    RolUsuario.Administrador
                ),

                // Jefe de Cocina
                Usuario.Crear(
                    "chef.principal",
                    "Carmen Rodríguez Vega",
                    "chef@elbuensabor.com",
                    RolUsuario.Cocinero
                ),

                // Meseros del turno matutino
                Usuario.Crear(
                    "mesero.matutino1",
                    "Luis Alberto Hernández",
                    "luis.mesero@elbuensabor.com",
                    RolUsuario.Mesero
                ),

                Usuario.Crear(
                    "mesero.matutino2",
                    "Ana María Jiménez",
                    "ana.mesero@elbuensabor.com",
                    RolUsuario.Mesero
                ),

                // Cajero principal
                Usuario.Crear(
                    "cajero.principal",
                    "Miguel Ángel Torres",
                    "cajero@elbuensabor.com",
                    RolUsuario.Cajero
                )
            };

            // 🏪 CATEGORÍAS DE MENÚ COMPLETO
            // Verificar categorías existentes
            var categoriasExistentes = await context.ProductoCategorias
                .ToDictionaryAsync(c => c.Nombre, c => c, cancellationToken);

            var categoriasNuevas = new[]
            {
                ("Entradas y Aperitivos", "Deliciosos aperitivos para comenzar", 1),
                ("Platos Fuertes", "Especialidades de la casa", 2),
                ("Postres Caseros", "Dulces tentaciones hechas en casa", 3),
                ("Bebidas Naturales", "Aguas frescas y jugos naturales", 4),
                ("Bebidas Calientes", "Café, té y chocolate", 5),
                ("Cócteles sin Alcohol", "Bebidas especiales de la casa", 6)
            };

            var categoriasMenu = new List<ProductoCategoria>();
            foreach (var (nombre, descripcion, orden) in categoriasNuevas)
            {
                if (categoriasExistentes.ContainsKey(nombre))
                {
                    // Usar la categoría existente
                    categoriasMenu.Add(categoriasExistentes[nombre]);
                    logger.LogDebug("  ⏭️ Usando categoría existente: {Nombre}", nombre);
                }
                else
                {
                    // Crear nueva categoría
                    var nuevaCategoria = ProductoCategoria.Crear(nombre, descripcion, orden);
                    categoriasMenu.Add(nuevaCategoria);
                    context.ProductoCategorias.Add(nuevaCategoria);
                    logger.LogInformation("  ✅ Creando nueva categoría: {Nombre}", nombre);
                }
            }

            // 🍽️ MENÚ COMPLETO DEL RESTAURANTE
            var menuCompleto = new[]
            {
                // === ENTRADAS Y APERITIVOS ===
                Producto.Crear(
                    "Nachos El Buen Sabor",
                    "Totopos caseros con queso derretido, jalapeños, frijoles y guacamole",
                    new PrecioProducto(145.00m),
                    categoriasMenu[0].Id,
                    categoriasMenu[0].Nombre
                ),

                Producto.Crear(
                    "Quesadillas de Flor de Calabaza",
                    "Tortillas de maíz rellenas de flor de calabaza y queso Oaxaca",
                    new PrecioProducto(95.00m),
                    categoriasMenu[0].Id,
                    categoriasMenu[0].Nombre
                ),

                // === PLATOS FUERTES ===
                Producto.Crear(
                    "Mole Poblano de la Casa",
                    "Pollo en mole poblano tradicional con arroz mexicano y tortillas",
                    new PrecioProducto(285.00m),
                    categoriasMenu[1].Id,
                    categoriasMenu[1].Nombre
                ),

                Producto.Crear(
                    "Arrachera a la Parrilla",
                    "Carne de res marinada con guacamole, frijoles charros y tortillas",
                    new PrecioProducto(320.00m),
                    categoriasMenu[1].Id,
                    categoriasMenu[1].Nombre
                ),

                Producto.Crear(
                    "Pescado a la Veracruzana",
                    "Huachinango fresco en salsa veracruzana con arroz blanco",
                    new PrecioProducto(275.00m),
                    categoriasMenu[1].Id,
                    categoriasMenu[1].Nombre
                ),

                // === POSTRES CASEROS ===
                Producto.Crear(
                    "Flan Napolitano de la Abuela",
                    "Flan casero con caramelo y crema batida",
                    new PrecioProducto(75.00m),
                    categoriasMenu[2].Id,
                    categoriasMenu[2].Nombre
                ),

                Producto.Crear(
                    "Tres Leches Especial",
                    "Pastel tres leches con fresas naturales",
                    new PrecioProducto(85.00m),
                    categoriasMenu[2].Id,
                    categoriasMenu[2].Nombre
                ),

                // === BEBIDAS ===
                Producto.Crear(
                    "Agua Fresca de Horchata",
                    "Horchata de arroz con canela, servida bien fría",
                    new PrecioProducto(45.00m),
                    categoriasMenu[3].Id,
                    categoriasMenu[3].Nombre
                ),

                Producto.Crear(
                    "Café de Olla Tradicional",
                    "Café de olla con canela y piloncillo",
                    new PrecioProducto(35.00m),
                    categoriasMenu[4].Id,
                    categoriasMenu[4].Nombre
                ),

                Producto.Crear(
                    "Cóctel de Frutas Tropicales",
                    "Mezcla de frutas tropicales con chile y limón",
                    new PrecioProducto(65.00m),
                    categoriasMenu[5].Id,
                    categoriasMenu[5].Nombre
                )
            };

            // 🥘 INGREDIENTES PRINCIPALES CON INVENTARIO REALISTA
            var ingredientesPrincipales = new[]
            {
                // Carnes
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Pollo Entero Fresco",
                    "DEMO-POLLO-001",
                    "Pollo fresco de granja para mole y otros platillos",
                    UnidadMedida.Kilogramo,
                    5.0m,
                    45.0m,
                    RotacionIngrediente.Alta,
                    TemporadaIngrediente.TodoElAño
                ),

                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Arrachera Premium",
                    "DEMO-ARRACHERA-001",
                    "Carne de res premium para parrilla",
                    UnidadMedida.Kilogramo,
                    2.0m,
                    18.0m,
                    RotacionIngrediente.Alta,
                    TemporadaIngrediente.TodoElAño
                ),

                // Pescados y Mariscos
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Huachinango Fresco",
                    "DEMO-HUACHINANGO-001",
                    "Huachinango fresco del Golfo de México",
                    UnidadMedida.Kilogramo,
                    1.0m,
                    12.0m,
                    RotacionIngrediente.Critica,
                    TemporadaIngrediente.TodoElAño
                ),

                // Vegetales y Verduras
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Flor de Calabaza",
                    "DEMO-FLOR-001",
                    "Flor de calabaza fresca para quesadillas",
                    UnidadMedida.Kilogramo,
                    0.5m,
                    8.0m,
                    RotacionIngrediente.Critica,
                    TemporadaIngrediente.Verano
                ),

                // Lácteos
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Queso Oaxaca Artesanal",
                    "DEMO-QUESO-001",
                    "Queso Oaxaca artesanal para quesadillas",
                    UnidadMedida.Kilogramo,
                    1.0m,
                    15.0m,
                    RotacionIngrediente.Media,
                    TemporadaIngrediente.TodoElAño
                ),

                // Especias y Condimentos
                Ingrediente.Crear(
                    Guid.NewGuid(),
                    "Chile Mulato para Mole",
                    "DEMO-CHILE-001",
                    "Chile mulato seco para preparación de mole",
                    UnidadMedida.Kilogramo,
                    0.2m,
                    5.0m,
                    RotacionIngrediente.Baja,
                    TemporadaIngrediente.TodoElAño
                )
            };

            // 👥 CLIENTES REGULARES DEL RESTAURANTE
            var clientesRegulares = new[]
            {
                // Familia Martínez - Clientes VIP
                Cliente.Crear(
                    ClienteNombre.Crear("Fernando", "Martínez"),
                    "fernando.martinez@email.com",
                    "555-0101",
                    DateTime.Now.AddYears(-45)
                ),

                // Pareja joven - Clientes frecuentes
                Cliente.Crear(
                    ClienteNombre.Crear("Sofía", "Ramírez"),
                    "sofia.ramirez@email.com",
                    "555-0102",
                    DateTime.Now.AddYears(-25)
                ),

                // Empresario local - Cliente premium
                Cliente.Crear(
                    ClienteNombre.Crear("Ricardo", "Vega"),
                    "ricardo.vega@email.com",
                    "555-0103",
                    DateTime.Now.AddYears(-35)
                ),

                // Familia numerosa - Clientes regulares
                Cliente.Crear(
                    ClienteNombre.Crear("Patricia", "González"),
                    "patricia.gonzalez@email.com",
                    "555-0104",
                    DateTime.Now.AddYears(-40)
                ),

                // Estudiantes universitarios - Clientes jóvenes
                Cliente.Crear(
                    ClienteNombre.Crear("Alejandro", "Moreno"),
                    "alejandro.moreno@email.com",
                    "555-0105",
                    DateTime.Now.AddYears(-20)
                )
            };

            // Configurar segmentos de clientes
            clientesRegulares[0].ActualizarSegmento(SegmentoCliente.Premium);
            clientesRegulares[1].ActualizarSegmento(SegmentoCliente.FrecuenciaAlta);
            clientesRegulares[2].ActualizarSegmento(SegmentoCliente.Premium);
            clientesRegulares[3].ActualizarSegmento(SegmentoCliente.Regular);
            clientesRegulares[4].ActualizarSegmento(SegmentoCliente.Regular);

            // 🚚 RED DE PROVEEDORES LOCALES
            var proveedoresLocales = new[]
            {
                // Proveedor principal de carnes
                Proveedor.Crear(
                    "Carnicería San Miguel",
                    "Miguel Hernández",
                    "miguel@carniceriasanmiguel.com",
                    "555-3001",
                    "Mercado Central Local 45-47",
                    "Centro",
                    "45000",
                    "México",
                    "CSM240101XYZ",
                    "Banco Azteca - Cuenta: 1234567890",
                    30
                ),

                // Proveedor de pescados y mariscos
                Proveedor.Crear(
                    "Mariscos Frescos del Golfo",
                    "Ana Martínez",
                    "ana@mariscosdelgolfo.com",
                    "555-3002",
                    "Puerto Pesquero Km 12",
                    "Zona Costera",
                    "45100",
                    "México",
                    "MFG240101ABC",
                    "Bancomer - Cuenta: 2345678901",
                    15
                ),

                // Proveedor de vegetales locales
                Proveedor.Crear(
                    "Huerto Orgánico La Esperanza",
                    "José González",
                    "jose@huertoesperanza.com",
                    "555-3003",
                    "Zona Rural Km 25",
                    "Ejido La Esperanza",
                    "45200",
                    "México",
                    "HOE240101DEF",
                    "Banamex - Cuenta: 3456789012",
                    20
                ),

                // Proveedor de lácteos artesanales
                Proveedor.Crear(
                    "Lácteos Artesanales Don José",
                    "Carmen López",
                    "carmen@lacteosdonjose.com",
                    "555-3004",
                    "Rancho Lechero Km 18",
                    "Carretera Norte",
                    "45300",
                    "México",
                    "LAD240101GHI",
                    "Santander - Cuenta: 4567890123",
                    25
                )
            };

            // Configurar categorías de proveedores
            proveedoresLocales[0].AgregarCategoria(CategoriaProveedor.Carnes);
            proveedoresLocales[1].AgregarCategoria(CategoriaProveedor.Carnes);
            proveedoresLocales[2].AgregarCategoria(CategoriaProveedor.FrutasVerduras);
            proveedoresLocales[3].AgregarCategoria(CategoriaProveedor.Lacteos);

            // Los contactos ya están incluidos en la creación de cada proveedor

            // 🪑 DISTRIBUCIÓN COMPLETA DEL RESTAURANTE
            var distribucionMesas = new[]
            {
                // === TERRAZA PRINCIPAL (Vista al jardín) ===
                Mesa.Crear(101, 2, "Terraza Principal"),
                Mesa.Crear(102, 2, "Terraza Principal"),
                Mesa.Crear(103, 4, "Terraza Principal"),
                Mesa.Crear(104, 4, "Terraza Principal"),
                Mesa.Crear(105, 6, "Terraza Principal"),

                // === SALÓN INTERIOR (Ambiente familiar) ===
                Mesa.Crear(201, 2, "Salón Interior"),
                Mesa.Crear(202, 4, "Salón Interior"),
                Mesa.Crear(203, 4, "Salón Interior"),
                Mesa.Crear(204, 6, "Salón Interior"),
                Mesa.Crear(205, 8, "Salón Interior"),

                // === ÁREA VIP (Ambiente exclusivo) ===
                Mesa.Crear(301, 4, "Área VIP"),
                Mesa.Crear(302, 6, "Área VIP"),

                // === BARRA (Ambiente casual) ===
                Mesa.Crear(401, 2, "Barra"),
                Mesa.Crear(402, 2, "Barra"),
                Mesa.Crear(403, 2, "Barra")
            };

            // Simular algunas mesas ocupadas en horario pico
            distribucionMesas[2].MarcarComoOcupada(); // Mesa 103 - Familia
            distribucionMesas[6].MarcarComoOcupada(); // Mesa 202 - Pareja
            distribucionMesas[10].MarcarComoOcupada(); // Mesa 301 - VIP
            distribucionMesas[12].MarcarComoOcupada(); // Mesa 401 - Barra

            // Una mesa fuera de servicio por mantenimiento
            distribucionMesas[9].MarcarComoFueraDeServicio("Mantenimiento de sillas"); // Mesa 205

            // ===========================================
            // PERSISTIR TODOS LOS DATOS DEL ESCENARIO
            // ===========================================
            
            await context.Usuarios.AddRangeAsync(equipoTrabajo, cancellationToken);
            // Las categorías ya se agregaron individualmente si eran nuevas
            await context.Productos.AddRangeAsync(menuCompleto, cancellationToken);
            await context.Ingredientes.AddRangeAsync(ingredientesPrincipales, cancellationToken);
            await context.Clientes.AddRangeAsync(clientesRegulares, cancellationToken);
            await context.Proveedores.AddRangeAsync(proveedoresLocales, cancellationToken);
            await context.Mesas.AddRangeAsync(distribucionMesas, cancellationToken);
            
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("✅ Escenarios completos de demo creados exitosamente");
            logger.LogInformation("🎭 Restaurante 'El Buen Sabor' configurado completamente:");
            logger.LogInformation("👥 {Equipo} miembros del equipo", equipoTrabajo.Length);
            logger.LogInformation("📋 {Categorias} categorías de menú", categoriasMenu.Count);
            logger.LogInformation("🍽️ {Menu} productos en el menú", menuCompleto.Length);
            logger.LogInformation("🥘 {Ingredientes} ingredientes principales", ingredientesPrincipales.Length);
            logger.LogInformation("👨‍👩‍👧‍👦 {Clientes} clientes regulares", clientesRegulares.Length);
            logger.LogInformation("🚚 {Proveedores} proveedores locales", proveedoresLocales.Length);
            logger.LogInformation("🪑 {Mesas} mesas distribuidas", distribucionMesas.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error durante la creación de escenarios de demo");
            throw;
        }
    }
} 