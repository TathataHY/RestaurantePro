using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.ActualizarMesa;

public class ActualizarMesaCommandHandler : IRequestHandler<ActualizarMesaCommand, Result<MesaDto>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly ILogger<ActualizarMesaCommandHandler> _logger;
    private readonly IMapper _mapper;

    public ActualizarMesaCommandHandler(
        IMesaRepository mesaRepository, 
        ILogger<ActualizarMesaCommandHandler> logger,
        IMapper mapper)
    {
        _mesaRepository = mesaRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<MesaDto>> Handle(ActualizarMesaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Actualizando mesa con ID: {Id}", request.Id);

        // Buscar la mesa existente
        var mesa = await _mesaRepository.ObtenerPorIdAsync(request.Id);
        if (mesa == null)
        {
            _logger.LogWarning("No se encontró la mesa con ID {Id}", request.Id);
            return Result.Failure<MesaDto>(["Mesa no encontrada"]);
        }

        // Validar que el número no esté duplicado (si cambió)
        if (!int.TryParse(request.Numero, out int numeroMesa))
        {
            return Result.Failure<MesaDto>(["El número de mesa debe ser un valor numérico válido"]);
        }

        if (numeroMesa != mesa.Numero)
        {
            var existeNumero = await _mesaRepository.ExisteNumeroAsync(numeroMesa);
            if (existeNumero)
            {
                return Result.Failure<MesaDto>(["Ya existe una mesa con ese número"]);
            }
        }

        // Actualizar propiedades de la mesa usando el método de dominio
        mesa.ActualizarDatosCompletos(
            numeroMesa, 
            request.Capacidad, 
            request.Ubicacion, 
            request.Estado,
            request.Descripcion,
            request.Notas,
            request.TieneVentana,
            request.TieneSofa,
            request.EsAccesible,
            request.TieneEnchufe
        );

        await _mesaRepository.ActualizarAsync(mesa);
        await _mesaRepository.GuardarCambiosAsync();

        _logger.LogInformation("Mesa actualizada correctamente: {Id}", request.Id);

        // Mapear a DTO y retornar
        var mesaDto = _mapper.Map<MesaDto>(mesa);
        return Result.Success(mesaDto);
    }
} 