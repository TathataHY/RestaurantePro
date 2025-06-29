using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;
using RestaurantePro.Domain.Proveedores.Entities;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.ActualizarEvaluacionProveedor;

public class ActualizarEvaluacionProveedorCommandHandler : IRequestHandler<ActualizarEvaluacionProveedorCommand, Result<EvaluacionProveedorDto>>
{
    private readonly IProveedoresDbContext _context;
    private readonly ILogger<ActualizarEvaluacionProveedorCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public ActualizarEvaluacionProveedorCommandHandler(
        IProveedoresDbContext context,
        ILogger<ActualizarEvaluacionProveedorCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<EvaluacionProveedorDto>> Handle(ActualizarEvaluacionProveedorCommand request, CancellationToken cancellationToken)
    {
        var evaluacion = await _context.EvaluacionesProveedores
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (evaluacion == null)
        {
            _logger.LogWarning("No se encontró la evaluación con ID: {Id}", request.Id);
            return Result.Failure<EvaluacionProveedorDto>("Evaluación no encontrada");
        }

        // Solo el evaluador original o un admin debería poder actualizar (opcional)
        // if (evaluacion.EvaluadorId != _currentUserService.UserId) ...

        evaluacion.Actualizar(
            request.CalificacionGeneral,
            request.CalificacionCalidad,
            request.CalificacionPuntualidad,
            request.CalificacionComunicacion,
            request.CalificacionPrecios,
            request.Comentarios
        );

        // Opcional: setear campos de auditoría si existen
        // evaluacion.ActualizadoPor = _currentUserService.UserName;

        await _context.SaveChangesAsync(cancellationToken);

        // Obtener el nombre del proveedor
        var proveedor = await _context.Proveedores.FirstOrDefaultAsync(p => p.Id == evaluacion.ProveedorId, cancellationToken);
        var nombreProveedor = proveedor?.Nombre ?? string.Empty;

        var dto = new EvaluacionProveedorDto
        {
            Id = evaluacion.Id,
            ProveedorId = evaluacion.ProveedorId,
            NombreProveedor = nombreProveedor,
            EvaluadorId = evaluacion.EvaluadorId,
            NombreEvaluador = _currentUserService.UserName ?? "Usuario Sistema",
            CalificacionGeneral = evaluacion.CalificacionGeneral,
            CalificacionCalidad = evaluacion.CalificacionCalidad,
            CalificacionPuntualidad = evaluacion.CalificacionPuntualidad,
            CalificacionComunicacion = evaluacion.CalificacionComunicacion,
            CalificacionPrecios = evaluacion.CalificacionPrecios,
            PromedioPonderado = evaluacion.PromedioPonderado,
            Comentarios = evaluacion.Comentarios,
            FechaEvaluacion = evaluacion.FechaEvaluacion,
            FechaActualizacion = evaluacion.FechaActualizacion,
            Activa = evaluacion.Activa,
            CreadoPor = evaluacion.CreatedBy,
            ActualizadoPor = evaluacion.LastModifiedBy
        };

        _logger.LogInformation("Evaluación de proveedor actualizada exitosamente. EvaluacionId: {EvaluacionId}", evaluacion.Id);
        return Result.Success(dto);
    }
} 