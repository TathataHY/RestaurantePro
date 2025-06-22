using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesPaginadas;

/// <summary>
/// Handler para obtener reservaciones con paginación
/// </summary>
public class ObtenerReservacionesPaginadasHandler : IRequestHandler<ObtenerReservacionesPaginadasQuery, Result<PaginatedList<ReservacionDto>>>
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerReservacionesPaginadasHandler> _logger;

    public ObtenerReservacionesPaginadasHandler(
        IReservacionRepository reservacionRepository,
        IMapper mapper,
        ILogger<ObtenerReservacionesPaginadasHandler> logger)
    {
        _reservacionRepository = reservacionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedList<ReservacionDto>>> Handle(
        ObtenerReservacionesPaginadasQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📋 Obteniendo reservaciones paginadas - Página {PageNumber}, Tamaño {PageSize}", 
            request.PageNumber, request.PageSize);

        try
        {
            // Obtener todas las reservaciones como IQueryable
            var query = await _reservacionRepository.ObtenerTodasAsync(cancellationToken);

            // Aplicar filtros
            query = AplicarFiltros(query, request);

            // Contar total de registros antes de ordenar
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar ordenamiento en memoria para evitar problemas con SQLite
            var items = await query.ToListAsync(cancellationToken);
            
            // Ordenar en memoria
            items = AplicarOrdenamientoEnMemoria(items, request);

            // Aplicar paginación
            var itemsPaginados = items
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Mapear a DTOs
            var reservacionesDto = _mapper.Map<List<ReservacionDto>>(itemsPaginados);

            // Crear resultado paginado
            var resultado = new PaginatedList<ReservacionDto>(
                reservacionesDto,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogInformation("✅ Se encontraron {TotalReservaciones} reservaciones. Página {Pagina} de {TotalPaginas} ({ReservacionesEnPagina} reservaciones en esta página)", 
                resultado.TotalCount, 
                resultado.PageNumber, 
                resultado.TotalPages, 
                resultado.Items.Count);

            return Result.Success(resultado);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🚫 Operación cancelada al obtener reservaciones paginadas");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener reservaciones paginadas");
            return Result.Failure<PaginatedList<ReservacionDto>>("Error interno del servidor al obtener las reservaciones");
        }
    }

    private IQueryable<Domain.Operaciones.Reservaciones.Entities.Reservacion> AplicarFiltros(
        IQueryable<Domain.Operaciones.Reservaciones.Entities.Reservacion> query, 
        ObtenerReservacionesPaginadasQuery request)
    {
        // Filtro por término de búsqueda
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(r => 
                (r.Cliente != null && r.Cliente.Nombre.Nombre.ToLower().Contains(searchTerm)) ||
                (r.Cliente != null && r.Cliente.Nombre.Apellido.ToLower().Contains(searchTerm)) ||
                r.Telefono.ToLower().Contains(searchTerm) ||
                r.Email.ToLower().Contains(searchTerm));
        }

        // Filtro por fecha desde
        if (request.FechaDesde.HasValue)
        {
            query = query.Where(r => r.Fecha >= request.FechaDesde.Value.Date);
        }

        // Filtro por fecha hasta
        if (request.FechaHasta.HasValue)
        {
            query = query.Where(r => r.Fecha <= request.FechaHasta.Value.Date);
        }

        // Filtro por estado
        if (request.Estado.HasValue)
        {
            query = query.Where(r => r.Estado == request.Estado.Value);
        }

        // Filtro por cliente
        if (request.ClienteId.HasValue)
        {
            query = query.Where(r => r.ClienteId == request.ClienteId.Value);
        }

        // Filtro por mesa
        if (request.MesaId.HasValue)
        {
            query = query.Where(r => r.MesaId == request.MesaId.Value);
        }

        return query;
    }

    private IQueryable<Domain.Operaciones.Reservaciones.Entities.Reservacion> AplicarOrdenamiento(IQueryable<Domain.Operaciones.Reservaciones.Entities.Reservacion> query, ObtenerReservacionesPaginadasQuery request)
    {
        return request.SortBy?.ToLower() switch
        {
            "fecha" => request.SortDirection == "desc" 
                ? query.OrderByDescending(r => r.Fecha).ThenByDescending(r => r.Hora.TotalMinutes)
                : query.OrderBy(r => r.Fecha).ThenBy(r => r.Hora.TotalMinutes),
            "cliente" => request.SortDirection == "desc"
                ? query.OrderByDescending(r => r.Cliente != null ? r.Cliente.Nombre.NombreCompleto : "")
                : query.OrderBy(r => r.Cliente != null ? r.Cliente.Nombre.NombreCompleto : ""),
            "mesa" => request.SortDirection == "desc"
                ? query.OrderByDescending(r => r.Mesa != null ? r.Mesa.Numero : 0)
                : query.OrderBy(r => r.Mesa != null ? r.Mesa.Numero : 0),
            "estado" => request.SortDirection == "desc"
                ? query.OrderByDescending(r => r.Estado)
                : query.OrderBy(r => r.Estado),
            "personas" => request.SortDirection == "desc"
                ? query.OrderByDescending(r => r.CantidadPersonas)
                : query.OrderBy(r => r.CantidadPersonas),
            _ => query.OrderByDescending(r => r.Fecha).ThenByDescending(r => r.Hora.TotalMinutes) // Ordenamiento por defecto
        };
    }

    private List<Domain.Operaciones.Reservaciones.Entities.Reservacion> AplicarOrdenamientoEnMemoria(List<Domain.Operaciones.Reservaciones.Entities.Reservacion> items, ObtenerReservacionesPaginadasQuery request)
    {
        return request.SortBy?.ToLower() switch
        {
            "fecha" => request.SortDirection == "desc" 
                ? items.OrderByDescending(r => r.Fecha).ThenByDescending(r => r.Hora.TotalMinutes).ToList()
                : items.OrderBy(r => r.Fecha).ThenBy(r => r.Hora.TotalMinutes).ToList(),
            "cliente" => request.SortDirection == "desc"
                ? items.OrderByDescending(r => r.Cliente != null ? r.Cliente.Nombre.NombreCompleto : "").ToList()
                : items.OrderBy(r => r.Cliente != null ? r.Cliente.Nombre.NombreCompleto : "").ToList(),
            "mesa" => request.SortDirection == "desc"
                ? items.OrderByDescending(r => r.Mesa != null ? r.Mesa.Numero : 0).ToList()
                : items.OrderBy(r => r.Mesa != null ? r.Mesa.Numero : 0).ToList(),
            "estado" => request.SortDirection == "desc"
                ? items.OrderByDescending(r => r.Estado).ToList()
                : items.OrderBy(r => r.Estado).ToList(),
            "personas" => request.SortDirection == "desc"
                ? items.OrderByDescending(r => r.CantidadPersonas).ToList()
                : items.OrderBy(r => r.CantidadPersonas).ToList(),
            _ => items.OrderByDescending(r => r.Fecha).ThenByDescending(r => r.Hora.TotalMinutes).ToList() // Ordenamiento por defecto
        };
    }
} 