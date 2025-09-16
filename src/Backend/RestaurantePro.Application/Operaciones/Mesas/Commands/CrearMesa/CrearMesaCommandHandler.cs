using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.CrearMesa;

public class CrearMesaCommandHandler : IRequestHandler<CrearMesaCommand, Result<MesaDto>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearMesaCommandHandler> _logger;

    public CrearMesaCommandHandler(IMesaRepository mesaRepository, IMapper mapper, ILogger<CrearMesaCommandHandler> logger)
    {
        _mesaRepository = mesaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<MesaDto>> Handle(CrearMesaCommand request, CancellationToken cancellationToken)
    {
        // Validar unicidad del número de mesa
        var existe = await _mesaRepository.ExisteNumeroAsync(request.Numero);
        if (existe)
        {
            _logger.LogWarning("Ya existe una mesa con el número {Numero}", request.Numero);
            return Result.Failure<MesaDto>(["Ya existe una mesa con ese número."]);
        }

        // Crear la entidad Mesa
        var mesa = Mesa.Crear(request.Numero, request.Capacidad, request.Ubicacion, 
            request.Descripcion, request.Notas, request.TieneVentana, request.TieneSofa, 
            request.EsAccesible, request.TieneEnchufe);
        await _mesaRepository.AgregarAsync(mesa);
        await _mesaRepository.GuardarCambiosAsync();
        _logger.LogInformation("Mesa creada correctamente: {Numero}", request.Numero);

        // Mapear a DTO
        var dto = _mapper.Map<MesaDto>(mesa);
        return Result.Success(dto);
    }
} 