using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Critical;

/// <summary>
/// Seeder para sincronizar usuarios del dominio con Identity
/// Este seeder es crítico porque asegura que todos los usuarios del dominio
/// existan también en Identity para que el login funcione correctamente.
/// Respeta los flags de configuración (RunDemoData, RunTestingData).
/// </summary>
public class IdentityUsersSeeder : ISeedData
{
    private readonly UserManager<IdentityApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SeedDataConfiguration _config;

    public IdentityUsersSeeder(
        UserManager<IdentityApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<SeedDataConfiguration> config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config.Value;
    }

    public string Name => "Sincronización de Usuarios con Identity";
    public int Order => 300; // Después de todos los seeders de usuarios
    public bool IsDevOnly => false;
    public bool IsCritical => true;

    public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("🔄 Iniciando sincronización de usuarios del dominio con Identity...");

        // Obtener usuarios del dominio según configuración
        var usuariosDominio = await ObtenerUsuariosDominio(context, logger, cancellationToken);
        
        if (!usuariosDominio.Any())
        {
            logger.LogWarning("⚠️ No se encontraron usuarios del dominio para sincronizar");
            return;
        }

        logger.LogInformation("📋 Encontrados {Count} usuarios del dominio para sincronizar", usuariosDominio.Count);
        
        // Log detallado de usuarios encontrados
        foreach (var usuario in usuariosDominio)
        {
            logger.LogInformation("👤 Usuario encontrado: {Username} ({Email}) - Rol: {Rol}", 
                usuario.NombreUsuario, usuario.Email, usuario.Rol);
        }

        var resultados = new Dictionary<SyncResult, int>
        {
            { SyncResult.Created, 0 },
            { SyncResult.Updated, 0 },
            { SyncResult.NoChange, 0 },
            { SyncResult.Error, 0 }
        };

        // Sincronizar cada usuario
        foreach (var usuario in usuariosDominio)
        {
            var resultado = await SincronizarUsuarioConIdentity(usuario, logger);
            resultados[resultado]++;
        }

