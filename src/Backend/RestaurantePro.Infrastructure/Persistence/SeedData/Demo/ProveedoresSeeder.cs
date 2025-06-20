using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Proveedores.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Demo
{
    /// <summary>
    /// Seeder que crea proveedores de demostración con contactos y datos realistas
    /// Usa las entidades reales del sistema y datos típicos de proveedores chilenos
    /// </summary>
    public class ProveedoresSeeder : ISeedData
    {
        public string Name => "Proveedores Demo";
        public int Order => 200;
        public bool IsDevOnly => false;
        public bool IsCritical => false;

        public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("🏭 Iniciando seed de Proveedores Demo...");

                // Verificar si ya existen datos
                if (await ExistsAsync(context, cancellationToken))
                {
                    logger.LogInformation("⏭️ Proveedores Demo ya existen, omitiendo seeding");
                    return;
                }

                var proveedores = new List<Proveedor>
                {
                    // 🥩 Proveedor de Carnes
                    Proveedor.Crear(
                        "Carnes Premium del Norte S.A. de C.V.",
                        "María González Hernández",
                        "ventas@carnespremium.cl",
                        "555-0101",
                        "Av. Ganaderos 234",
                        "Santiago",
                        "7640000",
                        "Chile",
                        "CAR890123456",
                        "Banco Estado - 1122334455",
                        45
                    ),

                    // 🥬 Proveedor de Verduras
                    Proveedor.Crear(
                        "Distribuidora de Verduras Frescas La Huerta",
                        "José Luis Ramírez Castro",
                        "pedidos@lahuerta.cl",
                        "555-0102",
                        "Camino Rural Sur 567",
                        "Valparaíso",
                        "2340000",
                        "Chile",
                        "VER456789012",
                        "BancoChile - 2233445566",
                        30
                    ),

                    // 🥛 Proveedor de Lácteos
                    Proveedor.Crear(
                        "Lácteos y Derivados San Miguel S.A.",
                        "Ana Patricia Morales Vega",
                        "contacto@lacteossanmiguel.cl",
                        "555-0103",
                        "Ruta 5 Sur Km 45",
                        "Concepción",
                        "4070000",
                        "Chile",
                        "LAC234567890",
                        "Santander - 3344556677",
                        15
                    ),

                    // 🍺 Proveedor de Bebidas
                    Proveedor.Crear(
                        "Bebidas y Licores El Barril Dorado",
                        "Roberto Carlos Mendoza Silva",
                        "ventas@barrildorado.cl",
                        "555-0104",
                        "Av. Providencia 890",
                        "Santiago",
                        "7500000",
                        "Chile",
                        "BEB567890123",
                        "BCI - 4455667788",
                        30
                    ),

                    // 🧽 Proveedor de Limpieza
                    Proveedor.Crear(
                        "Productos de Limpieza Profesional CleanMax",
                        "Carmen Elena Jiménez Torres",
                        "pedidos@cleanmax.cl",
                        "555-0105",
                        "Av. Industrial Norte 875",
                        "Temuco",
                        "4810000",
                        "Chile",
                        "CLM345678901",
                        "Banco Estado - 5566778899",
                        20
                    ),

                    // 🍞 Proveedor de Panadería
                    Proveedor.Crear(
                        "Panadería Artesanal Los Andes",
                        "Francisco Javier Ruiz",
                        "ordenes@panaderialosandes.cl",
                        "555-0109",
                        "Calle Independencia 567, Centro",
                        "Valparaíso",
                        "2340000",
                        "Chile",
                        "PAN930818F94",
                        "Scotiabank - Cuenta: 5791346820",
                        3
                    ),

                    // 🌶️ Proveedor de Especias
                    Proveedor.Crear(
                        "Especias y Condimentos del Sur",
                        "Guadalupe Esperanza Flores",
                        "info@especiasdelsur.cl",
                        "555-0110",
                        "Mercado Central, Local 123-125",
                        "Santiago",
                        "8320000",
                        "Chile",
                        "ECT870304G15",
                        "BancoChile - Cuenta: 3691472580",
                        14
                    ),

                    // 🥤 Proveedor de Desechables
                    Proveedor.Crear(
                        "Empaques y Desechables EcoPack Chile",
                        "Miguel Ángel Herrera",
                        "ventas@ecopackchile.cl",
                        "555-0111",
                        "Parque Industrial Quilicura, Nave 15",
                        "Santiago",
                        "8700000",
                        "Chile",
                        "EDS901127H82",
                        "BCI - Cuenta: 7410258369",
                        21
                    ),

                    // 🔧 Proveedor de Equipos
                    Proveedor.Crear(
                        "Equipos de Cocina Profesional RestauTech Chile",
                        "Carlos Eduardo Sánchez",
                        "contacto@restautechcl.cl",
                        "555-0112",
                        "Av. Providencia 1640",
                        "Santiago",
                        "7500000",
                        "Chile",
                        "ECP820615I37",
                        "Santander - Cuenta: 9517534682",
                        45
                    ),



                    // Proveedor 6: Mariscos y Pescados del Pacífico
                    Proveedor.Crear(
                        "Mariscos y Pescados del Pacífico",
                        "Fernando Soto",
                        "ventas@mariscospacifico.cl",
                        "555-0106",
                        "Puerto Pesquero Local 12",
                        "Valparaíso",
                        "2340000",
                        "Chile",
                        "MAR678901234",
                        "BCI - 6677889900",
                        7
                    ),

                    // Proveedor 7: Panadería Artesanal Los Andes
                    Proveedor.Crear(
                        "Panadería Artesanal Los Andes",
                        "Sofía Herrera",
                        "contacto@panaderialosandes.cl",
                        "555-0107",
                        "Calle Artesanos 456",
                        "Concepción",
                        "4070000",
                        "Chile",
                        "PAN789012345",
                        "Santander - 7788990011",
                        5
                    ),

                    // Proveedor 8: Especias y Condimentos del Sur
                    Proveedor.Crear(
                        "Especias y Condimentos del Sur",
                        "Diego Morales",
                        "pedidos@especiasdelsur.cl",
                        "555-0108",
                        "Mercado Central Local 123-125",
                        "Santiago",
                        "8320000",
                        "Chile",
                        "ESP890123456",
                        "BancoChile - 8899001122",
                        14
                    )
                };

                // Agregar contactos adicionales ANTES de guardar
                await AgregarContactosAdicionales(proveedores, logger);

                // Agregar proveedores al contexto
                foreach (var proveedor in proveedores)
                {
                    context.Set<Proveedor>().Add(proveedor);
                }

                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("✅ Seed de Proveedores Demo completado exitosamente. {Count} proveedores creados", proveedores.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error durante el seed de Proveedores Demo");
                throw;
            }
        }

        private async Task AgregarContactosAdicionales(List<Proveedor> proveedores, ILogger logger)
        {
            try
            {
                logger.LogInformation("👥 Agregando contactos adicionales a proveedores...");

                // Carnes Premium - Contacto adicional de ventas
                var carnesPremium = proveedores.First(p => p.Nombre.Contains("Carnes Premium"));
                carnesPremium.AgregarContacto(
                    "Pedro Alejandro García Ruiz",
                    "Ejecutivo de Ventas",
                    "555-0201",
                    "pedro.garcia@carnespremium.cl"
                );

                // La Huerta - Contacto de logística
                var laHuerta = proveedores.First(p => p.Nombre.Contains("La Huerta"));
                laHuerta.AgregarContacto(
                    "Sandra Beatriz López Martínez",
                    "Coordinadora de Logística",
                    "555-0202",
                    "logistica@lahuerta.cl"
                );

                // Lácteos San Miguel - Contacto técnico
                var lacteosSanMiguel = proveedores.First(p => p.Nombre.Contains("San Miguel"));
                lacteosSanMiguel.AgregarContacto(
                    "Dr. Fernando Javier Rodríguez Peña",
                    "Director Técnico",
                    "555-0203",
                    "tecnico@lacteossanmiguel.cl"
                );

                // RestauTech - Contacto de soporte técnico
                var restauTech = proveedores.First(p => p.Nombre.Contains("RestauTech"));
                restauTech.AgregarContacto(
                    "Ing. Diana Cristina Morales Vázquez",
                    "Jefa de Soporte Técnico",
                    "555-0204",
                    "soporte@restautechcl.cl"
                );

                logger.LogInformation("✅ Contactos adicionales agregados exitosamente");
                
                // Retornamos Task completado para mantener la compatibilidad async
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error agregando contactos adicionales");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
        {
            return await context.Set<Proveedor>().AnyAsync(cancellationToken);
        }
    }
} 