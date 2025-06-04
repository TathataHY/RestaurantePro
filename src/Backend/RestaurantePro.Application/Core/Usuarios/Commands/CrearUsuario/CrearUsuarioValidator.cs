namespace RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario;

public class CrearUsuarioValidator : AbstractValidator<CrearUsuarioCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly string[] _rolesValidos = { "Empleado", "Supervisor", "Gerente", "Administrador", "SuperAdministrador" };
    private readonly string[] _departamentosValidos = { "Cocina", "Servicio", "Administración", "Limpieza", "Seguridad", "Sistemas" };
    private readonly string[] _permisosValidos = {
        "GestionarUsuarios", "GestionarRoles", "VerReportes", "VerTodosReportes", 
        "ConfigurarSistema", "GestionarSucursales", "GestionarEmpleados", 
        "AprobarDescuentos", "GestionarInventario", "GestionarProveedores"
    };

    public CrearUsuarioValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesCredenciales();
        ConfigurarValidacionesRoles();
        ConfigurarValidacionesContacto();
        ConfigurarValidacionesOrganizacion();
        ConfigurarValidacionesHorarios();
        ConfigurarValidacionesSeguridad();
        ConfigurarValidacionesNegocio();
    }

    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(v => v.NombreUsuario)
            .NotEmpty()
            .WithMessage("El nombre de usuario es requerido.")
            .MinimumLength(3)
            .WithMessage("El nombre de usuario debe tener al menos 3 caracteres.")
            .MaximumLength(50)
            .WithMessage("El nombre de usuario no puede exceder 50 caracteres.")
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos.")
            .MustAsync(NombreUsuarioEsUnico)
            .WithMessage("El nombre de usuario ya existe.");

        RuleFor(v => v.NombreCompleto)
            .NotEmpty()
            .WithMessage("El nombre completo es requerido.")
            .MinimumLength(2)
            .WithMessage("El nombre completo debe tener al menos 2 caracteres.")
            .MaximumLength(200)
            .WithMessage("El nombre completo no puede exceder 200 caracteres.")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s']+$")
            .WithMessage("El nombre completo solo puede contener letras, espacios, acentos y apostrofes.");

        RuleFor(v => v.Email)
            .NotEmpty()
            .WithMessage("El email es requerido.")
            .MaximumLength(320)
            .WithMessage("El email no puede exceder 320 caracteres.")
            .Must(BeValidEmail)
            .WithMessage("El email no tiene un formato válido.")
            .MustAsync(EmailEsUnico)
            .WithMessage("El email ya está en uso.");
    }

    private void ConfigurarValidacionesCredenciales()
    {
        RuleFor(v => v.Password)
            .NotEmpty()
            .WithMessage("La contraseña es requerida.")
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(128)
            .WithMessage("La contraseña no puede exceder 128 caracteres.")
            .Must(TenerPasswordSegura)
            .WithMessage("La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial.");

        RuleFor(v => v.ConfirmarPassword)
            .NotEmpty()
            .WithMessage("La confirmación de contraseña es requerida.")
            .Equal(v => v.Password)
            .WithMessage("La confirmación de contraseña debe coincidir con la contraseña.");
    }

    private void ConfigurarValidacionesRoles()
    {
        RuleFor(v => v.Rol)
            .NotEmpty()
            .WithMessage("El rol es requerido.")
            .Must(rol => _rolesValidos.Contains(rol, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El rol debe ser uno de: {string.Join(", ", _rolesValidos)}.");

        RuleFor(v => v.RolesAdicionales)
            .Must(roles => roles.All(rol => _rolesValidos.Contains(rol, StringComparer.OrdinalIgnoreCase)))
            .WithMessage($"Todos los roles adicionales deben ser válidos: {string.Join(", ", _rolesValidos)}.")
            .Must(roles => roles.Count <= 3)
            .WithMessage("No se pueden asignar más de 3 roles adicionales.");

        RuleFor(v => v.NivelAcceso)
            .GreaterThanOrEqualTo(1)
            .WithMessage("El nivel de acceso mínimo es 1.")
            .LessThanOrEqualTo(10)
            .WithMessage("El nivel de acceso máximo es 10.")
            .Must((command, nivelAcceso) => ValidarNivelSegunRol(command.Rol, nivelAcceso))
            .WithMessage("El nivel de acceso no es compatible con el rol asignado.");

        RuleFor(v => v.PermisosEspecificos)
            .Must(permisos => permisos.All(permiso => _permisosValidos.Contains(permiso, StringComparer.OrdinalIgnoreCase)))
            .WithMessage($"Todos los permisos deben ser válidos: {string.Join(", ", _permisosValidos)}.")
            .Must(permisos => permisos.Count <= 10)
            .WithMessage("No se pueden asignar más de 10 permisos específicos.");
    }

    private void ConfigurarValidacionesContacto()
    {
        RuleFor(v => v.Telefono)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("El formato del teléfono no es válido.")
            .When(v => !string.IsNullOrEmpty(v.Telefono));
    }

    private void ConfigurarValidacionesOrganizacion()
    {
        RuleFor(v => v.Departamento)
            .Must(departamento => string.IsNullOrEmpty(departamento) || _departamentosValidos.Contains(departamento, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El departamento debe ser uno de: {string.Join(", ", _departamentosValidos)}.")
            .When(v => !string.IsNullOrEmpty(v.Departamento));

        RuleFor(v => v.Puesto)
            .MaximumLength(100)
            .WithMessage("El puesto no puede exceder 100 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Puesto));

        // TODO: Descomentar cuando tengamos tabla Sucursales
        // RuleFor(v => v.SucursalId)
        //     .MustAsync(async (sucursalId, cancellation) =>
        //     {
        //         if (!sucursalId.HasValue) return true;
        //         return await _context.Sucursales.AnyAsync(s => s.Id == sucursalId.Value, cancellation);
        //     })
        //     .WithMessage("La sucursal especificada no existe.")
        //     .When(v => v.SucursalId.HasValue);

        // TODO: Descomentar cuando Usuario tenga propiedad Activo
        // RuleFor(v => v.SupervisorId)
        //     .MustAsync(async (supervisorId, cancellation) =>
        //     {
        //         if (!supervisorId.HasValue) return true;
        //         var supervisor = await _context.Usuarios.FindAsync(supervisorId.Value);
        //         return supervisor?.Activo == true;
        //     })
        //     .WithMessage("El supervisor especificado no está activo.")
        //     .When(v => v.SupervisorId.HasValue);

        // TODO: Descomentar cuando Usuario tenga propiedad Activo
        // RuleFor(v => v.SupervisorId)
        //     .MustAsync(async (supervisorId, cancellation) =>
        //     {
        //         if (!supervisorId.HasValue) return true;
        //         var supervisor = await _context.Usuarios.FindAsync(supervisorId.Value);
        //         return supervisor?.Activo == true;
        //     })
        //     .WithMessage("Solo se puede asignar un supervisor activo.")
        //     .When(v => v.SupervisorId.HasValue);

        RuleFor(v => v.UsuarioCreadorId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del usuario creador es requerido.")
            .MustAsync(async (usuarioCreadorId, cancellationToken) =>
            {
                return await _context.Usuarios
                    .AnyAsync(u => u.Id == usuarioCreadorId && u.Estado == EstadoUsuario.Activo, cancellationToken);
            })
            .WithMessage("El usuario creador especificado no existe.");
    }

    private void ConfigurarValidacionesHorarios()
    {
        RuleFor(v => v.HorariosTrabajo)
            .Must(horarios => horarios.Count <= 7)
            .WithMessage("No se pueden configurar más de 7 horarios (uno por día).");

        RuleForEach(v => v.HorariosTrabajo)
            .ChildRules(horario =>
            {
                horario.RuleFor(h => h.DiaSemana)
                    .NotEmpty()
                    .WithMessage("El día de la semana es requerido.")
                    .Must(dia => new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" }
                        .Contains(dia, StringComparer.OrdinalIgnoreCase))
                    .WithMessage("El día de la semana no es válido.");

                horario.RuleFor(h => h.HoraInicio)
                    .LessThan(h => h.HoraFin)
                    .WithMessage("La hora de inicio debe ser anterior a la hora de fin.")
                    .When(h => !h.EsDiaLibre);

                horario.RuleFor(h => h.HoraFin)
                    .GreaterThan(h => h.HoraInicio)
                    .WithMessage("La hora de fin debe ser posterior a la hora de inicio.")
                    .When(h => !h.EsDiaLibre);
            });
    }

    private void ConfigurarValidacionesSeguridad()
    {
        RuleFor(v => v.FechaIngreso)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(30))
            .WithMessage("La fecha de ingreso no puede ser más de 30 días en el futuro.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-10))
            .WithMessage("La fecha de ingreso no puede ser más de 10 años en el pasado.")
            .When(v => v.FechaIngreso.HasValue);

        RuleFor(v => v.NotasAdministrativas)
            .MaximumLength(1000)
            .WithMessage("Las notas administrativas no pueden exceder 1000 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.NotasAdministrativas));

        RuleFor(v => v.ConfiguracionPersonal)
            .Must(config => config.Count <= 20)
            .WithMessage("No se pueden configurar más de 20 parámetros personales.");
    }

    private void ConfigurarValidacionesNegocio()
    {
        // Validación de jerarquía organizacional
        RuleFor(v => v)
            .MustAsync(ValidarJerarquiaOrganizacional)
            .WithMessage("La configuración de roles y supervisión no es válida.")
            .WithName("JerarquiaOrganizacional");

        // Validación de permisos según rol
        RuleFor(v => v)
            .Must(ValidarPermisosSegunRol)
            .WithMessage("Los permisos asignados no son compatibles con el rol.")
            .WithName("PermisosCompatibles");
    }

    // Métodos de validación personalizados
    private async Task<bool> NombreUsuarioEsUnico(string nombreUsuario, CancellationToken cancellationToken)
    {
        return !await _context.Usuarios
            .AnyAsync(u => u.NombreUsuario == nombreUsuario, cancellationToken);
    }

    private async Task<bool> EmailEsUnico(string email, CancellationToken cancellationToken)
    {
        return !await _context.Usuarios
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    private static bool BeValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Expresión regular más estricta para validación de email
        // Esta versión mejorada rechaza puntos consecutivos y dominios que empiezan o terminan con punto
        var regex = new Regex(@"^[a-zA-Z0-9](?:[a-zA-Z0-9_%+-]+(?:\.[a-zA-Z0-9_%+-]+)*)?@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$");
        return regex.IsMatch(email);
    }

    private static bool TenerPasswordSegura(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;

        var tieneMayuscula = password.Any(char.IsUpper);
        var tieneMinuscula = password.Any(char.IsLower);
        var tieneNumero = password.Any(char.IsDigit);
        var tieneEspecial = password.Any(c => !char.IsLetterOrDigit(c));

        return tieneMayuscula && tieneMinuscula && tieneNumero && tieneEspecial;
    }

    private static bool ValidarNivelSegunRol(string rol, int nivelAcceso)
    {
        return rol.ToLower() switch
        {
            "empleado" => nivelAcceso <= 3,
            "supervisor" => nivelAcceso >= 2 && nivelAcceso <= 6,
            "gerente" => nivelAcceso >= 4 && nivelAcceso <= 8,
            "administrador" => nivelAcceso >= 6 && nivelAcceso <= 9,
            "superadministrador" => nivelAcceso >= 8 && nivelAcceso <= 10,
            _ => false
        };
    }

    private async Task<bool> ValidarJerarquiaOrganizacional(CrearUsuarioCommand command, CancellationToken cancellationToken)
    {
        // TODO: Descomentar cuando Usuario tenga SupervisorId, NivelAcceso y Rol
        // Si tiene supervisor, verificar que el supervisor tenga un rol superior
        // if (command.SupervisorId.HasValue)
        // {
        //     var supervisor = await _context.Usuarios
        //         .FirstOrDefaultAsync(u => u.Id == command.SupervisorId.Value, cancellationToken);
        //
        //     if (supervisor != null)
        //     {
        //         // El supervisor debe tener un nivel de acceso mayor
        //         // if (supervisor.NivelAcceso <= command.NivelAcceso)
        //         // {
        //         //     return false;
        //         // }
        //
        //         // Verificar compatibilidad de roles
        //         // var rolesSuperiores = new[] { "Supervisor", "Gerente", "Administrador", "SuperAdministrador" };
        //         // if (!rolesSuperiores.Contains(supervisor.Rol, StringComparer.OrdinalIgnoreCase))
        //         // {
        //         //     return false;
        //         // }
        //     }
        // }

        return await Task.FromResult(true); // Temporal: asumir que la jerarquía es válida
    }

    private static bool ValidarPermisosSegunRol(CrearUsuarioCommand command)
    {
        var permisosAdministrativos = new[] { "GestionarUsuarios", "GestionarRoles", "ConfigurarSistema" };
        
        // Los empleados no pueden tener permisos administrativos
        if (command.Rol.Equals("Empleado", StringComparison.OrdinalIgnoreCase))
        {
            return !command.PermisosEspecificos.Any(p => permisosAdministrativos.Contains(p, StringComparer.OrdinalIgnoreCase));
        }

        return true;
    }
} 