namespace RestaurantePro.Application.Core.Usuarios.Commands.ActualizarUsuario;

/// <summary>
/// Comando para actualizar un usuario existente con validaciones empresariales completas
/// Incluye control de roles, jerarquías, permisos y auditoría completa
/// </summary>
public class ActualizarUsuarioCommand : IRequest<Result<UsuarioDto>>
{
    /// <summary>
    /// ID único del usuario a actualizar
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Nuevo nombre completo del usuario
    /// </summary>
    public string? Nombre { get; set; }

    /// <summary>
    /// Nuevo nombre de usuario (único en el sistema)
    /// </summary>
    public string? NombreUsuario { get; set; }

    /// <summary>
    /// Nuevo email del usuario (único en el sistema)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Nuevo número de teléfono
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Nueva identificación personal (cédula, DNI, etc.)
    /// </summary>
    public string? Identificacion { get; set; }

    /// <summary>
    /// Nueva dirección de residencia
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Nuevo rol del usuario: Empleado, Supervisor, Gerente, Administrador, SuperAdministrador
    /// </summary>
    public string? Rol { get; set; }

    /// <summary>
    /// Nuevo nivel de acceso numérico (1-10)
    /// </summary>
    public int? NivelAcceso { get; set; }

    /// <summary>
    /// Estado activo/inactivo del usuario
    /// </summary>
    public bool? Activo { get; set; }

    /// <summary>
    /// Lista de permisos específicos a asignar
    /// </summary>
    public List<string> PermisosEspecificos { get; set; } = new();

    /// <summary>
    /// Nuevo supervisor directo del usuario
    /// </summary>
    public Guid? SupervisorId { get; set; }

    /// <summary>
    /// Nuevo departamento al que pertenece
    /// </summary>
    public string? Departamento { get; set; }

    /// <summary>
    /// Nueva posición/cargo en el organigrama
    /// </summary>
    public string? Posicion { get; set; }

    /// <summary>
    /// Nueva fecha de ingreso a la empresa
    /// </summary>
    public DateTime? FechaIngreso { get; set; }

    /// <summary>
    /// Nuevo salario base (información sensible)
    /// </summary>
    public decimal? SalarioBase { get; set; }

    /// <summary>
    /// Nueva configuración de horario de trabajo
    /// </summary>
    public HorarioTrabajoDto? HorarioTrabajo { get; set; }

    /// <summary>
    /// Configuración de notificaciones del usuario
    /// </summary>
    public ConfiguracionNotificacionesDto? ConfiguracionNotificaciones { get; set; }

    /// <summary>
    /// Configuración de preferencias del usuario
    /// </summary>
    public Dictionary<string, object> Preferencias { get; set; } = new();

    /// <summary>
    /// ID del usuario que autoriza la actualización
    /// </summary>
    public Guid UsuarioAutorizaId { get; set; }

