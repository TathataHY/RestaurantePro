namespace RestaurantePro.Application.Core.Usuarios.Commands.ActualizarUsuario;

public class ActualizarUsuarioValidator : AbstractValidator<ActualizarUsuarioCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly string[] _rolesValidos = { "Empleado", "Supervisor", "Gerente", "Administrador", "SuperAdministrador" };
    private readonly string[] _departamentosValidos = { "Cocina", "Servicio", "Administración", "Gerencia", "Mantenimiento" };

    public ActualizarUsuarioValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesUsuario();
        ConfigurarValidacionesContacto();
        ConfigurarValidacionesRolPermisos();
        ConfigurarValidacionesJerarquia();
        ConfigurarValidacionesLaborales();
        ConfigurarValidacionesSeguridad();
        ConfigurarValidacionesNegocio();
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
            .WithMessage("El ID del usuario que autoriza es requerido.")
            .MustAsync(UsuarioAutorizadorExiste)
            .WithMessage("El usuario autorizador especificado no existe.");

        RuleFor(v => v.MotivoActualizacion)
            .NotEmpty()
            .WithMessage("El motivo de actualización es requerido.")
            .MinimumLength(10)
            .WithMessage("El motivo debe tener al menos 10 caracteres.")
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder 500 caracteres.");

        RuleFor(v => v.Prioridad)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La prioridad mínima es 1.")
            .LessThanOrEqualTo(4)
            .WithMessage("La prioridad máxima es 4.");

        RuleFor(v => v)
            .Must(command => command.TieneAlMenosUnCambio())
            .WithMessage("Se debe especificar al menos un campo para actualizar.")
            .WithName("AlMenosUnCambio");
    }

    private void ConfigurarValidacionesUsuario()
    {
        RuleFor(v => v.UsuarioId)
            .MustAsync(UsuarioNoEstaEliminado)
            .WithMessage("No se puede actualizar un usuario eliminado.")
            .MustAsync(UsuarioNoEsElMismo)
            .WithMessage("Un usuario no puede modificar sus propios permisos críticos.");

        RuleFor(v => v.Nombre)
            .MinimumLength(2)
            .WithMessage("El nombre debe tener al menos 2 caracteres.")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres.")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")
            .WithMessage("El nombre solo puede contener letras y espacios.")
            .When(v => !string.IsNullOrWhiteSpace(v.Nombre));

        RuleFor(v => v.Identificacion)
            .MinimumLength(6)
            .WithMessage("La identificación debe tener al menos 6 caracteres.")
            .MaximumLength(20)
            .WithMessage("La identificación no puede exceder 20 caracteres.")
            .Matches(@"^[0-9A-Za-z\-]+$")
            .WithMessage("La identificación solo puede contener números, letras y guiones.")
            .MustAsync(IdentificacionEsUnica)
            .WithMessage("La identificación ya está en uso por otro usuario.")
            .When(v => !string.IsNullOrWhiteSpace(v.Identificacion));
    }

    private void ConfigurarValidacionesContacto()
    {
        RuleFor(v => v.Email)
            .EmailAddress()
            .WithMessage("El formato del email es inválido.")
            .MaximumLength(100)
            .WithMessage("El email no puede exceder 100 caracteres.")
            .MustAsync(EmailEsUnico)
            .WithMessage("El email ya está en uso por otro usuario.")
            .When(v => !string.IsNullOrWhiteSpace(v.Email));

        RuleFor(v => v.Telefono)
            .MinimumLength(7)
            .WithMessage("El teléfono debe tener al menos 7 dígitos.")
            .MaximumLength(15)
            .WithMessage("El teléfono no puede exceder 15 caracteres.")
            .Matches(@"^[\+]?[0-9\-\s\(\)]+$")
            .WithMessage("El formato del teléfono es inválido.")
            .When(v => !string.IsNullOrWhiteSpace(v.Telefono));

        RuleFor(v => v.Direccion)
            .MaximumLength(200)
            .WithMessage("La dirección no puede exceder 200 caracteres.")
            .When(v => !string.IsNullOrWhiteSpace(v.Direccion));
    }

    private void ConfigurarValidacionesRolPermisos()
    {
        RuleFor(v => v.Rol)
            .Must(rol => _rolesValidos.Contains(rol, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El rol debe ser uno de: {string.Join(", ", _rolesValidos)}.")
            .When(v => !string.IsNullOrWhiteSpace(v.Rol));

        RuleFor(v => v.NivelAcceso)
            .GreaterThanOrEqualTo(1)
            .WithMessage("El nivel de acceso mínimo es 1.")
            .LessThanOrEqualTo(10)
            .WithMessage("El nivel de acceso máximo es 10.")
            .When(v => v.NivelAcceso.HasValue);

        RuleFor(v => v)
            .MustAsync(RolEsCompatibleConNivelAcceso)
            .WithMessage("El rol especificado no es compatible con el nivel de acceso.")
            .When(v => !string.IsNullOrWhiteSpace(v.Rol) && v.NivelAcceso.HasValue)
            .WithName("CompatibilidadRolNivel");

        RuleFor(v => v)
            .MustAsync(UsuarioAutorizaTienePeermisosParaCambiarRol)
            .WithMessage("El usuario autorizador no tiene permisos para cambiar roles.")
            .When(v => !string.IsNullOrWhiteSpace(v.Rol))
            .WithName("PermisosParaCambiarRol");

        RuleFor(v => v.PermisosEspecificos)
            .Must(permisos => permisos.Count <= 20)
            .WithMessage("No se pueden asignar más de 20 permisos específicos.")
            .Must(permisos => permisos.All(p => !string.IsNullOrWhiteSpace(p)))
            .WithMessage("Todos los permisos específicos deben ser válidos.")
            .When(v => v.PermisosEspecificos.Any());
    }

    private void ConfigurarValidacionesJerarquia()
    {
        RuleFor(v => v.SupervisorId)
            .MustAsync(SupervisorExisteYEsValido)
            .WithMessage("El supervisor especificado no existe o no es válido.")
            .MustAsync(NoCreaCicloJerarquico)
            .WithMessage("La asignación de supervisor crearía un ciclo jerárquico.")
            .When(v => v.SupervisorId.HasValue);

        RuleFor(v => v.Departamento)
            .Must(dept => _departamentosValidos.Contains(dept, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El departamento debe ser uno de: {string.Join(", ", _departamentosValidos)}.")
            .When(v => !string.IsNullOrWhiteSpace(v.Departamento));

        RuleFor(v => v.Posicion)
            .MaximumLength(100)
            .WithMessage("La posición no puede exceder 100 caracteres.")
            .When(v => !string.IsNullOrWhiteSpace(v.Posicion));

        RuleFor(v => v)
            .MustAsync(SupervisorEsDelMismoDepartamento)
            .WithMessage("El supervisor debe pertenecer al mismo departamento.")
            .When(v => v.SupervisorId.HasValue && !string.IsNullOrWhiteSpace(v.Departamento))
            .WithName("SupervisorMismoDepartamento");
    }

    private void ConfigurarValidacionesLaborales()
    {
        RuleFor(v => v.FechaIngreso)
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("La fecha de ingreso no puede ser futura.")
            .GreaterThan(new DateTime(1980, 1, 1))
            .WithMessage("La fecha de ingreso debe ser posterior a 1980.")
            .When(v => v.FechaIngreso.HasValue);

        RuleFor(v => v.SalarioBase)
            .GreaterThan(0)
            .WithMessage("El salario base debe ser mayor que cero.")
            .LessThan(50000000)
            .WithMessage("El salario base no puede exceder $50,000,000.")
            .When(v => v.SalarioBase.HasValue);

        RuleFor(v => v)
            .MustAsync(UsuarioAutorizaTienePermisosParaCambiarSalario)
            .WithMessage("El usuario autorizador no tiene permisos para cambiar salarios.")
            .When(v => v.SalarioBase.HasValue)
            .WithName("PermisosParaCambiarSalario");

        RuleFor(v => v.ObservacionesAdicionales)
            .MaximumLength(1000)
            .WithMessage("Las observaciones no pueden exceder 1000 caracteres.")
            .When(v => !string.IsNullOrWhiteSpace(v.ObservacionesAdicionales));
    }

    private void ConfigurarValidacionesSeguridad()
    {
        RuleFor(v => v)
            .MustAsync(UsuarioAutorizaTieneNivelSuficiente)
            .WithMessage("El usuario autorizador no tiene nivel suficiente para esta actualización.")
            .WithName("NivelSuficienteAutorizador");

        RuleFor(v => v)
            .MustAsync(CambiosNoExcedenLimitesUsuario)
            .WithMessage("Los cambios exceden los límites permitidos para el usuario.")
            .WithName("LimitesUsuario");

        RuleFor(v => v)
            .Must(command => !command.TieneCambiosCriticos() || command.RequiereAprobacion)
            .WithMessage("Los cambios críticos requieren aprobación adicional.")
            .WithName("CambiosCriticosRequierenAprobacion");

        RuleFor(v => v.FechaEfectivacambios)
            .GreaterThan(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Los cambios programados deben ser al menos 5 minutos en el futuro.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(365))
            .WithMessage("Los cambios programados no pueden ser más de 1 año en el futuro.")
            .When(v => v.FechaEfectivacambios.HasValue);
    }

    private void ConfigurarValidacionesNegocio()
    {
        RuleFor(v => v)
            .MustAsync(UsuarioNoTieneTransaccionesPendientes)
            .WithMessage("No se puede actualizar un usuario con transacciones pendientes.")
            .When(v => v.Activo == false)
            .WithName("TransaccionesPendientes");

        RuleFor(v => v)
            .MustAsync(ValidarImpactoEnFacturacionActiva)
            .WithMessage("La actualización afectaría la facturación activa del usuario.")
            .When(v => v.Activo == false || !string.IsNullOrWhiteSpace(v.Rol))
            .WithName("ImpactoFacturacionActiva");

        RuleFor(v => v)
            .MustAsync(ValidarLimitesActualizacionesDiarias)
            .WithMessage("Se ha excedido el límite de actualizaciones diarias para este usuario.")
            .WithName("LimitesActualizacionesDiarias");

        RuleFor(v => v.DocumentosAdjuntos)
            .Must(docs => docs.Count <= 5)
            .WithMessage("No se pueden adjuntar más de 5 documentos.")
            .When(v => v.DocumentosAdjuntos.Any());
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

    private async Task<bool> UsuarioNoEstaEliminado(Guid usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        return usuario?.FechaEliminacion == null;
    }

    private async Task<bool> UsuarioNoEsElMismo(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        // Si está cambiando datos críticos, no puede ser el mismo usuario
        if (command.TieneCambiosCriticos())
        {
            return command.UsuarioId != command.UsuarioAutorizaId;
        }

        return true;
    }

    private async Task<bool> EmailEsUnico(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email)) return true;

        return !await _context.Usuarios
            .AnyAsync(u => u.Email == command.Email && u.Id != command.UsuarioId, cancellationToken);
    }

    private async Task<bool> IdentificacionEsUnica(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Identificacion)) return true;

        return !await _context.Usuarios
            .AnyAsync(u => u.Identificacion == command.Identificacion && u.Id != command.UsuarioId, cancellationToken);
    }

    private async Task<bool> RolEsCompatibleConNivelAcceso(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Rol) || !command.NivelAcceso.HasValue) return true;

        var nivelRequerido = command.Rol.ToLower() switch
        {
            "empleado" => 1,
            "supervisor" => 4,
            "gerente" => 7,
            "administrador" => 9,
            "superadministrador" => 10,
            _ => 1
        };

        return command.NivelAcceso.Value >= nivelRequerido;
    }

    private async Task<bool> UsuarioAutorizaTienePeermisosParaCambiarRol(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        if (autorizador == null) return false;

        // Solo gerentes y administradores pueden cambiar roles
        return autorizador.Rol == "Gerente" || 
               autorizador.Rol == "Administrador" || 
               autorizador.Rol == "SuperAdministrador" ||
               autorizador.NivelAcceso >= 7;
    }

    private async Task<bool> SupervisorExisteYEsValido(Guid? supervisorId, CancellationToken cancellationToken)
    {
        if (!supervisorId.HasValue) return true;

        var supervisor = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == supervisorId.Value, cancellationToken);

        return supervisor?.Activo == true && 
               (supervisor.Rol == "Supervisor" || supervisor.Rol == "Gerente" || supervisor.Rol == "Administrador");
    }

    private async Task<bool> NoCreaCicloJerarquico(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        if (!command.SupervisorId.HasValue) return true;

        // Verificar que el usuario no se convierta en supervisor de su propio supervisor (directo o indirecto)
        var supervisorId = command.SupervisorId.Value;
        var currentUserId = command.UsuarioId;

        // Buscar la cadena jerárquica del supervisor propuesto
        var jerarquia = new HashSet<Guid>();
        var currentSupervisorId = supervisorId;

        while (currentSupervisorId != Guid.Empty && !jerarquia.Contains(currentSupervisorId))
        {
            jerarquia.Add(currentSupervisorId);

            if (currentSupervisorId == currentUserId)
            {
                return false; // Crearía un ciclo
            }

            var nextSupervisor = await _context.Usuarios
                .Where(u => u.Id == currentSupervisorId)
                .Select(u => u.SupervisorId)
                .FirstOrDefaultAsync(cancellationToken);

            currentSupervisorId = nextSupervisor ?? Guid.Empty;
        }

        return true;
    }

    private async Task<bool> SupervisorEsDelMismoDepartamento(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        if (!command.SupervisorId.HasValue || string.IsNullOrWhiteSpace(command.Departamento)) return true;

        var supervisor = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.SupervisorId.Value, cancellationToken);

        return supervisor?.Departamento == command.Departamento;
    }

    private async Task<bool> UsuarioAutorizaTienePermisosParaCambiarSalario(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        if (autorizador == null) return false;

        // Solo gerentes y administradores pueden cambiar salarios
        return autorizador.Rol == "Gerente" || 
               autorizador.Rol == "Administrador" || 
               autorizador.Rol == "SuperAdministrador" ||
               autorizador.NivelAcceso >= 8 ||
               autorizador.Permisos?.Contains("ModificarSalarios") == true;
    }

    private async Task<bool> UsuarioAutorizaTieneNivelSuficiente(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        var usuarioAActualizar = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioId, cancellationToken);

        if (autorizador == null || usuarioAActualizar == null) return false;

        // El autorizador debe tener nivel igual o superior al usuario a actualizar
        var nivelMinimoRequerido = Math.Max(usuarioAActualizar.NivelAcceso, command.NivelAcceso ?? 0);

        return autorizador.NivelAcceso >= nivelMinimoRequerido || 
               autorizador.Rol == "SuperAdministrador";
    }

    private async Task<bool> CambiosNoExcedenLimitesUsuario(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        var autorizador = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == command.UsuarioAutorizaId, cancellationToken);

        if (autorizador == null) return false;

        // Validar límites específicos según el rol del autorizador
        var limitesCampos = autorizador.Rol.ToLower() switch
        {
            "supervisor" => 3,
            "gerente" => 10,
            "administrador" => 20,
            "superadministrador" => int.MaxValue,
            _ => 2
        };

        return command.ObtenerCamposAModificar().Count <= limitesCampos;
    }

    private async Task<bool> UsuarioNoTieneTransaccionesPendientes(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        // Verificar que no tenga comandas activas
        var comandasActivas = await _context.Comandas
            .AnyAsync(c => c.UsuarioAsignadoId == command.UsuarioId && 
                          c.Estado != EstadoComanda.Completada && 
                          c.Estado != EstadoComanda.Cancelada, cancellationToken);

        if (comandasActivas) return false;

        // Verificar que no tenga facturas pendientes de cobro
        var facturasPendientes = await _context.Facturas
            .AnyAsync(f => f.UsuarioCreaId == command.UsuarioId && 
                          f.Estado == EstadoFactura.Pendiente, cancellationToken);

        return !facturasPendientes;
    }

    private async Task<bool> ValidarImpactoEnFacturacionActiva(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        // Verificar si el usuario tiene facturación activa en las últimas 2 horas
        var hace2Horas = DateTime.UtcNow.AddHours(-2);
        
        var facturacionReciente = await _context.Facturas
            .AnyAsync(f => f.UsuarioCreaId == command.UsuarioId && 
                          f.FechaEmision >= hace2Horas, cancellationToken);

        // Si no hay facturación reciente, puede proceder
        if (!facturacionReciente) return true;

        // Si hay facturación reciente y es un cambio crítico, requiere validación adicional
        if (command.TieneCambiosCriticos())
        {
            return command.RequiereAprobacion;
        }

        return true;
    }

    private async Task<bool> ValidarLimitesActualizacionesDiarias(ActualizarUsuarioCommand command, CancellationToken cancellationToken)
    {
        var hoy = DateTime.Today;
        
        var actualizacionesHoy = await _context.EventosAuditoria
            .Where(e => e.EntidadId == command.UsuarioId && 
                       e.TipoEvento == "UsuarioActualizado" &&
                       e.FechaEvento.Date == hoy)
            .CountAsync(cancellationToken);

        // Límite de 5 actualizaciones por día por usuario
        return actualizacionesHoy < 5;
    }
} 