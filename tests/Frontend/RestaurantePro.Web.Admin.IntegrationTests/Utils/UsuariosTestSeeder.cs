using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Web.Admin.IntegrationTests.Utils;

/// <summary>
/// Seeder para datos de prueba de usuarios
/// </summary>
public static class UsuariosTestSeeder
{
    /// <summary>
    /// Crea un usuario administrador de prueba
    /// </summary>
    public static async Task<Guid> SeedUsuarioAdministradorAsync(RestauranteProDbContext context)
    {
        var usuario = Usuario.Crear(
            "admin.test",
            "Administrador de Prueba",
            "admin.test@restaurantepro.com",
            RolUsuario.Administrador
        );

        // Establecer contraseña
        usuario.EstablecerPassword("AdminPass123!", "salt123");

        // Activar el usuario
        usuario.Activar();

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        return usuario.Id;
    }

    /// <summary>
    /// Crea un usuario gerente de prueba
    /// </summary>
    public static async Task<Guid> SeedUsuarioGerenteAsync(RestauranteProDbContext context)
    {
        var usuario = Usuario.Crear(
            "gerente.test",
            "Gerente de Prueba",
            "gerente.test@restaurantepro.com",
            RolUsuario.Gerente
        );

        // Establecer contraseña
        usuario.EstablecerPassword("GerentePass123!", "salt456");

        // Activar el usuario
        usuario.Activar();

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        return usuario.Id;
    }

    /// <summary>
    /// Crea un usuario mesero de prueba
    /// </summary>
    public static async Task<Guid> SeedUsuarioMeseroAsync(RestauranteProDbContext context)
    {
        var usuario = Usuario.Crear(
            "mesero.test",
            "Mesero de Prueba",
            "mesero.test@restaurantepro.com",
            RolUsuario.Mesero
        );

        // Establecer contraseña
        usuario.EstablecerPassword("MeseroPass123!", "salt789");

        // Activar el usuario
        usuario.Activar();

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        return usuario.Id;
    }
}
