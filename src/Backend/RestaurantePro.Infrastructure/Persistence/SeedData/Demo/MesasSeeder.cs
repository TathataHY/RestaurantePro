using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Demo
{
    /// <summary>
    /// Seeder que crea mesas de demostración con distribución realista
    /// Usa las entidades reales del sistema y simula un restaurante típico chileno
    /// </summary>
    public class MesasSeeder : ISeedData
    {
        public string Name => "Mesas Demo";
        public int Order => 220;
        public bool IsDevOnly => false;
        public bool IsCritical => false;

        public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("🏠 Iniciando seeding de Mesas Demo...");

                // Verificar si ya existen datos
                if (await ExistsAsync(context, cancellationToken))
                {
                    logger.LogInformation("⏭️ Mesas Demo ya existen, omitiendo seeding");
                    return;
                }

                var mesas = new List<Mesa>
                {
                    // === ÁREA INTERIOR - MESAS PRINCIPALES ===
                    Mesa.Crear(1, 2, "Interior"),
                    Mesa.Crear(2, 2, "Interior"),
                    Mesa.Crear(3, 4, "Interior"),
                    Mesa.Crear(4, 4, "Interior"),
                    Mesa.Crear(5, 4, "Interior"),
                    Mesa.Crear(6, 6, "Interior"),
                    Mesa.Crear(7, 6, "Interior"),
                    Mesa.Crear(8, 8, "Interior"),

                    // === ÁREA TERRAZA - AMBIENTE EXTERIOR ===
                    Mesa.Crear(9, 2, "Terraza"),
                    Mesa.Crear(10, 2, "Terraza"),
                    Mesa.Crear(11, 4, "Terraza"),
                    Mesa.Crear(12, 4, "Terraza"),
                    Mesa.Crear(13, 6, "Terraza"),
                    Mesa.Crear(14, 6, "Terraza"),

                    // === ÁREA VIP - SECCIÓN EXCLUSIVA ===
                    Mesa.Crear(15, 4, "VIP"),
                    Mesa.Crear(16, 6, "VIP"),
                    Mesa.Crear(17, 8, "VIP"),

                    // === ÁREA VENTANA - VISTA PRIVILEGIADA ===
                    Mesa.Crear(18, 2, "Ventana"),
                    Mesa.Crear(19, 4, "Ventana"),
                    Mesa.Crear(20, 4, "Ventana"),

                    // === ÁREA PRIVADA - RESERVACIONES ESPECIALES ===
                    Mesa.Crear(21, 10, "Privada"),
                    Mesa.Crear(22, 12, "Privada"),

                    // === BARRA - ÁREA CASUAL ===
                    Mesa.Crear(23, 1, "Barra"),
                    Mesa.Crear(24, 1, "Barra"),
                    Mesa.Crear(25, 1, "Barra"),
                    Mesa.Crear(26, 1, "Barra"),
                    Mesa.Crear(27, 2, "Barra"),
                    Mesa.Crear(28, 2, "Barra"),

                    // === MESAS ADICIONALES PARA COMPLETAR ===
                    Mesa.Crear(29, 4, "Interior"),
                    Mesa.Crear(30, 4, "Interior"),
                    Mesa.Crear(31, 6, "Interior"),
                    Mesa.Crear(32, 6, "Interior"),
                    Mesa.Crear(33, 2, "Terraza"),
                    Mesa.Crear(34, 2, "Terraza"),
                    Mesa.Crear(35, 4, "Terraza"),
                    Mesa.Crear(36, 4, "Terraza"),
                    Mesa.Crear(37, 2, "Ventana"),
                    Mesa.Crear(38, 4, "Ventana"),
                    Mesa.Crear(39, 4, "Ventana"),
                    Mesa.Crear(40, 8, "VIP"),
                    Mesa.Crear(41, 8, "VIP"),
                    Mesa.Crear(42, 10, "Privada"),
                    Mesa.Crear(43, 12, "Privada")
                };

                // Configurar estados realistas de las mesas
                ConfigurarEstadosMesas(mesas, logger);

                // Agregar mesas al contexto
                foreach (var mesa in mesas)
                {
                    context.Set<Mesa>().Add(mesa);
                }

                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("✅ Seeding de Mesas Demo completado. {Count} mesas creadas", mesas.Count);
                logger.LogInformation("📊 Distribución: Interior({Interior}), Terraza({Terraza}), VIP({VIP}), Ventana({Ventana}), Privada({Privada}), Barra({Barra})",
                    mesas.Count(m => m.Ubicacion == "Interior"),
                    mesas.Count(m => m.Ubicacion == "Terraza"),
                    mesas.Count(m => m.Ubicacion == "VIP"),
                    mesas.Count(m => m.Ubicacion == "Ventana"),
                    mesas.Count(m => m.Ubicacion == "Privada"),
                    mesas.Count(m => m.Ubicacion == "Barra"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error durante el seeding de Mesas Demo");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken)
        {
            return await context.Set<Mesa>().AnyAsync(
                m => m.Numero == 1 && m.Ubicacion == "Interior",
                cancellationToken);
        }

        /// <summary>
        /// Configura estados realistas para simular un restaurante en operación
        /// </summary>
        private void ConfigurarEstadosMesas(List<Mesa> mesas, ILogger logger)
        {
            try
            {
                // Simulamos horario de almuerzo - algunas mesas ocupadas
                var mesasOcupadas = new[] { 3, 5, 12, 14, 20, 31, 25, 26 };
                foreach (var numeroMesa in mesasOcupadas)
                {
                    var mesa = mesas.FirstOrDefault(m => m.Numero == numeroMesa);
                    if (mesa != null)
                    {
                        mesa.MarcarComoOcupada();
                    }
                }

                // Algunas mesas reservadas para la cena
                var mesasReservadas = new[] { 6, 7, 21, 22, 40 };
                foreach (var numeroMesa in mesasReservadas)
                {
                    var mesa = mesas.FirstOrDefault(m => m.Numero == numeroMesa);
                    if (mesa != null)
                    {
                        mesa.MarcarComoReservada();
                    }
                }

                // Una mesa en limpieza (recién liberada)
                var mesaLimpieza = mesas.FirstOrDefault(m => m.Numero == 4);
                if (mesaLimpieza != null)
                {
                    mesaLimpieza.MarcarComoFueraDeServicio("En proceso de limpieza");
                }

                // Una mesa fuera de servicio (mantenimiento)
                var mesaFueraServicio = mesas.FirstOrDefault(m => m.Numero == 28);
                if (mesaFueraServicio != null)
                {
                    mesaFueraServicio.MarcarComoFueraDeServicio("Mantenimiento silla");
                }

                // El resto permanecen disponibles (estado por defecto)

                logger.LogInformation("🎯 Estados configurados - Ocupadas: {Ocupadas}, Reservadas: {Reservadas}, Limpieza: {Limpieza}, Fuera Servicio: {FueraServicio}",
                    mesasOcupadas.Length,
                    mesasReservadas.Length,
                    1,
                    1);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ Error configurando estados de mesas, usando estados por defecto");
            }
        }
    }
} 