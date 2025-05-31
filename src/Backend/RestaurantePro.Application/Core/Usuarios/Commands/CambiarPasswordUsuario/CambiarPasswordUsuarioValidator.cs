using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using System.Text.RegularExpressions;

namespace RestaurantePro.Application.Core.Usuarios.Commands.CambiarPasswordUsuario;

public class CambiarPasswordUsuarioValidator : AbstractValidator<CambiarPasswordUsuarioCommand>
{
    private readonly IApplicationDbContext _context;
    private static readonly Regex PasswordSegura = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{}|;':""\\.,<>?]).{8,}$", RegexOptions.Compiled);
    private static readonly Regex PatronesProhibidos = new(@"(123|abc|qwe|password|admin|user|pass|letmein)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public CambiarPasswordUsuarioValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesUsuario();
        ConfigurarValidacionesPassword();
        ConfigurarValidacionesSeguridad();
        ConfigurarValidacionesNegocio();
        ConfigurarValidacionesAutorizacion();
        ConfigurarValidacionesPoliticas();
        ConfigurarValidacionesAuditoria();
    }

    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(v => v.UsuarioId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del usuario es requerido.")
            .MustAsync(UsuarioExiste)
            .WithMessage("El usuario especificado no existe.");

        RuleFor(v => v.UsuarioAutorizaId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del usuario autorizador es requerido.")
            .MustAsync(UsuarioAutorizadorExiste)
            .WithMessage("El usuario autorizador especificado no existe.");

        RuleFor(v => v.MotivosCambio)
            .NotEmpty()
            .WithMessage("El motivo del cambio es requerido.")
            .MinimumLength(10)
            .WithMessage("El motivo debe tener al menos 10 caracteres.")
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder 500 caracteres.");

        RuleFor(v => v.Prioridad)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La prioridad mínima es 1.")
            .LessThanOrEqualTo(4)
            .WithMessage("La prioridad máxima es 4.");

        RuleFor(v => v.PasswordNueva)
            .NotEmpty()
            .WithMessage("La nueva contraseña es requerida.");

        RuleFor(v => v.ConfirmarPasswordNueva)
            .NotEmpty()
            .WithMessage("La confirmación de contraseña es requerida.")
            .Equal(x => x.PasswordNueva)
            .WithMessage("La confirmación de contraseña no coincide con la nueva contraseña.");
    }

    private void ConfigurarValidacionesUsuario()
    {
        RuleFor(v => v.UsuarioId)
            .MustAsync(UsuarioEstaActivo)
            .WithMessage("No se puede cambiar la contraseña de un usuario inactivo.")
            .MustAsync(UsuarioNoEstaBloqueado)
            .WithMessage("No se puede cambiar la contraseña de un usuario bloqueado.")
            .MustAsync(UsuarioNoEstaEliminado)
            .WithMessage("No se puede cambiar la contraseña de un usuario eliminado.");

        RuleFor(v => v)
            .MustAsync(ValidarTipoUsuario)
            .WithMessage("El tipo de usuario no permite cambios de contraseña.")
            .WithName("TipoUsuario");
    }

    private void ConfigurarValidacionesPassword()
    {
        RuleFor(v => v.PasswordNueva)
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(128)
            .WithMessage("La contraseña no puede exceder 128 caracteres.")
            .Must(TenerComplejidadSuficiente)
            .WithMessage("La contraseña debe contener al menos: 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial.")
            .Must(NoContenerPatronesProhibidos)
            .WithMessage("La contraseña contiene patrones comunes no permitidos.")
            .MustAsync(NoEsPasswordAnterior)
            .WithMessage("No se puede reutilizar una contraseña anterior.")
            .Must(NoContenerInformacionPersonal)
            .WithMessage("La contraseña no puede contener información personal del usuario.");

        RuleFor(v => v.PasswordActual)
            .NotEmpty()
            .WithMessage("La contraseña actual es requerida.")
            .When(v => !v.EsCambioAdministrativo() && !v.EsPrimerCambio)
            .MustAsync(PasswordActualEsCorrecta)
            .WithMessage("La contraseña actual es incorrecta.")
            .When(v => !v.EsCambioAdministrativo() && !v.EsPrimerCambio);
    }

    private void ConfigurarValidacionesSeguridad()
    {
        RuleFor(v => v)
            .MustAsync(NoExcedeLimiteDiario)
            .WithMessage("Se ha excedido el límite diario de cambios de contraseña.")
            .WithName("LimiteDiario");

        RuleFor(v => v)
            .MustAsync(NoHayCambiosRecientesSospechosos)
            .WithMessage("Se detectaron cambios recientes sospechosos. Contacte al administrador.")
            .WithName("CambiosSospechosos");

        RuleFor(v => v)
            .MustAsync(ValidarContextoSeguridad)
            .WithMessage("El contexto de seguridad del cambio no es válido.")
            .WithName("ContextoSeguridad");

        RuleFor(v => v.FechaExpiracion)
            .GreaterThan(DateTime.UtcNow.AddDays(7))
            .WithMessage("La fecha de expiración debe ser al menos 7 días en el futuro.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(365))
            .WithMessage("La fecha de expiración no puede ser más de 1 año en el futuro.")
            .When(v => v.FechaExpiracion.HasValue);
    }

    private void ConfigurarValidacionesNegocio()
    {
        RuleFor(v => v)
            .MustAsync(NoTieneTransaccionesCriticas)
            .WithMessage("No se puede cambiar la contraseña durante transacciones críticas activas.")
            .When(v => v.InvalidarSesionesActivas)
            .WithName("TransaccionesCriticas");

        RuleFor(v => v)
            .MustAsync(ValidarHorarioPermitido)
            .WithMessage("Los cambios de contraseña solo están permitidos en horario laboral.")
            .When(v => v.EsCambioCritico())
            .WithName("HorarioPermitido");

        RuleFor(v => v)
            .MustAsync(ValidarFrecuenciaCambios)
            .WithMessage("La frecuencia de cambios excede los límites establecidos.")
            .WithName("FrecuenciaCambios");

        RuleFor(v => v.ObservacionesAdicionales)
            .MaximumLength(1000)
            .WithMessage("Las observaciones no pueden exceder 1000 caracteres.")
            .When(v => !string.IsNullOrWhiteSpace(v.ObservacionesAdicionales));
    }

    private void ConfigurarValidacionesAutorizacion()
    {
        RuleFor(v => v)
            .MustAsync(UsuarioAutorizaTienePermisos)
            .WithMessage("El usuario autorizador no tiene permisos para este tipo de cambio.")
            .WithName("PermisosAutorizador");

        RuleFor(v => v)
            .MustAsync(ValidarJerarquiaAutorizacion)
            .WithMessage("La jerarquía de autorización no permite este cambio.")
            .When(v => v.EsCambioAdministrativo())
            .WithName("JerarquiaAutorizacion");

        RuleFor(v => v)
            .MustAsync(ValidarNivelAutorizacion)
            .WithMessage("El nivel de autorización es insuficiente para este tipo de cambio.")
            .When(v => v.EsCambioCritico())
            .WithName("NivelAutorizacion");
    }

    private void ConfigurarValidacionesPoliticas()
    {
        RuleFor(v => v)
            .MustAsync(CumplePoliticasEmpresariales)
            .WithMessage("El cambio no cumple con las políticas empresariales de seguridad.")
            .WithName("PoliticasEmpresariales");

        RuleFor(v => v)
            .MustAsync(ValidarPoliticaExpiracion)
            .WithMessage("La política de expiración de contraseñas no permite este cambio.")
            .When(v => v.EsPrimerCambio || v.EsCambioForzado)
            .WithName("PoliticaExpiracion");

        RuleFor(v => v)
            .MustAsync(ValidarComplejidadSegunRol)
            .WithMessage("La complejidad de la contraseña no cumple los requisitos del rol del usuario.")
            .WithName("ComplejidadSegunRol");
    }

    private void ConfigurarValidacionesAuditoria()
    {
        RuleFor(v => v.DireccionIP)
            .Matches(@"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$")
            .WithMessage("El formato de la dirección IP no es válido.")
            .When(v => !string.IsNullOrWhiteSpace(v.DireccionIP));

        RuleFor(v => v.UserAgent)
            .MaximumLength(500)
            .WithMessage("El User-Agent no puede exceder 500 caracteres.")
            .When(v => !string.IsNullOrWhiteSpace(v.UserAgent));
    }

    // Métodos de validación personalizados
    private async Task<bool> UsuarioExiste(Guid usuarioId, CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Id == usuarioId, cancellationToken);
    }

    private async Task<bool> UsuarioAutorizadorExiste(Guid autorizadorId, CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Id == autorizadorId && u.Activo, cancellationToken);
    }

