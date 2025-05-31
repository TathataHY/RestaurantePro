using System.Linq.Expressions;

namespace RestaurantePro.Application.Comercial.Clientes.Queries.BuscarClientesPorEmail;

public class BuscarClientesPorEmailHandler : IRequestHandler<BuscarClientesPorEmailQuery, Result<PaginatedList<ClienteSummaryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<BuscarClientesPorEmailHandler> _logger;

    private readonly Dictionary<string, Expression<Func<Cliente, object>>> _camposOrdenamiento = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Email", c => c.Email },
        { "Nombre", c => c.Nombre },
        { "FechaCreacion", c => c.FechaCreacion },
        { "PuntosAcumulados", c => c.PuntosAcumulados },
        { "CantidadVisitas", c => c.CantidadVisitas },
        { "Segmento", c => c.Segmento }
    };

    public BuscarClientesPorEmailHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<BuscarClientesPorEmailHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<ClienteSummaryDto>>> Handle(BuscarClientesPorEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando búsqueda de clientes por email: '{Email}'", request.Email);

            // 1. Query base de clientes
            var query = _context.Clientes.AsQueryable();

            // 2. Aplicar filtros de búsqueda
            query = AplicarFiltrosBusqueda(query, request);

            // 3. Contar total antes de paginación
            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                _logger.LogInformation("No se encontraron clientes con los criterios especificados");
                return Result.Success(new PaginatedList<ClienteSummaryDto>(
                    new List<ClienteSummaryDto>(), 0, request.Pagina, request.TamanoPagina));
            }

            // 4. Aplicar ordenamiento
            query = AplicarOrdenamiento(query, request);

            // 5. Aplicar paginación
            var clientes = await query
                .Skip((request.Pagina - 1) * request.TamanoPagina)
                .Take(request.TamanoPagina)
                .Select(c => new ClienteSummaryDto
                {
                    Id = c.Id,
                    Email = c.Email,
                    Telefono = c.Telefono ?? "",
                    // TODO: Agregar más propiedades cuando estén disponibles en ClienteSummaryDto
                    // Nombre = c.Nombre.ToString(),
                    // Activo = c.Estado == EstadoCliente.Activo,
                })
                .ToListAsync(cancellationToken);

            // 6. Crear resultado paginado
            var resultado = new PaginatedList<ClienteSummaryDto>(
                clientes, totalCount, request.Pagina, request.TamanoPagina);

            _logger.LogInformation("Búsqueda completada: {TotalEncontrados} clientes encontrados, {PaginaActual}/{TotalPaginas} páginas",
                totalCount, request.Pagina, resultado.TotalPages);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar clientes por email: '{Email}'", request.Email);
            return Result.Failure<PaginatedList<ClienteSummaryDto>>("Error interno al buscar clientes.");
        }
    }

    private IQueryable<Cliente> AplicarFiltrosBusqueda(IQueryable<Cliente> query, BuscarClientesPorEmailQuery request)
    {
        // Búsqueda por email
        if (!string.IsNullOrEmpty(request.Email))
        {
            if (request.BusquedaExacta)
            {
                // Búsqueda exacta
                query = query.Where(c => c.Email == request.Email);
            }
            else
            {
                // Búsqueda parcial (LIKE)
                query = query.Where(c => c.Email != null && 
                                       c.Email.Value.ToLower().Contains(request.Email.ToLower()));
            }
        }

        // Búsqueda por dominio
        if (!string.IsNullOrEmpty(request.Dominio))
        {
            var dominioLower = request.Dominio.ToLower();
            query = query.Where(c => c.Email != null && c.Email.Value.ToLower().Contains($"@{dominioLower}"));
        }

        return query;
    }

    private IQueryable<Cliente> AplicarOrdenamiento(IQueryable<Cliente> query, BuscarClientesPorEmailQuery request)
    {
        // Ordenamiento por defecto si no se especifica
        var campoOrden = string.IsNullOrEmpty(request.OrdenarPor) ? "Email" : request.OrdenarPor;
        var esDescendente = request.DireccionOrden.Equals("desc", StringComparison.OrdinalIgnoreCase);

        if (_camposOrdenamiento.TryGetValue(campoOrden, out var expresionOrden))
        {
            query = esDescendente
                ? query.OrderByDescending(expresionOrden)
                : query.OrderBy(expresionOrden);
        }
        else
        {
            // Fallback a ordenamiento por email
            query = query.OrderBy(c => c.Email);
        }

        return query;
    }

    private async Task<string?> ObtenerTelefono(Guid clienteId)
    {
        return await _context.Clientes
            .Where(c => c.Id == clienteId)
            .Select(c => c.Telefono)
            .FirstOrDefaultAsync();
    }

    private double CalcularFactorRelevanciaManual(string email, string emailBusqueda)
    {
        // Implementa la lógica para calcular la relevancia manualmente
        // Puedes usar diferentes métodos para calcular la similitud entre los correos electrónicos
        // Aquí se usa una implementación simple basada en la similitud de caracteres
        return 0.5; // Valor por defecto
    }

    private async Task<List<dynamic>> EnriquecerResultados(List<dynamic> resultados)
    {
        var resultadosEnriquecidos = new List<dynamic>();

        foreach (var resultado in resultados)
        {
            // TODO: Implementar enriquecimiento cuando estén disponibles las entidades relacionadas
            // var reservaciones = await ObtenerReservacionesCliente(resultado.Id);
            // var fidelizacion = await ObtenerDatosFidelizacion(resultado.Id);
            
            resultadosEnriquecidos.Add(new
            {
                resultado.Id,
                resultado.Nombre,
                resultado.Email,
                resultado.FechaRegistro,
                resultado.TotalCompras,
                resultado.CantidadReservaciones,
                resultado.EsClienteFrecuente,
                resultado.FactorRelevancia,
                // TODO: Agregar cuando estén disponibles
                // PuntosAcumulados = fidelizacion?.PuntosAcumulados ?? 0,
                // UltimaReservacion = reservaciones?.OrderByDescending(r => r.Fecha).FirstOrDefault()?.Fecha,
                // TipoCliente = DeterminarTipoCliente(resultado.TotalCompras, resultado.CantidadReservaciones)
            });
        }

        return resultadosEnriquecidos;
    }

    private async Task<List<dynamic>> FiltrarPorTerminoBusqueda(IQueryable<dynamic> query, string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return await query.ToListAsync();

        var terminoLower = termino.ToLowerInvariant();

        // Simular filtrado por término - TODO: Implementar filtrado real en base de datos
        var resultados = await query.ToListAsync();
        
        return resultados.Where(r => 
            r.Nombre.ToLowerInvariant().Contains(terminoLower) ||
            r.Email.ToLowerInvariant().Contains(terminoLower)
        ).ToList();
    }
} 