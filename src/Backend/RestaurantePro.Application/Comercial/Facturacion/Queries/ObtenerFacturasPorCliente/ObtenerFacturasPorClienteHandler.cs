using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;

namespace RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturasPorCliente;

/// <summary>
/// Handler para obtener facturas de un cliente específico
/// </summary>
public class ObtenerFacturasPorClienteHandler : IRequestHandler<ObtenerFacturasPorClienteQuery, Result<List<FacturaDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ObtenerFacturasPorClienteHandler> _logger;

    public ObtenerFacturasPorClienteHandler(IApplicationDbContext context, ILogger<ObtenerFacturasPorClienteHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<FacturaDto>>> Handle(ObtenerFacturasPorClienteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("👤 Obteniendo facturas del cliente {ClienteId}, SoloActivas={SoloActivas}", 
                request.ClienteId, request.SoloActivas);

            // Verificar que el cliente existe
            var clienteExiste = await _context.Clientes
                .AnyAsync(c => c.Id == request.ClienteId, cancellationToken);

            if (!clienteExiste)
            {
                _logger.LogWarning("⚠️ Cliente {ClienteId} no encontrado", request.ClienteId);
                return Result.Failure<List<FacturaDto>>($"El cliente con ID {request.ClienteId} no existe.");
            }

            var query = _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Detalles)
                .Where(f => f.ClienteId == request.ClienteId);

            // Aplicar filtros
            query = AplicarFiltros(query, request);

            // Aplicar ordenamiento
            query = AplicarOrdenamiento(query, request);

            // Aplicar límite si se especifica
            if (request.Limite.HasValue)
            {
                query = query.Take(request.Limite.Value);
            }

            var facturas = await query.ToListAsync(cancellationToken);

            // Mapear a DTOs
            var facturasDto = facturas.Select(f => new FacturaDto
            {
                Id = f.Id,
                Numero = f.NumeroFactura,
                ClienteId = f.ClienteId,
                NombreCliente = f.NombreCliente,
                ComandaId = f.ComandasIds.FirstOrDefault(),
                NumeroComanda = 0,
                FechaEmision = f.FechaEmision,
                FechaVencimiento = f.FechaVencimiento,
                Estado = f.Estado,
                Tipo = f.TipoFactura,
                Subtotal = f.Subtotal,
                Impuestos = f.TotalImpuestos,
                Descuentos = f.TotalDescuentos,
                Total = f.Total,
                MontoPagado = f.TotalPagado,
                FechaPago = f.FechaPago,
                MetodoPago = null,
                ReferenciaPago = null
            }).ToList();

            _logger.LogInformation("✅ Se obtuvieron {Count} facturas del cliente {ClienteId}", 
                facturasDto.Count, request.ClienteId);

            return Result.Success(facturasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener facturas del cliente {ClienteId}", request.ClienteId);
            return Result.Failure<List<FacturaDto>>($"Error al obtener facturas del cliente: {ex.Message}");
        }
    }

    private IQueryable<Factura> AplicarFiltros(IQueryable<Factura> query, ObtenerFacturasPorClienteQuery request)
    {
        // Filtro por estado específico
        if (!string.IsNullOrWhiteSpace(request.Estado))
        {
            if (Enum.TryParse<EstadoFactura>(request.Estado, true, out var estado))
            {
                query = query.Where(f => f.Estado == estado);
            }
        }

        // Filtro por fechas
        if (request.FechaDesde.HasValue)
        {
            query = query.Where(f => f.FechaEmision >= request.FechaDesde.Value);
        }

        if (request.FechaHasta.HasValue)
        {
            query = query.Where(f => f.FechaEmision <= request.FechaHasta.Value);
        }

        // Filtro por facturas activas
        if (request.SoloActivas)
        {
            query = query.Where(f => f.Estado != EstadoFactura.Anulada);
        }

        return query;
    }

    private IQueryable<Factura> AplicarOrdenamiento(IQueryable<Factura> query, ObtenerFacturasPorClienteQuery request)
    {
        var ordenarPor = request.OrdenarPor?.ToLower() ?? "fechaemision";
        var direccion = request.DireccionOrdenamiento?.ToLower() ?? "desc";

        return ordenarPor switch
        {
            "fechaemision" => direccion == "asc" 
                ? query.OrderBy(f => f.FechaEmision)
                : query.OrderByDescending(f => f.FechaEmision),
            
            "fechavencimiento" => direccion == "asc"
                ? query.OrderBy(f => f.FechaVencimiento)
                : query.OrderByDescending(f => f.FechaVencimiento),
            
            "total" => direccion == "asc"
                ? query.OrderBy(f => f.Total)
                : query.OrderByDescending(f => f.Total),
            
            "numerofactura" => direccion == "asc"
                ? query.OrderBy(f => f.NumeroFactura)
                : query.OrderByDescending(f => f.NumeroFactura),
            
            "estado" => direccion == "asc"
                ? query.OrderBy(f => f.Estado)
                : query.OrderByDescending(f => f.Estado),
            
            _ => query.OrderByDescending(f => f.FechaEmision)
        };
    }
} 