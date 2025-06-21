using AutoMapper;
using MediatR;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionPorId;

/// <summary>
/// Handler para obtener una preparación específica por ID
/// </summary>
public class ObtenerPreparacionPorIdHandler : IRequestHandler<ObtenerPreparacionPorIdQuery, Result<PreparacionDto>>
{
    private readonly IPreparacionRepository _preparacionRepository;
    private readonly IMapper _mapper;

    public ObtenerPreparacionPorIdHandler(IPreparacionRepository preparacionRepository, IMapper mapper)
    {
        _preparacionRepository = preparacionRepository;
        _mapper = mapper;
    }

    public async Task<Result<PreparacionDto>> Handle(ObtenerPreparacionPorIdQuery request, CancellationToken cancellationToken)
    {
        var preparacion = await _preparacionRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
        if (preparacion == null)
        {
            return Result.Failure<PreparacionDto>("No se encontró la preparación solicitada");
        }
        var dto = _mapper.Map<PreparacionDto>(preparacion);
        return Result<PreparacionDto>.Success(dto);
    }
} 