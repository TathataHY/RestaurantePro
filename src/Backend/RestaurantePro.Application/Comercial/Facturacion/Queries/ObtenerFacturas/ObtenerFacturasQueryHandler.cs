using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;

namespace RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturas;

/// <summary>
/// Handler para obtener facturas con filtros opcionales
/// </summary>
public class ObtenerFacturasQueryHandler : IRequestHandler<ObtenerFacturasQuery, Result<List<FacturaDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ObtenerFacturasQueryHandler> _logger;

    public ObtenerFacturasQueryHandler(IApplicationDbContext context, ILogger<ObtenerFacturasQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<FacturaDto>>> Handle(ObtenerFacturasQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo facturas con filtros: Estado={Estado}, ClienteId={ClienteId}, FechaDesde={FechaDesde}, FechaHasta={FechaHasta}, SoloPendientesPago={SoloPendientesPago}",
                request.Estado, request.ClienteId, request.FechaDesde, request.FechaHasta, request.SoloPendientesPago);

            var query = _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Detalles)
                .AsQueryable();

            // Filtro especial: solo pendientes de pago
            if (request.SoloPendientesPago)
            {
                query = query.Where(f => (f.Estado == EstadoFactura.Emitida || f.Estado == EstadoFactura.PagadaParcialmente) && f.Total > f.TotalPagado);
            }

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

            // Mapear a DTOs usando AutoMapper o mapeo manual
            var facturasDto = facturas.Select(f => new FacturaDto
            {
                Id = f.Id,
                Numero = f.NumeroFactura,
                ClienteId = f.ClienteId,
                NombreCliente = f.NombreCliente,
                ComandaId = f.ComandasIds.FirstOrDefault(), // Tomar la primera comanda
                NumeroComanda = int.TryParse(f.Comandas?.FirstOrDefault()?.NumeroComanda, out var numeroComanda) ? numeroComanda : 0, // Obtener número de comanda desde la relación
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
                MetodoPago = f.Pagos?.FirstOrDefault()?.MetodoPago.ToString(), // Obtener método de pago desde la relación
                ReferenciaPago = f.Pagos?.FirstOrDefault()?.ReferenciaTransaccion // Obtener referencia de pago desde la relación
            }).ToList();

            _logger.LogInformation("✅ Se obtuvieron {Count} facturas", facturasDto.Count);
            
            // Log detallado de la primera factura para debugging
            if (facturasDto.Any())
            {
                var primeraFactura = facturasDto.First();
                _logger.LogInformation("🔍 [DEBUG] Primera factura - ID: {Id}, Numero: {Numero}, Cliente: {Cliente}, Total: {Total}, Estado: {Estado}", 
                    primeraFactura.Id, primeraFactura.Numero, primeraFactura.NombreCliente, primeraFactura.Total, primeraFactura.Estado);
            }

            return Result.Success(facturasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener facturas");
            return Result.Failure<List<FacturaDto>>($"Error al obtener facturas: {ex.Message}");
        }
    }

    private IQueryable<Factura> AplicarFiltros(IQueryable<Factura> query, ObtenerFacturasQuery request)
    {
        // Filtro por estado
        if (!string.IsNullOrWhiteSpace(request.Estado))
        {
            if (Enum.TryParse<EstadoFactura>(request.Estado, true, out var estado))
            {
                query = query.Where(f => f.Estado == estado);
            }
        }

        // Filtro por cliente
        if (request.ClienteId.HasValue)
        {
            query = query.Where(f => f.ClienteId == request.ClienteId.Value);
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

        // Filtro por tipo de factura
        if (!string.IsNullOrWhiteSpace(request.TipoFactura))
        {
            if (Enum.TryParse<TipoFactura>(request.TipoFactura, true, out var tipoFactura))
            {
                query = query.Where(f => f.TipoFactura == tipoFactura);
            }
        }

        // Filtro por días de vencimiento
        if (request.DiasVencimiento.HasValue)
        {
            var fechaLimite = DateTime.Today.AddDays(request.DiasVencimiento.Value);
            query = query.Where(f => f.FechaVencimiento <= fechaLimite);
        }

        // Filtro por facturas activas
        if (request.SoloActivas)
        {
            query = query.Where(f => f.Estado != EstadoFactura.Anulada);
        }

        return query;
    }

    private IQueryable<Factura> AplicarOrdenamiento(IQueryable<Factura> query, ObtenerFacturasQuery request)
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
            
            "nombrecliente" => direccion == "asc"
                ? query.OrderBy(f => f.NombreCliente)
                : query.OrderByDescending(f => f.NombreCliente),
            
            "estado" => direccion == "asc"
                ? query.OrderBy(f => f.Estado)
                : query.OrderByDescending(f => f.Estado),
            
            _ => query.OrderByDescending(f => f.FechaEmision)
        };
    }
} 