using MediatR;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CerrarComanda;

public class CerrarComandaCommandHandler : IRequestHandler<CerrarComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CerrarComandaCommandHandler> _logger;

    public CerrarComandaCommandHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<CerrarComandaCommandHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(CerrarComandaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🏁 Cerrando comanda {ComandaId} - Método de pago: {MetodoPago}", 
            request.ComandaId, request.MetodoPago);

        // 1. Buscar la comanda
        var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId, true, cancellationToken);
        if (comanda == null)
            return Result.Failure<ComandaDto>($"No se encontró la comanda con ID {request.ComandaId}");

        // 2. Validar estado para cerrar
        if (comanda.Estado == EstadoComanda.Cancelada)
            return Result.Failure<ComandaDto>("No se puede cerrar una comanda cancelada");
        if (comanda.Estado == EstadoComanda.Finalizada)
            return Result.Failure<ComandaDto>("La comanda ya está cerrada");

        // 3. Validar que tenga productos
        if (!comanda.Items.Any())
            return Result.Failure<ComandaDto>("No se puede cerrar una comanda sin productos");

        // 4. Finalizar la comanda
        try
        {
            comanda.MarcarPagada();
            
            // Agregar observaciones de cierre si se proporcionan
            if (!string.IsNullOrWhiteSpace(request.Observaciones))
            {
                comanda.CambiarObservaciones(request.Observaciones);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar la comanda");
            return Result.Failure<ComandaDto>($"Error al cerrar la comanda: {ex.Message}");
        }

        // 5. Guardar cambios
        await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
        await _comandaRepository.GuardarCambiosAsync(cancellationToken);

        // 6. Mapear a DTO y retornar
        var dto = _mapper.Map<ComandaDto>(comanda);
        return Result.Success(dto);
    }
} 