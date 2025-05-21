using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda
{
    /// <summary>
    /// Manejador para el comando CrearComandaCommand
    /// </summary>
    public class CrearComandaCommandHandler : IRequestHandler<CrearComandaCommand, Result<ComandaDto>>
    {
        private readonly IMapper _mapper;
        private readonly IComandaRepository _comandaRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public CrearComandaCommandHandler(
            IMapper mapper,
            IComandaRepository comandaRepository,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task<Result<ComandaDto>> Handle(CrearComandaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener el ID del usuario actual que será el mesero
                var meseroId = _currentUserService.UserId;
                if (meseroId == Guid.Empty)
                {
                    return Result<ComandaDto>.Failure("No se pudo identificar al usuario actual.");
                }

                // Crear la comanda
                var comanda = Comanda.Crear(
                    request.MesaId,
                    meseroId,
                    request.ClienteId,
                    request.Observaciones);
                
                // Guardar la comanda
                await _comandaRepository.AddAsync(comanda, cancellationToken);
                await _comandaRepository.GuardarCambiosAsync(cancellationToken);
                
                // Mapear a DTO
                var comandaDto = _mapper.Map<ComandaDto>(comanda);
                
                return Result<ComandaDto>.Success(comandaDto);
            }
            catch (Exception ex)
            {
                return Result<ComandaDto>.Failure($"Error al crear la comanda: {ex.Message}");
            }
        }
    }
} 