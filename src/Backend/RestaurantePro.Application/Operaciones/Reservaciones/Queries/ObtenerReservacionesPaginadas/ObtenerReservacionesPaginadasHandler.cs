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

            // Aplicar ordenamiento
            query = AplicarOrdenamiento(query, request);

            // Contar total de registros
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs
            var reservacionesDto = _mapper.Map<List<ReservacionDto>>(items);

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
            query = query.Where(r => r.FechaReservacion >= request.FechaDesde.Value);
        }

        // Filtro por fecha hasta
        if (request.FechaHasta.HasValue)
        {
            query = query.Where(r => r.FechaReservacion <= request.FechaHasta.Value);
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
                ? query.OrderByDescending(r => r.FechaReservacion)
                : query.OrderBy(r => r.FechaReservacion),
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
            _ => query.OrderByDescending(r => r.FechaReservacion) // Ordenamiento por defecto
        };
    }
} 