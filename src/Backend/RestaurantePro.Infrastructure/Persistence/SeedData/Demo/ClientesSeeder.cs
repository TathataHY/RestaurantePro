using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Demo
{
    /// <summary>
    /// Seeder que crea clientes de demostración con datos realistas
    /// Usa las entidades reales del sistema y datos típicos de clientes chilenos
    /// </summary>
    public class ClientesSeeder : ISeedData
    {
        public string Name => "Clientes Demo";
        public int Order => 210;
        public bool IsDevOnly => false;
        public bool IsCritical => false;

        public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("🏃‍♂️ Iniciando seeding de Clientes Demo...");

                // Verificar si ya existen datos
                if (await ExistsAsync(context, cancellationToken))
                {
                    logger.LogInformation("⏭️ Clientes Demo ya existen, omitiendo seeding");
                    return;
                }

                var clientes = new List<Cliente>
                {
                    // Cliente Premium - Frecuente y alto gasto
                    Cliente.Crear(
                        Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        ClienteNombre.Crear("María Elena", "González Rodríguez"),
                        Email.Create("maria.gonzalez@gmail.com"),
                        PhoneNumber.Create("+52-55-5555-1234"),
                        new DateTime(1985, 3, 15),
                        true
                    ),

                    // Cliente Regular - Patrón estable
                    Cliente.Crear(
                        Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        ClienteNombre.Crear("Carlos Alberto", "Méndez Vázquez"),
                        Email.Create("carlos.mendez@outlook.com"),
                        PhoneNumber.Create("+52-81-8888-5678"),
                        new DateTime(1990, 7, 22),
                        true
                    ),

                    // Cliente Frecuencia Alta - Visita mucho, gasta poco
                    Cliente.Crear(
                        Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        ClienteNombre.Crear("Ana Sofía", "López Martínez"),
                        Email.Create("ana.lopez@hotmail.com"),
                        PhoneNumber.Create("+52-33-3333-9012"),
                        new DateTime(1995, 11, 8),
                        true
                    ),

                    // Cliente Ticket Alto - Visita poco, gasta mucho
                    Cliente.Crear(
                        Guid.Parse("44444444-4444-4444-4444-444444444444"),
                        ClienteNombre.Crear("Roberto", "Fernández Castillo"),
                        Email.Create("roberto.fernandez@empresa.cl"),
                        PhoneNumber.Create("+52-55-5555-3456"),
                        new DateTime(1982, 1, 30),
                        true
                    ),

                    // Cliente Creciente - Aumentando frecuencia
                    Cliente.Crear(
                        Guid.Parse("55555555-5555-5555-5555-555555555555"),
                        ClienteNombre.Crear("Lucía", "Herrera Jiménez"),
                        Email.Create("lucia.herrera@yahoo.com"),
                        PhoneNumber.Create("+52-81-8888-7890"),
                        new DateTime(1988, 9, 12),
                        true
                    ),

                    // Cliente Decreciente - Reduciendo frecuencia
                    Cliente.Crear(
                        Guid.Parse("66666666-6666-6666-6666-666666666666"),
                        ClienteNombre.Crear("Fernando", "Morales Ruiz"),
                        Email.Create("fernando.morales@gmail.com"),
                        PhoneNumber.Create("+52-33-3333-2345"),
                        new DateTime(1975, 5, 18),
                        true
                    ),

                    // Cliente Sin Clasificar - Nuevo
                    Cliente.Crear(
                        Guid.Parse("77777777-7777-7777-7777-777777777777"),
                        ClienteNombre.Crear("Gabriela", "Torres Sánchez"),
                        Email.Create("gabriela.torres@icloud.com"),
                        PhoneNumber.Create("+52-55-5555-6789"),
                        new DateTime(1992, 12, 3),
                        true
                    ),

                    // Cliente Inactivo - No ha visitado recientemente
                    Cliente.Crear(
                        Guid.Parse("88888888-8888-8888-8888-888888888888"),
                        ClienteNombre.Crear("Diego Alejandro", "Ramírez Peña"),
                        Email.Create("diego.ramirez@correo.com"),
                        PhoneNumber.Create("+52-81-8888-0123"),
                        new DateTime(1980, 4, 25),
                        false
                    ),

                    // Cliente Premium Joven
                    Cliente.Crear(
                        Guid.Parse("99999999-9999-9999-9999-999999999999"),
                        ClienteNombre.Crear("Valeria", "Castro Delgado"),
                        Email.Create("valeria.castro@estudiante.cl"),
                        PhoneNumber.Create("+52-33-3333-4567"),
                        new DateTime(1998, 8, 14),
                        true
                    ),

                    // Cliente Regular Corporativo
                    Cliente.Crear(
                        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        ClienteNombre.Crear("Alejandro", "Vargas Mendoza"),
                        Email.Create("alejandro.vargas@corporativo.cl"),
                        PhoneNumber.Create("+52-55-5555-8901"),
                        new DateTime(1987, 6, 9),
                        true
                    )
                };

                // Configurar segmentos y datos específicos
                ConfigurarSegmentosClientes(clientes, logger);

                // Agregar clientes al contexto
                foreach (var cliente in clientes)
                {
                    context.Set<Cliente>().Add(cliente);
                }

                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("✅ Seeding de Clientes Demo completado. {Count} clientes creados", clientes.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error durante el seeding de Clientes Demo");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken)
        {
            return await context.Set<Cliente>().AnyAsync(
                c => c.Id == Guid.Parse("11111111-1111-1111-1111-111111111111"),
                cancellationToken);
        }

        /// <summary>
        /// Configura los segmentos y datos específicos de cada cliente
        /// </summary>
        private void ConfigurarSegmentosClientes(List<Cliente> clientes, ILogger logger)
        {
            try
            {
                // María Elena - Premium (frecuente y alto gasto)
                var maria = clientes[0];
                maria.GetType().GetProperty("Segmento")?.SetValue(maria, SegmentoCliente.Premium);
                maria.GetType().GetProperty("PuntosAcumulados")?.SetValue(maria, 2850);
                maria.GetType().GetProperty("CantidadVisitas")?.SetValue(maria, 45);

                // Carlos Alberto - Regular
                var carlos = clientes[1];
                carlos.GetType().GetProperty("Segmento")?.SetValue(carlos, SegmentoCliente.Regular);
                carlos.GetType().GetProperty("PuntosAcumulados")?.SetValue(carlos, 1200);
                carlos.GetType().GetProperty("CantidadVisitas")?.SetValue(carlos, 28);

                // Ana Sofía - Frecuencia Alta
                var ana = clientes[2];
                ana.GetType().GetProperty("Segmento")?.SetValue(ana, SegmentoCliente.FrecuenciaAlta);
                ana.GetType().GetProperty("PuntosAcumulados")?.SetValue(ana, 890);
                ana.GetType().GetProperty("CantidadVisitas")?.SetValue(ana, 52);

                // Roberto - Ticket Alto
                var roberto = clientes[3];
                roberto.GetType().GetProperty("Segmento")?.SetValue(roberto, SegmentoCliente.TicketAlto);
                roberto.GetType().GetProperty("PuntosAcumulados")?.SetValue(roberto, 1850);
                roberto.GetType().GetProperty("CantidadVisitas")?.SetValue(roberto, 12);

                // Lucía - Creciente
                var lucia = clientes[4];
                lucia.GetType().GetProperty("Segmento")?.SetValue(lucia, SegmentoCliente.Creciente);
                lucia.GetType().GetProperty("PuntosAcumulados")?.SetValue(lucia, 650);
                lucia.GetType().GetProperty("CantidadVisitas")?.SetValue(lucia, 18);

                // Fernando - Decreciente
                var fernando = clientes[5];
                fernando.GetType().GetProperty("Segmento")?.SetValue(fernando, SegmentoCliente.Decreciente);
                fernando.GetType().GetProperty("PuntosAcumulados")?.SetValue(fernando, 320);
                fernando.GetType().GetProperty("CantidadVisitas")?.SetValue(fernando, 8);

                // Gabriela - Sin Clasificar (nueva)
                var gabriela = clientes[6];
                gabriela.GetType().GetProperty("Segmento")?.SetValue(gabriela, SegmentoCliente.SinClasificar);
                gabriela.GetType().GetProperty("PuntosAcumulados")?.SetValue(gabriela, 150);
                gabriela.GetType().GetProperty("CantidadVisitas")?.SetValue(gabriela, 3);

                // Diego - Inactivo
                var diego = clientes[7];
                diego.GetType().GetProperty("Segmento")?.SetValue(diego, SegmentoCliente.Inactivo);
                diego.GetType().GetProperty("PuntosAcumulados")?.SetValue(diego, 480);
                diego.GetType().GetProperty("CantidadVisitas")?.SetValue(diego, 15);

                // Valeria - Premium Joven
                var valeria = clientes[8];
                valeria.GetType().GetProperty("Segmento")?.SetValue(valeria, SegmentoCliente.Premium);
                valeria.GetType().GetProperty("PuntosAcumulados")?.SetValue(valeria, 1950);
                valeria.GetType().GetProperty("CantidadVisitas")?.SetValue(valeria, 35);

                // Alejandro - Regular Corporativo
                var alejandro = clientes[9];
                alejandro.GetType().GetProperty("Segmento")?.SetValue(alejandro, SegmentoCliente.Regular);
                alejandro.GetType().GetProperty("PuntosAcumulados")?.SetValue(alejandro, 1450);
                alejandro.GetType().GetProperty("CantidadVisitas")?.SetValue(alejandro, 32);

                logger.LogInformation("🎯 Segmentos configurados: 2 Premium, 3 Regular, 1 FrecuenciaAlta, 1 TicketAlto, 1 Creciente, 1 Decreciente, 1 Inactivo");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ Error configurando segmentos de clientes (continuando sin segmentos específicos)");
            }
        }
    }
} 