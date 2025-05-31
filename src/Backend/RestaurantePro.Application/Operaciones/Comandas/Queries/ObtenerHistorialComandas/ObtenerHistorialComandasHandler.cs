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
        _logger.LogInformation("📋 Obteniendo historial de comandas - Página: {PageNumber}, Tamaño: {PageSize}", 
            request.PageNumber, request.PageSize);

        try
        {
            // 1. Construir filtros base
            var filtros = ConstruirFiltros(request);
            _logger.LogDebug("🔍 Filtros aplicados: {@Filtros}", filtros);

            // 2. Obtener comandas filtradas
            var comandas = await _comandaRepository.ObtenerComandasFiltradas(
                filtros,
                request.PageNumber,
                request.PageSize,
                ConstruirOrden(request.OrdenarPor));

            if (!comandas.Any())
            {
                _logger.LogInformation("📄 No se encontraron comandas con los filtros especificados");
                return Result<PaginatedList<ComandaSummaryDto>>.Success(
                    new PaginatedList<ComandaSummaryDto>(
                        new List<ComandaSummaryDto>(),
                        0,
                        request.PageNumber,
                        request.PageSize));
            }

            // 3. Obtener total de registros
            var totalRegistros = await _comandaRepository.ContarComandasFiltradas(filtros);

            // 4. Mapear a DTOs
            var comandasDto = _mapper.Map<List<ComandaSummaryDto>>(comandas);

            // 5. Enriquecer DTOs con información adicional
            await EnriquecerComandas(comandasDto);

            // 6. Crear resultado paginado
            var resultado = new PaginatedList<ComandaSummaryDto>(
                comandasDto,
                totalRegistros,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("✅ Historial obtenido exitosamente. Total: {Total}, Página: {Pagina}/{TotalPaginas}", 
                totalRegistros, request.PageNumber, resultado.TotalPages);

            // 7. Log de estadísticas
            LogearEstadisticas(comandas, request);

            return Result<PaginatedList<ComandaSummaryDto>>.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error al obtener historial de comandas");
            return Result<PaginatedList<ComandaSummaryDto>>.Failure(
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
    /// Construye el criterio de ordenamiento
    /// </summary>
    private static string ConstruirOrden(OrdenHistorial orden)
    {
        return orden switch
        {
            OrdenHistorial.FechaMasReciente => "FechaCreacion DESC",
            OrdenHistorial.FechaMasAntigua => "FechaCreacion ASC",
            OrdenHistorial.MontoMayor => "Total DESC",
            OrdenHistorial.MontoMenor => "Total ASC",
            OrdenHistorial.MesaNombre => "Mesa.Nombre ASC",
            OrdenHistorial.MeseroNombre => "Mesero.NombreCompleto ASC",
            OrdenHistorial.ClienteNombre => "Cliente.NombreCompleto ASC",
            OrdenHistorial.EstadoComanda => "Estado ASC, FechaCreacion DESC",
            _ => "FechaCreacion DESC"
        };
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
                comanda.DuracionServicio = duracion;
                comanda.ServicioRapido = duracion.TotalMinutes <= 30;
            }

            // Marcar como comando VIP (clientes frecuentes o montos altos)
            comanda.EsVip = comanda.Total >= 100 || comanda.ClienteEsFrecuente;

            // Calcular eficiencia (items por minuto)
            if (comanda.DuracionServicio.HasValue && comanda.DuracionServicio.Value.TotalMinutes > 0)
            {
                comanda.EficienciaServicio = comanda.TotalItems / (decimal)comanda.DuracionServicio.Value.TotalMinutes;
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

        var stats = new
        {
            TotalComandas = comandas.Count(),
            MontoTotal = comandas.Sum(c => c.CalcularTotal()),
            MontoPromedio = comandas.Average(c => c.CalcularTotal()),
            ComandaMaxima = comandas.Max(c => c.CalcularTotal()),
            ComandaMinima = comandas.Min(c => c.CalcularTotal()),
            MesasMasUsadas = comandas.Where(c => c.MesaId.HasValue)
                .GroupBy(c => c.MesaId)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => new { MesaId = g.Key, Cantidad = g.Count() }),
            Estados = comandas.GroupBy(c => c.Estado)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
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