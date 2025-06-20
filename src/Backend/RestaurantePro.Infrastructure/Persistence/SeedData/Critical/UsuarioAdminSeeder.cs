using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Critical;

/// <summary>
/// Seeder para crear el usuario administrador inicial del sistema.
/// Este usuario es crítico para el funcionamiento del sistema ya que permite
/// el acceso inicial de administración.
/// 
/// Orden: 200 (después de roles, estados y permisos)
/// </summary>
public class UsuarioAdminSeeder : ISeedData
{
    // ID determinístico para el usuario administrador
    private static readonly Guid AdminUserId = new("11111111-1111-1111-1111-111111111111");
    
    // Credenciales por defecto (deberían cambiarse en producción)
    private const string DefaultAdminUsername = "admin";
    private const string DefaultAdminEmail = "admin@restaurantepro.com";
    private const string DefaultAdminName = "Administrador del Sistema";
    private const string DefaultAdminPassword = "AdminRestaurante123!";

    public string Name => "Usuario Administrador Inicial";
    public int Order => 200;
    public bool IsDevOnly => false;
    public bool IsCritical => true;

    public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("🔧 Iniciando seed de usuario administrador inicial...");

        try
        {
            // Verificar si ya existe el usuario administrador
            var adminExistente = await context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == AdminUserId || 
                                        u.NombreUsuario == DefaultAdminUsername ||
                                        u.Email == DefaultAdminEmail, cancellationToken);

            if (adminExistente != null)
            {
                logger.LogInformation("✅ Usuario administrador ya existe: {Username}", adminExistente.NombreUsuario);
                await ActualizarAdminSiEsNecesario(context, adminExistente, logger, cancellationToken);
                return;
            }

            // Crear nuevo usuario administrador
            var usuarioAdmin = await CrearUsuarioAdmin(logger);
            
            context.Usuarios.Add(usuarioAdmin);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("✅ Usuario administrador creado exitosamente: {Username}", usuarioAdmin.NombreUsuario);
            logger.LogWarning("🔐 IMPORTANTE: Cambiar las credenciales por defecto en producción");
            logger.LogInformation("📋 Credenciales por defecto - Usuario: {Username}, Email: {Email}", 
                DefaultAdminUsername, DefaultAdminEmail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error al crear usuario administrador");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Usuarios
            .AnyAsync(u => u.Id == AdminUserId || 
                          u.NombreUsuario == DefaultAdminUsername ||
                          u.Email == DefaultAdminEmail, cancellationToken);
    }

    private async Task<Usuario> CrearUsuarioAdmin(ILogger logger)
    {
        // Crear usuario con ID determinístico
        var usuario = Usuario.Crear(
            nombreUsuario: DefaultAdminUsername,
            nombreCompleto: DefaultAdminName,
            email: DefaultAdminEmail,
            rol: RolUsuario.Administrador
        );

        // Usar reflexión para establecer el ID determinístico
        var property = typeof(Usuario).GetProperty("Id");
        if (property != null && property.CanWrite)
        {
            property.SetValue(usuario, AdminUserId);
        }

        // Confirmar la cuenta directamente (no necesita confirmación por email)
        usuario.ConfirmarCuenta();

        // Establecer información organizacional
        usuario.EstablecerInformacionOrganizacional(
            supervisorId: null, // No tiene supervisor
            departamento: "Administración",
            posicion: "Administrador del Sistema"
        );

        // Establecer password hash (en producción, usar un servicio de hash seguro)
        var (passwordHash, salt) = GenerarPasswordHash(DefaultAdminPassword);
        usuario.EstablecerPassword(passwordHash, salt);

        // Agregar permisos específicos de administrador
        await AgregarPermisosAdministrador(usuario, logger);

        return usuario;
    }

    private async Task ActualizarAdminSiEsNecesario(RestauranteProDbContext context, Usuario adminExistente, ILogger logger, CancellationToken cancellationToken)
    {
        bool necesitaActualizacion = false;

        // Verificar si el usuario está inactivo y activarlo
        if (adminExistente.Estado != EstadoUsuario.Activo)
        {
            adminExistente.Activar();
            necesitaActualizacion = true;
            logger.LogInformation("🔄 Activando usuario administrador");
        }

        // Verificar si tiene el rol de administrador
        if (!adminExistente.TieneRol(RolUsuario.Administrador))
        {
            adminExistente.AsignarRol(RolUsuario.Administrador);
            necesitaActualizacion = true;
            logger.LogInformation("🔄 Asignando rol de administrador");
        }

        // Actualizar permisos si es necesario
        await AgregarPermisosAdministrador(adminExistente, logger);

        if (necesitaActualizacion)
        {
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("✅ Usuario administrador actualizado");
        }
    }

    private async Task AgregarPermisosAdministrador(Usuario usuario, ILogger logger)
    {
        var permisosAdmin = new[]
        {
            // Permisos de usuarios
            "usuarios.crear", "usuarios.leer", "usuarios.actualizar", "usuarios.eliminar",
            "usuarios.asignar_roles", "usuarios.gestionar_permisos",
            
            // Permisos de sistema
            "sistema.configurar", "sistema.monitorear", "sistema.backup",
            "sistema.logs", "sistema.mantenimiento",
            
            // Permisos de operaciones
            "operaciones.ver_todas", "operaciones.gestionar_todas",
            "reportes.ver_todos", "reportes.exportar_todos",
            
            // Permisos de inventario
            "inventario.gestionar_todo", "inventario.ajustes",
            
            // Permisos comerciales
            "comercial.gestionar_todo", "facturacion.gestionar_todo",
            
            // Permisos de proveedores
            "proveedores.gestionar_todo"
        };

        foreach (var permiso in permisosAdmin)
        {
            if (!usuario.TienePermiso(permiso))
            {
                usuario.AgregarPermiso(permiso);
                logger.LogDebug("  ➕ Agregado permiso: {Permiso}", permiso);
            }
        }

        await Task.CompletedTask;
    }

    private static (string passwordHash, string salt) GenerarPasswordHash(string password)
    {
        // IMPORTANTE: En producción, usar un servicio de hash más robusto como BCrypt
        // Esto es solo para el seed inicial
        
        var salt = Guid.NewGuid().ToString();
        var passwordWithSalt = password + salt;
        
        // Usar SHA256 como hash básico (EN PRODUCCIÓN USAR BCRYPT)
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(passwordWithSalt));
        var hash = Convert.ToBase64String(hashBytes);
        
        return (hash, salt);
    }
} 