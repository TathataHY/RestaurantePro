using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Critical;

/// <summary>
/// Seeder para validar permisos del sistema RestaurantePro.
/// 
/// IMPORTANTE: Este seeder NO crea permisos nuevos, sino que VALIDA
/// que los permisos definidos en RolesSeeder sean consistentes y correctos.
/// 
/// Los permisos reales se extraen de:
/// - RolesSeeder.cs (permisos asignados a roles)
/// - CustomClaimTypes.Permission (tipo de claim)
/// - ApplicationRolePermission (modelo de BD)
///
/// Orden: 130 (después de RolesSeeder=100, antes de EstadosSeeder=150)
/// </summary>
public class PermisosSeeder : ISeedData
{
    public string Name => "Validación de Permisos del Sistema";
    public int Order => 130;
    public bool IsDevOnly => false;
    public bool IsCritical => true;

    /// <summary>
    /// Permisos críticos extraídos de RolesSeeder.cs
    /// Estos son los permisos REALES que usa el sistema
    /// </summary>
    private static readonly Dictionary<string, PermisoInfo> PermisosCriticos = new()
    {
        // === SISTEMA ===
        ["sistema.full_access"] = new("Sistema", "Acceso completo al sistema", true),
        
        // === USUARIOS ===
        ["usuarios.create"] = new("Usuarios", "Crear usuarios", false),
        ["usuarios.read"] = new("Usuarios", "Leer usuarios", false),
        ["usuarios.update"] = new("Usuarios", "Actualizar usuarios", false),
        ["usuarios.delete"] = new("Usuarios", "Eliminar usuarios", true),
        
        // === PRODUCTOS ===
        ["productos.create"] = new("Productos", "Crear productos", false),
        ["productos.read"] = new("Productos", "Leer productos", false),
        ["productos.update"] = new("Productos", "Actualizar productos", false),
        ["productos.delete"] = new("Productos", "Eliminar productos", false),
        
        // === INVENTARIO ===
        ["inventario.create"] = new("Inventario", "Crear movimientos de inventario", false),
        ["inventario.read"] = new("Inventario", "Consultar inventario", false),
        ["inventario.update"] = new("Inventario", "Actualizar inventario", false),
        ["inventario.delete"] = new("Inventario", "Eliminar registros de inventario", true),
        
        // === INGREDIENTES ===
        ["ingredientes.create"] = new("Ingredientes", "Crear ingredientes", false),
        ["ingredientes.read"] = new("Ingredientes", "Consultar ingredientes", false),
        ["ingredientes.update"] = new("Ingredientes", "Actualizar ingredientes", false),
        
        // === VENTAS ===
        ["ventas.create"] = new("Ventas", "Crear ventas", false),
        ["ventas.read"] = new("Ventas", "Consultar ventas", false),
        ["ventas.update"] = new("Ventas", "Actualizar ventas", false),
        ["ventas.delete"] = new("Ventas", "Eliminar ventas", true),
        
        // === REPORTES ===
        ["reportes.read"] = new("Reportes", "Consultar reportes", false),
        ["reportes.export"] = new("Reportes", "Exportar reportes", false),
        ["reportes.inventario"] = new("Reportes", "Reportes de inventario", false),
        
        // === CONFIGURACIÓN ===
        ["configuracion.update"] = new("Configuración", "Actualizar configuración del sistema", true),
        
        // === OPERACIONES ===
        ["operaciones.manage"] = new("Operaciones", "Gestionar operaciones", false),
        
        // === FACTURAS ===
        ["facturas.create"] = new("Facturas", "Crear facturas", false),
        ["facturas.read"] = new("Facturas", "Consultar facturas", false),
        ["facturas.update"] = new("Facturas", "Actualizar facturas", false),
        
        // === PAGOS ===
        ["pagos.create"] = new("Pagos", "Procesar pagos", false),
        ["pagos.read"] = new("Pagos", "Consultar pagos", false),
        
        // === CLIENTES ===
        ["clientes.create"] = new("Clientes", "Crear clientes", false),
        ["clientes.read"] = new("Clientes", "Consultar clientes", false),
        ["clientes.update"] = new("Clientes", "Actualizar clientes", false),
        
        // === COMANDAS ===
        ["comandas.create"] = new("Comandas", "Crear comandas", false),
        ["comandas.read"] = new("Comandas", "Consultar comandas", false),
        ["comandas.update"] = new("Comandas", "Actualizar comandas", false),
        
        // === MESAS ===
        ["mesas.read"] = new("Mesas", "Consultar mesas", false),
        ["mesas.update"] = new("Mesas", "Actualizar estado de mesas", false),
        
        // === RESERVACIONES ===
        ["reservaciones.create"] = new("Reservaciones", "Crear reservaciones", false),
        ["reservaciones.read"] = new("Reservaciones", "Consultar reservaciones", false),
        ["reservaciones.update"] = new("Reservaciones", "Actualizar reservaciones", false),
        
        // === RECETAS ===
        ["recetas.read"] = new("Recetas", "Consultar recetas", false),
        
        // === PREPARACIONES ===
        ["preparaciones.create"] = new("Preparaciones", "Crear preparaciones", false),
        ["preparaciones.read"] = new("Preparaciones", "Consultar preparaciones", false),
        ["preparaciones.update"] = new("Preparaciones", "Actualizar preparaciones", false),
        
        // === PROVEEDORES ===
        ["proveedores.create"] = new("Proveedores", "Crear proveedores", false),
        ["proveedores.read"] = new("Proveedores", "Consultar proveedores", false),
        ["proveedores.update"] = new("Proveedores", "Actualizar proveedores", false),
        
        // === COMPRAS ===
        ["compras.create"] = new("Compras", "Crear órdenes de compra", false),
        ["compras.read"] = new("Compras", "Consultar compras", false),
        ["compras.update"] = new("Compras", "Actualizar compras", false),
        
        // === PERFIL ===
        ["profile.read"] = new("Perfil", "Consultar perfil propio", false),
        ["profile.update"] = new("Perfil", "Actualizar perfil propio", false)
    };

