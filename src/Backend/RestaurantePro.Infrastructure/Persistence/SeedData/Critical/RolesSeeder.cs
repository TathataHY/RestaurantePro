using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Critical;

/// <summary>
/// Seeder para los roles críticos del sistema
/// </summary>
public class RolesSeeder : ISeedData
{
    public string Name => "Roles del Sistema";
    public int Order => 1; // Primer orden - los roles son fundamentales
    public bool IsDevOnly => false;
    public bool IsCritical => true;

    /// <summary>
    /// Roles críticos del sistema
    /// </summary>
    private static readonly Dictionary<TipoUsuario, RoleDefinition> RolesCriticos = new()
    {
        [TipoUsuario.Administrador] = new RoleDefinition
        {
            Name = "Administrador",
            NormalizedName = "ADMINISTRADOR",
            Description = "Acceso total al sistema",
            Permissions = new[]
            {
                "sistema.full_access",
                "usuarios.create", "usuarios.read", "usuarios.update", "usuarios.delete",
                "productos.create", "productos.read", "productos.update", "productos.delete",
                "inventario.create", "inventario.read", "inventario.update", "inventario.delete",
                "ventas.create", "ventas.read", "ventas.update", "ventas.delete",
                "reportes.read", "reportes.export",
                "configuracion.update"
            }
        },
        [TipoUsuario.Gerente] = new RoleDefinition
        {
            Name = "Gerente",
            NormalizedName = "GERENTE",
            Description = "Gestión operativa y reportes",
            Permissions = new[]
            {
                "usuarios.read", "usuarios.update",
                "productos.create", "productos.read", "productos.update",
                "inventario.create", "inventario.read", "inventario.update",
                "ventas.read", "ventas.update",
                "reportes.read", "reportes.export",
                "operaciones.manage"
            }
        },
        [TipoUsuario.Cajero] = new RoleDefinition
        {
            Name = "Cajero",
            NormalizedName = "CAJERO",
            Description = "Facturación y pagos",
            Permissions = new[]
            {
                "ventas.create", "ventas.read", "ventas.update",
                "facturas.create", "facturas.read", "facturas.update",
                "pagos.create", "pagos.read",
                "productos.read",
                "clientes.create", "clientes.read", "clientes.update"
            }
        },
        [TipoUsuario.Mesero] = new RoleDefinition
        {
            Name = "Mesero",
            NormalizedName = "MESERO",
            Description = "Atención al cliente y comandas",
            Permissions = new[]
            {
                "comandas.create", "comandas.read", "comandas.update",
                "mesas.read", "mesas.update",
                "productos.read",
                "clientes.read",
                "reservaciones.create", "reservaciones.read", "reservaciones.update"
            }
        },
        [TipoUsuario.Cocinero] = new RoleDefinition
        {
            Name = "Cocinero",
            NormalizedName = "COCINERO",
            Description = "Preparación de alimentos",
            Permissions = new[]
            {
                "comandas.read", "comandas.update",
                "productos.read",
                "recetas.read",
                "preparaciones.create", "preparaciones.read", "preparaciones.update",
                "inventario.read"
            }
        },
        [TipoUsuario.EncargadoInventario] = new RoleDefinition
        {
            Name = "EncargadoInventario",
            NormalizedName = "ENCARGADOINVENTARIO",
            Description = "Control de stock y compras",
            Permissions = new[]
            {
                "inventario.create", "inventario.read", "inventario.update",
                "ingredientes.create", "ingredientes.read", "ingredientes.update",
                "proveedores.create", "proveedores.read", "proveedores.update",
                "compras.create", "compras.read", "compras.update",
                "reportes.inventario"
            }
        },
        [TipoUsuario.Empleado] = new RoleDefinition
        {
            Name = "Empleado",
            NormalizedName = "EMPLEADO",
            Description = "Acceso básico al sistema",
            Permissions = new[]
            {
                "productos.read",
                "clientes.read",
                "profile.read", "profile.update"
            }
        }
    };

    public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
    {
        // Verificar si TODOS los roles críticos existen
        var allCriticalRoles = RolesCriticos.Values.Select(r => r.NormalizedName).ToArray();
        var existingRoles = await context.Set<ApplicationRole>()
            .Where(r => allCriticalRoles.Contains(r.NormalizedName))
            .CountAsync(cancellationToken);
        return existingRoles >= allCriticalRoles.Length;
    }

    public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("🔐 Sembrando roles críticos del sistema...");

        var rolesCreados = 0;
        var rolesActualizados = 0;

        foreach (var (tipoUsuario, roleDefinition) in RolesCriticos)
        {
            try
            {
                // Buscar rol existente
                var existingRole = await context.Set<ApplicationRole>()
                    .FirstOrDefaultAsync(r => r.NormalizedName == roleDefinition.NormalizedName, cancellationToken);

                if (existingRole == null)
                {
                    // Crear nuevo rol
                    var newRole = new ApplicationRole
                    {
                        Id = GenerateRoleId(tipoUsuario),
                        Name = roleDefinition.Name,
                        NormalizedName = roleDefinition.NormalizedName,
                        Description = roleDefinition.Description,
                        IsSystemRole = true,
                        CreatedOn = DateTime.UtcNow
                    };

                    context.Set<ApplicationRole>().Add(newRole);
                    rolesCreados++;
                    
                    logger.LogInformation("✅ Rol creado: {RoleName}", roleDefinition.Name);
                }
                else
                {
                    // Actualizar rol existente si es necesario
                    var descripcionActual = existingRole.Description ?? "";
                    var descripcionNueva = roleDefinition.Description;

                    if (!descripcionActual.Equals(descripcionNueva, StringComparison.OrdinalIgnoreCase))
                    {
                        existingRole.Description = descripcionNueva;
                        existingRole.IsSystemRole = true;
                        
                        context.Set<ApplicationRole>().Update(existingRole);
                        rolesActualizados++;
                        
                        logger.LogInformation("🔄 Rol actualizado: {RoleName}", roleDefinition.Name);
                    }
                    else
                    {
                        logger.LogDebug("⏭️  Rol sin cambios: {RoleName}", roleDefinition.Name);
                    }
                    
                    // NOTE: Los permisos se manejan a través de ApplicationRolePermission
                    // que es una tabla separada, no como string en el rol
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error al procesar rol: {RoleName}", roleDefinition.Name);
                throw;
            }
        }

        logger.LogInformation("📊 Roles procesados: {Creados} creados, {Actualizados} actualizados", 
            rolesCreados, rolesActualizados);
    }

    /// <summary>
    /// Genera un ID determinístico para el rol basado en el tipo de usuario
    /// </summary>
    private static Guid GenerateRoleId(TipoUsuario tipoUsuario)
    {
        // Usar hash determinístico para IDs consistentes entre entornos
        var bytes = System.Text.Encoding.UTF8.GetBytes($"ROLE_{tipoUsuario}_{typeof(RolesSeeder).Assembly.GetName().Version}");
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return new Guid(hash.Take(16).ToArray());
    }

    /// <summary>
    /// Definición de un rol con sus permisos
    /// </summary>
    private record RoleDefinition
    {
        public required string Name { get; init; }
        public required string NormalizedName { get; init; }
        public required string Description { get; init; }
        public required string[] Permissions { get; init; }
    }
} 