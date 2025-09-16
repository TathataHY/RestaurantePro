namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerHistorialComandas;

/// <summary>
/// 📋 Handler para obtener historial de comandas con filtros avanzados
/// </summary>
public class ObtenerHistorialComandasHandler : IRequestHandler<ObtenerHistorialComandasQuery, Result<PaginatedList<ComandaSummaryDto>>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerHistorialComandasHandler> _logger;

    public ObtenerHistorialComandasHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<ObtenerHistorialComandasHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<ComandaSummaryDto>>> Handle(
        ObtenerHistorialComandasQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo historial de comandas - Página: {PageNumber}, Tamaño: {PageSize}", 
                request.PageNumber, request.PageSize);

            // 1. Construir filtros
            var filtros = ConstruirFiltros(request);

            // 2. Obtener comandas usando métodos reales que SÍ existen
            IEnumerable<Comanda> comandas;
            int totalRegistros;

            if (request.FechaDesde.HasValue && request.FechaHasta.HasValue)
            {
                // Usar método real ObtenerPorRangoFechasAsync que SÍ existe
                comandas = await _comandaRepository.ObtenerPorRangoFechasAsync(
                    request.FechaDesde.Value, 
                    request.FechaHasta.Value, 
                    true, 
                    cancellationToken);
                totalRegistros = comandas.Count();
            }
            else
            {
                // Usar método real ObtenerPaginadoAsync que SÍ existe
                var resultadoPaginado = await _comandaRepository.ObtenerPaginadoAsync(
                    request.PageNumber - 1, // Repository usa base 0
                    request.PageSize, 
                    true, 
                    cancellationToken);
                comandas = resultadoPaginado.Comandas;
                totalRegistros = resultadoPaginado.Total;
            }

            // 3. Aplicar filtros adicionales en memoria (filtros que no están en repository)
            comandas = AplicarFiltrosEnMemoria(comandas, request);

            // 4. Aplicar ordenamiento antes de paginación
            comandas = AplicarOrdenamiento(comandas, request.OrdenarPor);

            // 5. Si usamos filtros, recalcular paginación
            if (request.FechaDesde.HasValue && request.FechaHasta.HasValue)
            {
                totalRegistros = comandas.Count();
                comandas = comandas
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize);
            }

            var comandasList = comandas.ToList();

            if (!comandasList.Any())
            {
                _logger.LogInformation("📄 No se encontraron comandas con los filtros especificados");
                return Result.Success(new PaginatedList<ComandaSummaryDto>(
                    new List<ComandaSummaryDto>(),
                    0,
                    request.PageNumber,
                    request.PageSize));
            }

            // 5. Mapear a DTOs
            var comandasDto = _mapper.Map<List<ComandaSummaryDto>>(comandasList);

            // 6. Enriquecer DTOs con información adicional
            await EnriquecerComandas(comandasDto);

            // 7. Crear resultado paginado
            var resultadoFinal = new PaginatedList<ComandaSummaryDto>(
                comandasDto,
                totalRegistros,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("✅ Historial obtenido exitosamente. Total: {Total}, Página: {Pagina}/{TotalPaginas}", 
                totalRegistros, request.PageNumber, resultadoFinal.TotalPages);

            // 8. Log de estadísticas
            LogearEstadisticas(comandasList, request);

            return Result.Success(resultadoFinal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al obtener historial de comandas");
            return Result.Failure<PaginatedList<ComandaSummaryDto>>(
                $"Error interno al obtener historial: {ex.Message}");
        }
    }

    /// <summary>
    /// Construye los filtros para la consulta
    /// </summary>
    private static ComandaFiltros ConstruirFiltros(ObtenerHistorialComandasQuery request)
    {
        return new ComandaFiltros
        {
            FechaDesde = request.FechaDesde,
            FechaHasta = request.FechaHasta,
            MesaId = request.MesaId,
            MeseroId = request.MeseroId,
            ClienteId = request.ClienteId,
            Estado = request.Estado,
            MontoMinimo = request.MontoMinimo,
            MontoMaximo = request.MontoMaximo,
            TerminoBusqueda = request.TerminoBusqueda,
            IncluirCanceladas = request.IncluirCanceladas,
            SoloFinalizadas = request.SoloFinalizadas
        };
    }

    /// <summary>
    /// Aplica ordenamiento a las comandas según el criterio especificado
    /// </summary>
    private static IEnumerable<Comanda> AplicarOrdenamiento(IEnumerable<Comanda> comandas, OrdenHistorial ordenarPor)
    {
        return ordenarPor switch
        {
            OrdenHistorial.FechaMasReciente => comandas.OrderByDescending(c => c.FechaCreacion),
            OrdenHistorial.FechaMasAntigua => comandas.OrderBy(c => c.FechaCreacion),
            OrdenHistorial.MontoMayor => comandas.OrderByDescending(c => c.Total != null ? c.Total.Total : 0),
            OrdenHistorial.MontoMenor => comandas.OrderBy(c => c.Total != null ? c.Total.Total : 0),
            OrdenHistorial.MesaNombre => comandas.OrderBy(c => c.MesaId), // Ordenar por ID de mesa por simplicidad
            OrdenHistorial.MeseroNombre => comandas.OrderBy(c => c.MeseroId), // Ordenar por ID de mesero por simplicidad
            OrdenHistorial.ClienteNombre => comandas.OrderBy(c => c.ClienteId), // Ordenar por ID de cliente por simplicidad
            OrdenHistorial.EstadoComanda => comandas.OrderBy(c => c.Estado.ToString()),
            _ => comandas.OrderByDescending(c => c.FechaCreacion) // Default: más reciente primero
        };
    }

    /// <summary>
    /// Aplica filtros adicionales en memoria usando propiedades reales de Comanda
    /// </summary>
    private static IEnumerable<Comanda> AplicarFiltrosEnMemoria(IEnumerable<Comanda> comandas, ObtenerHistorialComandasQuery request)
    {
        // Aplicar filtros usando propiedades reales de la entidad Comanda
        if (request.MesaId.HasValue)
            comandas = comandas.Where(c => c.MesaId == request.MesaId.Value);
            
        if (request.ClienteId.HasValue)
            comandas = comandas.Where(c => c.ClienteId == request.ClienteId.Value);
            
        if (request.MeseroId.HasValue)
            comandas = comandas.Where(c => c.MeseroId == request.MeseroId.Value);
            
        if (request.Estado.HasValue)
            comandas = comandas.Where(c => c.Estado == request.Estado.Value);
            
        if (request.MontoMinimo.HasValue)
            comandas = comandas.Where(c => c.Total != null && c.Total.Total >= request.MontoMinimo.Value);
            
        if (request.MontoMaximo.HasValue)
            comandas = comandas.Where(c => c.Total != null && c.Total.Total <= request.MontoMaximo.Value);
            
        if (!string.IsNullOrWhiteSpace(request.TerminoBusqueda))
            comandas = comandas.Where(c => c.Observaciones.Contains(request.TerminoBusqueda, StringComparison.OrdinalIgnoreCase));
            
        if (!request.IncluirCanceladas)
            comandas = comandas.Where(c => c.Estado != EstadoComanda.Cancelada);
            
        if (request.SoloFinalizadas)
            comandas = comandas.Where(c => c.Estado == EstadoComanda.Finalizada);

        return comandas;
    }

    /// <summary>
    /// Enriquece las comandas con información adicional
    /// </summary>
    private async Task EnriquecerComandas(List<ComandaSummaryDto> comandas)
    {
        foreach (var comanda in comandas)
        {
            // Calcular tiempo de servicio si está finalizada
            if (comanda.Estado == "Finalizada" && comanda.FechaFinalizacion.HasValue)
            {
                var duracion = comanda.FechaFinalizacion.Value - comanda.FechaCreacion;
                comanda.DuracionServicio = (int)duracion.TotalMinutes;
                comanda.ServicioRapido = duracion.TotalMinutes <= 30;
            }

            // Marcar como comando VIP (clientes frecuentes o montos altos)
            comanda.EsVip = comanda.Total >= 100 || comanda.ClienteEsFrecuente;

            // Calcular eficiencia (items por minuto)
            if (comanda.DuracionServicio.HasValue && comanda.DuracionServicio.Value > 0)
            {
                comanda.EficienciaServicio = comanda.TotalItems / (decimal)comanda.DuracionServicio.Value;
            }
        }

        await Task.CompletedTask; // Para consistencia async
    }

    /// <summary>
    /// Registra estadísticas del historial obtenido
    /// </summary>
    private void LogearEstadisticas(IEnumerable<Comanda> comandas, ObtenerHistorialComandasQuery request)
    {
        if (!comandas.Any()) return;

        var comandasList = comandas.ToList();
        var comandasConTotal = comandasList.Where(c => c.Total != null).ToList();

        var stats = new
        {
            TotalComandas = comandasList.Count,
            MontoTotal = comandasConTotal.Sum(c => c.Total!.Total),
            MontoPromedio = comandasConTotal.Any() ? comandasConTotal.Average(c => c.Total!.Total) : 0,
            ComandaMaxima = comandasConTotal.Any() ? comandasConTotal.Max(c => c.Total!.Total) : 0,
            ComandaMinima = comandasConTotal.Any() ? comandasConTotal.Min(c => c.Total!.Total) : 0,
            MesasMasUsadas = comandasList.Where(c => c.MesaId != Guid.Empty)
                .GroupBy(c => c.MesaId)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => new { MesaId = g.Key, Cantidad = g.Count() }),
            Estados = comandasList.GroupBy(c => c.Estado)
                .Select(g => new { Estado = g.Key.ToString(), Cantidad = g.Count() })
        };

        _logger.LogInformation("📊 Estadísticas del historial: {@Stats}", stats);

        // Log específico de filtros aplicados
        var filtrosAplicados = new List<string>();
        if (request.FechaDesde.HasValue) filtrosAplicados.Add($"Desde: {request.FechaDesde:yyyy-MM-dd}");
        if (request.FechaHasta.HasValue) filtrosAplicados.Add($"Hasta: {request.FechaHasta:yyyy-MM-dd}");
        if (request.MesaId.HasValue) filtrosAplicados.Add($"Mesa: {request.MesaId}");
        if (request.MeseroId.HasValue) filtrosAplicados.Add($"Mesero: {request.MeseroId}");
        if (request.ClienteId.HasValue) filtrosAplicados.Add($"Cliente: {request.ClienteId}");
        if (!string.IsNullOrWhiteSpace(request.TerminoBusqueda)) filtrosAplicados.Add($"Búsqueda: '{request.TerminoBusqueda}'");

        if (filtrosAplicados.Any())
        {
            _logger.LogDebug("🔍 Filtros aplicados: {Filtros}", string.Join(", ", filtrosAplicados));
        }
    }
}

/// <summary>
/// 🔍 Clase para encapsular filtros de búsqueda
/// </summary>
public class ComandaFiltros
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public Guid? MesaId { get; set; }
    public Guid? MeseroId { get; set; }
    public Guid? ClienteId { get; set; }
    public EstadoComanda? Estado { get; set; }
    public decimal? MontoMinimo { get; set; }
    public decimal? MontoMaximo { get; set; }
    public string? TerminoBusqueda { get; set; }
    public bool IncluirCanceladas { get; set; }
    public bool SoloFinalizadas { get; set; }
} 