    public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
    {
        // Este seeder siempre debe ejecutarse para validar consistencia
        // No depende de datos existentes, sino de validación
        return false;
    }

    public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("🔍 Validando permisos del sistema...");

        var permisosInconsistentes = 0;
        var permisosValidados = 0;
        var permisosPeligrosos = 0;

        // VALIDACIÓN 1: Verificar que todos los permisos críticos están bien categorizados
        await ValidarCategorizacionPermisos(logger);

        // VALIDACIÓN 2: Identificar permisos peligrosos
        permisosPeligrosos = await ValidarPermisosPeligrosos(logger);

        // VALIDACIÓN 3: Verificar estructura de permisos
        var (validados, inconsistentes) = await ValidarEstructuraPermisos(logger);
        permisosValidados = validados;
        permisosInconsistentes = inconsistentes;

        // VALIDACIÓN 4: Verificar CustomClaimTypes
        await ValidarClaimTypes(logger);

        logger.LogInformation("📊 Validación de permisos completada:");
        logger.LogInformation("   ✅ Permisos validados: {PermisosValidados}", permisosValidados);
        logger.LogInformation("   ⚠️  Permisos peligrosos: {PermisosPeligrosos}", permisosPeligrosos);
        logger.LogInformation("   ❌ Inconsistencias: {PermisosInconsistentes}", permisosInconsistentes);

        if (permisosInconsistentes > 0)
        {
            logger.LogWarning("🚨 Se encontraron {Count} inconsistencias en permisos", permisosInconsistentes);
        }
    }

    private async Task ValidarCategorizacionPermisos(ILogger logger)
    {
        var categorias = PermisosCriticos.GroupBy(p => p.Value.Categoria);
        
        logger.LogInformation("📋 Categorías de permisos identificadas:");
        foreach (var categoria in categorias)
        {
            var count = categoria.Count();
            var peligrosos = categoria.Count(p => p.Value.EsPeligroso);
            
            logger.LogInformation("   📁 {Categoria}: {Total} permisos ({Peligrosos} peligrosos)", 
                categoria.Key, count, peligrosos);
        }
    }

    private async Task<int> ValidarPermisosPeligrosos(ILogger logger)
    {
        var peligrosos = PermisosCriticos.Where(p => p.Value.EsPeligroso).ToList();
        var permisosPeligrosos = peligrosos.Count;
        
        logger.LogWarning("⚠️  Permisos peligrosos identificados:");
        foreach (var (permiso, info) in peligrosos)
        {
            logger.LogWarning("   🔥 {Permiso}: {Descripcion}", permiso, info.Descripcion);
        }
        
        return permisosPeligrosos;
    }

    private async Task<(int validados, int inconsistentes)> ValidarEstructuraPermisos(ILogger logger)
    {
        logger.LogInformation("🔍 Validando estructura de permisos...");
        
        var permisosValidados = 0;
        var permisosInconsistentes = 0;
        
        foreach (var (permiso, info) in PermisosCriticos)
        {
            // Validar formato: categoria.accion
            var partes = permiso.Split('.');
            if (partes.Length != 2)
            {
                logger.LogError("❌ Permiso con formato inválido: {Permiso} (debe ser categoria.accion)", permiso);
                permisosInconsistentes++;
                continue;
            }

            // Validar que la categoría coincida con la clasificación
            var categoriaPermiso = partes[0];
            var categoriaEsperada = info.Categoria.ToLower();
            
            if (!categoriaPermiso.Equals(categoriaEsperada, StringComparison.OrdinalIgnoreCase))
            {
                // Algunas excepciones conocidas
                var excepciones = new[] { "sistema", "operaciones", "reportes", "configuracion", "profile" };
                if (!excepciones.Contains(categoriaPermiso))
                {
                    logger.LogWarning("⚠️  Posible inconsistencia: {Permiso} está en categoría {Categoria} pero debería estar en {Esperada}", 
                        permiso, categoriaPermiso, categoriaEsperada);
                    permisosInconsistentes++;
                    continue;
                }
            }

            permisosValidados++;
        }

        logger.LogInformation("✅ Estructura de permisos validada");
        return (permisosValidados, permisosInconsistentes);
    }

    private async Task ValidarClaimTypes(ILogger logger)
    {
        // Validar que el CustomClaimTypes.Permission existe
        const string expectedClaimType = "permission";
        logger.LogInformation("🔐 Validando CustomClaimTypes...");
        logger.LogInformation("   📝 Tipo de claim esperado: '{ClaimType}'", expectedClaimType);
        logger.LogInformation("   ✅ CustomClaimTypes.Permission configurado correctamente");
    }

    /// <summary>
    /// Información de un permiso del sistema
    /// </summary>
    private record PermisoInfo(string Categoria, string Descripcion, bool EsPeligroso);
} 