namespace RestaurantePro.Application.Core.Usuarios.DTOs;

/// <summary>
/// DTO para representar un horario de trabajo
/// </summary>
public class HorarioTrabajoDto
{
    /// <summary>
    /// ID del horario de trabajo
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Día de la semana (1=Lunes, 7=Domingo)
    /// </summary>
    public int DiaSemana { get; set; }

    /// <summary>
    /// Nombre del día de la semana
    /// </summary>
    public string NombreDia { get; set; } = string.Empty;

    /// <summary>
    /// Hora de inicio del turno
    /// </summary>
    public TimeSpan HoraInicio { get; set; }

    /// <summary>
    /// Hora de fin del turno
    /// </summary>
    public TimeSpan HoraFin { get; set; }

    /// <summary>
    /// Indica si es un día laborable
    /// </summary>
    public bool EsDiaLaborable { get; set; }

    /// <summary>
    /// Horas totales de trabajo en el día
    /// </summary>
    public decimal HorasTrabajo { get; set; }

    /// <summary>
    /// Hora de inicio del descanso/almuerzo
    /// </summary>
    public TimeSpan? HoraInicioDescanso { get; set; }

    /// <summary>
    /// Hora de fin del descanso/almuerzo
    /// </summary>
    public TimeSpan? HoraFinDescanso { get; set; }

    /// <summary>
    /// Duración del descanso en minutos
    /// </summary>
    public int MinutosDescanso { get; set; }

    /// <summary>
    /// Tipo de turno (Mañana, Tarde, Noche, Completo)
    /// </summary>
    public string TipoTurno { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones especiales del horario
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Indica si es horario activo/vigente
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Fecha de inicio de vigencia del horario
    /// </summary>
    public DateTime FechaInicioVigencia { get; set; }

    /// <summary>
    /// Fecha de fin de vigencia del horario
    /// </summary>
    public DateTime? FechaFinVigencia { get; set; }

    /// <summary>
    /// Zona horaria del horario
    /// </summary>
    public string ZonaHoraria { get; set; } = "America/Santiago";

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public HorarioTrabajoDto()
    {
        Id = Guid.NewGuid();
        EsDiaLaborable = true;
        Activo = true;
        FechaInicioVigencia = DateTime.UtcNow;
        TipoTurno = "Completo";
    }

    /// <summary>
    /// Factory method para crear horario de día completo
    /// </summary>
    public static HorarioTrabajoDto CrearDiaCompleto(int diaSemana, TimeSpan horaInicio, TimeSpan horaFin)
    {
        return new HorarioTrabajoDto
        {
            DiaSemana = diaSemana,
            NombreDia = ObtenerNombreDia(diaSemana),
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            HorasTrabajo = (decimal)(horaFin - horaInicio).TotalHours,
            TipoTurno = "Completo"
        };
    }

    /// <summary>
    /// Factory method para crear horario con descanso
    /// </summary>
    public static HorarioTrabajoDto CrearConDescanso(
        int diaSemana, 
        TimeSpan horaInicio, 
        TimeSpan horaFin,
        TimeSpan horaInicioDescanso,
        TimeSpan horaFinDescanso)
    {
        var horario = CrearDiaCompleto(diaSemana, horaInicio, horaFin);
        horario.HoraInicioDescanso = horaInicioDescanso;
        horario.HoraFinDescanso = horaFinDescanso;
        horario.MinutosDescanso = (int)(horaFinDescanso - horaInicioDescanso).TotalMinutes;
        horario.HorasTrabajo = (decimal)(horaFin - horaInicio).TotalHours - ((decimal)(horaFinDescanso - horaInicioDescanso).TotalHours);
        
        return horario;
    }

    /// <summary>
    /// Factory method para día no laborable
    /// </summary>
    public static HorarioTrabajoDto CrearDiaNoLaborable(int diaSemana)
    {
        return new HorarioTrabajoDto
        {
            DiaSemana = diaSemana,
            NombreDia = ObtenerNombreDia(diaSemana),
            EsDiaLaborable = false,
            HorasTrabajo = 0,
            TipoTurno = "NoLaborable"
        };
    }

    /// <summary>
    /// Obtiene el nombre del día según el número
    /// </summary>
    public static string ObtenerNombreDia(int diaSemana)
    {
        return diaSemana switch
        {
            1 => "Lunes",
            2 => "Martes",
            3 => "Miércoles",
            4 => "Jueves",
            5 => "Viernes",
            6 => "Sábado",
            7 => "Domingo",
            _ => "Desconocido"
        };
    }

    /// <summary>
    /// Valida si el horario es válido
    /// </summary>
    public bool EsValido()
    {
        if (!EsDiaLaborable) return true;

        if (HoraInicio >= HoraFin) return false;

        if (HoraInicioDescanso.HasValue && HoraFinDescanso.HasValue)
        {
            if (HoraInicioDescanso >= HoraFinDescanso) return false;
            if (HoraInicioDescanso < HoraInicio || HoraFinDescanso > HoraFin) return false;
        }

        return HorasTrabajo > 0 && HorasTrabajo <= 16; // Máximo 16 horas de trabajo
    }

    /// <summary>
    /// Calcula las horas efectivas de trabajo (sin descansos)
    /// </summary>
    public decimal CalcularHorasEfectivas()
    {
        if (!EsDiaLaborable) return 0;

        var horasBase = (decimal)(HoraFin - HoraInicio).TotalHours;
        var horasDescanso = HoraInicioDescanso.HasValue && HoraFinDescanso.HasValue
            ? (decimal)(HoraFinDescanso.Value - HoraInicioDescanso.Value).TotalHours
            : 0;

        return Math.Max(0, horasBase - horasDescanso);
    }
} 