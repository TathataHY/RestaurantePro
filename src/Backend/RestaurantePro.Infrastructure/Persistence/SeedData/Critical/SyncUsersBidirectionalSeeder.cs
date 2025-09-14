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
/// Seeder para sincronización bidireccional entre usuarios del dominio e Identity
/// Este seeder es crítico porque asegura que todos los usuarios existan en ambas tablas
/// para que el sistema funcione correctamente.
/// </summary>
public class SyncUsersBidirectionalSeeder : ISeedData
{
    private readonly UserManager<IdentityApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SeedDataConfiguration _config;

    public SyncUsersBidirectionalSeeder(
        UserManager<IdentityApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<SeedDataConfiguration> config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config.Value;
    }

    public string Name => "Sincronización Bidireccional de Usuarios";
    public int Order => 400; // Después de todos los seeders de usuarios
    public bool IsDevOnly => false;
    public bool IsCritical => true;

    public async Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("🔄 Iniciando sincronización bidireccional de usuarios...");

        // 1. Sincronizar desde Dominio hacia Identity
        await SyncFromDomainToIdentity(context, logger, cancellationToken);

        // 2. Sincronizar desde Identity hacia Dominio
        await SyncFromIdentityToDomain(context, logger, cancellationToken);

        logger.LogInformation("✅ Sincronización bidireccional completada");
    }

    public async Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default)
    {
        // Siempre ejecutar este seeder para asegurar sincronización
        return false;
    }

    private async Task SyncFromDomainToIdentity(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
    {
        logger.LogInformation("📋 Sincronizando usuarios del dominio hacia Identity...");

        var usuariosDominio = await context.Usuarios.ToListAsync(cancellationToken);
        
        foreach (var usuarioDominio in usuariosDominio)
        {
            var usuarioIdentity = await _userManager.FindByEmailAsync(usuarioDominio.Email);
            
            if (usuarioIdentity == null)
            {
                // Crear usuario en Identity
                var nuevoUsuarioIdentity = new IdentityApplicationUser
                {
                    Id = usuarioDominio.Id, // Usar el mismo ID del dominio
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

                var resultado = await _userManager.CreateAsync(nuevoUsuarioIdentity, "AdminRestaurante123!");
                
                if (resultado.Succeeded)
                {
                    // Asignar rol
                    var nombreRolIdentity = MapearRolDominioAIdentity(usuarioDominio.Rol);
                    await _userManager.AddToRoleAsync(nuevoUsuarioIdentity, nombreRolIdentity);
                    
                    logger.LogInformation("✅ Usuario creado en Identity: {Username} ({Email})", 
                        usuarioDominio.NombreUsuario, usuarioDominio.Email);
                }
                else
                {
                    var errors = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    logger.LogError("❌ Error creando usuario en Identity: {Username} - {Errors}", 
                        usuarioDominio.NombreUsuario, errors);
                }
            }
            else
            {
                logger.LogInformation("ℹ️ Usuario ya existe en Identity: {Username}", usuarioDominio.NombreUsuario);
            }
        }
    }

    private async Task SyncFromIdentityToDomain(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken)
    {
        logger.LogInformation("📋 Sincronizando usuarios de Identity hacia el dominio...");

        var usuariosIdentity = await _userManager.Users.ToListAsync();
        
        foreach (var usuarioIdentity in usuariosIdentity)
        {
            var usuarioDominio = await context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == usuarioIdentity.Email, cancellationToken);
            
            if (usuarioDominio == null)
            {
                // Crear usuario en el dominio
                var nuevoUsuarioDominio = Usuario.Crear(
                    nombreUsuario: usuarioIdentity.UserName,
                    nombreCompleto: $"{usuarioIdentity.Nombre} {usuarioIdentity.Apellidos}".Trim(),
                    email: usuarioIdentity.Email,
                    rol: MapearRolIdentityADominio(usuarioIdentity)
                );

                // Asignar el mismo ID de Identity
                var campoId = typeof(Usuario).GetProperty("Id");
                campoId?.SetValue(nuevoUsuarioDominio, usuarioIdentity.Id);

                // Activar el usuario si está activo en Identity
                if (usuarioIdentity.Activo)
                {
                    nuevoUsuarioDominio.Activar();
                    nuevoUsuarioDominio.ConfirmarCuenta();
                }

                context.Usuarios.Add(nuevoUsuarioDominio);
                
                logger.LogInformation("✅ Usuario creado en dominio: {Username} ({Email})", 
                    usuarioIdentity.UserName, usuarioIdentity.Email);
            }
            else
            {
                logger.LogInformation("ℹ️ Usuario ya existe en dominio: {Username}", usuarioIdentity.UserName);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private string MapearRolDominioAIdentity(string rolDominio)
    {
        return rolDominio switch
        {
            "Administrador" => "Administrador",
            "Gerente" => "Gerente", 
            "Mesero" => "Mesero",
            "Cocinero" => "Cocinero",
            "Cajero" => "Cajero",
            _ => "Empleado"
        };
    }

    private RolUsuario MapearRolIdentityADominio(IdentityApplicationUser usuarioIdentity)
    {
        // Obtener roles del usuario en Identity
        var roles = _userManager.GetRolesAsync(usuarioIdentity).Result;
        
        if (roles.Contains("Administrador"))
            return RolUsuario.Administrador;
        if (roles.Contains("Gerente"))
            return RolUsuario.Gerente;
        if (roles.Contains("Cocinero"))
            return RolUsuario.Cocinero;
        if (roles.Contains("Mesero"))
            return RolUsuario.Mesero;
        if (roles.Contains("Cajero"))
            return RolUsuario.Cajero;
        
        return RolUsuario.Mesero; // Rol por defecto
    }
}
