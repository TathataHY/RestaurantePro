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
            // Lanzar explícitamente si el token es cancelado
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Iniciando búsqueda de clientes por email: '{Email}'", request.Email);

            // 1. Query base de clientes
            var query = _context.Clientes.AsQueryable();

            // 2. Aplicar filtros de búsqueda
            query = AplicarFiltrosBusqueda(query, request);

            // 3. Aplicar ordenamiento
            query = AplicarOrdenamiento(query, request);

            // 4. ✅ Materializar la consulta completa respetando cancellationToken
            var todosLosClientes = await query.ToListAsync(cancellationToken);

            // 5. Contar total
            var totalCount = todosLosClientes.Count;

            if (totalCount == 0)
            {
                _logger.LogInformation("No se encontraron clientes con los criterios especificados");
                return Result.Success(new PaginatedList<ClienteSummaryDto>(
                    new List<ClienteSummaryDto>(), 0, request.Pagina, request.TamanoPagina));
            }

            // 6. Aplicar paginación en memoria
            var clientesPaginados = todosLosClientes
                .Skip((request.Pagina - 1) * request.TamanoPagina)
                .Take(request.TamanoPagina)
                .ToList();

            // 7. Mapear a DTOs de forma segura
            var clientesDto = clientesPaginados.Select(c => new ClienteSummaryDto
            {
                Id = c.Id,
                NombreCompleto = c.Nombre?.NombreCompleto ?? "Sin nombre",
                Email = c.Email?.Value ?? "",
                Telefono = c.Telefono?.Value ?? "",
                Activo = c.EstaActivo,
                FechaRegistro = c.FechaCreacion,
                TipoCliente = c.Segmento.ToString(),
                RegistradoPor = "Sistema", // TODO: Implementar cuando esté disponible
                FechaNacimiento = c.FechaNacimiento,
                Ciudad = "", // TODO: Implementar cuando esté disponible
                Pais = "", // TODO: Implementar cuando esté disponible
                PuntosFidelizacion = c.PuntosAcumulados,
                NivelFidelizacion = "Bronce", // TODO: Calcular desde fidelización
                TotalOrdenes = c.CantidadVisitas,
                MontoTotalCompras = 0m, // TODO: Calcular desde histórico
                FechaUltimaOrden = null, // TODO: Calcular desde histórico
                PromedioCompra = 0m, // TODO: Calcular desde histórico
                EsFrecuente = c.CantidadVisitas >= 10,
                DiasSinVisitar = (int)(DateTime.Now - c.FechaCreacion).TotalDays, // Aproximación
                EsVIP = c.Segmento == SegmentoCliente.Premium
            }).ToList();

            // 8. Crear resultado paginado
            var resultado = new PaginatedList<ClienteSummaryDto>(
                clientesDto, totalCount, request.Pagina, request.TamanoPagina);

            _logger.LogInformation("Búsqueda completada: {TotalEncontrados} clientes encontrados, {PaginaActual}/{TotalPaginas} páginas",
                totalCount, request.Pagina, resultado.TotalPages);

            return Result.Success(resultado);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Búsqueda de clientes cancelada por el usuario");
            throw; // Re-lanzar para que las pruebas de cancelación funcionen
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
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (request.BusquedaExacta)
            {
                // ✅ Para búsqueda exacta, usar comparación case-insensitive
                var emailLower = request.Email.ToLowerInvariant();
                query = query.Where(c => c.Email != null && 
                                       c.Email.Value.ToLower() == emailLower);
            }
            else
            {
                // ✅ Para búsqueda parcial, usar Contains case-insensitive
                var emailLower = request.Email.ToLowerInvariant();
                query = query.Where(c => c.Email != null && 
                                       c.Email.Value.ToLower().Contains(emailLower));
            }
        }

        // Búsqueda por dominio
        if (!string.IsNullOrWhiteSpace(request.Dominio))
        {
            var dominioPattern = $"@{request.Dominio.ToLowerInvariant()}";
            query = query.Where(c => c.Email != null && 
                                   c.Email.Value.ToLower().Contains(dominioPattern));
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
            // ✅ Fallback a ordenamiento por email de forma segura
            query = esDescendente
                ? query.OrderByDescending(c => c.Email != null ? c.Email.Value : "")
                : query.OrderBy(c => c.Email != null ? c.Email.Value : "");
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