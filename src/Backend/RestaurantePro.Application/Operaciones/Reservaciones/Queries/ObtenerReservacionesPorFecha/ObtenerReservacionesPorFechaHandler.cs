using EstadoReservacionDomain = RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesPorFecha;

/// <summary>
/// Handler para ObtenerReservacionesPorFechaQuery
/// </summary>
public class ObtenerReservacionesPorFechaHandler : IRequestHandler<ObtenerReservacionesPorFechaQuery, Result<PaginatedList<ReservacionDto>>>
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerReservacionesPorFechaHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public ObtenerReservacionesPorFechaHandler(
        IReservacionRepository reservacionRepository,
        IMapper mapper,
        ILogger<ObtenerReservacionesPorFechaHandler> logger,
        ICurrentUserService currentUser)
    {
        _reservacionRepository = reservacionRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<ReservacionDto>>> Handle(ObtenerReservacionesPorFechaQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📅 Obteniendo reservaciones para fecha {Fecha} - Página: {Pagina}, Estado: {Estado}, Mesa: {MesaId}", 
                request.Fecha.ToShortDateString(), 
                request.Pagina,
                request.Estado?.ToString() ?? "Todas",
                request.MesaId?.ToString() ?? "Todas");

            // Obtener reservaciones por fecha
            var reservaciones = await _reservacionRepository.ObtenerPorFechaAsync(request.Fecha, cancellationToken);

            if (reservaciones == null || !reservaciones.Any())
            {
                _logger.LogInformation("📋 No se encontraron reservaciones para la fecha {Fecha}", request.Fecha.ToShortDateString());
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

            // Filtrar por mesa si se especifica
            if (request.MesaId.HasValue)
            {
                reservacionesFiltradas = reservacionesFiltradas.Where(r => r.MesaId == request.MesaId.Value);
                _logger.LogDebug("🔍 Filtro aplicado - Mesa: {MesaId}", request.MesaId.Value);
            }

            // Filtrar solo activas si se especifica
            if (request.SoloActivas)
            {
                reservacionesFiltradas = reservacionesFiltradas.Where(r => 
                    r.Estado == EstadoReservacionDomain.Pendiente ||
                    r.Estado == EstadoReservacionDomain.Confirmada);
                _logger.LogDebug("🔍 Filtro aplicado - Solo reservaciones activas");
            }

            // Filtrar solo futuras si se especifica
            if (request.SoloFuturas)
            {
                var fechaHoraActual = DateTime.Now;
                reservacionesFiltradas = reservacionesFiltradas.Where(r => r.FechaReservacion > fechaHoraActual);
                _logger.LogDebug("🔍 Filtro aplicado - Solo reservaciones futuras");
            }

            if (!reservacionesFiltradas.Any())
            {
                _logger.LogInformation("⚠️ No hay reservaciones que cumplan los criterios especificados");
                return Result.Success(new PaginatedList<ReservacionDto>(new List<ReservacionDto>(), 0, request.Pagina, request.TamanoPagina));
            }

            // Ordenar si se especifica
            if (request.OrdenarPorHora)
            {
                reservacionesFiltradas = reservacionesFiltradas.OrderBy(r => r.Hora);
                _logger.LogDebug("📊 Ordenamiento aplicado por hora de reservación");
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

            _logger.LogInformation("✅ Se encontraron {TotalReservaciones} reservaciones para {Fecha}. Página {Pagina} de {TotalPaginas} ({ReservacionesEnPagina} reservaciones en esta página)", 
                resultado.TotalCount, 
                request.Fecha.ToShortDateString(),
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

                foreach (var estadistica in estadisticas)
                {
                    _logger.LogDebug("📊 {Estado}: {Cantidad} reservaciones", estadistica.Estado, estadistica.Cantidad);
                }
            }

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener reservaciones por fecha {Fecha}", request.Fecha);
            return Result.Failure<PaginatedList<ReservacionDto>>("Error interno del servidor al obtener las reservaciones");
        }
    }
} 