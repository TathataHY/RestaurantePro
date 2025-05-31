using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Domain.Common;
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
            _logger.LogInformation("Buscando clientes por email: '{Email}', Búsqueda exacta: {BusquedaExacta}, Dominio: '{Dominio}', Página: {Pagina}",
                request.Email, request.BusquedaExacta, request.Dominio, request.Pagina);

            // 1. Construir query base
            var query = _context.Clientes.AsQueryable();

            // 2. Aplicar filtro de actividad
            if (request.SoloActivos)
            {
                query = query.Where(c => c.Activo);
            }

            // 3. Aplicar filtros de búsqueda
            query = AplicarFiltrosBusqueda(query, request);

            // 4. Aplicar filtro de segmento si se especifica
            if (!string.IsNullOrEmpty(request.Segmento))
            {
                if (Enum.TryParse<SegmentoCliente>(request.Segmento, true, out var segmento))
                {
                    query = query.Where(c => c.Segmento == segmento);
                }
            }

            // 5. Incluir datos relacionados si es necesario
            if (request.IncluirFidelizacion)
            {
                query = query.Include(c => c.TarjetasFidelizacion.Where(t => t.Activa));
            }

            // 6. Aplicar ordenamiento
            query = AplicarOrdenamiento(query, request);

            // 7. Contar total antes de paginar
            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                _logger.LogInformation("No se encontraron clientes con los criterios especificados");
                return Result.Success(new PaginatedList<ClienteSummaryDto>(
                    new List<ClienteSummaryDto>(), 0, request.Pagina, request.TamanoPagina));
            }

            // 8. Aplicar paginación
            var clientes = await query
                .Skip((request.Pagina - 1) * request.TamanoPagina)
                .Take(request.TamanoPagina)
                .ToListAsync(cancellationToken);

            // 9. Mapear a DTOs
            var clientesDto = await MapearClientesADto(clientes, request);

            // 10. Crear resultado paginado
            var resultado = new PaginatedList<ClienteSummaryDto>(
                clientesDto, totalCount, request.Pagina, request.TamanoPagina);

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
                var emailLower = request.Email.ToLower();
                query = query.Where(c => EF.Functions.Like(c.Email.ToLower(), $"%{emailLower}%"));
            }
        }

        // Búsqueda por dominio
        if (!string.IsNullOrEmpty(request.Dominio))
        {
            var dominioLower = request.Dominio.ToLower();
            query = query.Where(c => EF.Functions.Like(c.Email.ToLower(), $"%@{dominioLower}%"));
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

    private async Task<List<ClienteSummaryDto>> MapearClientesADto(List<Cliente> clientes, BuscarClientesPorEmailQuery request)
    {
        var clientesDto = new List<ClienteSummaryDto>();

        foreach (var cliente in clientes)
        {
            var clienteDto = new ClienteSummaryDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Activo = cliente.Activo,
                Segmento = cliente.Segmento.ToString(),
                PuntosAcumulados = cliente.PuntosAcumulados,
                CantidadVisitas = cliente.CantidadVisitas,
                MontoTotalGastado = cliente.MontoTotalGastado,
                FechaCreacion = cliente.FechaCreacion,
                UltimaVisita = await ObtenerUltimaVisita(cliente.Id)
            };

            // Información de fidelización si se solicita
            if (request.IncluirFidelizacion)
            {
                var tarjetaActiva = cliente.TarjetasFidelizacion.FirstOrDefault(t => t.Activa);
                if (tarjetaActiva != null)
                {
                    clienteDto.InformacionFidelizacion = new FidelizacionSummaryDto
                    {
                        TarjetaId = tarjetaActiva.Id,
                        Numero = tarjetaActiva.Numero,
                        PuntosActuales = tarjetaActiva.PuntosActuales,
                        NivelFidelizacion = tarjetaActiva.Nivel.ToString(),
                        FechaAfiliacion = tarjetaActiva.FechaCreacion
                    };
                }
            }

            // Calcular estadísticas adicionales
            clienteDto.PromedioGastoPorVisita = cliente.CantidadVisitas > 0 
                ? cliente.MontoTotalGastado / cliente.CantidadVisitas 
                : 0;

            clienteDto.EsClienteFrecuente = cliente.CantidadVisitas >= 5;
            clienteDto.EsClienteVIP = cliente.Segmento == SegmentoCliente.VIP;

            clientesDto.Add(clienteDto);
        }

        return clientesDto;
    }

    private async Task<DateTime?> ObtenerUltimaVisita(Guid clienteId)
    {
        return await _context.Reservaciones
            .Where(r => r.ClienteId == clienteId && r.Estado == EstadoReservacion.Completada)
            .OrderByDescending(r => r.FechaHora)
            .Select(r => r.FechaHora)
            .FirstOrDefaultAsync();
    }
} 