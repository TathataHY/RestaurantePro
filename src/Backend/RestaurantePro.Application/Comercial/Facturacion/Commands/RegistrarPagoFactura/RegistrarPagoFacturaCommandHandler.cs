using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Pagos.Enums;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.RegistrarPagoFactura;

public class RegistrarPagoFacturaCommandHandler : IRequestHandler<RegistrarPagoFacturaCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RegistrarPagoFacturaCommandHandler> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;

    public RegistrarPagoFacturaCommandHandler(
        IApplicationDbContext context,
        ILogger<RegistrarPagoFacturaCommandHandler> logger,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(RegistrarPagoFacturaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Registrando pago para factura {FacturaId}", request.FacturaId);

            // Buscar la factura existente
            var factura = await _context.Facturas
                .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.Id == request.FacturaId, cancellationToken);

            if (factura == null)
            {
                var mensajeError = $"La factura con ID {request.FacturaId} no encontrada";
                _logger.LogWarning(mensajeError);
                return Result.Failure<bool>(mensajeError);
            }

            // Verificar que la factura se puede pagar
            if (factura.Estado == EstadoFactura.Anulada)
            {
                var mensajeError = "No se puede registrar pago en una factura anulada";
                _logger.LogWarning(mensajeError);
                return Result.Failure<bool>(mensajeError);
            }

            if (factura.Estado == EstadoFactura.Pagada)
            {
                var mensajeError = "La factura ya está completamente pagada";
                _logger.LogWarning(mensajeError);
                return Result.Failure<bool>(mensajeError);
            }

            // Registrar el pago
            var pagoId = Guid.NewGuid(); // Generar un ID único para el pago
            
            // Usar el monto del comando en lugar del total de la factura
            factura.RegistrarPago(request.Monto, pagoId, _dateTimeService);

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Pago registrado exitosamente para factura {FacturaId}", request.FacturaId);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            var mensajeError = $"Error al registrar el pago: {ex.Message}";
            _logger.LogError(ex, mensajeError);
            return Result.Failure<bool>(mensajeError);
        }
    }
} 