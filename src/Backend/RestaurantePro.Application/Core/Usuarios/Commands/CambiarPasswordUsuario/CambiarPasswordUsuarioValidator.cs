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
            .WithMessage("La nueva contraseña es requerida.")
            .MinimumLength(8)
            .WithMessage("La nueva contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(128)
            .WithMessage("La nueva contraseña no puede exceder 128 caracteres.")
            .Must(password => TenerComplejidadSuficiente(password))
            .WithMessage("La contraseña debe contener al menos: 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial.")
            .Must(password => NoContenerPatronesProhibidos(password))
            .WithMessage("La contraseña contiene patrones comunes no permitidos.")
            .Must((command, password, context) => NoEsPasswordAnteriorSync(command))
            .WithMessage("No se puede reutilizar una contraseña anterior.")
            .Must((command, password, context) => NoContenerInformacionPersonalSync(command))
            .WithMessage("La contraseña no puede contener información personal del usuario.");

        RuleFor(v => v.PasswordActual)
            .NotEmpty()
            .WithMessage("La contraseña actual es requerida.")
            .MinimumLength(8)
            .WithMessage("La contraseña actual debe tener al menos 8 caracteres.")
            .MaximumLength(128)
            .WithMessage("La contraseña actual no puede exceder 128 caracteres.")
            .MustAsync((command, passwordActual, cancellationToken) => PasswordActualEsCorrectaAsync(command.UsuarioId, passwordActual, cancellationToken))
            .WithMessage("La contraseña actual no es correcta.");
    }

    private void ConfigurarValidacionesUsuario()
    {
        RuleFor(v => v.UsuarioId)
            .MustAsync(UsuarioEstaActivoAsync)
            .WithMessage("No se puede cambiar la contraseña de un usuario inactivo.")
            .MustAsync(UsuarioNoEstaBloqueadoAsync)
            .WithMessage("No se puede cambiar la contraseña de un usuario bloqueado.")
            .MustAsync(UsuarioNoEstaEliminadoAsync)
            .WithMessage("No se puede cambiar la contraseña de un usuario eliminado.");

        RuleFor(v => v)
            .MustAsync(ValidarTipoUsuario)
            .WithMessage("El tipo de usuario no permite cambios de contraseña.")
            .WithName("TipoUsuario");

        // TODO: Descomentar cuando Usuario tenga propiedad Activo
        // RuleFor(x => x.UsuarioId)
        //     .MustAsync(async (usuarioId, cancellation) =>
        //     {
        //         var usuario = await context.Usuarios.FindAsync(usuarioId);
        //         return usuario?.Activo == true;
        //     })
        //     .WithMessage("No se puede cambiar la contraseña de un usuario inactivo.")
        //     .When(x => !x.EsCambioAdministrativo());

        // 8.2. Usuario con rol específico para cambios críticos
        // TODO: Descomentar cuando Usuario tenga propiedad Rol
        // RuleFor(x => x)
        //     .MustAsync(async (command, cancellation) =>
        //     {
        //         var usuario = await context.Usuarios.FindAsync(command.UsuarioId);
        //         if (usuario == null) return false;
        //
        //         // Solo SuperAdmins y Admins pueden cambiar passwords de otros roles críticos
        //         return !(usuario.Rol == "SuperAdministrador" || usuario.Rol == "Administrador") ||
        //                command.EsCambioAdministrativo();
        //     })
        //     .WithMessage("Los cambios de contraseña para roles críticos requieren autorización administrativa.")
        //     .When(x => x.EsCambioCritico());
    }

    private void ConfigurarValidacionesPassword()
    {
        RuleFor(v => v.PasswordActual)
            .NotEmpty()
            .WithMessage("La contraseña actual es requerida.")
            .MinimumLength(8)
            .WithMessage("La contraseña actual debe tener al menos 8 caracteres.")
            .MaximumLength(128)
            .WithMessage("La contraseña actual no puede exceder 128 caracteres.")
            .MustAsync((command, passwordActual, cancellationToken) => PasswordActualEsCorrectaAsync(command.UsuarioId, passwordActual, cancellationToken))
            .WithMessage("La contraseña actual no es correcta.");
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
            .AnyAsync(u => u.Id == autorizadorId && u.Estado == EstadoUsuario.Activo, cancellationToken);
    }

    private async Task<bool> UsuarioEstaActivoAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
        
        // TODO: Usar propiedad real Estado en lugar de Activo
        return usuario != null && usuario.Estado == EstadoUsuario.Activo;
    }

    private async Task<bool> UsuarioNoEstaBloqueadoAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
        
        // TODO: Descomentar cuando Usuario tenga FechaBloqueado
        // return usuario != null && usuario.FechaBloqueado == null;
        return usuario != null && usuario.Estado != EstadoUsuario.Bloqueado;
    }

    private async Task<bool> UsuarioNoEstaEliminadoAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
        
        // TODO: Descomentar cuando Usuario tenga FechaEliminacion
        // return usuario != null && usuario.FechaEliminacion == null;
        return usuario != null; // Temporal: asumir que no está eliminado
    }

    private async Task<bool> ValidarTipoUsuario(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioId, cancellationToken);

        if (usuario == null) return false;

        // TODO: Descomentar cuando Usuario tenga propiedad Rol
        // Algunos tipos de usuarios especiales no pueden cambiar contraseña
        // return usuario.Rol != "UsuarioSistema" && usuario.Rol != "UsuarioServicio";
        
        // Temporal: usar TipoUsuario en lugar de Rol
        return usuario.TipoUsuario != TipoUsuario.Administrador; // Cambiar Sistema por Administrador
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

    private bool NoEsPasswordAnteriorSync(CambiarPasswordUsuarioCommand command)
    {
        // TODO: Descomentar cuando tengamos tabla HistorialPasswords
        // Verificar que no reutilice las últimas 5 contraseñas
        // var historialPasswords = await _context.HistorialPasswords
        //     .Where(h => h.UsuarioId == command.UsuarioId)
        //     .OrderByDescending(h => h.FechaCreacion)
        //     .Take(5)
        //     .Select(h => h.PasswordHash)
        //     .ToListAsync(cancellationToken);
        //
        // if (!historialPasswords.Any()) return true;
        //
        // // Aquí se haría el hash de la nueva contraseña y se compararía
        // // Por simplicidad, asumimos que el servicio de hash está disponible
        // var hashNuevaPassword = await HashPassword(command.PasswordNueva);
        //
        // return !historialPasswords.Contains(hashNuevaPassword);
        
        return true; // Temporal: asumir que no es password anterior
    }

    private bool NoContenerInformacionPersonalSync(CambiarPasswordUsuarioCommand command)
    {
        var usuario = _context.Usuarios
            .FirstOrDefault(u => u.Id == command.UsuarioId);

        if (usuario == null) return true;

        var password = command.PasswordNueva.ToLower();
        var infoPersonal = new[]
        {
            usuario.NombreCompleto?.ToLower(),
            usuario.Email?.Split('@')[0].ToLower(),
            // TODO: Descomentar cuando Usuario tenga Identificacion
            // usuario.Identificacion?.ToLower()
        }.Where(info => !string.IsNullOrWhiteSpace(info));

        return !infoPersonal.Any(info => password.Contains(info!) || info!.Contains(password));
    }

    private async Task<bool> PasswordActualEsCorrectaAsync(Guid usuarioId, string passwordActual, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
        
        // TODO: Descomentar cuando Usuario tenga PasswordHash
        // return usuario != null && _passwordHasher.VerifyPassword(passwordActual, usuario.PasswordHash);
        return await Task.FromResult(true); // Temporal: asumir que es correcta
    }

    private async Task<bool> NoExcedeLimiteDiario(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando tengamos tabla EventosAuditoria
        // var hoy = DateTime.Today;
        // 
        // var cambiosHoy = await _context.EventosAuditoria
        //     .Where(e => e.EntidadId == command.UsuarioId &&
        //                e.TipoEvento == "CambioPassword" &&
        //                e.FechaEvento.Date == hoy)
        //     .CountAsync(cancellationToken);
        //
        // // Límite de 3 cambios por día por usuario
        // return cambiosHoy < 3;
        
        return await Task.FromResult(true); // Temporal: asumir que no excede límite
    }

    private async Task<bool> NoHayCambiosRecientesSospechosos(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando tengamos tabla EventosAuditoria
        // var ultimaHora = DateTime.UtcNow.AddHours(-1);
        // 
        // var cambiosRecientes = await _context.EventosAuditoria
        //     .Where(e => e.EntidadId == command.UsuarioId &&
        //                e.TipoEvento == "CambioPassword" &&
        //                e.FechaEvento >= ultimaHora)
        //     .CountAsync(cancellationToken);
        //
        // // Máximo 2 cambios por hora
        // return cambiosRecientes < 2;
        
        return await Task.FromResult(true); // Temporal: asumir que no hay cambios sospechosos
    }

    private async Task<bool> ValidarContextoSeguridad(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando tengamos tabla IPsBloqueadas
        // Validar que la IP no esté en lista negra
        // if (!string.IsNullOrWhiteSpace(command.DireccionIP))
        // {
        //     var ipBloqueada = await _context.IPsBloqueadas
        //         .AnyAsync(ip => ip.DireccionIP == command.DireccionIP && ip.Activo, cancellationToken);
        //
        //     if (ipBloqueada) return false;
        // }

        // TODO: Descomentar cuando tengamos tabla EventosAuditoria
        // Validar intentos fallidos recientes desde la misma IP
        // if (!string.IsNullOrWhiteSpace(command.DireccionIP))
        // {
        //     var ultimaHora = DateTime.UtcNow.AddHours(-1);
        //     var intentosFallidos = await _context.EventosAuditoria
        //         .Where(e => e.TipoEvento == "IntentoPasswordIncorrecto" &&
        //                    e.DatosAdicionales!.ContainsKey("DireccionIP") &&
        //                    e.DatosAdicionales["DireccionIP"].ToString() == command.DireccionIP &&
        //                    e.FechaEvento >= ultimaHora)
        //         .CountAsync(cancellationToken);
        //
        //     return intentosFallidos < 5;
        // }

        return await Task.FromResult(true); // Temporal: asumir que contexto es seguro
    }

    private async Task<bool> NoTieneTransaccionesCriticas(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando Factura tenga UsuarioCreaId
        // Verificar que no tenga facturas abiertas
        // var facturasPendientes = await _context.Facturas
        //     .AnyAsync(f => f.UsuarioCreaId == command.UsuarioId && 
        //               f.Estado == EstadoFactura.Pendiente, cancellationToken);
        //
        // if (facturasPendientes) return false;

        // TODO: Descomentar cuando Comanda tenga UsuarioAsignadoId y EstadoComanda.Finalizada
        // Verificar que no tenga comandas activas
        // var comandasActivas = await _context.Comandas
        //     .AnyAsync(c => c.UsuarioAsignadoId == command.UsuarioId && 
        //               c.Estado != EstadoComanda.Finalizada && 
        //               c.Estado != EstadoComanda.Cancelada, cancellationToken);

        // return !comandasActivas;
        return await Task.FromResult(true); // Temporal: asumir que no tiene transacciones críticas
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
        // TODO: Descomentar cuando tengamos tabla EventosAuditoria
        // var ultimaSemana = DateTime.UtcNow.AddDays(-7);
        // 
        // var cambiosRecientes = await _context.EventosAuditoria
        //     .Where(e => e.EntidadId == command.UsuarioId &&
        //                e.TipoEvento == "CambioPassword" &&
        //                e.FechaEvento >= ultimaSemana)
        //     .CountAsync(cancellationToken);
        //
        // // Máximo 3 cambios por semana
        // return cambiosRecientes < 3;
        
        return await Task.FromResult(true); // Temporal: asumir que no excede frecuencia
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

        // TODO: Descomentar cuando Usuario tenga Permisos, Rol, NivelAcceso
        // Para cambios administrativos, verificar permisos
        // return autorizador.Permisos?.Contains("GestionarUsuarios") == true ||
        //        autorizador.Rol == "Administrador" ||
        //        autorizador.Rol == "SuperAdministrador" ||
        //        autorizador.NivelAcceso >= 8;
        
        // Temporal: usar EsAdministrador
        return autorizador.EsAdministrador;
    }

    private async Task<bool> ValidarJerarquiaAutorizacion(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioId, cancellationToken);

        if (autorizador == null || usuario == null) return false;

        // TODO: Descomentar cuando Usuario tenga NivelAcceso
        // El autorizador debe tener nivel igual o superior
        // return autorizador.NivelAcceso >= usuario.NivelAcceso;
        
        // Temporal: usar EsAdministrador
        return autorizador.EsAdministrador || autorizador.Id == usuario.Id;
    }

    private async Task<bool> ValidarNivelAutorizacion(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        if (autorizador == null) return false;

        // TODO: Descomentar cuando Usuario tenga NivelAcceso
        // Nivel 8 o superior para cambios críticos
        // return autorizador.NivelAcceso >= 8 || autorizador.Rol == "SuperAdministrador";
        
        // Temporal: usar EsAdministrador
        return autorizador.EsAdministrador;
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
        
        // Verificar complejidad según "rol simulado"
        var requiresComplexity = password.Length >= 12 &&
                                password.Any(char.IsUpper) &&
                                password.Any(char.IsLower) &&
                                password.Any(char.IsDigit) &&
                                password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c));

        // TODO: Descomentar cuando Usuario tenga propiedad Rol
        /*
        // Complejidad adicional según rol
        return usuario.Rol switch
        {
            "SuperAdministrador" => requiresComplexity,
            "Administrador" => requiresComplexity,
            "Gerente" => requiresComplexity,
            _ => requiresComplexity
        };
        */
        
        return requiresComplexity;
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

    private async Task<bool> UsuarioTienePermisosParaCambiarPasswordAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
        
        // TODO: Implementar validación real cuando Usuario tenga propiedades de roles específicos
        return usuario != null && (usuario.EsAdministrador || usuario.Roles.Any(r => r == RolUsuario.Gerente));
    }

    private async Task<bool> ValidarUsuarioAdministrativo(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
        
        // TODO: Usar propiedades reales de Usuario en lugar de Identificacion
        // return usuario?.Identificacion != null;
        return usuario != null; // Temporal: asumir que es válido
    }

    private async Task<bool> ValidarEventosAuditoriaAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando tengamos tabla EventosAuditoria
        // var eventosSospechosos = await _context.EventosAuditoria
        //     .Where(e => e.UsuarioId == command.UsuarioId && 
        //                 e.TipoEvento == "CambioPasswordFallido" &&
        //                 e.FechaEvento >= DateTime.UtcNow.AddHours(-24))
        //     .CountAsync(cancellationToken);
        
        // return eventosSospechosos < 5;
        return await Task.FromResult(true); // Temporal: asumir que es válido
    }

    private async Task<bool> ValidarEventosAuditoriaUsuarioAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando tengamos tabla EventosAuditoria
        // var eventosRecientes = await _context.EventosAuditoria
        //     .Where(e => e.UsuarioId == command.UsuarioId && 
        //                 e.TipoEvento == "CambioPassword" &&
        //                 e.FechaEvento >= DateTime.UtcNow.AddDays(-1))
        //     .CountAsync(cancellationToken);
        
        // return eventosRecientes < 3;
        return await Task.FromResult(true); // Temporal: asumir que es válido
    }

    private async Task<bool> ValidarIPsBloqueadasAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando tengamos tabla IPsBloqueadas
        // var ipBloqueada = await _context.IPsBloqueadas
        //     .AnyAsync(ip => ip.DireccionIP == command.IPAddress && ip.Activa, cancellationToken);
        
        // return !ipBloqueada;
        return await Task.FromResult(true); // Temporal: asumir que es válida
    }

    private async Task<bool> ValidarLimitesEventosAuditoriaAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando tengamos tabla EventosAuditoria
        // var eventosRecientes = await _context.EventosAuditoria
        //     .Where(e => e.FechaEvento >= DateTime.UtcNow.AddHours(-1))
        //     .CountAsync(cancellationToken);
        
        // return eventosRecientes < 100;
        return await Task.FromResult(true); // Temporal: asumir que es válido
    }

    private async Task<bool> ValidarFacturasPendientesAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando Factura tenga UsuarioCreaId y EstadoFactura.Pendiente
        // var facturasPendientes = await _context.Facturas
        //     .Where(f => f.UsuarioCreaId == command.UsuarioId && 
        //                 f.Estado == EstadoFactura.Pendiente)
        //     .CountAsync(cancellationToken);
        
        // TODO: Descomentar cuando Comanda tenga UsuarioAsignadoId y EstadoComanda.Finalizada
        // var comandasPendientes = await _context.Comandas
        //     .Where(c => c.UsuarioAsignadoId == command.UsuarioId && 
        //                 c.Estado != EstadoComanda.Finalizada && c.Estado != EstadoComanda.Cancelada)
        //     .CountAsync(cancellationToken);
        
        // return facturasPendientes == 0 && comandasPendientes == 0;
        return await Task.FromResult(true); // Temporal: asumir que no hay pendientes
    }

    private async Task<bool> ValidarLimitesActualizacionesDiariaAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando tengamos tabla EventosAuditoria
        // var cambiosHoy = await _context.EventosAuditoria
        //     .Where(e => e.UsuarioId == command.UsuarioId && 
        //                 e.TipoEvento == "CambioPassword" &&
        //                 e.FechaEvento.Date == DateTime.UtcNow.Date)
        //     .CountAsync(cancellationToken);
        
        // return cambiosHoy < 3;
        return await Task.FromResult(true); // Temporal: asumir que no excede límites
    }

    private async Task<bool> ValidarControlAccesoAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);
        
        // TODO: Implementar validación real cuando Usuario tenga propiedades de permisos y roles
        // return autorizador?.Permisos?.Contains("CambiarPassword") ?? false ||
        //        autorizador?.Rol == "Administrador" ||
        //        autorizador?.NivelAcceso >= 8;
        return autorizador != null && autorizador.EsAdministrador; // Temporal: solo administradores
    }

    private async Task<bool> ValidarAutorizacionNivelesAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioId, cancellationToken);
        
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);
        
        // TODO: Implementar validación real cuando Usuario tenga NivelAcceso
        // return autorizador?.NivelAcceso >= usuario?.NivelAcceso;
        return usuario != null && autorizador != null; // Temporal: asumir que es válido
    }

    private async Task<bool> ValidarContextoNivelesAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);
        
        // TODO: Implementar validación real cuando Usuario tenga NivelAcceso
        // return autorizador?.NivelAcceso >= 7;
        return autorizador != null; // Temporal: asumir que es válido
    }

    private async Task<bool> ValidarHorarioLaboralAsync(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioId, cancellationToken);
        
        // TODO: Implementar validación real cuando Usuario tenga propiedades de rol específico
        return usuario != null && usuario.EsAdministrador; // Temporal: usar propiedades reales
    }

    private async Task<bool> CumplePoliticasEmpresariales(CambiarPasswordUsuarioCommand command, CancellationToken cancellationToken)
    {
        // Políticas básicas que no dependen de datos específicos
        
        // 1. Password no puede ser la fecha actual
        var fechaActual = DateTime.Now.ToString("ddMMyyyy");
        if (command.PasswordNueva.Contains(fechaActual)) return false;

        // 2. Password no puede ser "password" + número
        var passwordComun = System.Text.RegularExpressions.Regex.IsMatch(
            command.PasswordNueva, @"^password\d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (passwordComun) return false;

        // TODO: Agregar más validaciones cuando tengamos datos corporativos
        // 3. Password no puede contener nombre de la empresa
        // 4. Password no puede ser números secuenciales
        
        return true;
    }
} 