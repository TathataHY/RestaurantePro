using RestaurantePro.Application.Comercial.Clientes.DTOs;

namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados;

/// <summary>
/// Handler para obtener clientes paginados
/// Demuestra el uso de las extensiones QueryableExtensions
/// </summary>
public class ObtenerClientesPaginadosHandler : IRequestHandler<ObtenerClientesPaginadosQuery, Result<PaginatedList<ClienteSummaryDto>>>
{
    private readonly IClienteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerClientesPaginadosHandler> _logger;

    public ObtenerClientesPaginadosHandler(
        IClienteRepository repository,
        IMapper mapper,
        ILogger<ObtenerClientesPaginadosHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<ClienteSummaryDto>>> Handle(
        ObtenerClientesPaginadosQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Obteniendo clientes paginados - Página: {PageNumber}, Tamaño: {PageSize}", 
            request.Filtros.PageNumber, 
            request.Filtros.PageSize);

        try
        {
            // 1. Obtener datos paginados directamente del repositorio
            var (clientes, total) = await _repository.ObtenerPaginadoAsync(
                request.Filtros.PageNumber - 1, // El repositorio usa base 0
                request.Filtros.PageSize,
                cancellationToken);

            // 2. Aplicar filtros específicos si es necesario (esto se podría mover al repositorio)
            var clientesFiltrados = clientes.AsQueryable();
            clientesFiltrados = AplicarFiltrosEspecificos(clientesFiltrados, request);

            // 3. Mapear a DTOs
            var clientesDto = new PaginatedList<ClienteSummaryDto>(
                _mapper.Map<List<ClienteSummaryDto>>(clientesFiltrados.ToList()),
                total,
                request.Filtros.PageNumber,
                request.Filtros.PageSize);

            _logger.LogInformation("✅ Clientes obtenidos exitosamente - Total: {TotalCount}, Página: {PageNumber}/{TotalPages}", 
                clientesDto.TotalCount,
                clientesDto.PageNumber,
                clientesDto.TotalPages);

            return Result.Success(clientesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener clientes paginados");
            return Result.Failure<PaginatedList<ClienteSummaryDto>>("Error interno del servidor al obtener clientes");
        }
    }

    /// <summary>
    /// Aplica filtros específicos de cliente antes de los filtros base
    /// </summary>
    private IQueryable<Cliente> AplicarFiltrosEspecificos(IQueryable<Cliente> query, ObtenerClientesPaginadosQuery request)
    {
        // Filtro de activos (override del base si se especifica)
        if (request.SoloActivos.HasValue)
        {
            query = query.Where(c => c.EstaActivo == request.SoloActivos.Value);
        }

        // Filtro por segmento
        if (!string.IsNullOrWhiteSpace(request.Segmento))
        {
            query = query.Where(c => c.Segmento.ToString() == request.Segmento);
        }

        // Filtro por puntos mínimos
        if (request.PuntosMinimos.HasValue)
        {
            query = query.Where(c => c.PuntosAcumulados >= request.PuntosMinimos.Value);
        }

        // Filtro por visitas mínimas
        if (request.VisitasMinimas.HasValue)
        {
            query = query.Where(c => c.CantidadVisitas >= request.VisitasMinimas.Value);
        }

        // Filtro por tarjeta de fidelización
        if (request.ConTarjetaFidelizacion.HasValue)
        {
            if (request.ConTarjetaFidelizacion.Value)
                query = query.Where(c => c.TarjetaFidelizacionPrincipalId.HasValue);
            else
                query = query.Where(c => !c.TarjetaFidelizacionPrincipalId.HasValue);
        }

        // Filtros por edad (requiere cálculo)
        var fechaActual = DateTime.Now;
        if (request.EdadMinima.HasValue)
        {
            var fechaMaximaNacimiento = fechaActual.AddYears(-request.EdadMinima.Value);
            query = query.Where(c => c.FechaNacimiento <= fechaMaximaNacimiento);
        }

        if (request.EdadMaxima.HasValue)
        {
            var fechaMinimaNacimiento = fechaActual.AddYears(-request.EdadMaxima.Value - 1);
            query = query.Where(c => c.FechaNacimiento > fechaMinimaNacimiento);
        }

        return query;
    }
} 