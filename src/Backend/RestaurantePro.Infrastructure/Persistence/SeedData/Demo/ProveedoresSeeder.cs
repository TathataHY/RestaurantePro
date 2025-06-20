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
    /// Usa las entidades reales del sistema y datos típicos de proveedores mexicanos
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
                        "ventas@carnespremium.mx",
                        "+52-81-8888-1234",
                        "Av. Industrial 1250, Zona Industrial",
                        "Monterrey",
                        "64700",
                        "México",
                        "CPN850315A47",
                        "BBVA Bancomer - Cuenta: 0123456789",
                        15
                    ),

                    // 🥬 Proveedor de Verduras
                    Proveedor.Crear(
                        "Distribuidora de Verduras Frescas La Huerta",
                        "José Luis Ramírez Castro",
                        "pedidos@lahuerta.com.mx",
                        "+52-33-3456-7890",
                        "Mercado de Abastos Local 45-47",
                        "Guadalajara",
                        "44100",
                        "México",
                        "DVF920708B23",
                        "Santander - Cuenta: 9876543210",
                        7
                    ),

                    // 🥛 Proveedor de Lácteos
                    Proveedor.Crear(
                        "Lácteos y Derivados San Miguel S.A.",
                        "Ana Patricia Morales Vega",
                        "contacto@lacteossanmiguel.mx",
                        "+52-55-5555-2468",
                        "Carretera Federal México-Querétaro Km 45",
                        "Tepotzotlán",
                        "54600",
                        "México",
                        "LDS780420C89",
                        "Banorte - Cuenta: 1357924680",
                        10
                    ),

                    // 🍺 Proveedor de Bebidas
                    Proveedor.Crear(
                        "Bebidas y Licores El Barril Dorado",
                        "Roberto Carlos Mendoza Silva",
                        "ventas@barrildorado.mx",
                        "+52-55-1234-5678",
                        "Av. Revolución 2890, Col. San Ángel",
                        "Ciudad de México",
                        "01000",
                        "México",
                        "BLD941212D56",
                        "HSBC - Cuenta: 2468135790",
                        30
                    ),

                    // 🧽 Proveedor de Limpieza
                    Proveedor.Crear(
                        "Productos de Limpieza Profesional CleanMax",
                        "Carmen Elena Jiménez Torres",
                        "pedidos@cleanmax.com.mx",
                        "+52-81-9999-3333",
                        "Blvd. Díaz Ordaz 1875, Santa María",
                        "Monterrey",
                        "64650",
                        "México",
                        "PLP860925E71",
                        "Banamex - Cuenta: 8024681357",
                        20
                    ),

                    // 🍞 Proveedor de Panadería
                    Proveedor.Crear(
                        "Panadería Artesanal Don Pancho",
                        "Francisco Javier Ruiz Domínguez",
                        "ordenes@donpancho.mx",
                        "+52-33-7777-8888",
                        "Calle Independencia 567, Centro Histórico",
                        "Guadalajara",
                        "44100",
                        "México",
                        "PAD930818F94",
                        "Scotiabank - Cuenta: 5791346820",
                        3
                    ),

                    // 🌶️ Proveedor de Especias
                    Proveedor.Crear(
                        "Especias y Condimentos Tradición Mexicana",
                        "Guadalupe Esperanza Flores Mendoza",
                        "info@tradicionmexicana.mx",
                        "+52-55-8888-9999",
                        "Mercado de San Juan, Local 123-125",
                        "Ciudad de México",
                        "06050",
                        "México",
                        "ECT870304G15",
                        "Banco Azteca - Cuenta: 3691472580",
                        14
                    ),

                    // 🥤 Proveedor de Desechables
                    Proveedor.Crear(
                        "Empaques y Desechables EcoPack Solutions",
                        "Miguel Ángel Herrera López",
                        "ventas@ecopacksolutions.mx",
                        "+52-81-6666-7777",
                        "Parque Industrial Apodaca, Nave 15",
                        "Apodaca",
                        "66600",
                        "México",
                        "EDS901127H82",
                        "Inbursa - Cuenta: 7410258369",
                        21
                    ),

                    // 🔧 Proveedor de Equipos
                    Proveedor.Crear(
                        "Equipos de Cocina Profesional RestauTech",
                        "Carlos Eduardo Sánchez Moreno",
                        "contacto@restautech.mx",
                        "+52-33-4444-5555",
                        "Av. Américas 1640, Providencia",
                        "Guadalajara",
                        "44630",
                        "México",
                        "ECP820615I37",
                        "Citibanamex - Cuenta: 9517534682",
                        45
                    ),

                    // 🐟 Proveedor Local de Mariscos
                    Proveedor.Crear(
                        "Mariscos Frescos del Pacífico",
                        "Raúl Octavio Castillo Rivera",
                        "pedidos@mariscospacifico.mx",
                        "+52-33-2222-3333",
                        "Av. López Mateos Sur 2500",
                        "Guadalajara",
                        "45050",
                        "México",
                        "MFP890523J64",
                        "Banregio - Cuenta: 1593574680",
                        2
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
                    "+52-81-8888-1235",
                    "pedro.garcia@carnespremium.mx"
                );

                // La Huerta - Contacto de logística
                var laHuerta = proveedores.First(p => p.Nombre.Contains("La Huerta"));
                laHuerta.AgregarContacto(
                    "Sandra Beatriz López Martínez",
                    "Coordinadora de Logística",
                    "+52-33-3456-7891",
                    "logistica@lahuerta.com.mx"
                );

                // Lácteos San Miguel - Contacto técnico
                var lacteosSanMiguel = proveedores.First(p => p.Nombre.Contains("San Miguel"));
                lacteosSanMiguel.AgregarContacto(
                    "Dr. Fernando Javier Rodríguez Peña",
                    "Director Técnico",
                    "+52-55-5555-2469",
                    "tecnico@lacteossanmiguel.mx"
                );

                // RestauTech - Contacto de soporte técnico
                var restauTech = proveedores.First(p => p.Nombre.Contains("RestauTech"));
                restauTech.AgregarContacto(
                    "Ing. Diana Cristina Morales Vázquez",
                    "Jefa de Soporte Técnico",
                    "+52-33-4444-5556",
                    "soporte@restautech.mx"
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