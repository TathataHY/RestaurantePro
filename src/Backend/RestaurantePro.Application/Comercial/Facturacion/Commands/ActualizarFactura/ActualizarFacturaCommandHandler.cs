using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.ActualizarFactura;

public class ActualizarFacturaCommandHandler : IRequestHandler<ActualizarFacturaCommand, Result<FacturaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarFacturaCommandHandler> _logger;
    private readonly IDateTimeService _dateTimeService;

    public ActualizarFacturaCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ActualizarFacturaCommandHandler> logger,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<FacturaDto>> Handle(ActualizarFacturaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando actualización de factura {FacturaId}", request.Id);

            // Buscar la factura existente
            var factura = await _context.Facturas
                .Include(f => f.Detalles)
                .Include(f => f.Cliente)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

            if (factura == null)
            {
                var mensajeError = $"No se encontró la factura con ID {request.Id}";
                _logger.LogWarning(mensajeError);
                return Result.Failure<FacturaDto>(mensajeError);
            }

            // Verificar que la factura se puede actualizar
            if (factura.Estado == Domain.Comercial.Facturacion.Enums.EstadoFactura.Anulada)
            {
                var mensajeError = "No se puede actualizar una factura anulada";
                _logger.LogWarning(mensajeError);
                return Result.Failure<FacturaDto>(mensajeError);
            }

            // Aplicar las actualizaciones disponibles en el Command
            if (!string.IsNullOrWhiteSpace(request.NombreCliente))
            {
                // Actualizar datos del cliente si es necesario
                // factura.ActualizarDatosCliente(...) 
            }

            if (!string.IsNullOrWhiteSpace(request.Observaciones))
            {
                factura.ModificarObservaciones(request.Observaciones);
            }

            // Las entidades manejan automáticamente las fechas de modificación

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            // Mapear a DTO
            var facturaDto = _mapper.Map<FacturaDto>(factura);

            _logger.LogInformation("Factura {FacturaId} actualizada exitosamente", request.Id);
            return Result.Success(facturaDto);
        }
        catch (Exception ex)
        {
            var mensajeError = $"Error al actualizar la factura: {ex.Message}";
            _logger.LogError(ex, mensajeError);
            return Result.Failure<FacturaDto>(mensajeError);
        }
    }
} 