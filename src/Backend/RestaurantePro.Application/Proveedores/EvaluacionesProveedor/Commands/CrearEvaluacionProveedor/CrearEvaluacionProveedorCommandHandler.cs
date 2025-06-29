using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Proveedores.EvaluacionesProveedor.DTOs;
using RestaurantePro.Domain.Proveedores.Entities;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.CrearEvaluacionProveedor;

public class CrearEvaluacionProveedorCommandHandler : IRequestHandler<CrearEvaluacionProveedorCommand, Result<EvaluacionProveedorDto>>
{
    private readonly IProveedoresDbContext _context;
    private readonly ILogger<CrearEvaluacionProveedorCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CrearEvaluacionProveedorCommandHandler(
        IProveedoresDbContext context,
        ILogger<CrearEvaluacionProveedorCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<EvaluacionProveedorDto>> Handle(CrearEvaluacionProveedorCommand request, CancellationToken cancellationToken)
    {
        // Validar existencia del proveedor
        var proveedor = await _context.Proveedores.FirstOrDefaultAsync(p => p.Id == request.ProveedorId, cancellationToken);
        if (proveedor == null)
        {
            _logger.LogWarning("No se encontró el proveedor con ID: {ProveedorId}", request.ProveedorId);
            return Result.Failure<EvaluacionProveedorDto>("Proveedor no encontrado");
        }

        // Obtener el usuario autenticado
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var evaluadorId))
        {
            _logger.LogWarning("No se pudo determinar el usuario evaluador actual");
            return Result.Failure<EvaluacionProveedorDto>("No se pudo determinar el usuario evaluador actual");
        }

        // Crear la entidad de dominio
        var evaluacion = EvaluacionProveedor.Crear(
            request.ProveedorId,
            evaluadorId,
            request.CalificacionGeneral,
            request.CalificacionCalidad,
            request.CalificacionPuntualidad,
            request.CalificacionComunicacion,
            request.CalificacionPrecios,
            request.Comentarios
        );

        // Opcional: setear campos de auditoría si existen
        // evaluacion.CreadoPor = _currentUserService.UserName;

        _context.EvaluacionesProveedores.Add(evaluacion);
        await _context.SaveChangesAsync(cancellationToken);

        // Mapear a DTO
        var dto = new EvaluacionProveedorDto
        {
            Id = evaluacion.Id,
            ProveedorId = evaluacion.ProveedorId,
            NombreProveedor = proveedor.Nombre,
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
            CreadoPor = _currentUserService.UserName ?? string.Empty,
            ActualizadoPor = _currentUserService.UserName ?? string.Empty
        };

        _logger.LogInformation("Evaluación de proveedor creada exitosamente. EvaluacionId: {EvaluacionId}", evaluacion.Id);
        return Result.Success(dto);
    }
} 