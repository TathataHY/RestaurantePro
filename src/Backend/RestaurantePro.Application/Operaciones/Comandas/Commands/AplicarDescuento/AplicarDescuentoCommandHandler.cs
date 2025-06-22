using MediatR;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AplicarDescuento;

public class AplicarDescuentoCommandHandler : IRequestHandler<AplicarDescuentoCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AplicarDescuentoCommandHandler> _logger;

    public AplicarDescuentoCommandHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<AplicarDescuentoCommandHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(AplicarDescuentoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("💰 Aplicando descuento de {Porcentaje}% a comanda {ComandaId}", 
            request.PorcentajeDescuento, request.ComandaId);

        // 1. Buscar la comanda
        var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId, true, cancellationToken);
        if (comanda == null)
            return Result.Failure<ComandaDto>($"No se encontró la comanda con ID {request.ComandaId}");

        // 2. Validar estado para aplicar descuentos
        if (comanda.Estado == EstadoComanda.Cancelada || comanda.Estado == EstadoComanda.Finalizada)
            return Result.Failure<ComandaDto>($"No se puede aplicar descuento a una comanda en estado '{comanda.Estado}'");

        // 3. Validar que no tenga descuento previo
        if (comanda.DescuentoFidelizacion.HasValue && comanda.DescuentoFidelizacion.Value > 0)
            return Result.Failure<ComandaDto>("La comanda ya tiene un descuento aplicado");

        // 4. Aplicar descuento
        try
        {
            // Calcular el monto del descuento basado en el porcentaje
            decimal subtotal = comanda.Total?.Subtotal ?? 0;
            if (subtotal <= 0)
                return Result.Failure<ComandaDto>("No se puede aplicar descuento a una comanda sin subtotal");
            
            decimal montoDescuento = subtotal * (request.PorcentajeDescuento / 100m);
            
            // Usar el método que acepta monto fijo
            bool descuentoAplicado = comanda.AplicarDescuento(montoDescuento, request.Motivo);
            if (!descuentoAplicado)
                return Result.Failure<ComandaDto>("No se pudo aplicar el descuento a la comanda");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aplicar descuento a la comanda");
            return Result.Failure<ComandaDto>($"Error al aplicar descuento: {ex.Message}");
        }

        // 5. Guardar cambios
        await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
        await _comandaRepository.GuardarCambiosAsync(cancellationToken);

        // 6. Mapear a DTO y retornar
        var dto = _mapper.Map<ComandaDto>(comanda);
        return Result.Success(dto);
    }
} 