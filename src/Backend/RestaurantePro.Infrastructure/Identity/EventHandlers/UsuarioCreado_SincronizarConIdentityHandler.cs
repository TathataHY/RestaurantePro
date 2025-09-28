using Microsoft.AspNetCore.Identity;
using RestaurantePro.Domain.Core.Base.Events.Handlers;
using RestaurantePro.Domain.Core.Usuarios.Events.Usuario;
using RestaurantePro.Infrastructure.Identity.Models;

namespace RestaurantePro.Infrastructure.Identity.EventHandlers
{
    /// <summary>
    /// Manejador de eventos que sincroniza usuarios creados en el dominio con Identity
    /// </summary>
    public class UsuarioCreado_SincronizarConIdentityHandler : IDomainEventHandler<UsuarioCreado>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UsuarioCreado_SincronizarConIdentityHandler> _logger;

        public UsuarioCreado_SincronizarConIdentityHandler(
            UserManager<ApplicationUser> userManager,
            ILogger<UsuarioCreado_SincronizarConIdentityHandler> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
        }

        public async Task Handle(UsuarioCreado evento, CancellationToken cancellationToken)
        {
            
            try
            {
                _logger.LogInformation("🔄 Creando usuario en Identity: {UsuarioId} - {Email}", evento.UsuarioId, evento.Email);

                // Verificar si el usuario ya existe en Identity
                var existingUser = await _userManager.FindByEmailAsync(evento.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("⚠️ Usuario {Email} ya existe en Identity, saltando creación", evento.Email);
                    return;
                }

                // Crear nuevo usuario en Identity
                var identityUser = new ApplicationUser
                {
                    UserName = evento.NombreUsuario,
                    Email = evento.Email,
                    EmailConfirmed = false, // Requerirá confirmación de email
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    // Campos requeridos por la base de datos
                    Nombre = evento.NombreUsuario, // Usar el nombre de usuario como nombre temporal
                    Apellidos = "Usuario", // Apellido temporal por defecto
                    FotoPerfil = "", // Foto de perfil vacía por defecto
                    // RefreshToken removido - ahora se maneja en tabla separada
                    Activo = evento.Estado == Domain.Core.Usuarios.Enums.EstadoUsuario.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    UltimaModificacion = DateTime.UtcNow
                };

                // Usar la contraseña del evento si está disponible, sino generar una por defecto
                var passwordAUsar = !string.IsNullOrEmpty(evento.Password) 
                    ? evento.Password 
                    : GenerarPasswordPorDefecto(evento.NombreUsuario);
                
                if (!string.IsNullOrEmpty(evento.Password))
                {
                    _logger.LogInformation("🔑 Usando contraseña del formulario para {Email}", evento.Email);
                }
                else
                {
                    _logger.LogInformation("🔑 Generando contraseña por defecto para {Email}", evento.Email);
                }

                // Crear el usuario en Identity
                var result = await _userManager.CreateAsync(identityUser, passwordAUsar);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("✅ Usuario creado exitosamente en Identity: {Email}", evento.Email);
                    _logger.LogInformation("🔑 Contraseña utilizada: {Password}", passwordAUsar);
                    
                    // Asignar rol del evento (del formulario)
                    await AsignarRolDelEvento(identityUser, evento.Rol);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("❌ Error al crear usuario en Identity: {Email} - Errores: {Errors}", evento.Email, errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado al sincronizar usuario creado con Identity: {Email}", evento.Email);
            }
        }

        /// <summary>
        /// Genera una contraseña por defecto basada en el nombre de usuario
        /// </summary>
        private static string GenerarPasswordPorDefecto(string nombreUsuario)
        {
            // Lógica similar al seeder de Identity
            var passwordBase = $"RestaurantePro_{nombreUsuario}2024!";
            return passwordBase;
        }

        /// <summary>
        /// Asigna el rol del evento (del formulario) al usuario en Identity
        /// </summary>
        private async Task AsignarRolDelEvento(ApplicationUser user, string rolDelEvento)
        {
            try
            {
                // 🔍 DEBUG: Ver exactamente qué rol llega del evento
                _logger.LogInformation("🔍🏷️ [AsignarRol] Usuario: {Email}", user.Email);
                _logger.LogInformation("🔍🏷️ [AsignarRol] Rol del evento recibido: '{RolEvento}'", rolDelEvento ?? "NULL");
                _logger.LogInformation("🔍🏷️ [AsignarRol] ¿Es null o empty? {IsEmpty}", string.IsNullOrEmpty(rolDelEvento));
                
                if (string.IsNullOrEmpty(rolDelEvento))
                {
                    _logger.LogError("❌ PROBLEMA: No se especificó rol para usuario {Email} - El evento UsuarioCreado no incluye el rol", user.Email);
                    _logger.LogError("❌ ESTO ES UN BUG: Todos los usuarios necesitan un rol específico, no 'Empleado' por defecto");
                    rolDelEvento = "Mesero"; // 🔧 Cambio temporal para debug - debería ser el rol correcto del evento
                }

                _logger.LogInformation("🔍🏷️ [AsignarRol] Rol final a asignar: '{RolFinal}'", rolDelEvento);

                var result = await _userManager.AddToRoleAsync(user, rolDelEvento);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("✅ Rol '{Rol}' asignado exitosamente a usuario {Email}", rolDelEvento, user.Email);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("⚠️ Error al asignar rol '{Rol}' a usuario {Email}: {Errors}", rolDelEvento, user.Email, errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al asignar rol del evento a usuario {Email}", user.Email);
            }
        }
    }
}
