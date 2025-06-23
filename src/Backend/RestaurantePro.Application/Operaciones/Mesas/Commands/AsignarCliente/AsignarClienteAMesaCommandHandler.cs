using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.AsignarCliente;

public class AsignarClienteAMesaCommandHandler : IRequestHandler<AsignarClienteAMesaCommand, Result<Unit>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger<AsignarClienteAMesaCommandHandler> _logger;

    public AsignarClienteAMesaCommandHandler(
        IMesaRepository mesaRepository,
        IClienteRepository clienteRepository,
        ILogger<AsignarClienteAMesaCommandHandler> logger)
    {
        _mesaRepository = mesaRepository;
        _clienteRepository = clienteRepository;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(AsignarClienteAMesaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Asignando cliente {ClienteId} a mesa {MesaId}", request.ClienteId, request.MesaId);

        var mesa = await _mesaRepository.ObtenerPorIdAsync(request.MesaId);
        if (mesa == null)
        {
            _logger.LogWarning("Mesa no encontrada: {MesaId}", request.MesaId);
            return Result.Failure<Unit>("Mesa no encontrada");
        }

        var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId);
        if (cliente == null)
        {
            _logger.LogWarning("Cliente no encontrado: {ClienteId}", request.ClienteId);
            return Result.Failure<Unit>("Cliente no encontrado");
        }

        // Asignar el cliente a la mesa usando método de dominio
        mesa.AsignarCliente(cliente.Id, request.Observaciones);

        await _mesaRepository.ActualizarAsync(mesa);
        await _mesaRepository.GuardarCambiosAsync();

        _logger.LogInformation("Cliente {ClienteId} asignado correctamente a mesa {MesaId}", request.ClienteId, request.MesaId);
        return Result.Success(Unit.Value);
    }
} 