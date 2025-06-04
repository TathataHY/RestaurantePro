namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesCliente;

/// <summary>
/// Handler para ObtenerReservacionesClienteQuery
/// </summary>
public class ObtenerReservacionesClienteHandler : IRequestHandler<ObtenerReservacionesClienteQuery, Result<PaginatedList<ReservacionDto>>>
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerReservacionesClienteHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public ObtenerReservacionesClienteHandler(
        IReservacionRepository reservacionRepository,
        IMapper mapper,
        ILogger<ObtenerReservacionesClienteHandler> logger,
        ICurrentUserService currentUser)
    {
        _reservacionRepository = reservacionRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<ReservacionDto>>> Handle(ObtenerReservacionesClienteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("👤 Obteniendo reservaciones del cliente {ClienteId} - Página: {Pagina}, Estado: {Estado}, Tipo: {Tipo}", 
                request.ClienteId, 
                request.Pagina,
                request.Estado?.ToString() ?? "Todas",
                GetTipoConsulta(request));

            // Obtener reservaciones por cliente
            var reservaciones = await _reservacionRepository.ObtenerPorClienteAsync(request.ClienteId, cancellationToken);

            if (reservaciones == null || !reservaciones.Any())
            {
                _logger.LogInformation("📋 No se encontraron reservaciones para el cliente {ClienteId}", request.ClienteId);
                return Result.Success(new PaginatedList<ReservacionDto>(new List<ReservacionDto>(), 0, request.Pagina, request.TamanoPagina));
            }

            // Aplicar filtros
            var reservacionesFiltradas = reservaciones.AsEnumerable();

            // Filtrar por estado si se especifica
            if (request.Estado.HasValue)
            {
                reservacionesFiltradas = reservacionesFiltradas.Where(r => r.Estado == request.Estado.Value);
                _logger.LogDebug("🔍 Filtro aplicado - Estado: {Estado}", request.Estado.Value);
            }

            // Filtrar por rango de fechas si se especifica
            if (request.FechaDesde.HasValue && request.FechaHasta.HasValue)
            {
                reservacionesFiltradas = reservacionesFiltradas.Where(r => 
                    r.Fecha.Date >= request.FechaDesde.Value.Date && 
                    r.Fecha.Date <= request.FechaHasta.Value.Date);
                _logger.LogDebug("🔍 Filtro aplicado - Rango de fechas: {FechaDesde} - {FechaHasta}", 
                    request.FechaDesde.Value.ToShortDateString(), 
                    request.FechaHasta.Value.ToShortDateString());
            }
            else if (request.FechaDesde.HasValue)
            {
                reservacionesFiltradas = reservacionesFiltradas.Where(r => r.Fecha.Date >= request.FechaDesde.Value.Date);
                _logger.LogDebug("🔍 Filtro aplicado - Desde fecha: {FechaDesde}", request.FechaDesde.Value.ToShortDateString());
            }
            else if (request.FechaHasta.HasValue)
            {
                reservacionesFiltradas = reservacionesFiltradas.Where(r => r.Fecha.Date <= request.FechaHasta.Value.Date);
                _logger.LogDebug("🔍 Filtro aplicado - Hasta fecha: {FechaHasta}", request.FechaHasta.Value.ToShortDateString());
            }

            // Filtrar solo futuras si se especifica
            if (request.SoloFuturas)
            {
                var fechaHoraActual = DateTime.Now;
                reservacionesFiltradas = reservacionesFiltradas.Where(r => r.FechaReservacion > fechaHoraActual);
                _logger.LogDebug("🔍 Filtro aplicado - Solo reservaciones futuras");
            }

            // Filtrar solo activas si se especifica
            if (request.SoloActivas)
            {
                reservacionesFiltradas = reservacionesFiltradas.Where(r => 
                    r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Pendiente ||
                    r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Confirmada);
                _logger.LogDebug("🔍 Filtro aplicado - Solo reservaciones activas");
            }

            // Filtrar solo historial si se especifica
            if (request.SoloHistorial)
            {
                var fechaActual = DateTime.Today;
                reservacionesFiltradas = reservacionesFiltradas.Where(r => 
                    r.Fecha.Date < fechaActual ||
                    r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Completada ||
                    r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Cancelada ||
                    r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.NoShow);
                _logger.LogDebug("🔍 Filtro aplicado - Solo historial (reservaciones pasadas o finalizadas)");
            }

            if (!reservacionesFiltradas.Any())
            {
                _logger.LogInformation("⚠️ No hay reservaciones del cliente {ClienteId} que cumplan los criterios especificados", request.ClienteId);
                return Result.Success(new PaginatedList<ReservacionDto>(new List<ReservacionDto>(), 0, request.Pagina, request.TamanoPagina));
            }

            // Ordenar si se especifica
            if (request.OrdenarPorFecha)
            {
                // Ordenar por fecha descendente (más recientes primero)
                reservacionesFiltradas = reservacionesFiltradas.OrderByDescending(r => r.FechaReservacion);
                _logger.LogDebug("📊 Ordenamiento aplicado por fecha (más recientes primero)");
            }

            // Convertir a lista para aplicar paginación
            var listaReservaciones = reservacionesFiltradas.ToList();

            // Aplicar paginación
            var reservacionesPaginadas = listaReservaciones
                .Skip((request.Pagina - 1) * request.TamanoPagina)
                .Take(request.TamanoPagina)
                .ToList();

            // Mapear a DTOs
            var reservacionesDto = _mapper.Map<List<ReservacionDto>>(reservacionesPaginadas);

            // Crear resultado paginado
            var resultado = new PaginatedList<ReservacionDto>(
                reservacionesDto,
                listaReservaciones.Count,
                request.Pagina,
                request.TamanoPagina);

            _logger.LogInformation("✅ Se encontraron {TotalReservaciones} reservaciones para el cliente {ClienteId}. Página {Pagina} de {TotalPaginas} ({ReservacionesEnPagina} reservaciones en esta página)", 
                resultado.TotalCount, 
                request.ClienteId,
                resultado.PageNumber, 
                resultado.TotalPages, 
                resultado.Items.Count);

            // Log estadísticas adicionales por estado
            if (request.Estado == null && listaReservaciones.Any())
            {
                var estadisticas = listaReservaciones
                    .GroupBy(r => r.Estado)
                    .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                    .OrderBy(e => e.Estado);

                _logger.LogDebug("📊 Estadísticas del cliente {ClienteId}:", request.ClienteId);
                foreach (var estadistica in estadisticas)
                {
                    _logger.LogDebug("   📊 {Estado}: {Cantidad} reservaciones", estadistica.Estado, estadistica.Cantidad);
                }

                // Estadísticas de fidelidad del cliente
                var reservacionesCompletadas = listaReservaciones.Count(r => r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Completada);
                var reservacionesCanceladas = listaReservaciones.Count(r => 
                    r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Cancelada || 
                    r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.NoShow);

                if (listaReservaciones.Count > 0)
                {
                    var tasaCompletadas = Math.Round((decimal)reservacionesCompletadas / listaReservaciones.Count * 100, 1);
                    var tasaCanceladas = Math.Round((decimal)reservacionesCanceladas / listaReservaciones.Count * 100, 1);
                    
                    _logger.LogDebug("📈 Perfil del cliente - Tasa de completadas: {TasaCompletadas}%, Tasa de canceladas: {TasaCanceladas}%", 
                        tasaCompletadas, tasaCanceladas);
                }
            }

            return Result.Success(resultado);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🚫 Operación cancelada al obtener reservaciones del cliente {ClienteId}", request.ClienteId);
            throw; // Re-throw para que se propague correctamente
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener reservaciones del cliente {ClienteId}", request.ClienteId);
            return Result.Failure<PaginatedList<ReservacionDto>>("Error interno del servidor al obtener las reservaciones del cliente");
        }
    }

    private static string GetTipoConsulta(ObtenerReservacionesClienteQuery request)
    {
        if (request.SoloFuturas) return "Futuras";
        if (request.SoloActivas) return "Activas";
        if (request.SoloHistorial) return "Historial";
        if (request.Estado.HasValue) return $"Estado-{request.Estado.Value}";
        if (request.FechaDesde.HasValue || request.FechaHasta.HasValue) return "Rango de fechas";
        return "Todas";
    }
} 