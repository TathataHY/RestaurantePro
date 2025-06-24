using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Domain.Comercial.Facturacion;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturasPorComanda;

public class ObtenerFacturasPorComandaHandler : IRequestHandler<ObtenerFacturasPorComandaQuery, Result<List<FacturaDto>>>
{
    private readonly IApplicationDbContext _context;

    public ObtenerFacturasPorComandaHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FacturaDto>>> Handle(ObtenerFacturasPorComandaQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Traer todas las facturas candidatas a memoria (puedes optimizar con filtros adicionales si lo deseas)
            var facturasEnMemoria = await _context.Facturas
                .Include(f => f.Cliente)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync(cancellationToken);

            // Filtrar en memoria por ComandaId
            var facturas = facturasEnMemoria
                .Where(f => f.ComandasIds != null && f.ComandasIds.Contains(request.ComandaId))
                .Select(f => new FacturaDto
                {
                    Id = f.Id,
                    Numero = f.NumeroFactura,
                    Tipo = f.TipoFactura,
                    Estado = f.Estado,
                    Subtotal = f.Subtotal,
                    Impuestos = f.TotalImpuestos,
                    Descuentos = f.TotalDescuentos,
                    Total = f.Total,
                    MontoPagado = f.TotalPagado,
                    FechaPago = f.FechaPago,
                    FechaCreacion = f.FechaCreacion,
                    FechaModificacion = f.FechaActualizacion,
                    ClienteId = f.ClienteId,
                    NombreCliente = f.NombreCliente,
                    ComandaId = request.ComandaId,
                    NumeroComanda = 0, // Se puede obtener de la comanda si es necesario
                    FechaEmision = f.FechaEmision,
                    FechaVencimiento = f.FechaVencimiento,
                    MetodoPago = null, // Se puede obtener de los pagos si es necesario
                    ReferenciaPago = null // Se puede obtener de los pagos si es necesario
                })
                .ToList();

            return Result<List<FacturaDto>>.Success(facturas);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<FacturaDto>>(new List<string> { $"Error al obtener facturas por comanda: {ex.Message}" });
        }
    }
} 