    private async Task<bool> UsuarioEstaActivo(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        return usuario?.Activo == true;
    }

    private async Task<bool> UsuarioNoEstaBloqueado(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        return usuario?.FechaBloqueado == null;
    }

    private async Task<bool> UsuarioNoEstaEliminado(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        return usuario?.FechaEliminacion == null;
    }

    private async Task<bool> ValidarTipoUsuario(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioId, cancellationToken);

        if (usuario == null) return false;

        // Algunos tipos de usuarios especiales no pueden cambiar contraseña
        return usuario.Rol != "UsuarioSistema" && usuario.Rol != "UsuarioServicio";
    }

    private static bool TenerComplejidadSuficiente(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;

        return PasswordSegura.IsMatch(password);
    }

    private static bool NoContenerPatronesProhibidos(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;

        return !PatronesProhibidos.IsMatch(password);
    }

    private async Task<bool> NoEsPasswordAnterior(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // Verificar que no reutilice las últimas 5 contraseñas
        var historialPasswords = await _context.HistorialPasswords
            .Where(h => h.UsuarioId == command.UsuarioId)
            .OrderByDescending(h => h.FechaCreacion)
            .Take(5)
            .Select(h => h.PasswordHash)
            .ToListAsync(cancellationToken);

        if (!historialPasswords.Any()) return true;

        // Aquí se haría el hash de la nueva contraseña y se compararía
        // Por simplicidad, asumimos que el servicio de hash está disponible
        var hashNuevaPassword = await HashPassword(command.PasswordNueva);

        return !historialPasswords.Contains(hashNuevaPassword);
    }

    private async Task<bool> NoContenerInformacionPersonal(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioId, cancellationToken);

        if (usuario == null) return true;

        var password = command.PasswordNueva.ToLower();
        var infoPersonal = new[]
        {
            usuario.NombreCompleto?.ToLower(),
            usuario.Email?.Split('@')[0].ToLower(),
            usuario.Identificacion?.ToLower()
        }.Where(info => !string.IsNullOrWhiteSpace(info));

        return !infoPersonal.Any(info => password.Contains(info!) || info!.Contains(password));
    }

    private async Task<bool> PasswordActualEsCorrecta(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioId, cancellationToken);

        if (usuario == null) return false;

        // Verificar el hash de la contraseña actual
        var hashPasswordActual = await HashPassword(command.PasswordActual);
        return usuario.PasswordHash == hashPasswordActual;
    }

    private async Task<bool> NoExcedeLimiteDiario(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var hoy = DateTime.Today;
        
        var cambiosHoy = await _context.EventosAuditoria
            .Where(e => e.EntidadId == command.UsuarioId &&
                       e.TipoEvento == "CambioPassword" &&
                       e.FechaEvento.Date == hoy)
            .CountAsync(cancellationToken);

        // Límite de 3 cambios por día por usuario
        return cambiosHoy < 3;
    }

    private async Task<bool> NoHayCambiosRecientesSospechosos(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var ultimaHora = DateTime.UtcNow.AddHours(-1);
        
        var cambiosRecientes = await _context.EventosAuditoria
            .Where(e => e.EntidadId == command.UsuarioId &&
                       e.TipoEvento == "CambioPassword" &&
                       e.FechaEvento >= ultimaHora)
            .CountAsync(cancellationToken);

        // Máximo 2 cambios por hora
        return cambiosRecientes < 2;
    }

    private async Task<bool> ValidarContextoSeguridad(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // Validar que la IP no esté en lista negra
        if (!string.IsNullOrWhiteSpace(command.DireccionIP))
        {
            var ipBloqueada = await _context.IPsBloqueadas
                .AnyAsync(ip => ip.DireccionIP == command.DireccionIP && ip.Activo, cancellationToken);

            if (ipBloqueada) return false;
        }

        // Validar intentos fallidos recientes desde la misma IP
        if (!string.IsNullOrWhiteSpace(command.DireccionIP))
        {
            var ultimaHora = DateTime.UtcNow.AddHours(-1);
            var intentosFallidos = await _context.EventosAuditoria
                .Where(e => e.TipoEvento == "IntentoPasswordIncorrecto" &&
                           e.DatosAdicionales!.ContainsKey("DireccionIP") &&
                           e.DatosAdicionales["DireccionIP"].ToString() == command.DireccionIP &&
                           e.FechaEvento >= ultimaHora)
                .CountAsync(cancellationToken);

            return intentosFallidos < 5;
        }

        return true;
    }

    private async Task<bool> NoTieneTransaccionesCriticas(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // Verificar que no tenga facturas abiertas
        var facturasPendientes = await _context.Facturas
            .AnyAsync(f => f.UsuarioCreaId == command.UsuarioId && 
                          f.Estado == EstadoFactura.Pendiente, cancellationToken);

        if (facturasPendientes) return false;

        // Verificar que no tenga comandas activas
        var comandasActivas = await _context.Comandas
            .AnyAsync(c => c.UsuarioAsignadoId == command.UsuarioId && 
                          c.Estado != EstadoComanda.Completada && 
                          c.Estado != EstadoComanda.Cancelada, cancellationToken);

        return !comandasActivas;
    }

    private async Task<bool> ValidarHorarioPermitido(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var horaActual = DateTime.Now.TimeOfDay;
        var horaInicio = new TimeSpan(6, 0, 0); // 6:00 AM
        var horaFin = new TimeSpan(22, 0, 0);   // 10:00 PM

        // Los cambios críticos solo en horario laboral
        return horaActual >= horaInicio && horaActual <= horaFin;
    }

    private async Task<bool> ValidarFrecuenciaCambios(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var ultimaSemana = DateTime.UtcNow.AddDays(-7);
        
        var cambiosRecientes = await _context.EventosAuditoria
            .Where(e => e.EntidadId == command.UsuarioId &&
                       e.TipoEvento == "CambioPassword" &&
                       e.FechaEvento >= ultimaSemana)
            .CountAsync(cancellationToken);

        // Máximo 3 cambios por semana
        return cambiosRecientes < 3;
    }

    private async Task<bool> UsuarioAutorizaTienePermisos(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        if (autorizador == null) return false;

        // Si es el mismo usuario, siempre puede cambiar su contraseña (excepto casos especiales)
        if (command.UsuarioId == command.UsuarioAutorizaId)
        {
            return true;
        }

        // Para cambios administrativos, verificar permisos
        return autorizador.Permisos?.Contains("GestionarUsuarios") == true ||
               autorizador.Rol == "Administrador" ||
               autorizador.Rol == "SuperAdministrador" ||
               autorizador.NivelAcceso >= 8;
    }

    private async Task<bool> ValidarJerarquiaAutorizacion(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioId, cancellationToken);

        if (autorizador == null || usuario == null) return false;

        // El autorizador debe tener nivel igual o superior
        return autorizador.NivelAcceso >= usuario.NivelAcceso;
    }

    private async Task<bool> ValidarNivelAutorizacion(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        if (autorizador == null) return false;

        // Para cambios críticos se requiere nivel mínimo
        var nivelMinimoRequerido = command.Prioridad switch
        {
            4 => 8, // Crítica
            3 => 6, // Alta
            2 => 4, // Normal
            1 => 2, // Baja
            _ => 4
        };

        return autorizador.NivelAcceso >= nivelMinimoRequerido;
    }

    private async Task<bool> CumplePoliticasEmpresariales(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // Validar que cumple con políticas específicas de la empresa
        
        // Política: No cambios masivos en horario no laboral
        if (command.EsCambioCritico())
        {
            var horaActual = DateTime.Now.Hour;
            if (horaActual < 6 || horaActual > 22) return false;
        }

        // Política: Cambios forzados requieren documentación
        if (command.EsCambioForzado && string.IsNullOrWhiteSpace(command.ObservacionesAdicionales))
        {
            return false;
        }

        return true;
    }

    private async Task<bool> ValidarPoliticaExpiracion(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // Verificar que la política de expiración sea válida según el tipo de cambio
        if (!command.FechaExpiracion.HasValue) return true;

        var diasExpiracion = (command.FechaExpiracion.Value - DateTime.UtcNow).Days;

        return command.ObtenerTipoCambio() switch
        {
            "Primer Cambio" => diasExpiracion >= 30 && diasExpiracion <= 90,
            "Cambio Forzado Crítico" => diasExpiracion >= 15 && diasExpiracion <= 60,
            "Cambio Administrativo" => diasExpiracion >= 7 && diasExpiracion <= 30,
            _ => diasExpiracion >= 30 && diasExpiracion <= 365
        };
    }

    private async Task<bool> ValidarComplejidadSegunRol(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioId, cancellationToken);

        if (usuario == null) return false;

        var password = command.PasswordNueva;

        // Complejidad adicional según rol
        return usuario.Rol switch
        {
            "SuperAdministrador" => password.Length >= 12 && TieneCaracteresEspecialesAvanzados(password),
            "Administrador" => password.Length >= 10 && TenerComplejidadSuficiente(password),
            "Gerente" => password.Length >= 8 && TenerComplejidadSuficiente(password),
            _ => TenerComplejidadSuficiente(password)
        };
    }

    private static bool TieneCaracteresEspecialesAvanzados(string password)
    {
        var caracteresEspecialesAvanzados = new[] { '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '+', '=', '[', ']', '{', '}', '|', ';', ':', '"', '<', '>', ',', '.', '?', '/' };
        return caracteresEspecialesAvanzados.Count(c => password.Contains(c)) >= 2;
    }

    private async Task<string> HashPassword(string password)
    {
        // Implementación simplificada - en producción usar BCrypt o similar
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
} 