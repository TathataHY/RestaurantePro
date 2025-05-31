namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandasActivas;

/// <summary>
/// Handler para obtener comandas activas con filtros y paginación
/// Optimizado para dashboards operativos del restaurante
/// </summary>
public class ObtenerComandasActivasHandler : IRequestHandler<ObtenerComandasActivasQuery, Result<PaginatedList<ComandaSummaryDto>>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerComandasActivasHandler> _logger;

    public ObtenerComandasActivasHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<ObtenerComandasActivasHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<ComandaSummaryDto>>> Handle(ObtenerComandasActivasQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("📋 Obteniendo comandas activas - Página: {PageNumber}, Filtros aplicados: {HasFilters}", 
            request.PageNumber, HasActiveFilters(request));

        try
        {
            // 1. Construir criterios de filtro
            var criterios = ConstruirCriteriosFiltro(request);

            // 2. Obtener comandas con filtros aplicados
            var (comandasBase, totalCount) = await ObtenerComandasConFiltros(request, criterios);

            // 3. Aplicar filtros adicionales en memoria si es necesario
            var comandasFiltradas = AplicarFiltrosAdicionales(comandasBase, request);

            // 4. Mapear a DTOs resumidos
            var comandasDto = _mapper.Map<List<ComandaSummaryDto>>(comandasFiltradas);

            // 5. Aplicar cálculos específicos para el dashboard
            EnriquecerDatosParaDashboard(comandasDto);

            // 6. Crear resultado paginado
            var resultado = new PaginatedList<ComandaSummaryDto>(
                comandasDto,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("✅ Comandas activas obtenidas: {Count} de {Total}", 
                comandasDto.Count, totalCount);

            return Result.Success(resultado);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("📝 Error en parámetros de consulta: {Error}", ex.Message);
            return Result.Failure<PaginatedList<ComandaSummaryDto>>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al obtener comandas activas");
            return Result.Failure<PaginatedList<ComandaSummaryDto>>("Ocurrió un error interno al consultar las comandas");
        }
    }

    /// <summary>
    /// Construye los criterios de filtro para el repositorio
    /// </summary>
    private static Dictionary<string, object> ConstruirCriteriosFiltro(ObtenerComandasActivasQuery request)
    {
        var criterios = new Dictionary<string, object>();

        // Filtros básicos
        if (!string.IsNullOrEmpty(request.EstadoFiltro))
            criterios["Estado"] = request.EstadoFiltro;

        if (request.MesaId.HasValue)
            criterios["MesaId"] = request.MesaId.Value;

        if (request.MeseroId.HasValue)
            criterios["MeseroId"] = request.MeseroId.Value;

        if (request.ClienteId.HasValue)
            criterios["ClienteId"] = request.ClienteId.Value;

        // Filtros de fecha
        if (request.SoloHoy)
        {
            criterios["FechaInicio"] = DateTime.Today;
            criterios["FechaFin"] = DateTime.Today.AddDays(1).AddTicks(-1);
        }
        else if (request.FechaEspecifica.HasValue)
        {
            var fecha = request.FechaEspecifica.Value.Date;
            criterios["FechaInicio"] = fecha;
            criterios["FechaFin"] = fecha.AddDays(1).AddTicks(-1);
        }

        // Estados activos por defecto (excluye finalizadas y canceladas)
        if (string.IsNullOrEmpty(request.EstadoFiltro))
        {
            criterios["EstadosExcluidos"] = new[] { "Finalizada", "Cancelada" };
        }

        return criterios;
    }

    /// <summary>
    /// Aplica filtros adicionales que requieren lógica en memoria
    /// </summary>
    private List<Comanda> AplicarFiltrosAdicionales(List<Comanda> comandas, ObtenerComandasActivasQuery request)
    {
        var resultado = comandas.AsEnumerable();

        // Filtrar solo atrasadas
        if (request.SoloAtrasadas)
        {
            resultado = resultado.Where(c => EstaAtrasada(c));
        }

        // Filtrar solo con descuentos
        if (request.SoloConDescuentos)
        {
            resultado = resultado.Where(c => c.DescuentoFidelizacion.HasValue && c.DescuentoFidelizacion > 0);
        }

        return resultado.ToList();
    }

    /// <summary>
    /// Enriquece los DTOs con datos calculados para el dashboard
    /// </summary>
    private void EnriquecerDatosParaDashboard(List<ComandaSummaryDto> comandas)
    {
        // TODO: Implementar cuando estén disponibles todas las propiedades en ComandaSummaryDto
        /*
        foreach (var comanda in comandas)
        {
            // Calcular tiempo transcurrido
            var tiempoTranscurrido = DateTime.Now - comanda.FechaCreacion;
            comanda.TiempoTranscurridoTexto = FormatearTiempoTranscurrido(tiempoTranscurrido);

            // Determinar si está atrasada
            comanda.EstaAtrasada = DeterminarSiEstaAtrasada(comanda, tiempoTranscurrido);

            // Asignar color según estado y tiempo
            comanda.ColorEstado = AsignarColorEstado(comanda);

            // Formatear estado para display
            comanda.EstadoDisplay = FormatearEstadoDisplay(comanda.Estado);
        }
        */

        _logger.LogDebug("🎨 Datos de dashboard enriquecidos para {Count} comandas", comandas.Count);
    }

    /// <summary>
    /// Verifica si hay filtros activos aplicados
    /// </summary>
    private static bool HasActiveFilters(ObtenerComandasActivasQuery request)
    {
        return !string.IsNullOrEmpty(request.EstadoFiltro) ||
               request.MesaId.HasValue ||
               request.MeseroId.HasValue ||
               request.ClienteId.HasValue ||
               request.SoloAtrasadas ||
               request.SoloConDescuentos ||
               request.FechaEspecifica.HasValue;
    }

    /// <summary>
    /// Determina si una comanda está atrasada según reglas de negocio
    /// </summary>
    private static bool EstaAtrasada(Comanda comanda)
    {
        var tiempoTranscurrido = DateTime.Now - comanda.FechaCreacion;
        
        // Reglas de tiempo según estado
        return comanda.Estado.ToString() switch
        {
            "Creada" => tiempoTranscurrido.TotalMinutes > 5,
            "EnProceso" => tiempoTranscurrido.TotalMinutes > 30,
            "Lista" => tiempoTranscurrido.TotalMinutes > 45,
            _ => false
        };
    }

    /// <summary>
    /// Determina si una comanda está atrasada para el DTO
    /// </summary>
    private static bool DeterminarSiEstaAtrasada(ComandaSummaryDto comanda, TimeSpan tiempoTranscurrido)
    {
        // TODO: Implementar cuando esté disponible la propiedad Estado en ComandaSummaryDto
        /*
        return comanda.Estado switch
        {
            "Creada" => tiempoTranscurrido.TotalMinutes > 5,
            "EnProceso" => tiempoTranscurrido.TotalMinutes > 30,
            "Lista" => tiempoTranscurrido.TotalMinutes > 45,
            _ => false
        };
        */
        return false;
    }

    /// <summary>
    /// Formatea el tiempo transcurrido en texto legible
    /// </summary>
    private static string FormatearTiempoTranscurrido(TimeSpan tiempo)
    {
        if (tiempo.TotalMinutes < 1)
            return "< 1 min";
        
        if (tiempo.TotalHours < 1)
            return $"{(int)tiempo.TotalMinutes} min";
        
        return $"{(int)tiempo.TotalHours}h {tiempo.Minutes}m";
    }

    /// <summary>
    /// Asigna color según estado y condición de atraso
    /// </summary>
    private static string AsignarColorEstado(ComandaSummaryDto comanda)
    {
        // TODO: Implementar cuando estén disponibles las propiedades necesarias
        /*
        if (comanda.EstaAtrasada)
            return "#FF4444"; // Rojo para atrasadas

        return comanda.Estado switch
        {
            "Creada" => "#FFA500",    // Naranja
            "EnProceso" => "#FFD700", // Amarillo
            "Lista" => "#32CD32",     // Verde
            "Entregada" => "#87CEEB", // Azul claro
            "Finalizada" => "#90EE90", // Verde claro
            _ => "#808080"            // Gris por defecto
        };
        */
        return "#808080"; // Gris por defecto
    }

    /// <summary>
    /// Formatea el estado para mostrar en la UI
    /// </summary>
    private static string FormatearEstadoDisplay(string estado)
    {
        return estado switch
        {
            "EnProceso" => "En Proceso",
            "Lista" => "Lista para Entregar",
            "Entregada" => "Entregada",
            "Finalizada" => "Finalizada",
            _ => estado
        };
    }

    private async Task<(List<Comanda>, int)> ObtenerComandasConFiltros(ObtenerComandasActivasQuery request, Dictionary<string, object> criterios)
    {
        // Usar ObtenerComandasAbiertas que ya existe en el repositorio para comandas activas
        var comandasAbiertas = await _comandaRepository.ObtenerComandasAbiertas(true);
        var query = comandasAbiertas.AsQueryable();

        // Aplicar filtros básicos
        if (criterios.ContainsKey("MesaId"))
        {
            var mesaId = (Guid)criterios["MesaId"];
            query = query.Where(c => c.MesaId == mesaId);
        }

        if (criterios.ContainsKey("MeseroId"))
        {
            var meseroId = (Guid)criterios["MeseroId"];
            query = query.Where(c => c.MeseroId == meseroId);
        }

        if (criterios.ContainsKey("ClienteId"))
        {
            var clienteId = (Guid)criterios["ClienteId"];
            query = query.Where(c => c.ClienteId == clienteId);
        }

        if (criterios.ContainsKey("Estado"))
        {
            var estado = criterios["Estado"].ToString();
            query = query.Where(c => c.Estado.ToString() == estado);
        }

        // Aplicar filtros de fecha
        if (criterios.ContainsKey("FechaInicio"))
        {
            var fechaInicio = (DateTime)criterios["FechaInicio"];
            query = query.Where(c => c.FechaCreacion >= fechaInicio);
        }

        if (criterios.ContainsKey("FechaFin"))
        {
            var fechaFin = (DateTime)criterios["FechaFin"];
            query = query.Where(c => c.FechaCreacion <= fechaFin);
        }

        // Aplicar ordenamiento
        query = request.OrdenarPor switch
        {
            "TiempoTranscurrido" => request.DireccionOrden == "Desc" 
                ? query.OrderBy(c => c.FechaCreacion)  // Más antiguas primero = más tiempo transcurrido
                : query.OrderByDescending(c => c.FechaCreacion),
            "Total" => request.DireccionOrden == "Desc"
                ? query.OrderByDescending(c => c.Total!.Total)
                : query.OrderBy(c => c.Total!.Total),
            "Estado" => request.DireccionOrden == "Desc"
                ? query.OrderByDescending(c => c.Estado)
                : query.OrderBy(c => c.Estado),
            _ => request.DireccionOrden == "Desc"
                ? query.OrderByDescending(c => c.FechaCreacion)
                : query.OrderBy(c => c.FechaCreacion)
        };

        var totalCount = query.Count();
        
        // Aplicar paginación
        var comandasPaginadas = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return (comandasPaginadas, totalCount);
    }
} 