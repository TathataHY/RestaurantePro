using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.EliminarFactura;

public class EliminarFacturaCommandHandler : IRequestHandler<EliminarFacturaCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EliminarFacturaCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public EliminarFacturaCommandHandler(
        IApplicationDbContext context,
        ILogger<EliminarFacturaCommandHandler> logger,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<bool>> Handle(EliminarFacturaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando proceso de anulación para factura {FacturaId}", request.FacturaId);
            
            // Validar que el comando sea válido
            if (!request.EsValido())
            {
                string mensajeError = "El comando de anulación no es válido";
                _logger.LogWarning(mensajeError);
                return Result.Failure<bool>(mensajeError);
            }
            
            // Obtener la factura de la base de datos
            var factura = await _context.Facturas.FindAsync(new object[] { request.FacturaId }, cancellationToken);
            
            if (factura == null)
            {
                string mensajeError = $"No se encontró la factura con ID {request.FacturaId}";
                _logger.LogWarning(mensajeError);
                return Result.Failure<bool>(mensajeError);
            }
            
            // Verificar si la factura ya está anulada
            if (factura.Estado == EstadoFactura.Anulada)
            {
                string mensajeError = "La factura ya se encuentra anulada";
                _logger.LogWarning(mensajeError);
                return Result.Failure<bool>(mensajeError);
            }
            
            // Validar que la factura no esté pagada
            if (factura.Estado == EstadoFactura.Pagada)
            {
                string mensajeError = "No se puede anular una factura que ya ha sido pagada";
                _logger.LogWarning(mensajeError);
                return Result.Failure<bool>(mensajeError);
            }
            
            // Anular la factura usando el método de la entidad
            factura.Anular(request.Motivo, _dateTimeService);
            
            // Guardar cambios en la base de datos
            int result = await _context.SaveChangesAsync(cancellationToken);
            
            if (result <= 0)
            {
                string mensajeError = "No se pudieron guardar los cambios en la base de datos";
                _logger.LogWarning(mensajeError);
                return Result.Failure<bool>(mensajeError);
            }
            
            _logger.LogInformation("Factura {FacturaId} anulada exitosamente", request.FacturaId);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular factura {FacturaId}", request.FacturaId);
            return Result.Failure<bool>($"Error interno al anular factura: {ex.Message}");
        }
    }
} 