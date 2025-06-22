using MediatR;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarComanda;

public class ActualizarComandaCommandHandler : IRequestHandler<ActualizarComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarComandaCommandHandler> _logger;

    public ActualizarComandaCommandHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<ActualizarComandaCommandHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(ActualizarComandaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("✏️ Actualizando comanda {ComandaId}", request.Id);

        // 1. Buscar la comanda
        var comanda = await _comandaRepository.ObtenerPorIdAsync(request.Id, true, cancellationToken);
        if (comanda == null)
            return Result.Failure<ComandaDto>($"No se encontró la comanda con ID {request.Id}");

        // 2. Validar estado editable
        if (comanda.Estado != EstadoComanda.Creada && comanda.Estado != EstadoComanda.EnProceso)
            return Result.Failure<ComandaDto>($"No se puede editar una comanda en estado '{comanda.Estado}'");

        // 3. Actualizar campos permitidos
        if (request.MesaId.HasValue && request.MesaId.Value != Guid.Empty)
            comanda.CambiarMesa(request.MesaId.Value);
        if (request.ClienteId.HasValue)
            comanda.CambiarCliente(request.ClienteId);
        if (!string.IsNullOrWhiteSpace(request.Observaciones))
            comanda.CambiarObservaciones(request.Observaciones);
        if (request.MeseroId.HasValue && request.MeseroId.Value != Guid.Empty)
            comanda.CambiarMesero(request.MeseroId.Value);

        // 4. Guardar cambios
        await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
        await _comandaRepository.GuardarCambiosAsync(cancellationToken);

        // 5. Mapear a DTO y retornar
        var dto = _mapper.Map<ComandaDto>(comanda);
        return Result.Success(dto);
    }
} 