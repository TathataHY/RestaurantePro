using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados;

/// <summary>
/// Handler para obtener clientes paginados con filtros avanzados
/// Optimizado para dashboards comerciales y búsquedas complejas
/// </summary>
public class ObtenerClientesPaginadosHandler : IRequestHandler<ObtenerClientesPaginadosQuery, Result<PaginatedList<ClienteSummaryDto>>>
{
    private readonly IClienteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerClientesPaginadosHandler> _logger;

    // Campos válidos para ordenamiento
    private readonly Dictionary<string, string> _camposOrdenValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        { "FechaCreacion", "FechaCreacion" },
        { "Nombre", "Nombre.NombreCompleto" },
        { "Email", "Email.Value" },
        { "PuntosAcumulados", "PuntosAcumulados" },
        { "CantidadVisitas", "CantidadVisitas" },
        { "Segmento", "Segmento" }
    };

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
        _logger.LogInformation("📊 Obteniendo clientes paginados: Página {PageNumber}, Tamaño {PageSize}, Filtros: {Filtros}",
            request.PageNumber, request.PageSize, LogearFiltros(request));

        try
        {
            // 1. Obtener todos los clientes y aplicar filtros en memoria
            // TODO: Optimizar esto cuando IClienteRepository tenga métodos especializados
            var todosLosClientes = await _repository.ObtenerTodosAsync(cancellationToken);

            // 2. Aplicar filtros
            var clientesFiltrados = AplicarFiltros(todosLosClientes, request);

            // 3. Aplicar ordenamiento
            var clientesOrdenados = AplicarOrdenamiento(clientesFiltrados, request);

            // 4. Aplicar paginación
            var totalCount = clientesOrdenados.Count();
            var clientesPaginados = clientesOrdenados
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // 5. Mapear a DTOs
            var clientesDto = _mapper.Map<List<ClienteSummaryDto>>(clientesPaginados);

            // 6. Enriquecer DTOs con información calculada
            foreach (var clienteDto in clientesDto)
            {
                EnriquecerClienteDto(clienteDto);
            }

            // 7. Crear resultado paginado
            var resultado = new PaginatedList<ClienteSummaryDto>(
                clientesDto,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("✅ Clientes obtenidos: {TotalCount} total, {PageCount} páginas",
                resultado.TotalCount, resultado.TotalPages);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener clientes paginados");
            return Result.Failure<PaginatedList<ClienteSummaryDto>>("Error interno al obtener la lista de clientes");
        }
    }

    /// <summary>
    /// Aplica todos los filtros de la query
    /// </summary>
    private IEnumerable<Cliente> AplicarFiltros(IEnumerable<Cliente> clientes, ObtenerClientesPaginadosQuery request)
    {
        var query = clientes.AsQueryable();

        // Filtro por estado activo
        if (request.SoloActivos.HasValue)
        {
            query = query.Where(c => c.EstaActivo == request.SoloActivos.Value);
        }

        // Filtro de texto libre (nombre, email, teléfono)
        if (!string.IsNullOrWhiteSpace(request.FiltroTexto))
        {
            var filtroLower = request.FiltroTexto.ToLower();
            query = query.Where(c => 
                c.Nombre.NombreCompleto.ToLower().Contains(filtroLower) ||
                c.Email.Value.ToLower().Contains(filtroLower) ||
                c.Telefono.Value.Contains(filtroLower));
        }

        // Filtro por segmento
        if (!string.IsNullOrWhiteSpace(request.Segmento))
        {
            if (Enum.TryParse<SegmentoCliente>(request.Segmento, true, out var segmento))
            {
                query = query.Where(c => c.Segmento == segmento);
            }
        }

        // Filtro por tarjeta de fidelización
        if (request.SoloConTarjetaFidelizacion.HasValue)
        {
            if (request.SoloConTarjetaFidelizacion.Value)
                query = query.Where(c => c.TarjetaFidelizacionPrincipalId.HasValue);
            else
                query = query.Where(c => !c.TarjetaFidelizacionPrincipalId.HasValue);
        }

        // Filtro por clientes frecuentes (más de 10 visitas)
        if (request.SoloClientesFrecuentes)
        {
            query = query.Where(c => c.CantidadVisitas > 10);
        }

        // Filtros por fecha de registro
        if (request.FechaRegistroDesde.HasValue)
        {
            query = query.Where(c => c.FechaCreacion >= request.FechaRegistroDesde.Value);
        }

        if (request.FechaRegistroHasta.HasValue)
        {
            var fechaHasta = request.FechaRegistroHasta.Value.AddDays(1); // Incluir todo el día
            query = query.Where(c => c.FechaCreacion < fechaHasta);
        }

        return query;
    }

    /// <summary>
    /// Aplica ordenamiento según los parámetros de la query
    /// </summary>
    private IEnumerable<Cliente> AplicarOrdenamiento(IEnumerable<Cliente> clientes, ObtenerClientesPaginadosQuery request)
    {
        var query = clientes.AsQueryable();
        var esDescendente = request.DireccionOrden?.ToLower() == "desc";

        return (request.OrdenarPor?.ToLower()) switch
        {
            "nombre" => esDescendente 
                ? query.OrderByDescending(c => c.Nombre.NombreCompleto)
                : query.OrderBy(c => c.Nombre.NombreCompleto),
            "email" => esDescendente 
                ? query.OrderByDescending(c => c.Email.Value)
                : query.OrderBy(c => c.Email.Value),
            "puntosacumulados" => esDescendente 
                ? query.OrderByDescending(c => c.PuntosAcumulados)
                : query.OrderBy(c => c.PuntosAcumulados),
            "cantidadvisitas" => esDescendente 
                ? query.OrderByDescending(c => c.CantidadVisitas)
                : query.OrderBy(c => c.CantidadVisitas),
            "segmento" => esDescendente 
                ? query.OrderByDescending(c => c.Segmento)
                : query.OrderBy(c => c.Segmento),
            _ => esDescendente 
                ? query.OrderByDescending(c => c.FechaCreacion)
                : query.OrderBy(c => c.FechaCreacion)
        };
    }

    /// <summary>
    /// Enriquece el DTO con información calculada y campos display
    /// </summary>
    private void EnriquecerClienteDto(ClienteSummaryDto clienteDto)
    {
        // Calcular días desde última actividad
        var diasSinActividad = (DateTime.Now - clienteDto.FechaCreacion).Days;
        
        // Determinar segmento color para UI
        clienteDto.SegmentoColor = clienteDto.Segmento switch
        {
            "VIP" => "#FFD700",
            "Premium" => "#4CAF50", 
            "Regular" => "#2196F3",
            "Nuevo" => "#FF9800",
            _ => "#9E9E9E"
        };

        // Determinar nivel de actividad
        clienteDto.NivelActividad = diasSinActividad switch
        {
            <= 7 => "Muy Activo",
            <= 30 => "Activo", 
            <= 90 => "Moderado",
            _ => "Inactivo"
        };

        // Formatear información adicional para UI
        clienteDto.ResumenActividad = $"{clienteDto.CantidadVisitas} visitas, {clienteDto.PuntosAcumulados} puntos";
    }

    /// <summary>
    /// Crea un resumen de filtros para logging
    /// </summary>
    private string LogearFiltros(ObtenerClientesPaginadosQuery request)
    {
        var filtros = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.FiltroTexto))
            filtros.Add($"Texto: '{request.FiltroTexto}'");

        if (request.SoloActivos.HasValue)
            filtros.Add($"Activos: {request.SoloActivos.Value}");

        if (!string.IsNullOrWhiteSpace(request.Segmento))
            filtros.Add($"Segmento: {request.Segmento}");

        if (request.SoloConTarjetaFidelizacion.HasValue)
            filtros.Add($"ConTarjeta: {request.SoloConTarjetaFidelizacion.Value}");

        if (request.SoloClientesFrecuentes)
            filtros.Add("SoloFrecuentes: true");

        if (request.FechaRegistroDesde.HasValue)
            filtros.Add($"Desde: {request.FechaRegistroDesde:yyyy-MM-dd}");

        if (request.FechaRegistroHasta.HasValue)
            filtros.Add($"Hasta: {request.FechaRegistroHasta:yyyy-MM-dd}");

        return filtros.Any() ? string.Join(", ", filtros) : "Sin filtros";
    }
} 