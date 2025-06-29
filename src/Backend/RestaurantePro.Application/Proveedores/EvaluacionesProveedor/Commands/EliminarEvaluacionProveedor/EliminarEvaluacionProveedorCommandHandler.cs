using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Proveedores.Entities;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.EliminarEvaluacionProveedor;

public class EliminarEvaluacionProveedorCommandHandler : IRequestHandler<EliminarEvaluacionProveedorCommand, Result<bool>>
{
    private readonly IProveedoresDbContext _context;
    private readonly ILogger<EliminarEvaluacionProveedorCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public EliminarEvaluacionProveedorCommandHandler(
        IProveedoresDbContext context,
        ILogger<EliminarEvaluacionProveedorCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(EliminarEvaluacionProveedorCommand request, CancellationToken cancellationToken)
    {
        var evaluacion = await _context.EvaluacionesProveedores
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (evaluacion == null)
        {
            _logger.LogWarning("No se encontró la evaluación con ID: {Id}", request.Id);
            return Result.Failure<bool>("Evaluación no encontrada");
        }

        if (!evaluacion.Activa)
        {
            _logger.LogInformation("La evaluación con ID: {Id} ya estaba inactiva", request.Id);
            return Result.Success(true);
        }

        evaluacion.Desactivar();
        // evaluacion.ActualizadoPor = _currentUserService.UserName;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Evaluación de proveedor desactivada exitosamente. EvaluacionId: {EvaluacionId}", evaluacion.Id);
        return Result.Success(true);
    }
} 