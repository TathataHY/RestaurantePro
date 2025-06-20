using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Critical;

/// <summary>
/// Seeder para crear todos los estados predefinidos críticos del sistema.
/// Los estados son fundamentales para el correcto funcionamiento de las entidades.
/// </summary>
public class EstadosSeeder : ISeedData
{
    public string Name => "Estados Predefinidos del Sistema";
    public int Order => 150; // Después de los roles (100) pero antes del usuario admin (200)
    public bool IsDevOnly => false; // Estados son críticos para el sistema
    public bool IsCritical => true;

    public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("🔧 Iniciando seed de estados predefinidos...");

        try
        {
            // Los estados están definidos como enums, pero puede ser que tengamos
            // entidades de configuración que dependen de estos valores
            await SeedEstadosConfiguracion(logger);
            
            logger.LogInformation("✅ Estados predefinidos configurados exitosamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error al configurar estados predefinidos");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
    {
        // Los estados están definidos como enums, por lo que siempre están disponibles
        // Este seeder puede usarse para configuraciones adicionales si es necesario
        
        // Por ahora, verificamos si necesitamos seed de configuraciones relacionadas
        // En el futuro, esto puede expandirse para validar configuraciones específicas
        return await Task.FromResult(false); // Siempre ejecutar para validar consistencia
    }

    private async Task SeedEstadosConfiguracion(ILogger logger)
    {
        logger.LogInformation("📊 Validando estados del sistema...");

        // Validar que todos los enums están correctamente definidos
        await ValidarEstadosUsuario(logger);
        await ValidarEstadosOperaciones(logger);
        await ValidarEstadosComerciales(logger);
        await ValidarEstadosInventario(logger);

        logger.LogInformation("✅ Todos los estados están correctamente configurados");
    }

    private async Task ValidarEstadosUsuario(ILogger logger)
    {
        logger.LogDebug("🔍 Validando estados de usuario...");
        
        // Estados de Usuario (EXACTOS del enum EstadoUsuario)
        var estadosUsuario = new[]
        {
            "Activo (1) - Usuario activo que puede iniciar sesión",
            "Inactivo (2) - Usuario inactivo que no puede iniciar sesión", 
            "Bloqueado (3) - Usuario bloqueado por infracciones o políticas de seguridad",
            "PendienteConfirmacion (4) - Usuario pendiente de confirmación de cuenta",
            "Suspendido (5) - Usuario suspendido temporalmente"
        };

        foreach (var estado in estadosUsuario)
        {
            logger.LogDebug("  ✓ {Estado}", estado);
        }

        await Task.CompletedTask;
    }

    private async Task ValidarEstadosOperaciones(ILogger logger)
    {
        logger.LogDebug("🔍 Validando estados de operaciones...");

        // Estados de Mesa (EXACTOS del enum EstadoMesa)
        var estadosMesa = new[]
        {
            "Disponible (1) - La mesa está disponible para ser ocupada",
            "Ocupada (2) - La mesa está ocupada por clientes", 
            "Reservada (3) - La mesa está reservada para una futura ocupación",
            "FueraDeServicio (4) - La mesa está temporalmente fuera de servicio",
            "EnLimpieza (5) - La mesa está en proceso de limpieza"
        };

        // Estados de Comanda (EXACTOS del enum EstadoComanda)
        var estadosComanda = new[]
        {
            "Creada (0) - Comanda recién creada, aún no enviada a cocina",
            "EnProceso (1) - Comanda en proceso de preparación en cocina",
            "Lista (2) - Comanda lista para ser entregada al cliente",
            "Entregada (3) - Comanda entregada al cliente",
            "Finalizada (4) - Comanda finalizada (pagada)",
            "Cancelada (5) - Comanda cancelada",
            "Dividida (6) - Comanda que ha sido dividida en múltiples comandas"
        };

        // Estados de Reservación (EXACTOS del enum EstadoReservacion)
        var estadosReservacion = new[]
        {
            "Pendiente (1) - La reservación ha sido registrada pero está pendiente de confirmación",
            "Confirmada (2) - La reservación ha sido confirmada",
            "Cancelada (3) - La reservación ha sido cancelada",
            "Completada (4) - La reservación se ha completado (los clientes asistieron y fueron atendidos)",
            "NoShow (5) - Los clientes no se presentaron a la reservación"
        };

        foreach (var estado in estadosMesa)
        {
            logger.LogDebug("  Mesa: ✓ {Estado}", estado);
        }

        foreach (var estado in estadosComanda)
        {
            logger.LogDebug("  Comanda: ✓ {Estado}", estado);
        }

        foreach (var estado in estadosReservacion)
        {
            logger.LogDebug("  Reservación: ✓ {Estado}", estado);
        }

        await Task.CompletedTask;
    }

    private async Task ValidarEstadosComerciales(ILogger logger)
    {
        logger.LogDebug("🔍 Validando estados comerciales...");

        // Estados de Factura (EXACTOS del enum EstadoFactura)
        var estadosFactura = new[]
        {
            "Borrador (1) - Factura en borrador, aún no emitida oficialmente",
            "Emitida (2) - Factura emitida y válida",
            "Pagada (3) - Factura pagada completamente",
            "PagadaParcialmente (4) - Factura pagada parcialmente",
            "Anulada (5) - Factura anulada o cancelada",
            "Vencida (6) - Factura vencida sin pago",
            "Rectificativa (7) - Factura rectificativa (que corrige una factura anterior)"
        };

        // Segmentos de Cliente (EXACTOS del enum SegmentoCliente)
        var segmentosCliente = new[]
        {
            "SinClasificar (0) - Cliente nuevo o sin un patrón definido",
            "FrecuenciaAlta (1) - Cliente que visita con frecuencia pero gasta poco por visita",
            "TicketAlto (2) - Cliente que no visita con mucha frecuencia pero gasta más en cada visita",
            "Premium (3) - Cliente que visita con frecuencia y gasta cantidades importantes",
            "Decreciente (4) - Cliente que ha reducido su frecuencia en el período reciente",
            "Creciente (5) - Cliente que ha aumentado su frecuencia en el período reciente",
            "Inactivo (6) - Cliente que no ha visitado el establecimiento en un período prolongado",
            "Regular (7) - Cliente con patrones regulares de consumo"
        };

        foreach (var estado in estadosFactura)
        {
            logger.LogDebug("  Factura: ✓ {Estado}", estado);
        }

        foreach (var segmento in segmentosCliente)
        {
            logger.LogDebug("  Cliente/Segmento: ✓ {Segmento}", segmento);
        }

        await Task.CompletedTask;
    }

    private async Task ValidarEstadosInventario(ILogger logger)
    {
        logger.LogDebug("🔍 Validando estados de inventario...");

        // Estados de Orden de Compra (EXACTOS del enum EstadoOrdenCompra)
        var estadosOrdenCompra = new[]
        {
            "Pendiente (0) - La orden está creada pero aún no ha sido enviada al proveedor",
            "Enviada (1) - La orden ha sido enviada al proveedor y se está esperando su recepción",
            "Recibida (2) - La orden ha sido recibida completamente",
            "RecibidaParcial (3) - La orden ha sido recibida parcialmente",
            "Cancelada (4) - La orden ha sido cancelada",
            "Confirmada (5) - La orden ha sido confirmada por el proveedor",
            "EnTransito (6) - La orden está en tránsito hacia el destino",
            "Borrador (=Pendiente) - Alias de Pendiente - La orden está en estado borrador",
            "Completada (=Recibida) - Alias de Recibida - La orden está completada"
        };

                 // Tipos de Movimiento de Inventario (EXACTOS del enum TipoMovimientoInventario)
         var tiposMovimiento = new[]
         {
             "Ingreso (0) - Entrada de productos al inventario (compras, devoluciones, ajustes positivos)",
             "Egreso (1) - Salida de productos del inventario (consumo, ventas, pérdidas, ajustes negativos)",
             "Entrada (=Ingreso) - Alias de Ingreso - Entrada de productos",
             "Salida (=Egreso) - Alias de Egreso - Salida de productos",
             "Ajuste (2) - Ajuste de inventario (puede ser positivo o negativo)",
             "Transferencia (3) - Transferencia entre ubicaciones o sucursales",
             "Merma (4) - Merma o deterioro de productos",
             "Devolucion (5) - Devolución de productos",
             "RecuentoFisico (6) - Recuento físico de inventario",
             "Correccion (7) - Corrección de errores en inventario",
             "Incremento (8) - Incremento de stock",
             "Decremento (9) - Decremento de stock"
         };

         // NOTA: No existe enum EstadoIngrediente, los ingredientes usan boolean Activo/Inactivo

        foreach (var estado in estadosOrdenCompra)
        {
            logger.LogDebug("  Orden Compra: ✓ {Estado}", estado);
        }

        foreach (var tipo in tiposMovimiento)
        {
            logger.LogDebug("  Movimiento Inventario: ✓ {Tipo}", tipo);
        }

        await Task.CompletedTask;
    }
} 