        // Log resumen
        logger.LogInformation("✅ Sincronización completada: {Created} creados, {Updated} actualizados, {NoChange} sin cambios, {Error} errores",
            resultados[SyncResult.Created],
            resultados[SyncResult.Updated],
            resultados[SyncResult.NoChange],
            resultados[SyncResult.Error]);
    }

    public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
    {
        // Verificar si TODOS los usuarios del dominio están correctamente sincronizados en Identity
        var usuariosDominio = await ObtenerUsuariosDominio(context, null!, cancellationToken);
        
        if (!usuariosDominio.Any())
        {
            return false; // No hay usuarios del dominio, no necesita ejecutarse
        }
        
        foreach (var usuario in usuariosDominio.Take(5)) // Solo verificar los primeros 5
        {
            var usuarioIdentity = await _userManager.FindByEmailAsync(usuario.Email);
            if (usuarioIdentity == null)
            {
                return false; // Usuario no existe en Identity, necesita sincronización
            }
            
            // Verificar que el usuario esté activo y confirmado si debe estarlo
            if (usuario.Estado == Domain.Core.Usuarios.Enums.EstadoUsuario.Activo)
            {
                if (!usuarioIdentity.Activo || !usuarioIdentity.EmailConfirmed)
                {
                    return false; // Usuario no está correctamente activado, necesita sincronización
                }
            }
            
            // Verificar que el usuario tenga contraseña válida
            var passwordEsperada = GenerarPasswordPorDefecto(usuario.NombreUsuario);
            var passwordValida = await _userManager.CheckPasswordAsync(usuarioIdentity, passwordEsperada);
            if (!passwordValida)
            {
                Console.WriteLine($"🔐 [DEBUG] Usuario {usuario.NombreUsuario} no tiene contraseña válida. Esperada: {passwordEsperada}");
                return false; // Usuario no tiene contraseña válida, necesita sincronización
            }
        }
        
        return true; // Todos los usuarios están correctamente sincronizados
    }

    private async Task<List<Usuario>> ObtenerUsuariosDominio(
        RestauranteProDbContext context, 
        ILogger logger, 
        CancellationToken cancellationToken)
    {
        var usuarios = new List<Usuario>();

        // 1. USUARIOS CRÍTICOS (siempre se incluyen)
        var usuariosCriticos = await context.Usuarios
            .Where(u => u.Rol == "Administrador" && 
                       u.NombreUsuario == "admin" && 
                       u.Email == "admin@restaurantepro.com")
            .ToListAsync(cancellationToken);
        
        // Si no existe el usuario admin en el dominio, crearlo
        if (!usuariosCriticos.Any())
        {
            if (logger != null)
                logger.LogInformation("🔧 Usuario admin no existe en dominio, creándolo...");
            var usuarioAdmin = Usuario.Crear(
                nombreUsuario: "admin",
                nombreCompleto: "Administrador del Sistema",
                email: "admin@restaurantepro.com",
                rol: RolUsuario.Administrador
            );
            usuarioAdmin.ConfirmarCuenta();
            usuarioAdmin.Activar();
            
            context.Usuarios.Add(usuarioAdmin);
            await context.SaveChangesAsync(cancellationToken);
            
            usuariosCriticos.Add(usuarioAdmin);
            if (logger != null)
                logger.LogInformation("✅ Usuario admin creado en dominio");
        }
        
        usuarios.AddRange(usuariosCriticos);
        if (logger != null)
            logger.LogDebug("🔐 Usuarios críticos encontrados: {Count}", usuariosCriticos.Count);

        // 2. USUARIOS DEMO (solo si RunDemoData = true)
        if (_config.RunDemoData)
        {
            var usuariosDemo = await context.Usuarios
                .Where(u => u.Email.Contains("@lacocinaustral.cl") || 
                           u.NombreUsuario.StartsWith("gerente.") ||
                           u.NombreUsuario.StartsWith("chef.") ||
                           u.NombreUsuario.StartsWith("mesero.") ||
                           u.NombreUsuario.StartsWith("cajero."))
                .ToListAsync(cancellationToken);
            
            usuarios.AddRange(usuariosDemo);
            if (logger != null)
                logger.LogDebug("🎭 Usuarios demo encontrados: {Count}", usuariosDemo.Count);
        }

        // 3. USUARIOS TESTING (solo si RunTestingData = true)
        if (_config.RunTestingData)
        {
            var usuariosTesting = await context.Usuarios
                .Where(u => u.Email.Contains("@testing.com") || 
                           u.NombreUsuario.StartsWith("test-") ||
                           u.NombreUsuario == "test-admin" ||
                           u.NombreUsuario == "test-standard" ||
                           u.NombreUsuario == "test-inactive" ||
                           u.NombreUsuario == "test-user-edge-case" ||
                           u.NombreUsuario == "min")
                .ToListAsync(cancellationToken);
            
            usuarios.AddRange(usuariosTesting);
            if (logger != null)
                logger.LogDebug("🧪 Usuarios testing encontrados: {Count}", usuariosTesting.Count);
        }

        return usuarios.Distinct().ToList();
    }

    private async Task<SyncResult> SincronizarUsuarioConIdentity(Usuario usuarioDominio, ILogger logger)
    {
        // Verificar si el usuario ya existe en Identity
        var usuarioIdentity = await _userManager.FindByEmailAsync(usuarioDominio.Email);
        
        if (usuarioIdentity == null)
        {
            // Crear nuevo usuario en Identity
            return await CrearUsuarioEnIdentity(usuarioDominio, logger);
        }
        else
        {
            // Actualizar usuario existente en Identity
            return await ActualizarUsuarioEnIdentity(usuarioIdentity, usuarioDominio, logger);
        }
    }

    private async Task<SyncResult> CrearUsuarioEnIdentity(Usuario usuarioDominio, ILogger logger)
    {
        try
        {
            // Crear usuario en Identity
            var usuarioIdentity = new IdentityApplicationUser
            {
                UserName = usuarioDominio.NombreUsuario,
                Email = usuarioDominio.Email,
                Nombre = usuarioDominio.NombreCompleto.Split(' ').FirstOrDefault() ?? "",
                Apellidos = string.Join(" ", usuarioDominio.NombreCompleto.Split(' ').Skip(1)) ?? "",
                FechaCreacion = usuarioDominio.FechaCreacion,
                Activo = usuarioDominio.Estado == EstadoUsuario.Activo,
                FotoPerfil = "",
                RefreshToken = "",
                EmailConfirmed = usuarioDominio.Estado == EstadoUsuario.Activo
            };

            // Generar contraseña por defecto
            var password = GenerarPasswordPorDefecto(usuarioDominio.NombreUsuario);
            logger.LogInformation("🔐 Generando contraseña para usuario {Username}: {Password} ({PasswordLength} caracteres)", 
                usuarioDominio.NombreUsuario, password, password.Length);
            
            // Crear usuario sin contraseña primero
            var result = await _userManager.CreateAsync(usuarioIdentity);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("❌ Error creando usuario {Username} en Identity: {Errors}", 
                    usuarioDominio.NombreUsuario, errors);
                return SyncResult.Error;
            }

            // Agregar contraseña después de crear el usuario
            var addPasswordResult = await _userManager.AddPasswordAsync(usuarioIdentity, password);
            
            if (!addPasswordResult.Succeeded)
            {
                var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                logger.LogError("❌ Error agregando contraseña para usuario {Username}: {Errors}", 
                    usuarioDominio.NombreUsuario, errors);
                
                // Limpiar el usuario creado si falló la contraseña
                await _userManager.DeleteAsync(usuarioIdentity);
                return SyncResult.Error;
            }

            // Validar que la contraseña se generó correctamente
            var passwordValidationResult = await _userManager.CheckPasswordAsync(usuarioIdentity, password);
            if (!passwordValidationResult)
            {
                logger.LogError("❌ Error crítico: La contraseña no es válida para el usuario {Username} después de AddPasswordAsync", 
                    usuarioDominio.NombreUsuario);
                await _userManager.DeleteAsync(usuarioIdentity);
                return SyncResult.Error;
            }
            
            logger.LogInformation("✅ Usuario {Username} creado exitosamente con contraseña válida", 
                usuarioDominio.NombreUsuario);

            // Asignar rol
            var nombreRolIdentity = MapearRolDominioAIdentity(usuarioDominio.Rol);
            var resultadoRol = await _userManager.AddToRoleAsync(usuarioIdentity, nombreRolIdentity);
            if (!resultadoRol.Succeeded)
            {
                logger.LogWarning("⚠️ No se pudo asignar rol {Rol} al usuario {Username}", 
                    nombreRolIdentity, usuarioDominio.NombreUsuario);
            }

            logger.LogInformation("✅ Usuario creado en Identity: {Username} ({Email})", 
                usuarioDominio.NombreUsuario, usuarioDominio.Email);
            
            return SyncResult.Created;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error creando usuario {Username} en Identity", usuarioDominio.NombreUsuario);
            return SyncResult.Error;
        }
    }

    private async Task<SyncResult> ActualizarUsuarioEnIdentity(
        IdentityApplicationUser usuarioIdentity, 
        Usuario usuarioDominio, 
        ILogger logger)
    {
        try
        {
            bool necesitaActualizacion = false;

            // Actualizar datos básicos si han cambiado
            if (usuarioIdentity.Nombre != (usuarioDominio.NombreCompleto.Split(' ').FirstOrDefault() ?? ""))
            {
                usuarioIdentity.Nombre = usuarioDominio.NombreCompleto.Split(' ').FirstOrDefault() ?? "";
                necesitaActualizacion = true;
            }

            if (usuarioIdentity.Apellidos != (string.Join(" ", usuarioDominio.NombreCompleto.Split(' ').Skip(1)) ?? ""))
            {
                usuarioIdentity.Apellidos = string.Join(" ", usuarioDominio.NombreCompleto.Split(' ').Skip(1)) ?? "";
                necesitaActualizacion = true;
            }

            if (usuarioIdentity.Activo != (usuarioDominio.Estado == EstadoUsuario.Activo))
            {
                usuarioIdentity.Activo = usuarioDominio.Estado == EstadoUsuario.Activo;
                necesitaActualizacion = true;
            }

            if (usuarioIdentity.EmailConfirmed != (usuarioDominio.Estado == EstadoUsuario.Activo))
            {
                usuarioIdentity.EmailConfirmed = usuarioDominio.Estado == EstadoUsuario.Activo;
                necesitaActualizacion = true;
            }

            // Verificar y regenerar contraseña si es necesario
            var passwordEsperada = GenerarPasswordPorDefecto(usuarioDominio.NombreUsuario);
            var passwordValida = await _userManager.CheckPasswordAsync(usuarioIdentity, passwordEsperada);
            if (!passwordValida)
            {
                logger.LogInformation("🔐 Regenerando contraseña para usuario {Username}: {Password}", 
                    usuarioDominio.NombreUsuario, passwordEsperada);
                
                // Remover contraseña actual
                await _userManager.RemovePasswordAsync(usuarioIdentity);
                
                // Agregar nueva contraseña
                var resultadoPassword = await _userManager.AddPasswordAsync(usuarioIdentity, passwordEsperada);
                if (!resultadoPassword.Succeeded)
                {
                    var errors = string.Join(", ", resultadoPassword.Errors.Select(e => e.Description));
                    logger.LogError("❌ Error regenerando contraseña para usuario {Username}: {Errors}", 
                        usuarioDominio.NombreUsuario, errors);
                    return SyncResult.Error;
                }
                
                // Verificar que la nueva contraseña sea válida
                var nuevaPasswordValida = await _userManager.CheckPasswordAsync(usuarioIdentity, passwordEsperada);
                if (!nuevaPasswordValida)
                {
                    logger.LogError("❌ Error crítico: La contraseña regenerada no es válida para el usuario {Username}", 
                        usuarioDominio.NombreUsuario);
                    return SyncResult.Error;
                }
                
                logger.LogInformation("✅ Contraseña regenerada exitosamente para usuario {Username}", 
                    usuarioDominio.NombreUsuario);
                necesitaActualizacion = true;
            }

            // Actualizar si es necesario
            if (necesitaActualizacion)
            {
                var result = await _userManager.UpdateAsync(usuarioIdentity);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("❌ Error actualizando usuario {Username} en Identity: {Errors}", 
                        usuarioDominio.NombreUsuario, errors);
                    return SyncResult.Error;
                }

                logger.LogInformation("🔄 Usuario actualizado en Identity: {Username}", usuarioDominio.NombreUsuario);
                return SyncResult.Updated;
            }

            // Verificar rol
            var nombreRolIdentity = MapearRolDominioAIdentity(usuarioDominio.Rol);
            var rolesActuales = await _userManager.GetRolesAsync(usuarioIdentity);
            if (!rolesActuales.Contains(nombreRolIdentity))
            {
                var roleResult = await _userManager.AddToRoleAsync(usuarioIdentity, nombreRolIdentity);
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("🔄 Rol {Rol} asignado al usuario {Username}", 
                        nombreRolIdentity, usuarioDominio.NombreUsuario);
                    return SyncResult.Updated;
                }
            }

            return SyncResult.NoChange;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error actualizando usuario {Username} en Identity", usuarioDominio.NombreUsuario);
            return SyncResult.Error;
        }
    }

    private static string GenerarPasswordPorDefecto(string username)
    {
        // Para usuarios críticos (admin), usar la contraseña por defecto
        if (username == "admin")
        {
            Console.WriteLine($"🔐 [DEBUG] Generando contraseña para admin: AdminRestaurante123!");
            return "AdminRestaurante123!";
        }

        // Para otros usuarios, convertir a PascalCase sin puntos + '123!'
        if (!string.IsNullOrEmpty(username))
        {
            // Remover puntos y convertir a PascalCase
            var partes = username.Split('.');
            var usernamePascalCase = string.Join("", partes.Select(p => char.ToUpper(p[0]) + p.Substring(1).ToLower()));
            var password = $"{usernamePascalCase}123!";
            Console.WriteLine($"🔐 [DEBUG] Generando contraseña para {username}: {password}");
            return password;
        }
        // Fallback
        Console.WriteLine($"🔐 [DEBUG] Usando contraseña fallback: DemoUser123!");
        return "DemoUser123!";
    }

    private static string MapearRolDominioAIdentity(string rolDominio)
    {
        // 🔍 DEBUG: Ver exactamente qué rol llega del dominio
        Console.WriteLine($"🔍🏷️ [MapearRol] Rol del dominio recibido: '{rolDominio}'");
        
        // Hacer el mapeo insensible a mayúsculas/minúsculas
        var rolNormalizado = rolDominio?.ToLower();
        Console.WriteLine($"🔍🏷️ [MapearRol] Rol normalizado: '{rolNormalizado}'");
        
        var rolMapeado = rolNormalizado switch
        {
            "administrador" => "Administrador",
            "gerente" => "Gerente", 
            "cajero" => "Cajero",
            "mesero" => "Mesero",
            "cocinero" => "Cocinero",
            "encargadoinventario" => "EncargadoInventario",
            _ => "Empleado" // 🔍 CAMBIÉ: Empleado como fallback para debug
        };
        
        Console.WriteLine($"🔍🏷️ [MapearRol] Rol final mapeado: '{rolMapeado}'");
        return rolMapeado;
    }

    private enum SyncResult
    {
        Created,
        Updated,
        NoChange,
        Error
    }
} 