    /// <summary>
    /// Motivo de la actualización (requerido para auditoría)
    /// </summary>
    public string MotivoActualizacion { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones adicionales sobre los cambios
    /// </summary>
    public string? ObservacionesAdicionales { get; set; }

    /// <summary>
    /// Indica si enviar notificación al usuario sobre los cambios
    /// </summary>
    public bool NotificarUsuario { get; set; } = true;

    /// <summary>
    /// Indica si enviar notificación al supervisor sobre los cambios
    /// </summary>
    public bool NotificarSupervisor { get; set; } = true;

    /// <summary>
    /// Indica si los cambios requieren aprobación adicional
    /// </summary>
    public bool RequiereAprobacion { get; set; } = false;

    /// <summary>
    /// Indica si se debe crear backup antes de actualizar
    /// </summary>
    public bool CrearBackup { get; set; } = true;

    /// <summary>
    /// Prioridad de la actualización: Baja=1, Normal=2, Alta=3, Crítica=4
    /// </summary>
    public int Prioridad { get; set; } = 2;

    /// <summary>
    /// Documentos adjuntos relacionados con la actualización
    /// </summary>
    public List<string> DocumentosAdjuntos { get; set; } = new();

    /// <summary>
    /// Indica si invalidar sesiones activas del usuario tras la actualización
    /// </summary>
    public bool InvalidarSesionesActivas { get; set; } = false;

    /// <summary>
    /// Fecha efectiva de los cambios (para cambios programados)
    /// </summary>
    public DateTime? FechaEfectivacambios { get; set; }

    // Factory Methods para diferentes tipos de actualización

    /// <summary>
    /// Actualización básica de información personal
    /// </summary>
    public static ActualizarUsuarioCommand ActualizacionInformacionBasica(
        Guid usuarioId,
        string? nombre = null,
        string? telefono = null,
        string? direccion = null,
        Guid usuarioAutoriza = default)
    {
        return new ActualizarUsuarioCommand
        {
            UsuarioId = usuarioId,
            Nombre = nombre,
            Telefono = telefono,
            Direccion = direccion,
            UsuarioAutorizaId = usuarioAutoriza,
            MotivoActualizacion = "Actualización de información personal",
            Prioridad = 1,
            RequiereAprobacion = false,
            NotificarUsuario = true,
            NotificarSupervisor = false
        };
    }

    /// <summary>
    /// Actualización de rol y permisos (requiere validaciones especiales)
    /// </summary>
    public static ActualizarUsuarioCommand ActualizacionRolPermisos(
        Guid usuarioId,
        string nuevoRol,
        int nuevoNivelAcceso,
        List<string> nuevosPermisos,
        Guid usuarioAutoriza,
        string motivo)
    {
        return new ActualizarUsuarioCommand
        {
            UsuarioId = usuarioId,
            Rol = nuevoRol,
            NivelAcceso = nuevoNivelAcceso,
            PermisosEspecificos = nuevosPermisos,
            UsuarioAutorizaId = usuarioAutoriza,
            MotivoActualizacion = motivo,
            Prioridad = 4,
            RequiereAprobacion = true,
            NotificarUsuario = true,
            NotificarSupervisor = true,
            InvalidarSesionesActivas = true,
            CrearBackup = true
        };
    }

    /// <summary>
    /// Cambio de supervisor o departamento
    /// </summary>
    public static ActualizarUsuarioCommand CambioJerarquico(
        Guid usuarioId,
        Guid? nuevoSupervisorId,
        string? nuevoDepartamento,
        string? nuevaPosicion,
        Guid usuarioAutoriza,
        string motivo)
    {
        return new ActualizarUsuarioCommand
        {
            UsuarioId = usuarioId,
            SupervisorId = nuevoSupervisorId,
            Departamento = nuevoDepartamento,
            Posicion = nuevaPosicion,
            UsuarioAutorizaId = usuarioAutoriza,
            MotivoActualizacion = motivo,
            Prioridad = 3,
            RequiereAprobacion = true,
            NotificarUsuario = true,
            NotificarSupervisor = true
        };
    }

    /// <summary>
    /// Activación o desactivación de usuario
    /// </summary>
    public static ActualizarUsuarioCommand CambioEstadoActivacion(
        Guid usuarioId,
        bool nuevoEstado,
        Guid usuarioAutoriza,
        string motivo)
    {
        return new ActualizarUsuarioCommand
        {
            UsuarioId = usuarioId,
            Activo = nuevoEstado,
            UsuarioAutorizaId = usuarioAutoriza,
            MotivoActualizacion = motivo,
            Prioridad = nuevoEstado ? 2 : 4,
            RequiereAprobacion = !nuevoEstado, // Desactivar requiere aprobación
            NotificarUsuario = true,
            NotificarSupervisor = true,
            InvalidarSesionesActivas = !nuevoEstado,
            CrearBackup = true
        };
    }

    /// <summary>
    /// Actualización de información laboral y salarial
    /// </summary>
    public static ActualizarUsuarioCommand ActualizacionInformacionLaboral(
        Guid usuarioId,
        string? nuevaPosicion,
        decimal? nuevoSalario,
        DateTime? nuevaFechaIngreso,
        HorarioTrabajoDto? nuevoHorario,
        Guid usuarioAutoriza,
        string motivo)
    {
        return new ActualizarUsuarioCommand
        {
            UsuarioId = usuarioId,
            Posicion = nuevaPosicion,
            SalarioBase = nuevoSalario,
            FechaIngreso = nuevaFechaIngreso,
            HorarioTrabajo = nuevoHorario,
            UsuarioAutorizaId = usuarioAutoriza,
            MotivoActualizacion = motivo,
            Prioridad = 3,
            RequiereAprobacion = nuevoSalario.HasValue, // Cambios salariales requieren aprobación
            NotificarUsuario = true,
            NotificarSupervisor = true,
            CrearBackup = true
        };
    }

    /// <summary>
    /// Actualización de configuraciones y preferencias
    /// </summary>
    public static ActualizarUsuarioCommand ActualizacionConfiguraciones(
        Guid usuarioId,
        ConfiguracionNotificacionesDto? nuevasNotificaciones,
        Dictionary<string, object>? nuevasPreferencias,
        Guid usuarioAutoriza)
    {
        return new ActualizarUsuarioCommand
        {
            UsuarioId = usuarioId,
            ConfiguracionNotificaciones = nuevasNotificaciones,
            Preferencias = nuevasPreferencias ?? new(),
            UsuarioAutorizaId = usuarioAutoriza,
            MotivoActualizacion = "Actualización de configuraciones y preferencias",
            Prioridad = 1,
            RequiereAprobacion = false,
            NotificarUsuario = false,
            NotificarSupervisor = false
        };
    }

    /// <summary>
    /// Actualización programada (para ejecutar en fecha futura)
    /// </summary>
    public static ActualizarUsuarioCommand ActualizacionProgramada(
        Guid usuarioId,
        DateTime fechaEfectiva,
        ActualizarUsuarioCommand cambiosAProgramar,
        Guid usuarioAutoriza,
        string motivo)
    {
        cambiosAProgramar.UsuarioId = usuarioId;
        cambiosAProgramar.FechaEfectivacambios = fechaEfectiva;
        cambiosAProgramar.UsuarioAutorizaId = usuarioAutoriza;
        cambiosAProgramar.MotivoActualizacion = motivo;
        cambiosAProgramar.Prioridad = 2;
        cambiosAProgramar.RequiereAprobacion = true;
        
        return cambiosAProgramar;
    }

    /// <summary>
    /// Validación básica del comando
    /// </summary>
    public bool EsValido()
    {
        return UsuarioId != Guid.Empty &&
               UsuarioAutorizaId != Guid.Empty &&
               !string.IsNullOrWhiteSpace(MotivoActualizacion) &&
               Prioridad >= 1 && Prioridad <= 4 &&
               TieneAlMenosUnCambio();
    }

    /// <summary>
    /// Verifica si el comando tiene al menos un cambio para aplicar
    /// </summary>
    public bool TieneAlMenosUnCambio()
    {
        return !string.IsNullOrWhiteSpace(Nombre) ||
               !string.IsNullOrWhiteSpace(NombreUsuario) ||
               !string.IsNullOrWhiteSpace(Email) ||
               !string.IsNullOrWhiteSpace(Telefono) ||
               !string.IsNullOrWhiteSpace(Identificacion) ||
               !string.IsNullOrWhiteSpace(Direccion) ||
               !string.IsNullOrWhiteSpace(Rol) ||
               NivelAcceso.HasValue ||
               Activo.HasValue ||
               PermisosEspecificos.Any() ||
               SupervisorId.HasValue ||
               !string.IsNullOrWhiteSpace(Departamento) ||
               !string.IsNullOrWhiteSpace(Posicion) ||
               FechaIngreso.HasValue ||
               SalarioBase.HasValue ||
               HorarioTrabajo != null ||
               ConfiguracionNotificaciones != null ||
               Preferencias.Any();
    }

    /// <summary>
    /// Obtiene lista de campos que van a ser modificados
    /// </summary>
    public List<string> ObtenerCamposAModificar()
    {
        var campos = new List<string>();

        if (!string.IsNullOrWhiteSpace(Nombre)) campos.Add("Nombre");
        if (!string.IsNullOrWhiteSpace(NombreUsuario)) campos.Add("NombreUsuario");
        if (!string.IsNullOrWhiteSpace(Email)) campos.Add("Email");
        if (!string.IsNullOrWhiteSpace(Telefono)) campos.Add("Telefono");
        if (!string.IsNullOrWhiteSpace(Identificacion)) campos.Add("Identificacion");
        if (!string.IsNullOrWhiteSpace(Direccion)) campos.Add("Direccion");
        if (!string.IsNullOrWhiteSpace(Rol)) campos.Add("Rol");
        if (NivelAcceso.HasValue) campos.Add("NivelAcceso");
        if (Activo.HasValue) campos.Add("Activo");
        if (PermisosEspecificos.Any()) campos.Add("PermisosEspecificos");
        if (SupervisorId.HasValue) campos.Add("SupervisorId");
        if (!string.IsNullOrWhiteSpace(Departamento)) campos.Add("Departamento");
        if (!string.IsNullOrWhiteSpace(Posicion)) campos.Add("Posicion");
        if (FechaIngreso.HasValue) campos.Add("FechaIngreso");
        if (SalarioBase.HasValue) campos.Add("SalarioBase");
        if (HorarioTrabajo != null) campos.Add("HorarioTrabajo");
        if (ConfiguracionNotificaciones != null) campos.Add("ConfiguracionNotificaciones");
        if (Preferencias.Any()) campos.Add("Preferencias");

        return campos;
    }

    /// <summary>
    /// Determina si los cambios son considerados críticos
    /// </summary>
    public bool TieneCambiosCriticos()
    {
        return !string.IsNullOrWhiteSpace(Rol) ||
               NivelAcceso.HasValue ||
               Activo == false ||
               SalarioBase.HasValue ||
               PermisosEspecificos.Any();
    }

    /// <summary>
    /// Obtiene resumen del comando para logging
    /// </summary>
    public string ObtenerResumen()
    {
        var campos = string.Join(", ", ObtenerCamposAModificar());
        return $"ActualizarUsuario: {UsuarioId} - Campos: [{campos}] - Motivo: {MotivoActualizacion} - Crítico: {TieneCambiosCriticos()}";
    }
} 