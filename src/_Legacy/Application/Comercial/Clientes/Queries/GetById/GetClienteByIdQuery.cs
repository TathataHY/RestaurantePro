using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Comercial.Clientes.Queries.GetById
{
    public class GetClienteByIdQuery : IRequest<ClienteDto>
    {
        public int Id { get; set; }
    }

    public class GetClienteByIdQueryHandler : IRequestHandler<GetClienteByIdQuery, ClienteDto>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;

        public GetClienteByIdQueryHandler(IClienteRepository clienteRepository, IMapper mapper)
        {
            _clienteRepository = clienteRepository;
            _mapper = mapper;
        }

        public async Task<ClienteDto> Handle(GetClienteByIdQuery request, CancellationToken cancellationToken)
        {
            var cliente = await _clienteRepository.GetByIdAsync(request.Id);

            if (cliente == null)
            {
                throw new NotFoundException($"Cliente con ID {request.Id} no encontrado");
            }

            return _mapper.Map<ClienteDto>(cliente);
        }
    }
} 