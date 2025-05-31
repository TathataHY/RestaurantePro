namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ConsultarDisponibilidad;

/// <summary>
/// Query para consultar disponibilidad de mesas para reservaciones
/// Permite verificar disponibilidad por fecha, hora, número de personas y zona
/// </summary>
public class ConsultarDisponibilidadQuery : IRequest<Result<DisponibilidadDto>>
{
    /// <summary>
    /// Fecha y hora deseada para la reservación
    /// </summary>
    public DateTime FechaHora { get; set; }

    /// <summary>
    /// Número de comensales/personas
    /// </summary>
    public int NumeroPersonas { get; set; }

    /// <summary>
    /// Duración estimada de la reservación en minutos
    /// </summary>
    public int DuracionEstimadaMinutos { get; set; } = 120; // 2 horas por defecto

    /// <summary>
    /// Zona específica preferida (opcional)
    /// </summary>
    public string? ZonaPreferida { get; set; }

    /// <summary>
    /// Mesa específica preferida (opcional)
    /// </summary>
    public Guid? MesaPreferida { get; set; }

    /// <summary>
    /// Mostrar alternativas si no hay disponibilidad exacta
    /// </summary>
    public bool MostrarAlternativas { get; set; } = true;

    /// <summary>
    /// Rango de tiempo en minutos para buscar alternativas
    /// </summary>
    public int RangoAlternativasMinutos { get; set; } = 60;

    /// <summary>
    /// Incluir mesas con capacidad mayor a la solicitada
    /// </summary>
    public bool PermitirCapacidadMayor { get; set; } = true;

    /// <summary>
    /// Margen de tolerancia en el número de personas
    /// </summary>
    public int MargenToleranciaPersonas { get; set; } = 2;

    /// <summary>
    /// Incluir información detallada de cada mesa
    /// </summary>
    public bool IncluirDetallesMesas { get; set; } = true;

    /// <summary>
    /// Verificar disponibilidad para eventos especiales
    /// </summary>
    public bool EsEventoEspecial { get; set; } = false;

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public ConsultarDisponibilidadQuery()
    {
    }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public ConsultarDisponibilidadQuery(DateTime fechaHora, int numeroPersonas)
    {
        FechaHora = fechaHora;
        NumeroPersonas = numeroPersonas;
    }

    /// <summary>
    /// Factory method para consulta básica
    /// </summary>
    public static ConsultarDisponibilidadQuery Create(DateTime fechaHora, int numeroPersonas)
    {
        return new ConsultarDisponibilidadQuery(fechaHora, numeroPersonas);
    }

    /// <summary>
    /// Factory method para consulta con zona específica
    /// </summary>
    public static ConsultarDisponibilidadQuery CreateConZona(DateTime fechaHora, int numeroPersonas, string zona)
    {
        return new ConsultarDisponibilidadQuery(fechaHora, numeroPersonas)
        {
            ZonaPreferida = zona,
            IncluirDetallesMesas = true
        };
    }

    /// <summary>
    /// Factory method para consulta de mesa específica
    /// </summary>
    public static ConsultarDisponibilidadQuery CreateMesaEspecifica(DateTime fechaHora, int numeroPersonas, Guid mesaId)
    {
        return new ConsultarDisponibilidadQuery(fechaHora, numeroPersonas)
        {
            MesaPreferida = mesaId,
            MostrarAlternativas = true,
            IncluirDetallesMesas = true
        };
    }

    /// <summary>
    /// Factory method para evento especial
    /// </summary>
    public static ConsultarDisponibilidadQuery CreateEventoEspecial(DateTime fechaHora, int numeroPersonas, int duracionMinutos)
    {
        return new ConsultarDisponibilidadQuery(fechaHora, numeroPersonas)
        {
            DuracionEstimadaMinutos = duracionMinutos,
            EsEventoEspecial = true,
            MostrarAlternativas = true,
            RangoAlternativasMinutos = 120,
            PermitirCapacidadMayor = true,
            IncluirDetallesMesas = true
        };
    }

    /// <summary>
    /// Factory method para búsqueda flexible
    /// </summary>
    public static ConsultarDisponibilidadQuery CreateFlexible(DateTime fechaHora, int numeroPersonas, int rangoMinutos = 90)
    {
        return new ConsultarDisponibilidadQuery(fechaHora, numeroPersonas)
        {
            MostrarAlternativas = true,
            RangoAlternativasMinutos = rangoMinutos,
            PermitirCapacidadMayor = true,
            MargenToleranciaPersonas = 3,
            IncluirDetallesMesas = true
        };
    }
} 