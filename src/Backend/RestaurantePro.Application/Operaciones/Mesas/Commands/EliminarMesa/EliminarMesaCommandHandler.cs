using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.EliminarMesa;

public class EliminarMesaCommandHandler : IRequestHandler<EliminarMesaCommand, Result<bool>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly ILogger<EliminarMesaCommandHandler> _logger;

    public EliminarMesaCommandHandler(IMesaRepository mesaRepository, ILogger<EliminarMesaCommandHandler> logger)
    {
        _mesaRepository = mesaRepository;
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

        await _mesaRepository.EliminarAsync(mesa);
        await _mesaRepository.GuardarCambiosAsync();
        _logger.LogInformation("Mesa eliminada correctamente: {Id}", request.Id);
        return Result.Success(true);
    }
} 