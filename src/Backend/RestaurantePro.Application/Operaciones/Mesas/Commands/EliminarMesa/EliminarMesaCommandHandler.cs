using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.EliminarMesa;

public class EliminarMesaCommandHandler : IRequestHandler<EliminarMesaCommand, Result<bool>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EliminarMesaCommandHandler> _logger;

    public EliminarMesaCommandHandler(
        IMesaRepository mesaRepository, 
        IApplicationDbContext context,
        ILogger<EliminarMesaCommandHandler> logger)
    {
        _mesaRepository = mesaRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(EliminarMesaCommand request, CancellationToken cancellationToken)
    {
        var mesa = await _mesaRepository.ObtenerPorIdAsync(request.Id);
        if (mesa == null)
        {
            _logger.LogWarning("No se encontró la mesa con ID {Id}", request.Id);
            return Result.Failure<bool>(["Mesa no encontrada"]);
        }

        // Eliminar usando el contexto directamente para asegurar que se guarde
        _context.Mesas.Remove(mesa);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Mesa eliminada correctamente: {Id}", request.Id);
        return Result.Success(true);
    }
} 